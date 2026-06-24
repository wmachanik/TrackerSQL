-- Migration script for table: UsedItemGroupsTbl
DELETE FROM [UsedItemGroupsTbl];
SET IDENTITY_INSERT [UsedItemGroupsTbl] ON;
INSERT INTO [UsedItemGroupsTbl] (
[UsedItemGroupID], [ContactID], [GroupReferenceItemID], [LastItemID], [LastItemSortPos], [LastItemDateChanged], [Notes]
)
SELECT
[UsedItemGroupID], [ContactID], [GroupItemTypeID], [LastItemTypeID], [LastItemTypeSortPos], dbo.SafeDateConvert([LastItemDateChanged]) AS [LastItemDateChanged], [Notes]
FROM [AccessSrc].[UsedItemGroupTbl];
SET IDENTITY_INSERT [UsedItemGroupsTbl] OFF;
