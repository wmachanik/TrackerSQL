-- Migration script for table: SectionTypesTbl
DELETE FROM [SectionTypesTbl];
SET IDENTITY_INSERT [SectionTypesTbl] ON;
INSERT INTO [SectionTypesTbl] (
[SectionID], [SectionType], [Notes]
)
SELECT
[SectionID], [SectionType], [Notes]
FROM [AccessSrc].[SectionTypesTbl];
SET IDENTITY_INSERT [SectionTypesTbl] OFF;
