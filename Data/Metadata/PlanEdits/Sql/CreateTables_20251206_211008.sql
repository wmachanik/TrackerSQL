-- Auto-generated CREATE TABLE script
-- Metadata paths:
--   AccessSchema: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema
--   PlanConstraints: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\PlanConstraints.json
-- Tables: 43, Columns: 375
-- Identity suppressed: No
-- Drop existing tables: Yes
-- Plan scan (source -> decision):
-- [ArchivedCustomersTbl] IGNORE (plan.Ignore=true)
-- [ArichivedOrdersTbl] IGNORE (plan.Ignore=true)
-- [AwayReasonTbl] Class=Copy Target=AwayReasonTbl EmittedCols=2
-- [CityPrepDaysTbl] Class=Copy Target=CityPrepDaysTbl EmittedCols=5
-- [CityTbl] Class=Copy Target=CityTbl EmittedCols=4
-- [ClientAwayPeriodTbl] Class=Copy Target=ClientAwayPeriodTbl EmittedCols=5
-- [ClientUsageHistoryTbl] IGNORE (plan.Ignore=true)
-- [ClientUsageLinesTbl] Class=Copy Target=ClientUsageLinesTbl EmittedCols=7
-- [ClientUsageTbl] Class=Copy Target=ClientUsageTbl EmittedCols=12
-- [ClosureDatesTbl] Class=Copy Target=ClosureDatesTbl EmittedCols=5
-- [CustomersAccInfoTbl] Class=Copy Target=CustomersAccInfoTbl EmittedCols=30
-- [CustomersTbl] Class=Copy Target=CustomersTbl EmittedCols=43
-- [CustomerTrackedServiceItemsTbl] Class=Copy Target=CustomerTrackedServiceItemsTbl EmittedCols=4
-- [CustomerTypeTbl] Class=Copy Target=CustomerTypeTbl EmittedCols=3
-- [EquipTypeTbl] Class=Copy Target=EquipTypeTbl EmittedCols=3
-- [HolidayClosureTbl] Class=Copy Target=HolidayClosureTbl EmittedCols=7
-- [InvoiceTypeTbl] Class=Copy Target=InvoiceTypeTbl EmittedCols=4
-- [ItemGroupTbl] Class=Copy Target=ItemGroupTbl EmittedCols=6
-- [ItemTypeTbl] Class=Copy Target=ItemTypeTbl EmittedCols=13
-- [ItemUnitsTbl] Class=Copy Target=ItemUnitsTbl EmittedCols=3
-- [ItemUsageTbl] Class=Copy Target=ItemUsageTbl EmittedCols=8
-- [LogTbl] IGNORE (plan.Ignore=true)
-- [MachineConditionsTbl] Class=Copy Target=MachineConditionsTbl EmittedCols=4
-- [NextRoastDateByCityTbl] Class=Copy Target=NextRoastDateByCityTbl EmittedCols=7
-- [OrderList] IGNORE (plan.Ignore=true)
-- [OrdersTbl] IGNORE (plan.Ignore=true)
-- [OrdersTbl_Apr26_2008] IGNORE (plan.Ignore=true)
-- [PackagingTbl] Class=Copy Target=ItemPackagingsTbl EmittedCols=6
-- [PaymentTermsTbl] Class=Copy Target=PaymentTermsTbl EmittedCols=7
-- [PersonsTbl] Class=Copy Target=PersonsTbl EmittedCols=6
-- [PredictedOrdersTbl] IGNORE (plan.Ignore=true)
-- [PrepTypesTbl] Class=Copy Target=PrepTypesTbl EmittedCols=3
-- [PriceLevelsTbl] Class=Copy Target=PriceLevelsTbl EmittedCols=5
-- [ReoccuranceTypeTbl] Class=Copy Target=ReoccuranceTypeTbl EmittedCols=2
-- [ReoccuringOrderTbl] IGNORE (plan.Ignore=true)
-- [RepairFaultsTbl] Class=Copy Target=RepairFaultsTbl EmittedCols=4
-- [RepairStatusesTbl] Class=Copy Target=RepairStatusesTbl EmittedCols=6
-- [RepairsTbl] Class=Copy Target=RepairsTbl EmittedCols=22
-- [SectionTypesTbl] Class=Copy Target=SectionTypesTbl EmittedCols=3
-- [SendCheckEmailTextsTbl] Class=Copy Target=SendCheckEmailTextsTbl EmittedCols=6
-- [SentRemindersLogTbl] Class=Copy Target=SentRemindersLogTbl EmittedCols=7
-- [ServiceTypesTbl] Class=Copy Target=ServiceTypesTbl EmittedCols=5
-- [SysDataTbl] Class=Copy Target=SysDataTbl EmittedCols=7
-- [TempCoffeecheckupCustomerTbl] Class=Copy Target=TempCoffeecheckupCustomerTbl EmittedCols=25
-- [TempCoffeecheckupItemsTbl] Class=Copy Target=TempCoffeecheckupItemsTbl EmittedCols=9
-- [TempOrdersHeaderTbl] Class=Copy Target=TempOrdersHeaderTbl EmittedCols=9
-- [TempOrdersLinesTbl] Class=Copy Target=TempOrdersLinesTbl EmittedCols=7
-- [TempOrdersTbl] Class=Copy Target=TempOrdersTbl EmittedCols=13
-- [tmpOrdersReplyTbl] IGNORE (plan.Ignore=true)
-- [TotalCountTrackerTbl] Class=Copy Target=TotalCountTrackerTbl EmittedCols=4
-- [TrackedServiceItemTbl] Class=Copy Target=TrackedServiceItemTbl EmittedCols=7
-- [TransactionTypesTbl] Class=Copy Target=TransactionTypesTbl EmittedCols=3
-- [UsageAveTbl] IGNORE (plan.Ignore=true)
-- [UsageTblByDate] IGNORE (plan.Ignore=true)
-- [UsedItemGroupTbl] Class=Copy Target=UsedItemGroupTbl EmittedCols=7
-- [VisitLogTbl] IGNORE (plan.Ignore=true)
-- [WeekDaysTbl] IGNORE (plan.Ignore=true)
-- [_ClientUsageTbl] IGNORE (plan.Ignore=true)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

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
        [ReasonDesc] NVARCHAR(100) NOT NULL
        , CONSTRAINT [PK_AwayReasonTbl] PRIMARY KEY CLUSTERED ([AwayReasonID])
    );
