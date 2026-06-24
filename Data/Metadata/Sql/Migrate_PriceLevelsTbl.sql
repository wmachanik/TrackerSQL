-- Migration script for table: PriceLevelsTbl
DELETE FROM [PriceLevelsTbl];
SET IDENTITY_INSERT [PriceLevelsTbl] ON;
INSERT INTO [PriceLevelsTbl] (
[PriceLevelID], [PriceLevelDesc], [PricingFactor], [Enabled], [Notes]
)
SELECT
[PriceLevelID], [PriceLevelDesc], [PricingFactor], [Enabled], [Notes]
FROM [AccessSrc].[PriceLevelsTbl];
SET IDENTITY_INSERT [PriceLevelsTbl] OFF;
