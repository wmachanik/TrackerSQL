-- Migration script for table: ItemPackagingsTbl
DELETE FROM [ItemPackagingsTbl];
SET IDENTITY_INSERT [ItemPackagingsTbl] ON;
INSERT INTO [ItemPackagingsTbl] (
[ItemPackagingID], [ItemPrepDescription], [AdditionalNotes], [Symbol], [Colour], [BGColour]
)
SELECT
[PackagingID], [Description], [AdditionalNotes], [Symbol], [Colour], [BGColour]
FROM [AccessSrc].[PackagingTbl];
SET IDENTITY_INSERT [ItemPackagingsTbl] OFF;
