-- Migration script for table: RepairStatusesTbl
DELETE FROM [RepairStatusesTbl];
SET IDENTITY_INSERT [RepairStatusesTbl] ON;
INSERT INTO [RepairStatusesTbl] (
[RepairStatusID], [RepairStatusDesc], [EmailContact], [SortOrder], [Notes], [StatusNote]
)
SELECT
[RepairStatusID], [RepairStatusDesc], [EmailClient], [SortOrder], [Notes], [StatusNote]
FROM [AccessSrc].[RepairStatusesTbl];
SET IDENTITY_INSERT [RepairStatusesTbl] OFF;
