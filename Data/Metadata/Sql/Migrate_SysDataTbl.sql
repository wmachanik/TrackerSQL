-- Migration script for table: SysDataTbl
DELETE FROM [SysDataTbl];
SET IDENTITY_INSERT [SysDataTbl] ON;
INSERT INTO [SysDataTbl] (
[ID], [LastReoccurringDate], [DoReoccuringOrders], [DateLastPrepDateCalcd], [MinReminderDate], [GroupReferenceItemID], [InternalContactIDs]
)
SELECT
[ID], dbo.SafeDateConvert([LastReoccurringDate]) AS [LastReoccurringDate], [DoReoccuringOrders], dbo.SafeDateConvert([DateLastPrepDateCalcd]) AS [DateLastPrepDateCalcd], dbo.SafeDateConvert([MinReminderDate]) AS [MinReminderDate], [GroupItemTypeID], [InternalCustomerIds]
FROM [AccessSrc].[SysDataTbl];
SET IDENTITY_INSERT [SysDataTbl] OFF;
