-- Migration script for table: InvoiceTypesTbl
DELETE FROM [InvoiceTypesTbl];
SET IDENTITY_INSERT [InvoiceTypesTbl] ON;
INSERT INTO [InvoiceTypesTbl] (
[InvoiceTypeID], [InvoiceTypeDesc], [Enabled], [Notes]
)
SELECT
[InvoiceTypeID], [InvoiceTypeDesc], [Enabled], [Notes]
FROM [AccessSrc].[InvoiceTypeTbl];
SET IDENTITY_INSERT [InvoiceTypesTbl] OFF;
