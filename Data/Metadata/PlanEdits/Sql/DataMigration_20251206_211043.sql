-- Auto-generated DATA MIGRATION script
-- Assumes source data is available under schema [AccessSrc] using Access source table names.
-- If you do not have [AccessSrc] objects, the generator will fall back to unqualified [Source] when Target != Source.
-- If Target == Source and AccessSrc.Source is missing, the table is skipped (no self-select).
-- Create schema once if needed: IF SCHEMA_ID('AccessSrc') IS NULL EXEC('CREATE SCHEMA AccessSrc');
-- AccessSchema: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema
-- PlanConstraints: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\PlanConstraints.json
-- Tables to migrate (ordered): 43
--   - AwayReasonTbl
--   - CityPrepDaysTbl
--   - CityTbl
--   - ClientAwayPeriodTbl
--   - ClientUsageLinesTbl
--   - ClientUsageTbl
--   - ClosureDatesTbl
--   - CustomersAccInfoTbl
--   - CustomersTbl
--   - CustomerTrackedServiceItemsTbl
--   - CustomerTypeTbl
--   - EquipTypeTbl
--   - HolidayClosureTbl
--   - InvoiceTypeTbl
--   - ItemGroupTbl
--   - ItemPackagingsTbl
--   - ItemTypeTbl
--   - ItemUnitsTbl
--   - ItemUsageTbl
--   - MachineConditionsTbl
--   - NextRoastDateByCityTbl
--   - PaymentTermsTbl
--   - PersonsTbl
--   - PrepTypesTbl
--   - PriceLevelsTbl
--   - ReoccuranceTypeTbl
--   - RepairFaultsTbl
--   - RepairStatusesTbl
--   - RepairsTbl
--   - SectionTypesTbl
--   - SendCheckEmailTextsTbl
--   - SentRemindersLogTbl
--   - ServiceTypesTbl
--   - SysDataTbl
--   - TempCoffeecheckupCustomerTbl
--   - TempCoffeecheckupItemsTbl
--   - TempOrdersHeaderTbl
--   - TempOrdersLinesTbl
--   - TempOrdersTbl
--   - TotalCountTrackerTbl
--   - TrackedServiceItemTbl
--   - TransactionTypesTbl
--   - UsedItemGroupTbl
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Disable foreign keys on migrated targets
DECLARE @tbl sysname, @fk sysname, @sql nvarchar(max);
DECLARE fk_cur CURSOR LOCAL FAST_FORWARD FOR
SELECT QUOTENAME(SCHEMA_NAME(t.schema_id))+'.'+QUOTENAME(t.name), QUOTENAME(fk.name)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id=fk.parent_object_id
WHERE t.name IN (N'AwayReasonTbl', N'CityPrepDaysTbl', N'CityTbl', N'ClientAwayPeriodTbl', N'ClientUsageLinesTbl', N'ClientUsageTbl', N'ClosureDatesTbl', N'CustomersAccInfoTbl', N'CustomersTbl', N'CustomerTrackedServiceItemsTbl', N'CustomerTypeTbl', N'EquipTypeTbl', N'HolidayClosureTbl', N'InvoiceTypeTbl', N'ItemGroupTbl', N'ItemPackagingsTbl', N'ItemTypeTbl', N'ItemUnitsTbl', N'ItemUsageTbl', N'MachineConditionsTbl', N'NextRoastDateByCityTbl', N'PaymentTermsTbl', N'PersonsTbl', N'PrepTypesTbl', N'PriceLevelsTbl', N'ReoccuranceTypeTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'SectionTypesTbl', N'SendCheckEmailTextsTbl', N'SentRemindersLogTbl', N'ServiceTypesTbl', N'SysDataTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersTbl', N'TotalCountTrackerTbl', N'TrackedServiceItemTbl', N'TransactionTypesTbl', N'UsedItemGroupTbl', N'RepairsTbl', N'TempOrdersLinesTbl');
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
PRINT N'Purging [UsedItemGroupTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'UsedItemGroupTbl')
        TRUNCATE TABLE [UsedItemGroupTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [UsedItemGroupTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [UsedItemGroupTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [UsedItemGroupTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [UsedItemGroupTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [TrackedServiceItemTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'TrackedServiceItemTbl')
        TRUNCATE TABLE [TrackedServiceItemTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [TrackedServiceItemTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [TrackedServiceItemTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [TrackedServiceItemTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [TrackedServiceItemTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [ServiceTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ServiceTypesTbl')
        TRUNCATE TABLE [ServiceTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ServiceTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ServiceTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ServiceTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ServiceTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [SendCheckEmailTextsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'SendCheckEmailTextsTbl')
        TRUNCATE TABLE [SendCheckEmailTextsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [SendCheckEmailTextsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [SendCheckEmailTextsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [SendCheckEmailTextsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [SendCheckEmailTextsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [ReoccuranceTypeTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ReoccuranceTypeTbl')
        TRUNCATE TABLE [ReoccuranceTypeTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ReoccuranceTypeTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ReoccuranceTypeTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ReoccuranceTypeTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ReoccuranceTypeTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [PrepTypesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'PrepTypesTbl')
        TRUNCATE TABLE [PrepTypesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [PrepTypesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [PrepTypesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [PrepTypesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [PrepTypesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [PersonsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'PersonsTbl')
        TRUNCATE TABLE [PersonsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [PersonsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [PersonsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [PersonsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [PersonsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [NextRoastDateByCityTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'NextRoastDateByCityTbl')
        TRUNCATE TABLE [NextRoastDateByCityTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [NextRoastDateByCityTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [NextRoastDateByCityTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [NextRoastDateByCityTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [NextRoastDateByCityTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [MachineConditionsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'MachineConditionsTbl')
        TRUNCATE TABLE [MachineConditionsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [MachineConditionsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [MachineConditionsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [MachineConditionsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [MachineConditionsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ItemUsageTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemUsageTbl')
        TRUNCATE TABLE [ItemUsageTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemUsageTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemUsageTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemUsageTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemUsageTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [ItemTypeTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemTypeTbl')
        TRUNCATE TABLE [ItemTypeTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemTypeTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemTypeTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemTypeTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemTypeTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [ItemGroupTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ItemGroupTbl')
        TRUNCATE TABLE [ItemGroupTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ItemGroupTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ItemGroupTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ItemGroupTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ItemGroupTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [InvoiceTypeTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'InvoiceTypeTbl')
        TRUNCATE TABLE [InvoiceTypeTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [InvoiceTypeTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [InvoiceTypeTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [InvoiceTypeTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [InvoiceTypeTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [HolidayClosureTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'HolidayClosureTbl')
        TRUNCATE TABLE [HolidayClosureTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [HolidayClosureTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [HolidayClosureTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [HolidayClosureTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [HolidayClosureTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [EquipTypeTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'EquipTypeTbl')
        TRUNCATE TABLE [EquipTypeTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [EquipTypeTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [EquipTypeTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [EquipTypeTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [EquipTypeTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [CustomerTypeTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'CustomerTypeTbl')
        TRUNCATE TABLE [CustomerTypeTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [CustomerTypeTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [CustomerTypeTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [CustomerTypeTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [CustomerTypeTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [CustomerTrackedServiceItemsTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'CustomerTrackedServiceItemsTbl')
        TRUNCATE TABLE [CustomerTrackedServiceItemsTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [CustomerTrackedServiceItemsTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [CustomerTrackedServiceItemsTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [CustomerTrackedServiceItemsTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [CustomerTrackedServiceItemsTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [CustomersTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'CustomersTbl')
        TRUNCATE TABLE [CustomersTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [CustomersTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [CustomersTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [CustomersTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [CustomersTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [CustomersAccInfoTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'CustomersAccInfoTbl')
        TRUNCATE TABLE [CustomersAccInfoTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [CustomersAccInfoTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [CustomersAccInfoTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [CustomersAccInfoTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [CustomersAccInfoTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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
PRINT N'Purging [ClientUsageTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ClientUsageTbl')
        TRUNCATE TABLE [ClientUsageTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ClientUsageTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ClientUsageTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ClientUsageTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ClientUsageTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ClientUsageLinesTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ClientUsageLinesTbl')
        TRUNCATE TABLE [ClientUsageLinesTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ClientUsageLinesTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ClientUsageLinesTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ClientUsageLinesTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ClientUsageLinesTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [ClientAwayPeriodTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'ClientAwayPeriodTbl')
        TRUNCATE TABLE [ClientAwayPeriodTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [ClientAwayPeriodTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [ClientAwayPeriodTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [ClientAwayPeriodTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [ClientAwayPeriodTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [CityTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'CityTbl')
        TRUNCATE TABLE [CityTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [CityTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [CityTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [CityTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [CityTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
END CATCH
PRINT N'Purging [CityPrepDaysTbl]';
BEGIN TRY
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys fk
                   JOIN sys.tables rt ON rt.object_id = fk.referenced_object_id
                   WHERE rt.name = N'CityPrepDaysTbl')
        TRUNCATE TABLE [CityPrepDaysTbl];
    ELSE
    BEGIN
        PRINT N'INFO: [CityPrepDaysTbl] has referencing foreign keys – using DELETE';
        DELETE FROM [CityPrepDaysTbl];
    END
END TRY
BEGIN CATCH
    PRINT N'WARN: purge of [CityPrepDaysTbl] failed: ' + ERROR_MESSAGE();
    BEGIN TRY DELETE FROM [CityPrepDaysTbl]; END TRY BEGIN CATCH PRINT N'WARN: DELETE also failed: ' + ERROR_MESSAGE(); END CATCH
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

-- CityPrepDaysTbl -> CityPrepDaysTbl
-- Mapping: columnsCount=5
--   CityPrepDaysID -> CityPrepDaysID
--   CityID -> CityID
--   PrepDayOfWeekID -> PrepDayOfWeekID
--   DeliveryDelayDays -> DeliveryDelayDays
--   DeliveryOrder -> DeliveryOrder
IF OBJECT_ID(N'AccessSrc.CityPrepDaysTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [CityPrepDaysTbl]: missing source [AccessSrc].[CityPrepDaysTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [CityPrepDaysTbl] ([CityPrepDaysID], [CityID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]) SELECT NULLIF([CityPrepDaysID], N'''') AS [CityPrepDaysID], NULLIF([CityID], N'''') AS [CityID], NULLIF([PrepDayOfWeekID], N'''') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'''') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder] FROM [AccessSrc].[CityPrepDaysTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [CityPrepDaysTbl]
        (
            [CityPrepDaysID], [CityID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]
        )
        SELECT
            NULLIF([CityPrepDaysID], N'') AS [CityPrepDaysID], NULLIF([CityID], N'') AS [CityID], NULLIF([PrepDayOfWeekID], N'') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder]
        FROM [AccessSrc].[CityPrepDaysTbl];
        COMMIT;
        PRINT 'OK migrate [CityPrepDaysTbl] from ' + N'[AccessSrc].[CityPrepDaysTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [CityPrepDaysTbl] from ' + N'[AccessSrc].[CityPrepDaysTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CityTbl -> CityTbl
-- Mapping: columnsCount=4
--   ID -> ID
--   City -> City
--   RoastingDay -> RoastingDay
--   DeliveryDelay -> DeliveryDelay
IF OBJECT_ID(N'AccessSrc.CityTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [CityTbl]: missing source [AccessSrc].[CityTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [CityTbl] ([ID], [City], [RoastingDay], [DeliveryDelay]) SELECT NULLIF([ID], N'''') AS [ID], NULLIF([City], N'''') AS [City], NULLIF([RoastingDay], N'''') AS [RoastingDay], NULLIF([DeliveryDelay], N'''') AS [DeliveryDelay] FROM [AccessSrc].[CityTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [CityTbl]
        (
            [ID], [City], [RoastingDay], [DeliveryDelay]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], NULLIF([City], N'') AS [City], NULLIF([RoastingDay], N'') AS [RoastingDay], NULLIF([DeliveryDelay], N'') AS [DeliveryDelay]
        FROM [AccessSrc].[CityTbl];
        COMMIT;
        PRINT 'OK migrate [CityTbl] from ' + N'[AccessSrc].[CityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [CityTbl] from ' + N'[AccessSrc].[CityTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClientAwayPeriodTbl -> ClientAwayPeriodTbl
-- Mapping: columnsCount=5
--   AwayPeriodID -> AwayPeriodID
--   ClientID -> ClientID
--   AwayStartDate -> AwayStartDate
--   AwayEndDate -> AwayEndDate
--   ReasonID -> ReasonID
IF OBJECT_ID(N'AccessSrc.ClientAwayPeriodTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ClientAwayPeriodTbl]: missing source [AccessSrc].[ClientAwayPeriodTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ClientAwayPeriodTbl] ([AwayPeriodID], [ClientID], [AwayStartDate], [AwayEndDate], [ReasonID]) SELECT NULLIF([AwayPeriodID], N'''') AS [AwayPeriodID], NULLIF([ClientID], N'''') AS [ClientID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''))) AS [AwayStartDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''))) AS [AwayEndDate], NULLIF([ReasonID], N'''') AS [ReasonID] FROM [AccessSrc].[ClientAwayPeriodTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ClientAwayPeriodTbl]
        (
            [AwayPeriodID], [ClientID], [AwayStartDate], [AwayEndDate], [ReasonID]
        )
        SELECT
            NULLIF([AwayPeriodID], N'') AS [AwayPeriodID], NULLIF([ClientID], N'') AS [ClientID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''))) AS [AwayStartDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''))) AS [AwayEndDate], NULLIF([ReasonID], N'') AS [ReasonID]
        FROM [AccessSrc].[ClientAwayPeriodTbl];
        COMMIT;
        PRINT 'OK migrate [ClientAwayPeriodTbl] from ' + N'[AccessSrc].[ClientAwayPeriodTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ClientAwayPeriodTbl] from ' + N'[AccessSrc].[ClientAwayPeriodTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClientUsageLinesTbl -> ClientUsageLinesTbl
-- Mapping: columnsCount=7
--   ClientUsageLineNo -> ClientUsageLineNo
--   CustomerID -> CustomerID
--   Date -> Date
--   CupCount -> CupCount
--   ServiceTypeId -> ServiceTypeId
--   Qty -> Qty
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ClientUsageLinesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ClientUsageLinesTbl]: missing source [AccessSrc].[ClientUsageLinesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ClientUsageLinesTbl] ([ClientUsageLineNo], [CustomerID], [Date], [CupCount], [ServiceTypeId], [Qty], [Notes]) SELECT NULLIF([ClientUsageLineNo], N'''') AS [ClientUsageLineNo], NULLIF([CustomerID], N'''') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''))) AS [Date], NULLIF([CupCount], N'''') AS [CupCount], NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([Qty], N'''') AS [Qty], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[ClientUsageLinesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ClientUsageLinesTbl]
        (
            [ClientUsageLineNo], [CustomerID], [Date], [CupCount], [ServiceTypeId], [Qty], [Notes]
        )
        SELECT
            NULLIF([ClientUsageLineNo], N'') AS [ClientUsageLineNo], NULLIF([CustomerID], N'') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''))) AS [Date], NULLIF([CupCount], N'') AS [CupCount], NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([Qty], N'') AS [Qty], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[ClientUsageLinesTbl];
        COMMIT;
        PRINT 'OK migrate [ClientUsageLinesTbl] from ' + N'[AccessSrc].[ClientUsageLinesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ClientUsageLinesTbl] from ' + N'[AccessSrc].[ClientUsageLinesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClientUsageTbl -> ClientUsageTbl
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
IF OBJECT_ID(N'AccessSrc.ClientUsageTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ClientUsageTbl]: missing source [AccessSrc].[ClientUsageTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ClientUsageTbl] ([CustomerId], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]) SELECT NULLIF([CustomerId], N'''') AS [CustomerId], NULLIF([LastCupCount], N'''') AS [LastCupCount], NULLIF([NextCoffeeBy], N'''') AS [NextCoffeeBy], NULLIF([NextCleanOn], N'''') AS [NextCleanOn], NULLIF([NextFilterEst], N'''') AS [NextFilterEst], NULLIF([NextDescaleEst], N'''') AS [NextDescaleEst], NULLIF([NextServiceEst], N'''') AS [NextServiceEst], NULLIF([DailyConsumption], N'''') AS [DailyConsumption], NULLIF([FilterAveCount], N'''') AS [FilterAveCount], NULLIF([DescaleAveCount], N'''') AS [DescaleAveCount], NULLIF([ServiceAveCount], N'''') AS [ServiceAveCount], NULLIF([CleanAveCount], N'''') AS [CleanAveCount] FROM [AccessSrc].[ClientUsageTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ClientUsageTbl]
        (
            [CustomerId], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]
        )
        SELECT
            NULLIF([CustomerId], N'') AS [CustomerId], NULLIF([LastCupCount], N'') AS [LastCupCount], NULLIF([NextCoffeeBy], N'') AS [NextCoffeeBy], NULLIF([NextCleanOn], N'') AS [NextCleanOn], NULLIF([NextFilterEst], N'') AS [NextFilterEst], NULLIF([NextDescaleEst], N'') AS [NextDescaleEst], NULLIF([NextServiceEst], N'') AS [NextServiceEst], NULLIF([DailyConsumption], N'') AS [DailyConsumption], NULLIF([FilterAveCount], N'') AS [FilterAveCount], NULLIF([DescaleAveCount], N'') AS [DescaleAveCount], NULLIF([ServiceAveCount], N'') AS [ServiceAveCount], NULLIF([CleanAveCount], N'') AS [CleanAveCount]
        FROM [AccessSrc].[ClientUsageTbl];
        COMMIT;
        PRINT 'OK migrate [ClientUsageTbl] from ' + N'[AccessSrc].[ClientUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ClientUsageTbl] from ' + N'[AccessSrc].[ClientUsageTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'INSERT INTO [ClosureDatesTbl] ([DateClosed], [DateReopen], [Comments]) SELECT COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''))) AS [DateClosed], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''))) AS [DateReopen], NULLIF([Comments], N'''') AS [Comments] FROM [AccessSrc].[ClosureDatesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ClosureDatesTbl]
        (
            [DateClosed], [DateReopen], [Comments]
        )
        SELECT
            COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''))) AS [DateClosed], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''))) AS [DateReopen], NULLIF([Comments], N'') AS [Comments]
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

-- CustomersAccInfoTbl -> CustomersAccInfoTbl
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
IF OBJECT_ID(N'AccessSrc.CustomersAccInfoTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [CustomersAccInfoTbl]: missing source [AccessSrc].[CustomersAccInfoTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [CustomersAccInfoTbl] ([CustomersAccInfoID], [CustomerID], [RequiresPurchOrder], [CustomerVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]) SELECT NULLIF([CustomersAccInfoID], N'''') AS [CustomersAccInfoID], NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([RequiresPurchOrder], N'''') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'''') AS [CustomerVATNo], NULLIF([BillAddr1], N'''') AS [BillAddr1], NULLIF([BillAddr2], N'''') AS [BillAddr2], NULLIF([BillAddr3], N'''') AS [BillAddr3], NULLIF([BillAddr4], N'''') AS [BillAddr4], NULLIF([BillAddr5], N'''') AS [BillAddr5], NULLIF([ShipAddr1], N'''') AS [ShipAddr1], NULLIF([ShipAddr2], N'''') AS [ShipAddr2], NULLIF([ShipAddr3], N'''') AS [ShipAddr3], NULLIF([ShipAddr4], N'''') AS [ShipAddr4], NULLIF([ShipAddr5], N'''') AS [ShipAddr5], NULLIF([AccEmail], N'''') AS [AccEmail], NULLIF([AltAccEmail], N'''') AS [AltAccEmail], NULLIF([PaymentTermID], N'''') AS [PaymentTermID], NULLIF([Limit], N'''') AS [Limit], NULLIF([FullCoName], N'''') AS [FullCoName], NULLIF([AccFirstName], N'''') AS [AccFirstName], NULLIF([AccLastName], N'''') AS [AccLastName], NULLIF([AltAccFirstName], N'''') AS [AltAccFirstName], NULLIF([AltAccLastName], N'''') AS [AltAccLastName], NULL ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [CustomersAccInfoTbl]
        (
            [CustomersAccInfoID], [CustomerID], [RequiresPurchOrder], [CustomerVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]
        )
        SELECT
            NULLIF([CustomersAccInfoID], N'') AS [CustomersAccInfoID], NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([RequiresPurchOrder], N'') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'') AS [CustomerVATNo], NULLIF([BillAddr1], N'') AS [BillAddr1], NULLIF([BillAddr2], N'') AS [BillAddr2], NULLIF([BillAddr3], N'') AS [BillAddr3], NULLIF([BillAddr4], N'') AS [BillAddr4], NULLIF([BillAddr5], N'') AS [BillAddr5], NULLIF([ShipAddr1], N'') AS [ShipAddr1], NULLIF([ShipAddr2], N'') AS [ShipAddr2], NULLIF([ShipAddr3], N'') AS [ShipAddr3], NULLIF([ShipAddr4], N'') AS [ShipAddr4], NULLIF([ShipAddr5], N'') AS [ShipAddr5], NULLIF([AccEmail], N'') AS [AccEmail], NULLIF([AltAccEmail], N'') AS [AltAccEmail], NULLIF([PaymentTermID], N'') AS [PaymentTermID], NULLIF([Limit], N'') AS [Limit], NULLIF([FullCoName], N'') AS [FullCoName], NULLIF([AccFirstName], N'') AS [AccFirstName], NULLIF([AccLastName], N'') AS [AccLastName], NULLIF([AltAccFirstName], N'') AS [AltAccFirstName], NULLIF([AltAccLastName], N'') AS [AltAccLastName], NULLIF([PriceLevelID], N'') AS [PriceLevelID], NULLIF([InvoiceTypeID], N'') AS [InvoiceTypeID], NULLIF([RegNo], N'') AS [RegNo], NULLIF([BankAccNo], N'') AS [BankAccNo], NULLIF([BankBranch], N'') AS [BankBranch], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomersAccInfoTbl];
        COMMIT;
        PRINT 'OK migrate [CustomersAccInfoTbl] from ' + N'[AccessSrc].[CustomersAccInfoTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [CustomersAccInfoTbl] from ' + N'[AccessSrc].[CustomersAccInfoTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomersTbl -> CustomersTbl
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
IF OBJECT_ID(N'AccessSrc.CustomersTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [CustomersTbl]: missing source [AccessSrc].[CustomersTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [CustomersTbl] ([CustomerID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [City], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [CustomerTypeID], [CustomerTypeOLD], [EquipType], [CoffeePreference], [PriPrefQty], [PrefPrepTypeID], [PrefPackagingID], [SecondaryPreference], [SecPrefQty], [TypicallySecToo], [PreferedAgent], [SalesAgentID], [MachineSN], [UsesFilter], [autofulfill], [enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]) SELECT NULLIF([CustomerID], N'''') AS [CustomerID], NULLIF([CompanyName], N'''') AS [CompanyName], NULLIF([ContactTitle], N'''') AS [ContactTitle], NULLIF([ContactFirstName], N'''') AS [ContactFirstName], NULLIF([ContactLastName], N'''') AS [ContactLastName], NULLIF([ContactAltFirstName], N'''') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'''') AS [ContactAltLastName], NULLIF([Department], N'''') AS [Department], NULLIF([BillingAddress], N'''') AS [BillingAddress], NULLIF([City], N'''') AS [City], NULLIF([StateOrProvince], N'''') AS [StateOrProvince], NULLIF([PostalCode], N'''') AS [PostalCode], NULLIF([Country/Region], N'''') AS [Country/Region], NULLIF([PhoneNumber], N'''') AS [PhoneNumber], NULLIF([Extension], N'''') AS [Extension], NULLIF([FaxNumber] ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [CustomersTbl]
        (
            [CustomerID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [City], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [CustomerTypeID], [CustomerTypeOLD], [EquipType], [CoffeePreference], [PriPrefQty], [PrefPrepTypeID], [PrefPackagingID], [SecondaryPreference], [SecPrefQty], [TypicallySecToo], [PreferedAgent], [SalesAgentID], [MachineSN], [UsesFilter], [autofulfill], [enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]
        )
        SELECT
            NULLIF([CustomerID], N'') AS [CustomerID], NULLIF([CompanyName], N'') AS [CompanyName], NULLIF([ContactTitle], N'') AS [ContactTitle], NULLIF([ContactFirstName], N'') AS [ContactFirstName], NULLIF([ContactLastName], N'') AS [ContactLastName], NULLIF([ContactAltFirstName], N'') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'') AS [ContactAltLastName], NULLIF([Department], N'') AS [Department], NULLIF([BillingAddress], N'') AS [BillingAddress], NULLIF([City], N'') AS [City], NULLIF([StateOrProvince], N'') AS [StateOrProvince], NULLIF([PostalCode], N'') AS [PostalCode], NULLIF([Country/Region], N'') AS [Country/Region], NULLIF([PhoneNumber], N'') AS [PhoneNumber], NULLIF([Extension], N'') AS [Extension], NULLIF([FaxNumber], N'') AS [FaxNumber], NULLIF([CellNumber], N'') AS [CellNumber], NULLIF([EmailAddress], N'') AS [EmailAddress], NULLIF([AltEmailAddress], N'') AS [AltEmailAddress], NULLIF([ContractNo], N'') AS [ContractNo], NULLIF([CustomerTypeID], N'') AS [CustomerTypeID], NULLIF([CustomerTypeOLD], N'') AS [CustomerTypeOLD], NULLIF([EquipType], N'') AS [EquipType], NULLIF([CoffeePreference], N'') AS [CoffeePreference], NULLIF([PriPrefQty], N'') AS [PriPrefQty], NULLIF([PrefPrepTypeID], N'') AS [PrefPrepTypeID], NULLIF([PrefPackagingID], N'') AS [PrefPackagingID], NULLIF([SecondaryPreference], N'') AS [SecondaryPreference], NULLIF([SecPrefQty], N'') AS [SecPrefQty], CASE WHEN NULLIF([TypicallySecToo], N'') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TypicallySecToo], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'')) END AS [TypicallySecToo], NULLIF([PreferedAgent], N'') AS [PreferedAgent], NULLIF([SalesAgentID], N'') AS [SalesAgentID], NULLIF([MachineSN], N'') AS [MachineSN], CASE WHEN NULLIF([UsesFilter], N'') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UsesFilter], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'')) END AS [UsesFilter], CASE WHEN NULLIF([autofulfill], N'') IS NULL THEN NULL WHEN NULLIF([autofulfill], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([autofulfill], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([autofulfill], N'')) END AS [autofulfill], CASE WHEN NULLIF([enabled], N'') IS NULL THEN NULL WHEN NULLIF([enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([enabled], N'')) END AS [enabled], NULLIF([PredictionDisabled], N'') AS [PredictionDisabled], NULLIF([AlwaysSendChkUp], N'') AS [AlwaysSendChkUp], NULLIF([NormallyResponds], N'') AS [NormallyResponds], NULLIF([ReminderCount], N'') AS [ReminderCount], NULLIF([Notes], N'') AS [Notes], CASE WHEN NULLIF([SendDeliveryConfirmation], N'') IS NULL THEN NULL WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([SendDeliveryConfirmation], N'')) END AS [SendDeliveryConfirmation], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''))) AS [LastDateSentReminder]
        FROM [AccessSrc].[CustomersTbl];
        COMMIT;
        PRINT 'OK migrate [CustomersTbl] from ' + N'[AccessSrc].[CustomersTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [CustomersTbl] from ' + N'[AccessSrc].[CustomersTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomerTrackedServiceItemsTbl -> CustomerTrackedServiceItemsTbl
-- Mapping: columnsCount=4
--   CustomerTrackedServiceItemsID -> CustomerTrackedServiceItemsID
--   CustomerTypeID -> CustomerTypeID
--   ServiceTypeID -> ServiceTypeID
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.CustomerTrackedServiceItemsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [CustomerTrackedServiceItemsTbl]: missing source [AccessSrc].[CustomerTrackedServiceItemsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [CustomerTrackedServiceItemsTbl] ([CustomerTrackedServiceItemsID], [CustomerTypeID], [ServiceTypeID], [Notes]) SELECT NULLIF([CustomerTrackedServiceItemsID], N'''') AS [CustomerTrackedServiceItemsID], NULLIF([CustomerTypeID], N'''') AS [CustomerTypeID], NULLIF([ServiceTypeID], N'''') AS [ServiceTypeID], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[CustomerTrackedServiceItemsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [CustomerTrackedServiceItemsTbl]
        (
            [CustomerTrackedServiceItemsID], [CustomerTypeID], [ServiceTypeID], [Notes]
        )
        SELECT
            NULLIF([CustomerTrackedServiceItemsID], N'') AS [CustomerTrackedServiceItemsID], NULLIF([CustomerTypeID], N'') AS [CustomerTypeID], NULLIF([ServiceTypeID], N'') AS [ServiceTypeID], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomerTrackedServiceItemsTbl];
        COMMIT;
        PRINT 'OK migrate [CustomerTrackedServiceItemsTbl] from ' + N'[AccessSrc].[CustomerTrackedServiceItemsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [CustomerTrackedServiceItemsTbl] from ' + N'[AccessSrc].[CustomerTrackedServiceItemsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomerTypeTbl -> CustomerTypeTbl
-- Mapping: columnsCount=3
--   CustTypeID -> CustTypeID
--   CustTypeDesc -> CustTypeDesc
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.CustomerTypeTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [CustomerTypeTbl]: missing source [AccessSrc].[CustomerTypeTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [CustomerTypeTbl] ([CustTypeID], [CustTypeDesc], [Notes]) SELECT NULLIF([CustTypeID], N'''') AS [CustTypeID], NULLIF([CustTypeDesc], N'''') AS [CustTypeDesc], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[CustomerTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [CustomerTypeTbl]
        (
            [CustTypeID], [CustTypeDesc], [Notes]
        )
        SELECT
            NULLIF([CustTypeID], N'') AS [CustTypeID], NULLIF([CustTypeDesc], N'') AS [CustTypeDesc], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomerTypeTbl];
        COMMIT;
        PRINT 'OK migrate [CustomerTypeTbl] from ' + N'[AccessSrc].[CustomerTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [CustomerTypeTbl] from ' + N'[AccessSrc].[CustomerTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- EquipTypeTbl -> EquipTypeTbl
-- Mapping: columnsCount=3
--   EquipTypeId -> EquipTypeId
--   EquipTypeName -> EquipTypeName
--   EquipTypeDesc -> EquipTypeDesc
IF OBJECT_ID(N'AccessSrc.EquipTypeTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [EquipTypeTbl]: missing source [AccessSrc].[EquipTypeTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [EquipTypeTbl] ([EquipTypeId], [EquipTypeName], [EquipTypeDesc]) SELECT NULLIF([EquipTypeId], N'''') AS [EquipTypeId], NULLIF([EquipTypeName], N'''') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'''') AS [EquipTypeDesc] FROM [AccessSrc].[EquipTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [EquipTypeTbl]
        (
            [EquipTypeId], [EquipTypeName], [EquipTypeDesc]
        )
        SELECT
            NULLIF([EquipTypeId], N'') AS [EquipTypeId], NULLIF([EquipTypeName], N'') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'') AS [EquipTypeDesc]
        FROM [AccessSrc].[EquipTypeTbl];
        COMMIT;
        PRINT 'OK migrate [EquipTypeTbl] from ' + N'[AccessSrc].[EquipTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [EquipTypeTbl] from ' + N'[AccessSrc].[EquipTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- HolidayClosureTbl -> HolidayClosureTbl
-- Mapping: columnsCount=7
--   ID -> ID
--   ClosureDate -> ClosureDate
--   DaysClosed -> DaysClosed
--   AppliesToPrep -> AppliesToPrep
--   AppliesToDelivery -> AppliesToDelivery
--   ShiftStrategy -> ShiftStrategy
--   Description -> Description
IF OBJECT_ID(N'AccessSrc.HolidayClosureTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [HolidayClosureTbl]: missing source [AccessSrc].[HolidayClosureTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [HolidayClosureTbl] ([ID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]) SELECT NULLIF([ID], N'''') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''))) AS [ClosureDate], NULLIF([DaysClosed], N'''') AS [DaysClosed], NULLIF([AppliesToPrep], N'''') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'''') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'''') AS [ShiftStrategy], NULLIF([Description], N'''') AS [Description] FROM [AccessSrc].[HolidayClosureTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [HolidayClosureTbl]
        (
            [ID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''))) AS [ClosureDate], NULLIF([DaysClosed], N'') AS [DaysClosed], NULLIF([AppliesToPrep], N'') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'') AS [ShiftStrategy], NULLIF([Description], N'') AS [Description]
        FROM [AccessSrc].[HolidayClosureTbl];
        COMMIT;
        PRINT 'OK migrate [HolidayClosureTbl] from ' + N'[AccessSrc].[HolidayClosureTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [HolidayClosureTbl] from ' + N'[AccessSrc].[HolidayClosureTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- InvoiceTypeTbl -> InvoiceTypeTbl
-- Mapping: columnsCount=4
--   InvoiceTypeID -> InvoiceTypeID
--   InvoiceTypeDesc -> InvoiceTypeDesc
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.InvoiceTypeTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [InvoiceTypeTbl]: missing source [AccessSrc].[InvoiceTypeTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [InvoiceTypeTbl] ([InvoiceTypeID], [InvoiceTypeDesc], [Enabled], [Notes]) SELECT NULLIF([InvoiceTypeID], N'''') AS [InvoiceTypeID], NULLIF([InvoiceTypeDesc], N'''') AS [InvoiceTypeDesc], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[InvoiceTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [InvoiceTypeTbl]
        (
            [InvoiceTypeID], [InvoiceTypeDesc], [Enabled], [Notes]
        )
        SELECT
            NULLIF([InvoiceTypeID], N'') AS [InvoiceTypeID], NULLIF([InvoiceTypeDesc], N'') AS [InvoiceTypeDesc], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[InvoiceTypeTbl];
        COMMIT;
        PRINT 'OK migrate [InvoiceTypeTbl] from ' + N'[AccessSrc].[InvoiceTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [InvoiceTypeTbl] from ' + N'[AccessSrc].[InvoiceTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemGroupTbl -> ItemGroupTbl
-- Mapping: columnsCount=6
--   ItemGroupID -> ItemGroupID
--   GroupItemTypeID -> GroupItemTypeID
--   ItemTypeID -> ItemTypeID
--   ItemTypeSortPos -> ItemTypeSortPos
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ItemGroupTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ItemGroupTbl]: missing source [AccessSrc].[ItemGroupTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemGroupTbl] ([ItemGroupID], [GroupItemTypeID], [ItemTypeID], [ItemTypeSortPos], [Enabled], [Notes]) SELECT NULLIF([ItemGroupID], N'''') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'''') AS [GroupItemTypeID], NULLIF([ItemTypeID], N'''') AS [ItemTypeID], NULLIF([ItemTypeSortPos], N'''') AS [ItemTypeSortPos], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[ItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemGroupTbl]
        (
            [ItemGroupID], [GroupItemTypeID], [ItemTypeID], [ItemTypeSortPos], [Enabled], [Notes]
        )
        SELECT
            NULLIF([ItemGroupID], N'') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'') AS [GroupItemTypeID], NULLIF([ItemTypeID], N'') AS [ItemTypeID], NULLIF([ItemTypeSortPos], N'') AS [ItemTypeSortPos], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[ItemGroupTbl];
        COMMIT;
        PRINT 'OK migrate [ItemGroupTbl] from ' + N'[AccessSrc].[ItemGroupTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemGroupTbl] from ' + N'[AccessSrc].[ItemGroupTbl]' + ': ' + ERROR_MESSAGE();
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

-- ItemTypeTbl -> ItemTypeTbl
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
IF OBJECT_ID(N'AccessSrc.ItemTypeTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ItemTypeTbl]: missing source [AccessSrc].[ItemTypeTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemTypeTbl] ([ItemTypeID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ServiceTypeId], [ReplacementID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]) SELECT NULLIF([ItemTypeID], N'''') AS [ItemTypeID], NULLIF([SKU], N'''') AS [SKU], NULLIF([ItemDesc], N'''') AS [ItemDesc], NULLIF([ItemEnabled], N'''') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'''') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'''') AS [ItemDetail], NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([ReplacementID], N'''') AS [ReplacementID], NULLIF([ItemShortName], N'''') AS [ItemShortName], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([UnitsPerQty], N'''') AS [UnitsPerQty], NULLIF([ItemUnitID], N'''') AS [ItemUnitID], NULLIF([BasePrice], N'''') AS [BasePrice] FROM [AccessSrc].[ItemTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemTypeTbl]
        (
            [ItemTypeID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ServiceTypeId], [ReplacementID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]
        )
        SELECT
            NULLIF([ItemTypeID], N'') AS [ItemTypeID], NULLIF([SKU], N'') AS [SKU], NULLIF([ItemDesc], N'') AS [ItemDesc], NULLIF([ItemEnabled], N'') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'') AS [ItemDetail], NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([ReplacementID], N'') AS [ReplacementID], NULLIF([ItemShortName], N'') AS [ItemShortName], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([UnitsPerQty], N'') AS [UnitsPerQty], NULLIF([ItemUnitID], N'') AS [ItemUnitID], NULLIF([BasePrice], N'') AS [BasePrice]
        FROM [AccessSrc].[ItemTypeTbl];
        COMMIT;
        PRINT 'OK migrate [ItemTypeTbl] from ' + N'[AccessSrc].[ItemTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemTypeTbl] from ' + N'[AccessSrc].[ItemTypeTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemUnitsTbl] ([ItemUnitID], [UnitOfMeasure], [UnitDescription]) SELECT NULLIF([ItemUnitID], N'''') AS [ItemUnitID], NULLIF([UnitOfMeasure], N'''') AS [UnitOfMeasure], NULLIF([UnitDescription], N'''') AS [UnitDescription] FROM [AccessSrc].[ItemUnitsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemUnitsTbl] ON;
        INSERT INTO [ItemUnitsTbl]
        (
            [ItemUnitID], [UnitOfMeasure], [UnitDescription]
        )
        SELECT
            NULLIF([ItemUnitID], N'') AS [ItemUnitID], NULLIF([UnitOfMeasure], N'') AS [UnitOfMeasure], NULLIF([UnitDescription], N'') AS [UnitDescription]
        FROM [AccessSrc].[ItemUnitsTbl];
        SET IDENTITY_INSERT [ItemUnitsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemUnitsTbl'))
            DBCC CHECKIDENT (N'ItemUnitsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemUnitsTbl] from ' + N'[AccessSrc].[ItemUnitsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemUnitsTbl') IS NOT NULL SET IDENTITY_INSERT [ItemUnitsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemUnitsTbl] from ' + N'[AccessSrc].[ItemUnitsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemUsageTbl -> ItemUsageTbl
-- Mapping: columnsCount=8
--   ClientUsageLineNo -> ClientUsageLineNo
--   CustomerID -> CustomerID
--   Date -> Date
--   ItemProvided -> ItemProvided
--   AmountProvided -> AmountProvided
--   PrepTypeID -> PrepTypeID
--   PackagingID -> PackagingID
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ItemUsageTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ItemUsageTbl]: missing source [AccessSrc].[ItemUsageTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ItemUsageTbl] ([ClientUsageLineNo], [CustomerID], [Date], [ItemProvided], [AmountProvided], [PrepTypeID], [PackagingID], [Notes]) SELECT NULLIF([ClientUsageLineNo], N'''') AS [ClientUsageLineNo], NULLIF([CustomerID], N'''') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''))) AS [Date], NULLIF([ItemProvided], N'''') AS [ItemProvided], NULLIF([AmountProvided], N'''') AS [AmountProvided], NULLIF([PrepTypeID], N'''') AS [PrepTypeID], NULLIF([PackagingID], N'''') AS [PackagingID], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[ItemUsageTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ItemUsageTbl]
        (
            [ClientUsageLineNo], [CustomerID], [Date], [ItemProvided], [AmountProvided], [PrepTypeID], [PackagingID], [Notes]
        )
        SELECT
            NULLIF([ClientUsageLineNo], N'') AS [ClientUsageLineNo], NULLIF([CustomerID], N'') AS [CustomerID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''))) AS [Date], NULLIF([ItemProvided], N'') AS [ItemProvided], NULLIF([AmountProvided], N'') AS [AmountProvided], NULLIF([PrepTypeID], N'') AS [PrepTypeID], NULLIF([PackagingID], N'') AS [PackagingID], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[ItemUsageTbl];
        COMMIT;
        PRINT 'OK migrate [ItemUsageTbl] from ' + N'[AccessSrc].[ItemUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ItemUsageTbl] from ' + N'[AccessSrc].[ItemUsageTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- MachineConditionsTbl -> MachineConditionsTbl
-- Mapping: columnsCount=4
--   MachineConditionID -> MachineConditionID
--   ConditionDesc -> ConditionDesc
--   SortOrder -> SortOrder
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.MachineConditionsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [MachineConditionsTbl]: missing source [AccessSrc].[MachineConditionsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [MachineConditionsTbl] ([MachineConditionID], [ConditionDesc], [SortOrder], [Notes]) SELECT NULLIF([MachineConditionID], N'''') AS [MachineConditionID], NULLIF([ConditionDesc], N'''') AS [ConditionDesc], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[MachineConditionsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [MachineConditionsTbl]
        (
            [MachineConditionID], [ConditionDesc], [SortOrder], [Notes]
        )
        SELECT
            NULLIF([MachineConditionID], N'') AS [MachineConditionID], NULLIF([ConditionDesc], N'') AS [ConditionDesc], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[MachineConditionsTbl];
        COMMIT;
        PRINT 'OK migrate [MachineConditionsTbl] from ' + N'[AccessSrc].[MachineConditionsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [MachineConditionsTbl] from ' + N'[AccessSrc].[MachineConditionsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- NextRoastDateByCityTbl -> NextRoastDateByCityTbl
-- Mapping: columnsCount=7
--   NextRoastDayID -> NextRoastDayID
--   CityID -> CityID
--   PreperationDate -> PreperationDate
--   DeliveryDate -> DeliveryDate
--   DeliveryOrder -> DeliveryOrder
--   NextPreperationDate -> NextPreperationDate
--   NextDeliveryDate -> NextDeliveryDate
IF OBJECT_ID(N'AccessSrc.NextRoastDateByCityTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [NextRoastDateByCityTbl]: missing source [AccessSrc].[NextRoastDateByCityTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [NextRoastDateByCityTbl] ([NextRoastDayID], [CityID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPreperationDate], [NextDeliveryDate]) SELECT NULLIF([NextRoastDayID], N'''') AS [NextRoastDayID], NULLIF([CityID], N'''') AS [CityID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''))) AS [PreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''))) AS [DeliveryDate], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''''))) AS [NextPreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''''), 126), TRY_CONVERT(da ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [NextRoastDateByCityTbl]
        (
            [NextRoastDayID], [CityID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPreperationDate], [NextDeliveryDate]
        )
        SELECT
            NULLIF([NextRoastDayID], N'') AS [NextRoastDayID], NULLIF([CityID], N'') AS [CityID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''))) AS [PreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''))) AS [DeliveryDate], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''))) AS [NextPreperationDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''))) AS [NextDeliveryDate]
        FROM [AccessSrc].[NextRoastDateByCityTbl];
        COMMIT;
        PRINT 'OK migrate [NextRoastDateByCityTbl] from ' + N'[AccessSrc].[NextRoastDateByCityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [NextRoastDateByCityTbl] from ' + N'[AccessSrc].[NextRoastDateByCityTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [PaymentTermsTbl] ([PaymentTermID], [PaymentTermDesc], [PaymentDays], [DayOfMonth], [UseDays], [Enabled], [Notes]) SELECT NULLIF([PaymentTermID], N'''') AS [PaymentTermID], NULLIF([PaymentTermDesc], N'''') AS [PaymentTermDesc], NULLIF([PaymentDays], N'''') AS [PaymentDays], NULLIF([DayOfMonth], N'''') AS [DayOfMonth], CASE WHEN NULLIF([UseDays], N'''') IS NULL THEN NULL WHEN NULLIF([UseDays], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([UseDays], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UseDays], N'''')) END AS [UseDays], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[PaymentTermsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [PaymentTermsTbl] ON;
        INSERT INTO [PaymentTermsTbl]
        (
            [PaymentTermID], [PaymentTermDesc], [PaymentDays], [DayOfMonth], [UseDays], [Enabled], [Notes]
        )
        SELECT
            NULLIF([PaymentTermID], N'') AS [PaymentTermID], NULLIF([PaymentTermDesc], N'') AS [PaymentTermDesc], NULLIF([PaymentDays], N'') AS [PaymentDays], NULLIF([DayOfMonth], N'') AS [DayOfMonth], CASE WHEN NULLIF([UseDays], N'') IS NULL THEN NULL WHEN NULLIF([UseDays], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UseDays], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UseDays], N'')) END AS [UseDays], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[PaymentTermsTbl];
        SET IDENTITY_INSERT [PaymentTermsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'PaymentTermsTbl'))
            DBCC CHECKIDENT (N'PaymentTermsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [PaymentTermsTbl] from ' + N'[AccessSrc].[PaymentTermsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'PaymentTermsTbl') IS NOT NULL SET IDENTITY_INSERT [PaymentTermsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [PaymentTermsTbl] from ' + N'[AccessSrc].[PaymentTermsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- PersonsTbl -> PersonsTbl
-- Mapping: columnsCount=6
--   PersonID -> PersonID
--   Person -> Person
--   Abreviation -> Abreviation
--   Enabled -> Enabled
--   NormalDeliveryDoW -> NormalDeliveryDoW
--   SecurityUsername -> SecurityUsername
IF OBJECT_ID(N'AccessSrc.PersonsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [PersonsTbl]: missing source [AccessSrc].[PersonsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [PersonsTbl] ([PersonID], [Person], [Abreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]) SELECT NULLIF([PersonID], N'''') AS [PersonID], NULLIF([Person], N'''') AS [Person], NULLIF([Abreviation], N'''') AS [Abreviation], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'''') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'''') AS [SecurityUsername] FROM [AccessSrc].[PersonsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [PersonsTbl]
        (
            [PersonID], [Person], [Abreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]
        )
        SELECT
            NULLIF([PersonID], N'') AS [PersonID], NULLIF([Person], N'') AS [Person], NULLIF([Abreviation], N'') AS [Abreviation], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'') AS [SecurityUsername]
        FROM [AccessSrc].[PersonsTbl];
        COMMIT;
        PRINT 'OK migrate [PersonsTbl] from ' + N'[AccessSrc].[PersonsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [PersonsTbl] from ' + N'[AccessSrc].[PersonsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- PrepTypesTbl -> PrepTypesTbl
-- Mapping: columnsCount=3
--   PrepID -> PrepID
--   PrepType -> PrepType
--   IdentifyingChar -> IdentifyingChar
IF OBJECT_ID(N'AccessSrc.PrepTypesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [PrepTypesTbl]: missing source [AccessSrc].[PrepTypesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [PrepTypesTbl] ([PrepID], [PrepType], [IdentifyingChar]) SELECT NULLIF([PrepID], N'''') AS [PrepID], NULLIF([PrepType], N'''') AS [PrepType], NULLIF([IdentifyingChar], N'''') AS [IdentifyingChar] FROM [AccessSrc].[PrepTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [PrepTypesTbl]
        (
            [PrepID], [PrepType], [IdentifyingChar]
        )
        SELECT
            NULLIF([PrepID], N'') AS [PrepID], NULLIF([PrepType], N'') AS [PrepType], NULLIF([IdentifyingChar], N'') AS [IdentifyingChar]
        FROM [AccessSrc].[PrepTypesTbl];
        COMMIT;
        PRINT 'OK migrate [PrepTypesTbl] from ' + N'[AccessSrc].[PrepTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [PrepTypesTbl] from ' + N'[AccessSrc].[PrepTypesTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [PriceLevelsTbl] ([PriceLevelID], [PriceLevelDesc], [PricingFactor], [Enabled], [Notes]) SELECT NULLIF([PriceLevelID], N'''') AS [PriceLevelID], NULLIF([PriceLevelDesc], N'''') AS [PriceLevelDesc], NULLIF([PricingFactor], N'''') AS [PricingFactor], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[PriceLevelsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [PriceLevelsTbl] ON;
        INSERT INTO [PriceLevelsTbl]
        (
            [PriceLevelID], [PriceLevelDesc], [PricingFactor], [Enabled], [Notes]
        )
        SELECT
            NULLIF([PriceLevelID], N'') AS [PriceLevelID], NULLIF([PriceLevelDesc], N'') AS [PriceLevelDesc], NULLIF([PricingFactor], N'') AS [PricingFactor], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[PriceLevelsTbl];
        SET IDENTITY_INSERT [PriceLevelsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'PriceLevelsTbl'))
            DBCC CHECKIDENT (N'PriceLevelsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [PriceLevelsTbl] from ' + N'[AccessSrc].[PriceLevelsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'PriceLevelsTbl') IS NOT NULL SET IDENTITY_INSERT [PriceLevelsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [PriceLevelsTbl] from ' + N'[AccessSrc].[PriceLevelsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ReoccuranceTypeTbl -> ReoccuranceTypeTbl
-- Mapping: columnsCount=2
--   ID -> ID
--   Type -> Type
IF OBJECT_ID(N'AccessSrc.ReoccuranceTypeTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ReoccuranceTypeTbl]: missing source [AccessSrc].[ReoccuranceTypeTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ReoccuranceTypeTbl] ([ID], [Type]) SELECT NULLIF([ID], N'''') AS [ID], NULLIF([Type], N'''') AS [Type] FROM [AccessSrc].[ReoccuranceTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ReoccuranceTypeTbl]
        (
            [ID], [Type]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], NULLIF([Type], N'') AS [Type]
        FROM [AccessSrc].[ReoccuranceTypeTbl];
        COMMIT;
        PRINT 'OK migrate [ReoccuranceTypeTbl] from ' + N'[AccessSrc].[ReoccuranceTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ReoccuranceTypeTbl] from ' + N'[AccessSrc].[ReoccuranceTypeTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [RepairFaultsTbl] ([RepairFaultID], [RepairFaultDesc], [SortOrder], [Notes]) SELECT NULLIF([RepairFaultID], N'''') AS [RepairFaultID], NULLIF([RepairFaultDesc], N'''') AS [RepairFaultDesc], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[RepairFaultsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RepairFaultsTbl] ON;
        INSERT INTO [RepairFaultsTbl]
        (
            [RepairFaultID], [RepairFaultDesc], [SortOrder], [Notes]
        )
        SELECT
            NULLIF([RepairFaultID], N'') AS [RepairFaultID], NULLIF([RepairFaultDesc], N'') AS [RepairFaultDesc], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[RepairFaultsTbl];
        SET IDENTITY_INSERT [RepairFaultsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'RepairFaultsTbl'))
            DBCC CHECKIDENT (N'RepairFaultsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [RepairFaultsTbl] from ' + N'[AccessSrc].[RepairFaultsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'RepairFaultsTbl') IS NOT NULL SET IDENTITY_INSERT [RepairFaultsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [RepairStatusesTbl] ([RepairStatusID], [RepairStatusDesc], [SortOrder], [Notes], [StatusNote]) SELECT NULLIF([RepairStatusID], N'''') AS [RepairStatusID], NULLIF([RepairStatusDesc], N'''') AS [RepairStatusDesc], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes], NULLIF([StatusNote], N'''') AS [StatusNote] FROM [AccessSrc].[RepairStatusesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RepairStatusesTbl] ON;
        INSERT INTO [RepairStatusesTbl]
        (
            [RepairStatusID], [RepairStatusDesc], [SortOrder], [Notes], [StatusNote]
        )
        SELECT
            NULLIF([RepairStatusID], N'') AS [RepairStatusID], NULLIF([RepairStatusDesc], N'') AS [RepairStatusDesc], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes], NULLIF([StatusNote], N'') AS [StatusNote]
        FROM [AccessSrc].[RepairStatusesTbl];
        SET IDENTITY_INSERT [RepairStatusesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'RepairStatusesTbl'))
            DBCC CHECKIDENT (N'RepairStatusesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [RepairStatusesTbl] from ' + N'[AccessSrc].[RepairStatusesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'RepairStatusesTbl') IS NOT NULL SET IDENTITY_INSERT [RepairStatusesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [RepairStatusesTbl] from ' + N'[AccessSrc].[RepairStatusesTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [SectionTypesTbl] ([SectionID], [SectionType], [Notes]) SELECT NULLIF([SectionID], N'''') AS [SectionID], NULLIF([SectionType], N'''') AS [SectionType], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[SectionTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [SectionTypesTbl] ON;
        INSERT INTO [SectionTypesTbl]
        (
            [SectionID], [SectionType], [Notes]
        )
        SELECT
            NULLIF([SectionID], N'') AS [SectionID], NULLIF([SectionType], N'') AS [SectionType], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[SectionTypesTbl];
        SET IDENTITY_INSERT [SectionTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'SectionTypesTbl'))
            DBCC CHECKIDENT (N'SectionTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [SectionTypesTbl] from ' + N'[AccessSrc].[SectionTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'SectionTypesTbl') IS NOT NULL SET IDENTITY_INSERT [SectionTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [SectionTypesTbl] from ' + N'[AccessSrc].[SectionTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- SendCheckEmailTextsTbl -> SendCheckEmailTextsTbl
-- Mapping: columnsCount=6
--   SCEMTID -> SCEMTID
--   Header -> Header
--   Body -> Body
--   Footer -> Footer
--   DateLastChange -> DateLastChange
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.SendCheckEmailTextsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [SendCheckEmailTextsTbl]: missing source [AccessSrc].[SendCheckEmailTextsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [SendCheckEmailTextsTbl] ([SCEMTID], [Header], [Body], [Footer], [DateLastChange], [Notes]) SELECT NULLIF([SCEMTID], N'''') AS [SCEMTID], NULLIF([Header], N'''') AS [Header], NULLIF([Body], N'''') AS [Body], NULLIF([Footer], N'''') AS [Footer], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''))) AS [DateLastChange], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[SendCheckEmailTextsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [SendCheckEmailTextsTbl]
        (
            [SCEMTID], [Header], [Body], [Footer], [DateLastChange], [Notes]
        )
        SELECT
            NULLIF([SCEMTID], N'') AS [SCEMTID], NULLIF([Header], N'') AS [Header], NULLIF([Body], N'') AS [Body], NULLIF([Footer], N'') AS [Footer], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''))) AS [DateLastChange], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[SendCheckEmailTextsTbl];
        COMMIT;
        PRINT 'OK migrate [SendCheckEmailTextsTbl] from ' + N'[AccessSrc].[SendCheckEmailTextsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [SendCheckEmailTextsTbl] from ' + N'[AccessSrc].[SendCheckEmailTextsTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [SentRemindersLogTbl] ([ReminderID], [DateSentReminder], [NextPrepDate], [ReminderSent], [HadAutoFulfilItem]) SELECT NULLIF([ReminderID], N'''') AS [ReminderID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''))) AS [DateSentReminder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''))) AS [NextPrepDate], CASE WHEN NULLIF([ReminderSent], N'''') IS NULL THEN NULL WHEN NULLIF([ReminderSent], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([ReminderSent], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([ReminderSent], N'''')) END AS [ReminderSent], CASE WHEN NULLIF([HadAutoFulfilItem], N'''') IS NULL THEN NULL WHEN NULLIF([HadAutoFulfilItem], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([HadAutoFulfilItem], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([HadAutoFulfilItem], ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [SentRemindersLogTbl] ON;
        INSERT INTO [SentRemindersLogTbl]
        (
            [ReminderID], [DateSentReminder], [NextPrepDate], [ReminderSent], [HadAutoFulfilItem]
        )
        SELECT
            NULLIF([ReminderID], N'') AS [ReminderID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''))) AS [DateSentReminder], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''))) AS [NextPrepDate], CASE WHEN NULLIF([ReminderSent], N'') IS NULL THEN NULL WHEN NULLIF([ReminderSent], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([ReminderSent], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([ReminderSent], N'')) END AS [ReminderSent], CASE WHEN NULLIF([HadAutoFulfilItem], N'') IS NULL THEN NULL WHEN NULLIF([HadAutoFulfilItem], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([HadAutoFulfilItem], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([HadAutoFulfilItem], N'')) END AS [HadAutoFulfilItem]
        FROM [AccessSrc].[SentRemindersLogTbl];
        SET IDENTITY_INSERT [SentRemindersLogTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'SentRemindersLogTbl'))
            DBCC CHECKIDENT (N'SentRemindersLogTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [SentRemindersLogTbl] from ' + N'[AccessSrc].[SentRemindersLogTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'SentRemindersLogTbl') IS NOT NULL SET IDENTITY_INSERT [SentRemindersLogTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [SentRemindersLogTbl] from ' + N'[AccessSrc].[SentRemindersLogTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ServiceTypesTbl -> ServiceTypesTbl
-- Mapping: columnsCount=5
--   ServiceTypeId -> ServiceTypeId
--   ServiceType -> ServiceType
--   Description -> Description
--   PackagingID -> PackagingID
--   PrepTypeID -> PrepTypeID
IF OBJECT_ID(N'AccessSrc.ServiceTypesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ServiceTypesTbl]: missing source [AccessSrc].[ServiceTypesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ServiceTypesTbl] ([ServiceTypeId], [ServiceType], [Description], [PackagingID], [PrepTypeID]) SELECT NULLIF([ServiceTypeId], N'''') AS [ServiceTypeId], NULLIF([ServiceType], N'''') AS [ServiceType], NULLIF([Description], N'''') AS [Description], NULLIF([PackagingID], N'''') AS [PackagingID], NULLIF([PrepTypeID], N'''') AS [PrepTypeID] FROM [AccessSrc].[ServiceTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ServiceTypesTbl]
        (
            [ServiceTypeId], [ServiceType], [Description], [PackagingID], [PrepTypeID]
        )
        SELECT
            NULLIF([ServiceTypeId], N'') AS [ServiceTypeId], NULLIF([ServiceType], N'') AS [ServiceType], NULLIF([Description], N'') AS [Description], NULLIF([PackagingID], N'') AS [PackagingID], NULLIF([PrepTypeID], N'') AS [PrepTypeID]
        FROM [AccessSrc].[ServiceTypesTbl];
        COMMIT;
        PRINT 'OK migrate [ServiceTypesTbl] from ' + N'[AccessSrc].[ServiceTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ServiceTypesTbl] from ' + N'[AccessSrc].[ServiceTypesTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [SysDataTbl] ([ID], [LastReoccurringDate], [DoReoccuringOrders], [DateLastPrepDateCalcd], [MinReminderDate]) SELECT NULLIF([ID], N'''') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''))) AS [LastReoccurringDate], CASE WHEN NULLIF([DoReoccuringOrders], N'''') IS NULL THEN NULL WHEN NULLIF([DoReoccuringOrders], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([DoReoccuringOrders], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([DoReoccuringOrders], N'''')) END AS [DoReoccuringOrders], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''))) AS [DateLastPrepDateCalcd], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF( ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [SysDataTbl] ON;
        INSERT INTO [SysDataTbl]
        (
            [ID], [LastReoccurringDate], [DoReoccuringOrders], [DateLastPrepDateCalcd], [MinReminderDate]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''))) AS [LastReoccurringDate], CASE WHEN NULLIF([DoReoccuringOrders], N'') IS NULL THEN NULL WHEN NULLIF([DoReoccuringOrders], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([DoReoccuringOrders], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([DoReoccuringOrders], N'')) END AS [DoReoccuringOrders], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''))) AS [DateLastPrepDateCalcd], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''))) AS [MinReminderDate]
        FROM [AccessSrc].[SysDataTbl];
        SET IDENTITY_INSERT [SysDataTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'SysDataTbl'))
            DBCC CHECKIDENT (N'SysDataTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [SysDataTbl] from ' + N'[AccessSrc].[SysDataTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'SysDataTbl') IS NOT NULL SET IDENTITY_INSERT [SysDataTbl] OFF;
        END TRY BEGIN CATCH END CATCH
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TempCoffeecheckupCustomerTbl] ([TCCID], [CompanyName], [ContactFirstName], [ContactAltFirstName], [EmailAddress], [AltEmailAddress], [EquipTypeID], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [UsesFilter], [enabled], [AlwaysSendChkUp], [ReminderCount], [NextPrepDate], [NextDeliveryDate], [NextCoffee], [NextClean], [NextFilter], [NextDescal], [NextService], [RequiresPurchOrder]) SELECT NULLIF([TCCID], N'''') AS [TCCID], NULLIF([CompanyName], N'''') AS [CompanyName], NULLIF([ContactFirstName], N'''') AS [ContactFirstName], NULLIF([ContactAltFirstName], N'''') AS [ContactAltFirstName], NULLIF([EmailAddress], N'''') AS [EmailAddress], NULLIF([AltEmailAddress], N'''') AS [AltEmailAddress], NULLIF([EquipTypeID], N'''') AS [EquipTypeID], CASE WHEN NULLIF([TypicallySecToo], N'''') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([TypicallySecToo], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'''')) END AS [TypicallySecToo], NULLIF([PreferedAgentID], N'''') AS [PreferedAgentID], NULLIF([SalesAgentID], N'''') AS [SalesAgentID], CASE WHEN NULLIF([UsesFilter], N'''') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([UsesFilter], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'''')) END AS [ ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempCoffeecheckupCustomerTbl] ON;
        INSERT INTO [TempCoffeecheckupCustomerTbl]
        (
            [TCCID], [CompanyName], [ContactFirstName], [ContactAltFirstName], [EmailAddress], [AltEmailAddress], [EquipTypeID], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [UsesFilter], [enabled], [AlwaysSendChkUp], [ReminderCount], [NextPrepDate], [NextDeliveryDate], [NextCoffee], [NextClean], [NextFilter], [NextDescal], [NextService], [RequiresPurchOrder]
        )
        SELECT
            NULLIF([TCCID], N'') AS [TCCID], NULLIF([CompanyName], N'') AS [CompanyName], NULLIF([ContactFirstName], N'') AS [ContactFirstName], NULLIF([ContactAltFirstName], N'') AS [ContactAltFirstName], NULLIF([EmailAddress], N'') AS [EmailAddress], NULLIF([AltEmailAddress], N'') AS [AltEmailAddress], NULLIF([EquipTypeID], N'') AS [EquipTypeID], CASE WHEN NULLIF([TypicallySecToo], N'') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TypicallySecToo], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'')) END AS [TypicallySecToo], NULLIF([PreferedAgentID], N'') AS [PreferedAgentID], NULLIF([SalesAgentID], N'') AS [SalesAgentID], CASE WHEN NULLIF([UsesFilter], N'') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UsesFilter], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'')) END AS [UsesFilter], CASE WHEN NULLIF([enabled], N'') IS NULL THEN NULL WHEN NULLIF([enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([enabled], N'')) END AS [enabled], CASE WHEN NULLIF([AlwaysSendChkUp], N'') IS NULL THEN NULL WHEN NULLIF([AlwaysSendChkUp], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([AlwaysSendChkUp], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([AlwaysSendChkUp], N'')) END AS [AlwaysSendChkUp], NULLIF([ReminderCount], N'') AS [ReminderCount], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''))) AS [NextPrepDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''))) AS [NextDeliveryDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextCoffee], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextCoffee], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextCoffee], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextCoffee], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextCoffee], N''))) AS [NextCoffee], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextClean], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextClean], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextClean], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextClean], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextClean], N''))) AS [NextClean], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextFilter], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextFilter], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextFilter], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextFilter], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextFilter], N''))) AS [NextFilter], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDescal], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDescal], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDescal], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDescal], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDescal], N''))) AS [NextDescal], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextService], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextService], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextService], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextService], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextService], N''))) AS [NextService], CASE WHEN NULLIF([RequiresPurchOrder], N'') IS NULL THEN NULL WHEN NULLIF([RequiresPurchOrder], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([RequiresPurchOrder], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([RequiresPurchOrder], N'')) END AS [RequiresPurchOrder]
        FROM [AccessSrc].[TempCoffeecheckupCustomerTbl];
        SET IDENTITY_INSERT [TempCoffeecheckupCustomerTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TempCoffeecheckupCustomerTbl'))
            DBCC CHECKIDENT (N'TempCoffeecheckupCustomerTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TempCoffeecheckupCustomerTbl] from ' + N'[AccessSrc].[TempCoffeecheckupCustomerTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TempCoffeecheckupCustomerTbl') IS NOT NULL SET IDENTITY_INSERT [TempCoffeecheckupCustomerTbl] OFF;
        END TRY BEGIN CATCH END CATCH
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TempCoffeecheckupItemsTbl] ([TCIID], [ItemID], [ItemQty], [ItemPrepID], [AutoFulfill], [NextDateRequired]) SELECT NULLIF([TCIID], N'''') AS [TCIID], NULLIF([ItemID], N'''') AS [ItemID], NULLIF([ItemQty], N'''') AS [ItemQty], NULLIF([ItemPrepID], N'''') AS [ItemPrepID], CASE WHEN NULLIF([AutoFulfill], N'''') IS NULL THEN NULL WHEN NULLIF([AutoFulfill], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([AutoFulfill], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([AutoFulfill], N'''')) END AS [AutoFulfill], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''))) AS [NextDateRequired] FROM [AccessSrc].[TempCoffeecheckupItemsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempCoffeecheckupItemsTbl] ON;
        INSERT INTO [TempCoffeecheckupItemsTbl]
        (
            [TCIID], [ItemID], [ItemQty], [ItemPrepID], [AutoFulfill], [NextDateRequired]
        )
        SELECT
            NULLIF([TCIID], N'') AS [TCIID], NULLIF([ItemID], N'') AS [ItemID], NULLIF([ItemQty], N'') AS [ItemQty], NULLIF([ItemPrepID], N'') AS [ItemPrepID], CASE WHEN NULLIF([AutoFulfill], N'') IS NULL THEN NULL WHEN NULLIF([AutoFulfill], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([AutoFulfill], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([AutoFulfill], N'')) END AS [AutoFulfill], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''))) AS [NextDateRequired]
        FROM [AccessSrc].[TempCoffeecheckupItemsTbl];
        SET IDENTITY_INSERT [TempCoffeecheckupItemsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TempCoffeecheckupItemsTbl'))
            DBCC CHECKIDENT (N'TempCoffeecheckupItemsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TempCoffeecheckupItemsTbl] from ' + N'[AccessSrc].[TempCoffeecheckupItemsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TempCoffeecheckupItemsTbl') IS NOT NULL SET IDENTITY_INSERT [TempCoffeecheckupItemsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TempOrdersHeaderTbl] ([TOHeaderID], [OrderDate], [RoastDate], [RequiredByDate], [ToBeDeliveredByID], [Confirmed], [Done], [Notes]) SELECT NULLIF([TOHeaderID], N'''') AS [TOHeaderID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''))) AS [OrderDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''))) AS [RoastDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''))) AS [RequiredByDate], NULLIF([ToBeDeliveredByID], N'''') AS [ToBeDeliveredByID], CASE WHEN NULLIF([Confirmed], N'''') IS NULL THEN NULL WHEN NULLIF([Confirmed], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Confirmed], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Confirmed], N ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempOrdersHeaderTbl] ON;
        INSERT INTO [TempOrdersHeaderTbl]
        (
            [TOHeaderID], [OrderDate], [RoastDate], [RequiredByDate], [ToBeDeliveredByID], [Confirmed], [Done], [Notes]
        )
        SELECT
            NULLIF([TOHeaderID], N'') AS [TOHeaderID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''))) AS [OrderDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''))) AS [RoastDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''))) AS [RequiredByDate], NULLIF([ToBeDeliveredByID], N'') AS [ToBeDeliveredByID], CASE WHEN NULLIF([Confirmed], N'') IS NULL THEN NULL WHEN NULLIF([Confirmed], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Confirmed], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Confirmed], N'')) END AS [Confirmed], CASE WHEN NULLIF([Done], N'') IS NULL THEN NULL WHEN NULLIF([Done], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Done], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Done], N'')) END AS [Done], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TempOrdersHeaderTbl];
        SET IDENTITY_INSERT [TempOrdersHeaderTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TempOrdersHeaderTbl'))
            DBCC CHECKIDENT (N'TempOrdersHeaderTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TempOrdersHeaderTbl] from ' + N'[AccessSrc].[TempOrdersHeaderTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TempOrdersHeaderTbl') IS NOT NULL SET IDENTITY_INSERT [TempOrdersHeaderTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [TempOrdersHeaderTbl] from ' + N'[AccessSrc].[TempOrdersHeaderTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TempOrdersTbl] ([TempOrderId], [OrderID], [OrderDate], [RoastDate], [RequiredByDate], [Delivered], [Notes]) SELECT NULLIF([TempOrderId], N'''') AS [TempOrderId], NULLIF([OrderID], N'''') AS [OrderID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''))) AS [OrderDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''))) AS [RoastDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''))) AS [RequiredByDate], CASE WHEN NULLIF([Delivered], N'''') IS NULL THEN NULL WHEN NULLIF([Delivered], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Delivered], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Delivered], N'''')) END AS [Delivered], NULLIF([Notes],  ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempOrdersTbl] ON;
        INSERT INTO [TempOrdersTbl]
        (
            [TempOrderId], [OrderID], [OrderDate], [RoastDate], [RequiredByDate], [Delivered], [Notes]
        )
        SELECT
            NULLIF([TempOrderId], N'') AS [TempOrderId], NULLIF([OrderID], N'') AS [OrderID], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''))) AS [OrderDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''))) AS [RoastDate], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''))) AS [RequiredByDate], CASE WHEN NULLIF([Delivered], N'') IS NULL THEN NULL WHEN NULLIF([Delivered], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Delivered], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Delivered], N'')) END AS [Delivered], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TempOrdersTbl];
        SET IDENTITY_INSERT [TempOrdersTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TempOrdersTbl'))
            DBCC CHECKIDENT (N'TempOrdersTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TempOrdersTbl] from ' + N'[AccessSrc].[TempOrdersTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TempOrdersTbl') IS NOT NULL SET IDENTITY_INSERT [TempOrdersTbl] OFF;
        END TRY BEGIN CATCH END CATCH
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
    PRINT N'INSERT INTO [TotalCountTrackerTbl] ([CountDate], [TotalCount], [Comments]) SELECT COALESCE(TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''))) AS [CountDate], NULLIF([TotalCount], N'''') AS [TotalCount], NULLIF([Comments], N'''') AS [Comments] FROM [AccessSrc].[TotalCountTrackerTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TotalCountTrackerTbl]
        (
            [CountDate], [TotalCount], [Comments]
        )
        SELECT
            COALESCE(TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''))) AS [CountDate], NULLIF([TotalCount], N'') AS [TotalCount], NULLIF([Comments], N'') AS [Comments]
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

-- TrackedServiceItemTbl -> TrackedServiceItemTbl
-- Mapping: columnsCount=7
--   TrackerServiceItemID -> TrackerServiceItemID
--   ServiceTypeID -> ServiceTypeID
--   TypicalAvePerItem -> TypicalAvePerItem
--   UsageDateFieldName -> UsageDateFieldName
--   UsageAveFieldName -> UsageAveFieldName
--   ThisItemSetsDailyAverage -> ThisItemSetsDailyAverage
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.TrackedServiceItemTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TrackedServiceItemTbl]: missing source [AccessSrc].[TrackedServiceItemTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [TrackedServiceItemTbl] ([TrackerServiceItemID], [ServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]) SELECT NULLIF([TrackerServiceItemID], N'''') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'''') AS [ServiceTypeID], NULLIF([TypicalAvePerItem], N'''') AS [TypicalAvePerItem], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''))) AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'''') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'''') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[TrackedServiceItemTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [TrackedServiceItemTbl]
        (
            [TrackerServiceItemID], [ServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]
        )
        SELECT
            NULLIF([TrackerServiceItemID], N'') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'') AS [ServiceTypeID], NULLIF([TypicalAvePerItem], N'') AS [TypicalAvePerItem], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''))) AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TrackedServiceItemTbl];
        COMMIT;
        PRINT 'OK migrate [TrackedServiceItemTbl] from ' + N'[AccessSrc].[TrackedServiceItemTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [TrackedServiceItemTbl] from ' + N'[AccessSrc].[TrackedServiceItemTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TransactionTypesTbl] ([TransactionID], [TransactionType], [Notes]) SELECT NULLIF([TransactionID], N'''') AS [TransactionID], NULLIF([TransactionType], N'''') AS [TransactionType], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[TransactionTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TransactionTypesTbl] ON;
        INSERT INTO [TransactionTypesTbl]
        (
            [TransactionID], [TransactionType], [Notes]
        )
        SELECT
            NULLIF([TransactionID], N'') AS [TransactionID], NULLIF([TransactionType], N'') AS [TransactionType], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TransactionTypesTbl];
        SET IDENTITY_INSERT [TransactionTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TransactionTypesTbl'))
            DBCC CHECKIDENT (N'TransactionTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TransactionTypesTbl] from ' + N'[AccessSrc].[TransactionTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TransactionTypesTbl') IS NOT NULL SET IDENTITY_INSERT [TransactionTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [TransactionTypesTbl] from ' + N'[AccessSrc].[TransactionTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- UsedItemGroupTbl -> UsedItemGroupTbl
-- Mapping: columnsCount=7
--   UsedItemGroupID -> UsedItemGroupID
--   ContactID -> ContactID
--   GroupItemTypeID -> GroupItemTypeID
--   LastItemTypeID -> LastItemTypeID
--   LastItemTypeSortPos -> LastItemTypeSortPos
--   LastItemDateChanged -> LastItemDateChanged
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.UsedItemGroupTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [UsedItemGroupTbl]: missing source [AccessSrc].[UsedItemGroupTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [UsedItemGroupTbl] ([UsedItemGroupID], [ContactID], [GroupItemTypeID], [LastItemTypeID], [LastItemTypeSortPos], [LastItemDateChanged], [Notes]) SELECT NULLIF([UsedItemGroupID], N'''') AS [UsedItemGroupID], NULLIF([ContactID], N'''') AS [ContactID], NULLIF([GroupItemTypeID], N'''') AS [GroupItemTypeID], NULLIF([LastItemTypeID], N'''') AS [LastItemTypeID], NULLIF([LastItemTypeSortPos], N'''') AS [LastItemTypeSortPos], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''))) AS [LastItemDateChanged], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[UsedItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [UsedItemGroupTbl]
        (
            [UsedItemGroupID], [ContactID], [GroupItemTypeID], [LastItemTypeID], [LastItemTypeSortPos], [LastItemDateChanged], [Notes]
        )
        SELECT
            NULLIF([UsedItemGroupID], N'') AS [UsedItemGroupID], NULLIF([ContactID], N'') AS [ContactID], NULLIF([GroupItemTypeID], N'') AS [GroupItemTypeID], NULLIF([LastItemTypeID], N'') AS [LastItemTypeID], NULLIF([LastItemTypeSortPos], N'') AS [LastItemTypeSortPos], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''))) AS [LastItemDateChanged], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[UsedItemGroupTbl];
        COMMIT;
        PRINT 'OK migrate [UsedItemGroupTbl] from ' + N'[AccessSrc].[UsedItemGroupTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [UsedItemGroupTbl] from ' + N'[AccessSrc].[UsedItemGroupTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [RepairsTbl] ([RepairID], [ContactName], [ContactEmail], [JobCardNumber], [DateLogged], [LastStatusChange], [SwopOutMachineID], [TakenFrother], [TakenBeanLid], [TakenWaterLid], [BrokenFrother], [BrokenBeanLid], [BrokenWaterLid], [RepairFaultID], [RepairFaultDesc], [RepairStatusID], [RelatedOrderID], [Notes]) SELECT NULLIF([RepairID], N'''') AS [RepairID], NULLIF([ContactName], N'''') AS [ContactName], NULLIF([ContactEmail], N'''') AS [ContactEmail], NULLIF([JobCardNumber], N'''') AS [JobCardNumber], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''))) AS [DateLogged], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''''))) AS [LastStatusChange], NULLIF([SwopOutMachineID], N'''') AS [SwopOutMachineID], CASE WHEN NULLIF([TakenFrother], N'''') IS NULL THEN NULL WHEN NULLIF([TakenFrother], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([TakenFrother], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONV ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RepairsTbl] ON;
        INSERT INTO [RepairsTbl]
        (
            [RepairID], [ContactName], [ContactEmail], [JobCardNumber], [DateLogged], [LastStatusChange], [SwopOutMachineID], [TakenFrother], [TakenBeanLid], [TakenWaterLid], [BrokenFrother], [BrokenBeanLid], [BrokenWaterLid], [RepairFaultID], [RepairFaultDesc], [RepairStatusID], [RelatedOrderID], [Notes]
        )
        SELECT
            NULLIF([RepairID], N'') AS [RepairID], NULLIF([ContactName], N'') AS [ContactName], NULLIF([ContactEmail], N'') AS [ContactEmail], NULLIF([JobCardNumber], N'') AS [JobCardNumber], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''))) AS [DateLogged], COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''))) AS [LastStatusChange], NULLIF([SwopOutMachineID], N'') AS [SwopOutMachineID], CASE WHEN NULLIF([TakenFrother], N'') IS NULL THEN NULL WHEN NULLIF([TakenFrother], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TakenFrother], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TakenFrother], N'')) END AS [TakenFrother], CASE WHEN NULLIF([TakenBeanLid], N'') IS NULL THEN NULL WHEN NULLIF([TakenBeanLid], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TakenBeanLid], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TakenBeanLid], N'')) END AS [TakenBeanLid], CASE WHEN NULLIF([TakenWaterLid], N'') IS NULL THEN NULL WHEN NULLIF([TakenWaterLid], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TakenWaterLid], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TakenWaterLid], N'')) END AS [TakenWaterLid], CASE WHEN NULLIF([BrokenFrother], N'') IS NULL THEN NULL WHEN NULLIF([BrokenFrother], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([BrokenFrother], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([BrokenFrother], N'')) END AS [BrokenFrother], CASE WHEN NULLIF([BrokenBeanLid], N'') IS NULL THEN NULL WHEN NULLIF([BrokenBeanLid], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([BrokenBeanLid], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([BrokenBeanLid], N'')) END AS [BrokenBeanLid], CASE WHEN NULLIF([BrokenWaterLid], N'') IS NULL THEN NULL WHEN NULLIF([BrokenWaterLid], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([BrokenWaterLid], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([BrokenWaterLid], N'')) END AS [BrokenWaterLid], NULLIF([RepairFaultID], N'') AS [RepairFaultID], NULLIF([RepairFaultDesc], N'') AS [RepairFaultDesc], NULLIF([RepairStatusID], N'') AS [RepairStatusID], NULLIF([RelatedOrderID], N'') AS [RelatedOrderID], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[RepairsTbl];
        SET IDENTITY_INSERT [RepairsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'RepairsTbl'))
            DBCC CHECKIDENT (N'RepairsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [RepairsTbl] from ' + N'[AccessSrc].[RepairsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'RepairsTbl') IS NOT NULL SET IDENTITY_INSERT [RepairsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [RepairsTbl] from ' + N'[AccessSrc].[RepairsTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TempOrdersLinesTbl] ([TOLineID], [TOHeaderID], [ItemID], [Qty], [OriginalOrderID]) SELECT NULLIF([TOLineID], N'''') AS [TOLineID], NULLIF([TOHeaderID], N'''') AS [TOHeaderID], NULLIF([ItemID], N'''') AS [ItemID], NULLIF([Qty], N'''') AS [Qty], NULLIF([OriginalOrderID], N'''') AS [OriginalOrderID] FROM [AccessSrc].[TempOrdersLinesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempOrdersLinesTbl] ON;
        INSERT INTO [TempOrdersLinesTbl]
        (
            [TOLineID], [TOHeaderID], [ItemID], [Qty], [OriginalOrderID]
        )
        SELECT
            NULLIF([TOLineID], N'') AS [TOLineID], NULLIF([TOHeaderID], N'') AS [TOHeaderID], NULLIF([ItemID], N'') AS [ItemID], NULLIF([Qty], N'') AS [Qty], NULLIF([OriginalOrderID], N'') AS [OriginalOrderID]
        FROM [AccessSrc].[TempOrdersLinesTbl];
        SET IDENTITY_INSERT [TempOrdersLinesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TempOrdersLinesTbl'))
            DBCC CHECKIDENT (N'TempOrdersLinesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TempOrdersLinesTbl] from ' + N'[AccessSrc].[TempOrdersLinesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TempOrdersLinesTbl') IS NOT NULL SET IDENTITY_INSERT [TempOrdersLinesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [TempOrdersLinesTbl] from ' + N'[AccessSrc].[TempOrdersLinesTbl]' + ': ' + ERROR_MESSAGE();
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
WHERE t.name IN (N'AwayReasonTbl', N'CityPrepDaysTbl', N'CityTbl', N'ClientAwayPeriodTbl', N'ClientUsageLinesTbl', N'ClientUsageTbl', N'ClosureDatesTbl', N'CustomersAccInfoTbl', N'CustomersTbl', N'CustomerTrackedServiceItemsTbl', N'CustomerTypeTbl', N'EquipTypeTbl', N'HolidayClosureTbl', N'InvoiceTypeTbl', N'ItemGroupTbl', N'ItemPackagingsTbl', N'ItemTypeTbl', N'ItemUnitsTbl', N'ItemUsageTbl', N'MachineConditionsTbl', N'NextRoastDateByCityTbl', N'PaymentTermsTbl', N'PersonsTbl', N'PrepTypesTbl', N'PriceLevelsTbl', N'ReoccuranceTypeTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'SectionTypesTbl', N'SendCheckEmailTextsTbl', N'SentRemindersLogTbl', N'ServiceTypesTbl', N'SysDataTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersTbl', N'TotalCountTrackerTbl', N'TrackedServiceItemTbl', N'TransactionTypesTbl', N'UsedItemGroupTbl', N'RepairsTbl', N'TempOrdersLinesTbl');
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
WHERE tp.name IN (N'AwayReasonTbl', N'CityPrepDaysTbl', N'CityTbl', N'ClientAwayPeriodTbl', N'ClientUsageLinesTbl', N'ClientUsageTbl', N'ClosureDatesTbl', N'CustomersAccInfoTbl', N'CustomersTbl', N'CustomerTrackedServiceItemsTbl', N'CustomerTypeTbl', N'EquipTypeTbl', N'HolidayClosureTbl', N'InvoiceTypeTbl', N'ItemGroupTbl', N'ItemPackagingsTbl', N'ItemTypeTbl', N'ItemUnitsTbl', N'ItemUsageTbl', N'MachineConditionsTbl', N'NextRoastDateByCityTbl', N'PaymentTermsTbl', N'PersonsTbl', N'PrepTypesTbl', N'PriceLevelsTbl', N'ReoccuranceTypeTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'SectionTypesTbl', N'SendCheckEmailTextsTbl', N'SentRemindersLogTbl', N'ServiceTypesTbl', N'SysDataTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersTbl', N'TotalCountTrackerTbl', N'TrackedServiceItemTbl', N'TransactionTypesTbl', N'UsedItemGroupTbl', N'RepairsTbl', N'TempOrdersLinesTbl') AND (fk.is_not_trusted = 1 OR fk.is_disabled = 1);
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

