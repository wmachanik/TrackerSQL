-- Migration script for table: RecurranceTypesTbl
DELETE FROM [RecurranceTypesTbl];
SET IDENTITY_INSERT [RecurranceTypesTbl] ON;
INSERT INTO [RecurranceTypesTbl] (
[RecurringTypeID], [RecurringTypeDesc]
)
SELECT
[ID], [Type]
FROM [AccessSrc].[ReoccuranceTypeTbl];
SET IDENTITY_INSERT [RecurranceTypesTbl] OFF;
