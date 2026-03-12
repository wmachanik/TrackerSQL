using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MigrationRunner
{
    internal class PlanHumanReviewImporter
    {
        // Load the schema files and update them based on CSV plan mappings
        public static int ImportPlan(string migrationsDir, string csvPath, out string constraintsPath)
        {
            constraintsPath = null;
            Console.WriteLine($"?? Starting CSV import from: {csvPath}");

            // Debug: show what paths we're working with
            Console.WriteLine($"?? Input migrationsDir: {migrationsDir}");
            Console.WriteLine($"?? Current directory: {Directory.GetCurrentDirectory()}");

            // Determine project root directory more reliably
            string projectRootDir;
            
            // If we're in MigrationRunner folder, go up to TrackerSQL
            var currentDir = Directory.GetCurrentDirectory();
            if (currentDir.EndsWith("MigrationRunner"))
            {
                // Go up: MigrationRunner -> Migrations -> TrackerSQL
                projectRootDir = Path.GetDirectoryName(Path.GetDirectoryName(currentDir));
            }
            else if (currentDir.Contains("TrackerSQL"))
            {
                // Find TrackerSQL in the path
                var parts = currentDir.Split(Path.DirectorySeparatorChar);
                var trackerIndex = Array.FindLastIndex(parts, p => p.Equals("TrackerSQL", StringComparison.OrdinalIgnoreCase));
                if (trackerIndex >= 0)
                {
                    projectRootDir = string.Join(Path.DirectorySeparatorChar.ToString(), parts.Take(trackerIndex + 1));
                }
                else
                {
                    projectRootDir = currentDir; // Fallback
                }
            }
            else
            {
                // Fallback: use the provided migrationsDir and navigate up
                var migrationsRunnerDir = Path.GetDirectoryName(migrationsDir); // gets MigrationRunner folder
                var migrationsParentDir = Path.GetDirectoryName(migrationsRunnerDir); // gets Migrations folder  
                projectRootDir = Path.GetDirectoryName(migrationsParentDir); // gets TrackerSQL folder
            }
            
            var dataDir = Path.Combine(projectRootDir, "Data");
            
            if (!Directory.Exists(dataDir))
            {
                Console.WriteLine($"? Data directory not found: {dataDir}");
                Console.WriteLine($"? Calculated project root: {projectRootDir}");
                Console.WriteLine($"? Please check paths. Current working directory: {Directory.GetCurrentDirectory()}");
                return 1;
            }

            var schemaDir = Path.Combine(dataDir, "Metadata", "AccessSchema");
            if (!Directory.Exists(schemaDir))
            {
                Console.WriteLine($"? Schema directory not found: {schemaDir}");
                return 1;
            }

            Console.WriteLine($"?? Project root: {projectRootDir}");
            Console.WriteLine($"?? Data directory: {dataDir}");
            Console.WriteLine($"?? Schema directory: {schemaDir}");

            // List schema files for verification
            var schemaFiles = Directory.GetFiles(schemaDir, "*.schema.json");
            Console.WriteLine($"?? Found {schemaFiles.Length} schema files in directory:");
            foreach (var file in schemaFiles.Take(10))
            {
                Console.WriteLine($"    - {Path.GetFileName(file)}");
            }
            if (schemaFiles.Length > 10)
            {
                Console.WriteLine($"    ... and {schemaFiles.Length - 10} more");
            }

            // Parse CSV
            var tableMappings = ParseCsv(csvPath);
            if (tableMappings.Count == 0)
            {
                Console.WriteLine("? No table mappings found in CSV");
                return 1;
            }

            Console.WriteLine($"?? Parsed {tableMappings.Count} table mappings from CSV");
            Console.WriteLine();

            // Process each table mapping
            int successCount = 0;
            int failCount = 0;
            var allConstraints = new List<ConstraintTable>();

            for (int i = 0; i < tableMappings.Count; i++)
            {
                var mapping = tableMappings[i];
                Console.WriteLine($"=== TABLE {i + 1}/{tableMappings.Count}: {mapping.BeforeTable} -> {mapping.AfterTable ?? "n/a"} (Action: {mapping.Action}) ===");

                try
                {
                    if (ProcessTableMapping(schemaDir, mapping, allConstraints))
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? Error processing table {mapping.BeforeTable}: {ex.Message}");
                    failCount++;
                }

                Console.WriteLine();
            }

            // Generate constraints file
            constraintsPath = Path.Combine(dataDir, "Metadata", "PlanEdits", "PlanConstraints.json");
            Directory.CreateDirectory(Path.GetDirectoryName(constraintsPath));
            
            // SECOND PASS: Resolve FK reference columns by looking up the PK of each referenced table
            Console.WriteLine();
            Console.WriteLine("============================================================");
            Console.WriteLine("⮞ SECOND PASS: Resolving FK reference columns...");
            Console.WriteLine("============================================================");
            ResolveForeignKeyReferences(allConstraints);
            
            var constraintsIndex = new ConstraintsIndex { Tables = allConstraints };
            var constraintsSummary = GenerateConstraintsSummary(allConstraints);
            
            File.WriteAllText(constraintsPath, JsonConvert.SerializeObject(constraintsIndex, Formatting.Indented));

            // Final summary
            Console.WriteLine("============================================================");
            Console.WriteLine("?? FINAL SUMMARY");
            Console.WriteLine("============================================================");
            Console.WriteLine($"?? Total tables in CSV: {tableMappings.Count}");
            Console.WriteLine($"??  Tables ignored: {tableMappings.Count(m => m.Action.Equals("Ignore", StringComparison.OrdinalIgnoreCase))}");
            Console.WriteLine($"?? Tables processed: {successCount + failCount}");
            Console.WriteLine($"? Tables successful: {successCount}");
            Console.WriteLine($"? Tables failed: {failCount}");
            Console.WriteLine("============================================================");
            Console.WriteLine($"?? Constraints summary: {constraintsSummary}");
            Console.WriteLine($"?? Constraints JSON: {constraintsPath}");
            
            if (successCount > 0)
            {
                Console.WriteLine($"?? SUCCESS! {successCount} tables were successfully updated!");
            }
            else
            {
                Console.WriteLine("? No tables were successfully processed!");
            }

            Console.WriteLine();
            Console.WriteLine("?? CSV import completed!");

            return successCount > 0 ? 0 : 1;
        }

        private static bool ProcessTableMapping(string schemaDir, TableMapping mapping, List<ConstraintTable> constraints)
        {
            if (string.Equals(mapping.Action, "Ignore", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"??  Ignoring table '{mapping.BeforeTable}' as specified");
                
                // For ignored tables, also mark them as ignored in their schema
                var ignoreSchemaPath1 = Path.Combine(schemaDir, mapping.BeforeTable + ".schema.json");
                var ignoreSchemaPath2 = Path.Combine(schemaDir, mapping.BeforeTable + ".json");
                
                string ignoreSchemaPath = null;
                if (File.Exists(ignoreSchemaPath1))
                {
                    ignoreSchemaPath = ignoreSchemaPath1;
                }
                else if (File.Exists(ignoreSchemaPath2))
                {
                    ignoreSchemaPath = ignoreSchemaPath2;
                }

                if (ignoreSchemaPath != null)
                {
                    try
                    {
                        var ignoreSchemaJson = File.ReadAllText(ignoreSchemaPath);
                        dynamic ignoreSchema = JsonConvert.DeserializeObject(ignoreSchemaJson);
                        
                        if (ignoreSchema?.Plan == null)
                        {
                            ignoreSchema.Plan = new { Ignore = true };
                        }
                        else
                        {
                            ignoreSchema.Plan.Ignore = true;
                        }

                        var ignoreUpdatedJson = JsonConvert.SerializeObject(ignoreSchema, Formatting.Indented);
                        File.WriteAllText(ignoreSchemaPath, ignoreUpdatedJson);
                        Console.WriteLine($"    ??  Marked schema as ignored: {ignoreSchemaPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"    ??  Could not update ignored schema: {ex.Message}");
                    }
                }
                
                return true;
            }

            Console.WriteLine($"?? Processing table '{mapping.BeforeTable}' -> '{mapping.AfterTable}'");
            
            // Look for schema file
            var primaryPath = Path.Combine(schemaDir, mapping.BeforeTable + ".schema.json");
            var fallbackPath = Path.Combine(schemaDir, mapping.BeforeTable + ".json");
            
            Console.WriteLine($"    ?? Looking for schema file:");
            Console.WriteLine($"      Primary: {primaryPath}");
            Console.WriteLine($"      Fallback: {fallbackPath}");

            string schemaPath = null;
            if (File.Exists(primaryPath))
            {
                schemaPath = primaryPath;
                Console.WriteLine($"    ? Found primary schema file");
            }
            else if (File.Exists(fallbackPath))
            {
                schemaPath = fallbackPath;
                Console.WriteLine($"    ? Found fallback schema file");
            }
            else
            {
                Console.WriteLine($"    ? Schema file not found for table {mapping.BeforeTable}");
                return false;
            }

            // Load existing schema - try to deserialize as TableSchema from AccessSchemaExporter
            var schemaJson = File.ReadAllText(schemaPath);
            
            // Try to read as the proper AccessSchemaExporter.TableSchema format
            dynamic schema;
            try
            {
                schema = JsonConvert.DeserializeObject(schemaJson);
                if (schema == null)
                {
                    Console.WriteLine($"    ? Failed to deserialize schema file");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ? Error parsing schema file: {ex.Message}");
                return false;
            }

            // Ensure we have a Plan object
            if (schema.Plan == null)
            {
                schema.Plan = new
                {
                    Classification = "Copy",
                    TargetTable = mapping.BeforeTable,
                    ColumnActions = new List<object>(),
                    Ignore = false
                };
            }

            // Clear any ignore flag since we're processing this table
            if (schema.Plan.Ignore != null)
            {
                schema.Plan.Ignore = false;
            }

            // Update table name if needed
            var targetTableName = mapping.AfterTable ?? mapping.BeforeTable;
            if (!string.Equals((string)schema.Plan.TargetTable, targetTableName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"    ??  Renaming table: {schema.Plan.TargetTable} -> {targetTableName}");
                schema.Plan.TargetTable = targetTableName;
            }

            // Set action classification
            if (string.Equals(mapping.Action, "Copy", StringComparison.OrdinalIgnoreCase))
            {
                schema.Plan.Classification = "Copy";
            }
            else if (string.Equals(mapping.Action, "Rename", StringComparison.OrdinalIgnoreCase))
            {
                schema.Plan.Classification = "Rename";
            }
            else if (string.Equals(mapping.Action, "Normalize", StringComparison.OrdinalIgnoreCase))
            {
                schema.Plan.Classification = "Normalize";
                
                    // Handle normalization specific settings
                    if (mapping.NormalizationInfo != null)
                    {
                        Console.WriteLine($"    ?? Setting up normalization: {mapping.BeforeTable} -> {mapping.NormalizationInfo.HeaderTable} + {mapping.NormalizationInfo.LinesTable}");
                        
                        // Create or update the Normalize object in the plan
                        if (schema.Plan.Normalize == null)
                        {
                            schema.Plan.Normalize = new
                            {
                                HeaderTable = mapping.NormalizationInfo.HeaderTable,
                                LineTable = mapping.NormalizationInfo.LinesTable,
                                IsHeader = false,
                                IsLine = false,
                                NormalizeInto = new[]
                                {
                                    new { TargetTable = mapping.NormalizationInfo.HeaderTable, Description = "Header table" },
                                    new { TargetTable = mapping.NormalizationInfo.LinesTable, Description = "Lines table" }
                                }
                            };
                        }
                        else
                        {
                            schema.Plan.Normalize.HeaderTable = mapping.NormalizationInfo.HeaderTable;
                            schema.Plan.Normalize.LineTable = mapping.NormalizationInfo.LinesTable;
                        }
                        
                        // CRITICAL: Do NOT mark normalization tables as ignored - they need column mappings applied!
                        // The custom normalizer will handle the special migration logic
                        schema.Plan.Ignore = false;
                        
                        Console.WriteLine($"    ?? Normalization setup complete, table will be handled by custom normalizer");
                        Console.WriteLine($"    ?? IMPORTANT: Normalized tables need column mappings applied to Plan.ColumnActions for correct DDL generation!");
                    }
            }

            // Apply column mappings
            int appliedMappings = 0;
            var existingActions = new List<Dictionary<string, string>>();
            
            if (mapping.ColumnMappings?.Any() == true)
            {
                Console.WriteLine($"    ?? Processing {mapping.ColumnMappings.Count} column mappings:");
                
                // Initialize ColumnActions if missing
                if (schema.Plan.ColumnActions == null)
                {
                    schema.Plan.ColumnActions = new List<object>();
                }

                // Convert existing ColumnActions to a workable list
                if (schema.Plan.ColumnActions != null)
                {
                    // Handle JArray from JSON deserialization
                    if (schema.Plan.ColumnActions is Newtonsoft.Json.Linq.JArray jArray)
                    {
                        foreach (var item in jArray)
                        {
                            existingActions.Add(new Dictionary<string, string>
                            {
                                ["Source"] = item["Source"]?.ToString() ?? "",
                                ["Target"] = item["Target"]?.ToString() ?? "",
                                ["Action"] = item["Action"]?.ToString() ?? "",
                                ["Expression"] = item["Expression"]?.ToString() ?? ""
                            });
                        }
                    }
                    else if (schema.Plan.ColumnActions is IEnumerable<object> objList)
                    {
                        foreach (dynamic action in objList)
                        {
                            existingActions.Add(new Dictionary<string, string>
                            {
                                ["Source"] = action.Source?.ToString() ?? "",
                                ["Target"] = action.Target?.ToString() ?? "",
                                ["Action"] = action.Action?.ToString() ?? "",
                                ["Expression"] = action.Expression?.ToString() ?? ""
                            });
                        }
                    }
                }

                // Process each column mapping
                foreach (var columnMapping in mapping.ColumnMappings)
                {
                    if (string.IsNullOrEmpty(columnMapping.BeforeColumn))
                        continue;

                    Console.WriteLine($"      ?? Processing column: {columnMapping.BeforeColumn} -> {columnMapping.AfterColumn} (Action: {columnMapping.Action}, Target: {columnMapping.NormalizationTarget ?? "N/A"})");

                    // Find existing action for this column
                    var existingAction = existingActions.FirstOrDefault(a => 
                        string.Equals(a["Source"], columnMapping.BeforeColumn, StringComparison.OrdinalIgnoreCase));

                    if (existingAction != null)
                    {
                        // Update existing action
                        if (string.Equals(columnMapping.Action, "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"        ???  Dropping column: {columnMapping.BeforeColumn}");
                            existingAction["Action"] = "Drop";
                            existingAction["Target"] = columnMapping.BeforeColumn; // Keep original for dropped columns
                        }
                        else if (!string.IsNullOrEmpty(columnMapping.AfterColumn))
                        {
                            Console.WriteLine($"        ?? Column mapping: {columnMapping.BeforeColumn} -> {columnMapping.AfterColumn}");
                            existingAction["Target"] = columnMapping.AfterColumn;
                            
                            // CRITICAL: Properly detect rename vs copy with OCD precision
                            var isRename = !string.Equals(columnMapping.BeforeColumn, columnMapping.AfterColumn, StringComparison.OrdinalIgnoreCase);
                            existingAction["Action"] = isRename ? "Rename" : "Copy";
                            
                            Console.WriteLine($"          ??? Action determined: {existingAction["Action"]} (Source: {columnMapping.BeforeColumn}, Target: {columnMapping.AfterColumn})");
                            
                            // Add normalization target info if available
                            if (!string.IsNullOrEmpty(columnMapping.NormalizationTarget))
                            {
                                existingAction["NormalizationTarget"] = columnMapping.NormalizationTarget;
                                Console.WriteLine($"          ??? Normalization target: {columnMapping.NormalizationTarget}");
                            }
                        }
                        appliedMappings++;
                    }
                    else if (string.Equals(columnMapping.Action, "Drop", StringComparison.OrdinalIgnoreCase))
                    {
                        // Add new DROP column action
                        Console.WriteLine($"        ? New DROP column mapping: {columnMapping.BeforeColumn} (Action: Drop)");
                        
                        var newAction = new Dictionary<string, string>
                        {
                            ["Source"] = columnMapping.BeforeColumn,
                            ["Target"] = columnMapping.BeforeColumn, // Keep original for dropped columns
                            ["Action"] = "Drop",
                            ["Expression"] = ""
                        };
                        
                        Console.WriteLine($"          ??? New DROP action added for: {columnMapping.BeforeColumn}");
                        
                        existingActions.Add(newAction);
                        appliedMappings++;
                    }
                    else if (!string.IsNullOrEmpty(columnMapping.AfterColumn))
                    {
                        // Add new column action for Copy/Rename
                        var targetInfo = !string.IsNullOrEmpty(columnMapping.NormalizationTarget) 
                            ? $" ({columnMapping.NormalizationTarget})" 
                            : "";
                        Console.WriteLine($"        ? New column mapping: {columnMapping.BeforeColumn} -> {columnMapping.AfterColumn}{targetInfo}");
                        
                        var newAction = new Dictionary<string, string>
                        {
                            ["Source"] = columnMapping.BeforeColumn,
                            ["Target"] = columnMapping.AfterColumn,
                            ["Action"] = string.Equals(columnMapping.BeforeColumn, columnMapping.AfterColumn, StringComparison.OrdinalIgnoreCase) ? "Copy" : "Rename",
                            ["Expression"] = ""
                        };
                        
                        Console.WriteLine($"          ??? New action determined: {newAction["Action"]} (Source: {columnMapping.BeforeColumn}, Target: {columnMapping.AfterColumn})");
                        
                        // Add normalization target info if available
                        if (!string.IsNullOrEmpty(columnMapping.NormalizationTarget))
                        {
                            newAction["NormalizationTarget"] = columnMapping.NormalizationTarget;
                            Console.WriteLine($"          ??? Normalization target: {columnMapping.NormalizationTarget}");
                        }
                        
                        existingActions.Add(newAction);
                        appliedMappings++;
                    }
                }

                // Don't update schema.Plan.ColumnActions here as it causes JSON serialization issues
                // We'll apply the changes via text replacement instead
            }
            else
            {
                Console.WriteLine($"    ??  No column mappings to apply");
                
                // Still need to populate existingActions from the schema for serialization
                if (schema.Plan.ColumnActions != null)
                {
                    if (schema.Plan.ColumnActions is Newtonsoft.Json.Linq.JArray jArray)
                    {
                        foreach (var item in jArray)
                        {
                            existingActions.Add(new Dictionary<string, string>
                            {
                                ["Source"] = item["Source"]?.ToString() ?? "",
                                ["Target"] = item["Target"]?.ToString() ?? "",
                                ["Action"] = item["Action"]?.ToString() ?? "",
                                ["Expression"] = item["Expression"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            Console.WriteLine($"    ? Applied {appliedMappings} column mappings");

            // Create constraints for this table
            CreateConstraintsForTable(mapping, constraints);

            // Save updated schema - Use text-based approach to avoid JSON serialization issues
            try
            {
                var originalJson = File.ReadAllText(schemaPath);
                var updatedJson = originalJson;
                bool hasUpdates = false;
                
                // Update TargetTable
                if (!string.Equals((string)schema.Plan.TargetTable, mapping.BeforeTable, StringComparison.OrdinalIgnoreCase))
                {
                    updatedJson = System.Text.RegularExpressions.Regex.Replace(
                        updatedJson, 
                        @"""TargetTable"":\s*""[^""]*""", 
                        $@"""TargetTable"": ""{targetTableName}""");
                    hasUpdates = true;
                    Console.WriteLine($"    ??  Updated TargetTable: {targetTableName}");
                }
                
                // Update Classification
                if (!string.IsNullOrEmpty((string)schema.Plan.Classification))
                {
                    updatedJson = System.Text.RegularExpressions.Regex.Replace(
                        updatedJson, 
                        @"""Classification"":\s*""[^""]*""", 
                        $@"""Classification"": ""{schema.Plan.Classification}""");
                    hasUpdates = true;
                    Console.WriteLine($"    ??  Updated Classification: {schema.Plan.Classification}");
                }
                
                // Update Ignore flag for normalized tables
                if (schema.Plan.Ignore != null)
                {
                    updatedJson = System.Text.RegularExpressions.Regex.Replace(
                        updatedJson, 
                        @"""Ignore"":\s*(true|false)", 
                        $@"""Ignore"": {schema.Plan.Ignore.ToString().ToLower()}");
                    hasUpdates = true;
                    Console.WriteLine($"    ??  Updated Ignore: {schema.Plan.Ignore}");
                }
                
                // Apply key column mappings using proper JSON manipulation instead of fragile regex
                if (appliedMappings > 0)
                {
                    Console.WriteLine($"    ?? Applying {appliedMappings} column mappings to schema JSON...");
                    
                    try
                    {
                        // Use proper JSON deserialization/serialization to update ColumnActions
                        var schemaObj = JsonConvert.DeserializeObject<dynamic>(updatedJson);
                        var columnActions = schemaObj?.Plan?.ColumnActions as Newtonsoft.Json.Linq.JArray;
                        
                        if (columnActions != null)
                        {
                            int updatedColumns = 0;
                            
                            // Apply all column mappings from our processed list
                            foreach (var processedAction in existingActions)
                            {
                                if (string.IsNullOrEmpty(processedAction["Source"])) continue;
                                
                                // Find corresponding JSON action
                                foreach (var jsonAction in columnActions)
                                {
                                    var sourceValue = jsonAction["Source"]?.ToString();
                                    if (string.Equals(sourceValue, processedAction["Source"], StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Update the JSON action with new values
                                        if (!string.IsNullOrEmpty(processedAction["Target"]))
                                        {
                                            var oldTarget = jsonAction["Target"]?.ToString();
                                            var newTarget = processedAction["Target"];
                                            
                                            if (!string.Equals(oldTarget, newTarget, StringComparison.Ordinal))
                                            {
                                                jsonAction["Target"] = newTarget;
                                                updatedColumns++;
                                                Console.WriteLine($"      ??? Updated column: {processedAction["Source"]} Target: {oldTarget} -> {newTarget}");
                                            }
                                        }
                                        
                                        if (!string.IsNullOrEmpty(processedAction["Action"]))
                                        {
                                            var oldAction = jsonAction["Action"]?.ToString();
                                            var newAction = processedAction["Action"];
                                            
                                            if (!string.Equals(oldAction, newAction, StringComparison.Ordinal))
                                            {
                                                jsonAction["Action"] = newAction;
                                                Console.WriteLine($"      ??? Updated action: {processedAction["Source"]} Action: {oldAction} -> {newAction}");
                                            }
                                        }
                                        
                                        break; // Found the matching action, move to next
                                    }
                                }
                            }
                            
                            if (updatedColumns > 0)
                            {
                                updatedJson = JsonConvert.SerializeObject(schemaObj, Formatting.Indented);
                                hasUpdates = true;
                                Console.WriteLine($"    ?? Successfully updated {updatedColumns} column mappings in schema JSON");
                            }
                            else
                            {
                                Console.WriteLine($"    ??  No column mapping updates were needed in schema JSON");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"    ??  Warning: Could not find ColumnActions array in schema JSON");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"    ??  Error applying JSON-based column mappings: {ex.Message}");
                        Console.WriteLine($"    ??  Attempting regex fallback for key mappings...");
                        
                        // Fallback to regex for critical mappings only
                        foreach (var columnMapping in mapping.ColumnMappings.Where(cm => !string.IsNullOrEmpty(cm.BeforeColumn) && !string.IsNullOrEmpty(cm.AfterColumn)))
                        {
                            var isRename = !string.Equals(columnMapping.BeforeColumn, columnMapping.AfterColumn, StringComparison.OrdinalIgnoreCase);
                            
                            if (isRename)
                            {
                                try
                                {
                                    // Multi-line regex patterns for more reliable matching
                                    var multiLineTargetPattern = @"""Source"":\s*""" + System.Text.RegularExpressions.Regex.Escape(columnMapping.BeforeColumn) + @""",\s*""Target"":\s*""[^""]*""";
                                    var targetReplacement = "\"Source\": \"" + columnMapping.BeforeColumn + "\",\r\n        \"Target\": \"" + columnMapping.AfterColumn + "\"";
                                    
                                    if (System.Text.RegularExpressions.Regex.IsMatch(updatedJson, multiLineTargetPattern, System.Text.RegularExpressions.RegexOptions.Multiline))
                                    {
                                        updatedJson = System.Text.RegularExpressions.Regex.Replace(updatedJson, multiLineTargetPattern, targetReplacement, System.Text.RegularExpressions.RegexOptions.Multiline);
                                        hasUpdates = true;
                                        Console.WriteLine($"      ?? Applied column target mapping (regex fallback): {columnMapping.BeforeColumn} -> {columnMapping.AfterColumn}");
                                    }
                                }
                                catch (Exception regexEx)
                                {
                                    Console.WriteLine($"      ??  Regex fallback also failed for {columnMapping.BeforeColumn}: {regexEx.Message}");
                                }
                            }
                        }
                    }
                }
                
                if (hasUpdates)
                {
                    File.WriteAllText(schemaPath, updatedJson);
                    Console.WriteLine($"    ? Successfully saved schema with {appliedMappings} column mappings: {mapping.BeforeTable} -> {targetTableName}");
                }
                else
                {
                    Console.WriteLine($"    ??  No updates needed for schema: {mapping.BeforeTable}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ? Error saving schema for {mapping.BeforeTable}: {ex.Message}");
                return false;
            }

            return true;
        }

        private static void CreateConstraintsForTable(TableMapping mapping, List<ConstraintTable> constraints)
        {
            if (string.Equals(mapping.Action, "Ignore", StringComparison.OrdinalIgnoreCase))
                return;

            var targetTable = mapping.AfterTable ?? mapping.BeforeTable;
            
            var constraintTable = new ConstraintTable
            {
                Table = targetTable,
                PrimaryKey = new List<string>(),
                IdentityColumns = new List<string>(),
                ForeignKeys = new List<ForeignKeyDef>(),
                NotNullColumns = new List<string>()
            };

            // FIRST: Look for columns explicitly marked as PK in the CSV (most reliable)
            var pkColumns = mapping.ColumnMappings?
                .Where(cm => cm.IsPrimaryKey && !string.IsNullOrEmpty(cm.AfterColumn))
                .ToList();

            if (pkColumns?.Any() == true)
            {
                foreach (var pkCol in pkColumns)
                {
                    constraintTable.PrimaryKey.Add(pkCol.AfterColumn);
                    
                    // Add to identity columns if marked as identity
                    if (pkCol.IsIdentity)
                    {
                        constraintTable.IdentityColumns.Add(pkCol.AfterColumn);
                    }
                    
                    Console.WriteLine($"    ⮞ Creating constraints for {targetTable}: PK={pkCol.AfterColumn} (from CSV After Key=PK, Identity={pkCol.IsIdentity})");
                }
            }
            // FALLBACK: Look for ID columns (legacy heuristic logic)
            else if (mapping.ColumnMappings?.Any() == true)
            {
                // Find columns that are likely primary keys (end with "ID")
                // EXCLUDE columns that are foreign keys!
                var idColumns = mapping.ColumnMappings
                    .Where(cm => !string.IsNullOrEmpty(cm.BeforeColumn) && 
                                !string.IsNullOrEmpty(cm.AfterColumn) &&
                                cm.BeforeColumn.EndsWith("ID", StringComparison.OrdinalIgnoreCase) &&
                                !cm.IsForeignKey)  // Exclude columns marked as FK in CSV
                    .ToList();

                // For normalized tables, look for the table-specific ID (e.g., OrderID for OrdersTbl)
                // Priority order: 1) Table-specific ID, 2) Generic "ID", 3) Others
                var tableBaseName = mapping.BeforeTable.Replace("Tbl", "");
                var expectedPkName = tableBaseName + "ID"; // e.g., "OrderID" for "OrdersTbl"
                
                var pkColumn = idColumns.FirstOrDefault(cm => 
                    string.Equals(cm.BeforeColumn, expectedPkName, StringComparison.OrdinalIgnoreCase)) ??
                    idColumns.FirstOrDefault(cm => 
                        string.Equals(cm.BeforeColumn, "ID", StringComparison.OrdinalIgnoreCase)) ??
                    idColumns.FirstOrDefault(); // Fallback to any ID column

                if (pkColumn != null)
                {
                    var finalPkName = pkColumn.AfterColumn;
                    Console.WriteLine($"    ⮞ Creating constraints for {targetTable}: PK={finalPkName} (heuristic fallback from {pkColumn.BeforeColumn})");
                    
                    constraintTable.PrimaryKey.Add(finalPkName);
                    constraintTable.IdentityColumns.Add(finalPkName);
                }
                else
                {
                    Console.WriteLine($"    ⚠ No obvious primary key found for {targetTable}");
                }
            }

            // Extract foreign keys from column mappings
            // NOTE: We store the FK column and ref table, but defer determining the actual
            // ref column until after all constraints are built (so we can look up the PK)
            if (mapping.ColumnMappings?.Any() == true)
            {
                var fkColumns = mapping.ColumnMappings
                    .Where(cm => cm.IsForeignKey && 
                                !string.IsNullOrEmpty(cm.AfterColumn) && 
                                !string.IsNullOrEmpty(cm.ForeignKeyRefTable))
                    .ToList();

                foreach (var fkCol in fkColumns)
                {
                    // Store FK with RefColumn = null for now; we'll resolve it in a second pass
                    constraintTable.ForeignKeys.Add(new ForeignKeyDef
                    {
                        Column = fkCol.AfterColumn,
                        RefTable = fkCol.ForeignKeyRefTable,
                        RefColumn = null  // Will be resolved in second pass
                    });
                    
                    Console.WriteLine($"    ⮞ Adding FK (pending ref column resolution): {fkCol.AfterColumn} -> {fkCol.ForeignKeyRefTable}");
                }
            }

            constraints.Add(constraintTable);

            // CRITICAL FIX: For normalized tables, also create constraints for line tables
            if (string.Equals(mapping.Action, "Normalize", StringComparison.OrdinalIgnoreCase) &&
                mapping.NormalizationInfo != null)
            {
                Console.WriteLine($"    ?? Creating constraints for normalized line table: {mapping.NormalizationInfo.LinesTable}");
                
                var lineConstraintTable = new ConstraintTable
                {
                    Table = mapping.NormalizationInfo.LinesTable,
                    PrimaryKey = new List<string>(),
                    IdentityColumns = new List<string>(),
                    ForeignKeys = new List<ForeignKeyDef>(),
                    NotNullColumns = new List<string>()
                };
                
                // Determine line table primary key based on naming convention
                var lineTableBaseName = mapping.NormalizationInfo.LinesTable.Replace("Tbl", "");
                var linePkName = lineTableBaseName + "ID"; // e.g., "OrderLineID" for "OrderLinesTbl"
                
                // Special case handling for common patterns
                if (lineTableBaseName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                {
                    // "OrderLines" -> "OrderLineID", "RecurringOrderItems" -> "RecurringOrderItemID"
                    linePkName = lineTableBaseName.TrimEnd('s') + "ID";
                }
                
                Console.WriteLine($"    ??   Line table PK: {linePkName}");
                
                lineConstraintTable.PrimaryKey.Add(linePkName);
                lineConstraintTable.IdentityColumns.Add(linePkName);
                
                // Add foreign key back to header table
                var headerPkName = constraintTable.PrimaryKey.FirstOrDefault();
                if (!string.IsNullOrEmpty(headerPkName))
                {
                    lineConstraintTable.ForeignKeys.Add(new ForeignKeyDef
                    {
                        Column = headerPkName, // Same column name in line table
                        RefTable = mapping.NormalizationInfo.HeaderTable,
                        RefColumn = headerPkName
                    });
                    Console.WriteLine($"    ??   Line table FK: {headerPkName} -> {mapping.NormalizationInfo.HeaderTable}.{headerPkName}");
                }
                
                constraints.Add(lineConstraintTable);
            }
        }

        private static void ResolveForeignKeyReferences(List<ConstraintTable> constraints)
        {
            // Build a lookup dictionary of table name -> primary key column
            var tablePkLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var constraint in constraints)
            {
                if (constraint.PrimaryKey != null && constraint.PrimaryKey.Count > 0)
                {
                    // For now, assume single-column PKs (composite PKs are rare in this schema)
                    tablePkLookup[constraint.Table] = constraint.PrimaryKey[0];
                }
            }
            
            Console.WriteLine($"Built PK lookup for {tablePkLookup.Count} tables");
            
            // Now resolve all FK reference columns
            int resolvedCount = 0;
            int unresolvedCount = 0;
            
            foreach (var constraint in constraints)
            {
                if (constraint.ForeignKeys == null || constraint.ForeignKeys.Count == 0)
                    continue;
                    
                foreach (var fk in constraint.ForeignKeys)
                {
                    if (fk.RefColumn == null && !string.IsNullOrEmpty(fk.RefTable))
                    {
                        if (tablePkLookup.TryGetValue(fk.RefTable, out var refPk))
                        {
                            fk.RefColumn = refPk;
                            resolvedCount++;
                            Console.WriteLine($"  ✓ Resolved FK: {constraint.Table}.{fk.Column} -> {fk.RefTable}.{refPk}");
                        }
                        else
                        {
                            // Fallback: try table name + "ID" pattern
                            var refTableBase = fk.RefTable.Replace("Tbl", "");
                            fk.RefColumn = refTableBase + "ID";
                            unresolvedCount++;
                            Console.WriteLine($"  ⚠ FK fallback (table not found): {constraint.Table}.{fk.Column} -> {fk.RefTable}.{fk.RefColumn}");
                        }
                    }
                }
            }
            
            Console.WriteLine($"FK resolution complete: {resolvedCount} resolved, {unresolvedCount} used fallback");
        }

        private static string GenerateConstraintsSummary(List<ConstraintTable> constraints)
        {
            var tables = constraints.Count;
            var pkCols = constraints.SelectMany(c => c.PrimaryKey).Count();
            var identityCols = constraints.SelectMany(c => c.IdentityColumns).Count();
            var fks = constraints.SelectMany(c => c.ForeignKeys).Count();
            var notNullCols = constraints.SelectMany(c => c.NotNullColumns).Count();

            return $"Tables={tables}, PKCols={pkCols}, IdentityCols={identityCols}, FKs={fks}, NotNullCols={notNullCols}";
        }

        private static List<TableMapping> ParseCsv(string csvPath)
        {
            var lines = File.ReadAllLines(csvPath);
            var mappings = new List<TableMapping>();

            if (lines.Length == 0) return mappings;

            Console.WriteLine($"?? Parsing CSV with {lines.Length} total lines");

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                
                // Skip empty lines and report headers
                if (string.IsNullOrEmpty(line) || line.StartsWith("Table Migration report:") || line.StartsWith("====") || line.StartsWith("-------"))
                    continue;

                Console.WriteLine($"?? Processing line {i + 1}: {line.Substring(0, Math.Min(80, line.Length))}...");

                // Handle normalization section differently
                if (line.StartsWith("Table:,Before,After Header Tbl,After Lines Tbl,Action"))
                {
                    Console.WriteLine($"?? Found NORMALIZATION section at line {i + 1}");
                    
                    // Move to next line to find the table data
                    i++;
                    if (i >= lines.Length) continue;
                    
                    var tableDataLine = lines[i].Trim();
                    Console.WriteLine($"?? Next line: {tableDataLine}");
                    
                    // Process the table data line (starts with "=====")
                    if (tableDataLine.StartsWith("====="))
                    {
                        Console.WriteLine($"??   Processing table data line: {tableDataLine.Substring(0, Math.Min(80, tableDataLine.Length))}...");
                        
                        var normParts = ParseCsvLine(tableDataLine);
                        if (normParts.Length >= 5)
                        {
                            var beforeTable = normParts[1]?.Trim();
                            var afterHeaderTable = normParts[2]?.Trim();
                            var afterLinesTable = normParts[3]?.Trim();
                            var action = normParts[4]?.Trim();
                            
                            Console.WriteLine($"??   Parsed: Before={beforeTable}, HeaderTbl={afterHeaderTable}, LinesTbl={afterLinesTable}, Action={action}");
                            
                            if (!string.IsNullOrEmpty(beforeTable) && 
                                string.Equals(action, "Normalise", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"?? Found normalization: {beforeTable} -> {afterHeaderTable} + {afterLinesTable}");
                                
                                var mapping = new TableMapping
                                {
                                    BeforeTable = beforeTable,
                                    AfterTable = afterHeaderTable, // Use header table as the target table name
                                    Action = "Normalize",
                                    ColumnMappings = new List<ColumnMapping>(),
                                    NormalizationInfo = new NormalizationMapping
                                    {
                                        HeaderTable = afterHeaderTable,
                                        LinesTable = afterLinesTable
                                    }
                                };
                                
                                // Parse normalization column mappings
                                i = ParseNormalizationColumns(lines, i, mapping);
                                mappings.Add(mapping);
                                
                                Console.WriteLine($"?? Added normalization mapping for {beforeTable}");
                            }
                        }
                    }
                    continue; // Continue to process more sections instead of breaking
                }
                // Look for standard table definition lines: "Table,Before,After,Action"
                else if (line.StartsWith("Table,Before,After,Action"))
                {
                    // Parse the table section
                    i++; // Move to next line (the actual table data)
                    if (i >= lines.Length) break;
                    
                    var tableDataLine = lines[i].Trim();
                    if (string.IsNullOrEmpty(tableDataLine)) continue;
                    
                    var tableParts = ParseCsvLine(tableDataLine);
                    if (tableParts.Length < 4) continue;
                    
                    var beforeTable = tableParts[1]?.Trim(); // Skip first empty column
                    var afterTable = tableParts[2]?.Trim();
                    var action = tableParts[3]?.Trim();
                    
                    if (string.IsNullOrEmpty(beforeTable)) continue;
                    
                    // Clean up AfterTable
                    if (string.IsNullOrEmpty(afterTable) || string.Equals(afterTable, "n/a", StringComparison.OrdinalIgnoreCase))
                    {
                        afterTable = null;
                    }
                    
                    // Default action if empty
                    if (string.IsNullOrEmpty(action))
                    {
                        action = "Copy";
                    }
                    
                    var mapping = new TableMapping
                    {
                        BeforeTable = beforeTable,
                        AfterTable = afterTable,
                        Action = action,
                        ColumnMappings = new List<ColumnMapping>()
                    };
                    
                    Console.WriteLine($"?? Found table: {beforeTable} -> {afterTable ?? "same"} ({action})");
                    
                    // Now look for the column mappings that follow
                    i++; // Move to next line
                    if (i < lines.Length)
                    {
                        var columnHeaderLine = lines[i].Trim();
                        
                        // Check if this is a column definition section
                        if (columnHeaderLine.StartsWith("Rows:,Before Col name"))
                        {
                            Console.WriteLine($"    ?? Found column section for {beforeTable}");
                            
                            // Parse all column mappings for this table
                            i++; // Move to first column data line
                            int columnCount = 0;
                            
                            while (i < lines.Length)
                            {
                                var columnLine = lines[i].Trim();
                                
                                // Stop when we hit a section separator or another table
                                if (string.IsNullOrEmpty(columnLine) || 
                                    columnLine.StartsWith("------") ||
                                    columnLine.StartsWith("Table,Before,After,Action") ||
                                    columnLine.StartsWith("Table:,"))
                                {
                                    break;
                                }
                                
                                // Parse column mapping
                                var columnParts = ParseCsvLine(columnLine);
                                if (columnParts.Length >= 13) // We need at least 13 columns to get the Action and Source
                                {
                                    var beforeCol = columnParts[1]?.Trim(); // Before Col name
                                    var afterCol = columnParts[6]?.Trim();  // After Col Name
                                    var afterKey = columnParts[8]?.Trim();  // After Key (PK, FK, No)
                                    var afterAuto = columnParts[9]?.Trim(); // After Auto (Yes/No)
                                    var colAction = columnParts[12]?.Trim(); // Action

                                    // IMPROVED: Try different column positions for Action if standard position is empty
                                    if (string.IsNullOrEmpty(colAction) && columnParts.Length > 12)
                                    {
                                        // Check other common positions for the Action column
                                        for (int actionIndex = 11; actionIndex < Math.Min(columnParts.Length, 16); actionIndex++)
                                        {
                                            var potentialAction = columnParts[actionIndex]?.Trim();
                                            if (!string.IsNullOrEmpty(potentialAction) && 
                                                (string.Equals(potentialAction, "Drop", StringComparison.OrdinalIgnoreCase) ||
                                                 string.Equals(potentialAction, "Copy", StringComparison.OrdinalIgnoreCase) ||
                                                 string.Equals(potentialAction, "Rename", StringComparison.OrdinalIgnoreCase)))
                                            {
                                                colAction = potentialAction;
                                                Console.WriteLine($"        ??? Found Action '{colAction}' at column index {actionIndex}");
                                                break;
                                            }
                                        }
                                    }
                                    
                                    if (!string.IsNullOrEmpty(beforeCol))
                                    {
                                        // Handle dropped columns (empty after column OR explicit Drop action)
                                        if (string.IsNullOrEmpty(afterCol) || string.Equals(colAction, "Drop", StringComparison.OrdinalIgnoreCase))
                                        {
                                            colAction = "Drop";
                                            afterCol = null; // Ensure afterCol is null for dropped columns
                                            Console.WriteLine($"        ??? Detected DROP column: {beforeCol} (afterCol empty or explicit Drop action)");
                                        }
                                        
                                        // Default column action
                                        if (string.IsNullOrEmpty(colAction))
                                        {
                                            colAction = string.Equals(beforeCol, afterCol, StringComparison.OrdinalIgnoreCase) ? "Copy" : "Rename";
                                        }
                                        
                                        // Detect primary key, identity, and foreign key from CSV
                                        var isPrimaryKey = string.Equals(afterKey, "PK", StringComparison.OrdinalIgnoreCase);
                                        var isIdentity = string.Equals(afterAuto, "Yes", StringComparison.OrdinalIgnoreCase);
                                        var isForeignKey = (afterKey ?? "").StartsWith("FK", StringComparison.OrdinalIgnoreCase);
                                        string fkRefTable = null;
                                        
                                        if (isForeignKey)
                                        {
                                            // Parse FK (TableName) from afterKey - extract table name between parentheses
                                            var match = System.Text.RegularExpressions.Regex.Match(afterKey, @"FK\s*\(([^)]+)\)");
                                            if (match.Success)
                                            {
                                                fkRefTable = match.Groups[1].Value.Trim();
                                            }
                                        }
                                        
                                        var columnMapping = new ColumnMapping
                                        {
                                            BeforeColumn = beforeCol,
                                            AfterColumn = afterCol,
                                            Action = colAction,
                                            IsPrimaryKey = isPrimaryKey,
                                            IsIdentity = isIdentity,
                                            IsForeignKey = isForeignKey,
                                            ForeignKeyRefTable = fkRefTable
                                        };
                                        
                                        mapping.ColumnMappings.Add(columnMapping);
                                        columnCount++;

                                        var pkMarker = isPrimaryKey ? " [PK]" : "";
                                        var identityMarker = isIdentity ? " [IDENTITY]" : "";
                                        var fkMarker = isForeignKey ? $" [FK->{fkRefTable}]" : "";
                                        Console.WriteLine($"      ⮞ Column: {beforeCol} -> {afterCol ?? "DROP"} ({colAction}){pkMarker}{identityMarker}{fkMarker}");
                                    }
                                    // Handle NEW columns (no before column, but has after column)
                                    else if (!string.IsNullOrEmpty(afterCol))
                                    {
                                        if (string.IsNullOrEmpty(colAction))
                                            colAction = "New";
                                        
                                        // Detect primary key, identity, and foreign key from CSV
                                        var isPrimaryKey = string.Equals(afterKey, "PK", StringComparison.OrdinalIgnoreCase);
                                        var isIdentity = string.Equals(afterAuto, "Yes", StringComparison.OrdinalIgnoreCase);
                                        var isForeignKey = (afterKey ?? "").StartsWith("FK", StringComparison.OrdinalIgnoreCase);
                                        string fkRefTable = null;
                                        
                                        if (isForeignKey)
                                        {
                                            // Parse FK (TableName) from afterKey
                                            var match = System.Text.RegularExpressions.Regex.Match(afterKey, @"FK\s*\(([^)]+)\)");
                                            if (match.Success)
                                            {
                                                fkRefTable = match.Groups[1].Value.Trim();
                                            }
                                        }
                                        
                                        var columnMapping = new ColumnMapping
                                        {
                                            BeforeColumn = afterCol,  // Use target name as source for NEW columns
                                            AfterColumn = afterCol,
                                            Action = colAction,
                                            IsPrimaryKey = isPrimaryKey,
                                            IsIdentity = isIdentity,
                                            IsForeignKey = isForeignKey,
                                            ForeignKeyRefTable = fkRefTable
                                        };
                                        
                                        mapping.ColumnMappings.Add(columnMapping);
                                        columnCount++;
                                        
                                        var pkMarker = isPrimaryKey ? " [PK]" : "";
                                        var identityMarker = isIdentity ? " [IDENTITY]" : "";
                                        var fkMarker = isForeignKey ? $" [FK->{fkRefTable}]" : "";
                                        Console.WriteLine($"      ⮞ NEW Column: {afterCol} ({colAction}){pkMarker}{identityMarker}{fkMarker}");
                                    }
                                }
                                
                                i++;
                            }
                            
                            Console.WriteLine($"    ?? Added {columnCount} column mappings for {beforeTable}");
                            i--; // Back up one since the outer loop will increment
                        }
                    }
                    
                    mappings.Add(mapping);
                }
            }

            Console.WriteLine($"?? Successfully parsed {mappings.Count} table mappings from CSV");
            
            // Summary of what we found
            var ignoredCount = mappings.Count(m => string.Equals(m.Action, "Ignore", StringComparison.OrdinalIgnoreCase));
            var renamedCount = mappings.Count(m => string.Equals(m.Action, "Rename", StringComparison.OrdinalIgnoreCase));
            var copiedCount = mappings.Count(m => string.Equals(m.Action, "Copy", StringComparison.OrdinalIgnoreCase));
            var normalizedCount = mappings.Count(m => string.Equals(m.Action, "Normalize", StringComparison.OrdinalIgnoreCase));
            var totalColumns = mappings.Sum(m => m.ColumnMappings?.Count ?? 0);
            
            Console.WriteLine($"?? Summary: {ignoredCount} ignored, {renamedCount} renamed, {copiedCount} copied, {normalizedCount} normalized, {totalColumns} column mappings");
            
            return mappings;
        }

        private static int ParseNormalizationColumns(string[] lines, int currentIndex, TableMapping mapping)
        {
            int i = currentIndex + 1;
            
            // Look for the header line with column definitions
            while (i < lines.Length)
            {
                var line = lines[i].Trim();
                
                if (line.StartsWith("Rows:,Before Col name"))
                {
                    Console.WriteLine($"    ?? Found normalization column section");
                    i++; // Move to first data row
                    break;
                }
                
                if (string.IsNullOrEmpty(line) || line.StartsWith("Table"))
                {
                    return i - 1; // No column section found
                }
                
                i++;
            }
            
            int columnCount = 0;
            
            // Parse normalization column mappings
            while (i < lines.Length)
            {
                var line = lines[i].Trim();
                
                // Stop at section separators or new tables
                if (string.IsNullOrEmpty(line) || 
                    line.StartsWith("------") ||
                    line.StartsWith("Table"))
                {
                    break;
                }
                
                var parts = ParseCsvLine(line);
                if (parts.Length >= 23) // Normalization has more columns
                {
                    var beforeCol = parts[1]?.Trim();
                    var headerCol = parts[6]?.Trim();
                    var linesCol = parts[15]?.Trim();
                    var action = parts[12]?.Trim();
                    
                    if (!string.IsNullOrEmpty(beforeCol))
                    {
                        // Determine where this column goes (header or lines)
                        var targetCol = !string.IsNullOrEmpty(headerCol) ? headerCol : linesCol;
                        var targetLocation = !string.IsNullOrEmpty(headerCol) ? "Header" : "Lines";
                        
                        if (!string.IsNullOrEmpty(targetCol))
                        {
                            var columnMapping = new ColumnMapping
                            {
                                BeforeColumn = beforeCol,
                                AfterColumn = targetCol,
                                Action = action ?? "Copy",
                                NormalizationTarget = targetLocation
                            };
                            
                            mapping.ColumnMappings.Add(columnMapping);
                            columnCount++;
                            
                            Console.WriteLine($"      ?? Normalization column: {beforeCol} -> {targetCol} ({targetLocation})");
                        }
                    }
                }
                
                i++;
            }
            
            Console.WriteLine($"    ?? Added {columnCount} normalization column mappings");
            
            return i - 1;
        }

        private static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var inQuotes = false;
            var currentValue = "";

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue += '"';
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue);
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }

            values.Add(currentValue);
            return values.ToArray();
        }

        // This method is called by the Menu system
        public static int Import(string migrationsDir, string csvPath, out string planLogPath)
        {
            return ImportPlan(migrationsDir, csvPath, out planLogPath);
        }
    }

    public class TableMapping
    {
        public string BeforeTable { get; set; }
        public string AfterTable { get; set; }
        public string Action { get; set; }
        public List<ColumnMapping> ColumnMappings { get; set; }
        public NormalizationMapping NormalizationInfo { get; set; }
    }

    public class ColumnMapping
    {
        public string BeforeColumn { get; set; }
        public string AfterColumn { get; set; }
        public string Action { get; set; }
        public string NormalizationTarget { get; set; } // "Header" or "Lines"
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsForeignKey { get; set; }
        public string ForeignKeyRefTable { get; set; }  // The referenced table name from FK (TableName)
    }

    public class NormalizationMapping
    {
        public string HeaderTable { get; set; }
        public string LinesTable { get; set; }
    }
}