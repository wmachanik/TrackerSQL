-- Emitted FKs: 6, Skipped: 0
-- Auto-generated FK constraints script
-- FKs (total=6)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactUsageLinesTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[ContactUsageLinesTbl]'))
BEGIN
    ALTER TABLE [ContactUsageLinesTbl] WITH CHECK ADD CONSTRAINT [FK_ContactUsageLinesTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsUsageTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[ContactsUsageTbl]'))
BEGIN
    ALTER TABLE [ContactsUsageTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsUsageTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ContactsItemUsageTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[ContactsItemUsageTbl]'))
BEGIN
    ALTER TABLE [ContactsItemUsageTbl] WITH CHECK ADD CONSTRAINT [FK_ContactsItemUsageTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_RepairsTbl_ContactID' AND parent_object_id = OBJECT_ID(N'[RepairsTbl]'))
BEGIN
    ALTER TABLE [RepairsTbl] WITH CHECK ADD CONSTRAINT [FK_RepairsTbl_ContactID] FOREIGN KEY([ContactID]) REFERENCES [ContactsTbl]([ContactID]);
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

