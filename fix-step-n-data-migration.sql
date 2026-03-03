-- Quick Fix for Step N Data Migration Issues
-- Run this script to diagnose and fix common data migration problems

PRINT '=== STEP N DATA MIGRATION QUICK FIX ===';
PRINT 'Timestamp: ' + CAST(GETDATE() AS NVARCHAR(50));
PRINT '';

-- 1. Check if AccessSrc schema exists
PRINT '1. Checking AccessSrc schema...';
IF SCHEMA_ID('AccessSrc') IS NULL
BEGIN
    PRINT '   ERROR: AccessSrc schema does not exist!';
    PRINT '   SOLUTION: Run option MS (Stage Access) first';
    PRINT '   This creates AccessSrc schema and populates it with Access data';
END
ELSE
BEGIN
    PRINT '   ? AccessSrc schema exists';
    
    -- Count tables in AccessSrc
    DECLARE @AccessSrcTableCount INT;
    SELECT @AccessSrcTableCount = COUNT(*)
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'AccessSrc';
    
    PRINT '   AccessSrc tables found: ' + CAST(@AccessSrcTableCount AS NVARCHAR(10));
    
    IF @AccessSrcTableCount = 0
    BEGIN
        PRINT '   WARNING: AccessSrc schema exists but has no tables';
        PRINT '   SOLUTION: Run option MS (Stage Access) to populate AccessSrc';
    END
    ELSE
    BEGIN
        -- Check row counts in key AccessSrc tables
        PRINT '   Sample AccessSrc table row counts:';
        
        -- Check CustomersTbl (should be ContactsTbl in target)
        IF OBJECT_ID('AccessSrc.CustomersTbl', 'U') IS NOT NULL
        BEGIN
            DECLARE @CustomerCount INT;
            EXEC sp_executesql N'SELECT @count = COUNT(*) FROM AccessSrc.CustomersTbl', N'@count INT OUTPUT', @count = @CustomerCount OUTPUT;
            PRINT '     AccessSrc.CustomersTbl: ' + CAST(@CustomerCount AS NVARCHAR(10)) + ' rows';
            
            IF @CustomerCount = 0
                PRINT '     WARNING: CustomersTbl is empty - check Access staging';
        END
        ELSE
            PRINT '     AccessSrc.CustomersTbl: NOT FOUND';
            
        -- Check OrdersTbl
        IF OBJECT_ID('AccessSrc.OrdersTbl', 'U') IS NOT NULL
        BEGIN
            DECLARE @OrderCount INT;
            EXEC sp_executesql N'SELECT @count = COUNT(*) FROM AccessSrc.OrdersTbl', N'@count INT OUTPUT', @count = @OrderCount OUTPUT;
            PRINT '     AccessSrc.OrdersTbl: ' + CAST(@OrderCount AS NVARCHAR(10)) + ' rows';
        END
        ELSE
            PRINT '     AccessSrc.OrdersTbl: NOT FOUND';
    END
END
PRINT '';

-- 2. Check target table status
PRINT '2. Checking target table status...';

-- Key target tables to check
CREATE TABLE #TargetTables (TableName NVARCHAR(128));
INSERT INTO #TargetTables VALUES ('ContactsTbl'), ('AreasTbl'), ('ItemsTbl'), ('PeopleTbl'), ('PaymentTermsTbl'), ('EquipTypesTbl');

DECLARE @TableName NVARCHAR(128);
DECLARE table_cursor CURSOR FOR SELECT TableName FROM #TargetTables;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @RowCount INT;
    
    SET @SQL = N'SELECT @count = COUNT(*) FROM [' + @TableName + ']';
    
    IF OBJECT_ID(@TableName, 'U') IS NOT NULL
    BEGIN
        EXEC sp_executesql @SQL, N'@count INT OUTPUT', @count = @RowCount OUTPUT;
        
        IF @RowCount = 0
            PRINT '   ??  ' + @TableName + ': EXISTS but EMPTY (this is the problem!)';
        ELSE
            PRINT '   ? ' + @TableName + ': ' + CAST(@RowCount AS NVARCHAR(10)) + ' rows';
    END
    ELSE
        PRINT '   ? ' + @TableName + ': TABLE DOES NOT EXIST';
        
    FETCH NEXT FROM table_cursor INTO @TableName;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;
DROP TABLE #TargetTables;
PRINT '';

-- 3. Check for foreign key constraints (might be blocking INSERTs)
PRINT '3. Checking foreign key constraints...';
DECLARE @FKCount INT;
SELECT @FKCount = COUNT(*)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id = fk.parent_object_id
WHERE t.name IN ('ContactsTbl', 'AreasTbl', 'ItemsTbl', 'PeopleTbl');

PRINT '   Active FK constraints on key tables: ' + CAST(@FKCount AS NVARCHAR(10));

IF @FKCount > 0
BEGIN
    PRINT '   Foreign keys that might block migration:';
    SELECT 
        'FK: ' + t.name + '.' + c.name + ' -> ' + rt.name + '.' + rc.name as ConstraintInfo
    FROM sys.foreign_keys fk
    JOIN sys.tables t ON t.object_id = fk.parent_object_id
    JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
    JOIN sys.columns rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
    WHERE t.name IN ('ContactsTbl', 'AreasTbl', 'ItemsTbl', 'PeopleTbl');
END
ELSE
BEGIN
    PRINT '   ? No foreign key constraints detected';
END
PRINT '';

-- 4. Provide specific recommendations
PRINT '4. RECOMMENDED ACTIONS:';
PRINT '';

IF SCHEMA_ID('AccessSrc') IS NULL
BEGIN
    PRINT '?? CRITICAL: Run step MS first!';
    PRINT '   cd Migrations\MigrationRunner';
    PRINT '   dotnet run --configuration Release';
    PRINT '   Select "MS" - Stage Access to SQL';
    PRINT '   This creates and populates AccessSrc.* tables from Access DB';
END
ELSE
BEGIN
    SELECT @AccessSrcTableCount = COUNT(*)
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE s.name = 'AccessSrc';
    
    IF @AccessSrcTableCount = 0
    BEGIN
        PRINT '?? ISSUE: AccessSrc schema empty';
        PRINT '   Run step MS (Stage Access) to populate source tables';
    END
    ELSE
    BEGIN
        PRINT '?? LIKELY ISSUE: Data migration script execution failed';
        PRINT '   Try these solutions in order:';
        PRINT '';
        PRINT '   A) Re-run step N (Apply Data Migration):';
        PRINT '      dotnet run --configuration Release';
        PRINT '      Select "N" - Apply DATA migration script';
        PRINT '';
        PRINT '   B) Check migration logs:';
        PRINT '      Look in Data\Metadata\PlanEdits\Logs\ApplyData_*.log';
        PRINT '      Check for INSERT statement errors or foreign key violations';
        PRINT '';
        PRINT '   C) Run full pipeline (nuclear option):';
        PRINT '      dotnet run --configuration Release';
        PRINT '      Select "Z" - Full pipeline: A?B?C?MS?M?N?!';
        PRINT '';
        PRINT '   D) If Orders are failing (ContactID FK issues):';
        PRINT '      Make sure ContactsTbl is populated before running CustomNormalizeRunner';
        PRINT '      Run steps in correct order: MS ? M ? N ? !';
    END
END

PRINT '';
PRINT '=== QUICK FIX COMPLETE ===';
PRINT 'Check the results above and follow the recommended actions.';
PRINT 'The most common issue is running step N without running step MS first.';
PRINT '';