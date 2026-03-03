-- Auto-generated CREATE TABLE script
-- Metadata paths:
--   AccessSchema: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\AccessSchema
--   PlanConstraints: C:\SRC\ASP.net\TrackerSQL\Data\Metadata\PlanEdits\PlanConstraints.json
-- Tables: 45, Columns: 398
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
-- [CustomersTbl] Class=Copy Target=ContactsTbl EmittedCols=43
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
-- [ReoccuringOrderTbl] IGNORE (plan.Ignore=true)
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
        [CityID] INT IDENTITY(1,1) NOT NULL,
        [AreaID] INT NULL,
        [AreaName] NVARCHAR(255) NULL,
        [PrepDayOfWeekID] INT NULL,
        [DeliveryDelay] INT NULL
        , CONSTRAINT [PK_AreasTbl] PRIMARY KEY CLUSTERED ([CityID])
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
        [ClosureDatesID] INT IDENTITY(1,1) NOT NULL,
        [ClosureDateID] INT NULL,
        [DateClosed] DATETIME NULL,
        [DateReopen] DATETIME NULL,
        [NextPrepDate] DATETIME NULL,
        [Comments] NVARCHAR(255) NULL
        , CONSTRAINT [PK_ClosureDatesTbl] PRIMARY KEY CLUSTERED ([ClosureDatesID])
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
        [ClientAwayPeriodID] INT IDENTITY(1,1) NOT NULL,
        [AwayPeriodID] INT NULL,
        [ContactID] INT NULL,
        [AwayStartDate] DATETIME NULL,
        [AwayEndDate] DATETIME NULL,
        [ReasonID] INT NULL
        , CONSTRAINT [PK_ContactsAwayPeriodTbl] PRIMARY KEY CLUSTERED ([ClientAwayPeriodID])
    );
END
GO

