-- ========================================
-- DROP ALL TABLES IN DATABASE SCRIPT
-- WARNING: This will delete ALL DATA!
-- ========================================

-- Change this to your target database name
USE [TrackerData] -- or [OtterDb] - update as needed
GO

PRINT 'WARNING: This script will DROP ALL TABLES and DELETE ALL DATA!'
PRINT 'Make sure you have a backup before running this script.'
PRINT ''
PRINT 'Starting to drop all tables...'

-- Disable all foreign key constraints first
DECLARE @sql NVARCHAR(MAX) = N''

-- Generate commands to disable all foreign key constraints
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + 
              N' NOCHECK CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id

IF LEN(@sql) > 0
BEGIN
    PRINT 'Disabling foreign key constraints...'
    EXEC sp_executesql @sql
    PRINT 'Foreign key constraints disabled.'
END

-- Generate commands to drop all user tables
SET @sql = N''
SELECT @sql = @sql + N'DROP TABLE ' + QUOTENAME(SCHEMA_NAME(schema_id)) + N'.' + QUOTENAME(name) + N';' + CHAR(13)
FROM sys.tables 
WHERE type = 'U'  -- User tables only
  AND name NOT LIKE 'aspnet_%'  -- Preserve ASP.NET membership tables
  AND name NOT LIKE 'webpages_%'  -- Preserve WebPages tables
  AND name NOT LIKE '__MigrationHistory'  -- Preserve EF migration history
  -- Add exclusions for ignored tables if you want to keep them
  -- AND name NOT IN ('ArchivedCustomersTbl', 'LogTbl', 'ClientUsageHistoryTbl')
ORDER BY name

IF LEN(@sql) > 0
BEGIN
    PRINT 'Dropping all user tables...'
    PRINT @sql  -- Show what will be executed
    EXEC sp_executesql @sql
    PRINT 'All user tables dropped successfully!'
END
ELSE
BEGIN
    PRINT 'No user tables found to drop.'
END

PRINT ''
PRINT 'Database cleanup completed!'
PRINT ''
PRINT 'Next steps:'
PRINT '1. Run MigrationRunner Option A (Generate CREATE TABLE DDL) with DROP=Yes'
PRINT '2. Run MigrationRunner Option M (Generate Data Migration Script)'
PRINT '3. Apply the generated scripts to recreate your schema 