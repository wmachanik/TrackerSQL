-- Migration script for table: ContactsTbl
DELETE FROM [ContactsTbl];
SET IDENTITY_INSERT [ContactsTbl] ON;
INSERT INTO [ContactsTbl] (
[ContactID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [AreaID], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [ContactTypeID], [EquipTypeID], [ItemPrefID], [PriPrefQty], [PrefItemPrepTypeID], [PrefItemPackagingID], [SecondaryItemPrefID], [SecPrefQty], [TypicallySecToo], [PreferredAgentID], [SalesAgentID], [EquipentSN], [UsesFilter], [AutoFulfill], [Enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], [LastDateSentReminder]
)
SELECT
[CustomerID], [CompanyName], [ContactTitle], [ContactFirstName], [ContactLastName], [ContactAltFirstName], [ContactAltLastName], [Department], [BillingAddress], [City], [StateOrProvince], [PostalCode], [Country/Region], [PhoneNumber], [Extension], [FaxNumber], [CellNumber], [EmailAddress], [AltEmailAddress], [ContractNo], [CustomerTypeID], [EquipType], [CoffeePreference], [PriPrefQty], [PrefPrepTypeID], [PrefPackagingID], [SecondaryPreference], [SecPrefQty], [TypicallySecToo], [PreferedAgent], [SalesAgentID], [MachineSN], [UsesFilter], [autofulfill], [enabled], [PredictionDisabled], [AlwaysSendChkUp], [NormallyResponds], [ReminderCount], [Notes], [SendDeliveryConfirmation], dbo.SafeDateConvert([LastDateSentReminder]) AS [LastDateSentReminder]
FROM [AccessSrc].[CustomersTbl];
SET IDENTITY_INSERT [ContactsTbl] OFF;