IF OBJECT_ID(N'[ContactsItemUsageTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ContactsItemUsageTbl]
    (
        [ItemUsageID] INT IDENTITY(1,1) NOT NULL,
        [ContactUsageLineNo] INT NULL,
        [ContactID] INT NULL,
        [DeliveryDate] DATETIME NULL,
        [ItemProvidedID] INT NULL,
        [QtyProvided] REAL NULL,
        [ItemPrepTypeID] INT NULL,
        [ItemPackagingID] INT NULL,
        [Notes] NVARCHAR(150) NULL
        , CONSTRAINT [PK_ContactsItemUsageTbl] PRIMARY KEY CLUSTERED ([ItemUsageID])
    );
END
GO

IF OBJECT_ID(N'[ContactsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ContactsTbl]
    (
        [CustomersID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
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
        [CustomerTypeOLD] NVARCHAR(30) NULL,
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
        [autofulfill] BIT NULL,
        [enabled] BIT NULL,
        [PredictionDisabled] BIT NULL,
        [AlwaysSendChkUp] BIT NULL,
        [NormallyResponds] BIT NULL,
        [ReminderCount] INT NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [SendDeliveryConfirmation] BIT NULL,
        [LastDateSentReminder] DATETIME NULL
        , CONSTRAINT [PK_ContactsTbl] PRIMARY KEY CLUSTERED ([CustomersID])
    );
END
GO

IF OBJECT_ID(N'[ContactsUsageTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ContactsUsageTbl]
    (
        [ClientUsageID] INT IDENTITY(1,1) NOT NULL,
        [ContactID] INT NULL,
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
        , CONSTRAINT [PK_ContactsUsageTbl] PRIMARY KEY CLUSTERED ([ClientUsageID])
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
        [CustomerTypeID] INT IDENTITY(1,1) NOT NULL,
        [ContactTypeID] INT NULL,
        [ContactTypeDesc] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_ContactTypesTbl] PRIMARY KEY CLUSTERED ([CustomerTypeID])
    );
END
GO

IF OBJECT_ID(N'[ContactUsageLinesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ContactUsageLinesTbl]
    (
        [ClientUsageLinesID] INT IDENTITY(1,1) NOT NULL,
        [ContactUsageLineNo] INT NULL,
        [ContactID] INT NULL,
        [UsageDate] DATETIME NULL,
        [CupCount] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [Qty] REAL NULL,
        [Notes] NVARCHAR(150) NULL
        , CONSTRAINT [PK_ContactUsageLinesTbl] PRIMARY KEY CLUSTERED ([ClientUsageLinesID])
    );
END
GO

IF OBJECT_ID(N'[EquipConditionsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [EquipConditionsTbl]
    (
        [MachineConditionsID] INT IDENTITY(1,1) NOT NULL,
        [EquipConditionID] INT NULL,
        [ConditionDesc] NVARCHAR(50) NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_EquipConditionsTbl] PRIMARY KEY CLUSTERED ([MachineConditionsID])
    );
END
GO

IF OBJECT_ID(N'[EquipTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [EquipTypesTbl]
    (
        [EquipTypeId] INT IDENTITY(1,1) NOT NULL,
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
        [PrepTypesID] INT IDENTITY(1,1) NOT NULL,
        [ItemPrepID] INT NULL,
        [ItemPrepType] NVARCHAR(50) NULL,
        [IdentifyingChar] NVARCHAR(1) NULL
        , CONSTRAINT [PK_ItemPrepTypesTbl] PRIMARY KEY CLUSTERED ([PrepTypesID])
    );
END
GO

IF OBJECT_ID(N'[ItemServiceTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [ItemServiceTypesTbl]
    (
        [ServiceTypesID] INT IDENTITY(1,1) NOT NULL,
        [ItemServiceTypeID] INT NULL,
        [ItemServiceType] NVARCHAR(20) NULL,
        [Description] NVARCHAR(100) NULL,
        [ItemPackagingID] INT NULL,
        [ItemPrepTypeID] INT NULL
        , CONSTRAINT [PK_ItemServiceTypesTbl] PRIMARY KEY CLUSTERED ([ServiceTypesID])
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
        [ItemUnitsID] INT IDENTITY(1,1) NOT NULL,
        [ItemUnitID] INT NULL,
        [UnitOfMeasure] NVARCHAR(5) NULL,
        [UnitDescription] NVARCHAR(50) NULL
        , CONSTRAINT [PK_ItemUnitsTbl] PRIMARY KEY CLUSTERED ([ItemUnitsID])
    );
END
GO

IF OBJECT_ID(N'[NextPrepDateByAreasTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [NextPrepDateByAreasTbl]
    (
        [NextRoastDateByCityID] INT IDENTITY(1,1) NOT NULL,
        [NextPrepDayID] INT NULL,
        [AreaID] INT NULL,
        [PreperationDate] DATETIME NULL,
        [DeliveryDate] DATETIME NULL,
        [DeliveryOrder] SMALLINT NULL,
        [NextPrepDate] DATETIME NULL,
        [NextDeliveryDate] DATETIME NULL
        , CONSTRAINT [PK_NextPrepDateByAreasTbl] PRIMARY KEY CLUSTERED ([NextRoastDateByCityID])
    );
END
GO

IF OBJECT_ID(N'[OrderLinesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [OrderLinesTbl]
    (
        [OrderLineID] INT NULL,
        [OrderID] INT NOT NULL,
        [RoastDate] DATETIME NULL,
        [ItemTypeID] INT NULL,
        [QuantityOrdered] REAL NULL,
        [PrepTypeID] INT NULL,
        [PackagingID] INT NULL
    );
END
GO

IF OBJECT_ID(N'[OrdersTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [OrdersTbl]
    (
        [OrderID] INT NOT NULL,
        [CustomerId] INT NULL,
        [OrderDate] DATETIME NULL,
        [RequiredByDate] DATETIME NULL,
        [ToBeDeliveredBy] INT NULL,
        [Confirmed] BIT NULL,
        [Done] BIT NULL,
        [Packed] BIT NULL,
        [Notes] NVARCHAR(255) NULL,
        [PurchaseOrder] NVARCHAR(30) NULL,
        [InvoiceDone] BIT NULL
    );
END
GO

IF OBJECT_ID(N'[PaymentTermsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [PaymentTermsTbl]
    (
        [PaymentTermsID] INT IDENTITY(1,1) NOT NULL,
        [PaymentTermID] INT NULL,
        [PaymentTermDesc] NVARCHAR(20) NULL,
        [PaymentDays] INT NULL,
        [DayOfMonth] INT NULL,
        [UseDays] BIT NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_PaymentTermsTbl] PRIMARY KEY CLUSTERED ([PaymentTermsID])
    );
END
GO

IF OBJECT_ID(N'[PeopleTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [PeopleTbl]
    (
        [PersonsID] INT IDENTITY(1,1) NOT NULL,
        [PersonID] INT NULL,
        [Person] NVARCHAR(50) NULL,
        [Abbreviation] NVARCHAR(5) NULL,
        [Enabled] BIT NULL,
        [NormalDeliveryDoW] INT NULL,
        [SecurityUsername] NVARCHAR(255) NULL
        , CONSTRAINT [PK_PeopleTbl] PRIMARY KEY CLUSTERED ([PersonsID])
    );
END
GO

IF OBJECT_ID(N'[PriceLevelsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [PriceLevelsTbl]
    (
        [PriceLevelsID] INT IDENTITY(1,1) NOT NULL,
        [PriceLevelID] INT NULL,
        [PriceLevelDesc] NVARCHAR(20) NULL,
        [PricingFactor] REAL NULL,
        [Enabled] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_PriceLevelsTbl] PRIMARY KEY CLUSTERED ([PriceLevelsID])
    );
END
GO

IF OBJECT_ID(N'[RecurranceTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [RecurranceTypesTbl]
    (
        [ReoccuranceTypeID] INT IDENTITY(1,1) NOT NULL,
        [RecurringTypeID] INT NULL,
        [RecurringTypeDesc] NVARCHAR(255) NULL
        , CONSTRAINT [PK_RecurranceTypesTbl] PRIMARY KEY CLUSTERED ([ReoccuranceTypeID])
    );
END
GO

IF OBJECT_ID(N'[RepairFaultsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [RepairFaultsTbl]
    (
        [RepairFaultsID] INT IDENTITY(1,1) NOT NULL,
        [RepairFaultID] INT NULL,
        [RepairFaultDesc] NVARCHAR(50) NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_RepairFaultsTbl] PRIMARY KEY CLUSTERED ([RepairFaultsID])
    );
END
GO

IF OBJECT_ID(N'[RepairStatusesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [RepairStatusesTbl]
    (
        [RepairStatusesID] INT IDENTITY(1,1) NOT NULL,
        [RepairStatusID] INT NULL,
        [RepairStatusDesc] NVARCHAR(50) NULL,
        [EmailContact] BIT NULL,
        [SortOrder] INT NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [StatusNote] NVARCHAR(255) NULL
        , CONSTRAINT [PK_RepairStatusesTbl] PRIMARY KEY CLUSTERED ([RepairStatusesID])
    );
END
GO

IF OBJECT_ID(N'[RepairsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [RepairsTbl]
    (
        [RepairsID] INT IDENTITY(1,1) NOT NULL,
        [RepairID] INT NULL,
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
        , CONSTRAINT [PK_RepairsTbl] PRIMARY KEY CLUSTERED ([RepairsID])
    );
END
GO

IF OBJECT_ID(N'[SectionTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [SectionTypesTbl]
    (
        [SectionTypesID] INT IDENTITY(1,1) NOT NULL,
        [SectionID] INT NULL,
        [SectionType] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_SectionTypesTbl] PRIMARY KEY CLUSTERED ([SectionTypesID])
    );
END
GO

IF OBJECT_ID(N'[SendCheckupEmailTextsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [SendCheckupEmailTextsTbl]
    (
        [SendCheckEmailTextsID] INT IDENTITY(1,1) NOT NULL,
        [SCEMTID] INT NULL,
        [HeaderText] NVARCHAR(MAX) NULL,
        [BodyText] NVARCHAR(MAX) NULL,
        [FooterText] NVARCHAR(MAX) NULL,
        [DateLastChange] DATETIME NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_SendCheckupEmailTextsTbl] PRIMARY KEY CLUSTERED ([SendCheckEmailTextsID])
    );
END
GO

IF OBJECT_ID(N'[SentRemindersLogTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [SentRemindersLogTbl]
    (
        [SentRemindersLogID] INT IDENTITY(1,1) NOT NULL,
        [ReminderID] INT NULL,
        [ContactID] INT NULL,
        [DateSentReminder] DATETIME NULL,
        [NextPrepDate] DATETIME NULL,
        [ReminderSent] BIT NULL,
        [HadAutoFulfilItem] BIT NULL,
        [HadRecurrItems] BIT NULL
        , CONSTRAINT [PK_SentRemindersLogTbl] PRIMARY KEY CLUSTERED ([SentRemindersLogID])
    );
END
GO

IF OBJECT_ID(N'[SysDataTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [SysDataTbl]
    (
        [SysDataID] INT IDENTITY(1,1) NOT NULL,
        [ID] INT NULL,
        [LastReoccurringDate] DATETIME NULL,
        [DoReoccuringOrders] BIT NULL,
        [DateLastPrepDateCalcd] DATETIME NULL,
        [MinReminderDate] DATETIME NULL,
        [GroupReferenceItemID] INT NULL,
        [InternalContactIDs] NVARCHAR(255) NULL
        , CONSTRAINT [PK_SysDataTbl] PRIMARY KEY CLUSTERED ([SysDataID])
    );
END
GO

IF OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TempCoffeecheckupCustomerTbl]
    (
        [TempCoffeecheckupCustomerID] INT IDENTITY(1,1) NOT NULL,
        [TCCID] INT NULL,
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
        , CONSTRAINT [PK_TempCoffeecheckupCustomerTbl] PRIMARY KEY CLUSTERED ([TempCoffeecheckupCustomerID])
    );
END
GO

IF OBJECT_ID(N'[TempCoffeecheckupItemsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TempCoffeecheckupItemsTbl]
    (
        [TempCoffeecheckupItemsID] INT IDENTITY(1,1) NOT NULL,
        [TCIID] INT NULL,
        [ContactID] INT NULL,
        [ItemID] INT NULL,
        [ItemQty] REAL NULL,
        [ItemPrepID] INT NULL,
        [ItemPackagingID] INT NULL,
        [AutoFulfill] BIT NULL,
        [NextDateRequired] DATETIME NULL,
        [RecurringOrderItemID] INT NULL
        , CONSTRAINT [PK_TempCoffeecheckupItemsTbl] PRIMARY KEY CLUSTERED ([TempCoffeecheckupItemsID])
    );
END
GO

IF OBJECT_ID(N'[TempOrdersHeaderTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TempOrdersHeaderTbl]
    (
        [TempOrdersHeaderID] INT IDENTITY(1,1) NOT NULL,
        [TOHeaderID] INT NULL,
        [ContactID] INT NULL,
        [OrderDate] DATETIME NULL,
        [RoastDate] DATETIME NULL,
        [RequiredByDate] DATETIME NULL,
        [ToBeDeliveredByID] INT NULL,
        [Confirmed] BIT NULL,
        [Done] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_TempOrdersHeaderTbl] PRIMARY KEY CLUSTERED ([TempOrdersHeaderID])
    );
END
GO

IF OBJECT_ID(N'[TempOrdersLinesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TempOrdersLinesTbl]
    (
        [TempOrdersLinesID] INT IDENTITY(1,1) NOT NULL,
        [TOLineID] INT NULL,
        [TOHeaderID] INT NULL,
        [ItemID] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [Qty] REAL NULL,
        [ItemPackagingID] INT NULL,
        [OriginalOrderID] INT NULL
        , CONSTRAINT [PK_TempOrdersLinesTbl] PRIMARY KEY CLUSTERED ([TempOrdersLinesID])
    );
END
GO

IF OBJECT_ID(N'[TempOrdersTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TempOrdersTbl]
    (
        [TempOrdersID] INT IDENTITY(1,1) NOT NULL,
        [TempOrderId] INT NULL,
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
        , CONSTRAINT [PK_TempOrdersTbl] PRIMARY KEY CLUSTERED ([TempOrdersID])
    );
END
GO

IF OBJECT_ID(N'[TotalCountTrackerTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TotalCountTrackerTbl]
    (
        [TotalCountTrackerID] INT IDENTITY(1,1) NOT NULL,
        [TotalCounterTrackerID] INT NULL,
        [CountDate] DATETIME NULL,
        [TotalCount] INT NULL,
        [Comments] NVARCHAR(255) NULL
        , CONSTRAINT [PK_TotalCountTrackerTbl] PRIMARY KEY CLUSTERED ([TotalCountTrackerID])
    );
END
GO

IF OBJECT_ID(N'[TrackedServiceItemsTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TrackedServiceItemsTbl]
    (
        [TrackedServiceItemID] INT IDENTITY(1,1) NOT NULL,
        [TrackerServiceItemID] INT NULL,
        [ItemServiceTypeID] INT NULL,
        [TypicalAvePerItem] REAL NULL,
        [UsageDateFieldName] NVARCHAR(20) NULL,
        [UsageAveFieldName] NVARCHAR(20) NULL,
        [ThisItemSetsDailyAverage] BIT NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_TrackedServiceItemsTbl] PRIMARY KEY CLUSTERED ([TrackedServiceItemID])
    );
END
GO

IF OBJECT_ID(N'[TransactionTypesTbl]', N'U') IS NULL
BEGIN
    CREATE TABLE [TransactionTypesTbl]
    (
        [TransactionTypesID] INT IDENTITY(1,1) NOT NULL,
        [TransactionID] INT NULL,
        [TransactionType] NVARCHAR(50) NULL,
        [Notes] NVARCHAR(MAX) NULL
        , CONSTRAINT [PK_TransactionTypesTbl] PRIMARY KEY CLUSTERED ([TransactionTypesID])
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

