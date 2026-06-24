using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class CoffeeCheckupRepository : RepositoryBase<Contact>
    {
        private readonly SentRemindersLogRepository _sentRemindersLogRepository = new SentRemindersLogRepository();
        private readonly SysDataRepository _sysDataRepository = new SysDataRepository();

        protected override string TableName => "ContactsTbl";
        protected override string KeyColumn => "ContactID";

        public List<CustomerCheckupData> GetCustomersWithoutOrderConflicts(int maxReminders)
        {
            const string sql = @"
                SELECT DISTINCT
                    c.ContactID, c.CompanyName, c.ContactFirstName, c.ContactAltFirstName,
                    c.EmailAddress, c.AltEmailAddress, c.AreaID, c.ContactTypeID, c.Enabled,
                    c.EquipTypeID, c.TypicallySecToo, c.PreferredAgentID, c.SalesAgentID,
                    c.UsesFilter, c.AlwaysSendChkUp, c.AutoFulfill, c.ReminderCount,
                    cu.NextCoffeeBy, cu.NextCleanOn, cu.NextDescaleEst, cu.NextFilterEst, cu.NextServiceEst,
                    nrd.PreperationDate AS PrepDate, nrd.DeliveryDate
                FROM ContactsTbl c
                INNER JOIN ContactsItemsPredictedTbl cu ON c.ContactID = cu.ContactID
                LEFT JOIN NextPreperationDateByAreasTbl nrd ON c.AreaID = nrd.AreaID
                WHERE c.Enabled = 1
                  AND c.ReminderCount < @MaxReminders
                  AND cu.NextCoffeeBy <= DATEADD(day, 7, CAST(GETDATE() AS date))
                  AND c.ContactID NOT IN (
                      SELECT DISTINCT o.ContactID
                      FROM OrdersTbl o
                      INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                      INNER JOIN ItemsTbl i ON ol.ItemID = i.ItemID
                      WHERE o.RequiredByDate BETWEEN CAST(GETDATE() AS date) AND DATEADD(day, 7, CAST(GETDATE() AS date))
                        AND i.ItemServiceTypeID IN (2, 21)
                  )
                  AND c.ContactID NOT IN (
                      SELECT DISTINCT r.ContactID
                      FROM RecurringOrdersTbl r
                      INNER JOIN RecurringOrderItemsTbl ri ON r.RecurringOrderID = ri.RecurringOrderID
                      WHERE r.Enabled = 1
                        AND ri.NextDateRequired <= DATEADD(day, 7, CAST(GETDATE() AS date))
                  )
                ORDER BY c.CompanyName";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@MaxReminders", DataValue = maxReminders, DataDbType = DbType.Int32 }
            };

            return MapCustomerCheckupList(sql, parameters);
        }

        public List<CustomerTypicalItem> GetCustomerTypicalItems(long contactId)
        {
            const string sql = @"
                SELECT DISTINCT u.ItemProvidedID, u.QtyProvided, u.ItemPackagingID
                FROM ContactsItemUsageTbl u
                INNER JOIN ItemsTbl i ON u.ItemProvidedID = i.ItemID
                WHERE u.ContactID = @ContactID
                  AND i.ItemServiceTypeID IN (2, 21)
                  AND u.DeliveryDate >= DATEADD(month, -6, CAST(GETDATE() AS date))
                ORDER BY u.DeliveryDate DESC";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 }
            };

            var items = new List<CustomerTypicalItem>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    items.Add(new CustomerTypicalItem
                    {
                        ItemID = GetInt(rdr, "ItemProvidedID"),
                        Quantity = GetDouble(rdr, "QtyProvided"),
                        PackagingID = GetInt(rdr, "ItemPackagingID")
                    });
                }
            }

            return items;
        }

        public bool HasCoffeeOrdersInDateRange(long contactId, DateTime startDate, DateTime endDate)
        {
            const string sql = @"
                SELECT TOP 1 o.ContactID
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                INNER JOIN ItemsTbl i ON ol.ItemID = i.ItemID
                WHERE o.ContactID = @ContactID
                  AND o.RequiredByDate BETWEEN @StartDate AND @EndDate
                  AND i.ItemServiceTypeID IN (2, 21)";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@StartDate", DataValue = startDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@EndDate", DataValue = endDate.Date, DataDbType = DbType.Date }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                return rdr != null && rdr.Read();
            }
        }

        public List<CoffeeOrderData> GetCoffeeOrdersInDateRange(long contactId, DateTime startDate, DateTime endDate)
        {
            const string sql = @"
                SELECT o.ContactID, o.OrderID, o.OrderDate, o.RequiredByDate, ol.ItemID AS ItemTypeID
                FROM OrdersTbl o
                INNER JOIN OrderLinesTbl ol ON o.OrderID = ol.OrderID
                INNER JOIN ItemsTbl i ON ol.ItemID = i.ItemID
                WHERE o.ContactID = @ContactID
                  AND o.RequiredByDate BETWEEN @StartDate AND @EndDate
                  AND i.ItemServiceTypeID IN (2, 21)";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@StartDate", DataValue = startDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@EndDate", DataValue = endDate.Date, DataDbType = DbType.Date }
            };

            var orders = new List<CoffeeOrderData>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    orders.Add(new CoffeeOrderData
                    {
                        CustomerID = GetLong(rdr, "ContactID"),
                        OrderID = GetLong(rdr, "OrderID"),
                        OrderDate = GetDate(rdr, "OrderDate"),
                        RequiredByDate = GetDate(rdr, "RequiredByDate"),
                        ItemTypeID = GetInt(rdr, "ItemTypeID")
                    });
                }
            }

            return orders;
        }

        public List<ContactMayNeedReminder> GetContactsThatMayNeedNextWeek(int reminderWindowDays)
        {
            DateTime baselineDate = TimeZoneUtils.Now().Date;
            DateTime lastCheckupDate = _sentRemindersLogRepository.GetLastSuccessfulCheckupDate();
            DateTime deliveryFilterDate = lastCheckupDate < baselineDate ? lastCheckupDate : baselineDate;
            DateTime minReminderDate = _sysDataRepository.GetMinReminderDate().Date;

            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                $"CoffeeCheckupRepository: Using deliveryFilterDate={deliveryFilterDate:yyyy-MM-dd} (LastCheckup={lastCheckupDate:yyyy-MM-dd}, Now={baselineDate:yyyy-MM-dd})");

            string sql = $@"
                SELECT c.ContactID, c.CompanyName, c.ContactFirstName, c.ContactAltFirstName,
                       c.EmailAddress, c.AltEmailAddress, c.AreaID, c.ContactTypeID, c.EquipTypeID,
                       c.TypicallySecToo, c.PreferredAgentID, c.SalesAgentID, c.UsesFilter, c.AutoFulfill,
                       c.AlwaysSendChkUp, c.Enabled, c.ReminderCount, ai.RequiresPurchOrder,
                       cu.NextCoffeeBy, cu.NextCleanOn, cu.NextFilterEst, cu.NextDescaleEst, cu.NextServiceEst,
                       nrd.PreperationDate, nrd.DeliveryDate, nrd.NextPreperationDate, nrd.NextDeliveryDate
                FROM ContactsTbl c
                INNER JOIN ContactsItemsPredictedTbl cu ON c.ContactID = cu.ContactID
                LEFT JOIN ContactsAccInfoTbl ai ON c.ContactID = ai.ContactID
                LEFT JOIN NextPreperationDateByAreasTbl nrd ON c.AreaID = nrd.AreaID
                WHERE (c.LastDateSentReminder IS NULL OR c.LastDateSentReminder <> @BaselineDate)
                  AND c.Enabled = 1
                  AND c.PredictionDisabled = 0
                  AND cu.NextCoffeeBy > @MinReminderDate
                  AND (nrd.NextDeliveryDate <= DATEADD(day, @ReminderWindowDays, cu.NextCoffeeBy)
                       OR c.AlwaysSendChkUp = 1)
                  AND NOT EXISTS (
                      SELECT 1
                      FROM OrdersTbl o
                      WHERE o.ContactID = c.ContactID
                        AND o.PrepDate >= CAST(GETDATE() AS date)
                        AND o.PrepDate <= DATEADD(day, @ReminderWindowDays, CAST(GETDATE() AS date))
                  )
                ORDER BY c.CompanyName";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@BaselineDate", DataValue = baselineDate, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@MinReminderDate", DataValue = minReminderDate, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ReminderWindowDays", DataValue = reminderWindowDays, DataDbType = DbType.Int32 }
            };

            var list = new List<ContactMayNeedReminder>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new ContactMayNeedReminder
                    {
                        ContactID = GetLong(rdr, "ContactID"),
                        CompanyName = GetString(rdr, "CompanyName"),
                        ContactFirstName = GetString(rdr, "ContactFirstName"),
                        ContactAltFirstName = GetString(rdr, "ContactAltFirstName"),
                        EmailAddress = GetString(rdr, "EmailAddress"),
                        AltEmailAddress = GetString(rdr, "AltEmailAddress"),
                        AreaID = GetInt(rdr, "AreaID"),
                        ContactTypeID = GetInt(rdr, "ContactTypeID"),
                        EquipTypeID = GetInt(rdr, "EquipTypeID"),
                        TypicallySecToo = GetBool(rdr, "TypicallySecToo"),
                        PreferredAgentID = GetInt(rdr, "PreferredAgentID"),
                        SalesAgentID = GetInt(rdr, "SalesAgentID"),
                        UsesFilter = GetBool(rdr, "UsesFilter"),
                        AutoFulfill = GetBool(rdr, "AutoFulfill"),
                        AlwaysSendChkUp = GetBool(rdr, "AlwaysSendChkUp"),
                        Enabled = GetBool(rdr, "Enabled"),
                        ReminderCount = GetInt(rdr, "ReminderCount"),
                        RequiresPurchOrder = GetBool(rdr, "RequiresPurchOrder"),
                        NextCoffeeBy = GetDate(rdr, "NextCoffeeBy"),
                        NextCleanOn = GetDate(rdr, "NextCleanOn"),
                        NextFilterEst = GetDate(rdr, "NextFilterEst"),
                        NextDescaleEst = GetDate(rdr, "NextDescaleEst"),
                        NextServiceEst = GetDate(rdr, "NextServiceEst"),
                        PrepDate = GetDate(rdr, "PreperationDate"),
                        DeliveryDate = GetDate(rdr, "DeliveryDate"),
                        NextPreperationDate = GetDate(rdr, "NextPreperationDate"),
                        NextDeliveryDate = GetDate(rdr, "NextDeliveryDate")
                    });
                }
            }

            return list;
        }

        public ContactToRemindWithItems GetCustomerDetails(long contactId)
        {
            const string sql = @"
                SELECT c.ContactID, c.CompanyName, c.ContactTitle, c.ContactFirstName, c.ContactAltFirstName,
                       c.EmailAddress, c.AltEmailAddress, c.ContactTypeID, c.TypicallySecToo,
                       c.PreferredAgentID, c.SalesAgentID, c.UsesFilter, c.AutoFulfill, c.Enabled,
                       c.AlwaysSendChkUp, c.ReminderCount, c.LastDateSentReminder, ai.RequiresPurchOrder,
                       nrd.AreaID, nrd.DeliveryDate, nrd.PreperationDate,
                       cu.NextCoffeeBy, cu.NextCleanOn, cu.NextFilterEst, cu.NextDescaleEst, cu.NextServiceEst
                FROM ContactsTbl c
                LEFT JOIN ContactsAccInfoTbl ai ON c.ContactID = ai.ContactID
                LEFT JOIN ContactsItemsPredictedTbl cu ON c.ContactID = cu.ContactID
                LEFT JOIN NextPreperationDateByAreasTbl nrd ON c.AreaID = nrd.AreaID
                WHERE c.ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr == null || !rdr.Read())
                {
                    return null;
                }

                return new ContactToRemindWithItems
                {
                    CustomerID = GetLong(rdr, "ContactID"),
                    CompanyName = GetString(rdr, "CompanyName"),
                    ContactTitle = GetString(rdr, "ContactTitle"),
                    ContactFirstName = GetString(rdr, "ContactFirstName"),
                    ContactAltFirstName = GetString(rdr, "ContactAltFirstName"),
                    EmailAddress = GetString(rdr, "EmailAddress"),
                    AltEmailAddress = GetString(rdr, "AltEmailAddress"),
                    CustomerTypeID = GetInt(rdr, "ContactTypeID"),
                    TypicallySecToo = GetBool(rdr, "TypicallySecToo"),
                    PreferredAgentID = GetInt(rdr, "PreferredAgentID"),
                    SalesAgentID = GetInt(rdr, "SalesAgentID"),
                    UsesFilter = GetBool(rdr, "UsesFilter"),
                    autofulfill = GetBool(rdr, "AutoFulfill"),
                    enabled = GetBool(rdr, "Enabled"),
                    AlwaysSendChkUp = GetBool(rdr, "AlwaysSendChkUp"),
                    ReminderCount = GetInt(rdr, "ReminderCount"),
                    LastDateSentReminder = rdr["LastDateSentReminder"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(rdr["LastDateSentReminder"]).Date,
                    RequiresPurchOrder = GetBool(rdr, "RequiresPurchOrder"),
                    AreaID = GetInt(rdr, "AreaID"),
                    NextDeliveryDate = GetDate(rdr, "DeliveryDate"),
                    NextPreperationDate = GetDate(rdr, "PreperationDate"),
                    NextCoffee = GetDate(rdr, "NextCoffeeBy"),
                    NextClean = GetDate(rdr, "NextCleanOn"),
                    NextFilter = GetDate(rdr, "NextFilterEst"),
                    NextDescal = GetDate(rdr, "NextDescaleEst"),
                    NextService = GetDate(rdr, "NextServiceEst")
                };
            }
        }

        private List<CustomerCheckupData> MapCustomerCheckupList(string sql, List<DBParameter> parameters)
        {
            var customers = new List<CustomerCheckupData>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    customers.Add(new CustomerCheckupData
                    {
                        CustomerID = GetLong(rdr, "ContactID"),
                        CompanyName = GetString(rdr, "CompanyName"),
                        ContactFirstName = GetString(rdr, "ContactFirstName"),
                        ContactAltFirstName = GetString(rdr, "ContactAltFirstName"),
                        EmailAddress = GetString(rdr, "EmailAddress"),
                        AltEmailAddress = GetString(rdr, "AltEmailAddress"),
                        AreaID = GetInt(rdr, "AreaID"),
                        CustomerTypeID = GetInt(rdr, "ContactTypeID"),
                        Enabled = GetBool(rdr, "Enabled"),
                        EquipTypeID = GetInt(rdr, "EquipTypeID"),
                        TypicallySecToo = GetBool(rdr, "TypicallySecToo"),
                        PreferredAgentID = GetInt(rdr, "PreferredAgentID"),
                        SalesAgentID = GetInt(rdr, "SalesAgentID"),
                        UsesFilter = GetBool(rdr, "UsesFilter"),
                        AlwaysSendChkUp = GetBool(rdr, "AlwaysSendChkUp"),
                        AutoFulfill = GetBool(rdr, "AutoFulfill"),
                        ReminderCount = GetInt(rdr, "ReminderCount"),
                        NextCoffee = GetDateOrNow(rdr, "NextCoffeeBy"),
                        NextClean = GetDateOrMax(rdr, "NextCleanOn"),
                        NextDescal = GetDateOrMax(rdr, "NextDescaleEst"),
                        NextFilter = GetDateOrMax(rdr, "NextFilterEst"),
                        NextService = GetDateOrMax(rdr, "NextServiceEst"),
                        NextPreperationDate = GetDateOrTomorrow(rdr, "PrepDate"),
                        NextDeliveryDate = GetDateOrDayAfterTomorrow(rdr, "DeliveryDate")
                    });
                }
            }

            return customers;
        }

        private static string GetString(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? string.Empty : Convert.ToString(rdr[name]);
        }

        private static int GetInt(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? 0 : Convert.ToInt32(rdr[name]);
        }

        private static long GetLong(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? 0 : Convert.ToInt64(rdr[name]);
        }

        private static double GetDouble(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? 0.0 : Convert.ToDouble(rdr[name]);
        }

        private static bool GetBool(IDataReader rdr, string name)
        {
            return rdr[name] != DBNull.Value && Convert.ToBoolean(rdr[name]);
        }

        private static DateTime GetDate(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(rdr[name]).Date;
        }

        private static DateTime GetDateOrNow(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(rdr[name]).Date;
        }

        private static DateTime GetDateOrMax(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(rdr[name]).Date;
        }

        private static DateTime GetDateOrTomorrow(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? TimeZoneUtils.Now().Date.AddDays(1) : Convert.ToDateTime(rdr[name]).Date;
        }

        private static DateTime GetDateOrDayAfterTomorrow(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? TimeZoneUtils.Now().Date.AddDays(2) : Convert.ToDateTime(rdr[name]).Date;
        }
    }
}
