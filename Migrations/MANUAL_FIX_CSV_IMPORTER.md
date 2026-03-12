# Manual Fix Instructions for CSV Importer Primary Key Detection
# ================================================================

## Problem Summary
The CSV importer (option 11) is incorrectly detecting ContactID as a PRIMARY KEY when it should be a FOREIGN KEY.
This happens because:
1. The CSV correctly marks ContactID as "FK (ContactsTbl)" in the "After Key" column
2. But the importer doesn't read or respect this - it only checks for "PK"
3. The fallback heuristic finds ContactID (ends with "ID") and incorrectly marks it as PK

## Affected Tables
- ContactUsageLinesTbl (PK should be: ContactUsageLineNo, NOT ContactID)
- ContactsItemsPredictedTbl (PK should be: ContactsItemsPredictedID, NOT ContactID)  
- ContactsItemUsageTbl (PK should be: ContactItemUsageLineNo, NOT ContactID)

## Required Code Changes to PlanHumanReviewImporter.cs

### Change 1: Add FK tracking to ColumnMapping class (around line 1151)
FIND:
    public class ColumnMapping
    {
        public string BeforeColumn { get; set; }
        public string AfterColumn { get; set; }
        public string Action { get; set; }
        public string NormalizationTarget { get; set; } // "Header" or "Lines"
    }

REPLACE WITH:
    public class ColumnMapping
    {
        public string BeforeColumn { get; set; }
        public string AfterColumn { get; set; }
        public string Action { get; set; }
        public string NormalizationTarget { get; set; } // "Header" or "Lines"
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsForeignKey { get; set; }
    }

### Change 2: Read After Key and After Auto in CSV parser (around line 939)
FIND:
                                    var beforeCol = columnParts[1]?.Trim(); // Before Col name
                                    var afterCol = columnParts[6]?.Trim();  // After Col Name
                                    var colAction = columnParts[12]?.Trim(); // Action

REPLACE WITH:
                                    var beforeCol = columnParts[1]?.Trim(); // Before Col name
                                    var afterCol = columnParts[6]?.Trim();  // After Col Name
                                    var afterKey = columnParts[8]?.Trim();  // After Key (PK, FK, No)
                                    var afterAuto = columnParts[9]?.Trim(); // After Auto (Yes/No)
                                    var colAction = columnParts[12]?.Trim(); // Action

### Change 3: Detect PK/Identity/FK from CSV and add to ColumnMapping (around line 975)
FIND:
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

REPLACE WITH:
                                        // Default column action
                                        if (string.IsNullOrEmpty(colAction))
                                        {
                                            colAction = string.Equals(beforeCol, afterCol, StringComparison.OrdinalIgnoreCase) ? "Copy" : "Rename";
                                        }
                                        
                                        // Detect primary key, identity, and foreign key from CSV
                                        var isPrimaryKey = string.Equals(afterKey, "PK", StringComparison.OrdinalIgnoreCase);
                                        var isIdentity = string.Equals(afterAuto, "Yes", StringComparison.OrdinalIgnoreCase);
                                        var isForeignKey = (afterKey ?? "").StartsWith("FK", StringComparison.OrdinalIgnoreCase);
                                        
                                        var columnMapping = new ColumnMapping
                                        {
                                            BeforeColumn = beforeCol,
                                            AfterColumn = afterCol,
                                            Action = colAction,
                                            IsPrimaryKey = isPrimaryKey,
                                            IsIdentity = isIdentity,
                                            IsForeignKey = isForeignKey
                                        };

### Change 4: Update console output (around line 988)
FIND:
                                        Console.WriteLine($"      ? Column: {beforeCol} -> {afterCol ?? "DROP"} ({colAction})");

REPLACE WITH:
                                        var pkMarker = isPrimaryKey ? " [PK]" : "";
                                        var identityMarker = isIdentity ? " [IDENTITY]" : "";
                                        var fkMarker = isForeignKey ? " [FK]" : "";
                                        Console.WriteLine($"      ? Column: {beforeCol} -> {afterCol ?? "DROP"} ({colAction}){pkMarker}{identityMarker}{fkMarker}");

### Change 5: Add NEW column handling after the first if block (around line 990)
ADD AFTER THE CLOSING } of the "if (!string.IsNullOrEmpty(beforeCol))" block:
                                    // Handle NEW columns (no before column, but has after column)
                                    else if (!string.IsNullOrEmpty(afterCol))
                                    {
                                        if (string.IsNullOrEmpty(colAction))
                                            colAction = "New";
                                        
                                        // Detect primary key, identity, and foreign key from CSV
                                        var isPrimaryKey = string.Equals(afterKey, "PK", StringComparison.OrdinalIgnoreCase);
                                        var isIdentity = string.Equals(afterAuto, "Yes", StringComparison.OrdinalIgnoreCase);
                                        var isForeignKey = (afterKey ?? "").StartsWith("FK", StringComparison.OrdinalIgnoreCase);
                                        
                                        var columnMapping = new ColumnMapping
                                        {
                                            BeforeColumn = afterCol,  // Use target name as source for NEW columns
                                            AfterColumn = afterCol,
                                            Action = colAction,
                                            IsPrimaryKey = isPrimaryKey,
                                            IsIdentity = isIdentity,
                                            IsForeignKey = isForeignKey
                                        };
                                        
                                        mapping.ColumnMappings.Add(columnMapping);
                                        columnCount++;
                                        
                                        var pkMarker = isPrimaryKey ? " [PK]" : "";
                                        var identityMarker = isIdentity ? " [IDENTITY]" : "";
                                        var fkMarker = isForeignKey ? " [FK]" : "";
                                        Console.WriteLine($"      ? NEW Column: {afterCol} ({colAction}){pkMarker}{identityMarker}{fkMarker}");
                                    }

### Change 6: Update CreateConstraintsForTable to check CSV PK FIRST and exclude FKs (around line 684)
FIND:
            // Look for ID columns in the column mappings to find the actual primary key
            if (mapping.ColumnMappings?.Any() == true)
            {
                // Find columns that are likely primary keys (end with "ID")
                var idColumns = mapping.ColumnMappings
                    .Where(cm => !string.IsNullOrEmpty(cm.BeforeColumn) && 
                                !string.IsNullOrEmpty(cm.AfterColumn) &&
                                cm.BeforeColumn.EndsWith("ID", StringComparison.OrdinalIgnoreCase))
                    .ToList();

REPLACE WITH:
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
                    
                    Console.WriteLine($"    ? Creating constraints for {targetTable}: PK={pkCol.AfterColumn} (from CSV After Key=PK, Identity={pkCol.IsIdentity})");
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

### Change 7: Update the heuristic fallback console message (around line 707)
FIND:
                    Console.WriteLine($"    ? Creating constraints for {targetTable}: PK={finalPkName} (mapped from {pkColumn.BeforeColumn})");

REPLACE WITH:
                    Console.WriteLine($"    ? Creating constraints for {targetTable}: PK={finalPkName} (heuristic fallback from {pkColumn.BeforeColumn})");

## After Making Changes
1. Build the solution to verify compilation
2. Run MigrationRunner option 11 to re-import the CSV
3. Verify PlanConstraints.json now has correct PKs without ContactID duplicates
4. Regenerate SQL scripts (options M and N)

## Expected Result
After these changes, the CSV importer will:
- Read "After Key" column to detect PK, FK, and Identity columns
- Prioritize CSV data over naming heuristics
- Exclude FK columns from PK detection
- Handle NEW columns (like ContactsItemsPredictedID)
- Generate correct PlanConstraints.json without ContactID as PK
