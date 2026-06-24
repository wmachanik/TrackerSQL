using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Models;
using TrackerSQL.Classes;

namespace TrackerSQL.Repositories
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
                // Area can be stored as Area, AreaID or Area (legacy). Try all.
                AreaID = GetValue<int?>(r, "Area") ?? GetValue<int?>(r, "AreaID") ?? GetValue<int?>(r, "AreaID"),
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
                PreferredAgentID = GetValue<int?>(r, "PreferredAgentID") ?? GetValue<int?>(r, "PreferedAgentID"),
                SalesAgentID = GetValue<int?>(r, "SalesAgentID"),
                EquipentSN = GetValue<string>(r, "EquipentSN") ?? GetValue<string>(r, "MachineSN"),
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

        /// <summary>
        /// Gets all contacts for dropdown binding: enabled first by name, then disabled (prefixed with "_") at the bottom.
        /// </summary>
        public List<ContactLookup> GetAllCompanyNames()
        {
            var list = new List<ContactLookup>();
            const string sql = @"
                SELECT ContactID, CompanyName, Enabled
                FROM ContactsTbl
                ORDER BY Enabled DESC, CompanyName";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                {
                    string companyName = rdr["CompanyName"]?.ToString() ?? string.Empty;
                    bool? enabled = rdr["Enabled"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(rdr["Enabled"]);

                    list.Add(new ContactLookup
                    {
                        ContactID = Convert.ToInt32(rdr["ContactID"]),
                        CompanyName = LookupFormatter.FormatLookupText(companyName, enabled),
                        Enabled = enabled
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// Gets all enabled contacts that have no orders in 3+ years or never ordered
        /// </summary>
        /// <param name="cutoffDate">Cutoff date - contacts with last order before this date will be returned</param>
        /// <returns>List of inactive contacts with contact ID, company name, and last order date</returns>
        public List<InactiveContactResult> GetInactiveContacts(DateTime cutoffDate)
        {
            var list = new List<InactiveContactResult>();
            
            string sql = @"
                SELECT C.ContactID, C.CompanyName, X.LastOrderDate 
                FROM ContactsTbl AS C
                LEFT JOIN (
                    SELECT O.ContactID, 
                           MAX(IIF(O.RequiredByDate IS NOT NULL, O.RequiredByDate, O.OrderDate)) AS LastOrderDate 
                    FROM OrdersTbl AS O 
                    GROUP BY O.ContactID
                ) AS X ON X.ContactID = C.ContactID
                WHERE C.Enabled = 1 
                  AND (X.LastOrderDate IS NULL OR X.LastOrderDate < @CutoffDate)
                ORDER BY C.CompanyName";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter 
                { 
                    DataValue = cutoffDate, 
                    DataDbType = DbType.Date, 
                    ParamName = "@CutoffDate" 
                }
            };
            
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                while (rdr.Read())
                {
                    var lastOrderDate = rdr["LastOrderDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(rdr["LastOrderDate"]).Date;
                    
                    list.Add(new InactiveContactResult
                    {
                        ContactID = GetValue<int>(rdr, "ContactID"),
                        CompanyName = GetValue<string>(rdr, "CompanyName") ?? string.Empty,
                        LastOrderDate = lastOrderDate?.ToString("yyyy-MM-dd") ?? "(none)"
                    });
                }
            }
            
            return list;
        }

        /// <summary>
        /// Disables all enabled contacts with no orders since the cutoff date
        /// </summary>
        /// <param name="cutoffDate">Cutoff date - contacts with no orders on/after this date will be disabled</param>
        /// <returns>Number of contacts disabled</returns>
        public int DisableInactiveContacts(DateTime cutoffDate)
        {
            string sql = @"
                UPDATE ContactsTbl AS C 
                SET C.Enabled = 0
                WHERE C.Enabled = 1 
                  AND NOT EXISTS (
                      SELECT 1 FROM OrdersTbl AS O
                      WHERE O.ContactID = C.ContactID
                        AND IIF(O.RequiredByDate IS NOT NULL, O.RequiredByDate, O.OrderDate) >= @CutoffDate
                  )";
            
            var parameters = new List<DBParameter>
            {
                new DBParameter 
                { 
                    DataValue = cutoffDate, 
                    DataDbType = DbType.Date, 
                    ParamName = "@CutoffDate" 
                }
            };
            
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }

        public Contact GetById(long id) => GetById((int)id);

        public int GetReminderCount(int contactId)
        {
            return ExecuteScalar<int>(
                "SELECT ReminderCount FROM ContactsTbl WHERE ContactID = @ContactID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
                });
        }

        public bool IncrementReminderCount(int contactId)
        {
            const string sql = "UPDATE ContactsTbl SET ReminderCount = ReminderCount + 1 WHERE ContactID = @ContactID";
            return ExecNonQuery(sql, ContactIdParam(contactId)) > 0;
        }

        public bool SetSentReminderAndIncrementReminderCount(DateTime lastSentDate, int contactId)
        {
            const string sql = @"
                UPDATE ContactsTbl
                SET LastDateSentReminder = @LastDateSentReminder, ReminderCount = ReminderCount + 1
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@LastDateSentReminder", DataValue = lastSentDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool ResetReminderCount(int contactId, bool forceEnable = false)
        {
            string sql = forceEnable
                ? "UPDATE ContactsTbl SET ReminderCount = 0, Enabled = 1 WHERE ContactID = @ContactID"
                : "UPDATE ContactsTbl SET ReminderCount = 0 WHERE ContactID = @ContactID";

            return ExecNonQuery(sql, ContactIdParam(contactId)) > 0;
        }

        public bool DisableContact(int contactId, string notes)
        {
            const string sql = @"
                UPDATE ContactsTbl
                SET Enabled = 0, Notes = @Notes + ISNULL(Notes, '')
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Notes", DataValue = notes ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        /// <summary>
        /// Applies disable choice from the public email disable link (DisableClient.aspx).
        /// </summary>
        public bool ApplyEmailDisableChoice(int contactId, bool disableAll)
        {
            string sql = disableAll
                ? @"UPDATE ContactsTbl SET Enabled = @Enabled, PredictionDisabled = @PredictionDisabled, AlwaysSendChkUp = @AlwaysSendChkUp WHERE ContactID = @ContactID"
                : @"UPDATE ContactsTbl SET PredictionDisabled = @PredictionDisabled, AlwaysSendChkUp = @AlwaysSendChkUp WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@PredictionDisabled", DataValue = true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@AlwaysSendChkUp", DataValue = false, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            if (disableAll)
            {
                parameters.Insert(0, new DBParameter { ParamName = "@Enabled", DataValue = false, DataDbType = DbType.Boolean });
            }

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool DisableContactReminders(int contactId, string notes)
        {
            const string sql = @"
                UPDATE ContactsTbl
                SET PredictionDisabled = 1, ReminderCount = 0, LastDateSentReminder = NULL,
                    Notes = @Notes + ISNULL(Notes, '')
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Notes", DataValue = notes ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool DisableContactIfReminderTooHigh(int contactId, int reminderThreshold)
        {
            const string sql = @"
                UPDATE ContactsTbl
                SET Enabled = 0, Notes = @Notes + ISNULL(Notes, '')
                WHERE ContactID = @ContactID AND ReminderCount > @ReminderThreshold";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Notes", DataValue = $"Contact set to disabled: {TimeZoneUtils.Now():d}\n", DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ReminderThreshold", DataValue = reminderThreshold, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool SetEquipmentIfEmpty(int equipTypeId, string equipmentSn, int contactId)
        {
            var current = GetById(contactId);
            if (current == null) return false;

            int equipToSet = equipTypeId;
            string snToSet = equipmentSn;
            bool needsUpdate = false;

            if (equipTypeId > 0 && (current.EquipTypeID ?? 0) == 0)
            {
                needsUpdate = true;
            }
            else if ((current.EquipTypeID ?? 0) > 0)
            {
                equipToSet = current.EquipTypeID.Value;
            }

            if (!string.IsNullOrEmpty(equipmentSn) && string.IsNullOrEmpty(current.EquipentSN))
            {
                needsUpdate = true;
            }
            else if (!string.IsNullOrEmpty(current.EquipentSN))
            {
                snToSet = current.EquipentSN;
            }

            if (!needsUpdate) return true;

            const string sql = @"
                UPDATE ContactsTbl SET EquipTypeID = @EquipTypeID, EquipentSN = @EquipentSN
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@EquipTypeID", DataValue = equipToSet, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@EquipentSN", DataValue = snToSet ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) >= 0;
        }

        public Contact GetByContactName(string contactName)
        {
            const string sql = "SELECT * FROM ContactsTbl WHERE CompanyName = @CompanyName";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@CompanyName", DataValue = contactName, DataDbType = DbType.String }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                if (rdr != null && rdr.Read()) return Map(rdr);
            }

            return null;
        }

        public List<Contact> SearchByContactNameLike(string namePattern)
        {
            if (string.IsNullOrWhiteSpace(namePattern)) return new List<Contact>();
            if (!namePattern.Contains("%")) namePattern = $"%{namePattern}%";

            return SearchContacts(
                "CompanyName LIKE @Pattern",
                new DBParameter { ParamName = "@Pattern", DataValue = namePattern, DataDbType = DbType.String });
        }

        public List<Contact> SearchByEmailLike(string emailPattern)
        {
            if (string.IsNullOrWhiteSpace(emailPattern)) return new List<Contact>();
            if (!emailPattern.Contains("%")) emailPattern = $"{emailPattern}%";

            return SearchContacts(
                "EmailAddress LIKE @Pattern OR AltEmailAddress LIKE @Pattern",
                new DBParameter { ParamName = "@Pattern", DataValue = emailPattern, DataDbType = DbType.String });
        }

        public string GetContactNameById(int contactId)
        {
            return ExecuteScalar<string>(
                "SELECT CompanyName FROM ContactsTbl WHERE ContactID = @ContactID",
                ContactIdParam(contactId)) ?? string.Empty;
        }

        public ContactEmailDetails GetContactEmailDetails(int contactId)
        {
            var contact = GetById(contactId);
            if (contact == null)
            {
                return new ContactEmailDetails();
            }

            return new ContactEmailDetails
            {
                FirstName = contact.ContactFirstName ?? string.Empty,
                LastName = contact.ContactLastName ?? string.Empty,
                EmailAddress = contact.EmailAddress ?? string.Empty,
                altFirstName = contact.ContactAltFirstName ?? string.Empty,
                altLastName = contact.ContactAltLastName ?? string.Empty,
                altEmailAddress = contact.AltEmailAddress ?? string.Empty
            };
        }

        public bool UpdateContactTypeIfInfoOnly(int contactId, int contactTypeId)
        {
            const string sql = @"
                UPDATE ContactsTbl SET ContactTypeID = @ContactTypeID
                WHERE ContactID = @ContactID AND ContactTypeID = 9";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactTypeID", DataValue = contactTypeId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) >= 0;
        }

        private List<Contact> SearchContacts(string whereClause, DBParameter parameter)
        {
            var list = new List<Contact>();
            string sql = $"SELECT * FROM ContactsTbl WHERE {whereClause}";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, new List<DBParameter> { parameter }))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(Map(rdr));
                }
            }

            return list;
        }

        private int ExecNonQuery(string sql, List<DBParameter> parameters)
        {
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }

        private T ExecuteScalar<T>(string sql, List<DBParameter> parameters)
        {
            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteScalar<T>(sql, parameters);
            }
        }

        private static List<DBParameter> ContactIdParam(int contactId)
        {
            return new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };
        }
    }

    /// <summary>
    /// Lightweight class for dropdown binding (ContactID + CompanyName only)
    /// </summary>
    public class ContactLookup
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public bool? Enabled { get; set; }
    }
}
