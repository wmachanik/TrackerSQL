-- Migration script for table: ContactsAwayPeriodTbl
DELETE FROM [ContactsAwayPeriodTbl];
SET IDENTITY_INSERT [ContactsAwayPeriodTbl] ON;
INSERT INTO [ContactsAwayPeriodTbl] (
[AwayPeriodID], [ContactID], [AwayStartDate], [AwayEndDate], [ReasonID]
)
SELECT
[AwayPeriodID], [ClientID], dbo.SafeDateConvert([AwayStartDate]) AS [AwayStartDate], dbo.SafeDateConvert([AwayEndDate]) AS [AwayEndDate], [ReasonID]
FROM [AccessSrc].[ClientAwayPeriodTbl];
SET IDENTITY_INSERT [ContactsAwayPeriodTbl] OFF;
