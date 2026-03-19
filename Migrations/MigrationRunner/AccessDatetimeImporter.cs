using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace MigrationRunner
{
    /// <summary>
    /// Imports ClientUsageTbl from Access with proper datetime handling
    /// This is called automatically before DML generation
    /// </summary>
    internal static class AccessDatetimeImporter
    {
        public static bool ImportClientUsageDates(string accessDbPath, string sqlConnectionString, bool whatIf = false)
        {
            try
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Auto-importing ClientUsageTbl dates from Access...");
                Console.WriteLine("========================================");
                Console.WriteLine($"  Access DB: {accessDbPath}");
                Console.WriteLine($"  WhatIf Mode: {whatIf}");
                Console.WriteLine();

                // Step 1: Connect to Access and read data
                Console.WriteLine("Step 1: Reading ClientUsageTbl from Access...");
                var dataTable = ReadFromAccess(accessDbPath);
                Console.WriteLine($"  Read {dataTable.Rows.Count} rows from Access");
                
                var datesFound = 0;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row["NextCoffeeBy"] != DBNull.Value) datesFound++;
                }
                Console.WriteLine($"  Found {datesFound} rows with NextCoffeeBy dates");

                if (whatIf)
                {
                    Console.WriteLine("WhatIf mode: Skipping SQL Server import");
                    return true;
                }

                // Step 2: Import to SQL Server
                Console.WriteLine("Step 2: Importing to SQL Server AccessSrc.ClientUsageTbl...");
                ImportToSqlServer(dataTable, sqlConnectionString);
                Console.WriteLine($"  Imported {dataTable.Rows.Count} rows successfully");
                
                // Step 3: Verify
                Console.WriteLine("Step 3: Verifying results...");
                VerifyImport(sqlConnectionString);
                
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("ClientUsageTbl import completed successfully!");
                Console.WriteLine("========================================");
                Console.WriteLine();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: Failed to import ClientUsageTbl dates");
                Console.WriteLine($"  {ex.Message}");
                Console.WriteLine();
                return false;
            }
        }

        private static DataTable ReadFromAccess(string accessDbPath)
        {
            var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={accessDbPath};";
            
            var dataTable = new DataTable();
            
            using (var conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                
                var sql = @"
                    SELECT 
                        CustomerId, 
                        LastCupCount, 
                        NextCoffeeBy, 
                        NextCleanOn, 
                        NextFilterEst, 
                        NextDescaleEst, 
                        NextServiceEst, 
                        DailyConsumption, 
                        FilterAveCount, 
                        DescaleAveCount, 
                        ServiceAveCount, 
                        CleanAveCount
                    FROM ClientUsageTbl";
                
                using (var cmd = new OleDbCommand(sql, conn))
                using (var adapter = new OleDbDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
            
            return dataTable;
        }

        private static void ImportToSqlServer(DataTable dataTable, string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                
                // Create backup
                Console.WriteLine("  Creating backup: AccessSrc.ClientUsageTbl_BACKUP...");
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        IF OBJECT_ID('AccessSrc.ClientUsageTbl_BACKUP', 'U') IS NOT NULL
                            DROP TABLE AccessSrc.ClientUsageTbl_BACKUP;
                        
                        IF OBJECT_ID('AccessSrc.ClientUsageTbl', 'U') IS NOT NULL
                            SELECT * INTO AccessSrc.ClientUsageTbl_BACKUP FROM AccessSrc.ClientUsageTbl;";
                    cmd.ExecuteNonQuery();
                }
                
                // Clear existing data
                Console.WriteLine("  Clearing existing data...");
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM AccessSrc.ClientUsageTbl";
                    cmd.ExecuteNonQuery();
                }
                
                // Insert with proper datetime handling
                var insertSql = @"
                    INSERT INTO AccessSrc.ClientUsageTbl 
                    (CustomerId, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, 
                     NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, 
                     DescaleAveCount, ServiceAveCount, CleanAveCount)
                    VALUES 
                    (@CustomerId, @LastCupCount, @NextCoffeeBy, @NextCleanOn, @NextFilterEst,
                     @NextDescaleEst, @NextServiceEst, @DailyConsumption, @FilterAveCount,
                     @DescaleAveCount, @ServiceAveCount, @CleanAveCount)";
                
                using (var cmd = new SqlCommand(insertSql, conn))
                {
                    // Define parameters with proper types
                    cmd.Parameters.Add("@CustomerId", SqlDbType.Int);
                    cmd.Parameters.Add("@LastCupCount", SqlDbType.Int);
                    cmd.Parameters.Add("@NextCoffeeBy", SqlDbType.DateTime);
                    cmd.Parameters.Add("@NextCleanOn", SqlDbType.DateTime);
                    cmd.Parameters.Add("@NextFilterEst", SqlDbType.DateTime);
                    cmd.Parameters.Add("@NextDescaleEst", SqlDbType.DateTime);
                    cmd.Parameters.Add("@NextServiceEst", SqlDbType.DateTime);
                    cmd.Parameters.Add("@DailyConsumption", SqlDbType.Float);
                    cmd.Parameters.Add("@FilterAveCount", SqlDbType.Int);
                    cmd.Parameters.Add("@DescaleAveCount", SqlDbType.Int);
                    cmd.Parameters.Add("@ServiceAveCount", SqlDbType.Int);
                    cmd.Parameters.Add("@CleanAveCount", SqlDbType.Int);
                    
                    var count = 0;
                    var total = dataTable.Rows.Count;
                    
                    foreach (DataRow row in dataTable.Rows)
                    {
                        cmd.Parameters["@CustomerId"].Value = row["CustomerId"];
                        cmd.Parameters["@LastCupCount"].Value = row["LastCupCount"] == DBNull.Value ? (object)DBNull.Value : row["LastCupCount"];
                        cmd.Parameters["@NextCoffeeBy"].Value = row["NextCoffeeBy"] == DBNull.Value ? (object)DBNull.Value : row["NextCoffeeBy"];
                        cmd.Parameters["@NextCleanOn"].Value = row["NextCleanOn"] == DBNull.Value ? (object)DBNull.Value : row["NextCleanOn"];
                        cmd.Parameters["@NextFilterEst"].Value = row["NextFilterEst"] == DBNull.Value ? (object)DBNull.Value : row["NextFilterEst"];
                        cmd.Parameters["@NextDescaleEst"].Value = row["NextDescaleEst"] == DBNull.Value ? (object)DBNull.Value : row["NextDescaleEst"];
                        cmd.Parameters["@NextServiceEst"].Value = row["NextServiceEst"] == DBNull.Value ? (object)DBNull.Value : row["NextServiceEst"];
                        cmd.Parameters["@DailyConsumption"].Value = row["DailyConsumption"] == DBNull.Value ? (object)DBNull.Value : row["DailyConsumption"];
                        cmd.Parameters["@FilterAveCount"].Value = row["FilterAveCount"] == DBNull.Value ? (object)DBNull.Value : row["FilterAveCount"];
                        cmd.Parameters["@DescaleAveCount"].Value = row["DescaleAveCount"] == DBNull.Value ? (object)DBNull.Value : row["DescaleAveCount"];
                        cmd.Parameters["@ServiceAveCount"].Value = row["ServiceAveCount"] == DBNull.Value ? (object)DBNull.Value : row["ServiceAveCount"];
                        cmd.Parameters["@CleanAveCount"].Value = row["CleanAveCount"] == DBNull.Value ? (object)DBNull.Value : row["CleanAveCount"];
                        
                        cmd.ExecuteNonQuery();
                        
                        count++;
                        if (count % 100 == 0 || count == total)
                        {
                            Console.Write($"\r  Progress: {count} / {total} rows ({count * 100.0 / total:F1}%)");
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        private static void VerifyImport(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            COUNT(*) AS Total,
                            SUM(CASE WHEN NextCoffeeBy IS NOT NULL THEN 1 ELSE 0 END) AS NextCoffeeBy_Dates,
                            SUM(CASE WHEN NextCleanOn IS NOT NULL THEN 1 ELSE 0 END) AS NextCleanOn_Dates,
                            SUM(CASE WHEN NextFilterEst IS NOT NULL THEN 1 ELSE 0 END) AS NextFilterEst_Dates
                        FROM AccessSrc.ClientUsageTbl";
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"  Total rows: {reader["Total"]}");
                            Console.WriteLine($"  NextCoffeeBy with dates: {reader["NextCoffeeBy_Dates"]}");
                            Console.WriteLine($"  NextCleanOn with dates: {reader["NextCleanOn_Dates"]}");
                            Console.WriteLine($"  NextFilterEst with dates: {reader["NextFilterEst_Dates"]}");
                        }
                    }
                }
            }
        }
    }
}
