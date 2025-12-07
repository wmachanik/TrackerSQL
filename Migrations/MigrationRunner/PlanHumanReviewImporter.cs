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
            else if (string.Equals(mapping.Action, "Normalise", StringComparison.OrdinalIgnoreCase))
            {
                schema.Plan.Classification = "Normalize";
            }

            // Apply column mappings
            int appliedMappings = 0;
            if (mapping.ColumnMappings?.Any() == true)
            {
                Console.WriteLine($"    ?? Processing {mapping.ColumnMappings.Count} column mappings:");
                
                // Initialize ColumnActions if missing
                if (schema.Plan.ColumnActions == null)
                {
                    schema.Plan.ColumnActions = new List<object>();
                }

                // Convert existing ColumnActions to a workable list
                var existingActions = new List<Dictionary<string, string>>();
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

                    // Find existing action for this column
                    var existingAction = existingActions.FirstOrDefault(a => 
                        string.Equals(a["Source"], columnMapping.BeforeColumn, StringComparison.OrdinalIgnoreCase));

                    if (existingAction != null)
                    {
                        // Update existing action
                        if (string.Equals(columnMapping.Action, "Drop", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"      ???  Dropping column: {columnMapping.BeforeColumn}");
                            existingAction["Action"] = "Drop";
                            existingAction["Target"] = columnMapping.BeforeColumn; // Keep original for dropped columns
                        }
                        else if (!string.IsNullOrEmpty(columnMapping.AfterColumn))
                        {
                            Console.WriteLine($"      ?? Column mapping: {columnMapping.BeforeColumn} -> {columnMapping.AfterColumn}");
                            existingAction["Target"] = columnMapping.AfterColumn;
                            existingAction["Action"] = string.Equals(columnMapping.BeforeColumn, columnMapping.AfterColumn, StringComparison.OrdinalIgnoreCase) ? "Copy" : "Rename";
                        }
                        appliedMappings++;
                    }
                    else if (!string.Equals(columnMapping.Action, "Drop", StringComparison.OrdinalIgnoreCase) && 
                             !string.IsNullOrEmpty(columnMapping.AfterColumn))
                    {
                        // Add new column action
                        Console.WriteLine($"      ? New column mapping: {columnMapping.BeforeColumn} -> {columnMapping.AfterColumn}");
                        existingActions.Add(new Dictionary<string, string>
                        {
                            ["Source"] = columnMapping.BeforeColumn,
                            ["Target"] = columnMapping.AfterColumn,
                            ["Action"] = string.Equals(columnMapping.BeforeColumn, columnMapping.AfterColumn, StringComparison.OrdinalIgnoreCase) ? "Copy" : "Rename",
                            ["Expression"] = ""
                        });
                        appliedMappings++;
                    }
                }

                // Update schema with the processed column actions
                schema.Plan.ColumnActions = existingActions;
            }
            else
            {
                Console.WriteLine($"    ??  No column mappings to apply");
            }

            Console.WriteLine($"    ? Applied {appliedMappings} column mappings");

            // Create constraints for this table
            CreateConstraintsForTable(mapping, constraints);

            // Save updated schema - Fix JSON serialization completely
            try
            {
                // Create a completely clean object for serialization
                var schemaDict = new Dictionary<string, object>();
                
                // Copy all properties from the original schema except Plan
                var originalSchema = (Newtonsoft.Json.Linq.JObject)schema;
                foreach (var property in originalSchema.Properties())
                {
                    if (property.Name != "Plan")
                    {
                        schemaDict[property.Name] = property.Value.ToObject<object>();
                    }
                }
                
                // Create a clean Plan object with explicit types
                var planDict = new Dictionary<string, object>
                {
                    ["Classification"] = (string)schema.Plan.Classification,
                    ["TargetTable"] = (string)schema.Plan.TargetTable,
                    ["PreserveIdsOnInsert"] = (bool)(schema.Plan.PreserveIdsOnInsert ?? false),
                    ["Reviewed"] = (bool)(schema.Plan.Reviewed ?? false),
                    ["Ignore"] = (bool)(schema.Plan.Ignore ?? false),
                    ["ColumnActions"] = schema.Plan.ColumnActions, // This should now be List<Dictionary<string,string>>
                    ["Normalize"] = schema.Plan.Normalize?.ToObject<object>()
                };
                
                schemaDict["Plan"] = planDict;
                
                var finalJson = JsonConvert.SerializeObject(schemaDict, Formatting.Indented);
                File.WriteAllText(schemaPath, finalJson);
                Console.WriteLine($"    ? Successfully saved schema: {mapping.BeforeTable} -> {targetTableName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ? Error saving schema for {mapping.BeforeTable}: {ex.Message}");
                Console.WriteLine($"    ?? Debug - TargetTable: {schema.Plan.TargetTable}, Classification: {schema.Plan.Classification}");
                
                // Try a simpler approach - just update the file directly as text
                try
                {
                    Console.WriteLine($"    ?? Attempting text-based update...");
                    var originalJson = File.ReadAllText(schemaPath);
                    
                    // Update TargetTable and Classification using string replacement
                    var updatedJson = originalJson;
                    if (!string.Equals((string)schema.Plan.TargetTable, mapping.BeforeTable, StringComparison.OrdinalIgnoreCase))
                    {
                        updatedJson = System.Text.RegularExpressions.Regex.Replace(
                            updatedJson, 
                            @"""TargetTable"":\s*""[^""]*""", 
                            $@"""TargetTable"": ""{targetTableName}""");
                    }
                    
                    if (schema.Plan.Classification != null)
                    {
                        updatedJson = System.Text.RegularExpressions.Regex.Replace(
                            updatedJson, 
                            @"""Classification"":\s*""[^""]*""", 
                            $@"""Classification"": ""{schema.Plan.Classification}""");
                    }
                    
                    File.WriteAllText(schemaPath, updatedJson);
                    Console.WriteLine($"    ? Text-based update successful for {mapping.BeforeTable}");
                }
                catch (Exception textEx)
                {
                    Console.WriteLine($"    ? Text update also failed: {textEx.Message}");
                    return false;
                }
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

            // Add typical primary key based on naming convention
            if (mapping.BeforeTable.EndsWith("Tbl", StringComparison.OrdinalIgnoreCase))
            {
                var baseName = mapping.BeforeTable.Substring(0, mapping.BeforeTable.Length - 3);
                var pkColumn = baseName + "ID";
                
                // Check if we have a column mapping for this PK
                var pkMapping = mapping.ColumnMappings?.FirstOrDefault(cm => 
                    string.Equals(cm.BeforeColumn, pkColumn, StringComparison.OrdinalIgnoreCase));
                
                var finalPkName = pkMapping?.AfterColumn ?? pkColumn;
                
                constraintTable.PrimaryKey.Add(finalPkName);
                constraintTable.IdentityColumns.Add(finalPkName);
            }

            constraints.Add(constraintTable);
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

                // Look for table definition lines: "Table,Before,After,Action"
                if (line.StartsWith("Table,Before,After,Action"))
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
                                    columnLine.StartsWith("Table,"))
                                {
                                    break;
                                }
                                
                                // Parse column mapping
                                var columnParts = ParseCsvLine(columnLine);
                                if (columnParts.Length >= 13) // We need at least 13 columns to get the Action and Source
                                {
                                    var beforeCol = columnParts[1]?.Trim(); // Before Col name
                                    var afterCol = columnParts[6]?.Trim();  // After Col Name
                                    var colAction = columnParts[12]?.Trim(); // Action
                                    
                                    if (!string.IsNullOrEmpty(beforeCol))
                                    {
                                        // Handle dropped columns (empty after column)
                                        if (string.IsNullOrEmpty(afterCol))
                                        {
                                            colAction = "Drop";
                                            afterCol = null;
                                        }
                                        
                                        // Default column action
                                        if (string.IsNullOrEmpty(colAction))
                                        {
                                            colAction = string.Equals(beforeCol, afterCol, StringComparison.OrdinalIgnoreCase) ? "Copy" : "Rename";
                                        }
                                        
                                        var columnMapping = new ColumnMapping
                                        {
                                            BeforeColumn = beforeCol,
                                            AfterColumn = afterCol,
                                            Action = colAction
                                        };
                                        
                                        mapping.ColumnMappings.Add(columnMapping);
                                        columnCount++;
                                        
                                        Console.WriteLine($"      ?? Column: {beforeCol} -> {afterCol ?? "DROP"} ({colAction})");
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
            var totalColumns = mappings.Sum(m => m.ColumnMappings?.Count ?? 0);
            
            Console.WriteLine($"?? Summary: {ignoredCount} ignored, {renamedCount} renamed, {copiedCount} copied, {totalColumns} column mappings");
            
            return mappings;
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
    }

    public class ColumnMapping
    {
        public string BeforeColumn { get; set; }
        public string AfterColumn { get; set; }
        public string Action { get; set; }
    }
}