-- Emitted FKs: 2, Skipped: 0
-- Auto-generated FK constraints script
-- FKs (total=2)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

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

