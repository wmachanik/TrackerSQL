using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Models;
using TrackerSQL.Classes;
using static TrackerSQL.Classes.DbParamHelpers;

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

        /// <summary>Enabled contacts with blank/null PostalCode (for backfill tool).</summary>
        public List<Contact> GetMissingPostalCodes(bool enabledOnly = true)
        {
            var list = new List<Contact>();
            string sql = @"
SELECT *
FROM ContactsTbl
WHERE (PostalCode IS NULL OR LTRIM(RTRIM(PostalCode)) = N'')
  AND (@EnabledOnly = 0 OR ISNULL(Enabled, 1) = 1)
ORDER BY CompanyName, ContactID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@EnabledOnly", DataValue = enabledOnly ? 1 : 0, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                while (rdr != null && rdr.Read())
                    list.Add(Map(rdr));
            }
            return list;
        }

        public bool UpdatePostalCode(int contactId, string postalCode, string noteMessage = null)
        {
            if (contactId <= 0)
                return false;
            string date = TimeZoneUtils.Now().ToString("yyyy-MM-dd");
            string noteLine = date + ": "
                + (string.IsNullOrWhiteSpace(noteMessage)
                    ? "Postal code " + postalCode + " assigned from Contact postal fill"
                    : noteMessage.Trim())
                + "\n";
            const string sql = @"
UPDATE ContactsTbl
SET PostalCode = @PostalCode,
    Notes = @Notes + ISNULL(Notes, '')
WHERE ContactID = @ContactID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@PostalCode", DataValue = (object)postalCode ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Notes", DataValue = noteLine, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };
            return ExecNonQuery(sql, parameters) > 0;
        }

        private static T GetValue<T>(IDataRecord r, string name)
        {
            if (!DbMapper.HasColumn(r, name)) return default(T);
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
                CountryOrRegion = GetValue<string>(r, "CountryOrRegion") ?? GetValue<string>(r, "Country/Region"),
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
                           MAX(COALESCE(O.RequiredByDate, O.OrderDate)) AS LastOrderDate 
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
        /// Disables all enabled contacts with no orders since the cutoff date,
        /// and prepends a dated note to each contact's Notes.
        /// </summary>
        /// <param name="cutoffDate">Cutoff date - contacts with no orders on/after this date will be disabled</param>
        /// <returns>Number of contacts disabled</returns>
        public int DisableInactiveContacts(DateTime cutoffDate)
        {
            string notePrefix = $"{TimeZoneUtils.Now():yyyy-MM-dd}: Contact disabled — inactive "
                + $"(no orders since {cutoffDate:yyyy-MM-dd})\n";

            string sql = @"
                UPDATE ContactsTbl
                SET Enabled = 0,
                    Notes = @Notes + ISNULL(Notes, '')
                WHERE Enabled = 1
                  AND NOT EXISTS (
                      SELECT 1
                      FROM OrdersTbl AS O
                      WHERE O.ContactID = ContactsTbl.ContactID
                        AND COALESCE(O.RequiredByDate, O.OrderDate) >= @CutoffDate
                  )";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Notes", DataValue = notePrefix, DataDbType = DbType.String },
                new DBParameter { ParamName = "@CutoffDate", DataValue = cutoffDate, DataDbType = DbType.Date }
            };

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }

        /// <summary>
        /// Prepends a dated system note to ContactsTbl.Notes (newest first).
        /// Used whenever the system changes account type, prediction, or enabled status.
        /// </summary>
        public bool AppendSystemNote(int contactId, string message)
        {
            if (contactId <= 0 || string.IsNullOrWhiteSpace(message))
                return false;

            string noteLine = $"{TimeZoneUtils.Now():yyyy-MM-dd}: {message.Trim()}\n";
            const string sql = @"
                UPDATE ContactsTbl
                SET Notes = @Notes + ISNULL(Notes, '')
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Notes", DataValue = noteLine, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public Contact GetById(long id) => GetById((int)id);

        /// <summary>
        /// Updates editable contact fields on ContactsTbl. Preserves ReminderCount / LastDateSentReminder.
        /// </summary>
        public bool Update(Contact contact)
        {
            if (contact == null || contact.ContactID <= 0)
                return false;

            const string sql = @"
                UPDATE ContactsTbl SET
                    CompanyName = @CompanyName,
                    ContactTitle = @ContactTitle,
                    ContactFirstName = @ContactFirstName,
                    ContactLastName = @ContactLastName,
                    ContactAltFirstName = @ContactAltFirstName,
                    ContactAltLastName = @ContactAltLastName,
                    Department = @Department,
                    BillingAddress = @BillingAddress,
                    AreaID = @AreaID,
                    StateOrProvince = @StateOrProvince,
                    PostalCode = @PostalCode,
                    [Country/Region] = @CountryOrRegion,
                    PhoneNumber = @PhoneNumber,
                    Extension = @Extension,
                    FaxNumber = @FaxNumber,
                    CellNumber = @CellNumber,
                    EmailAddress = @EmailAddress,
                    AltEmailAddress = @AltEmailAddress,
                    ContractNo = @ContractNo,
                    ContactTypeID = @ContactTypeID,
                    EquipTypeID = @EquipTypeID,
                    ItemPrefID = @ItemPrefID,
                    PriPrefQty = @PriPrefQty,
                    PrefItemPrepTypeID = @PrefItemPrepTypeID,
                    PrefItemPackagingID = @PrefItemPackagingID,
                    SecondaryItemPrefID = @SecondaryItemPrefID,
                    SecPrefQty = @SecPrefQty,
                    TypicallySecToo = @TypicallySecToo,
                    PreferredAgentID = @PreferredAgentID,
                    SalesAgentID = @SalesAgentID,
                    EquipentSN = @EquipentSN,
                    UsesFilter = @UsesFilter,
                    AutoFulfill = @AutoFulfill,
                    Enabled = @Enabled,
                    PredictionDisabled = @PredictionDisabled,
                    AlwaysSendChkUp = @AlwaysSendChkUp,
                    NormallyResponds = @NormallyResponds,
                    Notes = @Notes,
                    SendDeliveryConfirmation = @SendDeliveryConfirmation
                WHERE ContactID = @ContactID";

            return ExecNonQuery(sql, BuildContactParameters(contact, includeId: true)) > 0;
        }

        /// <summary>
        /// Inserts a new contact. Returns new ContactID, or 0 on failure.
        /// </summary>
        public int Insert(Contact contact)
        {
            if (contact == null)
                return 0;

            const string sql = @"
                INSERT INTO ContactsTbl (
                    CompanyName, ContactTitle, ContactFirstName, ContactLastName,
                    ContactAltFirstName, ContactAltLastName, Department, BillingAddress,
                    AreaID, StateOrProvince, PostalCode, [Country/Region],
                    PhoneNumber, Extension, FaxNumber, CellNumber,
                    EmailAddress, AltEmailAddress, ContractNo, ContactTypeID,
                    EquipTypeID, ItemPrefID, PriPrefQty, PrefItemPrepTypeID, PrefItemPackagingID,
                    SecondaryItemPrefID, SecPrefQty, TypicallySecToo,
                    PreferredAgentID, SalesAgentID, EquipentSN,
                    UsesFilter, AutoFulfill, Enabled, PredictionDisabled,
                    AlwaysSendChkUp, NormallyResponds, ReminderCount, Notes, SendDeliveryConfirmation
                ) VALUES (
                    @CompanyName, @ContactTitle, @ContactFirstName, @ContactLastName,
                    @ContactAltFirstName, @ContactAltLastName, @Department, @BillingAddress,
                    @AreaID, @StateOrProvince, @PostalCode, @CountryOrRegion,
                    @PhoneNumber, @Extension, @FaxNumber, @CellNumber,
                    @EmailAddress, @AltEmailAddress, @ContractNo, @ContactTypeID,
                    @EquipTypeID, @ItemPrefID, @PriPrefQty, @PrefItemPrepTypeID, @PrefItemPackagingID,
                    @SecondaryItemPrefID, @SecPrefQty, @TypicallySecToo,
                    @PreferredAgentID, @SalesAgentID, @EquipentSN,
                    @UsesFilter, @AutoFulfill, @Enabled, @PredictionDisabled,
                    @AlwaysSendChkUp, @NormallyResponds, @ReminderCount, @Notes, @SendDeliveryConfirmation
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return ExecuteScalar<int>(sql, BuildContactParameters(contact, includeId: false));
        }

        private static List<DBParameter> BuildContactParameters(Contact contact, bool includeId)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@CompanyName", DataValue = (object)contact.CompanyName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactTitle", DataValue = (object)contact.ContactTitle ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactFirstName", DataValue = (object)contact.ContactFirstName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactLastName", DataValue = (object)contact.ContactLastName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactAltFirstName", DataValue = (object)contact.ContactAltFirstName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactAltLastName", DataValue = (object)contact.ContactAltLastName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Department", DataValue = (object)contact.Department ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BillingAddress", DataValue = (object)contact.BillingAddress ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AreaID", DataValue = FkOrDbNull(contact.AreaID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@StateOrProvince", DataValue = (object)contact.StateOrProvince ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PostalCode", DataValue = (object)contact.PostalCode ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@CountryOrRegion", DataValue = (object)contact.CountryOrRegion ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PhoneNumber", DataValue = (object)contact.PhoneNumber ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Extension", DataValue = (object)contact.Extension ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@FaxNumber", DataValue = (object)contact.FaxNumber ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@CellNumber", DataValue = (object)contact.CellNumber ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@EmailAddress", DataValue = (object)contact.EmailAddress ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AltEmailAddress", DataValue = (object)contact.AltEmailAddress ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContractNo", DataValue = (object)contact.ContractNo ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactTypeID", DataValue = FkOrDbNull(contact.ContactTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@EquipTypeID", DataValue = FkOrDbNull(contact.EquipTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemPrefID", DataValue = FkOrDbNull(contact.ItemPrefID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PriPrefQty", DataValue = (object)contact.PriPrefQty ?? DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@PrefItemPrepTypeID", DataValue = FkOrDbNull(contact.PrefItemPrepTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PrefItemPackagingID", DataValue = FkOrDbNull(contact.PrefItemPackagingID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SecondaryItemPrefID", DataValue = FkOrDbNull(contact.SecondaryItemPrefID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SecPrefQty", DataValue = (object)contact.SecPrefQty ?? DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@TypicallySecToo", DataValue = (object)contact.TypicallySecToo ?? DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@PreferredAgentID", DataValue = FkOrDbNull(contact.PreferredAgentID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SalesAgentID", DataValue = FkOrDbNull(contact.SalesAgentID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@EquipentSN", DataValue = (object)contact.EquipentSN ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@UsesFilter", DataValue = (object)contact.UsesFilter ?? DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@AutoFulfill", DataValue = (object)contact.AutoFulfill ?? DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Enabled", DataValue = (object)contact.Enabled ?? DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@PredictionDisabled", DataValue = (object)contact.PredictionDisabled ?? DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@AlwaysSendChkUp", DataValue = (object)contact.AlwaysSendChkUp ?? DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@NormallyResponds", DataValue = (object)contact.NormallyResponds ?? DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = (object)contact.Notes ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SendDeliveryConfirmation", DataValue = (object)contact.SendDeliveryConfirmation ?? DBNull.Value, DataDbType = DbType.Boolean }
            };

            if (includeId)
            {
                parameters.Add(new DBParameter { ParamName = "@ContactID", DataValue = contact.ContactID, DataDbType = DbType.Int32 });
            }
            else
            {
                parameters.Add(new DBParameter { ParamName = "@ReminderCount", DataValue = contact.ReminderCount ?? 0, DataDbType = DbType.Int32 });
            }

            return parameters;
        }

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
        /// Always records a dated note on the contact.
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

            bool ok = ExecNonQuery(sql, parameters) > 0;
            if (ok)
            {
                AppendSystemNote(contactId, disableAll
                    ? "Contact disabled via email link (all reminders / contact)"
                    : "Prediction disabled via email link");
            }

            return ok;
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

        /// <summary>
        /// Sets only PredictionDisabled — used when recurring orders are added (disable prediction
        /// so the two systems don't conflict) or disabled (prediction may resume).
        /// When the flag actually changes, a dated note is prepended to ContactsTbl.Notes.
        /// </summary>
        /// <param name="reason">Optional context for the note, e.g. "recurring order added".</param>
        public bool SetPredictionDisabled(int contactId, bool predictionDisabled, string reason = null)
        {
            if (contactId <= 0)
                return false;

            var current = GetById(contactId);
            if (current == null)
                return false;

            bool wasDisabled = current.PredictionDisabled == true;

            const string sql = @"
                UPDATE ContactsTbl
                SET PredictionDisabled = @PredictionDisabled
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@PredictionDisabled", DataValue = predictionDisabled, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            bool ok = ExecNonQuery(sql, parameters) > 0;
            if (ok && wasDisabled != predictionDisabled)
            {
                string action = predictionDisabled ? "Prediction disabled" : "Prediction re-enabled";
                string note = string.IsNullOrWhiteSpace(reason) ? action : action + " — " + reason.Trim();
                AppendSystemNote(contactId, note);
            }

            return ok;
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
            return GetByContactNamePreferEnabled(contactName);
        }

        /// <summary>
        /// When duplicate company names exist, prefer the enabled contact.
        /// </summary>
        public Contact GetByContactNamePreferEnabled(string contactName)
        {
            const string sql = @"
                SELECT TOP 1 * FROM ContactsTbl
                WHERE CompanyName = @CompanyName
                ORDER BY CASE WHEN Enabled = 1 THEN 0 ELSE 1 END, ContactID";
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

        /// <summary>Exact match on primary or alternate email (case-insensitive, trimmed).</summary>
        public List<Contact> FindByEmailExact(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new List<Contact>();

            return SearchContacts(
                "LTRIM(RTRIM(EmailAddress)) = @Email OR LTRIM(RTRIM(AltEmailAddress)) = @Email",
                new DBParameter { ParamName = "@Email", DataValue = email.Trim(), DataDbType = DbType.String });
        }

        /// <summary>Find contacts by last name (primary, alternate, or company name contains).</summary>
        public List<Contact> SearchByLastName(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return new List<Contact>();

            string trimmed = lastName.Trim();
            string likePattern = $"%{trimmed}%";
            return SearchContacts(
                "ContactLastName = @Last OR ContactAltLastName = @Last OR CompanyName LIKE @Like",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Last", DataValue = trimmed, DataDbType = DbType.String },
                    new DBParameter { ParamName = "@Like", DataValue = likePattern, DataDbType = DbType.String }
                });
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
            return SearchContacts(whereClause, new List<DBParameter> { parameter });
        }

        private List<Contact> SearchContacts(string whereClause, IList<DBParameter> parameters)
        {
            var list = new List<Contact>();
            string sql = $"SELECT * FROM ContactsTbl WHERE {whereClause}";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters == null ? new List<DBParameter>() : new List<DBParameter>(parameters)))
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
