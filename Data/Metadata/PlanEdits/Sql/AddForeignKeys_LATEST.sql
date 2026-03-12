-- Emitted FKs: 63, Skipped: 6
-- Auto-generated FK constraints script
-- FKs (total=69)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_AreaPrepDaysTbl_AreaID' AND parent_object_id = OBJECT_ID(N'[AreaPrepDaysTbl]'))
BEGIN
    ALTER TABLE [AreaPrepDaysTbl] WITH CHECK ADD CONSTRAINT [FK_AreaPrepDaysTbl_AreaID] FOREIGN KEY([AreaID]) REFERENCES [AreasTbl]([AreaID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsAwayPeriodTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[ContactsAwayPeriodTbl]'))
BEGIN
    ALTER TABLE [ContactsAwayPeriodTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsAwayPeriodTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsItemSvcSummaryTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[ContactsItemSvcSummaryTbl]'))
BEGIN
    ALTER TABLE [ContactsItemSvcSummaryTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsItemSvcSummaryTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsItemSvcSummaryTbl_ItemServiceTypeID' AND parent_object_id = OBJECT_ID(N'[ContactsItemSvcSummaryTbl]'))
BEGIN
    ALTER TABLE [ContactsItemSvcSummaryTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsItemSvcSummaryTbl_ItemServiceTypeID] FOREIGN KEY([ItemServiceTypeID]) REFERENCES [ItemServiceTypesTbl]([ItemServiceTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsItemsPredictedTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[ContactsItemsPredictedTbl]'))
BEGIN
    ALTER TABLE [ContactsItemsPredictedTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsItemsPredictedTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsAccInfoTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[ContactsAccInfoTbl]'))
BEGIN
    ALTER TABLE [ContactsAccInfoTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsAccInfoTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsAccInfoTbl_PaymentTermID' AND parent_object_id = OBJECT_ID(N'[ContactsAccInfoTbl]'))
BEGIN
    ALTER TABLE [ContactsAccInfoTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsAccInfoTbl_PaymentTermID] FOREIGN KEY([PaymentTermID]) REFERENCES [PaymentTermsTbl]([PaymentTermID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsAccInfoTbl_PriceLevelID' AND parent_object_id = OBJECT_ID(N'[ContactsAccInfoTbl]'))
BEGIN
    ALTER TABLE [ContactsAccInfoTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsAccInfoTbl_PriceLevelID] FOREIGN KEY([PriceLevelID]) REFERENCES [PriceLevelsTbl]([PriceLevelID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsAccInfoTbl_InvoiceTypeID' AND parent_object_id = OBJECT_ID(N'[ContactsAccInfoTbl]'))
BEGIN
    ALTER TABLE [ContactsAccInfoTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsAccInfoTbl_InvoiceTypeID] FOREIGN KEY([InvoiceTypeID]) REFERENCES [InvoiceTypesTbl]([InvoiceTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsTbl_ContactTypeID' AND parent_object_id = OBJECT_ID(N'[ContactsTbl]'))
BEGIN
    ALTER TABLE [ContactsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsTbl_ContactTypeID] FOREIGN KEY([ContactTypeID]) REFERENCES [ContactTypesTbl]([ContactTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsTbl_EquipTypeID' AND parent_object_id = OBJECT_ID(N'[ContactsTbl]'))
BEGIN
    ALTER TABLE [ContactsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsTbl_EquipTypeID] FOREIGN KEY([EquipTypeID]) REFERENCES [EquipTypesTbl]([EquipTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsTbl_ItemPrefID' AND parent_object_id = OBJECT_ID(N'[ContactsTbl]'))
BEGIN
    ALTER TABLE [ContactsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsTbl_ItemPrefID] FOREIGN KEY([ItemPrefID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsTbl_PrefItemPrepTypeID' AND parent_object_id = OBJECT_ID(N'[ContactsTbl]'))
BEGIN
    ALTER TABLE [ContactsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsTbl_PrefItemPrepTypeID] FOREIGN KEY([PrefItemPrepTypeID]) REFERENCES [ItemPrepTypesTbl]([ItemPrepID]);
END
GO

-- Skipped: referenced table not in model for [ContactsTbl].[PrefItemPackagingID] -> [ItemPackagingTbl]
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsTbl_SecondaryItemPrefID' AND parent_object_id = OBJECT_ID(N'[ContactsTbl]'))
BEGIN
    ALTER TABLE [ContactsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsTbl_SecondaryItemPrefID] FOREIGN KEY([SecondaryItemPrefID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsTbl_PreferedAgentID' AND parent_object_id = OBJECT_ID(N'[ContactsTbl]'))
BEGIN
    ALTER TABLE [ContactsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsTbl_PreferedAgentID] FOREIGN KEY([PreferedAgentID]) REFERENCES [PeopleTbl]([PersonID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsTbl_SalesAgentID' AND parent_object_id = OBJECT_ID(N'[ContactsTbl]'))
BEGIN
    ALTER TABLE [ContactsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsTbl_SalesAgentID] FOREIGN KEY([SalesAgentID]) REFERENCES [PeopleTbl]([PersonID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactTrackedServiceItemsTbl_ContactTypeID' AND parent_object_id = OBJECT_ID(N'[ContactTrackedServiceItemsTbl]'))
BEGIN
    ALTER TABLE [ContactTrackedServiceItemsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactTrackedServiceItemsTbl_ContactTypeID] FOREIGN KEY([ContactTypeID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactTrackedServiceItemsTbl_ItemServiceTypeID' AND parent_object_id = OBJECT_ID(N'[ContactTrackedServiceItemsTbl]'))
BEGIN
    ALTER TABLE [ContactTrackedServiceItemsTbl] WITH CHECK ADD CONSTRAINT [FK_ContactTrackedServiceItemsTbl_ItemServiceTypeID] FOREIGN KEY([ItemServiceTypeID]) REFERENCES [ItemServiceTypesTbl]([ItemServiceTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ItemGroupsTbl_GroupReferenceItemID' AND parent_object_id = OBJECT_ID(N'[ItemGroupsTbl]'))
BEGIN
    ALTER TABLE [ItemGroupsTbl] WITH CHECK ADD CONSTRAINT [FK_ItemGroupsTbl_GroupReferenceItemID] FOREIGN KEY([GroupReferenceItemID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ItemGroupsTbl_ItemID' AND parent_object_id = OBJECT_ID(N'[ItemGroupsTbl]'))
BEGIN
    ALTER TABLE [ItemGroupsTbl] WITH CHECK ADD CONSTRAINT [FK_ItemGroupsTbl_ItemID] FOREIGN KEY([ItemID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ItemsTbl_ItemServiceTypeID' AND parent_object_id = OBJECT_ID(N'[ItemsTbl]'))
BEGIN
    ALTER TABLE [ItemsTbl] WITH CHECK ADD CONSTRAINT [FK_ItemsTbl_ItemServiceTypeID] FOREIGN KEY([ItemServiceTypeID]) REFERENCES [ItemServiceTypesTbl]([ItemServiceTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ItemsTbl_ReplacementItemID' AND parent_object_id = OBJECT_ID(N'[ItemsTbl]'))
BEGIN
    ALTER TABLE [ItemsTbl] WITH CHECK ADD CONSTRAINT [FK_ItemsTbl_ReplacementItemID] FOREIGN KEY([ReplacementItemID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ItemsTbl_ItemUnitID' AND parent_object_id = OBJECT_ID(N'[ItemsTbl]'))
BEGIN
    ALTER TABLE [ItemsTbl] WITH CHECK ADD CONSTRAINT [FK_ItemsTbl_ItemUnitID] FOREIGN KEY([ItemUnitID]) REFERENCES [ItemUnitsTbl]([ItemUnitID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsItemUsageTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[ContactsItemUsageTbl]'))
BEGIN
    ALTER TABLE [ContactsItemUsageTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsItemUsageTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsItemUsageTbl_ItemProvidedID' AND parent_object_id = OBJECT_ID(N'[ContactsItemUsageTbl]'))
BEGIN
    ALTER TABLE [ContactsItemUsageTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsItemUsageTbl_ItemProvidedID] FOREIGN KEY([ItemProvidedID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsItemUsageTbl_ItemPrepTypeID' AND parent_object_id = OBJECT_ID(N'[ContactsItemUsageTbl]'))
BEGIN
    ALTER TABLE [ContactsItemUsageTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsItemUsageTbl_ItemPrepTypeID] FOREIGN KEY([ItemPrepTypeID]) REFERENCES [ItemPrepTypesTbl]([ItemPrepID]);
END
GO

-- Skipped: referenced table not in model for [ContactsItemUsageTbl].[ItemPackagingID] -> [ItemPackagingTbl]
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_NextPrepDateByAreasTbl_AreaID' AND parent_object_id = OBJECT_ID(N'[NextPrepDateByAreasTbl]'))
BEGIN
    ALTER TABLE [NextPrepDateByAreasTbl] WITH CHECK ADD CONSTRAINT [FK_NextPrepDateByAreasTbl_AreaID] FOREIGN KEY([AreaID]) REFERENCES [AreasTbl]([AreaID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RepairsTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[RepairsTbl]'))
BEGIN
    ALTER TABLE [RepairsTbl] WITH CHECK ADD CONSTRAINT [FK_RepairsTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RepairsTbl_EquipTypeID' AND parent_object_id = OBJECT_ID(N'[RepairsTbl]'))
BEGIN
    ALTER TABLE [RepairsTbl] WITH CHECK ADD CONSTRAINT [FK_RepairsTbl_EquipTypeID] FOREIGN KEY([EquipTypeID]) REFERENCES [EquipTypesTbl]([EquipTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RepairsTbl_EquipConditionID' AND parent_object_id = OBJECT_ID(N'[RepairsTbl]'))
BEGIN
    ALTER TABLE [RepairsTbl] WITH CHECK ADD CONSTRAINT [FK_RepairsTbl_EquipConditionID] FOREIGN KEY([EquipConditionID]) REFERENCES [EquipConditionsTbl]([EquipConditionID]);
END
GO

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

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RepairsTbl_RelatedOrderID' AND parent_object_id = OBJECT_ID(N'[RepairsTbl]'))
BEGIN
    ALTER TABLE [RepairsTbl] WITH CHECK ADD CONSTRAINT [FK_RepairsTbl_RelatedOrderID] FOREIGN KEY([RelatedOrderID]) REFERENCES [OrdersTbl]([OrderID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SentRemindersLogTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[SentRemindersLogTbl]'))
BEGIN
    ALTER TABLE [SentRemindersLogTbl] WITH CHECK ADD CONSTRAINT [FK_SentRemindersLogTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

-- Skipped: referenced table not in model for [ItemServiceTypesTbl].[ItemPackagingID] -> [ItemPackagingTbl]
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ItemServiceTypesTbl_ItemPrepTypeID' AND parent_object_id = OBJECT_ID(N'[ItemServiceTypesTbl]'))
BEGIN
    ALTER TABLE [ItemServiceTypesTbl] WITH CHECK ADD CONSTRAINT [FK_ItemServiceTypesTbl_ItemPrepTypeID] FOREIGN KEY([ItemPrepTypeID]) REFERENCES [ItemPrepTypesTbl]([ItemPrepID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SysDataTbl_GroupReferenceItemID' AND parent_object_id = OBJECT_ID(N'[SysDataTbl]'))
BEGIN
    ALTER TABLE [SysDataTbl] WITH CHECK ADD CONSTRAINT [FK_SysDataTbl_GroupReferenceItemID] FOREIGN KEY([GroupReferenceItemID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupCustomerTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupCustomerTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupCustomerTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupCustomerTbl_AreaID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupCustomerTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupCustomerTbl_AreaID] FOREIGN KEY([AreaID]) REFERENCES [AreasTbl]([AreaID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupCustomerTbl_ContactTypeID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupCustomerTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupCustomerTbl_ContactTypeID] FOREIGN KEY([ContactTypeID]) REFERENCES [ContactTypesTbl]([ContactTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupCustomerTbl_EquipTypeID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupCustomerTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupCustomerTbl_EquipTypeID] FOREIGN KEY([EquipTypeID]) REFERENCES [EquipTypesTbl]([EquipTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupCustomerTbl_PreferedAgentID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupCustomerTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupCustomerTbl_PreferedAgentID] FOREIGN KEY([PreferedAgentID]) REFERENCES [PeopleTbl]([PersonID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupCustomerTbl_SalesAgentID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupCustomerTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupCustomerTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupCustomerTbl_SalesAgentID] FOREIGN KEY([SalesAgentID]) REFERENCES [PeopleTbl]([PersonID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupItemsTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupItemsTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupItemsTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupItemsTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupItemsTbl_ItemID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupItemsTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupItemsTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupItemsTbl_ItemID] FOREIGN KEY([ItemID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupItemsTbl_ItemPrepID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupItemsTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupItemsTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupItemsTbl_ItemPrepID] FOREIGN KEY([ItemPrepID]) REFERENCES [ItemPrepTypesTbl]([ItemPrepID]);
END
GO

-- Skipped: referenced table not in model for [TempCoffeecheckupItemsTbl].[ItemPackagingID] -> [ItemPackagingTbl]
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempCoffeecheckupItemsTbl_RecurringOrderItemID' AND parent_object_id = OBJECT_ID(N'[TempCoffeecheckupItemsTbl]'))
BEGIN
    ALTER TABLE [TempCoffeecheckupItemsTbl] WITH CHECK ADD CONSTRAINT [FK_TempCoffeecheckupItemsTbl_RecurringOrderItemID] FOREIGN KEY([RecurringOrderItemID]) REFERENCES [RecurringOrderItemsTbl]([RecurringOrderItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersHeaderTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[TempOrdersHeaderTbl]'))
BEGIN
    ALTER TABLE [TempOrdersHeaderTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersHeaderTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersHeaderTbl_ToBeDeliveredByID' AND parent_object_id = OBJECT_ID(N'[TempOrdersHeaderTbl]'))
BEGIN
    ALTER TABLE [TempOrdersHeaderTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersHeaderTbl_ToBeDeliveredByID] FOREIGN KEY([ToBeDeliveredByID]) REFERENCES [PeopleTbl]([PersonID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersLinesTbl_TOHeaderID' AND parent_object_id = OBJECT_ID(N'[TempOrdersLinesTbl]'))
BEGIN
    ALTER TABLE [TempOrdersLinesTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersLinesTbl_TOHeaderID] FOREIGN KEY([TOHeaderID]) REFERENCES [TempOrdersHeaderTbl]([TOHeaderID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersLinesTbl_ItemID' AND parent_object_id = OBJECT_ID(N'[TempOrdersLinesTbl]'))
BEGIN
    ALTER TABLE [TempOrdersLinesTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersLinesTbl_ItemID] FOREIGN KEY([ItemID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersLinesTbl_ItemServiceTypeID' AND parent_object_id = OBJECT_ID(N'[TempOrdersLinesTbl]'))
BEGIN
    ALTER TABLE [TempOrdersLinesTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersLinesTbl_ItemServiceTypeID] FOREIGN KEY([ItemServiceTypeID]) REFERENCES [ItemServiceTypesTbl]([ItemServiceTypeID]);
END
GO

-- Skipped: referenced table not in model for [TempOrdersLinesTbl].[ItemPackagingID] -> [ItemPackagingTbl]
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersLinesTbl_OriginalOrderID' AND parent_object_id = OBJECT_ID(N'[TempOrdersLinesTbl]'))
BEGIN
    ALTER TABLE [TempOrdersLinesTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersLinesTbl_OriginalOrderID] FOREIGN KEY([OriginalOrderID]) REFERENCES [OrdersTbl]([OrderID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersTbl_OrderID' AND parent_object_id = OBJECT_ID(N'[TempOrdersTbl]'))
BEGIN
    ALTER TABLE [TempOrdersTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersTbl_OrderID] FOREIGN KEY([OrderID]) REFERENCES [OrdersTbl]([OrderID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[TempOrdersTbl]'))
BEGIN
    ALTER TABLE [TempOrdersTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersTbl_ItemID' AND parent_object_id = OBJECT_ID(N'[TempOrdersTbl]'))
BEGIN
    ALTER TABLE [TempOrdersTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersTbl_ItemID] FOREIGN KEY([ItemID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersTbl_ItemServiceTypeID' AND parent_object_id = OBJECT_ID(N'[TempOrdersTbl]'))
BEGIN
    ALTER TABLE [TempOrdersTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersTbl_ItemServiceTypeID] FOREIGN KEY([ItemServiceTypeID]) REFERENCES [ItemServiceTypesTbl]([ItemServiceTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TempOrdersTbl_ItemPrepTypeID' AND parent_object_id = OBJECT_ID(N'[TempOrdersTbl]'))
BEGIN
    ALTER TABLE [TempOrdersTbl] WITH CHECK ADD CONSTRAINT [FK_TempOrdersTbl_ItemPrepTypeID] FOREIGN KEY([ItemPrepTypeID]) REFERENCES [ItemPrepTypesTbl]([ItemPrepID]);
END
GO

-- Skipped: referenced table not in model for [TempOrdersTbl].[ItemPackagingID] -> [ItemPackagingTbl]
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrackedServiceItemsTbl_ItemServiceTypeID' AND parent_object_id = OBJECT_ID(N'[TrackedServiceItemsTbl]'))
BEGIN
    ALTER TABLE [TrackedServiceItemsTbl] WITH CHECK ADD CONSTRAINT [FK_TrackedServiceItemsTbl_ItemServiceTypeID] FOREIGN KEY([ItemServiceTypeID]) REFERENCES [ItemServiceTypesTbl]([ItemServiceTypeID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UsedItemGroupsTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[UsedItemGroupsTbl]'))
BEGIN
    ALTER TABLE [UsedItemGroupsTbl] WITH CHECK ADD CONSTRAINT [FK_UsedItemGroupsTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UsedItemGroupsTbl_GroupReferenceItemID' AND parent_object_id = OBJECT_ID(N'[UsedItemGroupsTbl]'))
BEGIN
    ALTER TABLE [UsedItemGroupsTbl] WITH CHECK ADD CONSTRAINT [FK_UsedItemGroupsTbl_GroupReferenceItemID] FOREIGN KEY([GroupReferenceItemID]) REFERENCES [ItemGroupsTbl]([ItemGroupID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_UsedItemGroupsTbl_LastItemID' AND parent_object_id = OBJECT_ID(N'[UsedItemGroupsTbl]'))
BEGIN
    ALTER TABLE [UsedItemGroupsTbl] WITH CHECK ADD CONSTRAINT [FK_UsedItemGroupsTbl_LastItemID] FOREIGN KEY([LastItemID]) REFERENCES [ItemsTbl]([ItemID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_OrderLinesTbl_OrderID' AND parent_object_id = OBJECT_ID(N'[OrderLinesTbl]'))
BEGIN
    ALTER TABLE [OrderLinesTbl] WITH CHECK ADD CONSTRAINT [FK_OrderLinesTbl_OrderID] FOREIGN KEY([OrderID]) REFERENCES [OrdersTbl]([OrderID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RecurringOrderItemsTbl_RecurringOrderID' AND parent_object_id = OBJECT_ID(N'[RecurringOrderItemsTbl]'))
BEGIN
    ALTER TABLE [RecurringOrderItemsTbl] WITH CHECK ADD CONSTRAINT [FK_RecurringOrderItemsTbl_RecurringOrderID] FOREIGN KEY([RecurringOrderID]) REFERENCES [RecurringOrdersTbl]([RecurringOrderID]);
END
GO

