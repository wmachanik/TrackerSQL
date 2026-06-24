-- Migration script for table: PeopleTbl
DELETE FROM [PeopleTbl];
SET IDENTITY_INSERT [PeopleTbl] ON;
INSERT INTO [PeopleTbl] (
[PersonID], [Person], [Abbreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]
)
SELECT
[PersonID], [Person], [Abreviation], [Enabled], [NormalDeliveryDoW], [SecurityUsername]
FROM [AccessSrc].[PersonsTbl];
SET IDENTITY_INSERT [PeopleTbl] OFF;
