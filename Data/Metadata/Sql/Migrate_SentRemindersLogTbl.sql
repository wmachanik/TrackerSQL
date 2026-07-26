-- Migration script for table: SentRemindersLogTbl
DELETE FROM [SentRemindersLogTbl];
SET IDENTITY_INSERT [SentRemindersLogTbl] ON;
INSERT INTO [SentRemindersLogTbl] (
[ReminderID], [ContactID], [DateSentReminder], [NextPreperationDate], [ReminderSent], [HadAutoFulfilItem], [HadRecurringItems]
)
SELECT
[ReminderID], [CustomerID], dbo.SafeDateConvert([DateSentReminder]) AS [DateSentReminder], dbo.SafeDateConvert([NextPreperationDate]) AS [NextPreperationDate], [ReminderSent], [HadAutoFulfilItem], [HadReoccurItems]
FROM [AccessSrc].[SentRemindersLogTbl];
SET IDENTITY_INSERT [SentRemindersLogTbl] OFF;

