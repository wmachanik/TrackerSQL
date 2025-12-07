-- Emitted FKs: 3, Skipped: 73
-- Auto-generated FK constraints script
-- FKs (total=76)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

-- Skipped FK for table not in model: [AreaPrepDaysTbl].[AreaID] -> [AreasTbl]
-- Skipped FK for table not in model: [ContactsAwayPeriodTbl].[ContactID] -> [ContactsTbl]
-- Skipped FK for table not in model: [ContactUsageLinesTbl].[ContactID] -> [ContactsTbl]
-- Skipped FK for table not in model: [ContactUsageLinesTbl].[ItemServiceTypeID] -> [ItemServiceTypesTbl]
-- Skipped FK for table not in model: [ContactsAccInfoTbl].[ContactID] -> [ContactsTbl]
-- Skipped FK for table not in model: [ContactsAccInfoTbl].[PaymentTermID] -> [PaymentTermsTbl]
-- Skipped FK for table not in model: [ContactsAccInfoTbl].[PriceLevelID] -> [PriceLevelsTbl]
-- Skipped FK for table not in model: [ContactsAccInfoTbl].[InvoiceTypeID] -> [InvoiceTypesTbl]
-- Skipped FK for table not in model: [ContactsTbl].[ContactTypeID] -> [ContactTypesTbl]
-- Skipped FK for table not in model: [ContactsTbl].[EquipTypeID] -> [EquipTypesTbl]
-- Skipped FK for table not in model: [ContactsTbl].[ItemPrefID] -> [ItemsTbl]
-- Skipped FK for table not in model: [ContactsTbl].[PrefItemPrepTypeID] -> [ItemPrepTypesTbl]
-- Skipped FK for table not in model: [ContactsTbl].[PrefItemPackagingID] -> [ItemPackagingTbl]
-- Skipped FK for table not in model: [ContactsTbl].[SecondaryItemPrefID] -> [ItemsTbl]
-- Skipped FK for table not in model: [ContactsTbl].[PreferedAgentID] -> [PeopleTbl]
-- Skipped FK for table not in model: [ContactsTbl].[SalesAgentID] -> [PeopleTbl]
-- Skipped FK for table not in model: [ContactTrackedServiceItemsTbl].[ContactTypeID] -> [ContactsTbl]
-- Skipped FK for table not in model: [ContactTrackedServiceItemsTbl].[ItemServiceTypeID] -> [ItemServiceTypesTbl]
-- Skipped FK for table not in model: [ItemGroupsTbl].[GroupReferenceItemID] -> [ItemsTbl]
-- Skipped FK for table not in model: [ItemGroupsTbl].[ItemID] -> [ItemsTbl]
-- Skipped FK for table not in model: [ItemsTbl].[ItemServiceTypeID] -> [ItemServiceTypesTbl]
-- Skipped FK for table not in model: [ItemsTbl].[ReplacementItemID] -> [ItemsTbl]
-- Skipped FK for table not in model: [ItemsTbl].[ItemUnitID] -> [ItemUnitsTbl]
-- Skipped FK for table not in model: [ContactsItemUsageTbl].[ContactID] -> [ContactsTbl]
-- Skipped FK for table not in model: [ContactsItemUsageTbl].[ItemProvidedID] -> [ItemsTbl]
-- Skipped FK for table not in model: [ContactsItemUsageTbl].[ItemPrepTypeID] -> [ItemPrepTypesTbl]
-- Skipped FK for table not in model: [ContactsItemUsageTbl].[ItemPackagingID] -> [ItemPackagingTbl]
-- Skipped FK for table not in model: [NextPrepDateByAreasTbl].[AreaID] -> [AreasTbl]
-- Skipped: referenced table not in model for [RepairsTbl].[ContactID] -> [ContactsTbl]
-- Skipped: referenced table not in model for [RepairsTbl].[EquipTypeID] -> [EquipTypesTbl]
-- Skipped: referenced table not in model for [RepairsTbl].[EquipConditionID] -> [EquipConditionsTbl]
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RepairsTbl_RepairFaultID' AND parent_object_id = OBJECT_ID(N'[RepairsTbl]'))
BEGIN
    ALTER TABLE [RepairsTbl] WITH CHECK ADD CONSTRAINT [FK_RepairsTbl_RepairFaultID] FOREIGN KEY([RepairFaultID]) REFERENCES [RepairFaultsTbl]([RepairFaultID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RepairsTbl_RepairStatusID' AND parent_object_id = OBJECT_ID(N'[RepairsTbl]'))
BEGIN
    ALTER TABLE [RepairsTbl] WITH CHECK ADD CONSTRAINT [FK_RepairsTbl_RepairStatusID] FOREIGN KEY([RepairStatusID]) REFERENCES [RepairStatusesTbl]([RepairStatusID]);
END
GO

-- Skipped: referenced table not in model for [RepairsTbl].[RelatedOrderID] -> [OrdersTbl]
-- Skipped: referenced table not in model for [SentRemindersLogTbl].[ContactID] -> [ContactsTbl]
-- Skipped FK for table not in model: [ItemServiceTypesTbl].[ItemPackagingID] -> [ItemPackagingTbl]
-- Skipped FK for table not in model: [ItemServiceTypesTbl].[ItemPrepTypeID] -> [ItemPrepTypesTbl]
-- Skipped: referenced table not in model for [SysDataTbl].[GroupReferenceItemID] -> [ItemsTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupCustomerTbl].[ContactID] -> [ContactsTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupCustomerTbl].[AreaID] -> [AreasTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupCustomerTbl].[ContactTypeID] -> [ContactTypesTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupCustomerTbl].[EquipTypeID] -> [EquipTypesTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupCustomerTbl].[PreferedAgentID] -> [PeopleTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupCustomerTbl].[SalesAgentID] -> [PeopleTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupItemsTbl].[ContactID] -> [ContactsTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupItemsTbl].[ItemID] -> [ItemsTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupItemsTbl].[ItemPrepID] -> [ItemPrepTypesTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupItemsTbl].[ItemPackagingID] -> [ItemPackagingTbl]
-- Skipped: referenced table not in model for [TempCoffeecheckupItemsTbl].[RecurringOrderItemID] -> [RecurringOrderItemsTbl]
-- Skipped: referenced table not in model for [TempOrdersHeaderTbl].[ContactID] -> [ContactsTbl]
-- Skipped: referenced table not in model for [TempOrdersHeaderTbl].[ToBeDeliveredByID] -> [PeopleTbl]
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersLinesTbl_TOHeaderID' AND parent_object_id = OBJECT_ID(N'[TempOrdersLinesTbl]'))
BEGIN
    ALTER TABLE [TempOrdersLinesTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersLinesTbl_TOHeaderID] FOREIGN KEY([TOHeaderID]) REFERENCES [TempOrdersHeaderTbl]([TOHeaderID]);
END
GO

-- Skipped: referenced table not in model for [TempOrdersLinesTbl].[ItemID] -> [ItemsTbl]
-- Skipped: referenced table not in model for [TempOrdersLinesTbl].[ItemServiceTypeID] -> [ItemServiceTypesTbl]
-- Skipped: referenced table not in model for [TempOrdersLinesTbl].[ItemPackagingID] -> [ItemPackagingTbl]
-- Skipped: referenced table not in model for [TempOrdersLinesTbl].[OriginalOrderID] -> [OrdersTbl]
-- Skipped: referenced table not in model for [TempOrdersTbl].[OrderID] -> [OrdersTbl]
-- Skipped: referenced table not in model for [TempOrdersTbl].[ContactID] -> [ContactsTbl]
-- Skipped: referenced table not in model for [TempOrdersTbl].[ItemID] -> [ItemsTbl]
-- Skipped: referenced table not in model for [TempOrdersTbl].[ItemServiceTypeID] -> [ItemServiceTypesTbl]
-- Skipped: referenced table not in model for [TempOrdersTbl].[ItemPrepTypeID] -> [ItemPrepTypesTbl]
-- Skipped: referenced table not in model for [TempOrdersTbl].[ItemPackagingID] -> [ItemPackagingTbl]
-- Skipped FK for table not in model: [TrackedServiceItemsTbl].[ItemServiceTypeID] -> [ItemServiceTypesTbl]
-- Skipped FK for table not in model: [UsedItemGroupsTbl].[ContactID] -> [ContactsTbl]
-- Skipped FK for table not in model: [UsedItemGroupsTbl].[GroupReferenceItemID] -> [ItemGroupsTbl]
-- Skipped FK for table not in model: [UsedItemGroupsTbl].[LastItemID] -> [ItemsTbl]
-- Skipped FK for table not in model: [OrdersTbl].[ContactID] -> [ContactsTbl]
-- Skipped FK for table not in model: [OrdersTbl].[ToBeDeliveredByID] -> [PeopleTbl]
-- Skipped FK for table not in model: [OrderLinesTbl].[OrderID] -> [OrdersTbl]
-- Skipped FK for table not in model: [OrderLinesTbl].[ItemID] -> [ItemsTbl]
-- Skipped FK for table not in model: [OrderLinesTbl].[PrepTypeID] -> [ItemPrepTypesTbl]
-- Skipped FK for table not in model: [OrderLinesTbl].[PackagingID] -> [ItemPackagingTbl]
-- Skipped FK for table not in model: [RecurringOrderItemsTbl].[RecurringOrderID] -> [RecurringOrdersTbl]
-- Skipped FK for table not in model: [RecurringOrderItemsTbl].[RecurringTypeID] -> [RecurranceTypesTbl]
-- Skipped FK for table not in model: [RecurringOrderItemsTbl].[ItemRequiredID] -> [ItemsTbl]
-- Skipped FK for table not in model: [RecurringOrderItemsTbl].[ItemPackagingID] -> [ItemPackagingTbl]
