-- Migration script for table: AwayReasonTbl
DELETE FROM [AwayReasonTbl];
SET IDENTITY_INSERT [AwayReasonTbl] ON;
INSERT INTO [AwayReasonTbl] (
[AwayReasonID], [ReasonDesc]
)
SELECT
[AwayReasonID], [ReasonDesc]
FROM [AccessSrc].[AwayReasonTbl];
SET IDENTITY_INSERT [AwayReasonTbl] OFF;
