-- Migration script for table: ContactTypesTbl
DELETE FROM [ContactTypesTbl];
SET IDENTITY_INSERT [ContactTypesTbl] ON;
INSERT INTO [ContactTypesTbl] (
[ContactTypeID], [ContactTypeDesc], [Notes]
)
SELECT
[CustTypeID], [CustTypeDesc], [Notes]
FROM [AccessSrc].[CustomerTypeTbl];
SET IDENTITY_INSERT [ContactTypesTbl] OFF;
