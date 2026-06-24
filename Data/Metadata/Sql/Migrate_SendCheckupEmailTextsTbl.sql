-- Migration script for table: SendCheckupEmailTextsTbl
DELETE FROM [SendCheckupEmailTextsTbl];
SET IDENTITY_INSERT [SendCheckupEmailTextsTbl] ON;
INSERT INTO [SendCheckupEmailTextsTbl] (
[SCEMTID], [HeaderText], [BodyText], [FooterText], [DateLastChange], [Notes]
)
SELECT
[SCEMTID], [Header], [Body], [Footer], dbo.SafeDateConvert([DateLastChange]) AS [DateLastChange], [Notes]
FROM [AccessSrc].[SendCheckEmailTextsTbl];
SET IDENTITY_INSERT [SendCheckupEmailTextsTbl] OFF;
