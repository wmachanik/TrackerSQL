using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ItemSortOrdersRepository : RepositoryBase<ItemSortOrder>
    {
        private List<ItemSortOrder> _allCache;

        protected override string TableName => "ItemSortOrderTbl";
        protected override string KeyColumn => "SortOrderID";

        protected override string CoreColumns =>
            "SortOrderID, SortValue, SortOrderDesc, IsEnabled";

        protected override string LookupColumns =>
            "SortOrderID, SortValue, SortOrderDesc, IsEnabled";

        public override List<ItemSortOrder> GetAll(string SortBy)
        {
            var list = new List<ItemSortOrder>(LoadAll());
            if (string.Equals(SortBy, "SortOrderDesc", StringComparison.OrdinalIgnoreCase))
                list.Sort((a, b) => string.Compare(a.SortOrderDesc, b.SortOrderDesc, StringComparison.OrdinalIgnoreCase));
            else
                list.Sort((a, b) => a.SortValue.CompareTo(b.SortValue));
            return list;
        }

        public override int Insert(ItemSortOrder entity)
        {
            EnsureExists();
            const string sql = @"
INSERT INTO ItemSortOrderTbl (SortValue, SortOrderDesc, IsEnabled)
VALUES (@SortValue, @SortOrderDesc, @IsEnabled)";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@SortValue", DataValue = entity.SortValue, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SortOrderDesc", DataValue = entity.SortOrderDesc ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@IsEnabled", DataValue = entity.IsEnabled ?? true, DataDbType = DbType.Boolean }
            };
            int n = ExecNonQuery(sql, p);
            InvalidateCache();
            return n;
        }

        public override int Update(ItemSortOrder entity)
        {
            EnsureExists();
            const string sql = @"
UPDATE ItemSortOrderTbl
SET SortValue = @SortValue, SortOrderDesc = @SortOrderDesc, IsEnabled = @IsEnabled
WHERE SortOrderID = @SortOrderID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@SortValue", DataValue = entity.SortValue, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SortOrderDesc", DataValue = entity.SortOrderDesc ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@IsEnabled", DataValue = entity.IsEnabled ?? true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@SortOrderID", DataValue = entity.SortOrderID, DataDbType = DbType.Int32 }
            };
            int n = ExecNonQuery(sql, p);
            InvalidateCache();
            return n;
        }

        public override bool Delete(int id)
        {
            EnsureExists();
            const string sql = "DELETE FROM ItemSortOrderTbl WHERE SortOrderID = @SortOrderID";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@SortOrderID", DataValue = id, DataDbType = DbType.Int32 }
            };
            bool ok = ExecNonQuery(sql, p) > 0;
            InvalidateCache();
            return ok;
        }

        public void FillDropDown(ListControl ddl, int? selectedValue)
        {
            FillDropDown(ddl, selectedValue, includeBlank: false);
        }

        public void FillDropDown(ListControl ddl, int? selectedValue, bool includeBlank)
        {
            FillDropDown(ddl, selectedValue, includeBlank, compactDisplay: false);
        }

        /// <summary>Sort order dropdown showing number only; full label in option title.</summary>
        public void FillDropDownCompact(ListControl ddl, int? selectedValue)
        {
            FillDropDown(ddl, selectedValue, includeBlank: false, compactDisplay: true);
        }

        public void FillDropDown(ListControl ddl, int? selectedValue, bool includeBlank, bool compactDisplay)
        {
            if (ddl == null)
                return;
            ddl.Items.Clear();
            List<ItemSortOrder> rows;
            try
            {
                rows = GetAll("SortValue") ?? new List<ItemSortOrder>();
            }
            catch
            {
                rows = BuiltInRows();
            }

            if (includeBlank)
                ddl.Items.Add(new ListItem("(parent)", ""));

            foreach (var row in rows)
            {
                bool enabled = row.IsEnabled ?? true;
                if (!enabled && (!selectedValue.HasValue || selectedValue.Value != row.SortValue))
                    continue;
                string text = compactDisplay ? row.SortValue.ToString() : row.DisplayText;
                if (!enabled)
                    text = "_" + text;
                var item = new ListItem(text, row.SortValue.ToString());
                if (compactDisplay)
                {
                    string full = !enabled ? "_" + row.DisplayText : row.DisplayText;
                    item.Attributes["data-compact"] = text;
                    item.Attributes["data-full"] = full;
                }
                ddl.Items.Add(item);
            }

            if (includeBlank && (!selectedValue.HasValue || selectedValue.Value <= 0))
            {
                ddl.SelectedIndex = 0;
                return;
            }

            string sel = (selectedValue.HasValue ? selectedValue.Value : 1).ToString();
            if (ddl.Items.FindByValue(sel) == null && selectedValue.HasValue)
            {
                string unlisted = sel + " - (unlisted)";
                var item = new ListItem(compactDisplay ? sel : unlisted, sel);
                if (compactDisplay)
                {
                    item.Attributes["data-compact"] = sel;
                    item.Attributes["data-full"] = unlisted;
                }
                ddl.Items.Add(item);
            }
            if (ddl.Items.FindByValue(sel) != null)
                ddl.SelectedValue = sel;
            else if (ddl.Items.Count > 0)
                ddl.SelectedIndex = 0;
        }

        public string Describe(int? sortValue)
        {
            if (!sortValue.HasValue)
                return string.Empty;
            try
            {
                var match = LoadAll().Find(r => r.SortValue == sortValue.Value);
                if (match != null)
                    return match.DisplayText;
            }
            catch
            {
            }
            return sortValue.Value.ToString();
        }

        /// <summary>
        /// Best SortValue for a Woo categories string (comma-separated).
        /// Prefers exact category = description, then category containing the description.
        /// Longer descriptions win ("Jura Maintenance" over "Maintenance").
        /// </summary>
        public static int? MatchFromCategories(string categoriesLabel, IList<ItemSortOrder> rows)
        {
            if (string.IsNullOrWhiteSpace(categoriesLabel) || rows == null || rows.Count == 0)
                return null;

            var tokens = categoriesLabel.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var lookups = new List<ItemSortOrder>();
            foreach (var row in rows)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.SortOrderDesc))
                    continue;
                if (row.IsEnabled == false)
                    continue;
                lookups.Add(row);
            }
            lookups.Sort((a, b) => b.SortOrderDesc.Trim().Length.CompareTo(a.SortOrderDesc.Trim().Length));

            int? containsHit = null;
            foreach (var raw in tokens)
            {
                string token = raw.Trim();
                if (token.Length == 0)
                    continue;
                foreach (var row in lookups)
                {
                    string desc = row.SortOrderDesc.Trim();
                    if (string.Equals(token, desc, StringComparison.OrdinalIgnoreCase))
                        return row.SortValue;
                    if (containsHit == null
                        && token.IndexOf(desc, StringComparison.OrdinalIgnoreCase) >= 0)
                        containsHit = row.SortValue;
                }
            }
            return containsHit;
        }

        public void EnsureExists()
        {
            ExecNonQuery(@"
IF OBJECT_ID(N'dbo.ItemSortOrderTbl', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ItemSortOrderTbl
    (
        SortOrderID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ItemSortOrderTbl PRIMARY KEY,
        SortValue INT NOT NULL,
        SortOrderDesc NVARCHAR(100) NOT NULL,
        IsEnabled BIT NOT NULL CONSTRAINT DF_ItemSortOrder_Enabled DEFAULT (1),
        CONSTRAINT UQ_ItemSortOrder_Value UNIQUE (SortValue)
    );
END");

            ExecNonQuery(@"
MERGE dbo.ItemSortOrderTbl AS t
USING (VALUES
    (1,  N'Coffee'),
    (2,  N'Other Beverage'),
    (3,  N'Maintenance'),
    (4,  N'Jura Maintenance'),
    (5,  N'Water Filters'),
    (6,  N'Paper Filters'),
    (7,  N'Jura Accessories'),
    (8,  N'Gear'),
    (9,  N'Green'),
    (10, N'General'),
    (11, N'Misc'),
    (15, N'Groups')
) AS s(SortValue, SortOrderDesc)
ON t.SortValue = s.SortValue
WHEN NOT MATCHED THEN
    INSERT (SortValue, SortOrderDesc, IsEnabled)
    VALUES (s.SortValue, s.SortOrderDesc, 1);");
            InvalidateCache();
        }

        private List<ItemSortOrder> LoadAll()
        {
            if (_allCache != null)
                return _allCache;
            EnsureExists();
            try
            {
                _allCache = base.GetAll("SortValue") ?? new List<ItemSortOrder>();
            }
            catch
            {
                _allCache = BuiltInRows();
            }
            return _allCache;
        }

        private void InvalidateCache()
        {
            _allCache = null;
        }

        public static List<ItemSortOrder> BuiltInRows()
        {
            return new List<ItemSortOrder>
            {
                Row(1, "Coffee"),
                Row(2, "Other Beverage"),
                Row(3, "Maintenance"),
                Row(4, "Jura Maintenance"),
                Row(5, "Water Filters"),
                Row(6, "Paper Filters"),
                Row(7, "Jura Accessories"),
                Row(8, "Gear"),
                Row(9, "Green"),
                Row(10, "General"),
                Row(11, "Misc"),
                Row(15, "Groups")
            };
        }

        private static ItemSortOrder Row(int value, string desc)
        {
            return new ItemSortOrder { SortValue = value, SortOrderDesc = desc, IsEnabled = true };
        }
    }
}
