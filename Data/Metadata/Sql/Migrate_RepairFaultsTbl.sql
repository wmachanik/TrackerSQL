-- Migration script for table: RepairFaultsTbl
DELETE FROM [RepairFaultsTbl];
SET IDENTITY_INSERT [RepairFaultsTbl] ON;
INSERT INTO [RepairFaultsTbl] (
[RepairFaultID], [RepairFaultDesc], [SortOrder], [Notes]
)
SELECT
[RepairFaultID], [RepairFaultDesc], [SortOrder], [Notes]
FROM [AccessSrc].[RepairFaultsTbl];
SET IDENTITY_INSERT [RepairFaultsTbl] OFF;
