-- Migration script for table: RepairsTbl
DELETE FROM [RepairsTbl];
SET IDENTITY_INSERT [RepairsTbl] ON;
INSERT INTO [RepairsTbl] (
[RepairID], [ContactID], [ContactName], [ContactEmail], [JobCardNumber], [DateLogged], [LastStatusChange], [EquipTypeID], [EquipSerialNumber], [SwopOutMachineID], [EquipConditionID], [TakenFrother], [TakenBeanLid], [TakenWaterLid], [BrokenFrother], [BrokenBeanLid], [BrokenWaterLid], [RepairFaultID], [RepairFaultDesc], [RepairStatusID], [RelatedOrderID], [Notes]
)
SELECT
[RepairID], [CustomerID], [ContactName], [ContactEmail], [JobCardNumber], dbo.SafeDateConvert([DateLogged]) AS [DateLogged], dbo.SafeDateConvert([LastStatusChange]) AS [LastStatusChange], [MachineTypeID], [MachineSerialNumber], [SwopOutMachineID], [MachineConditionID], [TakenFrother], [TakenBeanLid], [TakenWaterLid], [BrokenFrother], [BrokenBeanLid], [BrokenWaterLid], [RepairFaultID], [RepairFaultDesc], [RepairStatusID], [RelatedOrderID], [Notes]
FROM [AccessSrc].[RepairsTbl];
SET IDENTITY_INSERT [RepairsTbl] OFF;
