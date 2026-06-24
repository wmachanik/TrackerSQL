-- Migration script for table: TempOrdersLinesTbl
DELETE FROM [TempOrdersLinesTbl];
SET IDENTITY_INSERT [TempOrdersLinesTbl] ON;
INSERT INTO [TempOrdersLinesTbl] (
[TOLineID], [TOHeaderID], [ItemID], [ItemServiceTypeID], [Qty], [ItemPackagingID], [OriginalOrderID]
)
SELECT
[TOLineID], [TOHeaderID], [ItemID], [ServiceTypeID], [Qty], [PackagingID], [OriginalOrderID]
FROM [AccessSrc].[TempOrdersLinesTbl];
SET IDENTITY_INSERT [TempOrdersLinesTbl] OFF;
