using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooSkuWildcardRuleRepository : RepositoryBase<WooSkuWildcardRule>
    {
        protected override string TableName => "WooSkuWildcardRulesTbl";
        protected override string KeyColumn => "RuleID";

        public List<WooSkuWildcardRule> GetAllWithItems()
        {
            var list = new List<WooSkuWildcardRule>();
            string sql = @"
SELECT r.*, i.ItemDesc
FROM WooSkuWildcardRulesTbl r
LEFT JOIN ItemsTbl i ON i.ItemID = r.ItemID
ORDER BY r.SkuPrefixPattern, r.SuffixToken";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                {
                    var row = DbMapper.Map<WooSkuWildcardRule>(rdr);
                    try { row.ItemDesc = rdr["ItemDesc"] as string; } catch { /* ignore */ }
                    list.Add(row);
                }
            }
            return list;
        }

        public override int Insert(WooSkuWildcardRule entity)
        {
            string sql = @"
INSERT INTO WooSkuWildcardRulesTbl
(SkuPrefixPattern, SuffixToken, QtyFactor, PackagingID, ItemID, IsActive, Notes)
VALUES
(@SkuPrefixPattern, @SuffixToken, @QtyFactor, @PackagingID, @ItemID, @IsActive, @Notes);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return ExecuteScalar<int>(sql, BuildParams(entity, false));
        }

        public override int Update(WooSkuWildcardRule entity)
        {
            string sql = @"
UPDATE WooSkuWildcardRulesTbl SET
 SkuPrefixPattern = @SkuPrefixPattern,
 SuffixToken = @SuffixToken,
 QtyFactor = @QtyFactor,
 PackagingID = @PackagingID,
 ItemID = @ItemID,
 IsActive = @IsActive,
 Notes = @Notes
WHERE RuleID = @RuleID";
            return ExecNonQuery(sql, BuildParams(entity, true));
        }

        public int DeleteRule(int ruleId)
        {
            string sql = "DELETE FROM WooSkuWildcardRulesTbl WHERE RuleID = @RuleID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RuleID", DataValue = ruleId, DataDbType = DbType.Int32 }
            };
            return ExecNonQuery(sql, p);
        }

        private static List<DBParameter> BuildParams(WooSkuWildcardRule e, bool includeKey)
        {
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@SkuPrefixPattern", DataValue = e.SkuPrefixPattern ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SuffixToken", DataValue = (object)e.SuffixToken ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@QtyFactor", DataValue = e.QtyFactor, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@PackagingID", DataValue = DbParamHelpers.FkOrDbNull(e.PackagingID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemID", DataValue = e.ItemID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@IsActive", DataValue = e.IsActive, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = (object)e.Notes ?? DBNull.Value, DataDbType = DbType.String }
            };
            if (includeKey)
                p.Add(new DBParameter { ParamName = "@RuleID", DataValue = e.RuleID, DataDbType = DbType.Int32 });
            return p;
        }
    }
}
