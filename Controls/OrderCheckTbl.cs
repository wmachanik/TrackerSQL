using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Controls
{
    /// <summary>
    /// Database access layer for order conflict checking and coffee checkup queries
    /// Follows the existing OrderTbl pattern
    /// </summary>
    public class OrderCheckTbl
    {
        private const string CONST_CUSTOMERS_WITHOUT_CONFLICTS = @"
            SELECT DISTINCT 
                c.CustomerID, c.CompanyName, c.ContactFirstName, c.ContactAltFirstName,
                c.EmailAddress, c.AltEmailAddress, c.City, c.CustomerTypeID, c.enabled,
                c.EquipType, c.TypicallySecToo, c.PreferedAgent, c.SalesAgentID,
                c.UsesFilter, c.AlwaysSendChkUp, c.autofulfill, c.ReminderCount,
                cu.NextCoffeeBy, cu.NextCleanOn, cu.NextDescaleEst, cu.NextFilterEst, cu.NextServiceEst,
                nrd.PrepDate, nrd.DeliveryDate
            FROM (((CustomersTbl c 
                INNER JOIN ClientUsageTbl cu ON c.CustomerID = cu.CustomerID)
                LEFT JOIN NextRoastDateByCityTbl nrd ON c.City = nrd.CityID)
                LEFT JOIN CustomerTrackedServiceItemsTbl ctsi ON c.CustomerTypeID = ctsi.CustomerTypeID)
            WHERE c.enabled = 1 
              AND c.ReminderCount < ? 
              AND cu.NextCoffeeBy <= DateAdd('d', 7, Now())
              AND c.CustomerID NOT IN (
                  SELECT DISTINCT o.CustomerID 
                  FROM OrdersTbl o
                  INNER JOIN ItemTypeTbl i ON o.ItemTypeID = i.ItemTypeID
                  WHERE o.RequiredByDate BETWEEN Now() AND DateAdd('d', 7, Now())
                    AND i.ServiceTypeID IN (2, 21)
              )
              AND c.CustomerID NOT IN (
                  SELECT DISTINCT r.CustomerID
                  FROM ReoccuringOrderTbl r  
                  WHERE r.Enabled = 1
                    AND r.NextDateRequired <= DateAdd('d', 7, Now())
              )
            ORDER BY c.CompanyName";

        private const string CONST_CUSTOMER_TYPICAL_ITEMS = @"
            SELECT DISTINCT iu.ItemProvidedID, iu.AmountProvided, iu.PackagingID
            FROM ItemUsageTbl iu
            INNER JOIN ItemTypeTbl it ON iu.ItemProvidedID = it.ItemTypeID
            WHERE iu.CustomerID = ? 
              AND it.ServiceTypeID IN (2, 21)
              AND iu.DateProvided >= DateAdd('m', -6, Now())
            ORDER BY iu.DateProvided DESC";

        private const string CONST_COFFEE_ORDERS_IN_DATE_RANGE = @"
            SELECT o.CustomerID, o.OrderID, o.OrderDate, o.RequiredByDate, o.ItemTypeID
            FROM OrdersTbl o
            INNER JOIN ItemTypeTbl i ON o.ItemTypeID = i.ItemTypeID
            WHERE o.CustomerID = ? 
              AND o.RequiredByDate BETWEEN ? AND ?
              AND i.ServiceTypeID IN (2, 21)";

        /// <summary>
        /// Gets customers eligible for coffee checkup reminders (without conflicts)
        /// Filters out customers who already have coffee orders or recurring orders due
        /// </summary>
        public List<CustomerCheckupData> GetCustomersWithoutOrderConflicts(int maxReminders)
        {
            List<CustomerCheckupData> customers = new List<CustomerCheckupData>();

            try
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddWhereParams(maxReminders, DbType.Int32);

                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_CUSTOMERS_WITHOUT_CONFLICTS);
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        customers.Add(new CustomerCheckupData
                        {
                            CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt64(dataReader["CustomerID"]),
                            CompanyName = dataReader["CompanyName"]?.ToString() ?? "",
                            ContactFirstName = dataReader["ContactFirstName"]?.ToString() ?? "",
                            ContactAltFirstName = dataReader["ContactAltFirstName"]?.ToString() ?? "",
                            EmailAddress = dataReader["EmailAddress"]?.ToString() ?? "",
                            AltEmailAddress = dataReader["AltEmailAddress"]?.ToString() ?? "",
                            CityID = dataReader["City"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["City"]),
                            CustomerTypeID = dataReader["CustomerTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTypeID"]),
                            Enabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"]),
                            EquipTypeID = dataReader["EquipType"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EquipType"]),
                            TypicallySecToo = dataReader["TypicallySecToo"] != DBNull.Value && Convert.ToBoolean(dataReader["TypicallySecToo"]),
                            PreferedAgentID = dataReader["PreferedAgent"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PreferedAgent"]),
                            SalesAgentID = dataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SalesAgentID"]),
                            UsesFilter = dataReader["UsesFilter"] != DBNull.Value && Convert.ToBoolean(dataReader["UsesFilter"]),
                            AlwaysSendChkUp = dataReader["AlwaysSendChkUp"] != DBNull.Value && Convert.ToBoolean(dataReader["AlwaysSendChkUp"]),
                            AutoFulfill = dataReader["autofulfill"] != DBNull.Value && Convert.ToBoolean(dataReader["autofulfill"]),
                            ReminderCount = dataReader["ReminderCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReminderCount"]),
                            NextCoffee = dataReader["NextCoffeeBy"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(dataReader["NextCoffeeBy"]),
                            NextClean = dataReader["NextCleanOn"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextCleanOn"]),
                            NextDescal = dataReader["NextDescaleEst"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextDescaleEst"]),
                            NextFilter = dataReader["NextFilterEst"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextFilterEst"]),
                            NextService = dataReader["NextServiceEst"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextServiceEst"]),
                            NextPrepDate = dataReader["PrepDate"] == DBNull.Value ? DateTime.Now.AddDays(1) : Convert.ToDateTime(dataReader["PrepDate"]),
                            NextDeliveryDate = dataReader["DeliveryDate"] == DBNull.Value ? DateTime.Now.AddDays(2) : Convert.ToDateTime(dataReader["DeliveryDate"])
                        });
                    }
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"OrderCheckTbl: Error getting customers without conflicts: {ex.Message}");
            }

            return customers;
        }

        /// <summary>
        /// Gets typical coffee items for a customer based on recent usage
        /// </summary>
        public List<CustomerTypicalItem> GetCustomerTypicalItems(long customerId)
        {
            List<CustomerTypicalItem> items = new List<CustomerTypicalItem>();

            try
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddWhereParams(customerId, DbType.Int64);

                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_CUSTOMER_TYPICAL_ITEMS);
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        items.Add(new CustomerTypicalItem
                        {
                            ItemID = dataReader["ItemProvidedID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemProvidedID"]),
                            Quantity = dataReader["AmountProvided"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["AmountProvided"]),
                            PackagingID = dataReader["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PackagingID"])
                        });
                    }
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"OrderCheckTbl: Error getting typical items for customer {customerId}: {ex.Message}");
            }

            return items;
        }

        /// <summary>
        /// Checks if customer has coffee orders in the specified date range
        /// </summary>
        public bool HasCoffeeOrdersInDateRange(long customerId, DateTime startDate, DateTime endDate)
        {
            bool hasOrders = false;

            try
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddWhereParams(customerId, DbType.Int64);
                trackerDb.AddWhereParams(startDate.Date, DbType.Date);
                trackerDb.AddWhereParams(endDate.Date, DbType.Date);

                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_COFFEE_ORDERS_IN_DATE_RANGE);
                if (dataReader != null)
                {
                    hasOrders = dataReader.Read(); // True if any rows returned
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"OrderCheckTbl: Error checking orders for customer {customerId}: {ex.Message}");
            }

            return hasOrders;
        }

        /// <summary>
        /// Gets coffee orders for a customer in the specified date range
        /// </summary>
        public List<CoffeeOrderData> GetCoffeeOrdersInDateRange(long customerId, DateTime startDate, DateTime endDate)
        {
            List<CoffeeOrderData> orders = new List<CoffeeOrderData>();

            try
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddWhereParams(customerId, DbType.Int64);
                trackerDb.AddWhereParams(startDate.Date, DbType.Date);
                trackerDb.AddWhereParams(endDate.Date, DbType.Date);

                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_COFFEE_ORDERS_IN_DATE_RANGE);
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        orders.Add(new CoffeeOrderData
                        {
                            CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt64(dataReader["CustomerID"]),
                            OrderID = dataReader["OrderID"] == DBNull.Value ? 0 : Convert.ToInt64(dataReader["OrderID"]),
                            OrderDate = dataReader["OrderDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["OrderDate"]),
                            RequiredByDate = dataReader["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["RequiredByDate"]),
                            ItemTypeID = dataReader["ItemTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemTypeID"])
                        });
                    }
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"OrderCheckTbl: Error getting coffee orders for customer {customerId}: {ex.Message}");
            }

            return orders;
        }
    }

    /// <summary>
    /// Data class for customer checkup information
    /// </summary>
    public class CustomerCheckupData
    {
        public long CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactAltFirstName { get; set; }
        public string EmailAddress { get; set; }
        public string AltEmailAddress { get; set; }
        public int CityID { get; set; }
        public int CustomerTypeID { get; set; }
        public bool Enabled { get; set; }
        public int EquipTypeID { get; set; }
        public bool TypicallySecToo { get; set; }
        public int PreferedAgentID { get; set; }
        public int SalesAgentID { get; set; }
        public bool UsesFilter { get; set; }
        public bool AlwaysSendChkUp { get; set; }
        public bool AutoFulfill { get; set; }
        public int ReminderCount { get; set; }
        public DateTime NextCoffee { get; set; }
        public DateTime NextClean { get; set; }
        public DateTime NextDescal { get; set; }
        public DateTime NextFilter { get; set; }
        public DateTime NextService { get; set; }
        public DateTime NextPrepDate { get; set; }
        public DateTime NextDeliveryDate { get; set; }
    }

    /// <summary>
    /// Data class for customer typical items
    /// </summary>
    public class CustomerTypicalItem
    {
        public int ItemID { get; set; }
        public double Quantity { get; set; }
        public int PackagingID { get; set; }
    }

    /// <summary>
    /// Data class for coffee order information
    /// </summary>
    public class CoffeeOrderData
    {
        public long CustomerID { get; set; }
        public long OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredByDate { get; set; }
        public int ItemTypeID { get; set; }
    }
}