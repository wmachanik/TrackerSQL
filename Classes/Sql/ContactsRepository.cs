using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerDotNet.Classes;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ContactsRepository
    {
        public Contact GetById(int id)
        {
            string sql = "SELECT * FROM ContactsTbl WHERE ContactID = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read()) return Map(rdr);
            }
            return null;
        }

        public List<Contact> GetAll()
        {
            var list = new List<Contact>();
            string sql = "SELECT * FROM ContactsTbl";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(Map(rdr));
            }
            return list;
        }

        private static T GetValue<T>(IDataRecord r, string name)
        {
            if (!HasColumn(r, name)) return default(T);
            object val = r[name];
            if (val == null || val == DBNull.Value) return default(T);
            try
            {
                if (typeof(T) == typeof(string)) return (T)(object)Convert.ToString(val);
                if (typeof(T) == typeof(int) || typeof(T) == typeof(int?)) return (T)(object)Convert.ToInt32(val);
                if (typeof(T) == typeof(long) || typeof(T) == typeof(long?)) return (T)(object)Convert.ToInt64(val);
                if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?)) return (T)(object)Convert.ToBoolean(val);
                if (typeof(T) == typeof(double) || typeof(T) == typeof(double?)) return (T)(object)Convert.ToDouble(val);
                if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?)) return (T)(object)Convert.ToDateTime(val);
                return (T)val;
            }
            catch { return default(T); }
        }

        private static bool HasColumn(IDataRecord r, string name)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (string.Equals(r.GetName(i), name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private Contact Map(IDataReader r)
        {
            // Align with TableMigrationReport: ContactsTbl column names
            var c = new Contact
            {
                ContactID = GetValue<int>(r, "ContactID"),
                CompanyName = GetValue<string>(r, "CompanyName"),
                ContactTitle = GetValue<string>(r, "ContactTitle"),
                ContactFirstName = GetValue<string>(r, "ContactFirstName"),
                ContactLastName = GetValue<string>(r, "ContactLastName"),
                ContactAltFirstName = GetValue<string>(r, "ContactAltFirstName"),
                ContactAltLastName = GetValue<string>(r, "ContactAltLastName"),
                Department = GetValue<string>(r, "Department"),
                BillingAddress = GetValue<string>(r, "BillingAddress"),
                // Area can be stored as Area, AreaID or City (legacy). Try all.
                Area = GetValue<int?>(r, "Area") ?? GetValue<int?>(r, "AreaID") ?? GetValue<int?>(r, "City"),
                StateOrProvince = GetValue<string>(r, "StateOrProvince"),
                PostalCode = GetValue<string>(r, "PostalCode"),
                CountryOrRegion = GetValue<string>(r, "CountryOrRegion"),
                PhoneNumber = GetValue<string>(r, "PhoneNumber"),
                Extension = GetValue<string>(r, "Extension"),
                FaxNumber = GetValue<string>(r, "FaxNumber"),
                CellNumber = GetValue<string>(r, "CellNumber"),
                EmailAddress = GetValue<string>(r, "EmailAddress"),
                AltEmailAddress = GetValue<string>(r, "AltEmailAddress"),
                ContractNo = GetValue<string>(r, "ContractNo"),
                ContactTypeID = GetValue<int?>(r, "ContactTypeID"),
                EquipTypeID = GetValue<int?>(r, "EquipTypeID"),
                ItemPrefID = GetValue<int?>(r, "ItemPrefID"),
                PriPrefQty = GetValue<double?>(r, "PriPrefQty"),
                PrefItemPrepTypeID = GetValue<int?>(r, "PrefItemPrepTypeID"),
                PrefItemPackagingID = GetValue<int?>(r, "PrefItemPackagingID"),
                SecondaryItemPrefID = GetValue<int?>(r, "SecondaryItemPrefID"),
                SecPrefQty = GetValue<double?>(r, "SecPrefQty"),
                TypicallySecToo = GetValue<bool?>(r, "TypicallySecToo"),
                PreferedAgentID = GetValue<int?>(r, "PreferedAgentID"),
                SalesAgentID = GetValue<int?>(r, "SalesAgentID"),
                MachineSN = GetValue<string>(r, "MachineSN"),
                UsesFilter = GetValue<bool?>(r, "UsesFilter"),
                AutoFulfill = GetValue<bool?>(r, "AutoFulfill"),
                Enabled = GetValue<bool?>(r, "Enabled"),
                PredictionDisabled = GetValue<bool?>(r, "PredictionDisabled"),
                AlwaysSendChkUp = GetValue<bool?>(r, "AlwaysSendChkUp"),
                NormallyResponds = GetValue<bool?>(r, "NormallyResponds"),
                ReminderCount = GetValue<int?>(r, "ReminderCount"),
                Notes = GetValue<string>(r, "Notes"),
                SendDeliveryConfirmation = GetValue<bool?>(r, "SendDeliveryConfirmation"),
                LastDateSentReminder = GetValue<DateTime?>(r, "LastDateSentReminder")
            };
            return c;
        }
    }
}
