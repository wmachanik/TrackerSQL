-- Migration script for table: ContactTrackedServiceItemsTbl
DELETE FROM [ContactTrackedServiceItemsTbl];
SET IDENTITY_INSERT [ContactTrackedServiceItemsTbl] ON;
INSERT INTO [ContactTrackedServiceItemsTbl] (
[ContactTrackedServiceItemsID], [ContactTypeID], [ItemServiceTypeID], [Notes]
)
SELECT
[CustomerTrackedServiceItemsID], [CustomerTypeID], [ServiceTypeID], [Notes]
FROM [AccessSrc].[CustomerTrackedServiceItemsTbl];
SET IDENTITY_INSERT [ContactTrackedServiceItemsTbl] OFF;
