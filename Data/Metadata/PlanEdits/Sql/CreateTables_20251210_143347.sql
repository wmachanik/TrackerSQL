-- Auto-generated CREATE TABLE script
-- Metadata paths:
--   AccessSchema: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema
--   PlanConstraints: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\PlanConstraints.json
-- Tables: 47, Columns: 379
-- Identity suppressed: No
-- Drop existing tables: Yes
-- Plan scan (source -> decision):
-- [ArchivedCustomersTbl] IGNORE (plan.Ignore=true)
-- [ArichivedOrdersTbl] IGNORE (plan.Ignore=true)
-- [AwayReasonTbl] Class=Copy Target=AwayReasonTbl EmittedCols=2
-- [CityPrepDaysTbl] Class=Copy Target=AreaPrepDaysTbl EmittedCols=5
-- [CityTbl] Class=Copy Target=AreasTbl EmittedCols=4
-- [ClientAwayPeriodTbl] Class=Copy Target=ContactsAwayPeriodTbl EmittedCols=5
-- [ClientUsageHistoryTbl] IGNORE (plan.Ignore=true)
-- [ClientUsageLinesTbl] Class=Copy Target=ContactUsageLinesTbl EmittedCols=7
-- [ClientUsageTbl] Class=Copy Target=ContactsUsageTbl EmittedCols=12
-- [ClosureDatesTbl] Class=Copy Target=ClosureDatesTbl EmittedCols=5
-- [CustomersAccInfoTbl] Class=Copy Target=ContactsAccInfoTbl EmittedCols=30
-- [CustomersTbl] Class=Copy Target=ContactsTbl EmittedCols=42
-- [CustomerTrackedServiceItemsTbl] Class=Copy Target=ContactTrackedServiceItemsTbl EmittedCols=4
-- [CustomerTypeTbl] Class=Copy Target=ContactTypesTbl EmittedCols=3
-- [EquipTypeTbl] Class=Copy Target=EquipTypesTbl EmittedCols=3
-- [HolidayClosureTbl] Class=Copy Target=HolidayClosuresTbl EmittedCols=7
-- [InvoiceTypeTbl] Class=Copy Target=InvoiceTypesTbl EmittedCols=4
-- [ItemGroupTbl] Class=Copy Target=ItemGroupsTbl EmittedCols=6
-- [ItemTypeTbl] Class=Copy Target=ItemsTbl EmittedCols=13
-- [ItemUnitsTbl] Class=Copy Target=ItemUnitsTbl EmittedCols=3
-- [ItemUsageTbl] Class=Copy Target=ContactsItemUsageTbl EmittedCols=8
-- [LogTbl] IGNORE (plan.Ignore=true)
-- [MachineConditionsTbl] Class=Copy Target=EquipConditionsTbl EmittedCols=4
-- [NextRoastDateByCityTbl] Class=Copy Target=NextPrepDateByAreasTbl EmittedCols=7
-- [OrderList] IGNORE (plan.Ignore=true)
-- [OrdersTbl] Class=Normalize Header=OrdersTbl Lines=OrderLinesTbl Emitted(H/L)=11/6 Synth=[NewHeaderKey=OrderID, NewLineKey=OrderLineID, LinkFK=OrderID] 
-- [OrdersTbl_Apr26_2008] IGNORE (plan.Ignore=true)
-- [PackagingTbl] Class=Copy Target=ItemPackagingsTbl EmittedCols=6
-- [PaymentTermsTbl] Class=Copy Target=PaymentTermsTbl EmittedCols=7
-- [PersonsTbl] Class=Copy Target=PeopleTbl EmittedCols=6
-- [PredictedOrdersTbl] IGNORE (plan.Ignore=true)
-- [PrepTypesTbl] Class=Copy Target=ItemPrepTypesTbl EmittedCols=3
-- [PriceLevelsTbl] Class=Copy Target=PriceLevelsTbl EmittedCols=5
-- [ReoccuranceTypeTbl] Class=Copy Target=RecurranceTypesTbl EmittedCols=2
-- [ReoccuringOrderTbl] Class=Normalize Header=RecurringOrdersTbl Lines=RecurringOrderItemsTbl Emitted(H/L)=9/4 Synth=[NewHeaderKey=RecurringOrderID, NewLineKey=RecurringOrderItemID, LinkFK=RecurringOrderID] 
-- [RepairFaultsTbl] Class=Copy Target=RepairFaultsTbl EmittedCols=4
-- [RepairStatusesTbl] Class=Copy Target=RepairStatusesTbl EmittedCols=6
-- [RepairsTbl] Class=Copy Target=RepairsTbl EmittedCols=22
-- [SectionTypesTbl] Class=Copy Target=SectionTypesTbl EmittedCols=3
-- [SendCheckEmailTextsTbl] Class=Copy Target=SendCheckupEmailTextsTbl EmittedCols=6
-- [SentRemindersLogTbl] Class=Copy Target=SentRemindersLogTbl EmittedCols=7
-- [ServiceTypesTbl] Class=Copy Target=ItemServiceTypesTbl EmittedCols=5
-- [SysDataTbl] Class=Copy Target=SysDataTbl EmittedCols=7
-- [TempCoffeecheckupCustomerTbl] Class=Copy Target=TempCoffeecheckupCustomerTbl EmittedCols=25
-- [TempCoffeecheckupItemsTbl] Class=Copy Target=TempCoffeecheckupItemsTbl EmittedCols=9
-- [TempOrdersHeaderTbl] Class=Copy Target=TempOrdersHeaderTbl EmittedCols=9
-- [TempOrdersLinesTbl] Class=Copy Target=TempOrdersLinesTbl EmittedCols=7
-- [TempOrdersTbl] Class=Copy Target=TempOrdersTbl EmittedCols=13
-- [tmpOrdersReplyTbl] IGNORE (plan.Ignore=true)
-- [TotalCountTrackerTbl] Class=Copy Target=TotalCountTrackerTbl EmittedCols=4
-- [TrackedServiceItemTbl] Class=Copy Target=TrackedServiceItemsTbl EmittedCols=7
-- [TransactionTypesTbl] Class=Copy Target=TransactionTypesTbl EmittedCols=3
-- [UsageAveTbl] IGNORE (plan.Ignore=true)
-- [UsageTblByDate] IGNORE (plan.Ignore=true)
-- [UsedItemGroupTbl] Class=Copy Target=UsedItemGroupsTbl EmittedCols=7
-- [VisitLogTbl] IGNORE (plan.Ignore=true)
-- [WeekDaysTbl] IGNORE (plan.Ignore=true)
-- [_ClientUsageTbl] IGNORE (plan.Ignore=true)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

