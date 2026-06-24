-- Migration script for table: TotalCountTrackerTbl
DELETE FROM [TotalCountTrackerTbl];
SET IDENTITY_INSERT [TotalCountTrackerTbl] ON;
INSERT INTO [TotalCountTrackerTbl] (
[TotalCounterTrackerID], [CountDate], [TotalCount], [Comments]
)
SELECT
[ID], dbo.SafeDateConvert([CountDate]) AS [CountDate], [TotalCount], [Comments]
FROM [AccessSrc].[TotalCountTrackerTbl];
SET IDENTITY_INSERT [TotalCountTrackerTbl] OFF;
