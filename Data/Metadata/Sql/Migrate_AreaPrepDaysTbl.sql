-- Migration script for table: AreaPrepDaysTbl
DELETE FROM [AreaPrepDaysTbl];
SET IDENTITY_INSERT [AreaPrepDaysTbl] ON;
INSERT INTO [AreaPrepDaysTbl] (
[AreaPrepDaysID], [AreaID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]
)
SELECT
[CityPrepDaysID], [CityID], [PrepDayOfWeekID], [DeliveryDelayDays], [DeliveryOrder]
FROM [AccessSrc].[CityPrepDaysTbl];
SET IDENTITY_INSERT [AreaPrepDaysTbl] OFF;
