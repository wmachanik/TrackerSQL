-- Migration script for table: ItemsTbl
DELETE FROM [ItemsTbl];
SET IDENTITY_INSERT [ItemsTbl] ON;
INSERT INTO [ItemsTbl] (
[ItemID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ItemServiceTypeID], [ReplacementItemID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]
)
SELECT
[ItemTypeID], [SKU], [ItemDesc], [ItemEnabled], [ItemsCharacteritics], [ItemDetail], [ServiceTypeId], [ReplacementID], [ItemShortName], [SortOrder], [UnitsPerQty], [ItemUnitID], [BasePrice]
FROM [AccessSrc].[ItemTypeTbl];
SET IDENTITY_INSERT [ItemsTbl] OFF;
