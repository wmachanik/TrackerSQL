-- Migration script for table: ItemPrepTypesTbl
DELETE FROM [ItemPrepTypesTbl];
SET IDENTITY_INSERT [ItemPrepTypesTbl] ON;
INSERT INTO [ItemPrepTypesTbl] (
[ItemPrepID], [ItemPrepType], [IdentifyingChar]
)
SELECT
[PrepID], [PrepType], [IdentifyingChar]
FROM [AccessSrc].[PrepTypesTbl];
SET IDENTITY_INSERT [ItemPrepTypesTbl] OFF;
