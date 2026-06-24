-- Migration script for table: NextPreperationDateByAreasTbl
DELETE FROM [NextPreperationDateByAreasTbl];
SET IDENTITY_INSERT [NextPreperationDateByAreasTbl] ON;
INSERT INTO [NextPreperationDateByAreasTbl] (
[NextPrepDayID], [AreaID], [PreperationDate], [DeliveryDate], [DeliveryOrder], [NextPreperationDate], [NextDeliveryDate]
)
SELECT
[NextRoastDayID], [CityID], dbo.SafeDateConvert([PreperationDate]) AS [PreperationDate], dbo.SafeDateConvert([DeliveryDate]) AS [DeliveryDate], [DeliveryOrder], dbo.SafeDateConvert([NextPreperationDate]) AS [NextPreperationDate], dbo.SafeDateConvert([NextDeliveryDate]) AS [NextDeliveryDate]
FROM [AccessSrc].[NextRoastDateByCityTbl];
SET IDENTITY_INSERT [NextPreperationDateByAreasTbl] OFF;


