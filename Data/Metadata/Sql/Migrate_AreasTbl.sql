-- Migration script for table: AreasTbl
DELETE FROM [AreasTbl];
SET IDENTITY_INSERT [AreasTbl] ON;
INSERT INTO [AreasTbl] (
[AreaID], [AreaName], [PrepDayOfWeekID], [DeliveryDelay]
)
SELECT
[ID], [City], [RoastingDay], [DeliveryDelay]
FROM [AccessSrc].[CityTbl];
SET IDENTITY_INSERT [AreasTbl] OFF;
