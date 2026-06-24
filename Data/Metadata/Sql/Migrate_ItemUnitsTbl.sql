-- Migration script for table: ItemUnitsTbl
DELETE FROM [ItemUnitsTbl];
SET IDENTITY_INSERT [ItemUnitsTbl] ON;
INSERT INTO [ItemUnitsTbl] (
[ItemUnitID], [UnitOfMeasure], [UnitDescription]
)
SELECT
[ItemUnitID], [UnitOfMeasure], [UnitDescription]
FROM [AccessSrc].[ItemUnitsTbl];
SET IDENTITY_INSERT [ItemUnitsTbl] OFF;
