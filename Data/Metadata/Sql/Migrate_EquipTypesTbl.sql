-- Migration script for table: EquipTypesTbl
DELETE FROM [EquipTypesTbl];
SET IDENTITY_INSERT [EquipTypesTbl] ON;
INSERT INTO [EquipTypesTbl] (
[EquipTypeID], [EquipTypeName], [EquipTypeDesc]
)
SELECT
[EquipTypeId], [EquipTypeName], [EquipTypeDesc]
FROM [AccessSrc].[EquipTypeTbl];
SET IDENTITY_INSERT [EquipTypesTbl] OFF;
