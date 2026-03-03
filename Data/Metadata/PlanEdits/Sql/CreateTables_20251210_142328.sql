-- Auto-generated CREATE TABLE script
-- Metadata paths:
--   AccessSchema: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema
--   PlanConstraints: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\PlanConstraints.json
-- Tables: 47, Columns: 379
-- Identity suppressed: No
-- Drop existing tables: No
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

IF OBJECT_ID(N'[AreaPrepDaysTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [AreaPrepDaysTbl]
    (
        [AreaPrepDaysID] INT IDENTITY(1,1) NOT NULL,
        [AreaID] INT NULL,
        [PrepDayOfWeekID] TINYINT NULL,
        [DeliveryDelayDays] SMALLINT NULL,
        [DeliveryOrder] SMALLINT NULL
        , CONSTRAINT [PK_AreaPrepDaysTbl] PRIMARY KEY CLUSTERED ([AreaPrepDaysID])
    );
END
GO

IF OBJECT_ID(N'[AreasTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [AreasTbl]
    (
        [AreaID] INT IDENTITY(1,1) NOT NULL,
        [AreaName] NVARCHAR(255) NULL,
        [PrepDayOfWeekID] INT NULL,
        [DeliveryDelay] INT NULL
        , CONSTRAINT [PK_AreasTbl] PRIMARY KEY CLUSTERED ([AreaID])
    );
END
GO

IF OBJECT_ID(N'[AwayReasonTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [AwayReasonTbl]
    (
        [AwayReasonID] INT IDENTITY(1,1) NOT NULL,
        [ReasonDesc] NVARCHAR(100) NULL
        , CONSTRAINT [PK_AwayReasonTbl] PRIMARY KEY CLUSTERED ([AwayReasonID])
    );
END
GO

IF OBJECT_ID(N'[ClosureDatesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ClosureDatesTbl]
    (
        [ClosureDateID] INT IDENTITY(1,1) NOT NULL,
        [DateClosed] DATETIME NULL,
        [DateReopen] DATETIME NULL,
        [NextPrepDate] DATETIME NULL,
        [Comments] NVARCHAR(255) NULL
        , CONSTRAINT [PK_ClosureDatesTbl] PRIMARY KEY CLUSTERED ([ClosureDateID])
    );
END
GO

IF OBJECT_ID(N'[ContactsAccInfoTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[ContactsAwayPeriodTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ContactsAwayPeriodTbl]
    (
        [AwayPeriodID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
        [AwayStartDate] DATETIME NULL,
        [AwayEndDate] DATETIME NULL,
        [ReasonID] INT NULL
        , CONSTRAINT [PK_ContactsAwayPeriodTbl] PRIMARY KEY CLUSTERED ([AwayPeriodID])
    );
END
GO

IF OBJECT_ID(N'[ContactsItemUsageTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[ContactsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[ContactsUsageTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[ContactTrackedServiceItemsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ContactTrackedServiceItemsTbl]
    (
        [ContactTrackedServiceItemsID] INT IDENTITY(1,1) NOT NULL,
        [ContactTypeID] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_ContactTrackedServiceItemsTbl] PRIMARY KEY CLUSTERED ([ContactTrackedServiceItemsID])
    );
END
GO

IF OBJECT_ID(N'[ContactTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ContactTypesTbl]
    (
        [ContactTypeID] INT IDENTITY(1,1) NOT NULL,
        [ContactTypeDesc] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_ContactTypesTbl] PRIMARY KEY CLUSTERED ([ContactTypeID])
    );
END
GO

IF OBJECT_ID(N'[ContactUsageLinesTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[EquipConditionsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [EquipConditionsTbl]
    (
        [EquipConditionID] INT IDENTITY(1,1) NOT NULL,
        [ConditionDesc] NVARCHAR(50) NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_EquipConditionsTbl] PRIMARY KEY CLUSTERED ([EquipConditionID])
    );
END
GO

IF OBJECT_ID(N'[EquipTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [EquipTypesTbl]
    (
        [EquipTypeID] INT IDENTITY(1,1) NOT NULL,
        [EquipTypeName] NVARCHAR(50) NULL,
        [EquipTypeDescription] NVARCHAR(50) NULL
        , CONSTRAINT [PK_EquipTypesTbl] PRIMARY KEY CLUSTERED ([EquipTypeID])
    );
END
GO

IF OBJECT_ID(N'[HolidayClosuresTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[InvoiceTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [InvoiceTypesTbl]
    (
        [InvoiceTypeID] INT IDENTITY(1,1) NOT NULL,
        [InvoiceTypeDesc] NVARCHAR(20) NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_InvoiceTypesTbl] PRIMARY KEY CLUSTERED ([InvoiceTypeID])
    );
END
GO

IF OBJECT_ID(N'[ItemGroupsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[ItemPackagingsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[ItemPrepTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ItemPrepTypesTbl]
    (
        [ItemPrepID] INT IDENTITY(1,1) NOT NULL,
        [ItemPrepType] NVARCHAR(50) NULL,
        [IdentifyingChar] NVARCHAR(1) NULL
        , CONSTRAINT [PK_ItemPrepTypesTbl] PRIMARY KEY CLUSTERED ([ItemPrepID])
    );
END
GO

IF OBJECT_ID(N'[ItemServiceTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ItemServiceTypesTbl]
    (
        [ItemServiceTypeID] INT IDENTITY(1,1) NOT NULL,
        [ItemServiceType] NVARCHAR(20) NULL,
        [Description] NVARCHAR(100) NULL,
        [ItemPackagingID] INT NULL,
        [ItemPrepTypeID] INT NULL
        , CONSTRAINT [PK_ItemServiceTypesTbl] PRIMARY KEY CLUSTERED ([ItemServiceTypeID])
    );
END
GO

IF OBJECT_ID(N'[ItemsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[ItemUnitsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ItemUnitsTbl]
    (
        [ItemUnitID] INT IDENTITY(1,1) NOT NULL,
        [UnitOfMeasure] NVARCHAR(5) NULL,
        [UnitDescription] NVARCHAR(50) NULL
        , CONSTRAINT [PK_ItemUnitsTbl] PRIMARY KEY CLUSTERED ([ItemUnitID])
    );
END
GO

IF OBJECT_ID(N'[NextPrepDateByAreasTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[OrderLinesTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[OrdersTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[PaymentTermsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[PeopleTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[PriceLevelsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [PriceLevelsTbl]
    (
        [PriceLevelID] INT IDENTITY(1,1) NOT NULL,
        [PriceLevelDesc] NVARCHAR(20) NULL,
        [PricingFactor] REAL NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_PriceLevelsTbl] PRIMARY KEY CLUSTERED ([PriceLevelID])
    );
END
GO

IF OBJECT_ID(N'[RecurranceTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [RecurranceTypesTbl]
    (
        [RecurringTypeID] INT IDENTITY(1,1) NOT NULL,
        [RecurringTypeDesc] NVARCHAR(255) NULL
        , CONSTRAINT [PK_RecurranceTypesTbl] PRIMARY KEY CLUSTERED ([RecurringTypeID])
    );
END
GO

IF OBJECT_ID(N'[RecurringOrderItemsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [RecurringOrderItemsTbl]
    (
        [RecurringOrderItemID] INT NULL,
        [RecurringOrderID] INT NOT NULL,
        [ItemRequiredID] INT NULL,
        [QtyRequired] REAL NULL,
        [ItemPackagingID] INT NULL
    );
END
GO

IF OBJECT_ID(N'[RecurringOrdersTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[RepairFaultsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [RepairFaultsTbl]
    (
        [RepairFaultID] INT IDENTITY(1,1) NOT NULL,
        [RepairFaultDesc] NVARCHAR(50) NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_RepairFaultsTbl] PRIMARY KEY CLUSTERED ([RepairFaultID])
    );
END
GO

IF OBJECT_ID(N'[RepairStatusesTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[RepairsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[SectionTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [SectionTypesTbl]
    (
        [SectionID] INT IDENTITY(1,1) NOT NULL,
        [SectionType] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_SectionTypesTbl] PRIMARY KEY CLUSTERED ([SectionID])
    );
END
GO

IF OBJECT_ID(N'[SendCheckupEmailTextsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[SentRemindersLogTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[SysDataTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[TempCoffeecheckupItemsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[TempOrdersHeaderTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[TempOrdersLinesTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[TempOrdersTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[TotalCountTrackerTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TotalCountTrackerTbl]
    (
        [TotalCounterTrackerID] INT IDENTITY(1,1) NOT NULL,
        [CountDate] DATETIME NULL,
        [TotalCount] INT NULL,
        [Comments] NVARCHAR(255) NULL
        , CONSTRAINT [PK_TotalCountTrackerTbl] PRIMARY KEY CLUSTERED ([TotalCounterTrackerID])
    );
END
GO

IF OBJECT_ID(N'[TrackedServiceItemsTbl]', N'U') IS NULL
BEGIN
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
END
GO

IF OBJECT_ID(N'[TransactionTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TransactionTypesTbl]
    (
        [TransactionID] INT IDENTITY(1,1) NOT NULL,
        [TransactionType] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_TransactionTypesTbl] PRIMARY KEY CLUSTERED ([TransactionID])
    );
END
GO

IF OBJECT_ID(N'[UsedItemGroupsTbl]', N'U') IS NULL
BEGIN
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
END
GO

