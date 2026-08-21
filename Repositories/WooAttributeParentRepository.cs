using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooAttributeParentRepository : RepositoryBase<WooAttributeParent>
    {
        protected override string TableName => "WooAttributeParentTbl";
        protected override string KeyColumn => "ParentID";

        public List<WooAttributeParent> GetAllOrdered()
        {
            const string sql = @"
SELECT * FROM WooAttributeParentTbl
ORDER BY ResolvePriority, AttributeName, WooAttributeId";
            var list = new List<WooAttributeParent>();
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(DbMapper.Map<WooAttributeParent>(rdr));
            }
            return list;
        }

        public List<WooAttributeParent> GetUsedForVariants()
        {
            return GetAllOrdered().Where(p => p.UseForVariants).ToList();
        }

        public HashSet<string> GetVariantAttributeNames()
        {
            return new HashSet<string>(
                GetUsedForVariants()
                    .Select(p => (p.AttributeName ?? string.Empty).Trim())
                    .Where(n => n.Length > 0),
                StringComparer.OrdinalIgnoreCase);
        }

        public Dictionary<string, int> GetPriorityByAttributeName()
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in GetAllOrdered())
            {
                string name = (p.AttributeName ?? string.Empty).Trim();
                if (name.Length == 0)
                    continue;
                if (!dict.ContainsKey(name))
                    dict[name] = p.ResolvePriority;
            }
            return dict;
        }

        public WooAttributeParent GetByWooAttributeId(long wooAttributeId)
        {
            const string sql = "SELECT * FROM WooAttributeParentTbl WHERE WooAttributeId = @WooAttributeId";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooAttributeId", DataValue = wooAttributeId, DataDbType = DbType.Int64 }
            };
            return ExecuteQuerySingle<WooAttributeParent>(sql, p);
        }

        public void UpsertFromWoo(long wooAttributeId, string name, string slug, int termCount, bool defaultUseForVariants)
        {
            var existing = GetByWooAttributeId(wooAttributeId);
            if (existing == null)
            {
                Insert(new WooAttributeParent
                {
                    WooAttributeId = wooAttributeId,
                    AttributeName = name ?? string.Empty,
                    Slug = slug,
                    UseForVariants = defaultUseForVariants,
                    TermCount = termCount < 0 ? 0 : termCount,
                    ResolvePriority = 100
                });
                return;
            }

            existing.AttributeName = name ?? existing.AttributeName;
            existing.Slug = slug ?? existing.Slug;
            existing.TermCount = termCount < 0 ? existing.TermCount : termCount;
            Update(existing);
        }

        public void UpdateSelection(int parentId, bool useForVariants, int resolvePriority)
        {
            const string sql = @"
UPDATE WooAttributeParentTbl
SET UseForVariants = @UseForVariants,
    ResolvePriority = @ResolvePriority
WHERE ParentID = @ParentID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@UseForVariants", DataValue = useForVariants, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@ResolvePriority", DataValue = resolvePriority, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ParentID", DataValue = parentId, DataDbType = DbType.Int32 }
            };
            ExecNonQuery(sql, p);
        }

        public override int Insert(WooAttributeParent entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            const string sql = @"
INSERT INTO WooAttributeParentTbl
(WooAttributeId, AttributeName, Slug, UseForVariants, TermCount, ResolvePriority)
VALUES
(@WooAttributeId, @AttributeName, @Slug, @UseForVariants, @TermCount, @ResolvePriority);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return ExecuteScalar<int>(sql, BuildParams(entity, includeKey: false));
        }

        public override int Update(WooAttributeParent entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            const string sql = @"
UPDATE WooAttributeParentTbl SET
 WooAttributeId = @WooAttributeId,
 AttributeName = @AttributeName,
 Slug = @Slug,
 UseForVariants = @UseForVariants,
 TermCount = @TermCount,
 ResolvePriority = @ResolvePriority
WHERE ParentID = @ParentID";
            return ExecNonQuery(sql, BuildParams(entity, includeKey: true));
        }

        private static List<DBParameter> BuildParams(WooAttributeParent e, bool includeKey)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooAttributeId", DataValue = e.WooAttributeId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@AttributeName", DataValue = (e.AttributeName ?? string.Empty).Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@Slug", DataValue = (object)e.Slug ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@UseForVariants", DataValue = e.UseForVariants, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@TermCount", DataValue = e.TermCount < 0 ? 0 : e.TermCount, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ResolvePriority", DataValue = e.ResolvePriority <= 0 ? 100 : e.ResolvePriority, DataDbType = DbType.Int32 }
            };
            if (includeKey)
                parameters.Add(new DBParameter { ParamName = "@ParentID", DataValue = e.ParentID, DataDbType = DbType.Int32 });
            return parameters;
        }
    }
}
