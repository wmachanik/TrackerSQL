/* Config export: dbo.WooPaymentMethodMapTbl (5 rows) */
SET NOCOUNT ON;
DELETE FROM dbo.[WooPaymentMethodMapTbl];
INSERT INTO dbo.[WooPaymentMethodMapTbl] ([MapID], [MethodMatch], [PaymentAbbrev], [IsActive], [Notes], [UpdatedAt], [UpdatedBy]) VALUES (1, N'payfast', N'PF', 1, N'Default seed', N'2026-08-31T09:42:57.627', N'seed');
INSERT INTO dbo.[WooPaymentMethodMapTbl] ([MapID], [MethodMatch], [PaymentAbbrev], [IsActive], [Notes], [UpdatedAt], [UpdatedBy]) VALUES (2, N'yoco', N'Yoco', 1, N'Default seed', N'2026-08-31T09:42:57.627', N'seed');
INSERT INTO dbo.[WooPaymentMethodMapTbl] ([MapID], [MethodMatch], [PaymentAbbrev], [IsActive], [Notes], [UpdatedAt], [UpdatedBy]) VALUES (3, N'snapscan', N'SS', 1, N'Default seed', N'2026-08-31T09:42:57.627', N'seed');
INSERT INTO dbo.[WooPaymentMethodMapTbl] ([MapID], [MethodMatch], [PaymentAbbrev], [IsActive], [Notes], [UpdatedAt], [UpdatedBy]) VALUES (4, N'bacs', N'EFT', 1, N'Direct bank / EFT', N'2026-08-31T09:42:57.627', N'seed');
INSERT INTO dbo.[WooPaymentMethodMapTbl] ([MapID], [MethodMatch], [PaymentAbbrev], [IsActive], [Notes], [UpdatedAt], [UpdatedBy]) VALUES (5, N'eft', N'EFT', 1, N'Direct bank / EFT', N'2026-08-31T09:42:57.627', N'seed');
GO
