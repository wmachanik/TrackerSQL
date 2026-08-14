using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooCategoryFilterRepository : RepositoryBase<WooCategoryFilter>
    {
        protected override string TableName => "WooCategoryFilterTbl";
        protected override string KeyColumn => "FilterID";

        public List<WooCategoryFilter> GetAllOrdered()
        {
            var list = new List<WooCategoryFilter>();
            string sql = "SELECT * FROM WooCategoryFilterTbl";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                    list.Add(DbMapper.Map<WooCategoryFilter>(rdr));
            }
            return OrderAsTree(list);
        }

        public WooCategoryFilter GetByWooCategoryId(long wooCategoryId)
        {
            string sql = "SELECT * FROM WooCategoryFilterTbl WHERE WooCategoryId = @WooCategoryId";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooCategoryId", DataValue = wooCategoryId, DataDbType = DbType.Int64 }
            };
            return ExecuteQuerySingle<WooCategoryFilter>(sql, p);
        }

        public void UpsertFromWoo(long wooCategoryId, string name, long parentWooCategoryId, bool defaultInclude)
        {
            var existing = GetByWooCategoryId(wooCategoryId);
            if (existing == null)
            {
                Insert(new WooCategoryFilter
                {
                    WooCategoryId = wooCategoryId,
                    CategoryName = name ?? string.Empty,
                    ParentWooCategoryId = parentWooCategoryId < 0 ? 0 : parentWooCategoryId,
                    IncludeInSync = defaultInclude
                });
                return;
            }

            existing.CategoryName = name ?? existing.CategoryName;
            existing.ParentWooCategoryId = parentWooCategoryId < 0 ? 0 : parentWooCategoryId;
            Update(existing);
        }

        public void UpdateInclude(int filterId, bool include)
        {
            string sql = @"
UPDATE WooCategoryFilterTbl
SET IncludeInSync = @IncludeInSync
WHERE FilterID = @FilterID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@IncludeInSync", DataValue = include, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@FilterID", DataValue = filterId, DataDbType = DbType.Int32 }
            };
            ExecNonQuery(sql, p);
        }

        public override int Insert(WooCategoryFilter entity)
        {
            string sql = @"
INSERT INTO WooCategoryFilterTbl (WooCategoryId, CategoryName, ParentWooCategoryId, IncludeInSync)
VALUES (@WooCategoryId, @CategoryName, @ParentWooCategoryId, @IncludeInSync);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooCategoryId", DataValue = entity.WooCategoryId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@CategoryName", DataValue = (object)entity.CategoryName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ParentWooCategoryId", DataValue = entity.ParentWooCategoryId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@IncludeInSync", DataValue = entity.IncludeInSync, DataDbType = DbType.Boolean }
            };
            return ExecuteScalar<int>(sql, p);
        }

        public override int Update(WooCategoryFilter entity)
        {
            string sql = @"
UPDATE WooCategoryFilterTbl
SET WooCategoryId = @WooCategoryId,
    CategoryName = @CategoryName,
    ParentWooCategoryId = @ParentWooCategoryId,
    IncludeInSync = @IncludeInSync
WHERE FilterID = @FilterID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooCategoryId", DataValue = entity.WooCategoryId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@CategoryName", DataValue = (object)entity.CategoryName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ParentWooCategoryId", DataValue = entity.ParentWooCategoryId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@IncludeInSync", DataValue = entity.IncludeInSync, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@FilterID", DataValue = entity.FilterID, DataDbType = DbType.Int32 }
            };
            return ExecNonQuery(sql, p);
        }

        private static List<WooCategoryFilter> OrderAsTree(List<WooCategoryFilter> all)
        {
            if (all == null || all.Count == 0)
                return new List<WooCategoryFilter>();

            var byParent = all
                .GroupBy(x => x.ParentWooCategoryId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.CategoryName ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList());

            var result = new List<WooCategoryFilter>();
            var seen = new HashSet<int>();

            void Walk(long parentId, int depth)
            {
                List<WooCategoryFilter> kids;
                if (!byParent.TryGetValue(parentId, out kids))
                    return;
                foreach (var c in kids)
                {
                    if (!seen.Add(c.FilterID))
                        continue;
                    c.Depth = depth;
                    result.Add(c);
                    Walk(c.WooCategoryId, depth + 1);
                }
            }

            Walk(0, 0);

            foreach (var c in all.OrderBy(x => x.CategoryName ?? string.Empty, StringComparer.OrdinalIgnoreCase))
            {
                if (seen.Contains(c.FilterID))
                    continue;
                c.Depth = 0;
                result.Add(c);
                seen.Add(c.FilterID);
                Walk(c.WooCategoryId, 1);
            }

            return result;
        }
    }
}
