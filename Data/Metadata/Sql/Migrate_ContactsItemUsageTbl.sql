-- Migration script for table: ContactsItemUsageTbl
DELETE FROM [ContactsItemUsageTbl];
SET IDENTITY_INSERT [ContactsItemUsageTbl] ON;
INSERT INTO [ContactsItemUsageTbl] (
[ContactItemUsageLineNo], [ContactID], [DeliveryDate], [ItemProvidedID], [QtyProvided], [ItemPrepTypeID], [ItemPackagingID], [Notes]
)
SELECT
[ClientUsageLineNo], [CustomerID], dbo.SafeDateConvert([Date]) AS [DeliveryDate], [ItemProvided], [AmountProvided], [PrepTypeID], [PackagingID], [Notes]
FROM [AccessSrc].[ItemUsageTbl];
SET IDENTITY_INSERT [ContactsItemUsageTbl] OFF;
