using System;
using System.Data.SqlClient;

namespace MigrationRunner
{
    class CleanupUtility
    {
        public static int CleanupMigrationTables(string sqlConnectionString)
        {
            Console.WriteLine("=== CLEANUP MIGRATION TABLES ===");
            Console.WriteLine("This will DROP the following tables if they exist:");
            Console.WriteLine("  - OrderLinesTbl, OrdersTbl");
            Console.WriteLine("  - RecurringOrderItemsTbl, RecurringOrdersTbl");
            Console.WriteLine("  - OrphanedOrderIdsTbl, OrphanedRecurringOrderIdsTbl");
            Console.WriteLine("  - Migration_OrphanedOrders, Migration_OrphanedOrderLines");
            Console.WriteLine("  - Migration_OrphanedRecurringOrderLines");
            Console.WriteLine();

            Console.Write("Are you sure you want to proceed? [y/N]: ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("Cleanup cancelled.");
                return 0;
            }

            try
            {
                using (var conn = new SqlConnection(sqlConnectionString))
                {
                    conn.Open();
                    
                    var cleanupSql = @"
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

PRINT 'Cleanup completed successfully.'
";

                    using (var cmd = new SqlCommand(cleanupSql, conn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes timeout
                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("? All migration tables have been cleaned up successfully!");
                Console.WriteLine("You can now run a fresh migration with Option A -> C -> !");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error during cleanup: {ex.Message}");
                return 1;
            }
        }
    }
}