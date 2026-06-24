-- Migration script for table: ContactsAccInfoTbl
DELETE FROM [ContactsAccInfoTbl];
SET IDENTITY_INSERT [ContactsAccInfoTbl] ON;
INSERT INTO [ContactsAccInfoTbl] (
[ContactsAccInfoID], [ContactID], [RequiresPurchOrder], [ContactVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]
)
SELECT
[CustomersAccInfoID], [CustomerID], [RequiresPurchOrder], [CustomerVATNo], [BillAddr1], [BillAddr2], [BillAddr3], [BillAddr4], [BillAddr5], [ShipAddr1], [ShipAddr2], [ShipAddr3], [ShipAddr4], [ShipAddr5], [AccEmail], [AltAccEmail], [PaymentTermID], [Limit], [FullCoName], [AccFirstName], [AccLastName], [AltAccFirstName], [AltAccLastName], [PriceLevelID], [InvoiceTypeID], [RegNo], [BankAccNo], [BankBranch], [Enabled], [Notes]
FROM [AccessSrc].[CustomersAccInfoTbl];
SET IDENTITY_INSERT [ContactsAccInfoTbl] OFF;
