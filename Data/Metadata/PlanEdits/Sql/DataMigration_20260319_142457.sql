-- Auto-generated DATA MIGRATION script
-- Assumes source data is available under schema [AccessSrc] using Access source table names.
-- If you do not have [AccessSrc] objects, the generator will fall back to unqualified [Source] when Target != Source.
-- If Target == Source and AccessSrc.Source is missing, the table is skipped (no self-select).
-- Create schema once if needed: IF SCHEMA_ID('AccessSrc') IS NULL EXEC('CREATE SCHEMA AccessSrc');
-- AccessSchema: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema
-- PlanConstraints: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\PlanConstraints.json
-- Tables to migrate (ordered): 45
--   - AreaPrepDaysTbl
--   - AreasTbl
--   - AwayReasonTbl
--   - ClosureDatesTbl
--   - ContactsAccInfoTbl
--   - ContactsAwayPeriodTbl
--   - ContactsItemsPredictedTbl
--   - ContactsItemSvcSummaryTbl
--   - ContactsItemUsageTbl
--   - ContactsTbl
--   - ContactTrackedServiceItemsTbl
--   - ContactTypesTbl
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
--   - OrdersTbl
--   - PaymentTermsTbl
--   - PeopleTbl
--   - PriceLevelsTbl
--   - RecurranceTypesTbl
--   - RecurringOrdersTbl
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
WHERE t.name IN (N'AreasTbl', N'AwayReasonTbl', N'ClosureDatesTbl', N'ContactTypesTbl', N'EquipConditionsTbl', N'EquipTypesTbl', N'HolidayClosuresTbl', N'InvoiceTypesTbl', N'ItemPackagingsTbl', N'ItemPrepTypesTbl', N'ItemUnitsTbl', N'OrdersTbl', N'PaymentTermsTbl', N'PeopleTbl', N'PriceLevelsTbl', N'RecurranceTypesTbl', N'RecurringOrdersTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'SectionTypesTbl', N'SendCheckupEmailTextsTbl', N'TotalCountTrackerTbl', N'TransactionTypesTbl', N'AreaPrepDaysTbl', N'NextPrepDateByAreasTbl', N'ItemServiceTypesTbl', N'ItemsTbl', N'TrackedServiceItemsTbl', N'ContactsTbl', N'ItemGroupsTbl', N'SysDataTbl', N'ContactsAwayPeriodTbl', N'ContactsItemSvcSummaryTbl', N'ContactsItemsPredictedTbl', N'ContactsAccInfoTbl', N'ContactTrackedServiceItemsTbl', N'ContactsItemUsageTbl', N'RepairsTbl', N'SentRemindersLogTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersTbl', N'UsedItemGroupsTbl', N'TempOrdersLinesTbl');
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
-- Using DELETE instead of TRUNCATE because SQL Server TRUNCATE checks for FK existence even when disabled
PRINT N'Purging [TempOrdersLinesTbl]';
BEGIN TRY
    DELETE FROM [TempOrdersLinesTbl];
    PRINT N'Successfully purged [TempOrdersLinesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [TempOrdersLinesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [UsedItemGroupsTbl]';
BEGIN TRY
    DELETE FROM [UsedItemGroupsTbl];
    PRINT N'Successfully purged [UsedItemGroupsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [UsedItemGroupsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [TempOrdersTbl]';
BEGIN TRY
    DELETE FROM [TempOrdersTbl];
    PRINT N'Successfully purged [TempOrdersTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [TempOrdersTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [TempOrdersHeaderTbl]';
BEGIN TRY
    DELETE FROM [TempOrdersHeaderTbl];
    PRINT N'Successfully purged [TempOrdersHeaderTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [TempOrdersHeaderTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [TempCoffeecheckupItemsTbl]';
BEGIN TRY
    DELETE FROM [TempCoffeecheckupItemsTbl];
    PRINT N'Successfully purged [TempCoffeecheckupItemsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [TempCoffeecheckupItemsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [TempCoffeecheckupCustomerTbl]';
BEGIN TRY
    DELETE FROM [TempCoffeecheckupCustomerTbl];
    PRINT N'Successfully purged [TempCoffeecheckupCustomerTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [TempCoffeecheckupCustomerTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [SentRemindersLogTbl]';
BEGIN TRY
    DELETE FROM [SentRemindersLogTbl];
    PRINT N'Successfully purged [SentRemindersLogTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [SentRemindersLogTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [RepairsTbl]';
BEGIN TRY
    DELETE FROM [RepairsTbl];
    PRINT N'Successfully purged [RepairsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [RepairsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ContactsItemUsageTbl]';
BEGIN TRY
    DELETE FROM [ContactsItemUsageTbl];
    PRINT N'Successfully purged [ContactsItemUsageTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ContactsItemUsageTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ContactTrackedServiceItemsTbl]';
BEGIN TRY
    DELETE FROM [ContactTrackedServiceItemsTbl];
    PRINT N'Successfully purged [ContactTrackedServiceItemsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ContactTrackedServiceItemsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ContactsAccInfoTbl]';
BEGIN TRY
    DELETE FROM [ContactsAccInfoTbl];
    PRINT N'Successfully purged [ContactsAccInfoTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ContactsAccInfoTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ContactsItemsPredictedTbl]';
BEGIN TRY
    DELETE FROM [ContactsItemsPredictedTbl];
    PRINT N'Successfully purged [ContactsItemsPredictedTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ContactsItemsPredictedTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ContactsItemSvcSummaryTbl]';
BEGIN TRY
    DELETE FROM [ContactsItemSvcSummaryTbl];
    PRINT N'Successfully purged [ContactsItemSvcSummaryTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ContactsItemSvcSummaryTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ContactsAwayPeriodTbl]';
BEGIN TRY
    DELETE FROM [ContactsAwayPeriodTbl];
    PRINT N'Successfully purged [ContactsAwayPeriodTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ContactsAwayPeriodTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [SysDataTbl]';
BEGIN TRY
    DELETE FROM [SysDataTbl];
    PRINT N'Successfully purged [SysDataTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [SysDataTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ItemGroupsTbl]';
BEGIN TRY
    DELETE FROM [ItemGroupsTbl];
    PRINT N'Successfully purged [ItemGroupsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ItemGroupsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ContactsTbl]';
BEGIN TRY
    DELETE FROM [ContactsTbl];
    PRINT N'Successfully purged [ContactsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ContactsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [TrackedServiceItemsTbl]';
BEGIN TRY
    DELETE FROM [TrackedServiceItemsTbl];
    PRINT N'Successfully purged [TrackedServiceItemsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [TrackedServiceItemsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ItemsTbl]';
BEGIN TRY
    DELETE FROM [ItemsTbl];
    PRINT N'Successfully purged [ItemsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ItemsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ItemServiceTypesTbl]';
BEGIN TRY
    DELETE FROM [ItemServiceTypesTbl];
    PRINT N'Successfully purged [ItemServiceTypesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ItemServiceTypesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [NextPrepDateByAreasTbl]';
BEGIN TRY
    DELETE FROM [NextPrepDateByAreasTbl];
    PRINT N'Successfully purged [NextPrepDateByAreasTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [NextPrepDateByAreasTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [AreaPrepDaysTbl]';
BEGIN TRY
    DELETE FROM [AreaPrepDaysTbl];
    PRINT N'Successfully purged [AreaPrepDaysTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [AreaPrepDaysTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [TransactionTypesTbl]';
BEGIN TRY
    DELETE FROM [TransactionTypesTbl];
    PRINT N'Successfully purged [TransactionTypesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [TransactionTypesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [TotalCountTrackerTbl]';
BEGIN TRY
    DELETE FROM [TotalCountTrackerTbl];
    PRINT N'Successfully purged [TotalCountTrackerTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [TotalCountTrackerTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [SendCheckupEmailTextsTbl]';
BEGIN TRY
    DELETE FROM [SendCheckupEmailTextsTbl];
    PRINT N'Successfully purged [SendCheckupEmailTextsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [SendCheckupEmailTextsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [SectionTypesTbl]';
BEGIN TRY
    DELETE FROM [SectionTypesTbl];
    PRINT N'Successfully purged [SectionTypesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [SectionTypesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [RepairStatusesTbl]';
BEGIN TRY
    DELETE FROM [RepairStatusesTbl];
    PRINT N'Successfully purged [RepairStatusesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [RepairStatusesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [RepairFaultsTbl]';
BEGIN TRY
    DELETE FROM [RepairFaultsTbl];
    PRINT N'Successfully purged [RepairFaultsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [RepairFaultsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [RecurringOrdersTbl]';
BEGIN TRY
    DELETE FROM [RecurringOrdersTbl];
    PRINT N'Successfully purged [RecurringOrdersTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [RecurringOrdersTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [RecurranceTypesTbl]';
BEGIN TRY
    DELETE FROM [RecurranceTypesTbl];
    PRINT N'Successfully purged [RecurranceTypesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [RecurranceTypesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [PriceLevelsTbl]';
BEGIN TRY
    DELETE FROM [PriceLevelsTbl];
    PRINT N'Successfully purged [PriceLevelsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [PriceLevelsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [PeopleTbl]';
BEGIN TRY
    DELETE FROM [PeopleTbl];
    PRINT N'Successfully purged [PeopleTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [PeopleTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [PaymentTermsTbl]';
BEGIN TRY
    DELETE FROM [PaymentTermsTbl];
    PRINT N'Successfully purged [PaymentTermsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [PaymentTermsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [OrdersTbl]';
BEGIN TRY
    DELETE FROM [OrdersTbl];
    PRINT N'Successfully purged [OrdersTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [OrdersTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ItemUnitsTbl]';
BEGIN TRY
    DELETE FROM [ItemUnitsTbl];
    PRINT N'Successfully purged [ItemUnitsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ItemUnitsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ItemPrepTypesTbl]';
BEGIN TRY
    DELETE FROM [ItemPrepTypesTbl];
    PRINT N'Successfully purged [ItemPrepTypesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ItemPrepTypesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ItemPackagingsTbl]';
BEGIN TRY
    DELETE FROM [ItemPackagingsTbl];
    PRINT N'Successfully purged [ItemPackagingsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ItemPackagingsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [InvoiceTypesTbl]';
BEGIN TRY
    DELETE FROM [InvoiceTypesTbl];
    PRINT N'Successfully purged [InvoiceTypesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [InvoiceTypesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [HolidayClosuresTbl]';
BEGIN TRY
    DELETE FROM [HolidayClosuresTbl];
    PRINT N'Successfully purged [HolidayClosuresTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [HolidayClosuresTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [EquipTypesTbl]';
BEGIN TRY
    DELETE FROM [EquipTypesTbl];
    PRINT N'Successfully purged [EquipTypesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [EquipTypesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [EquipConditionsTbl]';
BEGIN TRY
    DELETE FROM [EquipConditionsTbl];
    PRINT N'Successfully purged [EquipConditionsTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [EquipConditionsTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ContactTypesTbl]';
BEGIN TRY
    DELETE FROM [ContactTypesTbl];
    PRINT N'Successfully purged [ContactTypesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ContactTypesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [ClosureDatesTbl]';
BEGIN TRY
    DELETE FROM [ClosureDatesTbl];
    PRINT N'Successfully purged [ClosureDatesTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [ClosureDatesTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [AwayReasonTbl]';
BEGIN TRY
    DELETE FROM [AwayReasonTbl];
    PRINT N'Successfully purged [AwayReasonTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [AwayReasonTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
PRINT N'Purging [AreasTbl]';
BEGIN TRY
    DELETE FROM [AreasTbl];
    PRINT N'Successfully purged [AreasTbl]';
END TRY
BEGIN CATCH
    PRINT N'ERROR: Failed to purge [AreasTbl]: ' + ERROR_MESSAGE();
    PRINT N'      This may indicate foreign keys were not properly disabled.';
END CATCH
GO

-- CityTbl -> AreasTbl
-- Mapping: columnsCount=4
--   ID -> AreaID
--   City -> Area
--   RoastingDay -> PrepDayOfWeekID
--   DeliveryDelay -> DeliveryDelay
IF OBJECT_ID(N'AccessSrc.CityTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [AreasTbl] ([AreaID], [Area], [PrepDayOfWeekID], [DeliveryDelay]) SELECT NULLIF([ID], N'''') AS [AreaID], NULLIF([City], N'''') AS [Area], NULLIF([RoastingDay], N'''') AS [PrepDayOfWeekID], NULLIF([DeliveryDelay], N'''') AS [DeliveryDelay] FROM [AccessSrc].[CityTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [AreasTbl] ON;
        INSERT INTO [AreasTbl]
        (
            [AreaID], [Area], [PrepDayOfWeekID], [DeliveryDelay]
        )
        SELECT
            NULLIF([ID], N'') AS [AreaID], NULLIF([City], N'') AS [Area], NULLIF([RoastingDay], N'') AS [PrepDayOfWeekID], NULLIF([DeliveryDelay], N'') AS [DeliveryDelay]
        FROM [AccessSrc].[CityTbl];
        SET IDENTITY_INSERT [AreasTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'AreasTbl'))
            DBCC CHECKIDENT (N'AreasTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [AreasTbl] from ' + N'[AccessSrc].[CityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'AreasTbl') IS NOT NULL SET IDENTITY_INSERT [AreasTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [AreasTbl] from ' + N'[AccessSrc].[CityTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [AreasTbl] ([AreaID], [Area], [PrepDayOfWeekID], [DeliveryDelay]) SELECT NULLIF([ID], N'''') AS [AreaID], NULLIF([City], N'''') AS [Area], NULLIF([RoastingDay], N'''') AS [PrepDayOfWeekID], NULLIF([DeliveryDelay], N'''') AS [DeliveryDelay] FROM [CityTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [AreasTbl] ON;
        INSERT INTO [AreasTbl]
        (
            [AreaID], [Area], [PrepDayOfWeekID], [DeliveryDelay]
        )
        SELECT
            NULLIF([ID], N'') AS [AreaID], NULLIF([City], N'') AS [Area], NULLIF([RoastingDay], N'') AS [PrepDayOfWeekID], NULLIF([DeliveryDelay], N'') AS [DeliveryDelay]
        FROM [CityTbl];
        SET IDENTITY_INSERT [AreasTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'AreasTbl'))
            DBCC CHECKIDENT (N'AreasTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [AreasTbl] from ' + N'[CityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'AreasTbl') IS NOT NULL SET IDENTITY_INSERT [AreasTbl] OFF;
        END TRY BEGIN CATCH END CATCH
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
--   ID -> ClosureDateID
--   DateClosed -> DateClosed
--   DateReopen -> DateReopen
--   NextRoastDate -> NextPrepDate
--   Comments -> Comments
IF OBJECT_ID(N'AccessSrc.ClosureDatesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [ClosureDatesTbl]: missing source [AccessSrc].[ClosureDatesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ClosureDatesTbl] ([ClosureDateID], [DateClosed], [DateReopen], [NextPrepDate], [Comments]) SELECT NULLIF([ID], N'''') AS [ClosureDateID], CASE WHEN NULLIF([DateClosed], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateClosed], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N'''')), CAST(NULL AS datetime2(7))) END AS [DateClosed], CASE WHEN NULLIF([DateReopen], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateReopen], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N'''')), CAST(NULL AS datetime2(7))) END AS [DateReopen], CASE WHEN NULLIF([NextRoastDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextRoastDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''''), 121), TRY ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ClosureDatesTbl] ON;
        INSERT INTO [ClosureDatesTbl]
        (
            [ClosureDateID], [DateClosed], [DateReopen], [NextPrepDate], [Comments]
        )
        SELECT
            NULLIF([ID], N'') AS [ClosureDateID], CASE WHEN NULLIF([DateClosed], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateClosed], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateClosed], N'')), CAST(NULL AS datetime2(7))) END AS [DateClosed], CASE WHEN NULLIF([DateReopen], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateReopen], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateReopen], N'')), CAST(NULL AS datetime2(7))) END AS [DateReopen], CASE WHEN NULLIF([NextRoastDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextRoastDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextRoastDate], N'')), CAST(NULL AS datetime2(7))) END AS [NextPrepDate], NULLIF([Comments], N'') AS [Comments]
        FROM [AccessSrc].[ClosureDatesTbl];
        SET IDENTITY_INSERT [ClosureDatesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ClosureDatesTbl'))
            DBCC CHECKIDENT (N'ClosureDatesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ClosureDatesTbl] from ' + N'[AccessSrc].[ClosureDatesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ClosureDatesTbl') IS NOT NULL SET IDENTITY_INSERT [ClosureDatesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ClosureDatesTbl] from ' + N'[AccessSrc].[ClosureDatesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomerTypeTbl -> ContactTypesTbl
-- Mapping: columnsCount=3
--   CustTypeID -> ContactTypeID
--   CustTypeDesc -> ContactTypeDesc
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.CustomerTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactTypesTbl] ([ContactTypeID], [ContactTypeDesc], [Notes]) SELECT NULLIF([CustTypeID], N'''') AS [ContactTypeID], NULLIF([CustTypeDesc], N'''') AS [ContactTypeDesc], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[CustomerTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactTypesTbl] ON;
        INSERT INTO [ContactTypesTbl]
        (
            [ContactTypeID], [ContactTypeDesc], [Notes]
        )
        SELECT
            NULLIF([CustTypeID], N'') AS [ContactTypeID], NULLIF([CustTypeDesc], N'') AS [ContactTypeDesc], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomerTypeTbl];
        SET IDENTITY_INSERT [ContactTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactTypesTbl'))
            DBCC CHECKIDENT (N'ContactTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactTypesTbl] from ' + N'[AccessSrc].[CustomerTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactTypesTbl') IS NOT NULL SET IDENTITY_INSERT [ContactTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactTypesTbl] from ' + N'[AccessSrc].[CustomerTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactTypesTbl] ([ContactTypeID], [ContactTypeDesc], [Notes]) SELECT NULLIF([CustTypeID], N'''') AS [ContactTypeID], NULLIF([CustTypeDesc], N'''') AS [ContactTypeDesc], NULLIF([Notes], N'''') AS [Notes] FROM [CustomerTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactTypesTbl] ON;
        INSERT INTO [ContactTypesTbl]
        (
            [ContactTypeID], [ContactTypeDesc], [Notes]
        )
        SELECT
            NULLIF([CustTypeID], N'') AS [ContactTypeID], NULLIF([CustTypeDesc], N'') AS [ContactTypeDesc], NULLIF([Notes], N'') AS [Notes]
        FROM [CustomerTypeTbl];
        SET IDENTITY_INSERT [ContactTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactTypesTbl'))
            DBCC CHECKIDENT (N'ContactTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactTypesTbl] from ' + N'[CustomerTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactTypesTbl') IS NOT NULL SET IDENTITY_INSERT [ContactTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactTypesTbl] from ' + N'[CustomerTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- MachineConditionsTbl -> EquipConditionsTbl
-- Mapping: columnsCount=4
--   MachineConditionID -> EquipConditionID
--   ConditionDesc -> ConditionDesc
--   SortOrder -> SortOrder
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.MachineConditionsTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [EquipConditionsTbl] ([EquipConditionID], [ConditionDesc], [SortOrder], [Notes]) SELECT NULLIF([MachineConditionID], N'''') AS [EquipConditionID], NULLIF([ConditionDesc], N'''') AS [ConditionDesc], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[MachineConditionsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [EquipConditionsTbl] ON;
        INSERT INTO [EquipConditionsTbl]
        (
            [EquipConditionID], [ConditionDesc], [SortOrder], [Notes]
        )
        SELECT
            NULLIF([MachineConditionID], N'') AS [EquipConditionID], NULLIF([ConditionDesc], N'') AS [ConditionDesc], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[MachineConditionsTbl];
        SET IDENTITY_INSERT [EquipConditionsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'EquipConditionsTbl'))
            DBCC CHECKIDENT (N'EquipConditionsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [EquipConditionsTbl] from ' + N'[AccessSrc].[MachineConditionsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'EquipConditionsTbl') IS NOT NULL SET IDENTITY_INSERT [EquipConditionsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [EquipConditionsTbl] from ' + N'[AccessSrc].[MachineConditionsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [EquipConditionsTbl] ([EquipConditionID], [ConditionDesc], [SortOrder], [Notes]) SELECT NULLIF([MachineConditionID], N'''') AS [EquipConditionID], NULLIF([ConditionDesc], N'''') AS [ConditionDesc], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes] FROM [MachineConditionsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [EquipConditionsTbl] ON;
        INSERT INTO [EquipConditionsTbl]
        (
            [EquipConditionID], [ConditionDesc], [SortOrder], [Notes]
        )
        SELECT
            NULLIF([MachineConditionID], N'') AS [EquipConditionID], NULLIF([ConditionDesc], N'') AS [ConditionDesc], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes]
        FROM [MachineConditionsTbl];
        SET IDENTITY_INSERT [EquipConditionsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'EquipConditionsTbl'))
            DBCC CHECKIDENT (N'EquipConditionsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [EquipConditionsTbl] from ' + N'[MachineConditionsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'EquipConditionsTbl') IS NOT NULL SET IDENTITY_INSERT [EquipConditionsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [EquipConditionsTbl] from ' + N'[MachineConditionsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- EquipTypeTbl -> EquipTypesTbl
-- Mapping: columnsCount=3
--   EquipTypeId -> EquipTypeID
--   EquipTypeName -> EquipTypeName
--   EquipTypeDesc -> EquipTypeDesc
IF OBJECT_ID(N'AccessSrc.EquipTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [EquipTypesTbl] ([EquipTypeID], [EquipTypeName], [EquipTypeDesc]) SELECT NULLIF([EquipTypeId], N'''') AS [EquipTypeID], NULLIF([EquipTypeName], N'''') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'''') AS [EquipTypeDesc] FROM [AccessSrc].[EquipTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [EquipTypesTbl] ON;
        INSERT INTO [EquipTypesTbl]
        (
            [EquipTypeID], [EquipTypeName], [EquipTypeDesc]
        )
        SELECT
            NULLIF([EquipTypeId], N'') AS [EquipTypeID], NULLIF([EquipTypeName], N'') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'') AS [EquipTypeDesc]
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
    PRINT N'INSERT INTO [EquipTypesTbl] ([EquipTypeID], [EquipTypeName], [EquipTypeDesc]) SELECT NULLIF([EquipTypeId], N'''') AS [EquipTypeID], NULLIF([EquipTypeName], N'''') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'''') AS [EquipTypeDesc] FROM [EquipTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [EquipTypesTbl] ON;
        INSERT INTO [EquipTypesTbl]
        (
            [EquipTypeID], [EquipTypeName], [EquipTypeDesc]
        )
        SELECT
            NULLIF([EquipTypeId], N'') AS [EquipTypeID], NULLIF([EquipTypeName], N'') AS [EquipTypeName], NULLIF([EquipTypeDesc], N'') AS [EquipTypeDesc]
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
--   ID -> HolidayClosureID
--   ClosureDate -> ClosureDate
--   DaysClosed -> DaysClosed
--   AppliesToPrep -> AppliesToPrep
--   AppliesToDelivery -> AppliesToDelivery
--   ShiftStrategy -> ShiftStrategy
--   Description -> Description
IF OBJECT_ID(N'AccessSrc.HolidayClosureTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [HolidayClosuresTbl] ([HolidayClosureID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]) SELECT NULLIF([ID], N'''') AS [HolidayClosureID], CASE WHEN NULLIF([ClosureDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([ClosureDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N'''')), CAST(NULL AS datetime2(7))) END AS [ClosureDate], NULLIF([DaysClosed], N'''') AS [DaysClosed], NULLIF([AppliesToPrep], N'''') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'''') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'''') AS [ShiftStrategy], NULLIF([Description], N'''') AS [Description] FROM [AccessSrc].[HolidayClosureTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [HolidayClosuresTbl] ON;
        INSERT INTO [HolidayClosuresTbl]
        (
            [HolidayClosureID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]
        )
        SELECT
            NULLIF([ID], N'') AS [HolidayClosureID], CASE WHEN NULLIF([ClosureDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([ClosureDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N'')), CAST(NULL AS datetime2(7))) END AS [ClosureDate], NULLIF([DaysClosed], N'') AS [DaysClosed], NULLIF([AppliesToPrep], N'') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'') AS [ShiftStrategy], NULLIF([Description], N'') AS [Description]
        FROM [AccessSrc].[HolidayClosureTbl];
        SET IDENTITY_INSERT [HolidayClosuresTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'HolidayClosuresTbl'))
            DBCC CHECKIDENT (N'HolidayClosuresTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [HolidayClosuresTbl] from ' + N'[AccessSrc].[HolidayClosureTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'HolidayClosuresTbl') IS NOT NULL SET IDENTITY_INSERT [HolidayClosuresTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [HolidayClosuresTbl] from ' + N'[AccessSrc].[HolidayClosureTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [HolidayClosuresTbl] ([HolidayClosureID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]) SELECT NULLIF([ID], N'''') AS [HolidayClosureID], CASE WHEN NULLIF([ClosureDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([ClosureDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N'''')), CAST(NULL AS datetime2(7))) END AS [ClosureDate], NULLIF([DaysClosed], N'''') AS [DaysClosed], NULLIF([AppliesToPrep], N'''') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'''') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'''') AS [ShiftStrategy], NULLIF([Description], N'''') AS [Description] FROM [HolidayClosureTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [HolidayClosuresTbl] ON;
        INSERT INTO [HolidayClosuresTbl]
        (
            [HolidayClosureID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]
        )
        SELECT
            NULLIF([ID], N'') AS [HolidayClosureID], CASE WHEN NULLIF([ClosureDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([ClosureDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([ClosureDate], N'')), CAST(NULL AS datetime2(7))) END AS [ClosureDate], NULLIF([DaysClosed], N'') AS [DaysClosed], NULLIF([AppliesToPrep], N'') AS [AppliesToPrep], NULLIF([AppliesToDelivery], N'') AS [AppliesToDelivery], NULLIF([ShiftStrategy], N'') AS [ShiftStrategy], NULLIF([Description], N'') AS [Description]
        FROM [HolidayClosureTbl];
        SET IDENTITY_INSERT [HolidayClosuresTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'HolidayClosuresTbl'))
            DBCC CHECKIDENT (N'HolidayClosuresTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [HolidayClosuresTbl] from ' + N'[HolidayClosureTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'HolidayClosuresTbl') IS NOT NULL SET IDENTITY_INSERT [HolidayClosuresTbl] OFF;
        END TRY BEGIN CATCH END CATCH
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

-- PackagingTbl -> ItemPackagingsTbl
-- Mapping: columnsCount=6
--   PackagingID -> ItemPackagingID
--   Description -> ItemPrepDescription
--   AdditionalNotes -> AdditionalNotes
--   Symbol -> Symbol
--   Colour -> Colour
--   BGColour -> BGColour
IF OBJECT_ID(N'AccessSrc.PackagingTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemPackagingsTbl] ([ItemPackagingID], [ItemPrepDescription], [AdditionalNotes], [Symbol], [Colour], [BGColour]) SELECT NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([Description], N'''') AS [ItemPrepDescription], NULLIF([AdditionalNotes], N'''') AS [AdditionalNotes], NULLIF([Symbol], N'''') AS [Symbol], NULLIF([Colour], N'''') AS [Colour], NULLIF([BGColour], N'''') AS [BGColour] FROM [AccessSrc].[PackagingTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemPackagingsTbl] ON;
        INSERT INTO [ItemPackagingsTbl]
        (
            [ItemPackagingID], [ItemPrepDescription], [AdditionalNotes], [Symbol], [Colour], [BGColour]
        )
        SELECT
            NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([Description], N'') AS [ItemPrepDescription], NULLIF([AdditionalNotes], N'') AS [AdditionalNotes], NULLIF([Symbol], N'') AS [Symbol], NULLIF([Colour], N'') AS [Colour], NULLIF([BGColour], N'') AS [BGColour]
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
    PRINT N'INSERT INTO [ItemPackagingsTbl] ([ItemPackagingID], [ItemPrepDescription], [AdditionalNotes], [Symbol], [Colour], [BGColour]) SELECT NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([Description], N'''') AS [ItemPrepDescription], NULLIF([AdditionalNotes], N'''') AS [AdditionalNotes], NULLIF([Symbol], N'''') AS [Symbol], NULLIF([Colour], N'''') AS [Colour], NULLIF([BGColour], N'''') AS [BGColour] FROM [PackagingTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemPackagingsTbl] ON;
        INSERT INTO [ItemPackagingsTbl]
        (
            [ItemPackagingID], [ItemPrepDescription], [AdditionalNotes], [Symbol], [Colour], [BGColour]
        )
        SELECT
            NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([Description], N'') AS [ItemPrepDescription], NULLIF([AdditionalNotes], N'') AS [AdditionalNotes], NULLIF([Symbol], N'') AS [Symbol], NULLIF([Colour], N'') AS [Colour], NULLIF([BGColour], N'') AS [BGColour]
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
--   PrepID -> ItemPrepID
--   PrepType -> ItemPrepType
--   IdentifyingChar -> IdentifyingChar
IF OBJECT_ID(N'AccessSrc.PrepTypesTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemPrepTypesTbl] ([ItemPrepID], [ItemPrepType], [IdentifyingChar]) SELECT NULLIF([PrepID], N'''') AS [ItemPrepID], NULLIF([PrepType], N'''') AS [ItemPrepType], NULLIF([IdentifyingChar], N'''') AS [IdentifyingChar] FROM [AccessSrc].[PrepTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemPrepTypesTbl] ON;
        INSERT INTO [ItemPrepTypesTbl]
        (
            [ItemPrepID], [ItemPrepType], [IdentifyingChar]
        )
        SELECT
            NULLIF([PrepID], N'') AS [ItemPrepID], NULLIF([PrepType], N'') AS [ItemPrepType], NULLIF([IdentifyingChar], N'') AS [IdentifyingChar]
        FROM [AccessSrc].[PrepTypesTbl];
        SET IDENTITY_INSERT [ItemPrepTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemPrepTypesTbl'))
            DBCC CHECKIDENT (N'ItemPrepTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemPrepTypesTbl] from ' + N'[AccessSrc].[PrepTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemPrepTypesTbl') IS NOT NULL SET IDENTITY_INSERT [ItemPrepTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemPrepTypesTbl] from ' + N'[AccessSrc].[PrepTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemPrepTypesTbl] ([ItemPrepID], [ItemPrepType], [IdentifyingChar]) SELECT NULLIF([PrepID], N'''') AS [ItemPrepID], NULLIF([PrepType], N'''') AS [ItemPrepType], NULLIF([IdentifyingChar], N'''') AS [IdentifyingChar] FROM [PrepTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemPrepTypesTbl] ON;
        INSERT INTO [ItemPrepTypesTbl]
        (
            [ItemPrepID], [ItemPrepType], [IdentifyingChar]
        )
        SELECT
            NULLIF([PrepID], N'') AS [ItemPrepID], NULLIF([PrepType], N'') AS [ItemPrepType], NULLIF([IdentifyingChar], N'') AS [IdentifyingChar]
        FROM [PrepTypesTbl];
        SET IDENTITY_INSERT [ItemPrepTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemPrepTypesTbl'))
            DBCC CHECKIDENT (N'ItemPrepTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemPrepTypesTbl] from ' + N'[PrepTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemPrepTypesTbl') IS NOT NULL SET IDENTITY_INSERT [ItemPrepTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemPrepTypesTbl] from ' + N'[PrepTypesTbl]' + ': ' + ERROR_MESSAGE();
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

-- OrdersTbl -> OrdersTbl
-- Mapping: columnsCount=16
--   OrderID -> OrderID
--   CustomerId -> ContactID
--   OrderDate -> OrderDate
--   RoastDate -> PrepDate
--   ItemTypeID -> ItemID
--   QuantityOrdered -> QtyOrdered
--   RequiredByDate -> RequiredByDate
--   PrepTypeID -> PrepTypeID
--   PackagingID -> PackagingID
--   ToBeDeliveredBy -> ToBeDeliveredByID
--   Confirmed -> Confirmed
--   Done -> Done
--   Packed -> Packed
--   Notes -> Notes
--   PurchaseOrder -> PurchaseOrder
--   InvoiceDone -> InvoiceDone
IF OBJECT_ID(N'AccessSrc.OrdersTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [OrdersTbl]: missing source [AccessSrc].[OrdersTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [OrdersTbl] ([OrderID], [ContactID], [OrderDate], [PrepDate], [ItemID], [QtyOrdered], [RequiredByDate], [PrepTypeID], [PackagingID], [ToBeDeliveredByID], [Confirmed], [Done], [Packed], [Notes], [PurchaseOrder], [InvoiceDone]) SELECT NULLIF([OrderID], N'''') AS [OrderID], NULLIF([CustomerId], N'''') AS [ContactID], CASE WHEN NULLIF([OrderDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([OrderDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N'''')), CAST(NULL AS datetime2(7))) END AS [OrderDate], CASE WHEN NULLIF([RoastDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RoastDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N'''')), CAST(NULL AS datetime2(7))) END AS [PrepDate], NULLIF([ItemTypeID], N'''') AS [ItemID], NULLIF([QuantityOrdered], N'''') AS [QtyOrdered], CASE WHEN NULLIF([RequiredByDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLI ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [OrdersTbl] ON;
        INSERT INTO [OrdersTbl]
        (
            [OrderID], [ContactID], [OrderDate], [PrepDate], [ItemID], [QtyOrdered], [RequiredByDate], [PrepTypeID], [PackagingID], [ToBeDeliveredByID], [Confirmed], [Done], [Packed], [Notes], [PurchaseOrder], [InvoiceDone]
        )
        SELECT
            NULLIF([OrderID], N'') AS [OrderID], NULLIF([CustomerId], N'') AS [ContactID], CASE WHEN NULLIF([OrderDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([OrderDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N'')), CAST(NULL AS datetime2(7))) END AS [OrderDate], CASE WHEN NULLIF([RoastDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RoastDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N'')), CAST(NULL AS datetime2(7))) END AS [PrepDate], NULLIF([ItemTypeID], N'') AS [ItemID], NULLIF([QuantityOrdered], N'') AS [QtyOrdered], CASE WHEN NULLIF([RequiredByDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RequiredByDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N'')), CAST(NULL AS datetime2(7))) END AS [RequiredByDate], NULLIF([PrepTypeID], N'') AS [PrepTypeID], NULLIF([PackagingID], N'') AS [PackagingID], NULLIF([ToBeDeliveredBy], N'') AS [ToBeDeliveredByID], CASE WHEN NULLIF([Confirmed], N'') IS NULL THEN NULL WHEN NULLIF([Confirmed], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Confirmed], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Confirmed], N'')) END AS [Confirmed], CASE WHEN NULLIF([Done], N'') IS NULL THEN NULL WHEN NULLIF([Done], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Done], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Done], N'')) END AS [Done], NULLIF([Packed], N'') AS [Packed], NULLIF([Notes], N'') AS [Notes], NULLIF([PurchaseOrder], N'') AS [PurchaseOrder], NULLIF([InvoiceDone], N'') AS [InvoiceDone]
        FROM [AccessSrc].[OrdersTbl];
        SET IDENTITY_INSERT [OrdersTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'OrdersTbl'))
            DBCC CHECKIDENT (N'OrdersTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [OrdersTbl] from ' + N'[AccessSrc].[OrdersTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'OrdersTbl') IS NOT NULL SET IDENTITY_INSERT [OrdersTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [OrdersTbl] from ' + N'[AccessSrc].[OrdersTbl]' + ': ' + ERROR_MESSAGE();
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
    PRINT N'INSERT INTO [PaymentTermsTbl] ([PaymentTermID], [PaymentTermDesc], [PaymentDays], [DayOfMonth], [UseDays], [Enabled], [Notes]) SELECT NULLIF([PaymentTermID], N'''') AS [PaymentTermID], NULLIF([PaymentTermDesc], N'''') AS [PaymentTermDesc], NULLIF([PaymentDays], N'''') AS [PaymentDays], NULLIF([DayOfMonth], N'''') AS [DayOfMonth], NULLIF([UseDays], N'''') AS [UseDays], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[PaymentTermsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [PaymentTermsTbl] ON;
        INSERT INTO [PaymentTermsTbl]
        (
            [PaymentTermID], [PaymentTermDesc], [PaymentDays], [DayOfMonth], [UseDays], [Enabled], [Notes]
        )
        SELECT
            NULLIF([PaymentTermID], N'') AS [PaymentTermID], NULLIF([PaymentTermDesc], N'') AS [PaymentTermDesc], NULLIF([PaymentDays], N'') AS [PaymentDays], NULLIF([DayOfMonth], N'') AS [DayOfMonth], NULLIF([UseDays], N'') AS [UseDays], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
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

-- PersonsTbl -> PeopleTbl
-- Mapping: columnsCount=6
--   PersonID -> PersonID
--   Person -> Person
--   Abreviation -> Abbreviation
--   Enabled -> Enabled
--   NormalDeliveryDoW -> NormalDeliveryDoW
--   SecurityUsername -> SecurityUsername
IF OBJECT_ID(N'AccessSrc.PersonsTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [PeopleTbl] ([PersonID], [Person], [Abbreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]) SELECT NULLIF([PersonID], N'''') AS [PersonID], NULLIF([Person], N'''') AS [Person], NULLIF([Abreviation], N'''') AS [Abbreviation], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'''') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'''') AS [SecurityUsername] FROM [AccessSrc].[PersonsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [PeopleTbl] ON;
        INSERT INTO [PeopleTbl]
        (
            [PersonID], [Person], [Abbreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]
        )
        SELECT
            NULLIF([PersonID], N'') AS [PersonID], NULLIF([Person], N'') AS [Person], NULLIF([Abreviation], N'') AS [Abbreviation], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'') AS [SecurityUsername]
        FROM [AccessSrc].[PersonsTbl];
        SET IDENTITY_INSERT [PeopleTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'PeopleTbl'))
            DBCC CHECKIDENT (N'PeopleTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [PeopleTbl] from ' + N'[AccessSrc].[PersonsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'PeopleTbl') IS NOT NULL SET IDENTITY_INSERT [PeopleTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [PeopleTbl] from ' + N'[AccessSrc].[PersonsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [PeopleTbl] ([PersonID], [Person], [Abbreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]) SELECT NULLIF([PersonID], N'''') AS [PersonID], NULLIF([Person], N'''') AS [Person], NULLIF([Abreviation], N'''') AS [Abbreviation], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'''') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'''') AS [SecurityUsername] FROM [PersonsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [PeopleTbl] ON;
        INSERT INTO [PeopleTbl]
        (
            [PersonID], [Person], [Abbreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]
        )
        SELECT
            NULLIF([PersonID], N'') AS [PersonID], NULLIF([Person], N'') AS [Person], NULLIF([Abreviation], N'') AS [Abbreviation], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([NormalDeliveryDoW], N'') AS [NormalDeliveryDoW], NULLIF([SecurityUsername], N'') AS [SecurityUsername]
        FROM [PersonsTbl];
        SET IDENTITY_INSERT [PeopleTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'PeopleTbl'))
            DBCC CHECKIDENT (N'PeopleTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [PeopleTbl] from ' + N'[PersonsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'PeopleTbl') IS NOT NULL SET IDENTITY_INSERT [PeopleTbl] OFF;
        END TRY BEGIN CATCH END CATCH
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

-- ReoccuranceTypeTbl -> RecurranceTypesTbl
-- Mapping: columnsCount=2
--   ID -> RecurringTypeID
--   Type -> RecurringTypeDesc
IF OBJECT_ID(N'AccessSrc.ReoccuranceTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [RecurranceTypesTbl] ([RecurringTypeID], [RecurringTypeDesc]) SELECT NULLIF([ID], N'''') AS [RecurringTypeID], NULLIF([Type], N'''') AS [RecurringTypeDesc] FROM [AccessSrc].[ReoccuranceTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RecurranceTypesTbl] ON;
        INSERT INTO [RecurranceTypesTbl]
        (
            [RecurringTypeID], [RecurringTypeDesc]
        )
        SELECT
            NULLIF([ID], N'') AS [RecurringTypeID], NULLIF([Type], N'') AS [RecurringTypeDesc]
        FROM [AccessSrc].[ReoccuranceTypeTbl];
        SET IDENTITY_INSERT [RecurranceTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'RecurranceTypesTbl'))
            DBCC CHECKIDENT (N'RecurranceTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [RecurranceTypesTbl] from ' + N'[AccessSrc].[ReoccuranceTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'RecurranceTypesTbl') IS NOT NULL SET IDENTITY_INSERT [RecurranceTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [RecurranceTypesTbl] from ' + N'[AccessSrc].[ReoccuranceTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [RecurranceTypesTbl] ([RecurringTypeID], [RecurringTypeDesc]) SELECT NULLIF([ID], N'''') AS [RecurringTypeID], NULLIF([Type], N'''') AS [RecurringTypeDesc] FROM [ReoccuranceTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RecurranceTypesTbl] ON;
        INSERT INTO [RecurranceTypesTbl]
        (
            [RecurringTypeID], [RecurringTypeDesc]
        )
        SELECT
            NULLIF([ID], N'') AS [RecurringTypeID], NULLIF([Type], N'') AS [RecurringTypeDesc]
        FROM [ReoccuranceTypeTbl];
        SET IDENTITY_INSERT [RecurranceTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'RecurranceTypesTbl'))
            DBCC CHECKIDENT (N'RecurranceTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [RecurranceTypesTbl] from ' + N'[ReoccuranceTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'RecurranceTypesTbl') IS NOT NULL SET IDENTITY_INSERT [RecurranceTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [RecurranceTypesTbl] from ' + N'[ReoccuranceTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ReoccuringOrderTbl -> RecurringOrdersTbl
-- Mapping: columnsCount=12
--   ID -> RecurringOrderID
--   CustomerID -> ContactID
--   ReoccuranceType -> RecurringTypeID
--   Value -> Value
--   ItemRequired -> ItemRequiredID
--   QtyRequired -> QtyRequired
--   DateLastDone -> DateLastDone
--   NextDateRequired -> NextDateRequired
--   RequireUntilDate -> RequireUntilDate
--   PackagingID -> ItemPackagingID
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ReoccuringOrderTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [RecurringOrdersTbl] ([RecurringOrderID], [ContactID], [RecurringTypeID], [Value], [ItemRequiredID], [QtyRequired], [DateLastDone], [NextDateRequired], [RequireUntilDate], [ItemPackagingID], [Enabled], [Notes]) SELECT NULLIF([ID], N'''') AS [RecurringOrderID], NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([ReoccuranceType], N'''') AS [RecurringTypeID], NULLIF([Value], N'''') AS [Value], NULLIF([ItemRequired], N'''') AS [ItemRequiredID], NULLIF([QtyRequired], N'''') AS [QtyRequired], CASE WHEN NULLIF([DateLastDone], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastDone], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N'''')), CAST(NULL AS datetime2(7))) END AS [DateLastDone], CASE WHEN NULLIF([NextDateRequired], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextDateRequired], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 101), TRY_CONVERT(da ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RecurringOrdersTbl] ON;
        INSERT INTO [RecurringOrdersTbl]
        (
            [RecurringOrderID], [ContactID], [RecurringTypeID], [Value], [ItemRequiredID], [QtyRequired], [DateLastDone], [NextDateRequired], [RequireUntilDate], [ItemPackagingID], [Enabled], [Notes]
        )
        SELECT
            NULLIF([ID], N'') AS [RecurringOrderID], NULLIF([CustomerID], N'') AS [ContactID], NULLIF([ReoccuranceType], N'') AS [RecurringTypeID], NULLIF([Value], N'') AS [Value], NULLIF([ItemRequired], N'') AS [ItemRequiredID], NULLIF([QtyRequired], N'') AS [QtyRequired], CASE WHEN NULLIF([DateLastDone], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastDone], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N'')), CAST(NULL AS datetime2(7))) END AS [DateLastDone], CASE WHEN NULLIF([NextDateRequired], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextDateRequired], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N'')), CAST(NULL AS datetime2(7))) END AS [NextDateRequired], CASE WHEN NULLIF([RequireUntilDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RequireUntilDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N'')), CAST(NULL AS datetime2(7))) END AS [RequireUntilDate], NULLIF([PackagingID], N'') AS [ItemPackagingID], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[ReoccuringOrderTbl];
        SET IDENTITY_INSERT [RecurringOrdersTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'RecurringOrdersTbl'))
            DBCC CHECKIDENT (N'RecurringOrdersTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [RecurringOrdersTbl] from ' + N'[AccessSrc].[ReoccuringOrderTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'RecurringOrdersTbl') IS NOT NULL SET IDENTITY_INSERT [RecurringOrdersTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [RecurringOrdersTbl] from ' + N'[AccessSrc].[ReoccuringOrderTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [RecurringOrdersTbl] ([RecurringOrderID], [ContactID], [RecurringTypeID], [Value], [ItemRequiredID], [QtyRequired], [DateLastDone], [NextDateRequired], [RequireUntilDate], [ItemPackagingID], [Enabled], [Notes]) SELECT NULLIF([ID], N'''') AS [RecurringOrderID], NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([ReoccuranceType], N'''') AS [RecurringTypeID], NULLIF([Value], N'''') AS [Value], NULLIF([ItemRequired], N'''') AS [ItemRequiredID], NULLIF([QtyRequired], N'''') AS [QtyRequired], CASE WHEN NULLIF([DateLastDone], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastDone], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N'''')), CAST(NULL AS datetime2(7))) END AS [DateLastDone], CASE WHEN NULLIF([NextDateRequired], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextDateRequired], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''''), 101), TRY_CONVERT(da ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RecurringOrdersTbl] ON;
        INSERT INTO [RecurringOrdersTbl]
        (
            [RecurringOrderID], [ContactID], [RecurringTypeID], [Value], [ItemRequiredID], [QtyRequired], [DateLastDone], [NextDateRequired], [RequireUntilDate], [ItemPackagingID], [Enabled], [Notes]
        )
        SELECT
            NULLIF([ID], N'') AS [RecurringOrderID], NULLIF([CustomerID], N'') AS [ContactID], NULLIF([ReoccuranceType], N'') AS [RecurringTypeID], NULLIF([Value], N'') AS [Value], NULLIF([ItemRequired], N'') AS [ItemRequiredID], NULLIF([QtyRequired], N'') AS [QtyRequired], CASE WHEN NULLIF([DateLastDone], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastDone], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastDone], N'')), CAST(NULL AS datetime2(7))) END AS [DateLastDone], CASE WHEN NULLIF([NextDateRequired], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextDateRequired], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextDateRequired], N'')), CAST(NULL AS datetime2(7))) END AS [NextDateRequired], CASE WHEN NULLIF([RequireUntilDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RequireUntilDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([RequireUntilDate], N'')), CAST(NULL AS datetime2(7))) END AS [RequireUntilDate], NULLIF([PackagingID], N'') AS [ItemPackagingID], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [ReoccuringOrderTbl];
        SET IDENTITY_INSERT [RecurringOrdersTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'RecurringOrdersTbl'))
            DBCC CHECKIDENT (N'RecurringOrdersTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [RecurringOrdersTbl] from ' + N'[ReoccuringOrderTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'RecurringOrdersTbl') IS NOT NULL SET IDENTITY_INSERT [RecurringOrdersTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [RecurringOrdersTbl] from ' + N'[ReoccuringOrderTbl]' + ': ' + ERROR_MESSAGE();
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
--   EmailClient -> EmailContact
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
    PRINT N'INSERT INTO [RepairStatusesTbl] ([RepairStatusID], [RepairStatusDesc], [EmailContact], [SortOrder], [Notes], [StatusNote]) SELECT NULLIF([RepairStatusID], N'''') AS [RepairStatusID], NULLIF([RepairStatusDesc], N'''') AS [RepairStatusDesc], NULLIF([EmailClient], N'''') AS [EmailContact], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([Notes], N'''') AS [Notes], NULLIF([StatusNote], N'''') AS [StatusNote] FROM [AccessSrc].[RepairStatusesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RepairStatusesTbl] ON;
        INSERT INTO [RepairStatusesTbl]
        (
            [RepairStatusID], [RepairStatusDesc], [EmailContact], [SortOrder], [Notes], [StatusNote]
        )
        SELECT
            NULLIF([RepairStatusID], N'') AS [RepairStatusID], NULLIF([RepairStatusDesc], N'') AS [RepairStatusDesc], NULLIF([EmailClient], N'') AS [EmailContact], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([Notes], N'') AS [Notes], NULLIF([StatusNote], N'') AS [StatusNote]
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

-- SendCheckEmailTextsTbl -> SendCheckupEmailTextsTbl
-- Mapping: columnsCount=6
--   SCEMTID -> SCEMTID
--   Header -> HeaderText
--   Body -> BodyText
--   Footer -> FooterText
--   DateLastChange -> DateLastChange
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.SendCheckEmailTextsTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [SendCheckupEmailTextsTbl] ([SCEMTID], [HeaderText], [BodyText], [FooterText], [DateLastChange], [Notes]) SELECT NULLIF([SCEMTID], N'''') AS [SCEMTID], NULLIF([Header], N'''') AS [HeaderText], NULLIF([Body], N'''') AS [BodyText], NULLIF([Footer], N'''') AS [FooterText], CASE WHEN NULLIF([DateLastChange], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastChange], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N'''')), CAST(NULL AS datetime2(7))) END AS [DateLastChange], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[SendCheckEmailTextsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [SendCheckupEmailTextsTbl] ON;
        INSERT INTO [SendCheckupEmailTextsTbl]
        (
            [SCEMTID], [HeaderText], [BodyText], [FooterText], [DateLastChange], [Notes]
        )
        SELECT
            NULLIF([SCEMTID], N'') AS [SCEMTID], NULLIF([Header], N'') AS [HeaderText], NULLIF([Body], N'') AS [BodyText], NULLIF([Footer], N'') AS [FooterText], CASE WHEN NULLIF([DateLastChange], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastChange], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N'')), CAST(NULL AS datetime2(7))) END AS [DateLastChange], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[SendCheckEmailTextsTbl];
        SET IDENTITY_INSERT [SendCheckupEmailTextsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'SendCheckupEmailTextsTbl'))
            DBCC CHECKIDENT (N'SendCheckupEmailTextsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [SendCheckupEmailTextsTbl] from ' + N'[AccessSrc].[SendCheckEmailTextsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'SendCheckupEmailTextsTbl') IS NOT NULL SET IDENTITY_INSERT [SendCheckupEmailTextsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [SendCheckupEmailTextsTbl] from ' + N'[AccessSrc].[SendCheckEmailTextsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [SendCheckupEmailTextsTbl] ([SCEMTID], [HeaderText], [BodyText], [FooterText], [DateLastChange], [Notes]) SELECT NULLIF([SCEMTID], N'''') AS [SCEMTID], NULLIF([Header], N'''') AS [HeaderText], NULLIF([Body], N'''') AS [BodyText], NULLIF([Footer], N'''') AS [FooterText], CASE WHEN NULLIF([DateLastChange], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastChange], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N'''')), CAST(NULL AS datetime2(7))) END AS [DateLastChange], NULLIF([Notes], N'''') AS [Notes] FROM [SendCheckEmailTextsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [SendCheckupEmailTextsTbl] ON;
        INSERT INTO [SendCheckupEmailTextsTbl]
        (
            [SCEMTID], [HeaderText], [BodyText], [FooterText], [DateLastChange], [Notes]
        )
        SELECT
            NULLIF([SCEMTID], N'') AS [SCEMTID], NULLIF([Header], N'') AS [HeaderText], NULLIF([Body], N'') AS [BodyText], NULLIF([Footer], N'') AS [FooterText], CASE WHEN NULLIF([DateLastChange], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastChange], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastChange], N'')), CAST(NULL AS datetime2(7))) END AS [DateLastChange], NULLIF([Notes], N'') AS [Notes]
        FROM [SendCheckEmailTextsTbl];
        SET IDENTITY_INSERT [SendCheckupEmailTextsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'SendCheckupEmailTextsTbl'))
            DBCC CHECKIDENT (N'SendCheckupEmailTextsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [SendCheckupEmailTextsTbl] from ' + N'[SendCheckEmailTextsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'SendCheckupEmailTextsTbl') IS NOT NULL SET IDENTITY_INSERT [SendCheckupEmailTextsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [SendCheckupEmailTextsTbl] from ' + N'[SendCheckEmailTextsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TotalCountTrackerTbl -> TotalCountTrackerTbl
-- Mapping: columnsCount=4
--   ID -> TotalCounterTrackerID
--   CountDate -> CountDate
--   TotalCount -> TotalCount
--   Comments -> Comments
IF OBJECT_ID(N'AccessSrc.TotalCountTrackerTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TotalCountTrackerTbl]: missing source [AccessSrc].[TotalCountTrackerTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TotalCountTrackerTbl] ([TotalCounterTrackerID], [CountDate], [TotalCount], [Comments]) SELECT NULLIF([ID], N'''') AS [TotalCounterTrackerID], CASE WHEN NULLIF([CountDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([CountDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N'''')), CAST(NULL AS datetime2(7))) END AS [CountDate], NULLIF([TotalCount], N'''') AS [TotalCount], NULLIF([Comments], N'''') AS [Comments] FROM [AccessSrc].[TotalCountTrackerTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TotalCountTrackerTbl] ON;
        INSERT INTO [TotalCountTrackerTbl]
        (
            [TotalCounterTrackerID], [CountDate], [TotalCount], [Comments]
        )
        SELECT
            NULLIF([ID], N'') AS [TotalCounterTrackerID], CASE WHEN NULLIF([CountDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([CountDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([CountDate], N'')), CAST(NULL AS datetime2(7))) END AS [CountDate], NULLIF([TotalCount], N'') AS [TotalCount], NULLIF([Comments], N'') AS [Comments]
        FROM [AccessSrc].[TotalCountTrackerTbl];
        SET IDENTITY_INSERT [TotalCountTrackerTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TotalCountTrackerTbl'))
            DBCC CHECKIDENT (N'TotalCountTrackerTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TotalCountTrackerTbl] from ' + N'[AccessSrc].[TotalCountTrackerTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TotalCountTrackerTbl') IS NOT NULL SET IDENTITY_INSERT [TotalCountTrackerTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [TotalCountTrackerTbl] from ' + N'[AccessSrc].[TotalCountTrackerTbl]' + ': ' + ERROR_MESSAGE();
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

-- CityPrepDaysTbl -> AreaPrepDaysTbl
-- Mapping: columnsCount=5
--   CityPrepDaysID -> AreaPrepDaysID
--   CityID -> AreaID
--   PrepDayOfWeekID -> PrepDayOfWeekID
--   DeliveryDelayDays -> DeliveryDelayDays
--   DeliveryOrder -> DeliveryOrder
IF OBJECT_ID(N'AccessSrc.CityPrepDaysTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [AreaPrepDaysTbl] ([AreaPrepDaysID], [AreaID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]) SELECT NULLIF([CityPrepDaysID], N'''') AS [AreaPrepDaysID], NULLIF([CityID], N'''') AS [AreaID], NULLIF([PrepDayOfWeekID], N'''') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'''') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder] FROM [AccessSrc].[CityPrepDaysTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [AreaPrepDaysTbl] ON;
        INSERT INTO [AreaPrepDaysTbl]
        (
            [AreaPrepDaysID], [AreaID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]
        )
        SELECT
            NULLIF([CityPrepDaysID], N'') AS [AreaPrepDaysID], NULLIF([CityID], N'') AS [AreaID], NULLIF([PrepDayOfWeekID], N'') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder]
        FROM [AccessSrc].[CityPrepDaysTbl];
        SET IDENTITY_INSERT [AreaPrepDaysTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'AreaPrepDaysTbl'))
            DBCC CHECKIDENT (N'AreaPrepDaysTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [AreaPrepDaysTbl] from ' + N'[AccessSrc].[CityPrepDaysTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'AreaPrepDaysTbl') IS NOT NULL SET IDENTITY_INSERT [AreaPrepDaysTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [AreaPrepDaysTbl] from ' + N'[AccessSrc].[CityPrepDaysTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [AreaPrepDaysTbl] ([AreaPrepDaysID], [AreaID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]) SELECT NULLIF([CityPrepDaysID], N'''') AS [AreaPrepDaysID], NULLIF([CityID], N'''') AS [AreaID], NULLIF([PrepDayOfWeekID], N'''') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'''') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder] FROM [CityPrepDaysTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [AreaPrepDaysTbl] ON;
        INSERT INTO [AreaPrepDaysTbl]
        (
            [AreaPrepDaysID], [AreaID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]
        )
        SELECT
            NULLIF([CityPrepDaysID], N'') AS [AreaPrepDaysID], NULLIF([CityID], N'') AS [AreaID], NULLIF([PrepDayOfWeekID], N'') AS [PrepDayOfWeekID], NULLIF([DeliveryDelayDays], N'') AS [DeliveryDelayDays], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder]
        FROM [CityPrepDaysTbl];
        SET IDENTITY_INSERT [AreaPrepDaysTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'AreaPrepDaysTbl'))
            DBCC CHECKIDENT (N'AreaPrepDaysTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [AreaPrepDaysTbl] from ' + N'[CityPrepDaysTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'AreaPrepDaysTbl') IS NOT NULL SET IDENTITY_INSERT [AreaPrepDaysTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [AreaPrepDaysTbl] from ' + N'[CityPrepDaysTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- NextRoastDateByCityTbl -> NextPrepDateByAreasTbl
-- Mapping: columnsCount=7
--   NextRoastDayID -> NextPrepDayID
--   CityID -> AreaID
--   PreperationDate -> PreperationDate
--   DeliveryDate -> DeliveryDate
--   DeliveryOrder -> DeliveryOrder
--   NextPreperationDate -> NextPrepDate
--   NextDeliveryDate -> NextDeliveryDate
IF OBJECT_ID(N'AccessSrc.NextRoastDateByCityTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [NextPrepDateByAreasTbl] ([NextPrepDayID], [AreaID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPrepDate], [NextDeliveryDate]) SELECT NULLIF([NextRoastDayID], N'''') AS [NextPrepDayID], NULLIF([CityID], N'''') AS [AreaID], CASE WHEN NULLIF([PreperationDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([PreperationDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N'''')), CAST(NULL AS datetime2(7))) END AS [PreperationDate], CASE WHEN NULLIF([DeliveryDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DeliveryDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N'''')), CAST(NULL AS datetime2(7))) END AS [DeliveryDate], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder], CASE WHEN NULLIF([NextPreperationDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextPreperationDate], N'''')) ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [NextPrepDateByAreasTbl] ON;
        INSERT INTO [NextPrepDateByAreasTbl]
        (
            [NextPrepDayID], [AreaID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPrepDate], [NextDeliveryDate]
        )
        SELECT
            NULLIF([NextRoastDayID], N'') AS [NextPrepDayID], NULLIF([CityID], N'') AS [AreaID], CASE WHEN NULLIF([PreperationDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([PreperationDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N'')), CAST(NULL AS datetime2(7))) END AS [PreperationDate], CASE WHEN NULLIF([DeliveryDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DeliveryDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N'')), CAST(NULL AS datetime2(7))) END AS [DeliveryDate], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder], CASE WHEN NULLIF([NextPreperationDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextPreperationDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N'')), CAST(NULL AS datetime2(7))) END AS [NextPrepDate], CASE WHEN NULLIF([NextDeliveryDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextDeliveryDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N'')), CAST(NULL AS datetime2(7))) END AS [NextDeliveryDate]
        FROM [AccessSrc].[NextRoastDateByCityTbl];
        SET IDENTITY_INSERT [NextPrepDateByAreasTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'NextPrepDateByAreasTbl'))
            DBCC CHECKIDENT (N'NextPrepDateByAreasTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [NextPrepDateByAreasTbl] from ' + N'[AccessSrc].[NextRoastDateByCityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'NextPrepDateByAreasTbl') IS NOT NULL SET IDENTITY_INSERT [NextPrepDateByAreasTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [NextPrepDateByAreasTbl] from ' + N'[AccessSrc].[NextRoastDateByCityTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [NextPrepDateByAreasTbl] ([NextPrepDayID], [AreaID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPrepDate], [NextDeliveryDate]) SELECT NULLIF([NextRoastDayID], N'''') AS [NextPrepDayID], NULLIF([CityID], N'''') AS [AreaID], CASE WHEN NULLIF([PreperationDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([PreperationDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N'''')), CAST(NULL AS datetime2(7))) END AS [PreperationDate], CASE WHEN NULLIF([DeliveryDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DeliveryDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N'''')), CAST(NULL AS datetime2(7))) END AS [DeliveryDate], NULLIF([DeliveryOrder], N'''') AS [DeliveryOrder], CASE WHEN NULLIF([NextPreperationDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextPreperationDate], N'''')) ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [NextPrepDateByAreasTbl] ON;
        INSERT INTO [NextPrepDateByAreasTbl]
        (
            [NextPrepDayID], [AreaID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPrepDate], [NextDeliveryDate]
        )
        SELECT
            NULLIF([NextRoastDayID], N'') AS [NextPrepDayID], NULLIF([CityID], N'') AS [AreaID], CASE WHEN NULLIF([PreperationDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([PreperationDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([PreperationDate], N'')), CAST(NULL AS datetime2(7))) END AS [PreperationDate], CASE WHEN NULLIF([DeliveryDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DeliveryDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DeliveryDate], N'')), CAST(NULL AS datetime2(7))) END AS [DeliveryDate], NULLIF([DeliveryOrder], N'') AS [DeliveryOrder], CASE WHEN NULLIF([NextPreperationDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextPreperationDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextPreperationDate], N'')), CAST(NULL AS datetime2(7))) END AS [NextPrepDate], CASE WHEN NULLIF([NextDeliveryDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextDeliveryDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextDeliveryDate], N'')), CAST(NULL AS datetime2(7))) END AS [NextDeliveryDate]
        FROM [NextRoastDateByCityTbl];
        SET IDENTITY_INSERT [NextPrepDateByAreasTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'NextPrepDateByAreasTbl'))
            DBCC CHECKIDENT (N'NextPrepDateByAreasTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [NextPrepDateByAreasTbl] from ' + N'[NextRoastDateByCityTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'NextPrepDateByAreasTbl') IS NOT NULL SET IDENTITY_INSERT [NextPrepDateByAreasTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [NextPrepDateByAreasTbl] from ' + N'[NextRoastDateByCityTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ServiceTypesTbl -> ItemServiceTypesTbl
-- Mapping: columnsCount=5
--   ServiceTypeId -> ItemServiceTypeID
--   ServiceType -> ItemServiceType
--   Description -> Description
--   PackagingID -> ItemPackagingID
--   PrepTypeID -> ItemPrepTypeID
IF OBJECT_ID(N'AccessSrc.ServiceTypesTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemServiceTypesTbl] ([ItemServiceTypeID], [ItemServiceType], [Description], [ItemPackagingID], [ItemPrepTypeID]) SELECT NULLIF([ServiceTypeId], N'''') AS [ItemServiceTypeID], NULLIF([ServiceType], N'''') AS [ItemServiceType], NULLIF([Description], N'''') AS [Description], NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([PrepTypeID], N'''') AS [ItemPrepTypeID] FROM [AccessSrc].[ServiceTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemServiceTypesTbl] ON;
        INSERT INTO [ItemServiceTypesTbl]
        (
            [ItemServiceTypeID], [ItemServiceType], [Description], [ItemPackagingID], [ItemPrepTypeID]
        )
        SELECT
            NULLIF([ServiceTypeId], N'') AS [ItemServiceTypeID], NULLIF([ServiceType], N'') AS [ItemServiceType], NULLIF([Description], N'') AS [Description], NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([PrepTypeID], N'') AS [ItemPrepTypeID]
        FROM [AccessSrc].[ServiceTypesTbl];
        SET IDENTITY_INSERT [ItemServiceTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemServiceTypesTbl'))
            DBCC CHECKIDENT (N'ItemServiceTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemServiceTypesTbl] from ' + N'[AccessSrc].[ServiceTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemServiceTypesTbl') IS NOT NULL SET IDENTITY_INSERT [ItemServiceTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemServiceTypesTbl] from ' + N'[AccessSrc].[ServiceTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemServiceTypesTbl] ([ItemServiceTypeID], [ItemServiceType], [Description], [ItemPackagingID], [ItemPrepTypeID]) SELECT NULLIF([ServiceTypeId], N'''') AS [ItemServiceTypeID], NULLIF([ServiceType], N'''') AS [ItemServiceType], NULLIF([Description], N'''') AS [Description], NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([PrepTypeID], N'''') AS [ItemPrepTypeID] FROM [ServiceTypesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemServiceTypesTbl] ON;
        INSERT INTO [ItemServiceTypesTbl]
        (
            [ItemServiceTypeID], [ItemServiceType], [Description], [ItemPackagingID], [ItemPrepTypeID]
        )
        SELECT
            NULLIF([ServiceTypeId], N'') AS [ItemServiceTypeID], NULLIF([ServiceType], N'') AS [ItemServiceType], NULLIF([Description], N'') AS [Description], NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([PrepTypeID], N'') AS [ItemPrepTypeID]
        FROM [ServiceTypesTbl];
        SET IDENTITY_INSERT [ItemServiceTypesTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemServiceTypesTbl'))
            DBCC CHECKIDENT (N'ItemServiceTypesTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemServiceTypesTbl] from ' + N'[ServiceTypesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemServiceTypesTbl') IS NOT NULL SET IDENTITY_INSERT [ItemServiceTypesTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemServiceTypesTbl] from ' + N'[ServiceTypesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemTypeTbl -> ItemsTbl
-- Mapping: columnsCount=13
--   ItemTypeID -> ItemID
--   SKU -> SKU
--   ItemDesc -> ItemDesc
--   ItemEnabled -> ItemEnabled
--   ItemsCharacteritics -> ItemsCharacteritics
--   ItemDetail -> ItemDetail
--   ServiceTypeId -> ItemServiceTypeID
--   ReplacementID -> ReplacementItemID
--   ItemShortName -> ItemShortName
--   SortOrder -> SortOrder
--   UnitsPerQty -> UnitsPerQty
--   ItemUnitID -> ItemUnitID
--   BasePrice -> BasePrice
IF OBJECT_ID(N'AccessSrc.ItemTypeTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemsTbl] ([ItemID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ItemServiceTypeID], [ReplacementItemID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]) SELECT NULLIF([ItemTypeID], N'''') AS [ItemID], NULLIF([SKU], N'''') AS [SKU], NULLIF([ItemDesc], N'''') AS [ItemDesc], NULLIF([ItemEnabled], N'''') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'''') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'''') AS [ItemDetail], NULLIF([ServiceTypeId], N'''') AS [ItemServiceTypeID], NULLIF([ReplacementID], N'''') AS [ReplacementItemID], NULLIF([ItemShortName], N'''') AS [ItemShortName], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([UnitsPerQty], N'''') AS [UnitsPerQty], NULLIF([ItemUnitID], N'''') AS [ItemUnitID], NULLIF([BasePrice], N'''') AS [BasePrice] FROM [AccessSrc].[ItemTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemsTbl] ON;
        INSERT INTO [ItemsTbl]
        (
            [ItemID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ItemServiceTypeID], [ReplacementItemID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]
        )
        SELECT
            NULLIF([ItemTypeID], N'') AS [ItemID], NULLIF([SKU], N'') AS [SKU], NULLIF([ItemDesc], N'') AS [ItemDesc], NULLIF([ItemEnabled], N'') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'') AS [ItemDetail], NULLIF([ServiceTypeId], N'') AS [ItemServiceTypeID], NULLIF([ReplacementID], N'') AS [ReplacementItemID], NULLIF([ItemShortName], N'') AS [ItemShortName], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([UnitsPerQty], N'') AS [UnitsPerQty], NULLIF([ItemUnitID], N'') AS [ItemUnitID], NULLIF([BasePrice], N'') AS [BasePrice]
        FROM [AccessSrc].[ItemTypeTbl];
        SET IDENTITY_INSERT [ItemsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemsTbl'))
            DBCC CHECKIDENT (N'ItemsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemsTbl] from ' + N'[AccessSrc].[ItemTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemsTbl') IS NOT NULL SET IDENTITY_INSERT [ItemsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemsTbl] from ' + N'[AccessSrc].[ItemTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemsTbl] ([ItemID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ItemServiceTypeID], [ReplacementItemID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]) SELECT NULLIF([ItemTypeID], N'''') AS [ItemID], NULLIF([SKU], N'''') AS [SKU], NULLIF([ItemDesc], N'''') AS [ItemDesc], NULLIF([ItemEnabled], N'''') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'''') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'''') AS [ItemDetail], NULLIF([ServiceTypeId], N'''') AS [ItemServiceTypeID], NULLIF([ReplacementID], N'''') AS [ReplacementItemID], NULLIF([ItemShortName], N'''') AS [ItemShortName], NULLIF([SortOrder], N'''') AS [SortOrder], NULLIF([UnitsPerQty], N'''') AS [UnitsPerQty], NULLIF([ItemUnitID], N'''') AS [ItemUnitID], NULLIF([BasePrice], N'''') AS [BasePrice] FROM [ItemTypeTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemsTbl] ON;
        INSERT INTO [ItemsTbl]
        (
            [ItemID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ItemServiceTypeID], [ReplacementItemID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]
        )
        SELECT
            NULLIF([ItemTypeID], N'') AS [ItemID], NULLIF([SKU], N'') AS [SKU], NULLIF([ItemDesc], N'') AS [ItemDesc], NULLIF([ItemEnabled], N'') AS [ItemEnabled], NULLIF([ItemsCharacteritics], N'') AS [ItemsCharacteritics], NULLIF([ItemDetail], N'') AS [ItemDetail], NULLIF([ServiceTypeId], N'') AS [ItemServiceTypeID], NULLIF([ReplacementID], N'') AS [ReplacementItemID], NULLIF([ItemShortName], N'') AS [ItemShortName], NULLIF([SortOrder], N'') AS [SortOrder], NULLIF([UnitsPerQty], N'') AS [UnitsPerQty], NULLIF([ItemUnitID], N'') AS [ItemUnitID], NULLIF([BasePrice], N'') AS [BasePrice]
        FROM [ItemTypeTbl];
        SET IDENTITY_INSERT [ItemsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ItemsTbl'))
            DBCC CHECKIDENT (N'ItemsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ItemsTbl] from ' + N'[ItemTypeTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ItemsTbl') IS NOT NULL SET IDENTITY_INSERT [ItemsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ItemsTbl] from ' + N'[ItemTypeTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- TrackedServiceItemTbl -> TrackedServiceItemsTbl
-- Mapping: columnsCount=7
--   TrackerServiceItemID -> TrackerServiceItemID
--   ServiceTypeID -> ItemServiceTypeID
--   TypicalAvePerItem -> TypicalAvePerItem
--   UsageDateFieldName -> UsageDateFieldName
--   UsageAveFieldName -> UsageAveFieldName
--   ThisItemSetsDailyAverage -> ThisItemSetsDailyAverage
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.TrackedServiceItemTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TrackedServiceItemsTbl] ([TrackerServiceItemID], [ItemServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]) SELECT NULLIF([TrackerServiceItemID], N'''') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'''') AS [ItemServiceTypeID], NULLIF([TypicalAvePerItem], N'''') AS [TypicalAvePerItem], CASE WHEN NULLIF([UsageDateFieldName], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([UsageDateFieldName], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N'''')), CAST(NULL AS datetime2(7))) END AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'''') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'''') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[TrackedServiceItemTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TrackedServiceItemsTbl] ON;
        INSERT INTO [TrackedServiceItemsTbl]
        (
            [TrackerServiceItemID], [ItemServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]
        )
        SELECT
            NULLIF([TrackerServiceItemID], N'') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'') AS [ItemServiceTypeID], NULLIF([TypicalAvePerItem], N'') AS [TypicalAvePerItem], CASE WHEN NULLIF([UsageDateFieldName], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([UsageDateFieldName], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N'')), CAST(NULL AS datetime2(7))) END AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[TrackedServiceItemTbl];
        SET IDENTITY_INSERT [TrackedServiceItemsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TrackedServiceItemsTbl'))
            DBCC CHECKIDENT (N'TrackedServiceItemsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TrackedServiceItemsTbl] from ' + N'[AccessSrc].[TrackedServiceItemTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TrackedServiceItemsTbl') IS NOT NULL SET IDENTITY_INSERT [TrackedServiceItemsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [TrackedServiceItemsTbl] from ' + N'[AccessSrc].[TrackedServiceItemTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TrackedServiceItemsTbl] ([TrackerServiceItemID], [ItemServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]) SELECT NULLIF([TrackerServiceItemID], N'''') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'''') AS [ItemServiceTypeID], NULLIF([TypicalAvePerItem], N'''') AS [TypicalAvePerItem], CASE WHEN NULLIF([UsageDateFieldName], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([UsageDateFieldName], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N'''')), CAST(NULL AS datetime2(7))) END AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'''') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'''') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'''') AS [Notes] FROM [TrackedServiceItemTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TrackedServiceItemsTbl] ON;
        INSERT INTO [TrackedServiceItemsTbl]
        (
            [TrackerServiceItemID], [ItemServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]
        )
        SELECT
            NULLIF([TrackerServiceItemID], N'') AS [TrackerServiceItemID], NULLIF([ServiceTypeID], N'') AS [ItemServiceTypeID], NULLIF([TypicalAvePerItem], N'') AS [TypicalAvePerItem], CASE WHEN NULLIF([UsageDateFieldName], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([UsageDateFieldName], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([UsageDateFieldName], N'')), CAST(NULL AS datetime2(7))) END AS [UsageDateFieldName], NULLIF([UsageAveFieldName], N'') AS [UsageAveFieldName], NULLIF([ThisItemSetsDailyAverage], N'') AS [ThisItemSetsDailyAverage], NULLIF([Notes], N'') AS [Notes]
        FROM [TrackedServiceItemTbl];
        SET IDENTITY_INSERT [TrackedServiceItemsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'TrackedServiceItemsTbl'))
            DBCC CHECKIDENT (N'TrackedServiceItemsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [TrackedServiceItemsTbl] from ' + N'[TrackedServiceItemTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'TrackedServiceItemsTbl') IS NOT NULL SET IDENTITY_INSERT [TrackedServiceItemsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [TrackedServiceItemsTbl] from ' + N'[TrackedServiceItemTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomersTbl -> ContactsTbl
-- Mapping: columnsCount=42
--   CustomerID -> ContactID
--   CompanyName -> CompanyName
--   ContactTitle -> ContactTitle
--   ContactFirstName -> ContactFirstName
--   ContactLastName -> ContactLastName
--   ContactAltFirstName -> ContactAltFirstName
--   ContactAltLastName -> ContactAltLastName
--   Department -> Department
--   BillingAddress -> BillingAddress
--   City -> Area
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
--   CustomerTypeID -> ContactTypeID
--   EquipType -> EquipTypeID
--   CoffeePreference -> ItemPrefID
--   PriPrefQty -> PriPrefQty
--   PrefPrepTypeID -> PrefItemPrepTypeID
--   PrefPackagingID -> PrefItemPackagingID
--   SecondaryPreference -> SecondaryItemPrefID
--   SecPrefQty -> SecPrefQty
--   TypicallySecToo -> TypicallySecToo
--   PreferedAgent -> PreferedAgentID
--   SalesAgentID -> SalesAgentID
--   MachineSN -> EquipentSN
--   UsesFilter -> UsesFilter
--   autofulfill -> AutoFulfill
--   enabled -> Enabled
--   PredictionDisabled -> PredictionDisabled
--   AlwaysSendChkUp -> AlwaysSendChkUp
--   NormallyResponds -> NormallyResponds
--   ReminderCount -> ReminderCount
--   Notes -> Notes
--   SendDeliveryConfirmation -> SendDeliveryConfirmation
--   LastDateSentReminder -> LastDateSentReminder
IF OBJECT_ID(N'AccessSrc.CustomersTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactsTbl] ([ContactID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [Area], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [ContactTypeID], [EquipTypeID], [ItemPrefID], [PriPrefQty], [PrefItemPrepTypeID], [PrefItemPackagingID], [SecondaryItemPrefID], [SecPrefQty], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [EquipentSN], [UsesFilter], [AutoFulfill], [Enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]) SELECT NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([CompanyName], N'''') AS [CompanyName], NULLIF([ContactTitle], N'''') AS [ContactTitle], NULLIF([ContactFirstName], N'''') AS [ContactFirstName], NULLIF([ContactLastName], N'''') AS [ContactLastName], NULLIF([ContactAltFirstName], N'''') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'''') AS [ContactAltLastName], NULLIF([Department], N'''') AS [Department], NULLIF([BillingAddress], N'''') AS [BillingAddress], NULLIF([City], N'''') AS [Area], NULLIF([StateOrProvince], N'''') AS [StateOrProvince], NULLIF([PostalCode], N'''') AS [PostalCode], NULLIF([Country/Region], N'''') AS [Country/Region], NULLIF([PhoneNumber], N'''') AS [PhoneNumber], NULLIF([Extension], N'''') AS [Extension], NULLIF([FaxNumber], N'''') AS [FaxNu ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactsTbl] ON;
        INSERT INTO [ContactsTbl]
        (
            [ContactID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [Area], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [ContactTypeID], [EquipTypeID], [ItemPrefID], [PriPrefQty], [PrefItemPrepTypeID], [PrefItemPackagingID], [SecondaryItemPrefID], [SecPrefQty], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [EquipentSN], [UsesFilter], [AutoFulfill], [Enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]
        )
        SELECT
            NULLIF([CustomerID], N'') AS [ContactID], NULLIF([CompanyName], N'') AS [CompanyName], NULLIF([ContactTitle], N'') AS [ContactTitle], NULLIF([ContactFirstName], N'') AS [ContactFirstName], NULLIF([ContactLastName], N'') AS [ContactLastName], NULLIF([ContactAltFirstName], N'') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'') AS [ContactAltLastName], NULLIF([Department], N'') AS [Department], NULLIF([BillingAddress], N'') AS [BillingAddress], NULLIF([City], N'') AS [Area], NULLIF([StateOrProvince], N'') AS [StateOrProvince], NULLIF([PostalCode], N'') AS [PostalCode], NULLIF([Country/Region], N'') AS [Country/Region], NULLIF([PhoneNumber], N'') AS [PhoneNumber], NULLIF([Extension], N'') AS [Extension], NULLIF([FaxNumber], N'') AS [FaxNumber], NULLIF([CellNumber], N'') AS [CellNumber], NULLIF([EmailAddress], N'') AS [EmailAddress], NULLIF([AltEmailAddress], N'') AS [AltEmailAddress], NULLIF([ContractNo], N'') AS [ContractNo], NULLIF([CustomerTypeID], N'') AS [ContactTypeID], NULLIF([EquipType], N'') AS [EquipTypeID], NULLIF([CoffeePreference], N'') AS [ItemPrefID], NULLIF([PriPrefQty], N'') AS [PriPrefQty], NULLIF([PrefPrepTypeID], N'') AS [PrefItemPrepTypeID], NULLIF([PrefPackagingID], N'') AS [PrefItemPackagingID], NULLIF([SecondaryPreference], N'') AS [SecondaryItemPrefID], NULLIF([SecPrefQty], N'') AS [SecPrefQty], CASE WHEN NULLIF([TypicallySecToo], N'') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TypicallySecToo], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'')) END AS [TypicallySecToo], NULLIF([PreferedAgent], N'') AS [PreferedAgentID], NULLIF([SalesAgentID], N'') AS [SalesAgentID], NULLIF([MachineSN], N'') AS [EquipentSN], CASE WHEN NULLIF([UsesFilter], N'') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UsesFilter], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'')) END AS [UsesFilter], CASE WHEN NULLIF([autofulfill], N'') IS NULL THEN NULL WHEN NULLIF([autofulfill], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([autofulfill], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([autofulfill], N'')) END AS [AutoFulfill], CASE WHEN NULLIF([enabled], N'') IS NULL THEN NULL WHEN NULLIF([enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([enabled], N'')) END AS [Enabled], NULLIF([PredictionDisabled], N'') AS [PredictionDisabled], NULLIF([AlwaysSendChkUp], N'') AS [AlwaysSendChkUp], NULLIF([NormallyResponds], N'') AS [NormallyResponds], NULLIF([ReminderCount], N'') AS [ReminderCount], NULLIF([Notes], N'') AS [Notes], CASE WHEN NULLIF([SendDeliveryConfirmation], N'') IS NULL THEN NULL WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([SendDeliveryConfirmation], N'')) END AS [SendDeliveryConfirmation], CASE WHEN NULLIF([LastDateSentReminder], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastDateSentReminder], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N'')), CAST(NULL AS datetime2(7))) END AS [LastDateSentReminder]
        FROM [AccessSrc].[CustomersTbl];
        SET IDENTITY_INSERT [ContactsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactsTbl'))
            DBCC CHECKIDENT (N'ContactsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactsTbl] from ' + N'[AccessSrc].[CustomersTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactsTbl') IS NOT NULL SET IDENTITY_INSERT [ContactsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactsTbl] from ' + N'[AccessSrc].[CustomersTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactsTbl] ([ContactID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [Area], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [ContactTypeID], [EquipTypeID], [ItemPrefID], [PriPrefQty], [PrefItemPrepTypeID], [PrefItemPackagingID], [SecondaryItemPrefID], [SecPrefQty], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [EquipentSN], [UsesFilter], [AutoFulfill], [Enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]) SELECT NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([CompanyName], N'''') AS [CompanyName], NULLIF([ContactTitle], N'''') AS [ContactTitle], NULLIF([ContactFirstName], N'''') AS [ContactFirstName], NULLIF([ContactLastName], N'''') AS [ContactLastName], NULLIF([ContactAltFirstName], N'''') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'''') AS [ContactAltLastName], NULLIF([Department], N'''') AS [Department], NULLIF([BillingAddress], N'''') AS [BillingAddress], NULLIF([City], N'''') AS [Area], NULLIF([StateOrProvince], N'''') AS [StateOrProvince], NULLIF([PostalCode], N'''') AS [PostalCode], NULLIF([Country/Region], N'''') AS [Country/Region], NULLIF([PhoneNumber], N'''') AS [PhoneNumber], NULLIF([Extension], N'''') AS [Extension], NULLIF([FaxNumber], N'''') AS [FaxNu ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactsTbl] ON;
        INSERT INTO [ContactsTbl]
        (
            [ContactID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [Area], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [ContactTypeID], [EquipTypeID], [ItemPrefID], [PriPrefQty], [PrefItemPrepTypeID], [PrefItemPackagingID], [SecondaryItemPrefID], [SecPrefQty], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [EquipentSN], [UsesFilter], [AutoFulfill], [Enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]
        )
        SELECT
            NULLIF([CustomerID], N'') AS [ContactID], NULLIF([CompanyName], N'') AS [CompanyName], NULLIF([ContactTitle], N'') AS [ContactTitle], NULLIF([ContactFirstName], N'') AS [ContactFirstName], NULLIF([ContactLastName], N'') AS [ContactLastName], NULLIF([ContactAltFirstName], N'') AS [ContactAltFirstName], NULLIF([ContactAltLastName], N'') AS [ContactAltLastName], NULLIF([Department], N'') AS [Department], NULLIF([BillingAddress], N'') AS [BillingAddress], NULLIF([City], N'') AS [Area], NULLIF([StateOrProvince], N'') AS [StateOrProvince], NULLIF([PostalCode], N'') AS [PostalCode], NULLIF([Country/Region], N'') AS [Country/Region], NULLIF([PhoneNumber], N'') AS [PhoneNumber], NULLIF([Extension], N'') AS [Extension], NULLIF([FaxNumber], N'') AS [FaxNumber], NULLIF([CellNumber], N'') AS [CellNumber], NULLIF([EmailAddress], N'') AS [EmailAddress], NULLIF([AltEmailAddress], N'') AS [AltEmailAddress], NULLIF([ContractNo], N'') AS [ContractNo], NULLIF([CustomerTypeID], N'') AS [ContactTypeID], NULLIF([EquipType], N'') AS [EquipTypeID], NULLIF([CoffeePreference], N'') AS [ItemPrefID], NULLIF([PriPrefQty], N'') AS [PriPrefQty], NULLIF([PrefPrepTypeID], N'') AS [PrefItemPrepTypeID], NULLIF([PrefPackagingID], N'') AS [PrefItemPackagingID], NULLIF([SecondaryPreference], N'') AS [SecondaryItemPrefID], NULLIF([SecPrefQty], N'') AS [SecPrefQty], CASE WHEN NULLIF([TypicallySecToo], N'') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TypicallySecToo], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'')) END AS [TypicallySecToo], NULLIF([PreferedAgent], N'') AS [PreferedAgentID], NULLIF([SalesAgentID], N'') AS [SalesAgentID], NULLIF([MachineSN], N'') AS [EquipentSN], CASE WHEN NULLIF([UsesFilter], N'') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UsesFilter], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'')) END AS [UsesFilter], CASE WHEN NULLIF([autofulfill], N'') IS NULL THEN NULL WHEN NULLIF([autofulfill], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([autofulfill], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([autofulfill], N'')) END AS [AutoFulfill], CASE WHEN NULLIF([enabled], N'') IS NULL THEN NULL WHEN NULLIF([enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([enabled], N'')) END AS [Enabled], NULLIF([PredictionDisabled], N'') AS [PredictionDisabled], NULLIF([AlwaysSendChkUp], N'') AS [AlwaysSendChkUp], NULLIF([NormallyResponds], N'') AS [NormallyResponds], NULLIF([ReminderCount], N'') AS [ReminderCount], NULLIF([Notes], N'') AS [Notes], CASE WHEN NULLIF([SendDeliveryConfirmation], N'') IS NULL THEN NULL WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([SendDeliveryConfirmation], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([SendDeliveryConfirmation], N'')) END AS [SendDeliveryConfirmation], CASE WHEN NULLIF([LastDateSentReminder], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastDateSentReminder], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastDateSentReminder], N'')), CAST(NULL AS datetime2(7))) END AS [LastDateSentReminder]
        FROM [CustomersTbl];
        SET IDENTITY_INSERT [ContactsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactsTbl'))
            DBCC CHECKIDENT (N'ContactsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactsTbl] from ' + N'[CustomersTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactsTbl') IS NOT NULL SET IDENTITY_INSERT [ContactsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactsTbl] from ' + N'[CustomersTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemGroupTbl -> ItemGroupsTbl
-- Mapping: columnsCount=6
--   ItemGroupID -> ItemGroupID
--   GroupItemTypeID -> GroupReferenceItemID
--   ItemTypeID -> ItemID
--   ItemTypeSortPos -> ItemSortPos
--   Enabled -> Enabled
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ItemGroupTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ItemGroupsTbl] ([ItemGroupID], [GroupReferenceItemID], [ItemID], [ItemSortPos], [Enabled], [Notes]) SELECT NULLIF([ItemGroupID], N'''') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'''') AS [GroupReferenceItemID], NULLIF([ItemTypeID], N'''') AS [ItemID], NULLIF([ItemTypeSortPos], N'''') AS [ItemSortPos], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[ItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemGroupsTbl] ON;
        INSERT INTO [ItemGroupsTbl]
        (
            [ItemGroupID], [GroupReferenceItemID], [ItemID], [ItemSortPos], [Enabled], [Notes]
        )
        SELECT
            NULLIF([ItemGroupID], N'') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'') AS [GroupReferenceItemID], NULLIF([ItemTypeID], N'') AS [ItemID], NULLIF([ItemTypeSortPos], N'') AS [ItemSortPos], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
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
    PRINT N'INSERT INTO [ItemGroupsTbl] ([ItemGroupID], [GroupReferenceItemID], [ItemID], [ItemSortPos], [Enabled], [Notes]) SELECT NULLIF([ItemGroupID], N'''') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'''') AS [GroupReferenceItemID], NULLIF([ItemTypeID], N'''') AS [ItemID], NULLIF([ItemTypeSortPos], N'''') AS [ItemSortPos], CASE WHEN NULLIF([Enabled], N'''') IS NULL THEN NULL WHEN NULLIF([Enabled], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([Enabled], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'''')) END AS [Enabled], NULLIF([Notes], N'''') AS [Notes] FROM [ItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ItemGroupsTbl] ON;
        INSERT INTO [ItemGroupsTbl]
        (
            [ItemGroupID], [GroupReferenceItemID], [ItemID], [ItemSortPos], [Enabled], [Notes]
        )
        SELECT
            NULLIF([ItemGroupID], N'') AS [ItemGroupID], NULLIF([GroupItemTypeID], N'') AS [GroupReferenceItemID], NULLIF([ItemTypeID], N'') AS [ItemID], NULLIF([ItemTypeSortPos], N'') AS [ItemSortPos], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
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

-- SysDataTbl -> SysDataTbl
-- Mapping: columnsCount=7
--   ID -> ID
--   LastReoccurringDate -> LastReoccurringDate
--   DoReoccuringOrders -> DoReoccuringOrders
--   DateLastPrepDateCalcd -> DateLastPrepDateCalcd
--   MinReminderDate -> MinReminderDate
--   GroupItemTypeID -> GroupReferenceItemID
--   InternalCustomerIds -> InternalContactIDs
IF OBJECT_ID(N'AccessSrc.SysDataTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [SysDataTbl]: missing source [AccessSrc].[SysDataTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [SysDataTbl] ([ID], [LastReoccurringDate], [DoReoccuringOrders], [DateLastPrepDateCalcd], [MinReminderDate], [GroupReferenceItemID], [InternalContactIDs]) SELECT NULLIF([ID], N'''') AS [ID], CASE WHEN NULLIF([LastReoccurringDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastReoccurringDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N'''')), CAST(NULL AS datetime2(7))) END AS [LastReoccurringDate], NULLIF([DoReoccuringOrders], N'''') AS [DoReoccuringOrders], CASE WHEN NULLIF([DateLastPrepDateCalcd], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastPrepDateCalcd], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N'''')), CAST(NULL AS datetime2(7))) END AS [DateLastPrepDateCalcd], CASE WHEN NULLIF([MinRem ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [SysDataTbl] ON;
        INSERT INTO [SysDataTbl]
        (
            [ID], [LastReoccurringDate], [DoReoccuringOrders], [DateLastPrepDateCalcd], [MinReminderDate], [GroupReferenceItemID], [InternalContactIDs]
        )
        SELECT
            NULLIF([ID], N'') AS [ID], CASE WHEN NULLIF([LastReoccurringDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastReoccurringDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastReoccurringDate], N'')), CAST(NULL AS datetime2(7))) END AS [LastReoccurringDate], NULLIF([DoReoccuringOrders], N'') AS [DoReoccuringOrders], CASE WHEN NULLIF([DateLastPrepDateCalcd], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLastPrepDateCalcd], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLastPrepDateCalcd], N'')), CAST(NULL AS datetime2(7))) END AS [DateLastPrepDateCalcd], CASE WHEN NULLIF([MinReminderDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([MinReminderDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([MinReminderDate], N'')), CAST(NULL AS datetime2(7))) END AS [MinReminderDate], NULLIF([GroupItemTypeID], N'') AS [GroupReferenceItemID], NULLIF([InternalCustomerIds], N'') AS [InternalContactIDs]
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

-- ClientAwayPeriodTbl -> ContactsAwayPeriodTbl
-- Mapping: columnsCount=5
--   AwayPeriodID -> AwayPeriodID
--   ClientID -> ContactID
--   AwayStartDate -> AwayStartDate
--   AwayEndDate -> AwayEndDate
--   ReasonID -> ReasonID
IF OBJECT_ID(N'AccessSrc.ClientAwayPeriodTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactsAwayPeriodTbl] ([AwayPeriodID], [ContactID], [AwayStartDate], [AwayEndDate], [ReasonID]) SELECT NULLIF([AwayPeriodID], N'''') AS [AwayPeriodID], NULLIF([ClientID], N'''') AS [ContactID], CASE WHEN NULLIF([AwayStartDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([AwayStartDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N'''')), CAST(NULL AS datetime2(7))) END AS [AwayStartDate], CASE WHEN NULLIF([AwayEndDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([AwayEndDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N'''')), CAST(NULL AS datetime2(7))) END AS [AwayEndDate], NULLIF([ReasonID], N'''') AS [ReasonID] FROM [AccessSrc].[ClientAwayPeriodTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactsAwayPeriodTbl] ON;
        INSERT INTO [ContactsAwayPeriodTbl]
        (
            [AwayPeriodID], [ContactID], [AwayStartDate], [AwayEndDate], [ReasonID]
        )
        SELECT
            NULLIF([AwayPeriodID], N'') AS [AwayPeriodID], NULLIF([ClientID], N'') AS [ContactID], CASE WHEN NULLIF([AwayStartDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([AwayStartDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N'')), CAST(NULL AS datetime2(7))) END AS [AwayStartDate], CASE WHEN NULLIF([AwayEndDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([AwayEndDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N'')), CAST(NULL AS datetime2(7))) END AS [AwayEndDate], NULLIF([ReasonID], N'') AS [ReasonID]
        FROM [AccessSrc].[ClientAwayPeriodTbl];
        SET IDENTITY_INSERT [ContactsAwayPeriodTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactsAwayPeriodTbl'))
            DBCC CHECKIDENT (N'ContactsAwayPeriodTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactsAwayPeriodTbl] from ' + N'[AccessSrc].[ClientAwayPeriodTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactsAwayPeriodTbl') IS NOT NULL SET IDENTITY_INSERT [ContactsAwayPeriodTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactsAwayPeriodTbl] from ' + N'[AccessSrc].[ClientAwayPeriodTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactsAwayPeriodTbl] ([AwayPeriodID], [ContactID], [AwayStartDate], [AwayEndDate], [ReasonID]) SELECT NULLIF([AwayPeriodID], N'''') AS [AwayPeriodID], NULLIF([ClientID], N'''') AS [ContactID], CASE WHEN NULLIF([AwayStartDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([AwayStartDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N'''')), CAST(NULL AS datetime2(7))) END AS [AwayStartDate], CASE WHEN NULLIF([AwayEndDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([AwayEndDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N'''')), CAST(NULL AS datetime2(7))) END AS [AwayEndDate], NULLIF([ReasonID], N'''') AS [ReasonID] FROM [ClientAwayPeriodTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactsAwayPeriodTbl] ON;
        INSERT INTO [ContactsAwayPeriodTbl]
        (
            [AwayPeriodID], [ContactID], [AwayStartDate], [AwayEndDate], [ReasonID]
        )
        SELECT
            NULLIF([AwayPeriodID], N'') AS [AwayPeriodID], NULLIF([ClientID], N'') AS [ContactID], CASE WHEN NULLIF([AwayStartDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([AwayStartDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([AwayStartDate], N'')), CAST(NULL AS datetime2(7))) END AS [AwayStartDate], CASE WHEN NULLIF([AwayEndDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([AwayEndDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([AwayEndDate], N'')), CAST(NULL AS datetime2(7))) END AS [AwayEndDate], NULLIF([ReasonID], N'') AS [ReasonID]
        FROM [ClientAwayPeriodTbl];
        SET IDENTITY_INSERT [ContactsAwayPeriodTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactsAwayPeriodTbl'))
            DBCC CHECKIDENT (N'ContactsAwayPeriodTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactsAwayPeriodTbl] from ' + N'[ClientAwayPeriodTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactsAwayPeriodTbl') IS NOT NULL SET IDENTITY_INSERT [ContactsAwayPeriodTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactsAwayPeriodTbl] from ' + N'[ClientAwayPeriodTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClientUsageLinesTbl -> ContactsItemSvcSummaryTbl
-- Mapping: columnsCount=7
--   ClientUsageLineNo -> ContactsItemSvcSummaryId
--   CustomerID -> ContactID
--   Date -> UsageDate
--   CupCount -> CupCount
--   ServiceTypeId -> ItemServiceTypeID
--   Qty -> Qty
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ClientUsageLinesTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactsItemSvcSummaryTbl] with dates (only valid ContactIDs)';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactsItemSvcSummaryTbl] ON;
        INSERT INTO [ContactsItemSvcSummaryTbl]
        (
            [ContactsItemSvcSummaryId], [ContactID], [UsageDate], [CupCount], [ItemServiceTypeID], [Qty], [Notes]
        )
        SELECT
            NULLIF(src.[ClientUsageLineNo], N'') AS src.[ContactsItemSvcSummaryId], NULLIF(src.[CustomerID], N'') AS src.[ContactID], CASE WHEN NULLIF(src.[Date], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF(src.[Date], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 101), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N'')), CAST(NULL AS datetime2(7))) END AS src.[UsageDate], NULLIF(src.[CupCount], N'') AS src.[CupCount], NULLIF(src.[ServiceTypeId], N'') AS src.[ItemServiceTypeID], NULLIF(src.[Qty], N'') AS src.[Qty], NULLIF(src.[Notes], N'') AS src.[Notes]
        FROM [AccessSrc].[ClientUsageLinesTbl] src
        INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerID  -- Only migrate valid ContactIDs
        WHERE src.CustomerID IS NOT NULL;
        SET IDENTITY_INSERT [ContactsItemSvcSummaryTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactsItemSvcSummaryTbl'))
            DBCC CHECKIDENT (N'ContactsItemSvcSummaryTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactsItemSvcSummaryTbl] from ' + N'[AccessSrc].[ClientUsageLinesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactsItemSvcSummaryTbl') IS NOT NULL SET IDENTITY_INSERT [ContactsItemSvcSummaryTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactsItemSvcSummaryTbl] from ' + N'[AccessSrc].[ClientUsageLinesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactsItemSvcSummaryTbl] with dates (only valid ContactIDs)';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactsItemSvcSummaryTbl] ON;
        INSERT INTO [ContactsItemSvcSummaryTbl]
        (
            [ContactsItemSvcSummaryId], [ContactID], [UsageDate], [CupCount], [ItemServiceTypeID], [Qty], [Notes]
        )
        SELECT
            NULLIF(src.[ClientUsageLineNo], N'') AS src.[ContactsItemSvcSummaryId], NULLIF(src.[CustomerID], N'') AS src.[ContactID], CASE WHEN NULLIF(src.[Date], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF(src.[Date], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N''), 101), TRY_CONVERT(datetime2(7), NULLIF(src.[Date], N'')), CAST(NULL AS datetime2(7))) END AS src.[UsageDate], NULLIF(src.[CupCount], N'') AS src.[CupCount], NULLIF(src.[ServiceTypeId], N'') AS src.[ItemServiceTypeID], NULLIF(src.[Qty], N'') AS src.[Qty], NULLIF(src.[Notes], N'') AS src.[Notes]
        FROM [ClientUsageLinesTbl] src
        INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerID  -- Only migrate valid ContactIDs
        WHERE src.CustomerID IS NOT NULL;
        SET IDENTITY_INSERT [ContactsItemSvcSummaryTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactsItemSvcSummaryTbl'))
            DBCC CHECKIDENT (N'ContactsItemSvcSummaryTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactsItemSvcSummaryTbl] from ' + N'[ClientUsageLinesTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactsItemSvcSummaryTbl') IS NOT NULL SET IDENTITY_INSERT [ContactsItemSvcSummaryTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactsItemSvcSummaryTbl] from ' + N'[ClientUsageLinesTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ClientUsageTbl -> ContactsItemsPredictedTbl
-- Mapping: columnsCount=12
--   CustomerId -> ContactID
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
    PRINT N'INSERT INTO [ContactsItemsPredictedTbl] with dates (only valid ContactIDs)';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsItemsPredictedTbl]
        (
            [ContactID], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]
        )
        SELECT
            NULLIF(src.[CustomerId], N'') AS src.[ContactID], NULLIF(src.[LastCupCount], N'') AS src.[LastCupCount], [NextCoffeeBy] AS src.[NextCoffeeBy], [NextCleanOn] AS src.[NextCleanOn], [NextFilterEst] AS src.[NextFilterEst], [NextDescaleEst] AS src.[NextDescaleEst], [NextServiceEst] AS src.[NextServiceEst], NULLIF(src.[DailyConsumption], N'') AS src.[DailyConsumption], NULLIF(src.[FilterAveCount], N'') AS src.[FilterAveCount], NULLIF(src.[DescaleAveCount], N'') AS src.[DescaleAveCount], NULLIF(src.[ServiceAveCount], N'') AS src.[ServiceAveCount], NULLIF(src.[CleanAveCount], N'') AS src.[CleanAveCount]
        FROM [AccessSrc].[ClientUsageTbl] src
        INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerId  -- Only migrate valid ContactIDs
        WHERE src.CustomerId IS NOT NULL;
        COMMIT;
        PRINT 'OK migrate [ContactsItemsPredictedTbl] from ' + N'[AccessSrc].[ClientUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsItemsPredictedTbl] from ' + N'[AccessSrc].[ClientUsageTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsItemsPredictedTbl] with dates (only valid ContactIDs)';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsItemsPredictedTbl]
        (
            [ContactID], [LastCupCount], [NextCoffeeBy], [NextCleanOn], [NextFilterEst], [NextDescaleEst], [NextServiceEst], [DailyConsumption], [FilterAveCount], [DescaleAveCount], [ServiceAveCount], [CleanAveCount]
        )
        SELECT
            NULLIF(src.[CustomerId], N'') AS src.[ContactID], NULLIF(src.[LastCupCount], N'') AS src.[LastCupCount], [NextCoffeeBy] AS src.[NextCoffeeBy], [NextCleanOn] AS src.[NextCleanOn], [NextFilterEst] AS src.[NextFilterEst], [NextDescaleEst] AS src.[NextDescaleEst], [NextServiceEst] AS src.[NextServiceEst], NULLIF(src.[DailyConsumption], N'') AS src.[DailyConsumption], NULLIF(src.[FilterAveCount], N'') AS src.[FilterAveCount], NULLIF(src.[DescaleAveCount], N'') AS src.[DescaleAveCount], NULLIF(src.[ServiceAveCount], N'') AS src.[ServiceAveCount], NULLIF(src.[CleanAveCount], N'') AS src.[CleanAveCount]
        FROM [ClientUsageTbl] src
        INNER JOIN ContactsTbl c ON c.ContactID = src.CustomerId  -- Only migrate valid ContactIDs
        WHERE src.CustomerId IS NOT NULL;
        COMMIT;
        PRINT 'OK migrate [ContactsItemsPredictedTbl] from ' + N'[ClientUsageTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        PRINT 'ERROR migrate [ContactsItemsPredictedTbl] from ' + N'[ClientUsageTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomersAccInfoTbl -> ContactsAccInfoTbl
-- Mapping: columnsCount=30
--   CustomersAccInfoID -> ContactsAccInfoID
--   CustomerID -> ContactID
--   RequiresPurchOrder -> RequiresPurchOrder
--   CustomerVATNo -> ContactVATNo
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
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactsAccInfoTbl] ([ContactsAccInfoID], [ContactID], [RequiresPurchOrder], [ContactVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]) SELECT NULLIF([CustomersAccInfoID], N'''') AS [ContactsAccInfoID], NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([RequiresPurchOrder], N'''') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'''') AS [ContactVATNo], NULLIF([BillAddr1], N'''') AS [BillAddr1], NULLIF([BillAddr2], N'''') AS [BillAddr2], NULLIF([BillAddr3], N'''') AS [BillAddr3], NULLIF([BillAddr4], N'''') AS [BillAddr4], NULLIF([BillAddr5], N'''') AS [BillAddr5], NULLIF([ShipAddr1], N'''') AS [ShipAddr1], NULLIF([ShipAddr2], N'''') AS [ShipAddr2], NULLIF([ShipAddr3], N'''') AS [ShipAddr3], NULLIF([ShipAddr4], N'''') AS [ShipAddr4], NULLIF([ShipAddr5], N'''') AS [ShipAddr5], NULLIF([AccEmail], N'''') AS [AccEmail], NULLIF([AltAccEmail], N'''') AS [AltAccEmail], NULLIF([PaymentTermID], N'''') AS [PaymentTermID], NULLIF([Limit], N'''') AS [Limit], NULLIF([FullCoName], N'''') AS [FullCoName], NULLIF([AccFirstName], N'''') AS [AccFirstName], NULLIF([AccLastName], N'''') AS [AccLastName], NULLIF([AltAccFirstName], N'''') AS [AltAccFirstName], NULLIF([AltAccLastName], N'''') AS [AltAccLastName], NULLIF([Pri ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactsAccInfoTbl] ON;
        INSERT INTO [ContactsAccInfoTbl]
        (
            [ContactsAccInfoID], [ContactID], [RequiresPurchOrder], [ContactVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]
        )
        SELECT
            NULLIF([CustomersAccInfoID], N'') AS [ContactsAccInfoID], NULLIF([CustomerID], N'') AS [ContactID], NULLIF([RequiresPurchOrder], N'') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'') AS [ContactVATNo], NULLIF([BillAddr1], N'') AS [BillAddr1], NULLIF([BillAddr2], N'') AS [BillAddr2], NULLIF([BillAddr3], N'') AS [BillAddr3], NULLIF([BillAddr4], N'') AS [BillAddr4], NULLIF([BillAddr5], N'') AS [BillAddr5], NULLIF([ShipAddr1], N'') AS [ShipAddr1], NULLIF([ShipAddr2], N'') AS [ShipAddr2], NULLIF([ShipAddr3], N'') AS [ShipAddr3], NULLIF([ShipAddr4], N'') AS [ShipAddr4], NULLIF([ShipAddr5], N'') AS [ShipAddr5], NULLIF([AccEmail], N'') AS [AccEmail], NULLIF([AltAccEmail], N'') AS [AltAccEmail], NULLIF([PaymentTermID], N'') AS [PaymentTermID], NULLIF([Limit], N'') AS [Limit], NULLIF([FullCoName], N'') AS [FullCoName], NULLIF([AccFirstName], N'') AS [AccFirstName], NULLIF([AccLastName], N'') AS [AccLastName], NULLIF([AltAccFirstName], N'') AS [AltAccFirstName], NULLIF([AltAccLastName], N'') AS [AltAccLastName], NULLIF([PriceLevelID], N'') AS [PriceLevelID], NULLIF([InvoiceTypeID], N'') AS [InvoiceTypeID], NULLIF([RegNo], N'') AS [RegNo], NULLIF([BankAccNo], N'') AS [BankAccNo], NULLIF([BankBranch], N'') AS [BankBranch], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomersAccInfoTbl];
        SET IDENTITY_INSERT [ContactsAccInfoTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactsAccInfoTbl'))
            DBCC CHECKIDENT (N'ContactsAccInfoTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactsAccInfoTbl] from ' + N'[AccessSrc].[CustomersAccInfoTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactsAccInfoTbl') IS NOT NULL SET IDENTITY_INSERT [ContactsAccInfoTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactsAccInfoTbl] from ' + N'[AccessSrc].[CustomersAccInfoTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactsAccInfoTbl] ([ContactsAccInfoID], [ContactID], [RequiresPurchOrder], [ContactVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]) SELECT NULLIF([CustomersAccInfoID], N'''') AS [ContactsAccInfoID], NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([RequiresPurchOrder], N'''') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'''') AS [ContactVATNo], NULLIF([BillAddr1], N'''') AS [BillAddr1], NULLIF([BillAddr2], N'''') AS [BillAddr2], NULLIF([BillAddr3], N'''') AS [BillAddr3], NULLIF([BillAddr4], N'''') AS [BillAddr4], NULLIF([BillAddr5], N'''') AS [BillAddr5], NULLIF([ShipAddr1], N'''') AS [ShipAddr1], NULLIF([ShipAddr2], N'''') AS [ShipAddr2], NULLIF([ShipAddr3], N'''') AS [ShipAddr3], NULLIF([ShipAddr4], N'''') AS [ShipAddr4], NULLIF([ShipAddr5], N'''') AS [ShipAddr5], NULLIF([AccEmail], N'''') AS [AccEmail], NULLIF([AltAccEmail], N'''') AS [AltAccEmail], NULLIF([PaymentTermID], N'''') AS [PaymentTermID], NULLIF([Limit], N'''') AS [Limit], NULLIF([FullCoName], N'''') AS [FullCoName], NULLIF([AccFirstName], N'''') AS [AccFirstName], NULLIF([AccLastName], N'''') AS [AccLastName], NULLIF([AltAccFirstName], N'''') AS [AltAccFirstName], NULLIF([AltAccLastName], N'''') AS [AltAccLastName], NULLIF([Pri ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactsAccInfoTbl] ON;
        INSERT INTO [ContactsAccInfoTbl]
        (
            [ContactsAccInfoID], [ContactID], [RequiresPurchOrder], [ContactVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]
        )
        SELECT
            NULLIF([CustomersAccInfoID], N'') AS [ContactsAccInfoID], NULLIF([CustomerID], N'') AS [ContactID], NULLIF([RequiresPurchOrder], N'') AS [RequiresPurchOrder], NULLIF([CustomerVATNo], N'') AS [ContactVATNo], NULLIF([BillAddr1], N'') AS [BillAddr1], NULLIF([BillAddr2], N'') AS [BillAddr2], NULLIF([BillAddr3], N'') AS [BillAddr3], NULLIF([BillAddr4], N'') AS [BillAddr4], NULLIF([BillAddr5], N'') AS [BillAddr5], NULLIF([ShipAddr1], N'') AS [ShipAddr1], NULLIF([ShipAddr2], N'') AS [ShipAddr2], NULLIF([ShipAddr3], N'') AS [ShipAddr3], NULLIF([ShipAddr4], N'') AS [ShipAddr4], NULLIF([ShipAddr5], N'') AS [ShipAddr5], NULLIF([AccEmail], N'') AS [AccEmail], NULLIF([AltAccEmail], N'') AS [AltAccEmail], NULLIF([PaymentTermID], N'') AS [PaymentTermID], NULLIF([Limit], N'') AS [Limit], NULLIF([FullCoName], N'') AS [FullCoName], NULLIF([AccFirstName], N'') AS [AccFirstName], NULLIF([AccLastName], N'') AS [AccLastName], NULLIF([AltAccFirstName], N'') AS [AltAccFirstName], NULLIF([AltAccLastName], N'') AS [AltAccLastName], NULLIF([PriceLevelID], N'') AS [PriceLevelID], NULLIF([InvoiceTypeID], N'') AS [InvoiceTypeID], NULLIF([RegNo], N'') AS [RegNo], NULLIF([BankAccNo], N'') AS [BankAccNo], NULLIF([BankBranch], N'') AS [BankBranch], CASE WHEN NULLIF([Enabled], N'') IS NULL THEN NULL WHEN NULLIF([Enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Enabled], N'')) END AS [Enabled], NULLIF([Notes], N'') AS [Notes]
        FROM [CustomersAccInfoTbl];
        SET IDENTITY_INSERT [ContactsAccInfoTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactsAccInfoTbl'))
            DBCC CHECKIDENT (N'ContactsAccInfoTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactsAccInfoTbl] from ' + N'[CustomersAccInfoTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactsAccInfoTbl') IS NOT NULL SET IDENTITY_INSERT [ContactsAccInfoTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactsAccInfoTbl] from ' + N'[CustomersAccInfoTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- CustomerTrackedServiceItemsTbl -> ContactTrackedServiceItemsTbl
-- Mapping: columnsCount=4
--   CustomerTrackedServiceItemsID -> ContactTrackedServiceItemsID
--   CustomerTypeID -> ContactTypeID
--   ServiceTypeID -> ItemServiceTypeID
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.CustomerTrackedServiceItemsTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactTrackedServiceItemsTbl] ([ContactTrackedServiceItemsID], [ContactTypeID], [ItemServiceTypeID], [Notes]) SELECT NULLIF([CustomerTrackedServiceItemsID], N'''') AS [ContactTrackedServiceItemsID], NULLIF([CustomerTypeID], N'''') AS [ContactTypeID], NULLIF([ServiceTypeID], N'''') AS [ItemServiceTypeID], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[CustomerTrackedServiceItemsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactTrackedServiceItemsTbl] ON;
        INSERT INTO [ContactTrackedServiceItemsTbl]
        (
            [ContactTrackedServiceItemsID], [ContactTypeID], [ItemServiceTypeID], [Notes]
        )
        SELECT
            NULLIF([CustomerTrackedServiceItemsID], N'') AS [ContactTrackedServiceItemsID], NULLIF([CustomerTypeID], N'') AS [ContactTypeID], NULLIF([ServiceTypeID], N'') AS [ItemServiceTypeID], NULLIF([Notes], N'') AS [Notes]
        FROM [AccessSrc].[CustomerTrackedServiceItemsTbl];
        SET IDENTITY_INSERT [ContactTrackedServiceItemsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactTrackedServiceItemsTbl'))
            DBCC CHECKIDENT (N'ContactTrackedServiceItemsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactTrackedServiceItemsTbl] from ' + N'[AccessSrc].[CustomerTrackedServiceItemsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactTrackedServiceItemsTbl') IS NOT NULL SET IDENTITY_INSERT [ContactTrackedServiceItemsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactTrackedServiceItemsTbl] from ' + N'[AccessSrc].[CustomerTrackedServiceItemsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [ContactTrackedServiceItemsTbl] ([ContactTrackedServiceItemsID], [ContactTypeID], [ItemServiceTypeID], [Notes]) SELECT NULLIF([CustomerTrackedServiceItemsID], N'''') AS [ContactTrackedServiceItemsID], NULLIF([CustomerTypeID], N'''') AS [ContactTypeID], NULLIF([ServiceTypeID], N'''') AS [ItemServiceTypeID], NULLIF([Notes], N'''') AS [Notes] FROM [CustomerTrackedServiceItemsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [ContactTrackedServiceItemsTbl] ON;
        INSERT INTO [ContactTrackedServiceItemsTbl]
        (
            [ContactTrackedServiceItemsID], [ContactTypeID], [ItemServiceTypeID], [Notes]
        )
        SELECT
            NULLIF([CustomerTrackedServiceItemsID], N'') AS [ContactTrackedServiceItemsID], NULLIF([CustomerTypeID], N'') AS [ContactTypeID], NULLIF([ServiceTypeID], N'') AS [ItemServiceTypeID], NULLIF([Notes], N'') AS [Notes]
        FROM [CustomerTrackedServiceItemsTbl];
        SET IDENTITY_INSERT [ContactTrackedServiceItemsTbl] OFF;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'ContactTrackedServiceItemsTbl'))
            DBCC CHECKIDENT (N'ContactTrackedServiceItemsTbl', RESEED);
        COMMIT;
        PRINT 'OK migrate [ContactTrackedServiceItemsTbl] from ' + N'[CustomerTrackedServiceItemsTbl]';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK;
        BEGIN TRY
            IF OBJECT_ID(N'ContactTrackedServiceItemsTbl') IS NOT NULL SET IDENTITY_INSERT [ContactTrackedServiceItemsTbl] OFF;
        END TRY BEGIN CATCH END CATCH
        PRINT 'ERROR migrate [ContactTrackedServiceItemsTbl] from ' + N'[CustomerTrackedServiceItemsTbl]' + ': ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ItemUsageTbl -> ContactsItemUsageTbl
-- Mapping: columnsCount=7
--   CustomerID -> ContactID
--   Date -> DeliveryDate
--   ItemProvided -> ItemProvidedID
--   AmountProvided -> QtyProvided
--   PrepTypeID -> ItemPrepTypeID
--   PackagingID -> ItemPackagingID
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.ItemUsageTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=OFF):';
    PRINT N'INSERT INTO [ContactsItemUsageTbl] ([ContactID], [DeliveryDate], [ItemProvidedID], [QtyProvided], [ItemPrepTypeID], [ItemPackagingID], [Notes]) SELECT NULLIF([CustomerID], N'''') AS [ContactID], CASE WHEN NULLIF([Date], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([Date], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([Date], N'''')), CAST(NULL AS datetime2(7))) END AS [DeliveryDate], NULLIF([ItemProvided], N'''') AS [ItemProvidedID], NULLIF([AmountProvided], N'''') AS [QtyProvided], NULLIF([PrepTypeID], N'''') AS [ItemPrepTypeID], NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[ItemUsageTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsItemUsageTbl]
        (
            [ContactID], [DeliveryDate], [ItemProvidedID], [QtyProvided], [ItemPrepTypeID], [ItemPackagingID], [Notes]
        )
        SELECT
            NULLIF([CustomerID], N'') AS [ContactID], CASE WHEN NULLIF([Date], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([Date], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([Date], N'')), CAST(NULL AS datetime2(7))) END AS [DeliveryDate], NULLIF([ItemProvided], N'') AS [ItemProvidedID], NULLIF([AmountProvided], N'') AS [QtyProvided], NULLIF([PrepTypeID], N'') AS [ItemPrepTypeID], NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([Notes], N'') AS [Notes]
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
    PRINT N'INSERT INTO [ContactsItemUsageTbl] ([ContactID], [DeliveryDate], [ItemProvidedID], [QtyProvided], [ItemPrepTypeID], [ItemPackagingID], [Notes]) SELECT NULLIF([CustomerID], N'''') AS [ContactID], CASE WHEN NULLIF([Date], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([Date], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([Date], N'''')), CAST(NULL AS datetime2(7))) END AS [DeliveryDate], NULLIF([ItemProvided], N'''') AS [ItemProvidedID], NULLIF([AmountProvided], N'''') AS [QtyProvided], NULLIF([PrepTypeID], N'''') AS [ItemPrepTypeID], NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([Notes], N'''') AS [Notes] FROM [ItemUsageTbl];';
    BEGIN TRY
        BEGIN TRAN;
        INSERT INTO [ContactsItemUsageTbl]
        (
            [ContactID], [DeliveryDate], [ItemProvidedID], [QtyProvided], [ItemPrepTypeID], [ItemPackagingID], [Notes]
        )
        SELECT
            NULLIF([CustomerID], N'') AS [ContactID], CASE WHEN NULLIF([Date], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([Date], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([Date], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([Date], N'')), CAST(NULL AS datetime2(7))) END AS [DeliveryDate], NULLIF([ItemProvided], N'') AS [ItemProvidedID], NULLIF([AmountProvided], N'') AS [QtyProvided], NULLIF([PrepTypeID], N'') AS [ItemPrepTypeID], NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([Notes], N'') AS [Notes]
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

-- RepairsTbl -> RepairsTbl
-- Mapping: columnsCount=22
--   RepairID -> RepairID
--   CustomerID -> ContactID
--   ContactName -> ContactName
--   ContactEmail -> ContactEmail
--   JobCardNumber -> JobCardNumber
--   DateLogged -> DateLogged
--   LastStatusChange -> LastStatusChange
--   MachineTypeID -> EquipTypeID
--   MachineSerialNumber -> EquipSerialNumber
--   SwopOutMachineID -> SwopOutMachineID
--   MachineConditionID -> EquipConditionID
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
    PRINT N'INSERT INTO [RepairsTbl] ([RepairID], [ContactID], [ContactName], [ContactEmail], [JobCardNumber], [DateLogged], [LastStatusChange], [EquipTypeID], [EquipSerialNumber], [SwopOutMachineID], [EquipConditionID], [TakenFrother], [TakenBeanLid], [TakenWaterLid], [BrokenFrother], [BrokenBeanLid], [BrokenWaterLid], [RepairFaultID], [RepairFaultDesc], [RepairStatusID], [RelatedOrderID], [Notes]) SELECT NULLIF([RepairID], N'''') AS [RepairID], NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([ContactName], N'''') AS [ContactName], NULLIF([ContactEmail], N'''') AS [ContactEmail], NULLIF([JobCardNumber], N'''') AS [JobCardNumber], CASE WHEN NULLIF([DateLogged], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLogged], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N'''')), CAST(NULL AS datetime2(7))) END AS [DateLogged], CASE WHEN NULLIF([LastStatusChange], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastStatusChange], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([L ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [RepairsTbl] ON;
        INSERT INTO [RepairsTbl]
        (
            [RepairID], [ContactID], [ContactName], [ContactEmail], [JobCardNumber], [DateLogged], [LastStatusChange], [EquipTypeID], [EquipSerialNumber], [SwopOutMachineID], [EquipConditionID], [TakenFrother], [TakenBeanLid], [TakenWaterLid], [BrokenFrother], [BrokenBeanLid], [BrokenWaterLid], [RepairFaultID], [RepairFaultDesc], [RepairStatusID], [RelatedOrderID], [Notes]
        )
        SELECT
            NULLIF([RepairID], N'') AS [RepairID], NULLIF([CustomerID], N'') AS [ContactID], NULLIF([ContactName], N'') AS [ContactName], NULLIF([ContactEmail], N'') AS [ContactEmail], NULLIF([JobCardNumber], N'') AS [JobCardNumber], CASE WHEN NULLIF([DateLogged], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateLogged], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateLogged], N'')), CAST(NULL AS datetime2(7))) END AS [DateLogged], CASE WHEN NULLIF([LastStatusChange], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastStatusChange], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastStatusChange], N'')), CAST(NULL AS datetime2(7))) END AS [LastStatusChange], NULLIF([MachineTypeID], N'') AS [EquipTypeID], NULLIF([MachineSerialNumber], N'') AS [EquipSerialNumber], NULLIF([SwopOutMachineID], N'') AS [SwopOutMachineID], NULLIF([MachineConditionID], N'') AS [EquipConditionID], NULLIF([TakenFrother], N'') AS [TakenFrother], NULLIF([TakenBeanLid], N'') AS [TakenBeanLid], NULLIF([TakenWaterLid], N'') AS [TakenWaterLid], NULLIF([BrokenFrother], N'') AS [BrokenFrother], NULLIF([BrokenBeanLid], N'') AS [BrokenBeanLid], NULLIF([BrokenWaterLid], N'') AS [BrokenWaterLid], NULLIF([RepairFaultID], N'') AS [RepairFaultID], NULLIF([RepairFaultDesc], N'') AS [RepairFaultDesc], NULLIF([RepairStatusID], N'') AS [RepairStatusID], NULLIF([RelatedOrderID], N'') AS [RelatedOrderID], NULLIF([Notes], N'') AS [Notes]
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

