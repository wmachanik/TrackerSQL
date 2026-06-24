-- Migration script for table: TransactionTypesTbl
DELETE FROM [TransactionTypesTbl];
SET IDENTITY_INSERT [TransactionTypesTbl] ON;
INSERT INTO [TransactionTypesTbl] (
[TransactionID], [TransactionType], [Notes]
)
SELECT
[TransactionID], [TransactionType], [Notes]
FROM [AccessSrc].[TransactionTypesTbl];
SET IDENTITY_INSERT [TransactionTypesTbl] OFF;
