-- Migration script for table: TempOrdersHeaderTbl
DELETE FROM [TempOrdersHeaderTbl];
SET IDENTITY_INSERT [TempOrdersHeaderTbl] ON;
INSERT INTO [TempOrdersHeaderTbl] (
[TOHeaderID], [ContactID], [OrderDate], [RoastDate], [RequiredByDate], [ToBeDeliveredByID], [Confirmed], [Done], [Notes]
)
SELECT
[TOHeaderID], [CustomerID], dbo.SafeDateConvert([OrderDate]) AS [OrderDate], dbo.SafeDateConvert([RoastDate]) AS [RoastDate], dbo.SafeDateConvert([RequiredByDate]) AS [RequiredByDate], [ToBeDeliveredByID], [Confirmed], [Done], [Notes]
FROM [AccessSrc].[TempOrdersHeaderTbl];
SET IDENTITY_INSERT [TempOrdersHeaderTbl] OFF;
