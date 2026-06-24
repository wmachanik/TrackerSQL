-- Migration script for table: ItemServiceTypesTbl
DELETE FROM [ItemServiceTypesTbl];
SET IDENTITY_INSERT [ItemServiceTypesTbl] ON;
INSERT INTO [ItemServiceTypesTbl] (
[ItemServiceTypeID], [ItemServiceType], [Description], [ItemPackagingID], [ItemPrepTypeID]
)
SELECT
[ServiceTypeId], [ServiceType], [Description], [PackagingID], [PrepTypeID]
FROM [AccessSrc].[ServiceTypesTbl];
SET IDENTITY_INSERT [ItemServiceTypesTbl] OFF;
