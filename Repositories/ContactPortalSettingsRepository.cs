using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactPortalSettingsRepository
    {
        public static readonly string[] AllowedFieldCatalog =
        {
            "CompanyName", "ContactTitle", "ContactFirstName", "ContactLastName",
            "ContactAltFirstName", "ContactAltLastName", "Department",
            "BillingAddress", "PostalCode", "StateOrProvince",
            "PhoneNumber", "CellNumber",
            "EmailAddress", "AltEmailAddress"
        };

        private static readonly Dictionary<string, string> FieldLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "CompanyName", "Company name" },
            { "ContactTitle", "Title" },
            { "ContactFirstName", "First name" },
            { "ContactLastName", "Last name" },
            { "ContactAltFirstName", "Alternate first name" },
            { "ContactAltLastName", "Alternate last name" },
            { "Department", "Department" },
            { "BillingAddress", "Billing address" },
            { "PostalCode", "Postal code" },
            { "StateOrProvince", "Province" },
            { "PhoneNumber", "Phone number" },
            { "CellNumber", "Cell number" },
            { "EmailAddress", "Email address" },
            { "AltEmailAddress", "Alternate email address" }
        };

        /// <summary>Plain-English label for a catalog field ("ContactFirstName" → "First name").</summary>
        public static string FieldLabel(string field)
        {
            return field != null && FieldLabels.TryGetValue(field, out string label) ? label : field;
        }

        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'ContactPortalSettingsTbl'") > 0;
            }
            catch
            {
                return false;
            }
        }

        public ContactPortalSettings Get()
        {
            var defaults = new ContactPortalSettings();
            if (!TableExists())
                return defaults;

            const string sql = @"
SELECT SettingsID, EditableContactFields, UpdatedAt, UpdatedBy
FROM ContactPortalSettingsTbl WHERE SettingsID = 1";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                if (rdr == null || !rdr.Read())
                    return defaults;

                return new ContactPortalSettings
                {
                    SettingsID = Convert.ToInt32(rdr["SettingsID"]),
                    EditableContactFields = rdr["EditableContactFields"] == DBNull.Value
                        ? defaults.EditableContactFields
                        : rdr["EditableContactFields"].ToString(),
                    UpdatedAt = Convert.ToDateTime(rdr["UpdatedAt"]),
                    UpdatedBy = rdr["UpdatedBy"] == DBNull.Value ? null : rdr["UpdatedBy"].ToString()
                };
            }
        }

        public bool Save(string editableFieldsCsv, string updatedBy)
        {
            if (!TableExists())
                return false;

            string normalized = NormalizeFields(editableFieldsCsv);
            const string sql = @"
IF EXISTS (SELECT 1 FROM ContactPortalSettingsTbl WHERE SettingsID = 1)
    UPDATE ContactPortalSettingsTbl
    SET EditableContactFields = @Fields, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy
    WHERE SettingsID = 1
ELSE
    INSERT INTO ContactPortalSettingsTbl (SettingsID, EditableContactFields, UpdatedBy)
    VALUES (1, @Fields, @UpdatedBy)";

            using (var db = new TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Fields", DataValue = normalized, DataDbType = DbType.String },
                    new DBParameter { ParamName = "@UpdatedBy", DataValue = (object)updatedBy ?? DBNull.Value, DataDbType = DbType.String }
                }) > 0;
            }
        }

        public static HashSet<string> ParseFields(string csv)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(csv))
                return set;
            foreach (string part in csv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string name = part.Trim();
                if (AllowedFieldCatalog.Any(f => f.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    set.Add(AllowedFieldCatalog.First(f => f.Equals(name, StringComparison.OrdinalIgnoreCase)));
            }
            return set;
        }

        public static string NormalizeFields(string csv)
        {
            return string.Join(",", ParseFields(csv).OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        }
    }
}
