using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooPaymentMethodMapRepository : RepositoryBase<WooPaymentMethodMap>
    {
        protected override string TableName => "WooPaymentMethodMapTbl";
        protected override string KeyColumn => "MapID";

        public bool TableExists()
        {
            try
            {
                using (var db = CreateDb())
                    return db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'WooPaymentMethodMapTbl'") > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<WooPaymentMethodMap> GetAllOrdered(bool includeInactive = true)
        {
            if (!TableExists())
                return new List<WooPaymentMethodMap>();

            var list = new List<WooPaymentMethodMap>();
            string sql = "SELECT * FROM WooPaymentMethodMapTbl";
            if (!includeInactive)
                sql += " WHERE IsActive = 1";
            sql += " ORDER BY MethodMatch, MapID";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(MapRow(rdr));
            }
            return list;
        }

        /// <summary>Match Woo payment method id/title; fallback abbrev max 4 chars from title.</summary>
        public string ResolveAbbrev(string paymentMethod, string paymentMethodTitle)
        {
            return ResolveAbbrev(paymentMethod, paymentMethodTitle, null);
        }

        /// <summary>Match Woo payment method id/title; prefer exact id, then longest substring match.</summary>
        public string ResolveAbbrev(string paymentMethod, string paymentMethodTitle, IList<WooPaymentMethodMap> maps)
        {
            string method = (paymentMethod ?? string.Empty).Trim();
            string title = (paymentMethodTitle ?? string.Empty).Trim();
            string hay = (method + " " + title).Trim();
            if (string.IsNullOrEmpty(hay))
                return "Woo";

            List<WooPaymentMethodMap> list = (maps ?? GetAllOrdered(includeInactive: false))
                .Where(m => m != null && m.IsActive && !string.IsNullOrWhiteSpace(m.MethodMatch))
                .OrderByDescending(m => m.MethodMatch.Trim().Length)
                .ToList();

            // Exact match on Woo payment_method id (e.g. payfast).
            foreach (WooPaymentMethodMap map in list)
            {
                if (string.Equals(method, map.MethodMatch.Trim(), StringComparison.OrdinalIgnoreCase))
                    return TrimAbbrev(map.PaymentAbbrev);
            }

            // PayFast titles often include "Instant EFT" — do not let a generic EFT map win.
            if (ContainsPayFast(hay))
            {
                foreach (WooPaymentMethodMap map in list)
                {
                    string mm = map.MethodMatch.Trim();
                    if (ContainsPayFast(mm) || string.Equals(mm, "PF", StringComparison.OrdinalIgnoreCase))
                        return TrimAbbrev(map.PaymentAbbrev);
                }
                return "PF";
            }

            // Longest substring match for other gateways.
            foreach (WooPaymentMethodMap map in list)
            {
                if (hay.IndexOf(map.MethodMatch.Trim(), StringComparison.OrdinalIgnoreCase) >= 0)
                    return TrimAbbrev(map.PaymentAbbrev);
            }

            return FallbackAbbrev(hay);
        }

        private static bool ContainsPayFast(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            return value.IndexOf("payfast", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("pay_fast", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FallbackAbbrev(string paymentHaystack)
        {
            string hay = (paymentHaystack ?? string.Empty).ToLowerInvariant();
            if (ContainsPayFast(hay))
                return "PF";
            if (hay.Contains("yoco"))
                return "Yoco";
            if (hay.Contains("snapscan"))
                return "SS";
            if (hay.Contains("bacs") || hay.Contains("eft") || hay.Contains("direct bank")
                || hay.Contains("direct payment") || hay.Contains("bank transfer"))
                return "EFT";

            string title = (paymentHaystack ?? "Woo").Trim();
            if (title.Length <= 4)
                return title;
            return title.Substring(0, 4);
        }

        public int InsertMap(WooPaymentMethodMap map, string updatedBy)
        {
            const string sql = @"
INSERT INTO WooPaymentMethodMapTbl (MethodMatch, PaymentAbbrev, IsActive, Notes, UpdatedAt, UpdatedBy)
VALUES (@MethodMatch, @PaymentAbbrev, @IsActive, @Notes, SYSUTCDATETIME(), @UpdatedBy);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return ExecuteScalar<int>(sql, BuildParams(map, updatedBy, includeId: false));
        }

        public int UpdateMap(WooPaymentMethodMap map, string updatedBy)
        {
            const string sql = @"
UPDATE WooPaymentMethodMapTbl SET
 MethodMatch = @MethodMatch,
 PaymentAbbrev = @PaymentAbbrev,
 IsActive = @IsActive,
 Notes = @Notes,
 UpdatedAt = SYSUTCDATETIME(),
 UpdatedBy = @UpdatedBy
WHERE MapID = @MapID";
            return ExecNonQuery(sql, BuildParams(map, updatedBy, includeId: true));
        }

        public int DeleteMap(int mapId)
        {
            return ExecNonQuery(
                "DELETE FROM WooPaymentMethodMapTbl WHERE MapID = @MapID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@MapID", DataValue = mapId, DataDbType = DbType.Int32 }
                });
        }

        private static string TrimAbbrev(string abbrev)
        {
            if (string.IsNullOrWhiteSpace(abbrev))
                return "Woo";
            abbrev = abbrev.Trim();
            return abbrev.Length <= 4 ? abbrev : abbrev.Substring(0, 4);
        }

        private static List<DBParameter> BuildParams(WooPaymentMethodMap map, string updatedBy, bool includeId)
        {
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@MethodMatch", DataValue = map.MethodMatch ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PaymentAbbrev", DataValue = TrimAbbrev(map.PaymentAbbrev), DataDbType = DbType.String },
                new DBParameter { ParamName = "@IsActive", DataValue = map.IsActive, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = (object)map.Notes ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@UpdatedBy", DataValue = updatedBy ?? string.Empty, DataDbType = DbType.String }
            };
            if (includeId)
                p.Add(new DBParameter { ParamName = "@MapID", DataValue = map.MapID, DataDbType = DbType.Int32 });
            return p;
        }

        private static WooPaymentMethodMap MapRow(IDataRecord rdr)
        {
            return new WooPaymentMethodMap
            {
                MapID = Convert.ToInt32(rdr["MapID"]),
                MethodMatch = rdr["MethodMatch"] == DBNull.Value ? string.Empty : rdr["MethodMatch"].ToString(),
                PaymentAbbrev = rdr["PaymentAbbrev"] == DBNull.Value ? string.Empty : rdr["PaymentAbbrev"].ToString(),
                IsActive = rdr["IsActive"] != DBNull.Value && Convert.ToBoolean(rdr["IsActive"]),
                Notes = rdr["Notes"] == DBNull.Value ? null : rdr["Notes"].ToString()
            };
        }
    }
}
