-- Migration script for table: ContactsItemSvcSummaryTbl
DELETE FROM [ContactsItemSvcSummaryTbl];
SET IDENTITY_INSERT [ContactsItemSvcSummaryTbl] ON;
INSERT INTO [ContactsItemSvcSummaryTbl] (
[ContactsItemSvcSummaryId], [ContactID], [UsageDate], [CupCount], [ItemServiceTypeID], [Qty], [Notes]
)
SELECT
[ClientUsageLineNo], [CustomerID], dbo.SafeDateConvert([Date]) AS [UsageDate], [CupCount], [ServiceTypeId], [Qty], [Notes]
FROM [AccessSrc].[ClientUsageLinesTbl];
SET IDENTITY_INSERT [ContactsItemSvcSummaryTbl] OFF;
