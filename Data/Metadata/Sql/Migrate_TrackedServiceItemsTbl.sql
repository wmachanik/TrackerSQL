-- Migration script for table: TrackedServiceItemsTbl
DELETE FROM [TrackedServiceItemsTbl];
SET IDENTITY_INSERT [TrackedServiceItemsTbl] ON;
INSERT INTO [TrackedServiceItemsTbl] (
[TrackerServiceItemID], [ItemServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]
)
SELECT
[TrackerServiceItemID], [ServiceTypeID], [TypicalAvePerItem], [UsageDateFieldName], [UsageAveFieldName], [ThisItemSetsDailyAverage], [Notes]
FROM [AccessSrc].[TrackedServiceItemTbl];
SET IDENTITY_INSERT [TrackedServiceItemsTbl] OFF;
