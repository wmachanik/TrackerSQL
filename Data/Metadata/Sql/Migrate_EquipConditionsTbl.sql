-- Migration script for table: EquipConditionsTbl
DELETE FROM [EquipConditionsTbl];
SET IDENTITY_INSERT [EquipConditionsTbl] ON;
INSERT INTO [EquipConditionsTbl] (
[EquipConditionID], [ConditionDesc], [SortOrder], [Notes]
)
SELECT
[MachineConditionID], [ConditionDesc], [SortOrder], [Notes]
FROM [AccessSrc].[MachineConditionsTbl];
SET IDENTITY_INSERT [EquipConditionsTbl] OFF;
