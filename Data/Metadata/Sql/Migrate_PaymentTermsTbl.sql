-- Migration script for table: PaymentTermsTbl
DELETE FROM [PaymentTermsTbl];
SET IDENTITY_INSERT [PaymentTermsTbl] ON;
INSERT INTO [PaymentTermsTbl] (
[PaymentTermID], [PaymentTermDesc], [PaymentDays], [DayOfMonth], [UseDays], [Enabled], [Notes]
)
SELECT
[PaymentTermID], [PaymentTermDesc], [PaymentDays], [DayOfMonth], [UseDays], [Enabled], [Notes]
FROM [AccessSrc].[PaymentTermsTbl];
SET IDENTITY_INSERT [PaymentTermsTbl] OFF;