-- SentRemindersLogTbl -> SentRemindersLogTbl
-- Mapping: columnsCount=7
--   ReminderID -> ReminderID
--   CustomerID -> ContactID
--   DateSentReminder -> DateSentReminder
--   NextPrepDate -> NextPrepDate
--   ReminderSent -> ReminderSent
--   HadAutoFulfilItem -> HadAutoFulfilItem
--   HadReoccurItems -> HadRecurrItems
IF OBJECT_ID(N'AccessSrc.SentRemindersLogTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [SentRemindersLogTbl]: missing source [AccessSrc].[SentRemindersLogTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [SentRemindersLogTbl] ([ReminderID], [ContactID], [DateSentReminder], [NextPrepDate], [ReminderSent], [HadAutoFulfilItem], [HadRecurrItems]) SELECT NULLIF([ReminderID], N'''') AS [ReminderID], NULLIF([CustomerID], N'''') AS [ContactID], CASE WHEN NULLIF([DateSentReminder], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateSentReminder], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N'''')), CAST(NULL AS datetime2(7))) END AS [DateSentReminder], CASE WHEN NULLIF([NextPrepDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextPrepDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N'''')), CAST(NULL AS datetime2(7))) END AS [NextPrepDate], CASE WHEN NULLIF([ReminderSent], N'''') IS NULL THEN NULL WHEN NULLIF([ReminderSent], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [SentRemindersLogTbl] ON;
        INSERT INTO [SentRemindersLogTbl]
        (
            [ReminderID], [ContactID], [DateSentReminder], [NextPrepDate], [ReminderSent], [HadAutoFulfilItem], [HadRecurrItems]
        )
        SELECT
            NULLIF([ReminderID], N'') AS [ReminderID], NULLIF([CustomerID], N'') AS [ContactID], CASE WHEN NULLIF([DateSentReminder], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([DateSentReminder], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([DateSentReminder], N'')), CAST(NULL AS datetime2(7))) END AS [DateSentReminder], CASE WHEN NULLIF([NextPrepDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([NextPrepDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([NextPrepDate], N'')), CAST(NULL AS datetime2(7))) END AS [NextPrepDate], CASE WHEN NULLIF([ReminderSent], N'') IS NULL THEN NULL WHEN NULLIF([ReminderSent], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([ReminderSent], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([ReminderSent], N'')) END AS [ReminderSent], NULLIF([HadAutoFulfilItem], N'') AS [HadAutoFulfilItem], NULLIF([HadReoccurItems], N'') AS [HadRecurrItems]
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

-- TempCoffeecheckupCustomerTbl -> TempCoffeecheckupCustomerTbl
-- Mapping: columnsCount=25
--   TCCID -> TCCID
--   CustomerID -> ContactID
--   CompanyName -> CompanyName
--   ContactFirstName -> ContactFirstName
--   ContactAltFirstName -> ContactAltFirstName
--   CityID -> AreaID
--   EmailAddress -> EmailAddress
--   AltEmailAddress -> AltEmailAddress
--   CustomerTypeID -> ContactTypeID
--   EquipTypeID -> EquipTypeID
--   TypicallySecToo -> TypicallySecToo
--   PreferedAgentID -> PreferedAgentID
--   SalesAgentID -> SalesAgentID
--   UsesFilter -> UsesFilter
--   enabled -> Enabled
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
    PRINT N'INSERT INTO [TempCoffeecheckupCustomerTbl] ([TCCID], [ContactID], [CompanyName], [ContactFirstName], [ContactAltFirstName], [AreaID], [EmailAddress], [AltEmailAddress], [ContactTypeID], [EquipTypeID], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [UsesFilter], [Enabled], [AlwaysSendChkUp], [ReminderCount], [NextPrepDate], [NextDeliveryDate], [NextCoffee], [NextClean], [NextFilter], [NextDescal], [NextService], [RequiresPurchOrder]) SELECT NULLIF([TCCID], N'''') AS [TCCID], NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([CompanyName], N'''') AS [CompanyName], NULLIF([ContactFirstName], N'''') AS [ContactFirstName], NULLIF([ContactAltFirstName], N'''') AS [ContactAltFirstName], NULLIF([CityID], N'''') AS [AreaID], NULLIF([EmailAddress], N'''') AS [EmailAddress], NULLIF([AltEmailAddress], N'''') AS [AltEmailAddress], NULLIF([CustomerTypeID], N'''') AS [ContactTypeID], NULLIF([EquipTypeID], N'''') AS [EquipTypeID], CASE WHEN NULLIF([TypicallySecToo], N'''') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([TypicallySecToo], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'''')) END AS [TypicallySecToo], NULLIF([PreferedAgentID], N'''') AS [PreferedAgentID], NULLIF([SalesAgentID], N'''') AS [SalesAgentID], CASE WHEN NULLIF([UsesFilter], N'''') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempCoffeecheckupCustomerTbl] ON;
        INSERT INTO [TempCoffeecheckupCustomerTbl]
        (
            [TCCID], [ContactID], [CompanyName], [ContactFirstName], [ContactAltFirstName], [AreaID], [EmailAddress], [AltEmailAddress], [ContactTypeID], [EquipTypeID], [TypicallySecToo], [PreferedAgentID], [SalesAgentID], [UsesFilter], [Enabled], [AlwaysSendChkUp], [ReminderCount], [NextPrepDate], [NextDeliveryDate], [NextCoffee], [NextClean], [NextFilter], [NextDescal], [NextService], [RequiresPurchOrder]
        )
        SELECT
            NULLIF([TCCID], N'') AS [TCCID], NULLIF([CustomerID], N'') AS [ContactID], NULLIF([CompanyName], N'') AS [CompanyName], NULLIF([ContactFirstName], N'') AS [ContactFirstName], NULLIF([ContactAltFirstName], N'') AS [ContactAltFirstName], NULLIF([CityID], N'') AS [AreaID], NULLIF([EmailAddress], N'') AS [EmailAddress], NULLIF([AltEmailAddress], N'') AS [AltEmailAddress], NULLIF([CustomerTypeID], N'') AS [ContactTypeID], NULLIF([EquipTypeID], N'') AS [EquipTypeID], CASE WHEN NULLIF([TypicallySecToo], N'') IS NULL THEN NULL WHEN NULLIF([TypicallySecToo], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([TypicallySecToo], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([TypicallySecToo], N'')) END AS [TypicallySecToo], NULLIF([PreferedAgentID], N'') AS [PreferedAgentID], NULLIF([SalesAgentID], N'') AS [SalesAgentID], CASE WHEN NULLIF([UsesFilter], N'') IS NULL THEN NULL WHEN NULLIF([UsesFilter], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([UsesFilter], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([UsesFilter], N'')) END AS [UsesFilter], CASE WHEN NULLIF([enabled], N'') IS NULL THEN NULL WHEN NULLIF([enabled], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([enabled], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([enabled], N'')) END AS [Enabled], NULLIF([AlwaysSendChkUp], N'') AS [AlwaysSendChkUp], NULLIF([ReminderCount], N'') AS [ReminderCount], [NextPrepDate] AS [NextPrepDate], [NextDeliveryDate] AS [NextDeliveryDate], [NextCoffee] AS [NextCoffee], [NextClean] AS [NextClean], [NextFilter] AS [NextFilter], [NextDescal] AS [NextDescal], [NextService] AS [NextService], NULLIF([RequiresPurchOrder], N'') AS [RequiresPurchOrder]
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
--   CustomerID -> ContactID
--   ItemID -> ItemID
--   ItemQty -> ItemQty
--   ItemPrepID -> ItemPrepID
--   ItemPackagID -> ItemPackagingID
--   AutoFulfill -> AutoFulfill
--   NextDateRequired -> NextDateRequired
--   ReoccurOrderID -> RecurringOrderItemID
IF OBJECT_ID(N'AccessSrc.TempCoffeecheckupItemsTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TempCoffeecheckupItemsTbl]: missing source [AccessSrc].[TempCoffeecheckupItemsTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TempCoffeecheckupItemsTbl] ([TCIID], [ContactID], [ItemID], [ItemQty], [ItemPrepID], [ItemPackagingID], [AutoFulfill], [NextDateRequired], [RecurringOrderItemID]) SELECT NULLIF([TCIID], N'''') AS [TCIID], NULLIF([CustomerID], N'''') AS [ContactID], NULLIF([ItemID], N'''') AS [ItemID], NULLIF([ItemQty], N'''') AS [ItemQty], NULLIF([ItemPrepID], N'''') AS [ItemPrepID], NULLIF([ItemPackagID], N'''') AS [ItemPackagingID], CASE WHEN NULLIF([AutoFulfill], N'''') IS NULL THEN NULL WHEN NULLIF([AutoFulfill], N'''') IN (N''1'', N''-1'', N''true'', N''TRUE'', N''yes'', N''YES'', N''Y'', N''y'') THEN 1 WHEN NULLIF([AutoFulfill], N'''') IN (N''0'', N''false'', N''FALSE'', N''no'', N''NO'', N''N'', N''n'') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([AutoFulfill], N'''')) END AS [AutoFulfill], [NextDateRequired] AS [NextDateRequired], NULLIF([ReoccurOrderID], N'''') AS [RecurringOrderItemID] FROM [AccessSrc].[TempCoffeecheckupItemsTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempCoffeecheckupItemsTbl] ON;
        INSERT INTO [TempCoffeecheckupItemsTbl]
        (
            [TCIID], [ContactID], [ItemID], [ItemQty], [ItemPrepID], [ItemPackagingID], [AutoFulfill], [NextDateRequired], [RecurringOrderItemID]
        )
        SELECT
            NULLIF([TCIID], N'') AS [TCIID], NULLIF([CustomerID], N'') AS [ContactID], NULLIF([ItemID], N'') AS [ItemID], NULLIF([ItemQty], N'') AS [ItemQty], NULLIF([ItemPrepID], N'') AS [ItemPrepID], NULLIF([ItemPackagID], N'') AS [ItemPackagingID], CASE WHEN NULLIF([AutoFulfill], N'') IS NULL THEN NULL WHEN NULLIF([AutoFulfill], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([AutoFulfill], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([AutoFulfill], N'')) END AS [AutoFulfill], [NextDateRequired] AS [NextDateRequired], NULLIF([ReoccurOrderID], N'') AS [RecurringOrderItemID]
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
--   CustomerID -> ContactID
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
    PRINT N'INSERT INTO [TempOrdersHeaderTbl] ([TOHeaderID], [ContactID], [OrderDate], [RoastDate], [RequiredByDate], [ToBeDeliveredByID], [Confirmed], [Done], [Notes]) SELECT NULLIF([TOHeaderID], N'''') AS [TOHeaderID], NULLIF([CustomerID], N'''') AS [ContactID], CASE WHEN NULLIF([OrderDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([OrderDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N'''')), CAST(NULL AS datetime2(7))) END AS [OrderDate], CASE WHEN NULLIF([RoastDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RoastDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N'''')), CAST(NULL AS datetime2(7))) END AS [RoastDate], CASE WHEN NULLIF([RequiredByDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RequiredByDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([Required ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempOrdersHeaderTbl] ON;
        INSERT INTO [TempOrdersHeaderTbl]
        (
            [TOHeaderID], [ContactID], [OrderDate], [RoastDate], [RequiredByDate], [ToBeDeliveredByID], [Confirmed], [Done], [Notes]
        )
        SELECT
            NULLIF([TOHeaderID], N'') AS [TOHeaderID], NULLIF([CustomerID], N'') AS [ContactID], CASE WHEN NULLIF([OrderDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([OrderDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N'')), CAST(NULL AS datetime2(7))) END AS [OrderDate], CASE WHEN NULLIF([RoastDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RoastDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N'')), CAST(NULL AS datetime2(7))) END AS [RoastDate], CASE WHEN NULLIF([RequiredByDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RequiredByDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N'')), CAST(NULL AS datetime2(7))) END AS [RequiredByDate], NULLIF([ToBeDeliveredByID], N'') AS [ToBeDeliveredByID], CASE WHEN NULLIF([Confirmed], N'') IS NULL THEN NULL WHEN NULLIF([Confirmed], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Confirmed], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Confirmed], N'')) END AS [Confirmed], CASE WHEN NULLIF([Done], N'') IS NULL THEN NULL WHEN NULLIF([Done], N'') IN (N'1', N'-1', N'true', N'TRUE', N'yes', N'YES', N'Y', N'y') THEN 1 WHEN NULLIF([Done], N'') IN (N'0', N'false', N'FALSE', N'no', N'NO', N'N', N'n') THEN 0 ELSE TRY_CONVERT(bit, NULLIF([Done], N'')) END AS [Done], NULLIF([Notes], N'') AS [Notes]
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
--   TempOrderId -> TempOrderID
--   OrderID -> OrderID
--   CustomerId -> ContactID
--   OrderDate -> OrderDate
--   RoastDate -> RoastDate
--   ItemTypeID -> ItemID
--   ServiceTypeId -> ItemServiceTypeID
--   PrepTypeID -> ItemPrepTypeID
--   PackagingId -> ItemPackagingID
--   QuantityOrdered -> QtyOrdered
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
    PRINT N'INSERT INTO [TempOrdersTbl] ([TempOrderID], [OrderID], [ContactID], [OrderDate], [RoastDate], [ItemID], [ItemServiceTypeID], [ItemPrepTypeID], [ItemPackagingID], [QtyOrdered], [RequiredByDate], [Delivered], [Notes]) SELECT NULLIF([TempOrderId], N'''') AS [TempOrderID], NULLIF([OrderID], N'''') AS [OrderID], NULLIF([CustomerId], N'''') AS [ContactID], CASE WHEN NULLIF([OrderDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([OrderDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N'''')), CAST(NULL AS datetime2(7))) END AS [OrderDate], CASE WHEN NULLIF([RoastDate], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RoastDate], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N'''')), CAST(NULL AS datetime2(7))) END AS [RoastDate], NULLIF([ItemTypeID], N'''') AS [ItemID], NULLIF([ServiceTypeId], N'''') AS [ItemServiceTypeID], NULLIF([PrepTypeID], N'''') AS [ItemPrepTypeI ... [truncated]';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempOrdersTbl] ON;
        INSERT INTO [TempOrdersTbl]
        (
            [TempOrderID], [OrderID], [ContactID], [OrderDate], [RoastDate], [ItemID], [ItemServiceTypeID], [ItemPrepTypeID], [ItemPackagingID], [QtyOrdered], [RequiredByDate], [Delivered], [Notes]
        )
        SELECT
            NULLIF([TempOrderId], N'') AS [TempOrderID], NULLIF([OrderID], N'') AS [OrderID], NULLIF([CustomerId], N'') AS [ContactID], CASE WHEN NULLIF([OrderDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([OrderDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([OrderDate], N'')), CAST(NULL AS datetime2(7))) END AS [OrderDate], CASE WHEN NULLIF([RoastDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RoastDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([RoastDate], N'')), CAST(NULL AS datetime2(7))) END AS [RoastDate], NULLIF([ItemTypeID], N'') AS [ItemID], NULLIF([ServiceTypeId], N'') AS [ItemServiceTypeID], NULLIF([PrepTypeID], N'') AS [ItemPrepTypeID], NULLIF([PackagingId], N'') AS [ItemPackagingID], NULLIF([QuantityOrdered], N'') AS [QtyOrdered], CASE WHEN NULLIF([RequiredByDate], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([RequiredByDate], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([RequiredByDate], N'')), CAST(NULL AS datetime2(7))) END AS [RequiredByDate], NULLIF([Delivered], N'') AS [Delivered], NULLIF([Notes], N'') AS [Notes]
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

-- UsedItemGroupTbl -> UsedItemGroupsTbl
-- Mapping: columnsCount=7
--   UsedItemGroupID -> UsedItemGroupID
--   ContactID -> ContactID
--   GroupItemTypeID -> GroupReferenceItemID
--   LastItemTypeID -> LastItemID
--   LastItemTypeSortPos -> LastItemSortPos
--   LastItemDateChanged -> LastItemDateChanged
--   Notes -> Notes
IF OBJECT_ID(N'AccessSrc.UsedItemGroupTbl') IS NOT NULL
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [UsedItemGroupsTbl] ([UsedItemGroupID], [ContactID], [GroupReferenceItemID], [LastItemID], [LastItemSortPos], [LastItemDateChanged], [Notes]) SELECT NULLIF([UsedItemGroupID], N'''') AS [UsedItemGroupID], NULLIF([ContactID], N'''') AS [ContactID], NULLIF([GroupItemTypeID], N'''') AS [GroupReferenceItemID], NULLIF([LastItemTypeID], N'''') AS [LastItemID], NULLIF([LastItemTypeSortPos], N'''') AS [LastItemSortPos], CASE WHEN NULLIF([LastItemDateChanged], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastItemDateChanged], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N'''')), CAST(NULL AS datetime2(7))) END AS [LastItemDateChanged], NULLIF([Notes], N'''') AS [Notes] FROM [AccessSrc].[UsedItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [UsedItemGroupsTbl] ON;
        INSERT INTO [UsedItemGroupsTbl]
        (
            [UsedItemGroupID], [ContactID], [GroupReferenceItemID], [LastItemID], [LastItemSortPos], [LastItemDateChanged], [Notes]
        )
        SELECT
            NULLIF([UsedItemGroupID], N'') AS [UsedItemGroupID], NULLIF([ContactID], N'') AS [ContactID], NULLIF([GroupItemTypeID], N'') AS [GroupReferenceItemID], NULLIF([LastItemTypeID], N'') AS [LastItemID], NULLIF([LastItemTypeSortPos], N'') AS [LastItemSortPos], CASE WHEN NULLIF([LastItemDateChanged], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastItemDateChanged], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N'')), CAST(NULL AS datetime2(7))) END AS [LastItemDateChanged], NULLIF([Notes], N'') AS [Notes]
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
    PRINT N'INSERT INTO [UsedItemGroupsTbl] ([UsedItemGroupID], [ContactID], [GroupReferenceItemID], [LastItemID], [LastItemSortPos], [LastItemDateChanged], [Notes]) SELECT NULLIF([UsedItemGroupID], N'''') AS [UsedItemGroupID], NULLIF([ContactID], N'''') AS [ContactID], NULLIF([GroupItemTypeID], N'''') AS [GroupReferenceItemID], NULLIF([LastItemTypeID], N'''') AS [LastItemID], NULLIF([LastItemTypeSortPos], N'''') AS [LastItemSortPos], CASE WHEN NULLIF([LastItemDateChanged], N'''') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastItemDateChanged], N'''')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N'''')), CAST(NULL AS datetime2(7))) END AS [LastItemDateChanged], NULLIF([Notes], N'''') AS [Notes] FROM [UsedItemGroupTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [UsedItemGroupsTbl] ON;
        INSERT INTO [UsedItemGroupsTbl]
        (
            [UsedItemGroupID], [ContactID], [GroupReferenceItemID], [LastItemID], [LastItemSortPos], [LastItemDateChanged], [Notes]
        )
        SELECT
            NULLIF([UsedItemGroupID], N'') AS [UsedItemGroupID], NULLIF([ContactID], N'') AS [ContactID], NULLIF([GroupItemTypeID], N'') AS [GroupReferenceItemID], NULLIF([LastItemTypeID], N'') AS [LastItemID], NULLIF([LastItemTypeSortPos], N'') AS [LastItemSortPos], CASE WHEN NULLIF([LastItemDateChanged], N'') IS NULL OR LEN(LTRIM(RTRIM(NULLIF([LastItemDateChanged], N'')))) = 0 THEN NULL ELSE COALESCE(TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 127), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 126), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 121), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 103), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N''), 101), TRY_CONVERT(datetime2(7), NULLIF([LastItemDateChanged], N'')), CAST(NULL AS datetime2(7))) END AS [LastItemDateChanged], NULLIF([Notes], N'') AS [Notes]
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