-- Drop FKs referencing or owned by [AreaPrepDaysTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[AreaPrepDaysTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[AreaPrepDaysTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[AreaPrepDaysTbl]', N'U') IS NOT NULL DROP TABLE [AreaPrepDaysTbl];
GO
    CREATE TABLE [AreaPrepDaysTbl]
    (
        [AreaPrepDaysID] INT IDENTITY(1,1) NOT NULL,
        [AreaID] INT NULL,
        [PrepDayOfWeekID] TINYINT NULL,
        [DeliveryDelayDays] SMALLINT NULL,
        [DeliveryOrder] SMALLINT NULL
        , CONSTRAINT [PK_AreaPrepDaysTbl] PRIMARY KEY CLUSTERED ([AreaPrepDaysID])
    );
GO

-- Drop FKs referencing or owned by [AreasTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[AreasTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[AreasTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[AreasTbl]', N'U') IS NOT NULL DROP TABLE [AreasTbl];
GO
    CREATE TABLE [AreasTbl]
    (
        [AreaID] INT IDENTITY(1,1) NOT NULL,
        [AreaName] NVARCHAR(255) NULL,
        [PrepDayOfWeekID] INT NULL,
        [DeliveryDelay] INT NULL
        , CONSTRAINT [PK_AreasTbl] PRIMARY KEY CLUSTERED ([AreaID])
    );
GO

-- Drop FKs referencing or owned by [AwayReasonTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[AwayReasonTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[AwayReasonTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[AwayReasonTbl]', N'U') IS NOT NULL DROP TABLE [AwayReasonTbl];
GO
    CREATE TABLE [AwayReasonTbl]
    (
        [AwayReasonID] INT IDENTITY(1,1) NOT NULL,
        [ReasonDesc] NVARCHAR(100) NULL
        , CONSTRAINT [PK_AwayReasonTbl] PRIMARY KEY CLUSTERED ([AwayReasonID])
    );
GO

-- Drop FKs referencing or owned by [ClosureDatesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ClosureDatesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ClosureDatesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ClosureDatesTbl]', N'U') IS NOT NULL DROP TABLE [ClosureDatesTbl];
GO
    CREATE TABLE [ClosureDatesTbl]
    (
        [ClosureDateID] INT IDENTITY(1,1) NOT NULL,
        [DateClosed] DATETIME NULL,
        [DateReopen] DATETIME NULL,
        [NextPrepDate] DATETIME NULL,
        [Comments] NVARCHAR(255) NULL
        , CONSTRAINT [PK_ClosureDatesTbl] PRIMARY KEY CLUSTERED ([ClosureDateID])
    );
GO

-- Drop FKs referencing or owned by [ContactsAccInfoTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ContactsAccInfoTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ContactsAccInfoTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ContactsAccInfoTbl]', N'U') IS NOT NULL DROP TABLE [ContactsAccInfoTbl];
GO
    CREATE TABLE [ContactsAccInfoTbl]
    (
        [ContactsAccInfoID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [RequiresPurchOrder] BIT NULL,
        [ContactVATNo] NVARCHAR(30) NULL,
        [BillAddr1] NVARCHAR(50) NULL,
        [BillAddr2] NVARCHAR(50) NULL,
        [BillAddr3] NVARCHAR(50) NULL,
        [BillAddr4] NVARCHAR(50) NULL,
        [BillAddr5] NVARCHAR(50) NULL,
        [ShipAddr1] NVARCHAR(50) NULL,
        [ShipAddr2] NVARCHAR(50) NULL,
        [ShipAddr3] NVARCHAR(50) NULL,
        [ShipAddr4] NVARCHAR(50) NULL,
        [ShipAddr5] NVARCHAR(50) NULL,
        [AccEmail] NVARCHAR(50) NULL,
        [AltAccEmail] NVARCHAR(50) NULL,
        [PaymentTermID] INT NULL,
        [Limit] REAL NULL,
        [FullCoName] NVARCHAR(50) NULL,
        [AccFirstName] NVARCHAR(50) NULL,
        [AccLastName] NVARCHAR(50) NULL,
        [AltAccFirstName] NVARCHAR(50) NULL,
        [AltAccLastName] NVARCHAR(50) NULL,
        [PriceLevelID] INT NULL,
        [InvoiceTypeID] INT NULL,
        [RegNo] NVARCHAR(30) NULL,
        [BankAccNo] NVARCHAR(30) NULL,
        [BankBranch] NVARCHAR(50) NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_ContactsAccInfoTbl] PRIMARY KEY CLUSTERED ([ContactsAccInfoID])
    );
GO

-- Drop FKs referencing or owned by [ContactsAwayPeriodTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ContactsAwayPeriodTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ContactsAwayPeriodTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ContactsAwayPeriodTbl]', N'U') IS NOT NULL DROP TABLE [ContactsAwayPeriodTbl];
GO
    CREATE TABLE [ContactsAwayPeriodTbl]
    (
        [AwayPeriodID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [AwayStartDate] DATETIME NULL,
        [AwayEndDate] DATETIME NULL,
        [ReasonID] INT NULL
        , CONSTRAINT [PK_ContactsAwayPeriodTbl] PRIMARY KEY CLUSTERED ([AwayPeriodID])
    );
GO

-- Drop FKs referencing or owned by [ContactsItemUsageTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ContactsItemUsageTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ContactsItemUsageTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ContactsItemUsageTbl]', N'U') IS NOT NULL DROP TABLE [ContactsItemUsageTbl];
GO
    CREATE TABLE [ContactsItemUsageTbl]
    (
        [ContactID] INT IDENTITY(1,1) NOT NULL,
        [ContactUsageLineNo] INT NULL,
        [DeliveryDate] DATETIME NULL,
        [ItemProvidedID] INT NULL,
        [QtyProvided] REAL NULL,
        [ItemPrepTypeID] INT NULL,
        [ItemPackagingID] INT NULL,
        [Notes] NVARCHAR(150) NULL
        , CONSTRAINT [PK_ContactsItemUsageTbl] PRIMARY KEY CLUSTERED ([ContactID])
    );
GO

-- Drop FKs referencing or owned by [ContactsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ContactsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ContactsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ContactsTbl]', N'U') IS NOT NULL DROP TABLE [ContactsTbl];
GO
    CREATE TABLE [ContactsTbl]
    (
        [ContactID] INT IDENTITY(1,1) NOT NULL,
        [CompanyName] NVARCHAR(50) NULL,
        [ContactTitle] NVARCHAR(50) NULL,
        [ContactFirstName] NVARCHAR(30) NULL,
        [ContactLastName] NVARCHAR(50) NULL,
        [ContactAltFirstName] NVARCHAR(50) NULL,
        [ContactAltLastName] NVARCHAR(50) NULL,
        [Department] NVARCHAR(50) NULL,
        [BillingAddress] NVARCHAR(255) NULL,
        [Area] INT NULL,
        [StateOrProvince] NVARCHAR(20) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [CountryOrRegion] NVARCHAR(50) NULL,
        [PhoneNumber] NVARCHAR(30) NULL,
        [Extension] NVARCHAR(30) NULL,
        [FaxNumber] NVARCHAR(30) NULL,
        [CellNumber] NVARCHAR(50) NULL,
        [EmailAddress] NVARCHAR(50) NULL,
        [AltEmailAddress] NVARCHAR(255) NULL,
        [ContractNo] NVARCHAR(50) NULL,
        [ContactTypeID] INT NULL,
        [EquipTypeID] INT NULL,
        [ItemPrefID] INT NULL,
        [PriPrefQty] REAL NULL,
        [PrefItemPrepTypeID] INT NULL,
        [PrefItemPackagingID] INT NULL,
        [SecondaryItemPrefID] INT NULL,
        [SecPrefQty] REAL NULL,
        [TypicallySecToo] BIT NULL,
        [PreferedAgentID] INT NULL,
        [SalesAgentID] INT NULL,
        [MachineSN] NVARCHAR(50) NULL,
        [UsesFilter] BIT NULL,
        [AutoFulfill] BIT NULL,
        [Enabled] BIT NULL,
        [PredictionDisabled] BIT NULL,
        [AlwaysSendChkUp] BIT NULL,
        [NormallyResponds] BIT NULL,
        [ReminderCount] INT NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [SendDeliveryConfirmation] BIT NULL,
        [LastDateSentReminder] DATETIME NULL
        , CONSTRAINT [PK_ContactsTbl] PRIMARY KEY CLUSTERED ([ContactID])
    );
GO

-- Drop FKs referencing or owned by [ContactsUsageTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ContactsUsageTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ContactsUsageTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ContactsUsageTbl]', N'U') IS NOT NULL DROP TABLE [ContactsUsageTbl];
GO
    CREATE TABLE [ContactsUsageTbl]
    (
        [ContactID] INT IDENTITY(1,1) NOT NULL,
        [LastCupCount] INT NULL,
        [NextCoffeeBy] DATETIME NULL,
        [NextCleanOn] DATETIME NULL,
        [NextFilterEst] DATETIME NULL,
        [NextDescaleEst] DATETIME NULL,
        [NextServiceEst] DATETIME NULL,
        [DailyConsumption] REAL NULL,
        [FilterAveCount] REAL NULL,
        [DescaleAveCount] REAL NULL,
        [ServiceAveCount] REAL NULL,
        [CleanAveCount] REAL NULL
        , CONSTRAINT [PK_ContactsUsageTbl] PRIMARY KEY CLUSTERED ([ContactID])
    );
GO

-- Drop FKs referencing or owned by [ContactTrackedServiceItemsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ContactTrackedServiceItemsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ContactTrackedServiceItemsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ContactTrackedServiceItemsTbl]', N'U') IS NOT NULL DROP TABLE [ContactTrackedServiceItemsTbl];
GO
    CREATE TABLE [ContactTrackedServiceItemsTbl]
    (
        [ContactTrackedServiceItemsID] INT IDENTITY(1,1) NOT NULL,
        [ContactTypeID] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_ContactTrackedServiceItemsTbl] PRIMARY KEY CLUSTERED ([ContactTrackedServiceItemsID])
    );
GO

-- Drop FKs referencing or owned by [ContactTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ContactTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ContactTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ContactTypesTbl]', N'U') IS NOT NULL DROP TABLE [ContactTypesTbl];
GO
    CREATE TABLE [ContactTypesTbl]
    (
        [ContactTypeID] INT IDENTITY(1,1) NOT NULL,
        [ContactTypeDesc] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_ContactTypesTbl] PRIMARY KEY CLUSTERED ([ContactTypeID])
    );
GO

-- Drop FKs referencing or owned by [ContactUsageLinesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ContactUsageLinesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ContactUsageLinesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ContactUsageLinesTbl]', N'U') IS NOT NULL DROP TABLE [ContactUsageLinesTbl];
GO
    CREATE TABLE [ContactUsageLinesTbl]
    (
        [ContactID] INT IDENTITY(1,1) NOT NULL,
        [ContactUsageLineNo] INT NULL,
        [UsageDate] DATETIME NULL,
        [CupCount] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [Qty] REAL NULL,
        [Notes] NVARCHAR(150) NULL
        , CONSTRAINT [PK_ContactUsageLinesTbl] PRIMARY KEY CLUSTERED ([ContactID])
    );
GO

-- Drop FKs referencing or owned by [EquipConditionsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[EquipConditionsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[EquipConditionsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[EquipConditionsTbl]', N'U') IS NOT NULL DROP TABLE [EquipConditionsTbl];
GO
    CREATE TABLE [EquipConditionsTbl]
    (
        [EquipConditionID] INT IDENTITY(1,1) NOT NULL,
        [ConditionDesc] NVARCHAR(50) NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_EquipConditionsTbl] PRIMARY KEY CLUSTERED ([EquipConditionID])
    );
GO

-- Drop FKs referencing or owned by [EquipTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[EquipTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[EquipTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[EquipTypesTbl]', N'U') IS NOT NULL DROP TABLE [EquipTypesTbl];
GO
    CREATE TABLE [EquipTypesTbl]
    (
        [EquipTypeID] INT IDENTITY(1,1) NOT NULL,
        [EquipTypeName] NVARCHAR(50) NULL,
        [EquipTypeDescription] NVARCHAR(50) NULL
        , CONSTRAINT [PK_EquipTypesTbl] PRIMARY KEY CLUSTERED ([EquipTypeID])
    );
GO

-- Drop FKs referencing or owned by [HolidayClosuresTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[HolidayClosuresTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[HolidayClosuresTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[HolidayClosuresTbl]', N'U') IS NOT NULL DROP TABLE [HolidayClosuresTbl];
GO
    CREATE TABLE [HolidayClosuresTbl]
    (
        [HolidayClosureID] INT IDENTITY(1,1) NOT NULL,
        [ClosureDate] DATETIME NULL,
        [DaysClosed] INT NULL,
        [AppliesToPrep] BIT NULL,
        [AppliesToDelivery] BIT NULL,
        [ShiftStrategy] NVARCHAR(20) NULL,
        [Description] NVARCHAR(255) NULL
        , CONSTRAINT [PK_HolidayClosuresTbl] PRIMARY KEY CLUSTERED ([HolidayClosureID])
    );
GO

-- Drop FKs referencing or owned by [InvoiceTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[InvoiceTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[InvoiceTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[InvoiceTypesTbl]', N'U') IS NOT NULL DROP TABLE [InvoiceTypesTbl];
GO
    CREATE TABLE [InvoiceTypesTbl]
    (
        [InvoiceTypeID] INT IDENTITY(1,1) NOT NULL,
        [InvoiceTypeDesc] NVARCHAR(20) NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_InvoiceTypesTbl] PRIMARY KEY CLUSTERED ([InvoiceTypeID])
    );
GO

-- Drop FKs referencing or owned by [ItemGroupsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemGroupsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemGroupsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemGroupsTbl]', N'U') IS NOT NULL DROP TABLE [ItemGroupsTbl];
GO
    CREATE TABLE [ItemGroupsTbl]
    (
        [ItemGroupID] INT IDENTITY(1,1) NOT NULL,
        [GroupReferenceItemID] INT NULL,
        [ItemID] INT NULL,
        [ItemSortPos] INT NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_ItemGroupsTbl] PRIMARY KEY CLUSTERED ([ItemGroupID])
    );
GO

-- Drop FKs referencing or owned by [ItemPackagingsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemPackagingsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemPackagingsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemPackagingsTbl]', N'U') IS NOT NULL DROP TABLE [ItemPackagingsTbl];
GO
    CREATE TABLE [ItemPackagingsTbl]
    (
        [ItemPackagingID] INT IDENTITY(1,1) NOT NULL,
        [ItemPackagingDesc] NVARCHAR(50) NULL,
        [AdditionalNotes] NVARCHAR(255) NULL,
        [Symbol] NVARCHAR(255) NULL,
        [Colour] INT NULL,
        [BGColour] NVARCHAR(9) NULL
        , CONSTRAINT [PK_ItemPackagingsTbl] PRIMARY KEY CLUSTERED ([ItemPackagingID])
    );
GO

-- Drop FKs referencing or owned by [ItemPrepTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemPrepTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemPrepTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemPrepTypesTbl]', N'U') IS NOT NULL DROP TABLE [ItemPrepTypesTbl];
GO
    CREATE TABLE [ItemPrepTypesTbl]
    (
        [ItemPrepID] INT IDENTITY(1,1) NOT NULL,
        [ItemPrepType] NVARCHAR(50) NULL,
        [IdentifyingChar] NVARCHAR(1) NULL
        , CONSTRAINT [PK_ItemPrepTypesTbl] PRIMARY KEY CLUSTERED ([ItemPrepID])
    );
GO

-- Drop FKs referencing or owned by [ItemServiceTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemServiceTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemServiceTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemServiceTypesTbl]', N'U') IS NOT NULL DROP TABLE [ItemServiceTypesTbl];
GO
    CREATE TABLE [ItemServiceTypesTbl]
    (
        [ItemServiceTypeID] INT IDENTITY(1,1) NOT NULL,
        [ItemServiceType] NVARCHAR(20) NULL,
        [Description] NVARCHAR(100) NULL,
        [ItemPackagingID] INT NULL,
        [ItemPrepTypeID] INT NULL
        , CONSTRAINT [PK_ItemServiceTypesTbl] PRIMARY KEY CLUSTERED ([ItemServiceTypeID])
    );
GO

-- Drop FKs referencing or owned by [ItemsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemsTbl]', N'U') IS NOT NULL DROP TABLE [ItemsTbl];
GO
    CREATE TABLE [ItemsTbl]
    (
        [ItemID] INT IDENTITY(1,1) NOT NULL,
        [SKU] NVARCHAR(20) NULL,
        [ItemDesc] NVARCHAR(50) NULL,
        [ItemEnabled] BIT NULL,
        [ItemsCharacteritics] NVARCHAR(50) NULL,
        [ItemDetail] NVARCHAR(MAX) NULL,
        [ItemServiceTypeID] INT NULL,
        [ReplacementItemID] INT NULL,
        [ItemShortName] NVARCHAR(10) NULL,
        [SortOrder] INT NULL,
        [UnitsPerQty] REAL NULL,
        [ItemUnitID] INT NULL,
        [BasePrice] REAL NULL
        , CONSTRAINT [PK_ItemsTbl] PRIMARY KEY CLUSTERED ([ItemID])
    );
GO

-- Drop FKs referencing or owned by [ItemUnitsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemUnitsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemUnitsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemUnitsTbl]', N'U') IS NOT NULL DROP TABLE [ItemUnitsTbl];
GO
    CREATE TABLE [ItemUnitsTbl]
    (
        [ItemUnitID] INT IDENTITY(1,1) NOT NULL,
        [UnitOfMeasure] NVARCHAR(5) NULL,
        [UnitDescription] NVARCHAR(50) NULL
        , CONSTRAINT [PK_ItemUnitsTbl] PRIMARY KEY CLUSTERED ([ItemUnitID])
    );
GO

-- Drop FKs referencing or owned by [NextPrepDateByAreasTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[NextPrepDateByAreasTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[NextPrepDateByAreasTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[NextPrepDateByAreasTbl]', N'U') IS NOT NULL DROP TABLE [NextPrepDateByAreasTbl];
GO
    CREATE TABLE [NextPrepDateByAreasTbl]
    (
        [NextPrepDayID] INT IDENTITY(1,1) NOT NULL,
        [AreaID] INT NULL,
        [PreperationDate] DATETIME NULL,
        [DeliveryDate] DATETIME NULL,
        [DeliveryOrder] SMALLINT NULL,
        [NextPrepDate] DATETIME NULL,
        [NextDeliveryDate] DATETIME NULL
        , CONSTRAINT [PK_NextPrepDateByAreasTbl] PRIMARY KEY CLUSTERED ([NextPrepDayID])
    );
GO

-- Drop FKs referencing or owned by [OrderLinesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[OrderLinesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[OrderLinesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[OrderLinesTbl]', N'U') IS NOT NULL DROP TABLE [OrderLinesTbl];
GO
    CREATE TABLE [OrderLinesTbl]
    (
        [OrderLineID] INT NULL,
        [OrderID] INT NOT NULL,
        [PrepDate] DATETIME NULL,
        [ItemID] INT NULL,
        [QtyOrdered] REAL NULL,
        [PrepTypeID] INT NULL,
        [PackagingID] INT NULL
    );
GO

-- Drop FKs referencing or owned by [OrdersTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[OrdersTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[OrdersTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[OrdersTbl]', N'U') IS NOT NULL DROP TABLE [OrdersTbl];
GO
    CREATE TABLE [OrdersTbl]
    (
        [OrderID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [OrderDate] DATETIME NULL,
        [RequiredByDate] DATETIME NULL,
        [ToBeDeliveredByID] INT NULL,
        [Confirmed] BIT NULL,
        [Done] BIT NULL,
        [Packed] BIT NULL,
        [Notes] NVARCHAR(255) NULL,
        [PurchaseOrder] NVARCHAR(30) NULL,
        [InvoiceDone] BIT NULL
        , CONSTRAINT [PK_OrdersTbl] PRIMARY KEY CLUSTERED ([OrderID])
    );
GO

-- Drop FKs referencing or owned by [PaymentTermsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[PaymentTermsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[PaymentTermsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[PaymentTermsTbl]', N'U') IS NOT NULL DROP TABLE [PaymentTermsTbl];
GO
    CREATE TABLE [PaymentTermsTbl]
    (
        [PaymentTermID] INT IDENTITY(1,1) NOT NULL,
        [PaymentTermDesc] NVARCHAR(20) NULL,
        [PaymentDays] INT NULL,
        [DayOfMonth] INT NULL,
        [UseDays] BIT NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_PaymentTermsTbl] PRIMARY KEY CLUSTERED ([PaymentTermID])
    );
GO

-- Drop FKs referencing or owned by [PeopleTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[PeopleTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[PeopleTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[PeopleTbl]', N'U') IS NOT NULL DROP TABLE [PeopleTbl];
GO
    CREATE TABLE [PeopleTbl]
    (
        [PersonID] INT IDENTITY(1,1) NOT NULL,
        [Person] NVARCHAR(50) NULL,
        [Abbreviation] NVARCHAR(5) NULL,
        [Enabled] BIT NULL,
        [NormalDeliveryDoW] INT NULL,
        [SecurityUsername] NVARCHAR(255) NULL
        , CONSTRAINT [PK_PeopleTbl] PRIMARY KEY CLUSTERED ([PersonID])
    );
GO

-- Drop FKs referencing or owned by [PriceLevelsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[PriceLevelsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[PriceLevelsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[PriceLevelsTbl]', N'U') IS NOT NULL DROP TABLE [PriceLevelsTbl];
GO
    CREATE TABLE [PriceLevelsTbl]
    (
        [PriceLevelID] INT IDENTITY(1,1) NOT NULL,
        [PriceLevelDesc] NVARCHAR(20) NULL,
        [PricingFactor] REAL NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_PriceLevelsTbl] PRIMARY KEY CLUSTERED ([PriceLevelID])
    );
GO

-- Drop FKs referencing or owned by [RecurranceTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[RecurranceTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[RecurranceTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[RecurranceTypesTbl]', N'U') IS NOT NULL DROP TABLE [RecurranceTypesTbl];
GO
    CREATE TABLE [RecurranceTypesTbl]
    (
        [RecurringTypeID] INT IDENTITY(1,1) NOT NULL,
        [RecurringTypeDesc] NVARCHAR(255) NULL
        , CONSTRAINT [PK_RecurranceTypesTbl] PRIMARY KEY CLUSTERED ([RecurringTypeID])
    );
GO

-- Drop FKs referencing or owned by [RecurringOrderItemsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[RecurringOrderItemsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[RecurringOrderItemsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[RecurringOrderItemsTbl]', N'U') IS NOT NULL DROP TABLE [RecurringOrderItemsTbl];
GO
    CREATE TABLE [RecurringOrderItemsTbl]
    (
        [RecurringOrderItemID] INT NULL,
        [RecurringOrderID] INT NOT NULL,
        [ItemRequiredID] INT NULL,
        [QtyRequired] REAL NULL,
        [ItemPackagingID] INT NULL
    );
GO

-- Drop FKs referencing or owned by [RecurringOrdersTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[RecurringOrdersTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[RecurringOrdersTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[RecurringOrdersTbl]', N'U') IS NOT NULL DROP TABLE [RecurringOrdersTbl];
GO
    CREATE TABLE [RecurringOrdersTbl]
    (
        [RecurringOrderID] INT NOT NULL,
        [ContactID] INT NULL,
        [RecurringTypeID] INT NULL,
        [Value] INT NULL,
        [DateLastDone] DATETIME NULL,
        [NextDateRequired] DATETIME NULL,
        [RequireUntilDate] DATETIME NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(255) NULL
    );
GO

-- Drop FKs referencing or owned by [RepairFaultsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[RepairFaultsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[RepairFaultsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[RepairFaultsTbl]', N'U') IS NOT NULL DROP TABLE [RepairFaultsTbl];
GO
    CREATE TABLE [RepairFaultsTbl]
    (
        [RepairFaultID] INT IDENTITY(1,1) NOT NULL,
        [RepairFaultDesc] NVARCHAR(50) NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_RepairFaultsTbl] PRIMARY KEY CLUSTERED ([RepairFaultID])
    );
GO

-- Drop FKs referencing or owned by [RepairStatusesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[RepairStatusesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[RepairStatusesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[RepairStatusesTbl]', N'U') IS NOT NULL DROP TABLE [RepairStatusesTbl];
GO
    CREATE TABLE [RepairStatusesTbl]
    (
        [RepairStatusID] INT IDENTITY(1,1) NOT NULL,
        [RepairStatusDesc] NVARCHAR(50) NULL,
        [EmailContact] BIT NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [StatusNote] NVARCHAR(255) NULL
        , CONSTRAINT [PK_RepairStatusesTbl] PRIMARY KEY CLUSTERED ([RepairStatusID])
    );
GO

-- Drop FKs referencing or owned by [RepairsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[RepairsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[RepairsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[RepairsTbl]', N'U') IS NOT NULL DROP TABLE [RepairsTbl];
GO
    CREATE TABLE [RepairsTbl]
    (
        [RepairID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [ContactName] NVARCHAR(50) NULL,
        [ContactEmail] NVARCHAR(50) NULL,
        [JobCardNumber] NVARCHAR(20) NULL,
        [DateLogged] DATETIME NULL,
        [LastStatusChange] DATETIME NULL,
        [EquipTypeID] INT NULL,
        [EquipSerialNumber] NVARCHAR(50) NULL,
        [SwopOutMachineID] INT NULL,
        [EquipConditionID] INT NULL,
        [TakenFrother] BIT NULL,
        [TakenBeanLid] BIT NULL,
        [TakenWaterLid] BIT NULL,
        [BrokenFrother] BIT NULL,
        [BrokenBeanLid] BIT NULL,
        [BrokenWaterLid] BIT NULL,
        [RepairFaultID] INT NULL,
        [RepairFaultDesc] NVARCHAR(255) NULL,
        [RepairStatusID] INT NULL,
        [RelatedOrderID] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_RepairsTbl] PRIMARY KEY CLUSTERED ([RepairID])
    );
GO

-- Drop FKs referencing or owned by [SectionTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[SectionTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[SectionTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[SectionTypesTbl]', N'U') IS NOT NULL DROP TABLE [SectionTypesTbl];
GO
    CREATE TABLE [SectionTypesTbl]
    (
        [SectionID] INT IDENTITY(1,1) NOT NULL,
        [SectionType] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_SectionTypesTbl] PRIMARY KEY CLUSTERED ([SectionID])
    );
GO

-- Drop FKs referencing or owned by [SendCheckupEmailTextsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[SendCheckupEmailTextsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[SendCheckupEmailTextsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[SendCheckupEmailTextsTbl]', N'U') IS NOT NULL DROP TABLE [SendCheckupEmailTextsTbl];
GO
    CREATE TABLE [SendCheckupEmailTextsTbl]
    (
        [SCEMTID] INT IDENTITY(1,1) NOT NULL,
        [HeaderText] NVARCHAR(MAX) NULL,
        [BodyText] NVARCHAR(MAX) NULL,
        [FooterText] NVARCHAR(MAX) NULL,
        [DateLastChange] DATETIME NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_SendCheckupEmailTextsTbl] PRIMARY KEY CLUSTERED ([SCEMTID])
    );
GO

-- Drop FKs referencing or owned by [SentRemindersLogTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[SentRemindersLogTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[SentRemindersLogTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[SentRemindersLogTbl]', N'U') IS NOT NULL DROP TABLE [SentRemindersLogTbl];
GO
    CREATE TABLE [SentRemindersLogTbl]
    (
        [ReminderID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [DateSentReminder] DATETIME NULL,
        [NextPrepDate] DATETIME NULL,
        [ReminderSent] BIT NULL,
        [HadAutoFulfilItem] BIT NULL,
        [HadRecurrItems] BIT NULL
        , CONSTRAINT [PK_SentRemindersLogTbl] PRIMARY KEY CLUSTERED ([ReminderID])
    );
GO

-- Drop FKs referencing or owned by [SysDataTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[SysDataTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[SysDataTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[SysDataTbl]', N'U') IS NOT NULL DROP TABLE [SysDataTbl];
GO
    CREATE TABLE [SysDataTbl]
    (
        [ID] INT IDENTITY(1,1) NOT NULL,
        [LastReoccurringDate] DATETIME NULL,
        [DoReoccuringOrders] BIT NULL,
        [DateLastPrepDateCalcd] DATETIME NULL,
        [MinReminderDate] DATETIME NULL,
        [GroupReferenceItemID] INT NULL,
        [InternalContactIDs] NVARCHAR(255) NULL
        , CONSTRAINT [PK_SysDataTbl] PRIMARY KEY CLUSTERED ([ID])
    );
GO

-- Drop FKs referencing or owned by [TempCoffeecheckupCustomerTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]', N'U') IS NOT NULL DROP TABLE [TempCoffeecheckupCustomerTbl];
GO
    CREATE TABLE [TempCoffeecheckupCustomerTbl]
    (
        [TCCID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [CompanyName] NVARCHAR(50) NULL,
        [ContactFirstName] NVARCHAR(50) NULL,
        [ContactAltFirstName] NVARCHAR(50) NULL,
        [AreaID] INT NULL,
        [EmailAddress] NVARCHAR(50) NULL,
        [AltEmailAddress] NVARCHAR(50) NULL,
        [ContactTypeID] INT NULL,
        [EquipTypeID] INT NULL,
        [TypicallySecToo] BIT NULL,
        [PreferedAgentID] INT NULL,
        [SalesAgentID] INT NULL,
        [UsesFilter] BIT NULL,
        [Enabled] BIT NULL,
        [AlwaysSendChkUp] BIT NULL,
        [ReminderCount] INT NULL,
        [NextPrepDate] DATETIME NULL,
        [NextDeliveryDate] DATETIME NULL,
        [NextCoffee] DATETIME NULL,
        [NextClean] DATETIME NULL,
        [NextFilter] DATETIME NULL,
        [NextDescal] DATETIME NULL,
        [NextService] DATETIME NULL,
        [RequiresPurchOrder] BIT NULL
        , CONSTRAINT [PK_TempCoffeecheckupCustomerTbl] PRIMARY KEY CLUSTERED ([TCCID])
    );
GO

-- Drop FKs referencing or owned by [TempCoffeecheckupItemsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TempCoffeecheckupItemsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TempCoffeecheckupItemsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TempCoffeecheckupItemsTbl]', N'U') IS NOT NULL DROP TABLE [TempCoffeecheckupItemsTbl];
GO
    CREATE TABLE [TempCoffeecheckupItemsTbl]
    (
        [TCIID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [ItemID] INT NULL,
        [ItemQty] REAL NULL,
        [ItemPrepID] INT NULL,
        [ItemPackagingID] INT NULL,
        [AutoFulfill] BIT NULL,
        [NextDateRequired] DATETIME NULL,
        [RecurringOrderItemID] INT NULL
        , CONSTRAINT [PK_TempCoffeecheckupItemsTbl] PRIMARY KEY CLUSTERED ([TCIID])
    );
GO

-- Drop FKs referencing or owned by [TempOrdersHeaderTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TempOrdersHeaderTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TempOrdersHeaderTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TempOrdersHeaderTbl]', N'U') IS NOT NULL DROP TABLE [TempOrdersHeaderTbl];
GO
    CREATE TABLE [TempOrdersHeaderTbl]
    (
        [TOHeaderID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [OrderDate] DATETIME NULL,
        [RoastDate] DATETIME NULL,
        [RequiredByDate] DATETIME NULL,
        [ToBeDeliveredByID] INT NULL,
        [Confirmed] BIT NULL,
        [Done] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_TempOrdersHeaderTbl] PRIMARY KEY CLUSTERED ([TOHeaderID])
    );
GO

-- Drop FKs referencing or owned by [TempOrdersLinesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TempOrdersLinesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TempOrdersLinesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TempOrdersLinesTbl]', N'U') IS NOT NULL DROP TABLE [TempOrdersLinesTbl];
GO
    CREATE TABLE [TempOrdersLinesTbl]
    (
        [TOLineID] INT IDENTITY(1,1) NOT NULL,
        [TOHeaderID] INT NULL,
        [ItemID] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [Qty] REAL NULL,
        [ItemPackagingID] INT NULL,
        [OriginalOrderID] INT NULL
        , CONSTRAINT [PK_TempOrdersLinesTbl] PRIMARY KEY CLUSTERED ([TOLineID])
    );
GO

-- Drop FKs referencing or owned by [TempOrdersTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TempOrdersTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TempOrdersTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TempOrdersTbl]', N'U') IS NOT NULL DROP TABLE [TempOrdersTbl];
GO
    CREATE TABLE [TempOrdersTbl]
    (
        [TempOrderID] INT IDENTITY(1,1) NOT NULL,
        [OrderID] INT NULL,
        [ContactID] INT NULL,
        [OrderDate] DATETIME NULL,
        [RoastDate] DATETIME NULL,
        [ItemID] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [ItemPrepTypeID] INT NULL,
        [ItemPackagingID] INT NULL,
        [QtyOrdered] REAL NULL,
        [RequiredByDate] DATETIME NULL,
        [Delivered] BIT NULL,
        [Notes] NVARCHAR(255) NULL
        , CONSTRAINT [PK_TempOrdersTbl] PRIMARY KEY CLUSTERED ([TempOrderID])
    );
GO

-- Drop FKs referencing or owned by [TotalCountTrackerTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TotalCountTrackerTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TotalCountTrackerTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TotalCountTrackerTbl]', N'U') IS NOT NULL DROP TABLE [TotalCountTrackerTbl];
GO
    CREATE TABLE [TotalCountTrackerTbl]
    (
        [TotalCounterTrackerID] INT IDENTITY(1,1) NOT NULL,
        [CountDate] DATETIME NULL,
        [TotalCount] INT NULL,
        [Comments] NVARCHAR(255) NULL
        , CONSTRAINT [PK_TotalCountTrackerTbl] PRIMARY KEY CLUSTERED ([TotalCounterTrackerID])
    );
GO

-- Drop FKs referencing or owned by [TrackedServiceItemsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TrackedServiceItemsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TrackedServiceItemsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TrackedServiceItemsTbl]', N'U') IS NOT NULL DROP TABLE [TrackedServiceItemsTbl];
GO
    CREATE TABLE [TrackedServiceItemsTbl]
    (
        [TrackerServiceItemID] INT IDENTITY(1,1) NOT NULL,
        [ItemServiceTypeID] INT NULL,
        [TypicalAvePerItem] REAL NULL,
        [UsageDateFieldName] NVARCHAR(20) NULL,
        [UsageAveFieldName] NVARCHAR(20) NULL,
        [ThisItemSetsDailyAverage] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_TrackedServiceItemsTbl] PRIMARY KEY CLUSTERED ([TrackerServiceItemID])
    );
GO

-- Drop FKs referencing or owned by [TransactionTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TransactionTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TransactionTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TransactionTypesTbl]', N'U') IS NOT NULL DROP TABLE [TransactionTypesTbl];
GO
    CREATE TABLE [TransactionTypesTbl]
    (
        [TransactionID] INT IDENTITY(1,1) NOT NULL,
        [TransactionType] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_TransactionTypesTbl] PRIMARY KEY CLUSTERED ([TransactionID])
    );
GO

-- Drop FKs referencing or owned by [UsedItemGroupsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[UsedItemGroupsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[UsedItemGroupsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[UsedItemGroupsTbl]', N'U') IS NOT NULL DROP TABLE [UsedItemGroupsTbl];
GO
    CREATE TABLE [UsedItemGroupsTbl]
    (
        [UsedItemGroupID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [GroupReferenceItemID] INT NULL,
        [LastItemID] INT NULL,
        [LastItemSortPos] INT NULL,
        [LastItemDateChanged] DATETIME NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_UsedItemGroupsTbl] PRIMARY KEY CLUSTERED ([UsedItemGroupID])
    );
GO

