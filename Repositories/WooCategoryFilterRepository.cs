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
            EnsureSchemaColumns();
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
                    IncludeInSync = defaultInclude,
                    DefaultSortValue = GuessDefaultSort(name)
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

        public void UpdateIncludeAndSort(int filterId, bool include, int? defaultSortValue, string defaultImportMode = null)
        {
            EnsureSchemaColumns();
            const string sql = @"
UPDATE WooCategoryFilterTbl
SET IncludeInSync = @IncludeInSync,
    DefaultSortValue = @DefaultSortValue,
    DefaultImportMode = @DefaultImportMode
WHERE FilterID = @FilterID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@IncludeInSync", DataValue = include, DataDbType = DbType.Boolean },
                new DBParameter
                {
                    ParamName = "@DefaultSortValue",
                    DataValue = defaultSortValue.HasValue && defaultSortValue.Value > 0
                        ? (object)defaultSortValue.Value
                        : DBNull.Value,
                    DataDbType = DbType.Int32
                },
                new DBParameter
                {
                    ParamName = "@DefaultImportMode",
                    DataValue = (object)NormalizeImportModeOrNull(defaultImportMode) ?? DBNull.Value,
                    DataDbType = DbType.String
                },
                new DBParameter { ParamName = "@FilterID", DataValue = filterId, DataDbType = DbType.Int32 }
            };
            ExecNonQuery(sql, p);
        }

        public override int Insert(WooCategoryFilter entity)
        {
            EnsureSchemaColumns();
            string sql = @"
INSERT INTO WooCategoryFilterTbl (WooCategoryId, CategoryName, ParentWooCategoryId, IncludeInSync, DefaultSortValue, DefaultImportMode)
VALUES (@WooCategoryId, @CategoryName, @ParentWooCategoryId, @IncludeInSync, @DefaultSortValue, @DefaultImportMode);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooCategoryId", DataValue = entity.WooCategoryId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@CategoryName", DataValue = (object)entity.CategoryName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ParentWooCategoryId", DataValue = entity.ParentWooCategoryId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@IncludeInSync", DataValue = entity.IncludeInSync, DataDbType = DbType.Boolean },
                new DBParameter
                {
                    ParamName = "@DefaultSortValue",
                    DataValue = entity.DefaultSortValue.HasValue && entity.DefaultSortValue.Value > 0
                        ? (object)entity.DefaultSortValue.Value
                        : DBNull.Value,
                    DataDbType = DbType.Int32
                },
                new DBParameter
                {
                    ParamName = "@DefaultImportMode",
                    DataValue = (object)NormalizeImportModeOrNull(entity.DefaultImportMode) ?? DBNull.Value,
                    DataDbType = DbType.String
                }
            };
            return ExecuteScalar<int>(sql, p);
        }

        public override int Update(WooCategoryFilter entity)
        {
            EnsureSchemaColumns();
            string sql = @"
UPDATE WooCategoryFilterTbl
SET WooCategoryId = @WooCategoryId,
    CategoryName = @CategoryName,
    ParentWooCategoryId = @ParentWooCategoryId,
    IncludeInSync = @IncludeInSync,
    DefaultSortValue = @DefaultSortValue,
    DefaultImportMode = @DefaultImportMode
WHERE FilterID = @FilterID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WooCategoryId", DataValue = entity.WooCategoryId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@CategoryName", DataValue = (object)entity.CategoryName ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ParentWooCategoryId", DataValue = entity.ParentWooCategoryId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@IncludeInSync", DataValue = entity.IncludeInSync, DataDbType = DbType.Boolean },
                new DBParameter
                {
                    ParamName = "@DefaultSortValue",
                    DataValue = entity.DefaultSortValue.HasValue && entity.DefaultSortValue.Value > 0
                        ? (object)entity.DefaultSortValue.Value
                        : DBNull.Value,
                    DataDbType = DbType.Int32
                },
                new DBParameter
                {
                    ParamName = "@DefaultImportMode",
                    DataValue = (object)NormalizeImportModeOrNull(entity.DefaultImportMode) ?? DBNull.Value,
                    DataDbType = DbType.String
                },
                new DBParameter { ParamName = "@FilterID", DataValue = entity.FilterID, DataDbType = DbType.Int32 }
            };
            return ExecNonQuery(sql, p);
        }

        /// <summary>
        /// Default Tracker S/O for a product: primary Woo category (deepest assigned),
        /// then walk up parents while S/O is blank /(parent).
        /// </summary>
        public static int? ResolveDefaultSortValue(IList<long> categoryIds, IList<WooCategoryFilter> filters)
        {
            long primary;
            Dictionary<long, WooCategoryFilter> byWooId;
            if (!TryBuildCategoryWalk(categoryIds, filters, out primary, out byWooId))
                return null;

            int? found = WalkSort(primary, byWooId);
            if (found.HasValue)
                return found;

            foreach (long id in categoryIds)
            {
                if (id <= 0 || id == primary)
                    continue;
                found = WalkSort(id, byWooId);
                if (found.HasValue)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Default parent ImportMode for unmapped products.
        /// When a product is in several categories, prefer the deepest match; if depths tie
        /// (or one branch is only Variants), prefer Exclude / ParentNotes / ParentItem over Variants
        /// so a Services=ParentNotes default is not masked by Roasted Coffee Beans=Variants.
        /// Null when none assigned (caller uses Variants).
        /// </summary>
        public static string ResolveDefaultImportMode(IList<long> categoryIds, IList<WooCategoryFilter> filters)
        {
            long primary;
            Dictionary<long, WooCategoryFilter> byWooId;
            if (!TryBuildCategoryWalk(categoryIds, filters, out primary, out byWooId))
                return null;

            string best = null;
            int bestScore = -1;
            foreach (long id in categoryIds)
            {
                if (id <= 0)
                    continue;
                string mode = WalkImportMode(id, byWooId);
                if (string.IsNullOrEmpty(mode))
                    continue;
                int score = CategoryDepth(id, byWooId) * 10 + ImportModePreference(mode);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = mode;
                }
            }
            return best;
        }

        /// <summary>Higher = more specific than plain Import variants when categories conflict.</summary>
        private static int ImportModePreference(string mode)
        {
            if (string.Equals(mode, WooProductMapRow.ImportModeExclude, StringComparison.OrdinalIgnoreCase))
                return 4;
            if (string.Equals(mode, WooProductMapRow.ImportModeParentNotes, StringComparison.OrdinalIgnoreCase))
                return 3;
            if (string.Equals(mode, WooProductMapRow.ImportModeParentItem, StringComparison.OrdinalIgnoreCase))
                return 2;
            if (string.Equals(mode, WooProductMapRow.ImportModeVariants, StringComparison.OrdinalIgnoreCase))
                return 1;
            return 0;
        }

        public static string NormalizeImportModeOrNull(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
                return null;
            string m = mode.Trim();
            if (string.Equals(m, WooProductMapRow.ImportModeVariants, StringComparison.OrdinalIgnoreCase))
                return WooProductMapRow.ImportModeVariants;
            if (string.Equals(m, WooProductMapRow.ImportModeParentItem, StringComparison.OrdinalIgnoreCase))
                return WooProductMapRow.ImportModeParentItem;
            if (string.Equals(m, WooProductMapRow.ImportModeParentNotes, StringComparison.OrdinalIgnoreCase))
                return WooProductMapRow.ImportModeParentNotes;
            if (string.Equals(m, WooProductMapRow.ImportModeExclude, StringComparison.OrdinalIgnoreCase))
                return WooProductMapRow.ImportModeExclude;
            return null;
        }

        private static bool TryBuildCategoryWalk(
            IList<long> categoryIds,
            IList<WooCategoryFilter> filters,
            out long primary,
            out Dictionary<long, WooCategoryFilter> byWooId)
        {
            primary = 0;
            byWooId = null;
            if (categoryIds == null || categoryIds.Count == 0 || filters == null || filters.Count == 0)
                return false;

            byWooId = new Dictionary<long, WooCategoryFilter>();
            foreach (var f in filters)
            {
                if (f != null && f.WooCategoryId > 0 && !byWooId.ContainsKey(f.WooCategoryId))
                    byWooId[f.WooCategoryId] = f;
            }

            int bestDepth = -1;
            foreach (long id in categoryIds)
            {
                if (id <= 0)
                    continue;
                int d = CategoryDepth(id, byWooId);
                if (d > bestDepth)
                {
                    bestDepth = d;
                    primary = id;
                }
            }
            return primary > 0;
        }

        private static int CategoryDepth(long start, Dictionary<long, WooCategoryFilter> byWooId)
        {
            int d = 0;
            long cur = start;
            var seen = new HashSet<long>();
            WooCategoryFilter f;
            while (cur > 0 && seen.Add(cur) && byWooId.TryGetValue(cur, out f))
            {
                d++;
                cur = f.ParentWooCategoryId;
            }
            return d;
        }

        private static int? WalkSort(long start, Dictionary<long, WooCategoryFilter> byWooId)
        {
            long cur = start;
            var seen = new HashSet<long>();
            WooCategoryFilter f;
            while (cur > 0 && seen.Add(cur) && byWooId.TryGetValue(cur, out f))
            {
                if (f.DefaultSortValue.HasValue && f.DefaultSortValue.Value > 0)
                    return f.DefaultSortValue;
                cur = f.ParentWooCategoryId;
            }
            return null;
        }

        private static string WalkImportMode(long start, Dictionary<long, WooCategoryFilter> byWooId)
        {
            long cur = start;
            var seen = new HashSet<long>();
            WooCategoryFilter f;
            while (cur > 0 && seen.Add(cur) && byWooId.TryGetValue(cur, out f))
            {
                string mode = NormalizeImportModeOrNull(f.DefaultImportMode);
                if (!string.IsNullOrEmpty(mode))
                    return mode;
                cur = f.ParentWooCategoryId;
            }
            return null;
        }

        private bool _ensuringSchema;

        private void EnsureSchemaColumns()
        {
            if (_ensuringSchema)
                return;
            _ensuringSchema = true;
            try
            {
                ExecNonQuery(@"
IF OBJECT_ID(N'dbo.WooCategoryFilterTbl', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.WooCategoryFilterTbl', N'DefaultSortValue') IS NULL
BEGIN
    ALTER TABLE dbo.WooCategoryFilterTbl ADD DefaultSortValue INT NULL;
END");
                ExecNonQuery(@"
IF OBJECT_ID(N'dbo.WooCategoryFilterTbl', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.WooCategoryFilterTbl', N'DefaultImportMode') IS NULL
BEGIN
    ALTER TABLE dbo.WooCategoryFilterTbl ADD DefaultImportMode NVARCHAR(32) NULL;
END");
                BackfillNullDefaultsFromNames();
            }
            finally
            {
                _ensuringSchema = false;
            }
        }

        /// <summary>Seed unset defaults from category name vs ItemSortOrderTbl. Skips if any category already has a value.</summary>
        private void BackfillNullDefaultsFromNames()
        {
            List<WooCategoryFilter> rows;
            try
            {
                rows = LoadAllRaw();
            }
            catch
            {
                return;
            }
            if (rows.Count == 0)
                return;
            if (rows.Exists(r => r.DefaultSortValue.HasValue && r.DefaultSortValue.Value > 0))
                return;

            List<ItemSortOrder> sorts;
            try
            {
                sorts = new ItemSortOrdersRepository().GetAll("SortValue") ?? new List<ItemSortOrder>();
            }
            catch
            {
                sorts = ItemSortOrdersRepository.BuiltInRows();
            }

            foreach (var row in rows)
            {
                int? guess = GuessDefaultSort(row.CategoryName, sorts);
                if (!guess.HasValue)
                    continue;
                ExecNonQuery(@"
UPDATE WooCategoryFilterTbl
SET DefaultSortValue = @DefaultSortValue
WHERE FilterID = @FilterID AND DefaultSortValue IS NULL",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@DefaultSortValue", DataValue = guess.Value, DataDbType = DbType.Int32 },
                        new DBParameter { ParamName = "@FilterID", DataValue = row.FilterID, DataDbType = DbType.Int32 }
                    });
            }
        }

        private List<WooCategoryFilter> LoadAllRaw()
        {
            var list = new List<WooCategoryFilter>();
            const string sql = "SELECT * FROM WooCategoryFilterTbl";
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                    list.Add(DbMapper.Map<WooCategoryFilter>(rdr));
            }
            return list;
        }

        private static int? GuessDefaultSort(string categoryName, IList<ItemSortOrder> sorts = null)
        {
            if (sorts == null)
            {
                try
                {
                    sorts = new ItemSortOrdersRepository().GetAll("SortValue");
                }
                catch
                {
                    sorts = ItemSortOrdersRepository.BuiltInRows();
                }
            }
            return ItemSortOrdersRepository.MatchFromCategories(categoryName, sorts);
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