-- TempOrdersLinesTbl -> TempOrdersLinesTbl
-- Mapping: columnsCount=7
--   TOLineID -> TOLineID
--   TOHeaderID -> TOHeaderID
--   ItemID -> ItemID
--   ServiceTypeID -> ItemServiceTypeID
--   Qty -> Qty
--   PackagingID -> ItemPackagingID
--   OriginalOrderID -> OriginalOrderID
IF OBJECT_ID(N'AccessSrc.TempOrdersLinesTbl') IS NULL
BEGIN
    PRINT 'SKIP migrate [TempOrdersLinesTbl]: missing source [AccessSrc].[TempOrdersLinesTbl]';
END
ELSE
BEGIN
    PRINT N'About to execute (IdentityInsert=ON):';
    PRINT N'INSERT INTO [TempOrdersLinesTbl] ([TOLineID], [TOHeaderID], [ItemID], [ItemServiceTypeID], [Qty], [ItemPackagingID], [OriginalOrderID]) SELECT NULLIF([TOLineID], N'''') AS [TOLineID], NULLIF([TOHeaderID], N'''') AS [TOHeaderID], NULLIF([ItemID], N'''') AS [ItemID], NULLIF([ServiceTypeID], N'''') AS [ItemServiceTypeID], NULLIF([Qty], N'''') AS [Qty], NULLIF([PackagingID], N'''') AS [ItemPackagingID], NULLIF([OriginalOrderID], N'''') AS [OriginalOrderID] FROM [AccessSrc].[TempOrdersLinesTbl];';
    BEGIN TRY
        BEGIN TRAN;
        SET IDENTITY_INSERT [TempOrdersLinesTbl] ON;
        INSERT INTO [TempOrdersLinesTbl]
        (
            [TOLineID], [TOHeaderID], [ItemID], [ItemServiceTypeID], [Qty], [ItemPackagingID], [OriginalOrderID]
        )
        SELECT
            NULLIF([TOLineID], N'') AS [TOLineID], NULLIF([TOHeaderID], N'') AS [TOHeaderID], NULLIF([ItemID], N'') AS [ItemID], NULLIF([ServiceTypeID], N'') AS [ItemServiceTypeID], NULLIF([Qty], N'') AS [Qty], NULLIF([PackagingID], N'') AS [ItemPackagingID], NULLIF([OriginalOrderID], N'') AS [OriginalOrderID]
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
WHERE t.name IN (N'AreasTbl', N'AwayReasonTbl', N'ClosureDatesTbl', N'ContactTypesTbl', N'EquipConditionsTbl', N'EquipTypesTbl', N'HolidayClosuresTbl', N'InvoiceTypesTbl', N'ItemPackagingsTbl', N'ItemPrepTypesTbl', N'ItemUnitsTbl', N'OrdersTbl', N'PaymentTermsTbl', N'PeopleTbl', N'PriceLevelsTbl', N'RecurranceTypesTbl', N'RecurringOrdersTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'SectionTypesTbl', N'SendCheckupEmailTextsTbl', N'TotalCountTrackerTbl', N'TransactionTypesTbl', N'AreaPrepDaysTbl', N'NextPrepDateByAreasTbl', N'ItemServiceTypesTbl', N'ItemsTbl', N'TrackedServiceItemsTbl', N'ContactsTbl', N'ItemGroupsTbl', N'SysDataTbl', N'ContactsAwayPeriodTbl', N'ContactsItemSvcSummaryTbl', N'ContactsItemsPredictedTbl', N'ContactsAccInfoTbl', N'ContactTrackedServiceItemsTbl', N'ContactsItemUsageTbl', N'RepairsTbl', N'SentRemindersLogTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersTbl', N'UsedItemGroupsTbl', N'TempOrdersLinesTbl');
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
WHERE tp.name IN (N'AreasTbl', N'AwayReasonTbl', N'ClosureDatesTbl', N'ContactTypesTbl', N'EquipConditionsTbl', N'EquipTypesTbl', N'HolidayClosuresTbl', N'InvoiceTypesTbl', N'ItemPackagingsTbl', N'ItemPrepTypesTbl', N'ItemUnitsTbl', N'OrdersTbl', N'PaymentTermsTbl', N'PeopleTbl', N'PriceLevelsTbl', N'RecurranceTypesTbl', N'RecurringOrdersTbl', N'RepairFaultsTbl', N'RepairStatusesTbl', N'SectionTypesTbl', N'SendCheckupEmailTextsTbl', N'TotalCountTrackerTbl', N'TransactionTypesTbl', N'AreaPrepDaysTbl', N'NextPrepDateByAreasTbl', N'ItemServiceTypesTbl', N'ItemsTbl', N'TrackedServiceItemsTbl', N'ContactsTbl', N'ItemGroupsTbl', N'SysDataTbl', N'ContactsAwayPeriodTbl', N'ContactsItemSvcSummaryTbl', N'ContactsItemsPredictedTbl', N'ContactsAccInfoTbl', N'ContactTrackedServiceItemsTbl', N'ContactsItemUsageTbl', N'RepairsTbl', N'SentRemindersLogTbl', N'TempCoffeecheckupCustomerTbl', N'TempCoffeecheckupItemsTbl', N'TempOrdersHeaderTbl', N'TempOrdersTbl', N'UsedItemGroupsTbl', N'TempOrdersLinesTbl') AND (fk.is_not_trusted = 1 OR fk.is_disabled = 1);
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

