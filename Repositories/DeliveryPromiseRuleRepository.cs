using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class DeliveryPromiseRuleRepository
    {
        private const string CoreColumns =
            "RuleID, RuleGroup, AreaID, AreaMatchName, SortOrder, " +
            "WindowStartDow, WindowStartMinutes, WindowEndDow, WindowEndMinutes, " +
            "PromiseKind, ResultMode, ResultDow, NoThursdayDispatch, WedAfterNoonToFriday, Notes, Enabled";

        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    int n = db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'DeliveryPromiseRuleTbl'");
                    return n > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<DeliveryPromiseRule> GetAll(bool includeDisabled = true)
        {
            var list = new List<DeliveryPromiseRule>();
            if (!TableExists())
                return list;

            string sql = "SELECT " + CoreColumns + " FROM DeliveryPromiseRuleTbl";
            if (!includeDisabled)
                sql += " WHERE Enabled = 1";
            sql += " ORDER BY RuleGroup, SortOrder, RuleID";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(Map(rdr));
            }
            return list;
        }

        public List<DeliveryPromiseRule> GetEnabledRulesForArea(int areaId)
        {
            var list = new List<DeliveryPromiseRule>();
            if (areaId <= 0 || !TableExists())
                return list;

            string sql = @"
SELECT " + CoreColumns + @"
FROM DeliveryPromiseRuleTbl
WHERE Enabled = 1 AND AreaID = @AreaID
ORDER BY SortOrder, RuleID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@AreaID", DataValue = areaId, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                while (rdr != null && rdr.Read())
                    list.Add(Map(rdr));
            }
            return list;
        }

        public DeliveryPromiseRule GetById(int ruleId)
        {
            if (ruleId <= 0 || !TableExists())
                return null;

            string sql = "SELECT " + CoreColumns + " FROM DeliveryPromiseRuleTbl WHERE RuleID = @RuleID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RuleID", DataValue = ruleId, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                    return Map(rdr);
            }
            return null;
        }

        public int Insert(DeliveryPromiseRule rule)
        {
            if (rule == null || !TableExists())
                return 0;

            const string sql = @"
INSERT INTO DeliveryPromiseRuleTbl
(RuleGroup, AreaID, AreaMatchName, SortOrder, WindowStartDow, WindowStartMinutes,
 WindowEndDow, WindowEndMinutes, PromiseKind, ResultMode, ResultDow,
 NoThursdayDispatch, WedAfterNoonToFriday, Notes, Enabled)
VALUES
(@RuleGroup, @AreaID, @AreaMatchName, @SortOrder, @WindowStartDow, @WindowStartMinutes,
 @WindowEndDow, @WindowEndMinutes, @PromiseKind, @ResultMode, @ResultDow,
 @NoThursdayDispatch, @WedAfterNoonToFriday, @Notes, @Enabled);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<int>(sql, BuildParams(rule, includeId: false));
        }

        public bool Update(DeliveryPromiseRule rule)
        {
            if (rule == null || rule.RuleID <= 0 || !TableExists())
                return false;

            const string sql = @"
UPDATE DeliveryPromiseRuleTbl SET
    RuleGroup = @RuleGroup,
    AreaID = @AreaID,
    AreaMatchName = @AreaMatchName,
    SortOrder = @SortOrder,
    WindowStartDow = @WindowStartDow,
    WindowStartMinutes = @WindowStartMinutes,
    WindowEndDow = @WindowEndDow,
    WindowEndMinutes = @WindowEndMinutes,
    PromiseKind = @PromiseKind,
    ResultMode = @ResultMode,
    ResultDow = @ResultDow,
    NoThursdayDispatch = @NoThursdayDispatch,
    WedAfterNoonToFriday = @WedAfterNoonToFriday,
    Notes = @Notes,
    Enabled = @Enabled
WHERE RuleID = @RuleID";

            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(sql, BuildParams(rule, includeId: true)) > 0;
        }

        public bool Delete(int ruleId)
        {
            if (ruleId <= 0 || !TableExists())
                return false;

            const string sql = "DELETE FROM DeliveryPromiseRuleTbl WHERE RuleID = @RuleID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RuleID", DataValue = ruleId, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(sql, p) > 0;
        }

        public List<string> GetDistinctRuleGroups()
        {
            return GetAll(includeDisabled: true)
                .Select(r => r.RuleGroup ?? string.Empty)
                .Where(g => g.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g)
                .ToList();
        }

        private static List<DBParameter> BuildParams(DeliveryPromiseRule rule, bool includeId)
        {
            var list = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RuleGroup", DataValue = (object)rule.RuleGroup ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AreaID", DataValue = rule.AreaID.HasValue ? (object)rule.AreaID.Value : DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@AreaMatchName", DataValue = (object)rule.AreaMatchName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SortOrder", DataValue = rule.SortOrder, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@WindowStartDow", DataValue = rule.WindowStartDow, DataDbType = DbType.Byte },
                new DBParameter { ParamName = "@WindowStartMinutes", DataValue = rule.WindowStartMinutes, DataDbType = DbType.Int16 },
                new DBParameter { ParamName = "@WindowEndDow", DataValue = rule.WindowEndDow, DataDbType = DbType.Byte },
                new DBParameter { ParamName = "@WindowEndMinutes", DataValue = rule.WindowEndMinutes, DataDbType = DbType.Int16 },
                new DBParameter { ParamName = "@PromiseKind", DataValue = (object)rule.PromiseKind ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ResultMode", DataValue = (object)rule.ResultMode ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ResultDow", DataValue = rule.ResultDow.HasValue ? (object)rule.ResultDow.Value : DBNull.Value, DataDbType = DbType.Byte },
                new DBParameter { ParamName = "@NoThursdayDispatch", DataValue = rule.NoThursdayDispatch, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@WedAfterNoonToFriday", DataValue = rule.WedAfterNoonToFriday, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = (object)rule.Notes ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Enabled", DataValue = rule.Enabled, DataDbType = DbType.Boolean }
            };
            if (includeId)
                list.Add(new DBParameter { ParamName = "@RuleID", DataValue = rule.RuleID, DataDbType = DbType.Int32 });
            return list;
        }

        private static DeliveryPromiseRule Map(IDataReader rdr)
        {
            return new DeliveryPromiseRule
            {
                RuleID = Convert.ToInt32(rdr["RuleID"]),
                RuleGroup = rdr["RuleGroup"] == DBNull.Value ? null : rdr["RuleGroup"].ToString(),
                AreaID = rdr["AreaID"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["AreaID"]),
                AreaMatchName = rdr["AreaMatchName"] == DBNull.Value ? null : rdr["AreaMatchName"].ToString(),
                SortOrder = rdr["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SortOrder"]),
                WindowStartDow = Convert.ToByte(rdr["WindowStartDow"]),
                WindowStartMinutes = Convert.ToInt16(rdr["WindowStartMinutes"]),
                WindowEndDow = Convert.ToByte(rdr["WindowEndDow"]),
                WindowEndMinutes = Convert.ToInt16(rdr["WindowEndMinutes"]),
                PromiseKind = rdr["PromiseKind"] == DBNull.Value ? null : rdr["PromiseKind"].ToString(),
                ResultMode = rdr["ResultMode"] == DBNull.Value ? null : rdr["ResultMode"].ToString(),
                ResultDow = rdr["ResultDow"] == DBNull.Value ? (byte?)null : Convert.ToByte(rdr["ResultDow"]),
                NoThursdayDispatch = rdr["NoThursdayDispatch"] != DBNull.Value && Convert.ToBoolean(rdr["NoThursdayDispatch"]),
                WedAfterNoonToFriday = rdr["WedAfterNoonToFriday"] != DBNull.Value && Convert.ToBoolean(rdr["WedAfterNoonToFriday"]),
                Notes = rdr["Notes"] == DBNull.Value ? null : rdr["Notes"].ToString(),
                Enabled = rdr["Enabled"] == DBNull.Value || Convert.ToBoolean(rdr["Enabled"])
            };
        }
    }
}
