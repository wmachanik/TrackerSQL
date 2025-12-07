-- ========================================
-- Clean Migration Script for Order Tables  
-- Run this before custom migration (!)
-- ========================================

USE [OtterDb]  -- Change to your database name
GO

PRINT 'Starting cleanup of migration tables...'

-- Drop tables in correct dependency order
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderLinesTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping OrderLinesTbl...'
    DROP TABLE [dbo].[OrderLinesTbl];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecurringOrderItemsTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping RecurringOrderItemsTbl...'
    DROP TABLE [dbo].[RecurringOrderItemsTbl];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdersTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping OrdersTbl...'
    DROP TABLE [dbo].[OrdersTbl];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecurringOrdersTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping RecurringOrdersTbl...'
    DROP TABLE [dbo].[RecurringOrdersTbl];
END

-- Drop orphan tracking tables
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrphanedOrderIdsTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping OrphanedOrderIdsTbl...'
    DROP TABLE [dbo].[OrphanedOrderIdsTbl];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrphanedRecurringOrderIdsTbl]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping OrphanedRecurringOrderIdsTbl...'
    DROP TABLE [dbo].[OrphanedRecurringOrderIdsTbl];
END

-- Drop migration diagnostic tables
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Migration_OrphanedOrders]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping Migration_OrphanedOrders...'
    DROP TABLE [dbo].[Migration_OrphanedOrders];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Migration_OrphanedOrderLines]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping Migration_OrphanedOrderLines...'
    DROP TABLE [dbo].[Migration_OrphanedOrderLines];
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Migration_OrphanedRecurringOrderLines]') AND type in (N'U'))
BEGIN
    PRINT 'Dropping Migration_OrphanedRecurringOrderLines...'
    DROP TABLE [dbo].[Migration_OrphanedRecurringOrderLines];
END

PRINT 'Cleanup completed successfully!'
PRINT ''
PRINT 'Next steps:'
PRINT '1. Run MigrationRunner Option A (Generate CREATE TABLE DDL) with DROP=Yes'
PRINT '2. Run MigrationRunner Option C (Apply DDL scripts)'  
PRINT '3. Run MigrationRunner Option ! (Custom migrate Orders + Recurring tables)'
PRINT ''
PRINT 'DO NOT run Option N (regular data migration) as it conflicts with Option !'