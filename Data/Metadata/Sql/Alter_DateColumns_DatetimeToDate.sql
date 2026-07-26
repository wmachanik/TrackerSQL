-- Convert business-date columns from DATETIME to DATE on an existing TrackerSQL database.
-- Run once against OtterDb (or your target database) after backup.
--
-- Policy: every migrated column that represents a calendar day (orders, prep, delivery,
-- closures, reminders, repairs, usage, etc.) should be DATE, not DATETIME.
-- Access stored these as Date/Time fields without meaningful time-of-day semantics.
--
-- C# code can continue using DateTime / DateTime?; ADO.NET and SqlClient map DATE cleanly.
-- After this script, CAST(... AS DATE) in queries is optional but harmless.

SET NOCOUNT ON;
GO

DECLARE @changes TABLE (
    TableName sysname NOT NULL,
    ColumnName sysname NOT NULL,
  PRIMARY KEY (TableName, ColumnName)
);

INSERT INTO @changes (TableName, ColumnName) VALUES
    (N'ClosureDatesTbl', N'DateClosed'),
    (N'ClosureDatesTbl', N'DateReopen'),
    (N'ClosureDatesTbl', N'NextPreperationDate'),
    (N'ContactsAwayPeriodTbl', N'AwayStartDate'),
    (N'ContactsAwayPeriodTbl', N'AwayEndDate'),
    (N'ContactsItemsPredictedTbl', N'NextCoffeeBy'),
    (N'ContactsItemsPredictedTbl', N'NextCleanOn'),
    (N'ContactsItemsPredictedTbl', N'NextFilterEst'),
    (N'ContactsItemsPredictedTbl', N'NextDescaleEst'),
    (N'ContactsItemsPredictedTbl', N'NextServiceEst'),
    (N'ContactsItemSvcSummaryTbl', N'UsageDate'),
    (N'ContactsItemUsageTbl', N'DeliveryDate'),
    (N'ContactsTbl', N'LastDateSentReminder'),
    (N'HolidayClosuresTbl', N'ClosureDate'),
    (N'NextPreperationDateByAreasTbl', N'PreperationDate'),
    (N'NextPreperationDateByAreasTbl', N'DeliveryDate'),
    (N'NextPreperationDateByAreasTbl', N'NextPreperationDate'),
    (N'NextPreperationDateByAreasTbl', N'NextDeliveryDate'),
    (N'OrderList', N'Time'),
    (N'OrdersTbl', N'OrderDate'),
    (N'OrdersTbl', N'PrepDate'),
    (N'OrdersTbl', N'RequiredByDate'),
    (N'PredictedOrdersTbl', N'PrepDate'),
    (N'PredictedOrdersTbl', N'DeliveryDate'),
    (N'RecurringOrderItemsTbl', N'DateLastDone'),
    (N'RecurringOrderItemsTbl', N'NextDateRequired'),
    (N'RecurringOrderItemsTbl', N'RequireUntilDate'),
    (N'RepairsTbl', N'DateLogged'),
    (N'RepairsTbl', N'LastStatusChange'),
    (N'SendCheckupEmailTextsTbl', N'DateLastChange'),
    (N'SentRemindersLogTbl', N'DateSentReminder'),
    (N'SentRemindersLogTbl', N'NextPreperationDate'),
    (N'SysDataTbl', N'LastReoccurringDate'),
    (N'SysDataTbl', N'DateLastPrepDateCalcd'),
    (N'SysDataTbl', N'MinReminderDate'),
    (N'TempCoffeecheckupCustomerTbl', N'NextPreperationDate'),
    (N'TempCoffeecheckupCustomerTbl', N'NextDeliveryDate'),
    (N'TempCoffeecheckupCustomerTbl', N'NextCoffee'),
    (N'TempCoffeecheckupCustomerTbl', N'NextClean'),
    (N'TempCoffeecheckupCustomerTbl', N'NextFilter'),
    (N'TempCoffeecheckupCustomerTbl', N'NextDescal'),
    (N'TempCoffeecheckupCustomerTbl', N'NextService'),
    (N'TempCoffeecheckupItemsTbl', N'NextDateRequired'),
    (N'TempOrdersHeaderTbl', N'OrderDate'),
    (N'TempOrdersHeaderTbl', N'RoastDate'),
    (N'TempOrdersHeaderTbl', N'RequiredByDate'),
    (N'TempOrdersTbl', N'OrderDate'),
    (N'TempOrdersTbl', N'RoastDate'),
    (N'TempOrdersTbl', N'RequiredByDate'),
    (N'tmpOrdersReplyTbl', N'NextCoffeeBy'),
    (N'TotalCountTrackerTbl', N'CountDate'),
    (N'UsedItemGroupsTbl', N'LastItemDateChanged'),
    (N'VisitLogTbl', N'VisitDate');

DECLARE @table sysname;
DECLARE @column sysname;
DECLARE @sql nvarchar(max);

DECLARE change_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT c.TableName, c.ColumnName
    FROM @changes c
    INNER JOIN sys.tables t ON t.name = c.TableName AND t.schema_id = SCHEMA_ID(N'dbo')
    INNER JOIN sys.columns col
        ON col.object_id = t.object_id
       AND col.name = c.ColumnName
    INNER JOIN sys.types ty ON col.user_type_id = ty.user_type_id
    WHERE ty.name IN (N'datetime', N'datetime2', N'smalldatetime')
    ORDER BY c.TableName, c.ColumnName;

OPEN change_cursor;
FETCH NEXT FROM change_cursor INTO @table, @column;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'
        UPDATE ' + QUOTENAME(@table) + N'
        SET ' + QUOTENAME(@column) + N' = CAST(' + QUOTENAME(@column) + N' AS DATE)
        WHERE ' + QUOTENAME(@column) + N' IS NOT NULL
          AND CAST(' + QUOTENAME(@column) + N' AS TIME) <> ''00:00:00'';

    EXEC sp_executesql @sql;

    SET @sql = N'ALTER TABLE ' + QUOTENAME(@table) + N' ALTER COLUMN ' + QUOTENAME(@column) + N' DATE NULL;';
    PRINT @sql;
    EXEC sp_executesql @sql;

    FETCH NEXT FROM change_cursor INTO @table, @column;
END

CLOSE change_cursor;
DEALLOCATE change_cursor;

PRINT 'Date column conversion complete.';
GO

-- Verification: list any remaining datetime columns on business tables (should be none).
SELECT
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS SqlType
FROM sys.columns c
INNER JOIN sys.tables t ON c.object_id = t.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.schema_id = SCHEMA_ID(N'dbo')
  AND ty.name IN (N'datetime', N'datetime2', N'smalldatetime')
  AND t.name NOT IN (N'LogTbl')  -- legacy log table ignored by migration
ORDER BY t.name, c.name;
GO
