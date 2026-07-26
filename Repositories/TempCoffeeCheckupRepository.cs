using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class TempCoffeeCheckupRepository : RepositoryBase<TempCoffeecheckupCustomer>
    {
        private const string CustomerColumns = @"
            TCCID, ContactID, CompanyName, ContactFirstName, ContactAltFirstName, AreaID,
            EmailAddress, AltEmailAddress, ContactTypeID, EquipTypeID, TypicallySecToo,
            PreferredAgentID, SalesAgentID, UsesFilter, Enabled, AlwaysSendChkUp, ReminderCount,
            NextPreparationDate, NextDeliveryDate, NextCoffee, NextClean, NextFilter, NextDescal,
            NextService, RequiresPurchOrder";

        protected override string TableName => "TempCoffeecheckupCustomerTbl";
        protected override string KeyColumn => "TCCID";
        protected override string CoreColumns => CustomerColumns;

        public List<ContactToRemindDetails> GetAllContacts(string sortBy = null)
        {
            string sql = $"SELECT {CustomerColumns} FROM TempCoffeecheckupCustomerTbl";
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                sql += " ORDER BY " + MapCustomerSortColumn(sortBy);
            }
            else
            {
                sql += " ORDER BY CompanyName";
            }

            return MapContactList(sql, null);
        }

        public List<ContactToRemindWithItems> GetAllContactAndItems(string sortBy = null)
        {
            var contacts = GetAllContacts(sortBy);
            var result = new List<ContactToRemindWithItems>();

            foreach (var contact in contacts)
            {
                var withItems = MapToWithItems(contact);
                withItems.ItemsContactRequires = GetContactItems(contact.CustomerID);
                result.Add(withItems);
            }

            return result;
        }

        public List<ItemContactRequires> GetContactItems(long contactId, string sortBy = null)
        {
            string sql = @"
                SELECT TCIID, ContactID, ItemID, ItemQty, ItemPrepID, ItemPackagingID, AutoFulfill, RecurringOrderItemID
                FROM TempCoffeecheckupItemsTbl
                WHERE ContactID = @ContactID";

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                sql += " ORDER BY " + (sortBy.Contains("ItemID") ? "ItemID" : "ItemID");
            }

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 }
            };

            var list = new List<ItemContactRequires>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    int recurringItemId = GetInt(rdr, "RecurringOrderItemID");
                    list.Add(new ItemContactRequires
                    {
                        TCIID = GetInt(rdr, "TCIID"),
                        CustomerID = GetInt(rdr, "ContactID"),
                        ItemID = GetInt(rdr, "ItemID"),
                        ItemQty = GetDouble(rdr, "ItemQty"),
                        ItemPrepID = GetInt(rdr, "ItemPrepID"),
                        ItemPackagID = GetInt(rdr, "ItemPackagingID"),
                        AutoFulfill = GetBool(rdr, "AutoFulfill"),
                        RecurringOrderItemID = recurringItemId,
                        RecurringOrder = recurringItemId > 0
                    });
                }
            }

            return list;
        }

        public bool InsertContact(ContactToRemindDetails contact)
        {
            if (contact == null) throw new ArgumentNullException(nameof(contact));

            const string sql = @"
                INSERT INTO TempCoffeecheckupCustomerTbl
                (ContactID, CompanyName, ContactFirstName, ContactAltFirstName, AreaID, EmailAddress, AltEmailAddress,
                 ContactTypeID, EquipTypeID, TypicallySecToo, PreferredAgentID, SalesAgentID, UsesFilter, Enabled,
                 AlwaysSendChkUp, ReminderCount, NextPreparationDate, NextDeliveryDate, NextCoffee, NextClean,
                 NextFilter, NextDescal, NextService, RequiresPurchOrder)
                VALUES
                (@ContactID, @CompanyName, @ContactFirstName, @ContactAltFirstName, @AreaID, @EmailAddress, @AltEmailAddress,
                 @ContactTypeID, @EquipTypeID, @TypicallySecToo, @PreferredAgentID, @SalesAgentID, @UsesFilter, @Enabled,
                 @AlwaysSendChkUp, @ReminderCount, @NextPreparationDate, @NextDeliveryDate, @NextCoffee, @NextClean,
                 @NextFilter, @NextDescal, @NextService, @RequiresPurchOrder)";

            return ExecNonQuery(sql, BuildContactParameters(contact)) > 0;
        }

        public bool InsertContacts(ContactToRemindDetails contact) => InsertContact(contact);

        public bool InsertContactItems(ItemContactRequires item) => InsertContactItem(item);

        public bool InsertContactItem(ItemContactRequires item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            const string sql = @"
                INSERT INTO TempCoffeecheckupItemsTbl
                (ContactID, ItemID, ItemQty, ItemPrepID, ItemPackagingID, AutoFulfill, RecurringOrderItemID)
                VALUES
                (@ContactID, @ItemID, @ItemQty, @ItemPrepID, @ItemPackagingID, @AutoFulfill, @RecurringOrderItemID)";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = item.CustomerID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemID", DataValue = item.ItemID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemQty", DataValue = item.ItemQty, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@ItemPrepID", DataValue = FkOrDbNull(item.ItemPrepID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemPackagingID", DataValue = FkOrDbNull(item.ItemPackagID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@AutoFulfill", DataValue = item.AutoFulfill, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@RecurringOrderItemID", DataValue = FkOrDbNull(item.RecurringOrderItemID), DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool DeleteContactItems(long contactId)
        {
            const string sql = "DELETE FROM TempCoffeecheckupItemsTbl WHERE ContactID = @ContactID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 }
            };
            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool DeleteContact(long contactId)
        {
            DeleteContactItems(contactId);
            const string sql = "DELETE FROM TempCoffeecheckupCustomerTbl WHERE ContactID = @ContactID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 }
            };
            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool DeleteAllContactRecords()
        {
            return ExecNonQuery("DELETE FROM TempCoffeecheckupCustomerTbl") >= 0;
        }

        public bool DeleteAllContactItems()
        {
            return ExecNonQuery("DELETE FROM TempCoffeecheckupItemsTbl") >= 0;
        }

        public bool UpdateContactDates(int contactId, DateTime nextPrep, DateTime nextDelivery)
        {
            const string sql = @"
                UPDATE TempCoffeecheckupCustomerTbl
                SET NextPreparationDate = @NextPreparationDate, NextDeliveryDate = @NextDeliveryDate
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@NextPreparationDate", DataValue = nextPrep.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextDeliveryDate", DataValue = nextDelivery.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) >= 0;
        }

        private List<ContactToRemindDetails> MapContactList(string sql, List<DBParameter> parameters)
        {
            var list = new List<ContactToRemindDetails>();
            using (var rdr = parameters == null ? ExecReader(sql) : ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(MapContact(rdr));
                }
            }

            return list;
        }

        private static ContactToRemindDetails MapContact(IDataReader rdr)
        {
            return new ContactToRemindDetails
            {
                TCCID = GetInt(rdr, "TCCID"),
                CustomerID = GetInt(rdr, "ContactID"),
                CompanyName = GetString(rdr, "CompanyName"),
                ContactFirstName = GetString(rdr, "ContactFirstName"),
                ContactAltFirstName = GetString(rdr, "ContactAltFirstName"),
                AreaID = GetInt(rdr, "AreaID"),
                EmailAddress = GetString(rdr, "EmailAddress"),
                AltEmailAddress = GetString(rdr, "AltEmailAddress"),
                CustomerTypeID = GetInt(rdr, "ContactTypeID"),
                EquipTypeID = GetInt(rdr, "EquipTypeID"),
                TypicallySecToo = GetBool(rdr, "TypicallySecToo"),
                PreferredAgentID = GetInt(rdr, "PreferredAgentID"),
                SalesAgentID = GetInt(rdr, "SalesAgentID"),
                UsesFilter = GetBool(rdr, "UsesFilter"),
                enabled = GetBool(rdr, "Enabled"),
                AlwaysSendChkUp = GetBool(rdr, "AlwaysSendChkUp"),
                ReminderCount = GetInt(rdr, "ReminderCount"),
                NextPreparationDate = GetDate(rdr, "NextPreparationDate"),
                NextDeliveryDate = GetDate(rdr, "NextDeliveryDate"),
                NextCoffee = GetDate(rdr, "NextCoffee"),
                NextClean = GetDate(rdr, "NextClean"),
                NextFilter = GetDate(rdr, "NextFilter"),
                NextDescal = GetDate(rdr, "NextDescal"),
                NextService = GetDate(rdr, "NextService"),
                RequiresPurchOrder = GetBool(rdr, "RequiresPurchOrder")
            };
        }

        private static ContactToRemindWithItems MapToWithItems(ContactToRemindDetails contact)
        {
            return new ContactToRemindWithItems
            {
                TCCID = contact.TCCID,
                CustomerID = contact.CustomerID,
                CompanyName = contact.CompanyName,
                ContactTitle = contact.ContactTitle,
                ContactFirstName = contact.ContactFirstName,
                ContactAltFirstName = contact.ContactAltFirstName,
                AreaID = contact.AreaID,
                EmailAddress = contact.EmailAddress,
                AltEmailAddress = contact.AltEmailAddress,
                CustomerTypeID = contact.CustomerTypeID,
                EquipTypeID = contact.EquipTypeID,
                TypicallySecToo = contact.TypicallySecToo,
                PreferredAgentID = contact.PreferredAgentID,
                SalesAgentID = contact.SalesAgentID,
                UsesFilter = contact.UsesFilter,
                autofulfill = contact.autofulfill,
                enabled = contact.enabled,
                AlwaysSendChkUp = contact.AlwaysSendChkUp,
                ReminderCount = contact.ReminderCount,
                Notes = contact.Notes,
                RequiresPurchOrder = contact.RequiresPurchOrder,
                LastDateSentReminder = contact.LastDateSentReminder,
                NextPreparationDate = contact.NextPreparationDate,
                NextDeliveryDate = contact.NextDeliveryDate,
                NextCoffee = contact.NextCoffee,
                NextClean = contact.NextClean,
                NextFilter = contact.NextFilter,
                NextDescal = contact.NextDescal,
                NextService = contact.NextService
            };
        }

        private static List<DBParameter> BuildContactParameters(ContactToRemindDetails contact)
        {
            // Optional FKs: Access often stored 0 for "none". SQL Server FKs reject 0 — send NULL.
            return new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contact.CustomerID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@CompanyName", DataValue = contact.CompanyName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactFirstName", DataValue = contact.ContactFirstName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactAltFirstName", DataValue = contact.ContactAltFirstName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AreaID", DataValue = FkOrDbNull(contact.AreaID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@EmailAddress", DataValue = contact.EmailAddress ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AltEmailAddress", DataValue = contact.AltEmailAddress ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactTypeID", DataValue = FkOrDbNull(contact.CustomerTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@EquipTypeID", DataValue = FkOrDbNull(contact.EquipTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@TypicallySecToo", DataValue = contact.TypicallySecToo, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@PreferredAgentID", DataValue = FkOrDbNull(contact.PreferredAgentID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SalesAgentID", DataValue = FkOrDbNull(contact.SalesAgentID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@UsesFilter", DataValue = contact.UsesFilter, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Enabled", DataValue = contact.enabled, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@AlwaysSendChkUp", DataValue = contact.AlwaysSendChkUp, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@ReminderCount", DataValue = contact.ReminderCount, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@NextPreparationDate", DataValue = contact.NextPreparationDate, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextDeliveryDate", DataValue = contact.NextDeliveryDate, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextCoffee", DataValue = contact.NextCoffee, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextClean", DataValue = contact.NextClean, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextFilter", DataValue = contact.NextFilter, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextDescal", DataValue = contact.NextDescal, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextService", DataValue = contact.NextService, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@RequiresPurchOrder", DataValue = contact.RequiresPurchOrder, DataDbType = DbType.Boolean }
            };
        }

        private static string MapCustomerSortColumn(string sortBy)
        {
            if (sortBy.Contains("CustomerID") || sortBy.Contains("ContactID"))
            {
                return "ContactID";
            }

            return "CompanyName";
        }

        private static string GetString(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? string.Empty : Convert.ToString(rdr[name]);
        }

        private static int GetInt(IDataReader rdr, string name)
        {
            return rdr[name] == DBNull.Value ? 0 : Convert.ToInt32(rdr[name]);
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
            return rdr[name] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(rdr[name]).Date;
        }
    }
}
