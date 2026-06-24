-- Migration script for table: HolidayClosuresTbl
DELETE FROM [HolidayClosuresTbl];
SET IDENTITY_INSERT [HolidayClosuresTbl] ON;
INSERT INTO [HolidayClosuresTbl] (
[HolidayClosureID], [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]
)
SELECT
[ID], dbo.SafeDateConvert([ClosureDate]) AS [ClosureDate], [DaysClosed], [AppliesToPrep], [AppliesToDelivery], [ShiftStrategy], [Description]
FROM [AccessSrc].[HolidayClosureTbl];
SET IDENTITY_INSERT [HolidayClosuresTbl] OFF;
