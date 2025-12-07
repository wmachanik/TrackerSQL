using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes; // use SQL DB

namespace TrackerDotNet.Classes.Sql
{
    /// <summary>
    /// Repository that returns lightweight ContactSummary rows for list page.
    /// Uses new SQL schema (ContactsTbl / PeopleTbl / AreasTbl / EquipTypesTbl) while supporting legacy UI filter keys.
    /// </summary>
    public class ContactSummariesRepository
    {
        // UI dropdown values (legacy) mapped to current table/column expressions
        private static readonly Dictionary<string,string> ColumnMap = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)
        {
            {"CompanyName","c.CompanyName"},
            {"ContactFirstName","c.ContactFirstName"},
            {"EmailAddress","c.EmailAddress"},
            {"PersonsTbl.Abbreviation","p.Abbreviation"}, // legacy misspelling retained in UI
            {"CityTbl.City","a.Area"},                    // legacy 'City' now 'Area'
            {"EquipTypeTbl.EquipTypeName","e.EquipTypeName"},
            {"CustomersTbl.MachineSN","c.MachineSN"},      // legacy CustomersTbl reference
            {"CustomerID","c.ContactID"}                   // legacy alias
        };

        /// <summary>
        /// Returns filtered & sorted contact summaries.
        /// </summary>
        public List<ContactSummary> GetAllContactSummaries(string sortBy, int isEnabled, string whereFilter)
        {
            var list = new List<ContactSummary>();
            using (var db = new TrackerSQLDb())
            {
                string sql = @"SELECT c.ContactID, c.CompanyName, c.ContactFirstName, c.ContactLastName,
                                  a.Area AS City, c.PhoneNumber, c.EmailAddress, p.Abbreviation AS DeliveryBy,
                                  e.EquipTypeName, c.MachineSN, c.autofulfill, c.enabled
                               FROM ContactsTbl c
                               LEFT OUTER JOIN PeopleTbl p ON c.PreferedAgentID = p.PersonID
                               LEFT OUTER JOIN AreasTbl a ON c.Area = a.AreaID
                               LEFT OUTER JOIN EquipTypesTbl e ON c.EquipTypeID = e.EquipTypeID";

                var whereParts = new List<string>();
                if (isEnabled == 0) whereParts.Add("c.enabled = 0");
                else if (isEnabled == 1) whereParts.Add("c.enabled = 1");

                if (!string.IsNullOrWhiteSpace(whereFilter))
                {
                    var parsed = ParseFilter(whereFilter); // (column, operator, value)
                    if (parsed != null && ColumnMap.TryGetValue(parsed.Item1, out string mappedCol))
                    {
                        whereParts.Add($"{mappedCol} {parsed.Item2} {parsed.Item3}");
                    }
                }

                if (whereParts.Count > 0)
                    sql += " WHERE " + string.Join(" AND ", whereParts);

                if (!string.IsNullOrWhiteSpace(sortBy) && ColumnMap.TryGetValue(sortBy, out string sortCol))
                    sql += " ORDER BY " + sortCol;
                else
                    sql += " ORDER BY c.CompanyName";

                try
                {
                    using (var rdr = db.ExecuteReader(sql))
                    {
                        while (rdr != null && rdr.Read())
                        {
                            list.Add(new ContactSummary
                            {
                                CustomerID = rdr["ContactID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ContactID"]),
                                CompanyName = rdr["CompanyName"] as string ?? string.Empty,
                                ContactFirstName = rdr["ContactFirstName"] as string ?? string.Empty,
                                ContactLastName = rdr["ContactLastName"] as string ?? string.Empty,
                                City = rdr["City"] as string ?? string.Empty,
                                PhoneNumber = rdr["PhoneNumber"] as string ?? string.Empty,
                                EmailAddress = rdr["EmailAddress"] as string ?? string.Empty,
                                DeliveryBy = rdr["DeliveryBy"] as string ?? string.Empty,
                                EquipTypeName = rdr["EquipTypeName"] as string ?? string.Empty,
                                MachineSN = rdr["MachineSN"] as string ?? string.Empty,
                                autofulfill = rdr["autofulfill"] != DBNull.Value && Convert.ToBoolean(rdr["autofulfill"]),
                                enabled = rdr["enabled"] != DBNull.Value && Convert.ToBoolean(rdr["enabled"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, "ContactSummariesRepository SQL error: " + ex.Message + " | SQL: " + sql);
                }
            }
            return list;
        }

        // Very small parser for expressions like: Column LIKE '%value%' OR Column = 123
        private Tuple<string,string,string> ParseFilter(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            raw = raw.Trim();
            int likeIdx = raw.IndexOf(" LIKE ", StringComparison.OrdinalIgnoreCase);
            if (likeIdx > 0)
            {
                string col = raw.Substring(0, likeIdx).Trim();
                string val = raw.Substring(likeIdx + 6).Trim();
                return Tuple.Create(col, "LIKE", val);
            }
            int eqIdx = raw.IndexOf("=", StringComparison.OrdinalIgnoreCase);
            if (eqIdx > 0)
            {
                string col = raw.Substring(0, eqIdx).Trim();
                string val = raw.Substring(eqIdx + 1).Trim();
                return Tuple.Create(col, "=", val);
            }
            return null;
        }
    }
}
