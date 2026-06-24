-- Migration script for table: ClosureDatesTbl
DELETE FROM [ClosureDatesTbl];
SET IDENTITY_INSERT [ClosureDatesTbl] ON;
INSERT INTO [ClosureDatesTbl] (
[ClosureDateID], [DateClosed], [DateReopen], [NextPreperationDate], [Comments]
)
SELECT
[ID], dbo.SafeDateConvert([DateClosed]) AS [DateClosed], dbo.SafeDateConvert([DateReopen]) AS [DateReopen], dbo.SafeDateConvert([NextRoastDate]) AS [NextPreperationDate], [Comments]
FROM [AccessSrc].[ClosureDatesTbl];
SET IDENTITY_INSERT [ClosureDatesTbl] OFF;

