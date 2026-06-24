-- ============================================================================
-- MIGRATION VERIFICATION REPORT
-- ============================================================================
-- This script checks the success of data migration from AccessSrc to target tables

SET NOCOUNT ON;

DECLARE @TotalSourceRows INT = 0;
DECLARE @TotalTargetRows INT = 0;
DECLARE @SuccessCount INT = 0;
DECLARE @FailCount INT = 0;
DECLARE @PartialCount INT = 0;

PRINT '============================================================================';
PRINT 'MIGRATION VERIFICATION REPORT';
PRINT '============================================================================';
PRINT '';

-- Create a temp table to store results
CREATE TABLE #MigrationResults (
    SourceTable NVARCHAR(128),
    TargetTable NVARCHAR(128),
    SourceRows INT,
    TargetRows INT,
    Status NVARCHAR(50)
);

-- Get all source tables and their row counts
INSERT INTO #MigrationResults (SourceTable, TargetTable, SourceRows, TargetRows, Status)
SELECT 
    src.name AS SourceTable,
    tgt.name AS TargetTable,
    ISNULL(SUM(psrc.rows), 0) AS SourceRows,
    ISNULL(SUM(ptgt.rows), 0) AS TargetRows,
    CASE 
        WHEN SUM(ptgt.rows) = 0 AND SUM(psrc.rows) = 0 THEN 'EMPTY'
        WHEN SUM(ptgt.rows) = 0 AND SUM(psrc.rows) > 0 THEN 'FAILED'
        WHEN SUM(ptgt.rows) = SUM(psrc.rows) THEN 'SUCCESS'
        WHEN SUM(ptgt.rows) > 0 THEN 'PARTIAL'
        ELSE 'UNKNOWN'
    END AS Status
FROM sys.tables src
JOIN sys.schemas ssrc ON src.schema_id = ssrc.schema_id
JOIN sys.partitions psrc ON src.object_id = psrc.object_id
LEFT JOIN sys.tables tgt ON tgt.name = src.name AND tgt.schema_id = SCHEMA_ID('dbo')
LEFT JOIN sys.partitions ptgt ON tgt.object_id = ptgt.object_id AND ptgt.index_id IN (0,1)
WHERE ssrc.name = 'AccessSrc' 
  AND psrc.index_id IN (0,1)
GROUP BY src.name, tgt.name;

-- Display results
PRINT 'Source Table'.PADRIGHT(40, ' ') + 'Target Table'.PADRIGHT(40, ' ') + 'Source Rows'.PADRIGHT(15, ' ') + 'Target Rows'.PADRIGHT(15, ' ') + 'Status';
PRINT REPLICATE('-', 110);

SELECT 
    SourceTable = ISNULL(SourceTable, 'N/A').PADRIGHT(40, ' '),
    TargetTable = ISNULL(TargetTable, 'N/A').PADRIGHT(40, ' '),
    SourceRows = CONVERT(NVARCHAR(10), SourceRows).PADRIGHT(15, ' '),
    TargetRows = CONVERT(NVARCHAR(10), TargetRows).PADRIGHT(15, ' '),
    [Status]
FROM #MigrationResults
ORDER BY SourceRows DESC, TargetTable;

PRINT REPLICATE('-', 110);
PRINT '';

-- Summary statistics
SELECT 
    @TotalSourceRows = SUM(SourceRows),
    @TotalTargetRows = SUM(TargetRows),
    @SuccessCount = SUM(CASE WHEN Status = 'SUCCESS' THEN 1 ELSE 0 END),
    @FailCount = SUM(CASE WHEN Status = 'FAILED' THEN 1 ELSE 0 END),
    @PartialCount = SUM(CASE WHEN Status = 'PARTIAL' THEN 1 ELSE 0 END)
FROM #MigrationResults;

PRINT 'SUMMARY:';
PRINT '--------';
PRINT 'Total source rows: ' + CONVERT(NVARCHAR(20), @TotalSourceRows);
PRINT 'Total target rows: ' + CONVERT(NVARCHAR(20), @TotalTargetRows);
PRINT 'Tables migrated successfully (100%): ' + CONVERT(NVARCHAR(10), @SuccessCount);
PRINT 'Tables with partial data: ' + CONVERT(NVARCHAR(10), @PartialCount);
PRINT 'Tables failed (0 rows): ' + CONVERT(NVARCHAR(10), @FailCount);
PRINT '';

IF @FailCount > 0
BEGIN
    PRINT 'FAILED TABLES (need investigation):';
    PRINT '-----------------------------------';
    SELECT 
        SourceTable,
        TargetTable,
        SourceRows,
        TargetRows
    FROM #MigrationResults
    WHERE Status = 'FAILED'
    ORDER BY SourceRows DESC;
    PRINT '';
END

IF @PartialCount > 0
BEGIN
    PRINT 'PARTIAL TABLES (data loss detected):';
    PRINT '------------------------------------';
    SELECT 
        SourceTable,
        TargetTable,
        SourceRows,
        TargetRows,
        Difference = SourceRows - TargetRows,
        PercentMigrated = CONVERT(DECIMAL(5,1), (CONVERT(DECIMAL(10,2), TargetRows) / CONVERT(DECIMAL(10,2), SourceRows)) * 100)
    FROM #MigrationResults
    WHERE Status = 'PARTIAL'
    ORDER BY SourceRows DESC;
    PRINT '';
END

-- Detailed view for empty source tables
DECLARE @EmptySourceCount INT = (SELECT COUNT(*) FROM #MigrationResults WHERE SourceRows = 0);
IF @EmptySourceCount > 0
BEGIN
    PRINT 'TABLES WITH NO SOURCE DATA (expected empty):';
    PRINT '-------------------------------------------';
    SELECT TargetTable, SourceRows, TargetRows
    FROM #MigrationResults
    WHERE SourceRows = 0
    ORDER BY TargetTable;
    PRINT '';
END

DROP TABLE #MigrationResults;

PRINT '============================================================================';
PRINT 'END OF REPORT';
PRINT '============================================================================';