GO

-- Drop FKs referencing or owned by [CityPrepDaysTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[CityPrepDaysTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[CityPrepDaysTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[CityPrepDaysTbl]', N'U') IS NOT NULL DROP TABLE [CityPrepDaysTbl];
GO
    CREATE TABLE [CityPrepDaysTbl]
    (
        [CityPrepDaysID] INT NULL,
        [CityID] INT NULL,
        [PrepDayOfWeekID] TINYINT NULL,
        [DeliveryDelayDays] SMALLINT NULL,
        [DeliveryOrder] SMALLINT NULL
    );
GO

-- Drop FKs referencing or owned by [CityTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[CityTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[CityTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[CityTbl]', N'U') IS NOT NULL DROP TABLE [CityTbl];
GO
    CREATE TABLE [CityTbl]
    (
        [ID] INT NULL,
        [City] NVARCHAR(255) NULL,
        [RoastingDay] INT NULL,
        [DeliveryDelay] INT NULL
    );
GO

-- Drop FKs referencing or owned by [ClientAwayPeriodTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ClientAwayPeriodTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ClientAwayPeriodTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ClientAwayPeriodTbl]', N'U') IS NOT NULL DROP TABLE [ClientAwayPeriodTbl];
GO
    CREATE TABLE [ClientAwayPeriodTbl]
    (
        [AwayPeriodID] INT NULL,
        [ClientID] INT NULL,
        [AwayStartDate] DATETIME NULL,
        [AwayEndDate] DATETIME NULL,
        [ReasonID] INT NULL
    );
GO

-- Drop FKs referencing or owned by [ClientUsageLinesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ClientUsageLinesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ClientUsageLinesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ClientUsageLinesTbl]', N'U') IS NOT NULL DROP TABLE [ClientUsageLinesTbl];
GO
    CREATE TABLE [ClientUsageLinesTbl]
    (
        [ClientUsageLineNo] INT NULL,
        [CustomerID] INT NULL,
        [Date] DATETIME NULL,
        [CupCount] INT NULL,
        [ServiceTypeId] INT NULL,
        [Qty] REAL NULL,
        [Notes] NVARCHAR(150) NULL
    );
GO

-- Drop FKs referencing or owned by [ClientUsageTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ClientUsageTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ClientUsageTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ClientUsageTbl]', N'U') IS NOT NULL DROP TABLE [ClientUsageTbl];
GO
    CREATE TABLE [ClientUsageTbl]
    (
        [CustomerId] INT NULL,
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
        [NextPrepDate] DATETIME NULL,
        [ID] INT NULL,
        [DateClosed] DATETIME NOT NULL,
        [DateReopen] DATETIME NULL,
        [NextRoastDate] DATETIME NULL,
        [Comments] NVARCHAR(255) NULL
        , CONSTRAINT [PK_ClosureDatesTbl] PRIMARY KEY CLUSTERED ([ClosureDateID])
    );
GO

-- Drop FKs referencing or owned by [CustomersAccInfoTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[CustomersAccInfoTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[CustomersAccInfoTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[CustomersAccInfoTbl]', N'U') IS NOT NULL DROP TABLE [CustomersAccInfoTbl];
GO
    CREATE TABLE [CustomersAccInfoTbl]
    (
        [CustomersAccInfoID] INT NULL,
        [CustomerID] INT NULL,
        [RequiresPurchOrder] BIT NULL,
        [CustomerVATNo] NVARCHAR(30) NULL,
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
    );
GO

-- Drop FKs referencing or owned by [CustomersTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[CustomersTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[CustomersTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[CustomersTbl]', N'U') IS NOT NULL DROP TABLE [CustomersTbl];
GO
    CREATE TABLE [CustomersTbl]
    (
        [CustomerID] INT NULL,
        [CompanyName] NVARCHAR(50) NULL,
        [ContactTitle] NVARCHAR(50) NULL,
        [ContactFirstName] NVARCHAR(30) NULL,
        [ContactLastName] NVARCHAR(50) NULL,
        [ContactAltFirstName] NVARCHAR(50) NULL,
        [ContactAltLastName] NVARCHAR(50) NULL,
        [Department] NVARCHAR(50) NULL,
        [BillingAddress] NVARCHAR(255) NULL,
        [City] INT NULL,
        [StateOrProvince] NVARCHAR(20) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Country/Region] NVARCHAR(50) NULL,
        [PhoneNumber] NVARCHAR(30) NULL,
        [Extension] NVARCHAR(30) NULL,
        [FaxNumber] NVARCHAR(30) NULL,
        [CellNumber] NVARCHAR(50) NULL,
        [EmailAddress] NVARCHAR(50) NULL,
        [AltEmailAddress] NVARCHAR(255) NULL,
        [ContractNo] NVARCHAR(50) NULL,
        [CustomerTypeID] INT NULL,
        [CustomerTypeOLD] NVARCHAR(30) NULL,
        [EquipType] INT NULL,
        [CoffeePreference] INT NULL,
        [PriPrefQty] REAL NULL,
        [PrefPrepTypeID] INT NULL,
        [PrefPackagingID] INT NULL,
        [SecondaryPreference] INT NULL,
        [SecPrefQty] REAL NULL,
        [TypicallySecToo] BIT NULL,
        [PreferedAgent] INT NULL,
        [SalesAgentID] INT NULL,
        [MachineSN] NVARCHAR(50) NULL,
        [UsesFilter] BIT NULL,
        [autofulfill] BIT NULL,
        [enabled] BIT NULL,
        [PredictionDisabled] BIT NULL,
        [AlwaysSendChkUp] BIT NULL,
        [NormallyResponds] BIT NULL,
        [ReminderCount] INT NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [SendDeliveryConfirmation] BIT NULL,
        [LastDateSentReminder] DATETIME NULL
    );
GO

-- Drop FKs referencing or owned by [CustomerTrackedServiceItemsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[CustomerTrackedServiceItemsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[CustomerTrackedServiceItemsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[CustomerTrackedServiceItemsTbl]', N'U') IS NOT NULL DROP TABLE [CustomerTrackedServiceItemsTbl];
GO
    CREATE TABLE [CustomerTrackedServiceItemsTbl]
    (
        [CustomerTrackedServiceItemsID] INT NULL,
        [CustomerTypeID] INT NULL,
        [ServiceTypeID] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
    );
GO

-- Drop FKs referencing or owned by [CustomerTypeTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[CustomerTypeTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[CustomerTypeTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[CustomerTypeTbl]', N'U') IS NOT NULL DROP TABLE [CustomerTypeTbl];
GO
    CREATE TABLE [CustomerTypeTbl]
    (
        [CustTypeID] INT NULL,
        [CustTypeDesc] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
    );
GO

-- Drop FKs referencing or owned by [EquipTypeTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[EquipTypeTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[EquipTypeTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[EquipTypeTbl]', N'U') IS NOT NULL DROP TABLE [EquipTypeTbl];
GO
    CREATE TABLE [EquipTypeTbl]
    (
        [EquipTypeId] INT NULL,
        [EquipTypeName] NVARCHAR(50) NULL,
        [EquipTypeDesc] NVARCHAR(50) NULL
    );
GO

-- Drop FKs referencing or owned by [HolidayClosureTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[HolidayClosureTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[HolidayClosureTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[HolidayClosureTbl]', N'U') IS NOT NULL DROP TABLE [HolidayClosureTbl];
GO
    CREATE TABLE [HolidayClosureTbl]
    (
        [ID] INT NULL,
        [ClosureDate] DATETIME NULL,
        [DaysClosed] INT NULL,
        [AppliesToPrep] BIT NULL,
        [AppliesToDelivery] BIT NULL,
        [ShiftStrategy] NVARCHAR(20) NULL,
        [Description] NVARCHAR(255) NULL
    );
GO

-- Drop FKs referencing or owned by [InvoiceTypeTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[InvoiceTypeTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[InvoiceTypeTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[InvoiceTypeTbl]', N'U') IS NOT NULL DROP TABLE [InvoiceTypeTbl];
GO
    CREATE TABLE [InvoiceTypeTbl]
    (
        [InvoiceTypeID] INT NULL,
        [InvoiceTypeDesc] NVARCHAR(20) NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
    );
GO

-- Drop FKs referencing or owned by [ItemGroupTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemGroupTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemGroupTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemGroupTbl]', N'U') IS NOT NULL DROP TABLE [ItemGroupTbl];
GO
    CREATE TABLE [ItemGroupTbl]
    (
        [ItemGroupID] INT NULL,
        [GroupItemTypeID] INT NULL,
        [ItemTypeID] INT NULL,
        [ItemTypeSortPos] INT NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
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
        [ItemPackagingDesc] NVARCHAR(50) NOT NULL,
        [AdditionalNotes] NVARCHAR(255) NULL,
        [Symbol] NVARCHAR(255) NULL,
        [Colour] INT NULL,
        [BGColour] NVARCHAR(9) NULL
        , CONSTRAINT [PK_ItemPackagingsTbl] PRIMARY KEY CLUSTERED ([ItemPackagingID])
    );
GO

-- Drop FKs referencing or owned by [ItemTypeTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemTypeTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemTypeTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemTypeTbl]', N'U') IS NOT NULL DROP TABLE [ItemTypeTbl];
GO
    CREATE TABLE [ItemTypeTbl]
    (
        [ItemTypeID] INT NULL,
        [SKU] NVARCHAR(20) NULL,
        [ItemDesc] NVARCHAR(50) NULL,
        [ItemEnabled] BIT NULL,
        [ItemsCharacteritics] NVARCHAR(50) NULL,
        [ItemDetail] NVARCHAR(MAX) NULL,
        [ServiceTypeId] INT NULL,
        [ReplacementID] INT NULL,
        [ItemShortName] NVARCHAR(10) NULL,
        [SortOrder] INT NULL,
        [UnitsPerQty] REAL NULL,
        [ItemUnitID] INT NULL,
        [BasePrice] REAL NULL
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
        [UnitOfMeasure] NVARCHAR(5) NOT NULL,
        [UnitDescription] NVARCHAR(50) NULL
        , CONSTRAINT [PK_ItemUnitsTbl] PRIMARY KEY CLUSTERED ([ItemUnitID])
    );
GO

-- Drop FKs referencing or owned by [ItemUsageTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ItemUsageTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ItemUsageTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ItemUsageTbl]', N'U') IS NOT NULL DROP TABLE [ItemUsageTbl];
GO
    CREATE TABLE [ItemUsageTbl]
    (
        [ClientUsageLineNo] INT NULL,
        [CustomerID] INT NULL,
        [Date] DATETIME NULL,
        [ItemProvided] INT NULL,
        [AmountProvided] REAL NULL,
        [PrepTypeID] INT NULL,
        [PackagingID] INT NULL,
        [Notes] NVARCHAR(150) NULL
    );
GO

-- Drop FKs referencing or owned by [MachineConditionsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[MachineConditionsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[MachineConditionsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[MachineConditionsTbl]', N'U') IS NOT NULL DROP TABLE [MachineConditionsTbl];
GO
    CREATE TABLE [MachineConditionsTbl]
    (
        [MachineConditionID] INT NULL,
        [ConditionDesc] NVARCHAR(50) NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
    );
GO

-- Drop FKs referencing or owned by [NextRoastDateByCityTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[NextRoastDateByCityTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[NextRoastDateByCityTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[NextRoastDateByCityTbl]', N'U') IS NOT NULL DROP TABLE [NextRoastDateByCityTbl];
GO
    CREATE TABLE [NextRoastDateByCityTbl]
    (
        [NextRoastDayID] INT NULL,
        [CityID] INT NULL,
        [PreperationDate] DATETIME NULL,
        [DeliveryDate] DATETIME NULL,
        [DeliveryOrder] SMALLINT NULL,
        [NextPreperationDate] DATETIME NULL,
        [NextDeliveryDate] DATETIME NULL
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
        [PaymentTermDesc] NVARCHAR(20) NOT NULL,
        [PaymentDays] INT NULL,
        [DayOfMonth] INT NULL,
        [UseDays] BIT NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_PaymentTermsTbl] PRIMARY KEY CLUSTERED ([PaymentTermID])
    );
GO

-- Drop FKs referencing or owned by [PersonsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[PersonsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[PersonsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[PersonsTbl]', N'U') IS NOT NULL DROP TABLE [PersonsTbl];
GO
    CREATE TABLE [PersonsTbl]
    (
        [PersonID] INT NULL,
        [Person] NVARCHAR(50) NULL,
        [Abreviation] NVARCHAR(5) NULL,
        [Enabled] BIT NULL,
        [NormalDeliveryDoW] INT NULL,
        [SecurityUsername] NVARCHAR(255) NULL
    );
GO

-- Drop FKs referencing or owned by [PrepTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[PrepTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[PrepTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[PrepTypesTbl]', N'U') IS NOT NULL DROP TABLE [PrepTypesTbl];
GO
    CREATE TABLE [PrepTypesTbl]
    (
        [PrepID] INT NULL,
        [PrepType] NVARCHAR(50) NULL,
        [IdentifyingChar] NVARCHAR(1) NULL
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
        [PriceLevelDesc] NVARCHAR(20) NOT NULL,
        [PricingFactor] REAL NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_PriceLevelsTbl] PRIMARY KEY CLUSTERED ([PriceLevelID])
    );
GO

-- Drop FKs referencing or owned by [ReoccuranceTypeTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ReoccuranceTypeTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ReoccuranceTypeTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ReoccuranceTypeTbl]', N'U') IS NOT NULL DROP TABLE [ReoccuranceTypeTbl];
GO
    CREATE TABLE [ReoccuranceTypeTbl]
    (
        [ID] INT NULL,
        [Type] NVARCHAR(255) NULL
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
        [RepairFaultDesc] NVARCHAR(50) NOT NULL,
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
        [EmailContact] BIT NULL,
        [RepairStatusDesc] NVARCHAR(50) NOT NULL,
        [EmailClient] BIT NULL,
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
        [ContactID] INT NOT NULL,
        [EquipConditionID] INT NULL,
        [EquipSerialNumber] NVARCHAR(50) NULL,
        [EquipTypeID] INT NULL,
        [CustomerID] INT NULL,
        [ContactName] NVARCHAR(50) NULL,
        [ContactEmail] NVARCHAR(50) NULL,
        [JobCardNumber] NVARCHAR(20) NULL,
        [DateLogged] DATETIME NULL,
        [LastStatusChange] DATETIME NULL,
        [MachineTypeID] INT NULL,
        [MachineSerialNumber] NVARCHAR(50) NULL,
        [SwopOutMachineID] INT NULL,
        [MachineConditionID] INT NULL,
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
        [SectionType] NVARCHAR(50) NOT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_SectionTypesTbl] PRIMARY KEY CLUSTERED ([SectionID])
    );
GO

-- Drop FKs referencing or owned by [SendCheckEmailTextsTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[SendCheckEmailTextsTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[SendCheckEmailTextsTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[SendCheckEmailTextsTbl]', N'U') IS NOT NULL DROP TABLE [SendCheckEmailTextsTbl];
GO
    CREATE TABLE [SendCheckEmailTextsTbl]
    (
        [SCEMTID] INT NULL,
        [Header] NVARCHAR(MAX) NULL,
        [Body] NVARCHAR(MAX) NULL,
        [Footer] NVARCHAR(MAX) NULL,
        [DateLastChange] DATETIME NULL,
        [Notes] NVARCHAR(MAX) NULL
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
        [ContactID] INT NOT NULL,
        [HadRecurrItems] BIT NULL,
        [CustomerID] INT NULL,
        [DateSentReminder] DATETIME NULL,
        [NextPrepDate] DATETIME NULL,
        [ReminderSent] BIT NULL,
        [HadAutoFulfilItem] BIT NULL,
        [HadReoccurItems] BIT NULL
        , CONSTRAINT [PK_SentRemindersLogTbl] PRIMARY KEY CLUSTERED ([ReminderID])
    );
GO

-- Drop FKs referencing or owned by [ServiceTypesTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[ServiceTypesTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[ServiceTypesTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[ServiceTypesTbl]', N'U') IS NOT NULL DROP TABLE [ServiceTypesTbl];
GO
    CREATE TABLE [ServiceTypesTbl]
    (
        [ServiceTypeId] INT NULL,
        [ServiceType] NVARCHAR(20) NULL,
        [Description] NVARCHAR(100) NULL,
        [PackagingID] INT NULL,
        [PrepTypeID] INT NULL
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
        [GroupReferenceItemID] INT NULL,
        [InternalContactIDs] NVARCHAR(255) NULL,
        [LastReoccurringDate] DATETIME NULL,
        [DoReoccuringOrders] BIT NULL,
        [DateLastPrepDateCalcd] DATETIME NULL,
        [MinReminderDate] DATETIME NULL,
        [GroupItemTypeID] INT NULL,
        [InternalCustomerIds] NVARCHAR(255) NULL
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
        [AreaID] INT NULL,
        [ContactID] INT NULL,
        [ContactTypeID] INT NULL,
        [CustomerID] INT NULL,
        [CompanyName] NVARCHAR(50) NULL,
        [ContactFirstName] NVARCHAR(50) NULL,
        [ContactAltFirstName] NVARCHAR(50) NULL,
        [CityID] INT NULL,
        [EmailAddress] NVARCHAR(50) NULL,
        [AltEmailAddress] NVARCHAR(50) NULL,
        [CustomerTypeID] INT NULL,
        [EquipTypeID] INT NULL,
        [TypicallySecToo] BIT NULL,
        [PreferedAgentID] INT NULL,
        [SalesAgentID] INT NULL,
        [UsesFilter] BIT NULL,
        [enabled] BIT NULL,
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
        [ContactID] INT NOT NULL,
        [ItemPackagingID] INT NULL,
        [RecurringOrderItemID] INT NULL,
        [CustomerID] INT NULL,
        [ItemID] INT NULL,
        [ItemQty] REAL NULL,
        [ItemPrepID] INT NULL,
        [ItemPackagID] INT NULL,
        [AutoFulfill] BIT NULL,
        [NextDateRequired] DATETIME NULL,
        [ReoccurOrderID] INT NULL
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
        [ContactID] INT NOT NULL,
        [CustomerID] INT NULL,
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
        [ItemPackagingID] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [TOHeaderID] INT NULL,
        [ItemID] INT NULL,
        [ServiceTypeID] INT NULL,
        [Qty] REAL NULL,
        [PackagingID] INT NULL,
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
        [ContactID] INT NULL,
        [ItemID] INT NULL,
        [ItemPackagingID] INT NULL,
        [ItemPrepTypeID] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [QtyOrdered] REAL NULL,
        [OrderID] INT NOT NULL,
        [CustomerId] INT NULL,
        [OrderDate] DATETIME NULL,
        [RoastDate] DATETIME NULL,
        [ItemTypeID] INT NULL,
        [ServiceTypeId] INT NULL,
        [PrepTypeID] INT NULL,
        [PackagingId] INT NULL,
        [QuantityOrdered] REAL NULL,
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
        [ID] INT NULL,
        [CountDate] DATETIME NULL,
        [TotalCount] INT NULL,
        [Comments] NVARCHAR(255) NULL
        , CONSTRAINT [PK_TotalCountTrackerTbl] PRIMARY KEY CLUSTERED ([TotalCounterTrackerID])
    );
GO

-- Drop FKs referencing or owned by [TrackedServiceItemTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[TrackedServiceItemTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[TrackedServiceItemTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[TrackedServiceItemTbl]', N'U') IS NOT NULL DROP TABLE [TrackedServiceItemTbl];
GO
    CREATE TABLE [TrackedServiceItemTbl]
    (
        [TrackerServiceItemID] INT NULL,
        [ServiceTypeID] INT NULL,
        [TypicalAvePerItem] REAL NULL,
        [UsageDateFieldName] NVARCHAR(20) NULL,
        [UsageAveFieldName] NVARCHAR(20) NULL,
        [ThisItemSetsDailyAverage] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
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
        [TransactionType] NVARCHAR(50) NOT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_TransactionTypesTbl] PRIMARY KEY CLUSTERED ([TransactionID])
    );
GO

-- Drop FKs referencing or owned by [UsedItemGroupTbl]
DECLARE @sql nvarchar(max) = N'';
SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(o.schema_id)) + N'.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(13)
FROM sys.foreign_keys fk
JOIN sys.objects o ON fk.parent_object_id = o.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'[UsedItemGroupTbl]') OR fk.referenced_object_id = OBJECT_ID(N'[UsedItemGroupTbl]');
IF LEN(@sql) > 0 EXEC sp_executesql @sql;
IF OBJECT_ID(N'[UsedItemGroupTbl]', N'U') IS NOT NULL DROP TABLE [UsedItemGroupTbl];
GO
    CREATE TABLE [UsedItemGroupTbl]
    (
        [UsedItemGroupID] INT NULL,
        [ContactID] INT NULL,
        [GroupItemTypeID] INT NULL,
        [LastItemTypeID] INT NULL,
        [LastItemTypeSortPos] INT NULL,
        [LastItemDateChanged] DATETIME NULL,
        [Notes] NVARCHAR(MAX) NULL
    );
GO

