-- Auto-generated DATA MIGRATION script
-- Assumes source data is available under schema [AccessSrc] using Access source table names.
-- If you do not have [AccessSrc] objects, the generator will fall back to unqualified [Source] when Target != Source.
-- If Target == Source and AccessSrc.Source is missing, the table is skipped (no self-select).
-- Create schema once if needed: IF SCHEMA_ID('AccessSrc') IS NULL EXEC('CREATE SCHEMA AccessSrc');
-- AccessSchema: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema
-- PlanConstraints: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\PlanConstraints.json
-- Tables to migrate (ordered): 43
--   - AreaPrepDaysTbl
--   - AreasTbl
--   - AwayReasonTbl
--   - ClosureDatesTbl
--   - ContactsAccInfoTbl
--   - ContactsAwayPeriodTbl
--   - ContactsItemUsageTbl
--   - ContactsTbl
--   - ContactsUsageTbl
--   - ContactTrackedServiceItemsTbl
--   - ContactTypesTbl
--   - ContactUsageLinesTbl
--   - EquipConditionsTbl
--   - EquipTypesTbl
--   - HolidayClosuresTbl
--   - InvoiceTypesTbl
--   - ItemGroupsTbl
--   - ItemPackagingsTbl
--   - ItemPrepTypesTbl
--   - ItemServiceTypesTbl
--   - ItemsTbl
--   - ItemUnitsTbl
--   - NextPrepDateByAreasTbl
--   - PaymentTermsTbl
--   - PeopleTbl
--   - PriceLevelsTbl
--   - RecurranceTypesTbl
--   - RepairFaultsTbl
--   - RepairStatusesTbl
--   - RepairsTbl
--   - SectionTypesTbl
--   - SendCheckupEmailTextsTbl
--   - SentRemindersLogTbl
--   - SysDataTbl
--   - TempCoffeecheckupCustomerTbl
--   - TempCoffeecheckupItemsTbl
--   - TempOrdersHeaderTbl
--   - TempOrdersLinesTbl
--   - TempOrdersTbl
--   - TotalCountTrackerTbl
--   - TrackedServiceItemsTbl
--   - TransactionTypesTbl
--   - UsedItemGroupsTbl
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Disable foreign keys on migrated targets
DECLARE @tbl sysname, @fk sysname, @sql nvarchar(max);
DECLARE fk_cur CURSOR LOCAL FAST_FORWARD FOR
SELECT QUOTENAME(SCHEMA_NAME(t.schema_id))+'.'+QUOTENAME(t.name), QUOTENAME(fk.name)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id=fk.parent_object_id
WHERE t.name IN (N'AreaPrepDaysTbl', N'AreasTbl', N'AwayReasonTbl', N'ClosureDatesTbl', N'ContactsAccInfoTbl', N'ContactsAwayPeriodTbl', N'ContactsItemUsageTbl', N'ContactsTbl', N'ContactsUsageTbl', N'ContactTrackedServiceItemsTbl', N'ContactTypesTbl', N'ContactUsageLinesTbl', N'EquipConditionsTbl', N'EquipTypesTbl', N'HolidayClosuresTbl', N'InvoiceTypesTbl', N'ItemGroupsTbl', N'ItemPackagingsTbl', N'ItemPrepTypesTbl', N'ItemServiceTypesTbl', N'ItemsTbl', N'ItemUnitsTbl', N'NextPrepDateByAreasTbl', N'PaymentTermsTbl', N'PeopleTbl', N'PriceLevelsTbl', N'RecurranceTypesTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'RepairsTbl', N'SectionTypesTbl', N'SendCheckupEmailTextsTbl', N'SentRemindersLogTbl', N'SysDataTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersLinesTbl', N'TempOrdersTbl', N'TotalCountTrackerTbl', N'TrackedServiceItemsTbl', N'TransactionTypesTbl', N'UsedItemGroupsTbl');
OPEN fk_cur;
FETCH NEXT FROM fk_cur INTO @tbl, @fk;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'ALTER TABLE ' + @tbl + N' NOCHECK CONSTRAINT ' + @fk + N';';
    PRINT @sql; EXEC sp_executesql @sql;
    FETCH NEXT FROM fk_cur INTO @tbl, @fk;
END
CLOSE fk_cur; DEALLOCATE fk_cur;
GO

-- Purge target tables before load (child-to-parent)
PRINT N'Purging [UsedItemGroupsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'UsedItemGroupsTbl')
        TRUNCATE TABLE [UsedItemGroupsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [UsedItemGroupsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [UsedItemGroupsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [UsedItemGroupsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [UsedItemGroupsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [TransactionTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TransactionTypesTbl')
        TRUNCATE TABLE [TransactionTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TransactionTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TransactionTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TransactionTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TransactionTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [TrackedServiceItemsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TrackedServiceItemsTbl')
        TRUNCATE TABLE [TrackedServiceItemsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TrackedServiceItemsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TrackedServiceItemsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TrackedServiceItemsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TrackedServiceItemsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [TotalCountTrackerTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TotalCountTrackerTbl')
        TRUNCATE TABLE [TotalCountTrackerTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TotalCountTrackerTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TotalCountTrackerTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TotalCountTrackerTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TotalCountTrackerTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [TempOrdersTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TempOrdersTbl')
        TRUNCATE TABLE [TempOrdersTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TempOrdersTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TempOrdersTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TempOrdersTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TempOrdersTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [TempOrdersLinesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TempOrdersLinesTbl')
        TRUNCATE TABLE [TempOrdersLinesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TempOrdersLinesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TempOrdersLinesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TempOrdersLinesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TempOrdersLinesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [TempOrdersHeaderTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TempOrdersHeaderTbl')
        TRUNCATE TABLE [TempOrdersHeaderTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TempOrdersHeaderTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TempOrdersHeaderTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TempOrdersHeaderTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TempOrdersHeaderTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [TempCoffeecheckupItemsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TempCoffeecheckupItemsTbl')
        TRUNCATE TABLE [TempCoffeecheckupItemsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TempCoffeecheckupItemsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TempCoffeecheckupItemsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TempCoffeecheckupItemsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TempCoffeecheckupItemsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [TempCoffeecheckupCustomerTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TempCoffeecheckupCustomerTbl')
        TRUNCATE TABLE [TempCoffeecheckupCustomerTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TempCoffeecheckupCustomerTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TempCoffeecheckupCustomerTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TempCoffeecheckupCustomerTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TempCoffeecheckupCustomerTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [SysDataTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'SysDataTbl')
        TRUNCATE TABLE [SysDataTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [SysDataTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [SysDataTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [SysDataTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [SysDataTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [SentRemindersLogTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'SentRemindersLogTbl')
        TRUNCATE TABLE [SentRemindersLogTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [SentRemindersLogTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [SentRemindersLogTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [SentRemindersLogTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [SentRemindersLogTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [SendCheckupEmailTextsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'SendCheckupEmailTextsTbl')
        TRUNCATE TABLE [SendCheckupEmailTextsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [SendCheckupEmailTextsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [SendCheckupEmailTextsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [SendCheckupEmailTextsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [SendCheckupEmailTextsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [SectionTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'SectionTypesTbl')
        TRUNCATE TABLE [SectionTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [SectionTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [SectionTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [SectionTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [SectionTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [RepairsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'RepairsTbl')
        TRUNCATE TABLE [RepairsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [RepairsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [RepairsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [RepairsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [RepairsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [RepairStatusesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'RepairStatusesTbl')
        TRUNCATE TABLE [RepairStatusesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [RepairStatusesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [RepairStatusesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [RepairStatusesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [RepairStatusesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [RepairFaultsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'RepairFaultsTbl')
        TRUNCATE TABLE [RepairFaultsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [RepairFaultsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [RepairFaultsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [RepairFaultsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [RepairFaultsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [RecurranceTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'RecurranceTypesTbl')
        TRUNCATE TABLE [RecurranceTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [RecurranceTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [RecurranceTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [RecurranceTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [RecurranceTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [PriceLevelsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'PriceLevelsTbl')
        TRUNCATE TABLE [PriceLevelsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [PriceLevelsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [PriceLevelsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [PriceLevelsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [PriceLevelsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [PeopleTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'PeopleTbl')
        TRUNCATE TABLE [PeopleTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [PeopleTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [PeopleTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [PeopleTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [PeopleTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [PaymentTermsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'PaymentTermsTbl')
        TRUNCATE TABLE [PaymentTermsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [PaymentTermsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [PaymentTermsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [PaymentTermsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [PaymentTermsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [NextPrepDateByAreasTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'NextPrepDateByAreasTbl')
        TRUNCATE TABLE [NextPrepDateByAreasTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [NextPrepDateByAreasTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [NextPrepDateByAreasTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [NextPrepDateByAreasTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [NextPrepDateByAreasTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ItemUnitsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemUnitsTbl')
        TRUNCATE TABLE [ItemUnitsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemUnitsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemUnitsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemUnitsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemUnitsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ItemsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemsTbl')
        TRUNCATE TABLE [ItemsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ItemServiceTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemServiceTypesTbl')
        TRUNCATE TABLE [ItemServiceTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemServiceTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemServiceTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemServiceTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemServiceTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ItemPrepTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemPrepTypesTbl')
        TRUNCATE TABLE [ItemPrepTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemPrepTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemPrepTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemPrepTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemPrepTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ItemPackagingsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemPackagingsTbl')
        TRUNCATE TABLE [ItemPackagingsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemPackagingsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemPackagingsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemPackagingsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemPackagingsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ItemGroupsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemGroupsTbl')
        TRUNCATE TABLE [ItemGroupsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemGroupsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemGroupsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemGroupsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemGroupsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [InvoiceTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'InvoiceTypesTbl')
        TRUNCATE TABLE [InvoiceTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [InvoiceTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [InvoiceTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [InvoiceTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [InvoiceTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [HolidayClosuresTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'HolidayClosuresTbl')
        TRUNCATE TABLE [HolidayClosuresTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [HolidayClosuresTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [HolidayClosuresTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [HolidayClosuresTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [HolidayClosuresTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [EquipTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'EquipTypesTbl')
        TRUNCATE TABLE [EquipTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [EquipTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [EquipTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [EquipTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [EquipTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [EquipConditionsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'EquipConditionsTbl')
        TRUNCATE TABLE [EquipConditionsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [EquipConditionsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [EquipConditionsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [EquipConditionsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [EquipConditionsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ContactUsageLinesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ContactUsageLinesTbl')
        TRUNCATE TABLE [ContactUsageLinesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ContactUsageLinesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ContactUsageLinesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ContactUsageLinesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ContactUsageLinesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ContactTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ContactTypesTbl')
        TRUNCATE TABLE [ContactTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ContactTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ContactTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ContactTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ContactTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ContactTrackedServiceItemsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ContactTrackedServiceItemsTbl')
        TRUNCATE TABLE [ContactTrackedServiceItemsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ContactTrackedServiceItemsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ContactTrackedServiceItemsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ContactTrackedServiceItemsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ContactTrackedServiceItemsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ContactsUsageTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ContactsUsageTbl')
        TRUNCATE TABLE [ContactsUsageTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ContactsUsageTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ContactsUsageTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ContactsUsageTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ContactsUsageTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ContactsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ContactsTbl')
        TRUNCATE TABLE [ContactsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ContactsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ContactsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ContactsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ContactsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ContactsItemUsageTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ContactsItemUsageTbl')
        TRUNCATE TABLE [ContactsItemUsageTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ContactsItemUsageTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ContactsItemUsageTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ContactsItemUsageTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ContactsItemUsageTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ContactsAwayPeriodTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ContactsAwayPeriodTbl')
        TRUNCATE TABLE [ContactsAwayPeriodTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ContactsAwayPeriodTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ContactsAwayPeriodTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ContactsAwayPeriodTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ContactsAwayPeriodTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ContactsAccInfoTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ContactsAccInfoTbl')
        TRUNCATE TABLE [ContactsAccInfoTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ContactsAccInfoTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ContactsAccInfoTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ContactsAccInfoTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ContactsAccInfoTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ClosureDatesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ClosureDatesTbl')
        TRUNCATE TABLE [ClosureDatesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ClosureDatesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ClosureDatesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ClosureDatesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ClosureDatesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [AwayReasonTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'AwayReasonTbl')
        TRUNCATE TABLE [AwayReasonTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [AwayReasonTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [AwayReasonTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [AwayReasonTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [AwayReasonTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [AreasTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'AreasTbl')
        TRUNCATE TABLE [AreasTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [AreasTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [AreasTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [AreasTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [AreasTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [AreaPrepDaysTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'AreaPrepDaysTbl')
        TRUNCATE TABLE [AreaPrepDaysTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [AreaPrepDaysTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [AreaPrepDaysTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [AreaPrepDaysTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [AreaPrepDaysTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
GO

-- CityPrepDaysTbl -> AreaPrepDaysTbl
-- Mapping: columnsCount=5
--   CityPrepDaysID -> CityPrepDaysID
--   CityID -> CityID
--   PrepDayOfWeekID -> PrepDayOfWeekID
--   DeliveryDelayDays -> DeliveryDelayDays
--   DeliveryOrder -> DeliveryOrder
IF OBJECT_ID(N'AccessSrc.CityPrepDaysTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [AreaPrepDaysTbl] ([CityPrepDaysID], [CityID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]) SELECT NULLIF([CityPrepDaysID], N'''') AS [CityPrepDaysID], NULLIF([CityID], N'''') AS [CityID], NULLIF([PrepDayOfWeekID], N'''') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'''') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder] FROM [AccessSrc].[CityPrepDaysTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [AreaPrepDaysTbl]
        (
            [CityPrepDaysID], [CityID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]
        )
        SELECT
            NULLIF([CityPrepDaysID], N'') AS [CityPrepDaysID], NULLIF([CityID], N'') AS [CityID], NULLIF([PrepDayOfWeekID], N'') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder]
        FROM [AccessSrc].[CityPrepDaysTbl];
        COMMIT;
        PRINT 'OK migrate [AreaPrepDaysTbl] from ' + N'[AccessSrc].[CityPrepDaysTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [AreaPrepDaysTbl] from ' + N'[AccessSrc].[CityPrepDaysTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [AreaPrepDaysTbl] ([CityPrepDaysID], [CityID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]) SELECT NULLIF([CityPrepDaysID], N'''') AS [CityPrepDaysID], NULLIF([CityID], N'''') AS [CityID], NULLIF([PrepDayOfWeekID], N'''') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'''') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder] FROM [CityPrepDaysTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [AreaPrepDaysTbl]
        (
            [CityPrepDaysID], [CityID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]
        )
        SELECT
            NULLIF([CityPrepDaysID], N'') AS [CityPrepDaysID], NULLIF([CityID], N'') AS [CityID], NULLIF([PrepDayOfWeekID], N'') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder]
        FROM [CityPrepDaysTbl];
        COMMIT;
        PRINT 'OK migrate [AreaPrepDaysTbl] from ' + N'[CityPrepDaysTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [AreaPrepDaysTbl] from ' + N'[CityPrepDaysTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CityTbl -> AreasTbl
-- Mapping: columnsCount=4
--   ID -> ID
--   City -> City
--   RoastingDay -> RoastingDay
--   DeliveryDelay -> DeliveryDelay
IF OBJECT_ID(N'AccessSrc.CityTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [AreasTbl] ([ID], [City], [RoastingDay], [DeliveryDelay]) SELECT NULLIF([ID], N'''') AS [ID], NULLIF([City], N'''') AS [City], NULLIF([RoastingDay], N'''') AS [RoastingDay], NULLIF([DeliveryDelay], N'''') AS [DeliveryDelay] FROM [AccessSrc].[CityTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [AreasTbl]
        (
            [ID], [City], [RoastingDay], [DeliveryDelay]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], NULLIF([City], N'') AS [City], NULLIF([RoastingDay], N'') AS [RoastingDay], NULLIF([DeliveryDelay], N'') AS [DeliveryDelay]
        FROM [AccessSrc].[CityTbl];
        COMMIT;
        PRINT 'OK migrate [AreasTbl] from ' + N'[AccessSrc].[CityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [AreasTbl] from ' + N'[AccessSrc].[CityTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [AreasTbl] ([ID], [City], [RoastingDay], [DeliveryDelay]) SELECT NULLIF([ID], N'''') AS [ID], NULLIF([City], N'''') AS [City], NULLIF([RoastingDay], N'''') AS [RoastingDay], NULLIF([DeliveryDelay], N'''') AS [DeliveryDelay] FROM [CityTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [AreasTbl]
        (
            [ID], [City], [RoastingDay], [DeliveryDelay]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], NULLIF([City], N'') AS [City], NULLIF([RoastingDay], N'') AS [RoastingDay], NULLIF([DeliveryDelay], N'') AS [DeliveryDelay]
        FROM [CityTbl];
        COMMIT;
        PRINT 'OK migrate [AreasTbl] from ' + N'[CityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [AreasTbl] from ' + N'[CityTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- AwayReasonTbl -> AwayReasonTbl
-- Mapping: columnsCount=2
--   AwayReasonID -> AwayReasonID
--   ReasonDesc -> ReasonDesc
IF OBJECT_ID(N'AccessSrc.AwayReasonTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [AwayReasonTbl]: missing source [AccessSrc].[AwayReasonTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [AwayReasonTbl] ([AwayReasonID], [ReasonDesc]) SELECT NULLIF([AwayReasonID], N'''') AS [AwayReasonID], NULLIF([ReasonDesc], N'''') AS [ReasonDesc] FROM [AccessSrc].[AwayReasonTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [AwayReasonTbl] ON;
        INSERT INTO [AwayReasonTbl]
        (
            [AwayReasonID], [ReasonDesc]
        )
        SELECT
            NULLIF([AwayReasonID], N'') AS [AwayReasonID], NULLIF([ReasonDesc], N'') AS [ReasonDesc]
        FROM [AccessSrc].[AwayReasonTbl];
        SET IDENTITY_INSERT [AwayReasonTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'AwayReasonTbl'))
            DBCC CHECKIDENT (N'AwayReasonTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [AwayReasonTbl] from ' + N'[AccessSrc].[AwayReasonTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'AwayReasonTbl') IS NOT NULL SET IDENTITY_INSERT [AwayReasonTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [AwayReasonTbl] from ' + N'[AccessSrc].[AwayReasonTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClosureDatesTbl -> ClosureDatesTbl
-- Mapping: columnsCount=5
--   ID -> ID
--   DateClosed -> DateClosed
--   DateReopen -> DateReopen
--   NextRoastDate -> NextRoastDate
--   Comments -> Comments
IF OBJECT_ID(N'AccessSrc.ClosureDatesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ClosureDatesTbl]: missing source [AccessSrc].[ClosureDatesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ClosureDatesTbl] ([ID], [DateClosed], [DateReopen], [NextRoastDate], [Comments]) SELECT NULLIF([ID], N'''') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''))) AS [DateClosed], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''))) AS [DateReopen], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''''))) AS [NextRoastDate], NULLIF([Comments], N'''') AS [Comments] FROM [AccessSrc].[ClosureDatesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ClosureDatesTbl]
        (
            [ID], [DateClosed], [DateReopen], [NextRoastDate], [Comments]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''))) AS [DateClosed], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''))) AS [DateReopen], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''))) AS [NextRoastDate], NULLIF([Comments], N'') AS [Comments]
        FROM [AccessSrc].[ClosureDatesTbl];
        COMMIT;
        PRINT 'OK migrate [ClosureDatesTbl] from ' + N'[AccessSrc].[ClosureDatesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ClosureDatesTbl] from ' + N'[AccessSrc].[ClosureDatesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomersAccInfoTbl -> ContactsAccInfoTbl
-- Mapping: columnsCount=30
--   CustomersAccInfoID -> CustomersAccInfoID
--   CustomerID -> CustomerID
--   RequiresPurchOrder -> RequiresPurchOrder
--   CustomerVATNo -> CustomerVATNo
--   BillAddr1 -> BillAddr1
--   BillAddr2 -> BillAddr2
--   BillAddr3 -> BillAddr3
--   BillAddr4 -> BillAddr4
--   BillAddr5 -> BillAddr5
--   ShipAddr1 -> ShipAddr1
--   ShipAddr2 -> ShipAddr2
--   ShipAddr3 -> ShipAddr3
--   ShipAddr4 -> ShipAddr4
--   ShipAddr5 -> ShipAddr5
--   AccEmail -> AccEmail
--   AltAccEmail -> AltAccEmail
--   PaymentTermID -> PaymentTermID
--   Limit -> Limit
--   FullCoName -> FullCoName
--   AccFirstName -> AccFirstName
--   AccLastName -> AccLastName
--   AltAccFirstName -> AltAccFirstName
--   AltAccLastName -> AltAccLastName
--   PriceLevelID -> PriceLevelID
--   InvoiceTypeID -> InvoiceTypeID
--   RegNo -> RegNo
--   BankAccNo -> BankAccNo
--   BankBranch -> BankBranch
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.CustomersAccInfoTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsAccInfoTbl] ([CustomersAccInfoID], [CustomerID], [RequiresPurchOrder], [CustomerVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]) SELECT NULLIF([CustomersAccInfoID], N'''') AS [CustomersAccInfoID], NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([RequiresPurchOrder], N'''') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'''') AS [CustomerVATNo], NULLIF([BillAddr1], N'''') AS [BillAddr1], NULLIF([BillAddr2], N'''') AS [BillAddr2], NULLIF([BillAddr3], N'''') AS [BillAddr3], NULLIF([BillAddr4], N'''') AS [BillAddr4], NULLIF([BillAddr5], N'''') AS [BillAddr5], NULLIF([ShipAddr1], N'''') AS [ShipAddr1], NULLIF([ShipAddr2], N'''') AS [ShipAddr2], NULLIF([ShipAddr3], N'''') AS [ShipAddr3], NULLIF([ShipAddr4], N'''') AS [ShipAddr4], NULLIF([ShipAddr5], N'''') AS [ShipAddr5], NULLIF([AccEmail], N'''') AS [AccEmail], NULLIF([AltAccEmail], N'''') AS [AltAccEmail], NULLIF([PaymentTermID], N'''') AS [PaymentTermID], NULLIF([Limit], N'''') AS [Limit], NULLIF([FullCoName], N'''') AS [FullCoName], NULLIF([AccFirstName], N'''') AS [AccFirstName], NULLIF([AccLastName], N'''') AS [AccLastName], NULLIF([AltAccFirstName], N'''') AS [AltAccFirstName], NULLIF([AltAccLastName], N'''') AS [AltAccLastName], NULLI ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsAccInfoTbl]
        (
            [CustomersAccInfoID], [CustomerID], [RequiresPurchOrder], [CustomerVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]
        )
        SELECT
            NULLIF([CustomersAccInfoID], N'') AS [CustomersAccInfoID], NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([RequiresPurchOrder], N'') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'') AS [CustomerVATNo], NULLIF([BillAddr1], N'') AS [BillAddr1], NULLIF([BillAddr2], N'') AS [BillAddr2], NULLIF([BillAddr3], N'') AS [BillAddr3], NULLIF([BillAddr4], N'') AS [BillAddr4], NULLIF([BillAddr5], N'') AS [BillAddr5], NULLIF([ShipAddr1], N'') AS [ShipAddr1], NULLIF([ShipAddr2], N'') AS [ShipAddr2], NULLIF([ShipAddr3], N'') AS [ShipAddr3], NULLIF([ShipAddr4], N'') AS [ShipAddr4], NULLIF([ShipAddr5], N'') AS [ShipAddr5], NULLIF([AccEmail], N'') AS [AccEmail], NULLIF([AltAccEmail], N'') AS [AltAccEmail], NULLIF([PaymentTermID], N'') AS [PaymentTermID], NULLIF([Limit], N'') AS [Limit], NULLIF([FullCoName], N'') AS [FullCoName], NULLIF([AccFirstName], N'') AS [AccFirstName], NULLIF([AccLastName], N'') AS [AccLastName], NULLIF([AltAccFirstName], N'') AS [AltAccFirstName], NULLIF([AltAccLastName], N'') AS [AltAccLastName], NULLIF([PriceLevelID], N'') AS [PriceLevelID], NULLIF([InvoiceTypeID], N'') AS [InvoiceTypeID], NULLIF([RegNo], N'') AS [RegNo], NULLIF([BankAccNo], N'') AS [BankAccNo], NULLIF([BankBranch], N'') AS [BankBranch], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomersAccInfoTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsAccInfoTbl] from ' + N'[AccessSrc].[CustomersAccInfoTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsAccInfoTbl] from ' + N'[AccessSrc].[CustomersAccInfoTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsAccInfoTbl] ([CustomersAccInfoID], [CustomerID], [RequiresPurchOrder], [CustomerVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]) SELECT NULLIF([CustomersAccInfoID], N'''') AS [CustomersAccInfoID], NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([RequiresPurchOrder], N'''') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'''') AS [CustomerVATNo], NULLIF([BillAddr1], N'''') AS [BillAddr1], NULLIF([BillAddr2], N'''') AS [BillAddr2], NULLIF([BillAddr3], N'''') AS [BillAddr3], NULLIF([BillAddr4], N'''') AS [BillAddr4], NULLIF([BillAddr5], N'''') AS [BillAddr5], NULLIF([ShipAddr1], N'''') AS [ShipAddr1], NULLIF([ShipAddr2], N'''') AS [ShipAddr2], NULLIF([ShipAddr3], N'''') AS [ShipAddr3], NULLIF([ShipAddr4], N'''') AS [ShipAddr4], NULLIF([ShipAddr5], N'''') AS [ShipAddr5], NULLIF([AccEmail], N'''') AS [AccEmail], NULLIF([AltAccEmail], N'''') AS [AltAccEmail], NULLIF([PaymentTermID], N'''') AS [PaymentTermID], NULLIF([Limit], N'''') AS [Limit], NULLIF([FullCoName], N'''') AS [FullCoName], NULLIF([AccFirstName], N'''') AS [AccFirstName], NULLIF([AccLastName], N'''') AS [AccLastName], NULLIF([AltAccFirstName], N'''') AS [AltAccFirstName], NULLIF([AltAccLastName], N'''') AS [AltAccLastName], NULLI ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsAccInfoTbl]
        (
            [CustomersAccInfoID], [CustomerID], [RequiresPurchOrder], [CustomerVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]
        )
        SELECT
            NULLIF([CustomersAccInfoID], N'') AS [CustomersAccInfoID], NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([RequiresPurchOrder], N'') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'') AS [CustomerVATNo], NULLIF([BillAddr1], N'') AS [BillAddr1], NULLIF([BillAddr2], N'') AS [BillAddr2], NULLIF([BillAddr3], N'') AS [BillAddr3], NULLIF([BillAddr4], N'') AS [BillAddr4], NULLIF([BillAddr5], N'') AS [BillAddr5], NULLIF([ShipAddr1], N'') AS [ShipAddr1], NULLIF([ShipAddr2], N'') AS [ShipAddr2], NULLIF([ShipAddr3], N'') AS [ShipAddr3], NULLIF([ShipAddr4], N'') AS [ShipAddr4], NULLIF([ShipAddr5], N'') AS [ShipAddr5], NULLIF([AccEmail], N'') AS [AccEmail], NULLIF([AltAccEmail], N'') AS [AltAccEmail], NULLIF([PaymentTermID], N'') AS [PaymentTermID], NULLIF([Limit], N'') AS [Limit], NULLIF([FullCoName], N'') AS [FullCoName], NULLIF([AccFirstName], N'') AS [AccFirstName], NULLIF([AccLastName], N'') AS [AccLastName], NULLIF([AltAccFirstName], N'') AS [AltAccFirstName], NULLIF([AltAccLastName], N'') AS [AltAccLastName], NULLIF([PriceLevelID], N'') AS [PriceLevelID], NULLIF([InvoiceTypeID], N'') AS [InvoiceTypeID], NULLIF([RegNo], N'') AS [RegNo], NULLIF([BankAccNo], N'') AS [BankAccNo], NULLIF([BankBranch], N'') AS [BankBranch], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [CustomersAccInfoTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsAccInfoTbl] from ' + N'[CustomersAccInfoTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsAccInfoTbl] from ' + N'[CustomersAccInfoTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClientAwayPeriodTbl -> ContactsAwayPeriodTbl
-- Mapping: columnsCount=5
--   AwayPeriodID -> AwayPeriodID
--   ClientID -> ClientID
--   AwayStartDate -> AwayStartDate
--   AwayEndDate -> AwayEndDate
--   ReasonID -> ReasonID
IF OBJECT_ID(N'AccessSrc.ClientAwayPeriodTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsAwayPeriodTbl] ([AwayPeriodID], [ClientID], [AwayStartDate], [AwayEndDate], [ReasonID]) SELECT NULLIF([AwayPeriodID], N'''') AS [AwayPeriodID], NULLIF([ClientID], N'''') AS [ClientID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''))) AS [AwayStartDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''))) AS [AwayEndDate], NULLIF([ReasonID], N'''') AS [ReasonID] FROM [AccessSrc].[ClientAwayPeriodTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsAwayPeriodTbl]
        (
            [AwayPeriodID], [ClientID], [AwayStartDate], [AwayEndDate], [ReasonID]
        )
        SELECT
            NULLIF([AwayPeriodID], N'') AS [AwayPeriodID], NULLIF([ClientID], N'') AS [ClientID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''))) AS [AwayStartDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''))) AS [AwayEndDate], NULLIF([ReasonID], N'') AS [ReasonID]
        FROM [AccessSrc].[ClientAwayPeriodTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsAwayPeriodTbl] from ' + N'[AccessSrc].[ClientAwayPeriodTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsAwayPeriodTbl] from ' + N'[AccessSrc].[ClientAwayPeriodTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsAwayPeriodTbl] ([AwayPeriodID], [ClientID], [AwayStartDate], [AwayEndDate], [ReasonID]) SELECT NULLIF([AwayPeriodID], N'''') AS [AwayPeriodID], NULLIF([ClientID], N'''') AS [ClientID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''))) AS [AwayStartDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''))) AS [AwayEndDate], NULLIF([ReasonID], N'''') AS [ReasonID] FROM [ClientAwayPeriodTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsAwayPeriodTbl]
        (
            [AwayPeriodID], [ClientID], [AwayStartDate], [AwayEndDate], [ReasonID]
        )
        SELECT
            NULLIF([AwayPeriodID], N'') AS [AwayPeriodID], NULLIF([ClientID], N'') AS [ClientID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''))) AS [AwayStartDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''))) AS [AwayEndDate], NULLIF([ReasonID], N'') AS [ReasonID]
        FROM [ClientAwayPeriodTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsAwayPeriodTbl] from ' + N'[ClientAwayPeriodTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsAwayPeriodTbl] from ' + N'[ClientAwayPeriodTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemUsageTbl -> ContactsItemUsageTbl
-- Mapping: columnsCount=8
--   ClientUsageLineNo -> ClientUsageLineNo
--   CustomerID -> CustomerID
--   Date -> Date
--   ItemProvided -> ItemProvided
--   AmountProvided -> AmountProvided
--   PrepTypeID -> PrepTypeID
--   PackagingID -> PackagingID
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ItemUsageTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsItemUsageTbl] ([ClientUsageLineNo], [CustomerID], [Date], [ItemProvided], [AmountProvided], [PrepTypeID], [PackagingID], [Notes]) SELECT NULLIF([ClientUsageLineNo], N'''') AS [ClientUsageLineNo], NULLIF([CustomerID], N'''') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''))) AS [Date], NULLIF([ItemProvided], N'''') AS [ItemProvided], NULLIF([AmountProvided], N'''') AS [AmountProvided], NULLIF([PrepTypeID], N'''') AS [PrepTypeID], NULLIF([PackagingID], N'''') AS [PackagingID], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[ItemUsageTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsItemUsageTbl]
        (
            [ClientUsageLineNo], [CustomerID], [Date], [ItemProvided], [AmountProvided], [PrepTypeID], [PackagingID], [Notes]
        )
        SELECT
            NULLIF([ClientUsageLineNo], N'') AS [ClientUsageLineNo], NULLIF([CustomerID], N'') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''))) AS [Date], NULLIF([ItemProvided], N'') AS [ItemProvided], NULLIF([AmountProvided], N'') AS [AmountProvided], NULLIF([PrepTypeID], N'') AS [PrepTypeID], NULLIF([PackagingID], N'') AS [PackagingID], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[ItemUsageTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsItemUsageTbl] from ' + N'[AccessSrc].[ItemUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsItemUsageTbl] from ' + N'[AccessSrc].[ItemUsageTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsItemUsageTbl] ([ClientUsageLineNo], [CustomerID], [Date], [ItemProvided], [AmountProvided], [PrepTypeID], [PackagingID], [Notes]) SELECT NULLIF([ClientUsageLineNo], N'''') AS [ClientUsageLineNo], NULLIF([CustomerID], N'''') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''))) AS [Date], NULLIF([ItemProvided], N'''') AS [ItemProvided], NULLIF([AmountProvided], N'''') AS [AmountProvided], NULLIF([PrepTypeID], N'''') AS [PrepTypeID], NULLIF([PackagingID], N'''') AS [PackagingID], NULLIF([Notes], N'''') AS [Notes] FROM [ItemUsageTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsItemUsageTbl]
        (
            [ClientUsageLineNo], [CustomerID], [Date], [ItemProvided], [AmountProvided], [PrepTypeID], [PackagingID], [Notes]
        )
        SELECT
            NULLIF([ClientUsageLineNo], N'') AS [ClientUsageLineNo], NULLIF([CustomerID], N'') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''))) AS [Date], NULLIF([ItemProvided], N'') AS [ItemProvided], NULLIF([AmountProvided], N'') AS [AmountProvided], NULLIF([PrepTypeID], N'') AS [PrepTypeID], NULLIF([PackagingID], N'') AS [PackagingID], NULLIF([Notes], N'') AS [Notes]
        FROM [ItemUsageTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsItemUsageTbl] from ' + N'[ItemUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsItemUsageTbl] from ' + N'[ItemUsageTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomersTbl -> ContactsTbl
-- Mapping: columnsCount=43
--   CustomerID -> CustomerID
--   CompanyName -> CompanyName
--   ContactTitle -> ContactTitle
--   ContactFirstName -> ContactFirstName
--   ContactLastName -> ContactLastName
--   ContactAltFirstName -> ContactAltFirstName
--   ContactAltLastName -> ContactAltLastName
--   Department -> Department
--   BillingAddress -> BillingAddress
--   City -> City
--   StateOrProvince -> StateOrProvince
--   PostalCode -> PostalCode
--   Country/Region -> Country/Region
--   PhoneNumber -> PhoneNumber
--   Extension -> Extension
--   FaxNumber -> FaxNumber
--   CellNumber -> CellNumber
--   EmailAddress -> EmailAddress
--   AltEmailAddress -> AltEmailAddress
--   ContractNo -> ContractNo
--   CustomerTypeID -> CustomerTypeID
--   CustomerTypeOLD -> CustomerTypeOLD
--   EquipType -> EquipType
--   CoffeePreference -> CoffeePreference
--   PriPrefQty -> PriPrefQty
--   PrefPrepTypeID -> PrefPrepTypeID
--   PrefPackagingID -> PrefPackagingID
--   SecondaryPreference -> SecondaryPreference
--   SecPrefQty -> SecPrefQty
--   TypicallySecToo -> TypicallySecToo
--   PreferedAgent -> PreferedAgent
--   SalesAgentID -> SalesAgentID
--   MachineSN -> MachineSN
--   UsesFilter -> UsesFilter
--   autofulfill -> autofulfill
--   enabled -> enabled
--   PredictionDisabled -> PredictionDisabled
--   AlwaysSendChkUp -> AlwaysSendChkUp
--   NormallyResponds -> NormallyResponds
--   ReminderCount -> ReminderCount
--   Notes -> Notes
--   SendDeliveryConfirmation -> SendDeliveryConfirmation
--   LastDateSentReminder -> LastDateSentReminder
IF OBJECT_ID(N'AccessSrc.CustomersTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsTbl] ([CustomerID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [City], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [CustomerTypeID], [CustomerTypeOLD], [EquipType], [CoffeePreference], [PriPrefQty], [PrefPrepTypeID], [PrefPackagingID], [SecondaryPreference], [SecPrefQty], [TypicallySecToo], [PreferedAgent], [SalesAgentID], [MachineSN], [UsesFilter], [autofulfill], [enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]) SELECT NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([CompanyName], N'''') AS [CompanyName], NULLIF([ContactTitle], N'''') AS [ContactTitle], NULLIF([ContactFirstName], N'''') AS [ContactFirstName], NULLIF([ContactLastName], N'''') AS [ContactLastName], NULLIF([ContactAltFirstName], N'''') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'''') AS [ContactAltLastName], NULLIF([Department], N'''') AS [Department], NULLIF([BillingAddress], N'''') AS [BillingAddress], NULLIF([City], N'''') AS [City], NULLIF([StateOrProvince], N'''') AS [StateOrProvince], NULLIF([PostalCode], N'''') AS [PostalCode], NULLIF([Country/Region], N'''') AS [Country/Region], NULLIF([PhoneNumber], N'''') AS [PhoneNumber], NULLIF([Extension], N'''') AS [Extension], NULLIF([FaxNumber], ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsTbl]
        (
            [CustomerID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [City], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [CustomerTypeID], [CustomerTypeOLD], [EquipType], [CoffeePreference], [PriPrefQty], [PrefPrepTypeID], [PrefPackagingID], [SecondaryPreference], [SecPrefQty], [TypicallySecToo], [PreferedAgent], [SalesAgentID], [MachineSN], [UsesFilter], [autofulfill], [enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]
        )
        SELECT
            NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([CompanyName], N'') AS [CompanyName], NULLIF([ContactTitle], N'') AS [ContactTitle], NULLIF([ContactFirstName], N'') AS [ContactFirstName], NULLIF([ContactLastName], N'') AS [ContactLastName], NULLIF([ContactAltFirstName], N'') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'') AS [ContactAltLastName], NULLIF([Department], N'') AS [Department], NULLIF([BillingAddress], N'') AS [BillingAddress], NULLIF([City], N'') AS [City], NULLIF([StateOrProvince], N'') AS [StateOrProvince], NULLIF([PostalCode], N'') AS [PostalCode], NULLIF([Country/Region], N'') AS [Country/Region], NULLIF([PhoneNumber], N'') AS [PhoneNumber], NULLIF([Extension], N'') AS [Extension], NULLIF([FaxNumber], N'') AS [FaxNumber], NULLIF([CellNumber], N'') AS [CellNumber], NULLIF([EmailAddress], N'') AS [EmailAddress], NULLIF([AltEmailAddress], N'') AS [AltEmailAddress], NULLIF([ContractNo], N'') AS [ContractNo], NULLIF([CustomerTypeID], N'') AS [CustomerTypeID], NULLIF([CustomerTypeOLD], N'') AS [CustomerTypeOLD], NULLIF([EquipType], N'') AS [EquipType], NULLIF([CoffeePreference], N'') AS [CoffeePreference], NULLIF([PriPrefQty], N'') AS [PriPrefQty], NULLIF([PrefPrepTypeID], N'') AS [PrefPrepTypeID], NULLIF([PrefPackagingID], N'') AS [PrefPackagingID], NULLIF([SecondaryPreference], N'') AS [SecondaryPreference], NULLIF([SecPrefQty], N'') AS [SecPrefQty], CASE WHEN NULLIF([TypicallySecToo], N'') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TypicallySecToo], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'')) END AS [TypicallySecToo], NULLIF([PreferedAgent], N'') AS [PreferedAgent], NULLIF([SalesAgentID], N'') AS [SalesAgentID], NULLIF([MachineSN], N'') AS [MachineSN], CASE WHEN NULLIF([UsesFilter], N'') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UsesFilter], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'')) END AS [UsesFilter], CASE WHEN NULLIF([autofulfill], N'') IS NULL THEN NULL WHEN NULLIF([autofulfill], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([autofulfill], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([autofulfill], N'')) END AS [autofulfill], CASE WHEN NULLIF([enabled], N'') IS NULL THEN NULL WHEN NULLIF([enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([enabled], N'')) END AS [enabled], NULLIF([PredictionDisabled], N'') AS [PredictionDisabled], NULLIF([AlwaysSendChkUp], N'') AS [AlwaysSendChkUp], NULLIF([NormallyResponds], N'') AS [NormallyResponds], NULLIF([ReminderCount], N'') AS [ReminderCount], NULLIF([Notes], N'') AS [Notes], CASE WHEN NULLIF([SendDeliveryConfirmation], N'') IS NULL THEN NULL WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([SendDeliveryConfirmation], N'')) END AS [SendDeliveryConfirmation], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''))) AS [LastDateSentReminder]
        FROM [AccessSrc].[CustomersTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsTbl] from ' + N'[AccessSrc].[CustomersTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsTbl] from ' + N'[AccessSrc].[CustomersTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsTbl] ([CustomerID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [City], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [CustomerTypeID], [CustomerTypeOLD], [EquipType], [CoffeePreference], [PriPrefQty], [PrefPrepTypeID], [PrefPackagingID], [SecondaryPreference], [SecPrefQty], [TypicallySecToo], [PreferedAgent], [SalesAgentID], [MachineSN], [UsesFilter], [autofulfill], [enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]) SELECT NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([CompanyName], N'''') AS [CompanyName], NULLIF([ContactTitle], N'''') AS [ContactTitle], NULLIF([ContactFirstName], N'''') AS [ContactFirstName], NULLIF([ContactLastName], N'''') AS [ContactLastName], NULLIF([ContactAltFirstName], N'''') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'''') AS [ContactAltLastName], NULLIF([Department], N'''') AS [Department], NULLIF([BillingAddress], N'''') AS [BillingAddress], NULLIF([City], N'''') AS [City], NULLIF([StateOrProvince], N'''') AS [StateOrProvince], NULLIF([PostalCode], N'''') AS [PostalCode], NULLIF([Country/Region], N'''') AS [Country/Region], NULLIF([PhoneNumber], N'''') AS [PhoneNumber], NULLIF([Extension], N'''') AS [Extension], NULLIF([FaxNumber], ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsTbl]
        (
            [CustomerID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [City], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [CustomerTypeID], [CustomerTypeOLD], [EquipType], [CoffeePreference], [PriPrefQty], [PrefPrepTypeID], [PrefPackagingID], [SecondaryPreference], [SecPrefQty], [TypicallySecToo], [PreferedAgent], [SalesAgentID], [MachineSN], [UsesFilter], [autofulfill], [enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]
        )
        SELECT
            NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([CompanyName], N'') AS [CompanyName], NULLIF([ContactTitle], N'') AS [ContactTitle], NULLIF([ContactFirstName], N'') AS [ContactFirstName], NULLIF([ContactLastName], N'') AS [ContactLastName], NULLIF([ContactAltFirstName], N'') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'') AS [ContactAltLastName], NULLIF([Department], N'') AS [Department], NULLIF([BillingAddress], N'') AS [BillingAddress], NULLIF([City], N'') AS [City], NULLIF([StateOrProvince], N'') AS [StateOrProvince], NULLIF([PostalCode], N'') AS [PostalCode], NULLIF([Country/Region], N'') AS [Country/Region], NULLIF([PhoneNumber], N'') AS [PhoneNumber], NULLIF([Extension], N'') AS [Extension], NULLIF([FaxNumber], N'') AS [FaxNumber], NULLIF([CellNumber], N'') AS [CellNumber], NULLIF([EmailAddress], N'') AS [EmailAddress], NULLIF([AltEmailAddress], N'') AS [AltEmailAddress], NULLIF([ContractNo], N'') AS [ContractNo], NULLIF([CustomerTypeID], N'') AS [CustomerTypeID], NULLIF([CustomerTypeOLD], N'') AS [CustomerTypeOLD], NULLIF([EquipType], N'') AS [EquipType], NULLIF([CoffeePreference], N'') AS [CoffeePreference], NULLIF([PriPrefQty], N'') AS [PriPrefQty], NULLIF([PrefPrepTypeID], N'') AS [PrefPrepTypeID], NULLIF([PrefPackagingID], N'') AS [PrefPackagingID], NULLIF([SecondaryPreference], N'') AS [SecondaryPreference], NULLIF([SecPrefQty], N'') AS [SecPrefQty], CASE WHEN NULLIF([TypicallySecToo], N'') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TypicallySecToo], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'')) END AS [TypicallySecToo], NULLIF([PreferedAgent], N'') AS [PreferedAgent], NULLIF([SalesAgentID], N'') AS [SalesAgentID], NULLIF([MachineSN], N'') AS [MachineSN], CASE WHEN NULLIF([UsesFilter], N'') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UsesFilter], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'')) END AS [UsesFilter], CASE WHEN NULLIF([autofulfill], N'') IS NULL THEN NULL WHEN NULLIF([autofulfill], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([autofulfill], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([autofulfill], N'')) END AS [autofulfill], CASE WHEN NULLIF([enabled], N'') IS NULL THEN NULL WHEN NULLIF([enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([enabled], N'')) END AS [enabled], NULLIF([PredictionDisabled], N'') AS [PredictionDisabled], NULLIF([AlwaysSendChkUp], N'') AS [AlwaysSendChkUp], NULLIF([NormallyResponds], N'') AS [NormallyResponds], NULLIF([ReminderCount], N'') AS [ReminderCount], NULLIF([Notes], N'') AS [Notes], CASE WHEN NULLIF([SendDeliveryConfirmation], N'') IS NULL THEN NULL WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([SendDeliveryConfirmation], N'')) END AS [SendDeliveryConfirmation], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''))) AS [LastDateSentReminder]
        FROM [CustomersTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsTbl] from ' + N'[CustomersTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsTbl] from ' + N'[CustomersTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClientUsageTbl -> ContactsUsageTbl
-- Mapping: columnsCount=12
--   CustomerId -> CustomerId
--   LastCupCount -> LastCupCount
--   NextCoffeeBy -> NextCoffeeBy
--   NextCleanOn -> NextCleanOn
--   NextFilterEst -> NextFilterEst
--   NextDescaleEst -> NextDescaleEst
--   NextServiceEst -> NextServiceEst
--   DailyConsumption -> DailyConsumption
--   FilterAveCount -> FilterAveCount
--   DescaleAveCount -> DescaleAveCount
--   ServiceAveCount -> ServiceAveCount
--   CleanAveCount -> CleanAveCount
IF OBJECT_ID(N'AccessSrc.ClientUsageTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsUsageTbl] ([CustomerId], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]) SELECT NULLIF([CustomerId], N'''') AS [CustomerId], NULLIF([LastCupCount], N'''') AS [LastCupCount], NULLIF([NextCoffeeBy], N'''') AS [NextCoffeeBy], NULLIF([NextCleanOn], N'''') AS [NextCleanOn], NULLIF([NextFilterEst], N'''') AS [NextFilterEst], NULLIF([NextDescaleEst], N'''') AS [NextDescaleEst], NULLIF([NextServiceEst], N'''') AS [NextServiceEst], NULLIF([DailyConsumption], N'''') AS [DailyConsumption], NULLIF([FilterAveCount], N'''') AS [FilterAveCount], NULLIF([DescaleAveCount], N'''') AS [DescaleAveCount], NULLIF([ServiceAveCount], N'''') AS [ServiceAveCount], NULLIF([CleanAveCount], N'''') AS [CleanAveCount] FROM [AccessSrc].[ClientUsageTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsUsageTbl]
        (
            [CustomerId], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]
        )
        SELECT
            NULLIF([CustomerId], N'') AS [CustomerId], NULLIF([LastCupCount], N'') AS [LastCupCount], NULLIF([NextCoffeeBy], N'') AS [NextCoffeeBy], NULLIF([NextCleanOn], N'') AS [NextCleanOn], NULLIF([NextFilterEst], N'') AS [NextFilterEst], NULLIF([NextDescaleEst], N'') AS [NextDescaleEst], NULLIF([NextServiceEst], N'') AS [NextServiceEst], NULLIF([DailyConsumption], N'') AS [DailyConsumption], NULLIF([FilterAveCount], N'') AS [FilterAveCount], NULLIF([DescaleAveCount], N'') AS [DescaleAveCount], NULLIF([ServiceAveCount], N'') AS [ServiceAveCount], NULLIF([CleanAveCount], N'') AS [CleanAveCount]
        FROM [AccessSrc].[ClientUsageTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsUsageTbl] from ' + N'[AccessSrc].[ClientUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsUsageTbl] from ' + N'[AccessSrc].[ClientUsageTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsUsageTbl] ([CustomerId], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]) SELECT NULLIF([CustomerId], N'''') AS [CustomerId], NULLIF([LastCupCount], N'''') AS [LastCupCount], NULLIF([NextCoffeeBy], N'''') AS [NextCoffeeBy], NULLIF([NextCleanOn], N'''') AS [NextCleanOn], NULLIF([NextFilterEst], N'''') AS [NextFilterEst], NULLIF([NextDescaleEst], N'''') AS [NextDescaleEst], NULLIF([NextServiceEst], N'''') AS [NextServiceEst], NULLIF([DailyConsumption], N'''') AS [DailyConsumption], NULLIF([FilterAveCount], N'''') AS [FilterAveCount], NULLIF([DescaleAveCount], N'''') AS [DescaleAveCount], NULLIF([ServiceAveCount], N'''') AS [ServiceAveCount], NULLIF([CleanAveCount], N'''') AS [CleanAveCount] FROM [ClientUsageTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsUsageTbl]
        (
            [CustomerId], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]
        )
        SELECT
            NULLIF([CustomerId], N'') AS [CustomerId], NULLIF([LastCupCount], N'') AS [LastCupCount], NULLIF([NextCoffeeBy], N'') AS [NextCoffeeBy], NULLIF([NextCleanOn], N'') AS [NextCleanOn], NULLIF([NextFilterEst], N'') AS [NextFilterEst], NULLIF([NextDescaleEst], N'') AS [NextDescaleEst], NULLIF([NextServiceEst], N'') AS [NextServiceEst], NULLIF([DailyConsumption], N'') AS [DailyConsumption], NULLIF([FilterAveCount], N'') AS [FilterAveCount], NULLIF([DescaleAveCount], N'') AS [DescaleAveCount], NULLIF([ServiceAveCount], N'') AS [ServiceAveCount], NULLIF([CleanAveCount], N'') AS [CleanAveCount]
        FROM [ClientUsageTbl];
        COMMIT;
        PRINT 'OK migrate [ContactsUsageTbl] from ' + N'[ClientUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsUsageTbl] from ' + N'[ClientUsageTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomerTrackedServiceItemsTbl -> ContactTrackedServiceItemsTbl
-- Mapping: columnsCount=4
--   CustomerTrackedServiceItemsID -> CustomerTrackedServiceItemsID
--   CustomerTypeID -> CustomerTypeID
--   ServiceTypeID -> ServiceTypeID
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.CustomerTrackedServiceItemsTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactTrackedServiceItemsTbl] ([CustomerTrackedServiceItemsID], [CustomerTypeID], [ServiceTypeID], [Notes]) SELECT NULLIF([CustomerTrackedServiceItemsID], N'''') AS [CustomerTrackedServiceItemsID], NULLIF([CustomerTypeID], N'''') AS [CustomerTypeID], NULLIF([ServiceTypeID], N'''') AS [ServiceTypeID], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[CustomerTrackedServiceItemsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactTrackedServiceItemsTbl]
        (
            [CustomerTrackedServiceItemsID], [CustomerTypeID], [ServiceTypeID], [Notes]
        )
        SELECT
            NULLIF([CustomerTrackedServiceItemsID], N'') AS [CustomerTrackedServiceItemsID], NULLIF([CustomerTypeID], N'') AS [CustomerTypeID], NULLIF([ServiceTypeID], N'') AS [ServiceTypeID], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomerTrackedServiceItemsTbl];
        COMMIT;
        PRINT 'OK migrate [ContactTrackedServiceItemsTbl] from ' + N'[AccessSrc].[CustomerTrackedServiceItemsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactTrackedServiceItemsTbl] from ' + N'[AccessSrc].[CustomerTrackedServiceItemsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactTrackedServiceItemsTbl] ([CustomerTrackedServiceItemsID], [CustomerTypeID], [ServiceTypeID], [Notes]) SELECT NULLIF([CustomerTrackedServiceItemsID], N'''') AS [CustomerTrackedServiceItemsID], NULLIF([CustomerTypeID], N'''') AS [CustomerTypeID], NULLIF([ServiceTypeID], N'''') AS [ServiceTypeID], NULLIF([Notes], N'''') AS [Notes] FROM [CustomerTrackedServiceItemsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactTrackedServiceItemsTbl]
        (
            [CustomerTrackedServiceItemsID], [CustomerTypeID], [ServiceTypeID], [Notes]
        )
        SELECT
            NULLIF([CustomerTrackedServiceItemsID], N'') AS [CustomerTrackedServiceItemsID], NULLIF([CustomerTypeID], N'') AS [CustomerTypeID], NULLIF([ServiceTypeID], N'') AS [ServiceTypeID], NULLIF([Notes], N'') AS [Notes]
        FROM [CustomerTrackedServiceItemsTbl];
        COMMIT;
        PRINT 'OK migrate [ContactTrackedServiceItemsTbl] from ' + N'[CustomerTrackedServiceItemsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactTrackedServiceItemsTbl] from ' + N'[CustomerTrackedServiceItemsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomerTypeTbl -> ContactTypesTbl
-- Mapping: columnsCount=3
--   CustTypeID -> CustTypeID
--   CustTypeDesc -> CustTypeDesc
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.CustomerTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactTypesTbl] ([CustTypeID], [CustTypeDesc], [Notes]) SELECT NULLIF([CustTypeID], N'''') AS [CustTypeID], NULLIF([CustTypeDesc], N'''') AS [CustTypeDesc], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[CustomerTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactTypesTbl]
        (
            [CustTypeID], [CustTypeDesc], [Notes]
        )
        SELECT
            NULLIF([CustTypeID], N'') AS [CustTypeID], NULLIF([CustTypeDesc], N'') AS [CustTypeDesc], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomerTypeTbl];
        COMMIT;
        PRINT 'OK migrate [ContactTypesTbl] from ' + N'[AccessSrc].[CustomerTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactTypesTbl] from ' + N'[AccessSrc].[CustomerTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactTypesTbl] ([CustTypeID], [CustTypeDesc], [Notes]) SELECT NULLIF([CustTypeID], N'''') AS [CustTypeID], NULLIF([CustTypeDesc], N'''') AS [CustTypeDesc], NULLIF([Notes], N'''') AS [Notes] FROM [CustomerTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactTypesTbl]
        (
            [CustTypeID], [CustTypeDesc], [Notes]
        )
        SELECT
            NULLIF([CustTypeID], N'') AS [CustTypeID], NULLIF([CustTypeDesc], N'') AS [CustTypeDesc], NULLIF([Notes], N'') AS [Notes]
        FROM [CustomerTypeTbl];
        COMMIT;
        PRINT 'OK migrate [ContactTypesTbl] from ' + N'[CustomerTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactTypesTbl] from ' + N'[CustomerTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClientUsageLinesTbl -> ContactUsageLinesTbl
-- Mapping: columnsCount=7
--   ClientUsageLineNo -> ClientUsageLineNo
--   CustomerID -> CustomerID
--   Date -> Date
--   CupCount -> CupCount
--   ServiceTypeId -> ServiceTypeId
--   Qty -> Qty
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ClientUsageLinesTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactUsageLinesTbl] ([ClientUsageLineNo], [CustomerID], [Date], [CupCount], [ServiceTypeId], [Qty], [Notes]) SELECT NULLIF([ClientUsageLineNo], N'''') AS [ClientUsageLineNo], NULLIF([CustomerID], N'''') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''))) AS [Date], NULLIF([CupCount], N'''') AS [CupCount], NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([Qty], N'''') AS [Qty], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[ClientUsageLinesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactUsageLinesTbl]
        (
            [ClientUsageLineNo], [CustomerID], [Date], [CupCount], [ServiceTypeId], [Qty], [Notes]
        )
        SELECT
            NULLIF([ClientUsageLineNo], N'') AS [ClientUsageLineNo], NULLIF([CustomerID], N'') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''))) AS [Date], NULLIF([CupCount], N'') AS [CupCount], NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([Qty], N'') AS [Qty], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[ClientUsageLinesTbl];
        COMMIT;
        PRINT 'OK migrate [ContactUsageLinesTbl] from ' + N'[AccessSrc].[ClientUsageLinesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactUsageLinesTbl] from ' + N'[AccessSrc].[ClientUsageLinesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactUsageLinesTbl] ([ClientUsageLineNo], [CustomerID], [Date], [CupCount], [ServiceTypeId], [Qty], [Notes]) SELECT NULLIF([ClientUsageLineNo], N'''') AS [ClientUsageLineNo], NULLIF([CustomerID], N'''') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''))) AS [Date], NULLIF([CupCount], N'''') AS [CupCount], NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([Qty], N'''') AS [Qty], NULLIF([Notes], N'''') AS [Notes] FROM [ClientUsageLinesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactUsageLinesTbl]
        (
            [ClientUsageLineNo], [CustomerID], [Date], [CupCount], [ServiceTypeId], [Qty], [Notes]
        )
        SELECT
            NULLIF([ClientUsageLineNo], N'') AS [ClientUsageLineNo], NULLIF([CustomerID], N'') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''))) AS [Date], NULLIF([CupCount], N'') AS [CupCount], NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([Qty], N'') AS [Qty], NULLIF([Notes], N'') AS [Notes]
        FROM [ClientUsageLinesTbl];
        COMMIT;
        PRINT 'OK migrate [ContactUsageLinesTbl] from ' + N'[ClientUsageLinesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactUsageLinesTbl] from ' + N'[ClientUsageLinesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- MachineConditionsTbl -> EquipConditionsTbl
-- Mapping: columnsCount=4
--   MachineConditionID -> MachineConditionID
--   ConditionDesc -> ConditionDesc
--   SortOrder -> SortOrder
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.MachineConditionsTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [EquipConditionsTbl] ([MachineConditionID], [ConditionDesc], [SortOrder], [Notes]) SELECT NULLIF([MachineConditionID], N'''') AS [MachineConditionID], NULLIF([ConditionDesc], N'''') AS [ConditionDesc], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[MachineConditionsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [EquipConditionsTbl]
        (
            [MachineConditionID], [ConditionDesc], [SortOrder], [Notes]
        )
        SELECT
            NULLIF([MachineConditionID], N'') AS [MachineConditionID], NULLIF([ConditionDesc], N'') AS [ConditionDesc], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[MachineConditionsTbl];
        COMMIT;
        PRINT 'OK migrate [EquipConditionsTbl] from ' + N'[AccessSrc].[MachineConditionsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [EquipConditionsTbl] from ' + N'[AccessSrc].[MachineConditionsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [EquipConditionsTbl] ([MachineConditionID], [ConditionDesc], [SortOrder], [Notes]) SELECT NULLIF([MachineConditionID], N'''') AS [MachineConditionID], NULLIF([ConditionDesc], N'''') AS [ConditionDesc], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes] FROM [MachineConditionsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [EquipConditionsTbl]
        (
            [MachineConditionID], [ConditionDesc], [SortOrder], [Notes]
        )
        SELECT
            NULLIF([MachineConditionID], N'') AS [MachineConditionID], NULLIF([ConditionDesc], N'') AS [ConditionDesc], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes]
        FROM [MachineConditionsTbl];
        COMMIT;
        PRINT 'OK migrate [EquipConditionsTbl] from ' + N'[MachineConditionsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [EquipConditionsTbl] from ' + N'[MachineConditionsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- EquipTypeTbl -> EquipTypesTbl
-- Mapping: columnsCount=3
--   EquipTypeId -> EquipTypeId
--   EquipTypeName -> EquipTypeName
--   EquipTypeDesc -> EquipTypeDesc
IF OBJECT_ID(N'AccessSrc.EquipTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [EquipTypesTbl] ([EquipTypeId], [EquipTypeName], [EquipTypeDesc]) SELECT NULLIF([EquipTypeId], N'''') AS [EquipTypeId], NULLIF([EquipTypeName], N'''') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'''') AS [EquipTypeDesc] FROM [AccessSrc].[EquipTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [EquipTypesTbl] ON;
        INSERT INTO [EquipTypesTbl]
        (
            [EquipTypeId], [EquipTypeName], [EquipTypeDesc]
        )
        SELECT
            NULLIF([EquipTypeId], N'') AS [EquipTypeId], NULLIF([EquipTypeName], N'') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'') AS [EquipTypeDesc]
        FROM [AccessSrc].[EquipTypeTbl];
        SET IDENTITY_INSERT [EquipTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'EquipTypesTbl'))
            DBCC CHECKIDENT (N'EquipTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [EquipTypesTbl] from ' + N'[AccessSrc].[EquipTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'EquipTypesTbl') IS NOT NULL SET IDENTITY_INSERT [EquipTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [EquipTypesTbl] from ' + N'[AccessSrc].[EquipTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [EquipTypesTbl] ([EquipTypeId], [EquipTypeName], [EquipTypeDesc]) SELECT NULLIF([EquipTypeId], N'''') AS [EquipTypeId], NULLIF([EquipTypeName], N'''') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'''') AS [EquipTypeDesc] FROM [EquipTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [EquipTypesTbl] ON;
        INSERT INTO [EquipTypesTbl]
        (
            [EquipTypeId], [EquipTypeName], [EquipTypeDesc]
        )
        SELECT
            NULLIF([EquipTypeId], N'') AS [EquipTypeId], NULLIF([EquipTypeName], N'') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'') AS [EquipTypeDesc]
        FROM [EquipTypeTbl];
        SET IDENTITY_INSERT [EquipTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'EquipTypesTbl'))
            DBCC CHECKIDENT (N'EquipTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [EquipTypesTbl] from ' + N'[EquipTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'EquipTypesTbl') IS NOT NULL SET IDENTITY_INSERT [EquipTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [EquipTypesTbl] from ' + N'[EquipTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- HolidayClosureTbl -> HolidayClosuresTbl
-- Mapping: columnsCount=7
--   ID -> ID
--   ClosureDate -> ClosureDate
--   DaysClosed -> DaysClosed
--   AppliesToPrep -> AppliesToPrep
--   AppliesToDelivery -> AppliesToDelivery
--   ShiftStrategy -> ShiftStrategy
--   Description -> Description
IF OBJECT_ID(N'AccessSrc.HolidayClosureTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [HolidayClosuresTbl] ([ID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]) SELECT NULLIF([ID], N'''') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''))) AS [ClosureDate], NULLIF([DaysClosed], N'''') AS [DaysClosed], NULLIF([AppliesToPrep], N'''') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'''') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'''') AS [ShiftStrategy], NULLIF([Description], N'''') AS [Description] FROM [AccessSrc].[HolidayClosureTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [HolidayClosuresTbl]
        (
            [ID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''))) AS [ClosureDate], NULLIF([DaysClosed], N'') AS [DaysClosed], NULLIF([AppliesToPrep], N'') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'') AS [ShiftStrategy], NULLIF([Description], N'') AS [Description]
        FROM [AccessSrc].[HolidayClosureTbl];
        COMMIT;
        PRINT 'OK migrate [HolidayClosuresTbl] from ' + N'[AccessSrc].[HolidayClosureTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [HolidayClosuresTbl] from ' + N'[AccessSrc].[HolidayClosureTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [HolidayClosuresTbl] ([ID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]) SELECT NULLIF([ID], N'''') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''))) AS [ClosureDate], NULLIF([DaysClosed], N'''') AS [DaysClosed], NULLIF([AppliesToPrep], N'''') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'''') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'''') AS [ShiftStrategy], NULLIF([Description], N'''') AS [Description] FROM [HolidayClosureTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [HolidayClosuresTbl]
        (
            [ID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''))) AS [ClosureDate], NULLIF([DaysClosed], N'') AS [DaysClosed], NULLIF([AppliesToPrep], N'') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'') AS [ShiftStrategy], NULLIF([Description], N'') AS [Description]
        FROM [HolidayClosureTbl];
        COMMIT;
        PRINT 'OK migrate [HolidayClosuresTbl] from ' + N'[HolidayClosureTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [HolidayClosuresTbl] from ' + N'[HolidayClosureTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- InvoiceTypeTbl -> InvoiceTypesTbl
-- Mapping: columnsCount=4
--   InvoiceTypeID -> InvoiceTypeID
--   InvoiceTypeDesc -> InvoiceTypeDesc
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.InvoiceTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [InvoiceTypesTbl] ([InvoiceTypeID], [InvoiceTypeDesc], [Enabled], [Notes]) SELECT NULLIF([InvoiceTypeID], N'''') AS [InvoiceTypeID], NULLIF([InvoiceTypeDesc], N'''') AS [InvoiceTypeDesc], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[InvoiceTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [InvoiceTypesTbl] ON;
        INSERT INTO [InvoiceTypesTbl]
        (
            [InvoiceTypeID], [InvoiceTypeDesc], [Enabled], [Notes]
        )
        SELECT
            NULLIF([InvoiceTypeID], N'') AS [InvoiceTypeID], NULLIF([InvoiceTypeDesc], N'') AS [InvoiceTypeDesc], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[InvoiceTypeTbl];
        SET IDENTITY_INSERT [InvoiceTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'InvoiceTypesTbl'))
            DBCC CHECKIDENT (N'InvoiceTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [InvoiceTypesTbl] from ' + N'[AccessSrc].[InvoiceTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'InvoiceTypesTbl') IS NOT NULL SET IDENTITY_INSERT [InvoiceTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [InvoiceTypesTbl] from ' + N'[AccessSrc].[InvoiceTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [InvoiceTypesTbl] ([InvoiceTypeID], [InvoiceTypeDesc], [Enabled], [Notes]) SELECT NULLIF([InvoiceTypeID], N'''') AS [InvoiceTypeID], NULLIF([InvoiceTypeDesc], N'''') AS [InvoiceTypeDesc], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [InvoiceTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [InvoiceTypesTbl] ON;
        INSERT INTO [InvoiceTypesTbl]
        (
            [InvoiceTypeID], [InvoiceTypeDesc], [Enabled], [Notes]
        )
        SELECT
            NULLIF([InvoiceTypeID], N'') AS [InvoiceTypeID], NULLIF([InvoiceTypeDesc], N'') AS [InvoiceTypeDesc], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [InvoiceTypeTbl];
        SET IDENTITY_INSERT [InvoiceTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'InvoiceTypesTbl'))
            DBCC CHECKIDENT (N'InvoiceTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [InvoiceTypesTbl] from ' + N'[InvoiceTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'InvoiceTypesTbl') IS NOT NULL SET IDENTITY_INSERT [InvoiceTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [InvoiceTypesTbl] from ' + N'[InvoiceTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemGroupTbl -> ItemGroupsTbl
-- Mapping: columnsCount=6
--   ItemGroupID -> ItemGroupID
--   GroupItemTypeID -> GroupItemTypeID
--   ItemTypeID -> ItemTypeID
--   ItemTypeSortPos -> ItemTypeSortPos
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ItemGroupTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemGroupsTbl] ([ItemGroupID], [GroupItemTypeID], [ItemTypeID], [ItemTypeSortPos], [Enabled], [Notes]) SELECT NULLIF([ItemGroupID], N'''') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'''') AS [GroupItemTypeID], NULLIF([ItemTypeID], N'''') AS [ItemTypeID], NULLIF([ItemTypeSortPos], N'''') AS [ItemTypeSortPos], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[ItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemGroupsTbl] ON;
        INSERT INTO [ItemGroupsTbl]
        (
            [ItemGroupID], [GroupItemTypeID], [ItemTypeID], [ItemTypeSortPos], [Enabled], [Notes]
        )
        SELECT
            NULLIF([ItemGroupID], N'') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'') AS [GroupItemTypeID], NULLIF([ItemTypeID], N'') AS [ItemTypeID], NULLIF([ItemTypeSortPos], N'') AS [ItemTypeSortPos], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[ItemGroupTbl];
        SET IDENTITY_INSERT [ItemGroupsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemGroupsTbl'))
            DBCC CHECKIDENT (N'ItemGroupsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemGroupsTbl] from ' + N'[AccessSrc].[ItemGroupTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemGroupsTbl') IS NOT NULL SET IDENTITY_INSERT [ItemGroupsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemGroupsTbl] from ' + N'[AccessSrc].[ItemGroupTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemGroupsTbl] ([ItemGroupID], [GroupItemTypeID], [ItemTypeID], [ItemTypeSortPos], [Enabled], [Notes]) SELECT NULLIF([ItemGroupID], N'''') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'''') AS [GroupItemTypeID], NULLIF([ItemTypeID], N'''') AS [ItemTypeID], NULLIF([ItemTypeSortPos], N'''') AS [ItemTypeSortPos], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [ItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemGroupsTbl] ON;
        INSERT INTO [ItemGroupsTbl]
        (
            [ItemGroupID], [GroupItemTypeID], [ItemTypeID], [ItemTypeSortPos], [Enabled], [Notes]
        )
        SELECT
            NULLIF([ItemGroupID], N'') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'') AS [GroupItemTypeID], NULLIF([ItemTypeID], N'') AS [ItemTypeID], NULLIF([ItemTypeSortPos], N'') AS [ItemTypeSortPos], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [ItemGroupTbl];
        SET IDENTITY_INSERT [ItemGroupsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemGroupsTbl'))
            DBCC CHECKIDENT (N'ItemGroupsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemGroupsTbl] from ' + N'[ItemGroupTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemGroupsTbl') IS NOT NULL SET IDENTITY_INSERT [ItemGroupsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemGroupsTbl] from ' + N'[ItemGroupTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- PackagingTbl -> ItemPackagingsTbl
-- Mapping: columnsCount=6
--   PackagingID -> ItemPackagingID
--   Description -> ItemPackagingDesc
--   AdditionalNotes -> AdditionalNotes
--   Symbol -> Symbol
--   Colour -> Colour
--   BGColour -> BGColour
IF OBJECT_ID(N'AccessSrc.PackagingTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemPackagingsTbl] ([ItemPackagingID], [ItemPackagingDesc], [AdditionalNotes], [Symbol], [Colour], [BGColour]) SELECT NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([Description], N'''') AS [ItemPackagingDesc], NULLIF([AdditionalNotes], N'''') AS [AdditionalNotes], NULLIF([Symbol], N'''') AS [Symbol], NULLIF([Colour], N'''') AS [Colour], NULLIF([BGColour], N'''') AS [BGColour] FROM [AccessSrc].[PackagingTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemPackagingsTbl] ON;
        INSERT INTO [ItemPackagingsTbl]
        (
            [ItemPackagingID], [ItemPackagingDesc], [AdditionalNotes], [Symbol], [Colour], [BGColour]
        )
        SELECT
            NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([Description], N'') AS [ItemPackagingDesc], NULLIF([AdditionalNotes], N'') AS [AdditionalNotes], NULLIF([Symbol], N'') AS [Symbol], NULLIF([Colour], N'') AS [Colour], NULLIF([BGColour], N'') AS [BGColour]
        FROM [AccessSrc].[PackagingTbl];
        SET IDENTITY_INSERT [ItemPackagingsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemPackagingsTbl'))
            DBCC CHECKIDENT (N'ItemPackagingsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemPackagingsTbl] from ' + N'[AccessSrc].[PackagingTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemPackagingsTbl') IS NOT NULL SET IDENTITY_INSERT [ItemPackagingsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemPackagingsTbl] from ' + N'[AccessSrc].[PackagingTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemPackagingsTbl] ([ItemPackagingID], [ItemPackagingDesc], [AdditionalNotes], [Symbol], [Colour], [BGColour]) SELECT NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([Description], N'''') AS [ItemPackagingDesc], NULLIF([AdditionalNotes], N'''') AS [AdditionalNotes], NULLIF([Symbol], N'''') AS [Symbol], NULLIF([Colour], N'''') AS [Colour], NULLIF([BGColour], N'''') AS [BGColour] FROM [PackagingTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemPackagingsTbl] ON;
        INSERT INTO [ItemPackagingsTbl]
        (
            [ItemPackagingID], [ItemPackagingDesc], [AdditionalNotes], [Symbol], [Colour], [BGColour]
        )
        SELECT
            NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([Description], N'') AS [ItemPackagingDesc], NULLIF([AdditionalNotes], N'') AS [AdditionalNotes], NULLIF([Symbol], N'') AS [Symbol], NULLIF([Colour], N'') AS [Colour], NULLIF([BGColour], N'') AS [BGColour]
        FROM [PackagingTbl];
        SET IDENTITY_INSERT [ItemPackagingsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemPackagingsTbl'))
            DBCC CHECKIDENT (N'ItemPackagingsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemPackagingsTbl] from ' + N'[PackagingTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemPackagingsTbl') IS NOT NULL SET IDENTITY_INSERT [ItemPackagingsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemPackagingsTbl] from ' + N'[PackagingTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- PrepTypesTbl -> ItemPrepTypesTbl
-- Mapping: columnsCount=3
--   PrepID -> PrepID
--   PrepType -> PrepType
--   IdentifyingChar -> IdentifyingChar
IF OBJECT_ID(N'AccessSrc.PrepTypesTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemPrepTypesTbl] ([PrepID], [PrepType], [IdentifyingChar]) SELECT NULLIF([PrepID], N'''') AS [PrepID], NULLIF([PrepType], N'''') AS [PrepType], NULLIF([IdentifyingChar], N'''') AS [IdentifyingChar] FROM [AccessSrc].[PrepTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemPrepTypesTbl]
        (
            [PrepID], [PrepType], [IdentifyingChar]
        )
        SELECT
            NULLIF([PrepID], N'') AS [PrepID], NULLIF([PrepType], N'') AS [PrepType], NULLIF([IdentifyingChar], N'') AS [IdentifyingChar]
        FROM [AccessSrc].[PrepTypesTbl];
        COMMIT;
        PRINT 'OK migrate [ItemPrepTypesTbl] from ' + N'[AccessSrc].[PrepTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemPrepTypesTbl] from ' + N'[AccessSrc].[PrepTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemPrepTypesTbl] ([PrepID], [PrepType], [IdentifyingChar]) SELECT NULLIF([PrepID], N'''') AS [PrepID], NULLIF([PrepType], N'''') AS [PrepType], NULLIF([IdentifyingChar], N'''') AS [IdentifyingChar] FROM [PrepTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemPrepTypesTbl]
        (
            [PrepID], [PrepType], [IdentifyingChar]
        )
        SELECT
            NULLIF([PrepID], N'') AS [PrepID], NULLIF([PrepType], N'') AS [PrepType], NULLIF([IdentifyingChar], N'') AS [IdentifyingChar]
        FROM [PrepTypesTbl];
        COMMIT;
        PRINT 'OK migrate [ItemPrepTypesTbl] from ' + N'[PrepTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemPrepTypesTbl] from ' + N'[PrepTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ServiceTypesTbl -> ItemServiceTypesTbl
-- Mapping: columnsCount=5
--   ServiceTypeId -> ServiceTypeId
--   ServiceType -> ServiceType
--   Description -> Description
--   PackagingID -> PackagingID
--   PrepTypeID -> PrepTypeID
IF OBJECT_ID(N'AccessSrc.ServiceTypesTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemServiceTypesTbl] ([ServiceTypeId], [ServiceType], [Description], [PackagingID], [PrepTypeID]) SELECT NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([ServiceType], N'''') AS [ServiceType], NULLIF([Description], N'''') AS [Description], NULLIF([PackagingID], N'''') AS [PackagingID], NULLIF([PrepTypeID], N'''') AS [PrepTypeID] FROM [AccessSrc].[ServiceTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemServiceTypesTbl]
        (
            [ServiceTypeId], [ServiceType], [Description], [PackagingID], [PrepTypeID]
        )
        SELECT
            NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([ServiceType], N'') AS [ServiceType], NULLIF([Description], N'') AS [Description], NULLIF([PackagingID], N'') AS [PackagingID], NULLIF([PrepTypeID], N'') AS [PrepTypeID]
        FROM [AccessSrc].[ServiceTypesTbl];
        COMMIT;
        PRINT 'OK migrate [ItemServiceTypesTbl] from ' + N'[AccessSrc].[ServiceTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemServiceTypesTbl] from ' + N'[AccessSrc].[ServiceTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemServiceTypesTbl] ([ServiceTypeId], [ServiceType], [Description], [PackagingID], [PrepTypeID]) SELECT NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([ServiceType], N'''') AS [ServiceType], NULLIF([Description], N'''') AS [Description], NULLIF([PackagingID], N'''') AS [PackagingID], NULLIF([PrepTypeID], N'''') AS [PrepTypeID] FROM [ServiceTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemServiceTypesTbl]
        (
            [ServiceTypeId], [ServiceType], [Description], [PackagingID], [PrepTypeID]
        )
        SELECT
            NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([ServiceType], N'') AS [ServiceType], NULLIF([Description], N'') AS [Description], NULLIF([PackagingID], N'') AS [PackagingID], NULLIF([PrepTypeID], N'') AS [PrepTypeID]
        FROM [ServiceTypesTbl];
        COMMIT;
        PRINT 'OK migrate [ItemServiceTypesTbl] from ' + N'[ServiceTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemServiceTypesTbl] from ' + N'[ServiceTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemTypeTbl -> ItemsTbl
-- Mapping: columnsCount=13
--   ItemTypeID -> ItemTypeID
--   SKU -> SKU
--   ItemDesc -> ItemDesc
--   ItemEnabled -> ItemEnabled
--   ItemsCharacteritics -> ItemsCharacteritics
--   ItemDetail -> ItemDetail
--   ServiceTypeId -> ServiceTypeId
--   ReplacementID -> ReplacementID
--   ItemShortName -> ItemShortName
--   SortOrder -> SortOrder
--   UnitsPerQty -> UnitsPerQty
--   ItemUnitID -> ItemUnitID
--   BasePrice -> BasePrice
IF OBJECT_ID(N'AccessSrc.ItemTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemsTbl] ([ItemTypeID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ServiceTypeId], [ReplacementID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]) SELECT NULLIF([ItemTypeID], N'''') AS [ItemTypeID], NULLIF([SKU], N'''') AS [SKU], NULLIF([ItemDesc], N'''') AS [ItemDesc], NULLIF([ItemEnabled], N'''') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'''') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'''') AS [ItemDetail], NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([ReplacementID], N'''') AS [ReplacementID], NULLIF([ItemShortName], N'''') AS [ItemShortName], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([UnitsPerQty], N'''') AS [UnitsPerQty], NULLIF([ItemUnitID], N'''') AS [ItemUnitID], NULLIF([BasePrice], N'''') AS [BasePrice] FROM [AccessSrc].[ItemTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemsTbl]
        (
            [ItemTypeID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ServiceTypeId], [ReplacementID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]
        )
        SELECT
            NULLIF([ItemTypeID], N'') AS [ItemTypeID], NULLIF([SKU], N'') AS [SKU], NULLIF([ItemDesc], N'') AS [ItemDesc], NULLIF([ItemEnabled], N'') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'') AS [ItemDetail], NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([ReplacementID], N'') AS [ReplacementID], NULLIF([ItemShortName], N'') AS [ItemShortName], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([UnitsPerQty], N'') AS [UnitsPerQty], NULLIF([ItemUnitID], N'') AS [ItemUnitID], NULLIF([BasePrice], N'') AS [BasePrice]
        FROM [AccessSrc].[ItemTypeTbl];
        COMMIT;
        PRINT 'OK migrate [ItemsTbl] from ' + N'[AccessSrc].[ItemTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemsTbl] from ' + N'[AccessSrc].[ItemTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemsTbl] ([ItemTypeID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ServiceTypeId], [ReplacementID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]) SELECT NULLIF([ItemTypeID], N'''') AS [ItemTypeID], NULLIF([SKU], N'''') AS [SKU], NULLIF([ItemDesc], N'''') AS [ItemDesc], NULLIF([ItemEnabled], N'''') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'''') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'''') AS [ItemDetail], NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([ReplacementID], N'''') AS [ReplacementID], NULLIF([ItemShortName], N'''') AS [ItemShortName], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([UnitsPerQty], N'''') AS [UnitsPerQty], NULLIF([ItemUnitID], N'''') AS [ItemUnitID], NULLIF([BasePrice], N'''') AS [BasePrice] FROM [ItemTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemsTbl]
        (
            [ItemTypeID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ServiceTypeId], [ReplacementID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]
        )
        SELECT
            NULLIF([ItemTypeID], N'') AS [ItemTypeID], NULLIF([SKU], N'') AS [SKU], NULLIF([ItemDesc], N'') AS [ItemDesc], NULLIF([ItemEnabled], N'') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'') AS [ItemDetail], NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([ReplacementID], N'') AS [ReplacementID], NULLIF([ItemShortName], N'') AS [ItemShortName], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([UnitsPerQty], N'') AS [UnitsPerQty], NULLIF([ItemUnitID], N'') AS [ItemUnitID], NULLIF([BasePrice], N'') AS [BasePrice]
        FROM [ItemTypeTbl];
        COMMIT;
        PRINT 'OK migrate [ItemsTbl] from ' + N'[ItemTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemsTbl] from ' + N'[ItemTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemUnitsTbl -> ItemUnitsTbl
-- Mapping: columnsCount=3
--   ItemUnitID -> ItemUnitID
--   UnitOfMeasure -> UnitOfMeasure
--   UnitDescription -> UnitDescription
IF OBJECT_ID(N'AccessSrc.ItemUnitsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ItemUnitsTbl]: missing source [AccessSrc].[ItemUnitsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemUnitsTbl] ([ItemUnitID], [UnitOfMeasure], [UnitDescription]) SELECT NULLIF([ItemUnitID], N'''') AS [ItemUnitID], NULLIF([UnitOfMeasure], N'''') AS [UnitOfMeasure], NULLIF([UnitDescription], N'''') AS [UnitDescription] FROM [AccessSrc].[ItemUnitsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemUnitsTbl]
        (
            [ItemUnitID], [UnitOfMeasure], [UnitDescription]
        )
        SELECT
            NULLIF([ItemUnitID], N'') AS [ItemUnitID], NULLIF([UnitOfMeasure], N'') AS [UnitOfMeasure], NULLIF([UnitDescription], N'') AS [UnitDescription]
        FROM [AccessSrc].[ItemUnitsTbl];
        COMMIT;
        PRINT 'OK migrate [ItemUnitsTbl] from ' + N'[AccessSrc].[ItemUnitsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemUnitsTbl] from ' + N'[AccessSrc].[ItemUnitsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- NextRoastDateByCityTbl -> NextPrepDateByAreasTbl
-- Mapping: columnsCount=7
--   NextRoastDayID -> NextRoastDayID
--   CityID -> CityID
--   PreperationDate -> PreperationDate
--   DeliveryDate -> DeliveryDate
--   DeliveryOrder -> DeliveryOrder
--   NextPreperationDate -> NextPreperationDate
--   NextDeliveryDate -> NextDeliveryDate
IF OBJECT_ID(N'AccessSrc.NextRoastDateByCityTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [NextPrepDateByAreasTbl] ([NextRoastDayID], [CityID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPreperationDate], [NextDeliveryDate]) SELECT NULLIF([NextRoastDayID], N'''') AS [NextRoastDayID], NULLIF([CityID], N'''') AS [CityID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''))) AS [PreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''))) AS [DeliveryDate], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''))) AS [NextPreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''''), 126), TRY_CONVERT(da ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [NextPrepDateByAreasTbl]
        (
            [NextRoastDayID], [CityID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPreperationDate], [NextDeliveryDate]
        )
        SELECT
            NULLIF([NextRoastDayID], N'') AS [NextRoastDayID], NULLIF([CityID], N'') AS [CityID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''))) AS [PreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''))) AS [DeliveryDate], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''))) AS [NextPreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''))) AS [NextDeliveryDate]
        FROM [AccessSrc].[NextRoastDateByCityTbl];
        COMMIT;
        PRINT 'OK migrate [NextPrepDateByAreasTbl] from ' + N'[AccessSrc].[NextRoastDateByCityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [NextPrepDateByAreasTbl] from ' + N'[AccessSrc].[NextRoastDateByCityTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [NextPrepDateByAreasTbl] ([NextRoastDayID], [CityID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPreperationDate], [NextDeliveryDate]) SELECT NULLIF([NextRoastDayID], N'''') AS [NextRoastDayID], NULLIF([CityID], N'''') AS [CityID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''))) AS [PreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''))) AS [DeliveryDate], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''))) AS [NextPreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''''), 126), TRY_CONVERT(da ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [NextPrepDateByAreasTbl]
        (
            [NextRoastDayID], [CityID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPreperationDate], [NextDeliveryDate]
        )
        SELECT
            NULLIF([NextRoastDayID], N'') AS [NextRoastDayID], NULLIF([CityID], N'') AS [CityID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''))) AS [PreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''))) AS [DeliveryDate], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''))) AS [NextPreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''))) AS [NextDeliveryDate]
        FROM [NextRoastDateByCityTbl];
        COMMIT;
        PRINT 'OK migrate [NextPrepDateByAreasTbl] from ' + N'[NextRoastDateByCityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [NextPrepDateByAreasTbl] from ' + N'[NextRoastDateByCityTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- PaymentTermsTbl -> PaymentTermsTbl
-- Mapping: columnsCount=7
--   PaymentTermID -> PaymentTermID
--   PaymentTermDesc -> PaymentTermDesc
--   PaymentDays -> PaymentDays
--   DayOfMonth -> DayOfMonth
--   UseDays -> UseDays
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.PaymentTermsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [PaymentTermsTbl]: missing source [AccessSrc].[PaymentTermsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [PaymentTermsTbl] ([PaymentTermID], [PaymentTermDesc], [PaymentDays], [DayOfMonth], [UseDays], [Enabled], [Notes]) SELECT NULLIF([PaymentTermID], N'''') AS [PaymentTermID], NULLIF([PaymentTermDesc], N'''') AS [PaymentTermDesc], NULLIF([PaymentDays], N'''') AS [PaymentDays], NULLIF([DayOfMonth], N'''') AS [DayOfMonth], NULLIF([UseDays], N'''') AS [UseDays], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[PaymentTermsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [PaymentTermsTbl]
        (
            [PaymentTermID], [PaymentTermDesc], [PaymentDays], [DayOfMonth], [UseDays], [Enabled], [Notes]
        )
        SELECT
            NULLIF([PaymentTermID], N'') AS [PaymentTermID], NULLIF([PaymentTermDesc], N'') AS [PaymentTermDesc], NULLIF([PaymentDays], N'') AS [PaymentDays], NULLIF([DayOfMonth], N'') AS [DayOfMonth], NULLIF([UseDays], N'') AS [UseDays], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[PaymentTermsTbl];
        COMMIT;
        PRINT 'OK migrate [PaymentTermsTbl] from ' + N'[AccessSrc].[PaymentTermsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [PaymentTermsTbl] from ' + N'[AccessSrc].[PaymentTermsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- PersonsTbl -> PeopleTbl
-- Mapping: columnsCount=6
--   PersonID -> PersonID
--   Person -> Person
--   Abreviation -> Abreviation
--   Enabled -> Enabled
--   NormalDeliveryDoW -> NormalDeliveryDoW
--   SecurityUsername -> SecurityUsername
IF OBJECT_ID(N'AccessSrc.PersonsTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [PeopleTbl] ([PersonID], [Person], [Abreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]) SELECT NULLIF([PersonID], N'''') AS [PersonID], NULLIF([Person], N'''') AS [Person], NULLIF([Abreviation], N'''') AS [Abreviation], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'''') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'''') AS [SecurityUsername] FROM [AccessSrc].[PersonsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [PeopleTbl]
        (
            [PersonID], [Person], [Abreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]
        )
        SELECT
            NULLIF([PersonID], N'') AS [PersonID], NULLIF([Person], N'') AS [Person], NULLIF([Abreviation], N'') AS [Abreviation], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'') AS [SecurityUsername]
        FROM [AccessSrc].[PersonsTbl];
        COMMIT;
        PRINT 'OK migrate [PeopleTbl] from ' + N'[AccessSrc].[PersonsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [PeopleTbl] from ' + N'[AccessSrc].[PersonsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [PeopleTbl] ([PersonID], [Person], [Abreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]) SELECT NULLIF([PersonID], N'''') AS [PersonID], NULLIF([Person], N'''') AS [Person], NULLIF([Abreviation], N'''') AS [Abreviation], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'''') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'''') AS [SecurityUsername] FROM [PersonsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [PeopleTbl]
        (
            [PersonID], [Person], [Abreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]
        )
        SELECT
            NULLIF([PersonID], N'') AS [PersonID], NULLIF([Person], N'') AS [Person], NULLIF([Abreviation], N'') AS [Abreviation], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'') AS [SecurityUsername]
        FROM [PersonsTbl];
        COMMIT;
        PRINT 'OK migrate [PeopleTbl] from ' + N'[PersonsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [PeopleTbl] from ' + N'[PersonsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- PriceLevelsTbl -> PriceLevelsTbl
-- Mapping: columnsCount=5
--   PriceLevelID -> PriceLevelID
--   PriceLevelDesc -> PriceLevelDesc
--   PricingFactor -> PricingFactor
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.PriceLevelsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [PriceLevelsTbl]: missing source [AccessSrc].[PriceLevelsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [PriceLevelsTbl] ([PriceLevelID], [PriceLevelDesc], [PricingFactor], [Enabled], [Notes]) SELECT NULLIF([PriceLevelID], N'''') AS [PriceLevelID], NULLIF([PriceLevelDesc], N'''') AS [PriceLevelDesc], NULLIF([PricingFactor], N'''') AS [PricingFactor], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[PriceLevelsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [PriceLevelsTbl]
        (
            [PriceLevelID], [PriceLevelDesc], [PricingFactor], [Enabled], [Notes]
        )
        SELECT
            NULLIF([PriceLevelID], N'') AS [PriceLevelID], NULLIF([PriceLevelDesc], N'') AS [PriceLevelDesc], NULLIF([PricingFactor], N'') AS [PricingFactor], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[PriceLevelsTbl];
        COMMIT;
        PRINT 'OK migrate [PriceLevelsTbl] from ' + N'[AccessSrc].[PriceLevelsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [PriceLevelsTbl] from ' + N'[AccessSrc].[PriceLevelsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ReoccuranceTypeTbl -> RecurranceTypesTbl
-- Mapping: columnsCount=2
--   ID -> ID
--   Type -> Type
IF OBJECT_ID(N'AccessSrc.ReoccuranceTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [RecurranceTypesTbl] ([ID], [Type]) SELECT NULLIF([ID], N'''') AS [ID], NULLIF([Type], N'''') AS [Type] FROM [AccessSrc].[ReoccuranceTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [RecurranceTypesTbl]
        (
            [ID], [Type]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], NULLIF([Type], N'') AS [Type]
        FROM [AccessSrc].[ReoccuranceTypeTbl];
        COMMIT;
        PRINT 'OK migrate [RecurranceTypesTbl] from ' + N'[AccessSrc].[ReoccuranceTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [RecurranceTypesTbl] from ' + N'[AccessSrc].[ReoccuranceTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [RecurranceTypesTbl] ([ID], [Type]) SELECT NULLIF([ID], N'''') AS [ID], NULLIF([Type], N'''') AS [Type] FROM [ReoccuranceTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [RecurranceTypesTbl]
        (
            [ID], [Type]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], NULLIF([Type], N'') AS [Type]
        FROM [ReoccuranceTypeTbl];
        COMMIT;
        PRINT 'OK migrate [RecurranceTypesTbl] from ' + N'[ReoccuranceTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [RecurranceTypesTbl] from ' + N'[ReoccuranceTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- RepairFaultsTbl -> RepairFaultsTbl
-- Mapping: columnsCount=4
--   RepairFaultID -> RepairFaultID
--   RepairFaultDesc -> RepairFaultDesc
--   SortOrder -> SortOrder
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.RepairFaultsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [RepairFaultsTbl]: missing source [AccessSrc].[RepairFaultsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [RepairFaultsTbl] ([RepairFaultID], [RepairFaultDesc], [SortOrder], [Notes]) SELECT NULLIF([RepairFaultID], N'''') AS [RepairFaultID], NULLIF([RepairFaultDesc], N'''') AS [RepairFaultDesc], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[RepairFaultsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [RepairFaultsTbl]
        (
            [RepairFaultID], [RepairFaultDesc], [SortOrder], [Notes]
        )
        SELECT
            NULLIF([RepairFaultID], N'') AS [RepairFaultID], NULLIF([RepairFaultDesc], N'') AS [RepairFaultDesc], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[RepairFaultsTbl];
        COMMIT;
        PRINT 'OK migrate [RepairFaultsTbl] from ' + N'[AccessSrc].[RepairFaultsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [RepairFaultsTbl] from ' + N'[AccessSrc].[RepairFaultsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- RepairStatusesTbl -> RepairStatusesTbl
-- Mapping: columnsCount=6
--   RepairStatusID -> RepairStatusID
--   RepairStatusDesc -> RepairStatusDesc
--   EmailClient -> EmailClient
--   SortOrder -> SortOrder
--   Notes -> Notes
--   StatusNote -> StatusNote
IF OBJECT_ID(N'AccessSrc.RepairStatusesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [RepairStatusesTbl]: missing source [AccessSrc].[RepairStatusesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [RepairStatusesTbl] ([RepairStatusID], [RepairStatusDesc], [EmailClient], [SortOrder], [Notes], [StatusNote]) SELECT NULLIF([RepairStatusID], N'''') AS [RepairStatusID], NULLIF([RepairStatusDesc], N'''') AS [RepairStatusDesc], NULLIF([EmailClient], N'''') AS [EmailClient], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes], NULLIF([StatusNote], N'''') AS [StatusNote] FROM [AccessSrc].[RepairStatusesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [RepairStatusesTbl]
        (
            [RepairStatusID], [RepairStatusDesc], [EmailClient], [SortOrder], [Notes], [StatusNote]
        )
        SELECT
            NULLIF([RepairStatusID], N'') AS [RepairStatusID], NULLIF([RepairStatusDesc], N'') AS [RepairStatusDesc], NULLIF([EmailClient], N'') AS [EmailClient], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes], NULLIF([StatusNote], N'') AS [StatusNote]
        FROM [AccessSrc].[RepairStatusesTbl];
        COMMIT;
        PRINT 'OK migrate [RepairStatusesTbl] from ' + N'[AccessSrc].[RepairStatusesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [RepairStatusesTbl] from ' + N'[AccessSrc].[RepairStatusesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- RepairsTbl -> RepairsTbl
-- Mapping: columnsCount=22
--   RepairID -> RepairID
--   CustomerID -> CustomerID
--   ContactName -> ContactName
--   ContactEmail -> ContactEmail
--   JobCardNumber -> JobCardNumber
--   DateLogged -> DateLogged
--   LastStatusChange -> LastStatusChange
--   MachineTypeID -> MachineTypeID
--   MachineSerialNumber -> MachineSerialNumber
--   SwopOutMachineID -> SwopOutMachineID
--   MachineConditionID -> MachineConditionID
--   TakenFrother -> TakenFrother
--   TakenBeanLid -> TakenBeanLid
--   TakenWaterLid -> TakenWaterLid
--   BrokenFrother -> BrokenFrother
--   BrokenBeanLid -> BrokenBeanLid
--   BrokenWaterLid -> BrokenWaterLid
--   RepairFaultID -> RepairFaultID
--   RepairFaultDesc -> RepairFaultDesc
--   RepairStatusID -> RepairStatusID
--   RelatedOrderID -> RelatedOrderID
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.RepairsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [RepairsTbl]: missing source [AccessSrc].[RepairsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [RepairsTbl] ([RepairID], [CustomerID], [ContactName], [ContactEmail], [JobCardNumber], [DateLogged], [LastStatusChange], [MachineTypeID], [MachineSerialNumber], [SwopOutMachineID], [MachineConditionID], [TakenFrother], [TakenBeanLid], [TakenWaterLid], [BrokenFrother], [BrokenBeanLid], [BrokenWaterLid], [RepairFaultID], [RepairFaultDesc], [RepairStatusID], [RelatedOrderID], [Notes]) SELECT NULLIF([RepairID], N'''') AS [RepairID], NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([ContactName], N'''') AS [ContactName], NULLIF([ContactEmail], N'''') AS [ContactEmail], NULLIF([JobCardNumber], N'''') AS [JobCardNumber], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''))) AS [DateLogged], NULLIF([LastStatusChange], N'''') AS [LastStatusChange], NULLIF([MachineTypeID], N'''') AS [MachineTypeID], NULLIF([MachineSerialNumber], N'''') AS [MachineSerialNumber], NULLIF([SwopOutMachineID], N'''') AS [SwopOutMachineID], NULLIF([MachineConditionID], N'''') AS [MachineConditionID], NULLIF([TakenFrother], N'''') AS [TakenFrother], NULLIF([TakenBeanLid], N'''') AS [TakenBeanLid], NULLIF([TakenWaterLid], N'''') AS [TakenWaterLid], NULLIF([BrokenFrother], N'''') AS [BrokenFrother], NULLIF([BrokenBeanLid], N'''') AS [BrokenBeanLid], NULLIF([BrokenWaterLid], N'''') AS [Br ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [RepairsTbl]
        (
            [RepairID], [CustomerID], [ContactName], [ContactEmail], [JobCardNumber], [DateLogged], [LastStatusChange], [MachineTypeID], [MachineSerialNumber], [SwopOutMachineID], [MachineConditionID], [TakenFrother], [TakenBeanLid], [TakenWaterLid], [BrokenFrother], [BrokenBeanLid], [BrokenWaterLid], [RepairFaultID], [RepairFaultDesc], [RepairStatusID], [RelatedOrderID], [Notes]
        )
        SELECT
            NULLIF([RepairID], N'') AS [RepairID], NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([ContactName], N'') AS [ContactName], NULLIF([ContactEmail], N'') AS [ContactEmail], NULLIF([JobCardNumber], N'') AS [JobCardNumber], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''))) AS [DateLogged], NULLIF([LastStatusChange], N'') AS [LastStatusChange], NULLIF([MachineTypeID], N'') AS [MachineTypeID], NULLIF([MachineSerialNumber], N'') AS [MachineSerialNumber], NULLIF([SwopOutMachineID], N'') AS [SwopOutMachineID], NULLIF([MachineConditionID], N'') AS [MachineConditionID], NULLIF([TakenFrother], N'') AS [TakenFrother], NULLIF([TakenBeanLid], N'') AS [TakenBeanLid], NULLIF([TakenWaterLid], N'') AS [TakenWaterLid], NULLIF([BrokenFrother], N'') AS [BrokenFrother], NULLIF([BrokenBeanLid], N'') AS [BrokenBeanLid], NULLIF([BrokenWaterLid], N'') AS [BrokenWaterLid], NULLIF([RepairFaultID], N'') AS [RepairFaultID], NULLIF([RepairFaultDesc], N'') AS [RepairFaultDesc], NULLIF([RepairStatusID], N'') AS [RepairStatusID], NULLIF([RelatedOrderID], N'') AS [RelatedOrderID], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[RepairsTbl];
        COMMIT;
        PRINT 'OK migrate [RepairsTbl] from ' + N'[AccessSrc].[RepairsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [RepairsTbl] from ' + N'[AccessSrc].[RepairsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- SectionTypesTbl -> SectionTypesTbl
-- Mapping: columnsCount=3
--   SectionID -> SectionID
--   SectionType -> SectionType
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.SectionTypesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [SectionTypesTbl]: missing source [AccessSrc].[SectionTypesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [SectionTypesTbl] ([SectionID], [SectionType], [Notes]) SELECT NULLIF([SectionID], N'''') AS [SectionID], NULLIF([SectionType], N'''') AS [SectionType], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[SectionTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [SectionTypesTbl]
        (
            [SectionID], [SectionType], [Notes]
        )
        SELECT
            NULLIF([SectionID], N'') AS [SectionID], NULLIF([SectionType], N'') AS [SectionType], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[SectionTypesTbl];
        COMMIT;
        PRINT 'OK migrate [SectionTypesTbl] from ' + N'[AccessSrc].[SectionTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [SectionTypesTbl] from ' + N'[AccessSrc].[SectionTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- SendCheckEmailTextsTbl -> SendCheckupEmailTextsTbl
-- Mapping: columnsCount=6
--   SCEMTID -> SCEMTID
--   Header -> Header
--   Body -> Body
--   Footer -> Footer
--   DateLastChange -> DateLastChange
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.SendCheckEmailTextsTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [SendCheckupEmailTextsTbl] ([SCEMTID], [Header], [Body], [Footer], [DateLastChange], [Notes]) SELECT NULLIF([SCEMTID], N'''') AS [SCEMTID], NULLIF([Header], N'''') AS [Header], NULLIF([Body], N'''') AS [Body], NULLIF([Footer], N'''') AS [Footer], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''))) AS [DateLastChange], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[SendCheckEmailTextsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [SendCheckupEmailTextsTbl]
        (
            [SCEMTID], [Header], [Body], [Footer], [DateLastChange], [Notes]
        )
        SELECT
            NULLIF([SCEMTID], N'') AS [SCEMTID], NULLIF([Header], N'') AS [Header], NULLIF([Body], N'') AS [Body], NULLIF([Footer], N'') AS [Footer], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''))) AS [DateLastChange], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[SendCheckEmailTextsTbl];
        COMMIT;
        PRINT 'OK migrate [SendCheckupEmailTextsTbl] from ' + N'[AccessSrc].[SendCheckEmailTextsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [SendCheckupEmailTextsTbl] from ' + N'[AccessSrc].[SendCheckEmailTextsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [SendCheckupEmailTextsTbl] ([SCEMTID], [Header], [Body], [Footer], [DateLastChange], [Notes]) SELECT NULLIF([SCEMTID], N'''') AS [SCEMTID], NULLIF([Header], N'''') AS [Header], NULLIF([Body], N'''') AS [Body], NULLIF([Footer], N'''') AS [Footer], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''))) AS [DateLastChange], NULLIF([Notes], N'''') AS [Notes] FROM [SendCheckEmailTextsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [SendCheckupEmailTextsTbl]
        (
            [SCEMTID], [Header], [Body], [Footer], [DateLastChange], [Notes]
        )
        SELECT
            NULLIF([SCEMTID], N'') AS [SCEMTID], NULLIF([Header], N'') AS [Header], NULLIF([Body], N'') AS [Body], NULLIF([Footer], N'') AS [Footer], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''))) AS [DateLastChange], NULLIF([Notes], N'') AS [Notes]
        FROM [SendCheckEmailTextsTbl];
        COMMIT;
        PRINT 'OK migrate [SendCheckupEmailTextsTbl] from ' + N'[SendCheckEmailTextsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [SendCheckupEmailTextsTbl] from ' + N'[SendCheckEmailTextsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- SentRemindersLogTbl -> SentRemindersLogTbl
-- Mapping: columnsCount=7
--   ReminderID -> ReminderID
--   CustomerID -> CustomerID
--   DateSentReminder -> DateSentReminder
--   NextPrepDate -> NextPrepDate
--   ReminderSent -> ReminderSent
--   HadAutoFulfilItem -> HadAutoFulfilItem
--   HadReoccurItems -> HadReoccurItems
IF OBJECT_ID(N'AccessSrc.SentRemindersLogTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [SentRemindersLogTbl]: missing source [AccessSrc].[SentRemindersLogTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [SentRemindersLogTbl] ([ReminderID], [CustomerID], [DateSentReminder], [NextPrepDate], [ReminderSent], [HadAutoFulfilItem], [HadReoccurItems]) SELECT NULLIF([ReminderID], N'''') AS [ReminderID], NULLIF([CustomerID], N'''') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''))) AS [DateSentReminder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''))) AS [NextPrepDate], CASE WHEN NULLIF([ReminderSent], N'''') IS NULL THEN NULL WHEN NULLIF([ReminderSent], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([ReminderSent], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([ReminderSent], N'''')) END AS [ReminderSent], NULLIF([HadAutoFulfilItem], N'''') AS [HadAutoFulfilItem], NULLIF([HadReoccurItems], N'''') AS [HadReoccurItems] FROM [AccessSrc].[SentRemindersLogTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [SentRemindersLogTbl]
        (
            [ReminderID], [CustomerID], [DateSentReminder], [NextPrepDate], [ReminderSent], [HadAutoFulfilItem], [HadReoccurItems]
        )
        SELECT
            NULLIF([ReminderID], N'') AS [ReminderID], NULLIF([CustomerID], N'') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''))) AS [DateSentReminder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''))) AS [NextPrepDate], CASE WHEN NULLIF([ReminderSent], N'') IS NULL THEN NULL WHEN NULLIF([ReminderSent], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([ReminderSent], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([ReminderSent], N'')) END AS [ReminderSent], NULLIF([HadAutoFulfilItem], N'') AS [HadAutoFulfilItem], NULLIF([HadReoccurItems], N'') AS [HadReoccurItems]
        FROM [AccessSrc].[SentRemindersLogTbl];
        COMMIT;
        PRINT 'OK migrate [SentRemindersLogTbl] from ' + N'[AccessSrc].[SentRemindersLogTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [SentRemindersLogTbl] from ' + N'[AccessSrc].[SentRemindersLogTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- SysDataTbl -> SysDataTbl
-- Mapping: columnsCount=7
--   ID -> ID
--   LastReoccurringDate -> LastReoccurringDate
--   DoReoccuringOrders -> DoReoccuringOrders
--   DateLastPrepDateCalcd -> DateLastPrepDateCalcd
--   MinReminderDate -> MinReminderDate
--   GroupItemTypeID -> GroupItemTypeID
--   InternalCustomerIds -> InternalCustomerIds
IF OBJECT_ID(N'AccessSrc.SysDataTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [SysDataTbl]: missing source [AccessSrc].[SysDataTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [SysDataTbl] ([ID], [LastReoccurringDate], [DoReoccuringOrders], [DateLastPrepDateCalcd], [MinReminderDate], [GroupItemTypeID], [InternalCustomerIds]) SELECT NULLIF([ID], N'''') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''))) AS [LastReoccurringDate], NULLIF([DoReoccuringOrders], N'''') AS [DoReoccuringOrders], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''))) AS [DateLastPrepDateCalcd], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''''))) AS [MinReminderDate], NULLIF([GroupItemTypeID], N'''') AS [GroupItemTypeID], NULLIF([InternalCustomerIds], N'''') AS [InternalCustomerIds] FROM [AccessSrc].[SysDataTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [SysDataTbl]
        (
            [ID], [LastReoccurringDate], [DoReoccuringOrders], [DateLastPrepDateCalcd], [MinReminderDate], [GroupItemTypeID], [InternalCustomerIds]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''))) AS [LastReoccurringDate], NULLIF([DoReoccuringOrders], N'') AS [DoReoccuringOrders], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''))) AS [DateLastPrepDateCalcd], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''))) AS [MinReminderDate], NULLIF([GroupItemTypeID], N'') AS [GroupItemTypeID], NULLIF([InternalCustomerIds], N'') AS [InternalCustomerIds]
        FROM [AccessSrc].[SysDataTbl];
        COMMIT;
        PRINT 'OK migrate [SysDataTbl] from ' + N'[AccessSrc].[SysDataTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [SysDataTbl] from ' + N'[AccessSrc].[SysDataTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TempCoffeecheckupCustomerTbl -> TempCoffeecheckupCustomerTbl
-- Mapping: columnsCount=25
--   TCCID -> TCCID
--   CustomerID -> CustomerID
--   CompanyName -> CompanyName
--   ContactFirstName -> ContactFirstName
--   ContactAltFirstName -> ContactAltFirstName
--   CityID -> CityID
--   EmailAddress -> EmailAddress
--   AltEmailAddress -> AltEmailAddress
--   CustomerTypeID -> CustomerTypeID
--   EquipTypeID -> EquipTypeID
--   TypicallySecToo -> TypicallySecToo
--   PreferedAgentID -> PreferedAgentID
--   SalesAgentID -> SalesAgentID
--   UsesFilter -> UsesFilter
--   enabled -> enabled
--   AlwaysSendChkUp -> AlwaysSendChkUp
--   ReminderCount -> ReminderCount
--   NextPrepDate -> NextPrepDate
--   NextDeliveryDate -> NextDeliveryDate
--   NextCoffee -> NextCoffee
--   NextClean -> NextClean
--   NextFilter -> NextFilter
--   NextDescal -> NextDescal
--   NextService -> NextService
--   RequiresPurchOrder -> RequiresPurchOrder
IF OBJECT_ID(N'AccessSrc.TempCoffeecheckupCustomerTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TempCoffeecheckupCustomerTbl]: missing source [AccessSrc].[TempCoffeecheckupCustomerTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TempCoffeecheckupCustomerTbl] ([TCCID], [CustomerID], [CompanyName], [ContactFirstName], [ContactAltFirstName], [CityID], [EmailAddress], [AltEmailAddress], [CustomerTypeID], [EquipTypeID], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [UsesFilter], [enabled], [AlwaysSendChkUp], [ReminderCount], [NextPrepDate], [NextDeliveryDate], [NextCoffee], [NextClean], [NextFilter], [NextDescal], [NextService], [RequiresPurchOrder]) SELECT NULLIF([TCCID], N'''') AS [TCCID], NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([CompanyName], N'''') AS [CompanyName], NULLIF([ContactFirstName], N'''') AS [ContactFirstName], NULLIF([ContactAltFirstName], N'''') AS [ContactAltFirstName], NULLIF([CityID], N'''') AS [CityID], NULLIF([EmailAddress], N'''') AS [EmailAddress], NULLIF([AltEmailAddress], N'''') AS [AltEmailAddress], NULLIF([CustomerTypeID], N'''') AS [CustomerTypeID], NULLIF([EquipTypeID], N'''') AS [EquipTypeID], CASE WHEN NULLIF([TypicallySecToo], N'''') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([TypicallySecToo], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'''')) END AS [TypicallySecToo], NULLIF([PreferedAgentID], N'''') AS [PreferedAgentID], NULLIF([SalesAgentID], N'''') AS [SalesAgentID], CASE WHEN NULLIF([UsesFilter], N'''') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TempCoffeecheckupCustomerTbl]
        (
            [TCCID], [CustomerID], [CompanyName], [ContactFirstName], [ContactAltFirstName], [CityID], [EmailAddress], [AltEmailAddress], [CustomerTypeID], [EquipTypeID], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [UsesFilter], [enabled], [AlwaysSendChkUp], [ReminderCount], [NextPrepDate], [NextDeliveryDate], [NextCoffee], [NextClean], [NextFilter], [NextDescal], [NextService], [RequiresPurchOrder]
        )
        SELECT
            NULLIF([TCCID], N'') AS [TCCID], NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([CompanyName], N'') AS [CompanyName], NULLIF([ContactFirstName], N'') AS [ContactFirstName], NULLIF([ContactAltFirstName], N'') AS [ContactAltFirstName], NULLIF([CityID], N'') AS [CityID], NULLIF([EmailAddress], N'') AS [EmailAddress], NULLIF([AltEmailAddress], N'') AS [AltEmailAddress], NULLIF([CustomerTypeID], N'') AS [CustomerTypeID], NULLIF([EquipTypeID], N'') AS [EquipTypeID], CASE WHEN NULLIF([TypicallySecToo], N'') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TypicallySecToo], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'')) END AS [TypicallySecToo], NULLIF([PreferedAgentID], N'') AS [PreferedAgentID], NULLIF([SalesAgentID], N'') AS [SalesAgentID], CASE WHEN NULLIF([UsesFilter], N'') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UsesFilter], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'')) END AS [UsesFilter], CASE WHEN NULLIF([enabled], N'') IS NULL THEN NULL WHEN NULLIF([enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([enabled], N'')) END AS [enabled], NULLIF([AlwaysSendChkUp], N'') AS [AlwaysSendChkUp], NULLIF([ReminderCount], N'') AS [ReminderCount], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''))) AS [NextPrepDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''))) AS [NextDeliveryDate], NULLIF([NextCoffee], N'') AS [NextCoffee], NULLIF([NextClean], N'') AS [NextClean], NULLIF([NextFilter], N'') AS [NextFilter], NULLIF([NextDescal], N'') AS [NextDescal], NULLIF([NextService], N'') AS [NextService], NULLIF([RequiresPurchOrder], N'') AS [RequiresPurchOrder]
        FROM [AccessSrc].[TempCoffeecheckupCustomerTbl];
        COMMIT;
        PRINT 'OK migrate [TempCoffeecheckupCustomerTbl] from ' + N'[AccessSrc].[TempCoffeecheckupCustomerTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TempCoffeecheckupCustomerTbl] from ' + N'[AccessSrc].[TempCoffeecheckupCustomerTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TempCoffeecheckupItemsTbl -> TempCoffeecheckupItemsTbl
-- Mapping: columnsCount=9
--   TCIID -> TCIID
--   CustomerID -> CustomerID
--   ItemID -> ItemID
--   ItemQty -> ItemQty
--   ItemPrepID -> ItemPrepID
--   ItemPackagID -> ItemPackagID
--   AutoFulfill -> AutoFulfill
--   NextDateRequired -> NextDateRequired
--   ReoccurOrderID -> ReoccurOrderID
IF OBJECT_ID(N'AccessSrc.TempCoffeecheckupItemsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TempCoffeecheckupItemsTbl]: missing source [AccessSrc].[TempCoffeecheckupItemsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TempCoffeecheckupItemsTbl] ([TCIID], [CustomerID], [ItemID], [ItemQty], [ItemPrepID], [ItemPackagID], [AutoFulfill], [NextDateRequired], [ReoccurOrderID]) SELECT NULLIF([TCIID], N'''') AS [TCIID], NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([ItemID], N'''') AS [ItemID], NULLIF([ItemQty], N'''') AS [ItemQty], NULLIF([ItemPrepID], N'''') AS [ItemPrepID], NULLIF([ItemPackagID], N'''') AS [ItemPackagID], CASE WHEN NULLIF([AutoFulfill], N'''') IS NULL THEN NULL WHEN NULLIF([AutoFulfill], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([AutoFulfill], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([AutoFulfill], N'''')) END AS [AutoFulfill], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''))) AS [NextDateRequired], NULLIF([ReoccurOrderID], N'''') AS [ReoccurOrderID] FROM [AccessSrc].[TempCoffeecheckupItemsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TempCoffeecheckupItemsTbl]
        (
            [TCIID], [CustomerID], [ItemID], [ItemQty], [ItemPrepID], [ItemPackagID], [AutoFulfill], [NextDateRequired], [ReoccurOrderID]
        )
        SELECT
            NULLIF([TCIID], N'') AS [TCIID], NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([ItemID], N'') AS [ItemID], NULLIF([ItemQty], N'') AS [ItemQty], NULLIF([ItemPrepID], N'') AS [ItemPrepID], NULLIF([ItemPackagID], N'') AS [ItemPackagID], CASE WHEN NULLIF([AutoFulfill], N'') IS NULL THEN NULL WHEN NULLIF([AutoFulfill], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([AutoFulfill], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([AutoFulfill], N'')) END AS [AutoFulfill], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''))) AS [NextDateRequired], NULLIF([ReoccurOrderID], N'') AS [ReoccurOrderID]
        FROM [AccessSrc].[TempCoffeecheckupItemsTbl];
        COMMIT;
        PRINT 'OK migrate [TempCoffeecheckupItemsTbl] from ' + N'[AccessSrc].[TempCoffeecheckupItemsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TempCoffeecheckupItemsTbl] from ' + N'[AccessSrc].[TempCoffeecheckupItemsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TempOrdersHeaderTbl -> TempOrdersHeaderTbl
-- Mapping: columnsCount=9
--   TOHeaderID -> TOHeaderID
--   CustomerID -> CustomerID
--   OrderDate -> OrderDate
--   RoastDate -> RoastDate
--   RequiredByDate -> RequiredByDate
--   ToBeDeliveredByID -> ToBeDeliveredByID
--   Confirmed -> Confirmed
--   Done -> Done
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.TempOrdersHeaderTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TempOrdersHeaderTbl]: missing source [AccessSrc].[TempOrdersHeaderTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TempOrdersHeaderTbl] ([TOHeaderID], [CustomerID], [OrderDate], [RoastDate], [RequiredByDate], [ToBeDeliveredByID], [Confirmed], [Done], [Notes]) SELECT NULLIF([TOHeaderID], N'''') AS [TOHeaderID], NULLIF([CustomerID], N'''') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''))) AS [OrderDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''))) AS [RoastDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''))) AS [RequiredByDate], NULLIF([ToBeDeliveredByID], N'''') AS [ToBeDeliveredByID], CASE WHEN NULLIF([Confirmed], N'''') IS NULL THEN NULL WHEN NULLIF([Confirmed], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Confirmed], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TempOrdersHeaderTbl]
        (
            [TOHeaderID], [CustomerID], [OrderDate], [RoastDate], [RequiredByDate], [ToBeDeliveredByID], [Confirmed], [Done], [Notes]
        )
        SELECT
            NULLIF([TOHeaderID], N'') AS [TOHeaderID], NULLIF([CustomerID], N'') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''))) AS [OrderDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''))) AS [RoastDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''))) AS [RequiredByDate], NULLIF([ToBeDeliveredByID], N'') AS [ToBeDeliveredByID], CASE WHEN NULLIF([Confirmed], N'') IS NULL THEN NULL WHEN NULLIF([Confirmed], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Confirmed], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Confirmed], N'')) END AS [Confirmed], CASE WHEN NULLIF([Done], N'') IS NULL THEN NULL WHEN NULLIF([Done], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Done], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Done], N'')) END AS [Done], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TempOrdersHeaderTbl];
        COMMIT;
        PRINT 'OK migrate [TempOrdersHeaderTbl] from ' + N'[AccessSrc].[TempOrdersHeaderTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TempOrdersHeaderTbl] from ' + N'[AccessSrc].[TempOrdersHeaderTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TempOrdersLinesTbl -> TempOrdersLinesTbl
-- Mapping: columnsCount=7
--   TOLineID -> TOLineID
--   TOHeaderID -> TOHeaderID
--   ItemID -> ItemID
--   ServiceTypeID -> ServiceTypeID
--   Qty -> Qty
--   PackagingID -> PackagingID
--   OriginalOrderID -> OriginalOrderID
IF OBJECT_ID(N'AccessSrc.TempOrdersLinesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TempOrdersLinesTbl]: missing source [AccessSrc].[TempOrdersLinesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TempOrdersLinesTbl] ([TOLineID], [TOHeaderID], [ItemID], [ServiceTypeID], [Qty], [PackagingID], [OriginalOrderID]) SELECT NULLIF([TOLineID], N'''') AS [TOLineID], NULLIF([TOHeaderID], N'''') AS [TOHeaderID], NULLIF([ItemID], N'''') AS [ItemID], NULLIF([ServiceTypeID], N'''') AS [ServiceTypeID], NULLIF([Qty], N'''') AS [Qty], NULLIF([PackagingID], N'''') AS [PackagingID], NULLIF([OriginalOrderID], N'''') AS [OriginalOrderID] FROM [AccessSrc].[TempOrdersLinesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TempOrdersLinesTbl]
        (
            [TOLineID], [TOHeaderID], [ItemID], [ServiceTypeID], [Qty], [PackagingID], [OriginalOrderID]
        )
        SELECT
            NULLIF([TOLineID], N'') AS [TOLineID], NULLIF([TOHeaderID], N'') AS [TOHeaderID], NULLIF([ItemID], N'') AS [ItemID], NULLIF([ServiceTypeID], N'') AS [ServiceTypeID], NULLIF([Qty], N'') AS [Qty], NULLIF([PackagingID], N'') AS [PackagingID], NULLIF([OriginalOrderID], N'') AS [OriginalOrderID]
        FROM [AccessSrc].[TempOrdersLinesTbl];
        COMMIT;
        PRINT 'OK migrate [TempOrdersLinesTbl] from ' + N'[AccessSrc].[TempOrdersLinesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TempOrdersLinesTbl] from ' + N'[AccessSrc].[TempOrdersLinesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TempOrdersTbl -> TempOrdersTbl
-- Mapping: columnsCount=13
--   TempOrderId -> TempOrderId
--   OrderID -> OrderID
--   CustomerId -> CustomerId
--   OrderDate -> OrderDate
--   RoastDate -> RoastDate
--   ItemTypeID -> ItemTypeID
--   ServiceTypeId -> ServiceTypeId
--   PrepTypeID -> PrepTypeID
--   PackagingId -> PackagingId
--   QuantityOrdered -> QuantityOrdered
--   RequiredByDate -> RequiredByDate
--   Delivered -> Delivered
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.TempOrdersTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TempOrdersTbl]: missing source [AccessSrc].[TempOrdersTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TempOrdersTbl] ([TempOrderId], [OrderID], [CustomerId], [OrderDate], [RoastDate], [ItemTypeID], [ServiceTypeId], [PrepTypeID], [PackagingId], [QuantityOrdered], [RequiredByDate], [Delivered], [Notes]) SELECT NULLIF([TempOrderId], N'''') AS [TempOrderId], NULLIF([OrderID], N'''') AS [OrderID], NULLIF([CustomerId], N'''') AS [CustomerId], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''))) AS [OrderDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''))) AS [RoastDate], NULLIF([ItemTypeID], N'''') AS [ItemTypeID], NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([PrepTypeID], N'''') AS [PrepTypeID], NULLIF([PackagingId], N'''') AS [PackagingId], NULLIF([QuantityOrdered], N'''') AS [QuantityOrdered], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Requir ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TempOrdersTbl]
        (
            [TempOrderId], [OrderID], [CustomerId], [OrderDate], [RoastDate], [ItemTypeID], [ServiceTypeId], [PrepTypeID], [PackagingId], [QuantityOrdered], [RequiredByDate], [Delivered], [Notes]
        )
        SELECT
            NULLIF([TempOrderId], N'') AS [TempOrderId], NULLIF([OrderID], N'') AS [OrderID], NULLIF([CustomerId], N'') AS [CustomerId], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''))) AS [OrderDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''))) AS [RoastDate], NULLIF([ItemTypeID], N'') AS [ItemTypeID], NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([PrepTypeID], N'') AS [PrepTypeID], NULLIF([PackagingId], N'') AS [PackagingId], NULLIF([QuantityOrdered], N'') AS [QuantityOrdered], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''))) AS [RequiredByDate], NULLIF([Delivered], N'') AS [Delivered], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TempOrdersTbl];
        COMMIT;
        PRINT 'OK migrate [TempOrdersTbl] from ' + N'[AccessSrc].[TempOrdersTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TempOrdersTbl] from ' + N'[AccessSrc].[TempOrdersTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TotalCountTrackerTbl -> TotalCountTrackerTbl
-- Mapping: columnsCount=4
--   ID -> ID
--   CountDate -> CountDate
--   TotalCount -> TotalCount
--   Comments -> Comments
IF OBJECT_ID(N'AccessSrc.TotalCountTrackerTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TotalCountTrackerTbl]: missing source [AccessSrc].[TotalCountTrackerTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TotalCountTrackerTbl] ([ID], [CountDate], [TotalCount], [Comments]) SELECT NULLIF([ID], N'''') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''))) AS [CountDate], NULLIF([TotalCount], N'''') AS [TotalCount], NULLIF([Comments], N'''') AS [Comments] FROM [AccessSrc].[TotalCountTrackerTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TotalCountTrackerTbl]
        (
            [ID], [CountDate], [TotalCount], [Comments]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''))) AS [CountDate], NULLIF([TotalCount], N'') AS [TotalCount], NULLIF([Comments], N'') AS [Comments]
        FROM [AccessSrc].[TotalCountTrackerTbl];
        COMMIT;
        PRINT 'OK migrate [TotalCountTrackerTbl] from ' + N'[AccessSrc].[TotalCountTrackerTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TotalCountTrackerTbl] from ' + N'[AccessSrc].[TotalCountTrackerTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TrackedServiceItemTbl -> TrackedServiceItemsTbl
-- Mapping: columnsCount=7
--   TrackerServiceItemID -> TrackerServiceItemID
--   ServiceTypeID -> ServiceTypeID
--   TypicalAvePerItem -> TypicalAvePerItem
--   UsageDateFieldName -> UsageDateFieldName
--   UsageAveFieldName -> UsageAveFieldName
--   ThisItemSetsDailyAverage -> ThisItemSetsDailyAverage
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.TrackedServiceItemTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TrackedServiceItemsTbl] ([TrackerServiceItemID], [ServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]) SELECT NULLIF([TrackerServiceItemID], N'''') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'''') AS [ServiceTypeID], NULLIF([TypicalAvePerItem], N'''') AS [TypicalAvePerItem], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''))) AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'''') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'''') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[TrackedServiceItemTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TrackedServiceItemsTbl]
        (
            [TrackerServiceItemID], [ServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]
        )
        SELECT
            NULLIF([TrackerServiceItemID], N'') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'') AS [ServiceTypeID], NULLIF([TypicalAvePerItem], N'') AS [TypicalAvePerItem], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''))) AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TrackedServiceItemTbl];
        COMMIT;
        PRINT 'OK migrate [TrackedServiceItemsTbl] from ' + N'[AccessSrc].[TrackedServiceItemTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TrackedServiceItemsTbl] from ' + N'[AccessSrc].[TrackedServiceItemTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TrackedServiceItemsTbl] ([TrackerServiceItemID], [ServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]) SELECT NULLIF([TrackerServiceItemID], N'''') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'''') AS [ServiceTypeID], NULLIF([TypicalAvePerItem], N'''') AS [TypicalAvePerItem], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''))) AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'''') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'''') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'''') AS [Notes] FROM [TrackedServiceItemTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TrackedServiceItemsTbl]
        (
            [TrackerServiceItemID], [ServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]
        )
        SELECT
            NULLIF([TrackerServiceItemID], N'') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'') AS [ServiceTypeID], NULLIF([TypicalAvePerItem], N'') AS [TypicalAvePerItem], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''))) AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'') AS [Notes]
        FROM [TrackedServiceItemTbl];
        COMMIT;
        PRINT 'OK migrate [TrackedServiceItemsTbl] from ' + N'[TrackedServiceItemTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TrackedServiceItemsTbl] from ' + N'[TrackedServiceItemTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TransactionTypesTbl -> TransactionTypesTbl
-- Mapping: columnsCount=3
--   TransactionID -> TransactionID
--   TransactionType -> TransactionType
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.TransactionTypesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TransactionTypesTbl]: missing source [AccessSrc].[TransactionTypesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TransactionTypesTbl] ([TransactionID], [TransactionType], [Notes]) SELECT NULLIF([TransactionID], N'''') AS [TransactionID], NULLIF([TransactionType], N'''') AS [TransactionType], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[TransactionTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TransactionTypesTbl]
        (
            [TransactionID], [TransactionType], [Notes]
        )
        SELECT
            NULLIF([TransactionID], N'') AS [TransactionID], NULLIF([TransactionType], N'') AS [TransactionType], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TransactionTypesTbl];
        COMMIT;
        PRINT 'OK migrate [TransactionTypesTbl] from ' + N'[AccessSrc].[TransactionTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TransactionTypesTbl] from ' + N'[AccessSrc].[TransactionTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- UsedItemGroupTbl -> UsedItemGroupsTbl
-- Mapping: columnsCount=7
--   UsedItemGroupID -> UsedItemGroupID
--   ContactID -> ContactID
--   GroupItemTypeID -> GroupItemTypeID
--   LastItemTypeID -> LastItemTypeID
--   LastItemTypeSortPos -> LastItemTypeSortPos
--   LastItemDateChanged -> LastItemDateChanged
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.UsedItemGroupTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [UsedItemGroupsTbl] ([UsedItemGroupID], [ContactID], [GroupItemTypeID], [LastItemTypeID], [LastItemTypeSortPos], [LastItemDateChanged], [Notes]) SELECT NULLIF([UsedItemGroupID], N'''') AS [UsedItemGroupID], NULLIF([ContactID], N'''') AS [ContactID], NULLIF([GroupItemTypeID], N'''') AS [GroupItemTypeID], NULLIF([LastItemTypeID], N'''') AS [LastItemTypeID], NULLIF([LastItemTypeSortPos], N'''') AS [LastItemTypeSortPos], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''))) AS [LastItemDateChanged], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[UsedItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [UsedItemGroupsTbl] ON;
        INSERT INTO [UsedItemGroupsTbl]
        (
            [UsedItemGroupID], [ContactID], [GroupItemTypeID], [LastItemTypeID], [LastItemTypeSortPos], [LastItemDateChanged], [Notes]
        )
        SELECT
            NULLIF([UsedItemGroupID], N'') AS [UsedItemGroupID], NULLIF([ContactID], N'') AS [ContactID], NULLIF([GroupItemTypeID], N'') AS [GroupItemTypeID], NULLIF([LastItemTypeID], N'') AS [LastItemTypeID], NULLIF([LastItemTypeSortPos], N'') AS [LastItemTypeSortPos], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''))) AS [LastItemDateChanged], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[UsedItemGroupTbl];
        SET IDENTITY_INSERT [UsedItemGroupsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'UsedItemGroupsTbl'))
            DBCC CHECKIDENT (N'UsedItemGroupsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [UsedItemGroupsTbl] from ' + N'[AccessSrc].[UsedItemGroupTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'UsedItemGroupsTbl') IS NOT NULL SET IDENTITY_INSERT [UsedItemGroupsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [UsedItemGroupsTbl] from ' + N'[AccessSrc].[UsedItemGroupTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [UsedItemGroupsTbl] ([UsedItemGroupID], [ContactID], [GroupItemTypeID], [LastItemTypeID], [LastItemTypeSortPos], [LastItemDateChanged], [Notes]) SELECT NULLIF([UsedItemGroupID], N'''') AS [UsedItemGroupID], NULLIF([ContactID], N'''') AS [ContactID], NULLIF([GroupItemTypeID], N'''') AS [GroupItemTypeID], NULLIF([LastItemTypeID], N'''') AS [LastItemTypeID], NULLIF([LastItemTypeSortPos], N'''') AS [LastItemTypeSortPos], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''))) AS [LastItemDateChanged], NULLIF([Notes], N'''') AS [Notes] FROM [UsedItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [UsedItemGroupsTbl] ON;
        INSERT INTO [UsedItemGroupsTbl]
        (
            [UsedItemGroupID], [ContactID], [GroupItemTypeID], [LastItemTypeID], [LastItemTypeSortPos], [LastItemDateChanged], [Notes]
        )
        SELECT
            NULLIF([UsedItemGroupID], N'') AS [UsedItemGroupID], NULLIF([ContactID], N'') AS [ContactID], NULLIF([GroupItemTypeID], N'') AS [GroupItemTypeID], NULLIF([LastItemTypeID], N'') AS [LastItemTypeID], NULLIF([LastItemTypeSortPos], N'') AS [LastItemTypeSortPos], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''))) AS [LastItemDateChanged], NULLIF([Notes], N'') AS [Notes]
        FROM [UsedItemGroupTbl];
        SET IDENTITY_INSERT [UsedItemGroupsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'UsedItemGroupsTbl'))
            DBCC CHECKIDENT (N'UsedItemGroupsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [UsedItemGroupsTbl] from ' + N'[UsedItemGroupTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'UsedItemGroupsTbl') IS NOT NULL SET IDENTITY_INSERT [UsedItemGroupsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [UsedItemGroupsTbl] from ' + N'[UsedItemGroupTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

PRINT 'Identities present:';
SELECT t.name AS TableName, c.name AS IdentityColumn
FROM sys.identity_columns ic
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
JOIN sys.tables t ON t.object_id=c.object_id;
GO
SELECT COUNT(*) AS ForeignKeysPresent FROM sys.foreign_keys;
GO
-- Re-enable foreign keys on migrated targets
DECLARE @tbl2 sysname, @fk2 sysname, @sql2 nvarchar(max);
DECLARE fk_cur2 CURSOR LOCAL FAST_FORWARD FOR
SELECT QUOTENAME(SCHEMA_NAME(t.schema_id))+'.'+QUOTENAME(t.name), QUOTENAME(fk.name)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id=fk.parent_object_id
WHERE t.name IN (N'AreaPrepDaysTbl', N'AreasTbl', N'AwayReasonTbl', N'ClosureDatesTbl', N'ContactsAccInfoTbl', N'ContactsAwayPeriodTbl', N'ContactsItemUsageTbl', N'ContactsTbl', N'ContactsUsageTbl', N'ContactTrackedServiceItemsTbl', N'ContactTypesTbl', N'ContactUsageLinesTbl', N'EquipConditionsTbl', N'EquipTypesTbl', N'HolidayClosuresTbl', N'InvoiceTypesTbl', N'ItemGroupsTbl', N'ItemPackagingsTbl', N'ItemPrepTypesTbl', N'ItemServiceTypesTbl', N'ItemsTbl', N'ItemUnitsTbl', N'NextPrepDateByAreasTbl', N'PaymentTermsTbl', N'PeopleTbl', N'PriceLevelsTbl', N'RecurranceTypesTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'RepairsTbl', N'SectionTypesTbl', N'SendCheckupEmailTextsTbl', N'SentRemindersLogTbl', N'SysDataTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersLinesTbl', N'TempOrdersTbl', N'TotalCountTrackerTbl', N'TrackedServiceItemsTbl', N'TransactionTypesTbl', N'UsedItemGroupsTbl');
OPEN fk_cur2;
FETCH NEXT FROM fk_cur2 INTO @tbl2, @fk2;
WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        SET @sql2 = N'ALTER TABLE ' + @tbl2 + N' WITH CHECK CHECK CONSTRAINT ' + @fk2 + N';';
        PRINT @sql2; EXEC sp_executesql @sql2;
    END TRY
    BEGIN CATCH
        PRINT 'WARN: could not CHECK ' + @fk2 + ' on ' + @tbl2 + ': ' + ERROR_MESSAGE();
        SET @sql2 = N'ALTER TABLE ' + @tbl2 + N' WITH NOCHECK CHECK CONSTRAINT ' + @fk2 + N';';
        PRINT @sql2; EXEC sp_executesql @sql2;
    END CATCH
    FETCH NEXT FROM fk_cur2 INTO @tbl2, @fk2;
END
CLOSE fk_cur2; DEALLOCATE fk_cur2;
GO

-- Orphan check for foreign keys that could not be fully trusted after reload
DECLARE @ps sysname, @pt sysname, @rs sysname, @rt sysname, @fkn sysname, @fkId int, @sql nvarchar(max), @pred nvarchar(max);
DECLARE fk_orphans CURSOR LOCAL FAST_FORWARD FOR
SELECT SCHEMA_NAME(tp.schema_id), tp.name, SCHEMA_NAME(tr.schema_id), tr.name, fk.name, fk.object_id
FROM sys.foreign_keys fk
JOIN sys.tables tp ON tp.object_id = fk.parent_object_id
JOIN sys.tables tr ON tr.object_id = fk.referenced_object_id
WHERE tp.name IN (N'AreaPrepDaysTbl', N'AreasTbl', N'AwayReasonTbl', N'ClosureDatesTbl', N'ContactsAccInfoTbl', N'ContactsAwayPeriodTbl', N'ContactsItemUsageTbl', N'ContactsTbl', N'ContactsUsageTbl', N'ContactTrackedServiceItemsTbl', N'ContactTypesTbl', N'ContactUsageLinesTbl', N'EquipConditionsTbl', N'EquipTypesTbl', N'HolidayClosuresTbl', N'InvoiceTypesTbl', N'ItemGroupsTbl', N'ItemPackagingsTbl', N'ItemPrepTypesTbl', N'ItemServiceTypesTbl', N'ItemsTbl', N'ItemUnitsTbl', N'NextPrepDateByAreasTbl', N'PaymentTermsTbl', N'PeopleTbl', N'PriceLevelsTbl', N'RecurranceTypesTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'RepairsTbl', N'SectionTypesTbl', N'SendCheckupEmailTextsTbl', N'SentRemindersLogTbl', N'SysDataTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersLinesTbl', N'TempOrdersTbl', N'TotalCountTrackerTbl', N'TrackedServiceItemsTbl', N'TransactionTypesTbl', N'UsedItemGroupsTbl') AND (fk.is_not_trusted = 1 OR fk.is_disabled = 1);
OPEN fk_orphans;
FETCH NEXT FROM fk_orphans INTO @ps, @pt, @rs, @rt, @fkn, @fkId;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Build multi-column join predicate for this FK
    SELECT @pred = STUFF((
        SELECT ' AND t.' + QUOTENAME(pc.name) + ' = p.' + QUOTENAME(rc.name)
        FROM sys.foreign_key_columns fkc
        JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
        JOIN sys.columns rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
        WHERE fkc.constraint_object_id = @fkId
        ORDER BY fkc.constraint_column_id
        FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 5, '');
    DECLARE @pt2 nvarchar(300) = QUOTENAME(@ps) + N'.' + QUOTENAME(@pt);
    DECLARE @rt2 nvarchar(300) = QUOTENAME(@rs) + N'.' + QUOTENAME(@rt);
    SET @sql = N'PRINT ''ORPHAN CHECK [' + REPLACE(@fkn, '''', '''''') + N'] on ' + @pt2 + N' -> ' + @rt2 + N'''; ' +
              N'SELECT COUNT(*) AS OrphanCount FROM ' + @pt2 + N' t WHERE NOT EXISTS (SELECT 1 FROM ' + @rt2 + N' p WHERE ' + @pred + N'); ' +
              N'SELECT TOP 5 t.* FROM ' + @pt2 + N' t WHERE NOT EXISTS (SELECT 1 FROM ' + @rt2 + N' p WHERE ' + @pred + N') ORDER BY NEWID();';
    EXEC sp_executesql @sql;
    FETCH NEXT FROM fk_orphans INTO @ps, @pt, @rs, @rt, @fkn, @fkId;
END
CLOSE fk_orphans; DEALLOCATE fk_orphans;
GO

