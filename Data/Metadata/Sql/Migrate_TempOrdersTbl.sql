-- Migration script for table: TempOrdersTbl
DELETE FROM [TempOrdersTbl];
SET IDENTITY_INSERT [TempOrdersTbl] ON;
INSERT INTO [TempOrdersTbl] (
[TempOrderID], [OrderID], [ContactID], [OrderDate], [RoastDate], [ItemID], [ItemServiceTypeID], [ItemPrepTypeID], [ItemPackagingID], [QtyOrdered], [RequiredByDate], [Delivered], [Notes]
)
SELECT
[TempOrderId], [OrderID], [CustomerId], dbo.SafeDateConvert([OrderDate]) AS [OrderDate], dbo.SafeDateConvert([RoastDate]) AS [RoastDate], [ItemTypeID], [ServiceTypeId], [PrepTypeID], [PackagingId], [QuantityOrdered], dbo.SafeDateConvert([RequiredByDate]) AS [RequiredByDate], [Delivered], [Notes]
FROM [AccessSrc].[TempOrdersTbl];
SET IDENTITY_INSERT [TempOrdersTbl] OFF;
