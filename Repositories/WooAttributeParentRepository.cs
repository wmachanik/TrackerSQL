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
ORDER BY AttributeName, WooAttributeId";
            var list = new List<WooAttributeParent>();
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    var row = MapParent(rdr);
                    list.Add(row);
                }
            }
            return list
                .OrderBy(p => p.DisplaySortRank)
                .ThenBy(p => p.AttributeName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.WooAttributeId)
                .ToList();
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

        public HashSet<string> GetLineAttributeNames()
        {
            return new HashSet<string>(
                GetUsedForVariants()
                    .Where(p => p.ContributesLine)
                    .Select(p => (p.AttributeName ?? string.Empty).Trim())
                    .Where(n => n.Length > 0),
                StringComparer.OrdinalIgnoreCase);
        }

        public HashSet<string> GetNotesAttributeNames()
        {
            return new HashSet<string>(
                GetUsedForVariants()
                    .Where(p => p.ContributesNote)
                    .Select(p => (p.AttributeName ?? string.Empty).Trim())
                    .Where(n => n.Length > 0),
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Attribute name → ranks for stamping onto option maps at resolve time.</summary>
        public Dictionary<string, WooAttributeParent> GetUsedByAttributeName()
        {
            var dict = new Dictionary<string, WooAttributeParent>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in GetUsedForVariants())
            {
                string name = (p.AttributeName ?? string.Empty).Trim();
                if (name.Length == 0 || dict.ContainsKey(name))
                    continue;
                dict[name] = p;
            }
            return dict;
        }

        /// <summary>Lower display sort first (for attribute label ordering).</summary>
        public Dictionary<string, int> GetDisplaySortByAttributeName()
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in GetAllOrdered())
            {
                string name = (p.AttributeName ?? string.Empty).Trim();
                if (name.Length == 0)
                    continue;
                if (!dict.ContainsKey(name))
                    dict[name] = p.DisplaySortRank;
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
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                    return MapParent(rdr);
            }
            return null;
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
                    QtyRank = 0,
                    PackRank = 0,
                    NoteRank = 0
                });
                return;
            }

            existing.AttributeName = name ?? existing.AttributeName;
            existing.Slug = slug ?? existing.Slug;
            existing.TermCount = termCount < 0 ? existing.TermCount : termCount;
            Update(existing);
        }

        public void UpdateSelection(int parentId, bool useForVariants, int qtyRank, int packRank, int noteRank)
        {
            const string sql = @"
UPDATE WooAttributeParentTbl
SET UseForVariants = @UseForVariants,
    QtyRank = @QtyRank,
    PackRank = @PackRank,
    NoteRank = @NoteRank
WHERE ParentID = @ParentID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@UseForVariants", DataValue = useForVariants, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@QtyRank", DataValue = WooAttributeParent.NormalizeRank(qtyRank), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PackRank", DataValue = WooAttributeParent.NormalizeRank(packRank), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@NoteRank", DataValue = WooAttributeParent.NormalizeRank(noteRank), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ParentID", DataValue = parentId, DataDbType = DbType.Int32 }
            };
            ExecNonQuery(sql, p);
        }

        public override int Insert(WooAttributeParent entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            const string sql = @"
INSERT INTO WooAttributeParentTbl
(WooAttributeId, AttributeName, Slug, UseForVariants, TermCount, QtyRank, PackRank, NoteRank)
VALUES
(@WooAttributeId, @AttributeName, @Slug, @UseForVariants, @TermCount, @QtyRank, @PackRank, @NoteRank);
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
 QtyRank = @QtyRank,
 PackRank = @PackRank,
 NoteRank = @NoteRank
WHERE ParentID = @ParentID";
            return ExecNonQuery(sql, BuildParams(entity, includeKey: true));
        }

        private static WooAttributeParent MapParent(IDataReader rdr)
        {
            var row = DbMapper.Map<WooAttributeParent>(rdr);
            if (row == null)
                return null;
            row.QtyRank = WooAttributeParent.NormalizeRank(row.QtyRank);
            row.PackRank = WooAttributeParent.NormalizeRank(row.PackRank);
            row.NoteRank = WooAttributeParent.NormalizeRank(row.NoteRank);
            row.ApplyLegacyChannelIfNeeded();
            return row;
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
                new DBParameter { ParamName = "@QtyRank", DataValue = WooAttributeParent.NormalizeRank(e.QtyRank), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PackRank", DataValue = WooAttributeParent.NormalizeRank(e.PackRank), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@NoteRank", DataValue = WooAttributeParent.NormalizeRank(e.NoteRank), DataDbType = DbType.Int32 }
            };
            if (includeKey)
                parameters.Add(new DBParameter { ParamName = "@ParentID", DataValue = e.ParentID, DataDbType = DbType.Int32 });
            return parameters;
        }
    }
}
