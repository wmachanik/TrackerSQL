using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// Repository for ItemGroupsTbl.
    /// </summary>
    public class ItemGroupsRepository : RepositoryBase<ItemGroup>
    {
        private const string SelectColumns =
            "ItemGroupID, GroupItemServiceTypeID, ItemID, ItemSortPos, Enabled, Notes";

        protected override string TableName => "ItemGroupsTbl";
        protected override string KeyColumn => "ItemGroupID";

        protected override string CoreColumns =>
            "ItemGroupID, GroupItemServiceTypeID, ItemID, ItemSortPos, Enabled";

        protected override string LookupColumns =>
            "ItemGroupID, GroupItemServiceTypeID";

        public List<ItemGroup> GetAllByGroupReferenceItemId(int groupReferenceItemId, string sortBy = null)
        {
            if (groupReferenceItemId == SystemConstants.DatabaseConstants.InvalidID) return new List<ItemGroup>();

            string sql = $"SELECT {SelectColumns} FROM ItemGroupsTbl WHERE GroupItemServiceTypeID = @GroupItemServiceTypeID";
            sql += " ORDER BY " + (string.IsNullOrEmpty(sortBy) ? "ItemSortPos" : sortBy);

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 }
            };

            return QueryList(sql, parameters);
        }

        public ItemGroup GetFirstGroupItem(int groupReferenceItemId)
        {
            return GetSingleInGroup(groupReferenceItemId,
                "SELECT TOP 1 " + SelectColumns + " FROM ItemGroupsTbl WHERE GroupItemServiceTypeID = @GroupItemServiceTypeID ORDER BY ItemSortPos");
        }

        public ItemGroup GetNextGroupItem(int groupReferenceItemId, int lastItemSortPos)
        {
            var item = GetSingleInGroup(groupReferenceItemId,
                "SELECT TOP 1 " + SelectColumns + " FROM ItemGroupsTbl WHERE GroupItemServiceTypeID = @GroupItemServiceTypeID AND ItemSortPos > @ItemSortPos ORDER BY ItemSortPos",
                lastItemSortPos);
            return item ?? GetFirstGroupItem(groupReferenceItemId);
        }

        public ItemGroup GetPrevGroupItem(int groupReferenceItemId, int itemSortPos)
        {
            var item = GetSingleInGroup(groupReferenceItemId,
                "SELECT TOP 1 " + SelectColumns + " FROM ItemGroupsTbl WHERE GroupItemServiceTypeID = @GroupItemServiceTypeID AND ItemSortPos < @ItemSortPos ORDER BY ItemSortPos DESC",
                itemSortPos);
            return item ?? GetLastGroupItem(groupReferenceItemId);
        }

        public ItemGroup GetLastGroupItem(int groupReferenceItemId)
        {
            return GetSingleInGroup(groupReferenceItemId,
                "SELECT TOP 1 " + SelectColumns + " FROM ItemGroupsTbl WHERE GroupItemServiceTypeID = @GroupItemServiceTypeID ORDER BY ItemSortPos DESC");
        }

        public int GetLastGroupItemSortPos(int groupReferenceItemId)
        {
            var item = GetLastGroupItem(groupReferenceItemId);
            return item?.ItemSortPos ?? -1;
        }

        public int GetItemIdAtSortPos(int groupReferenceItemId, int sortPos)
        {
            const string sql = "SELECT ItemID FROM ItemGroupsTbl WHERE GroupItemServiceTypeID = @GroupItemServiceTypeID AND ItemSortPos = @ItemSortPos";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemSortPos", DataValue = sortPos, DataDbType = DbType.Int32 }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public bool UpdateItemSortPos(int sortPos, int groupReferenceItemId, int itemId)
        {
            const string sql = @"
                UPDATE ItemGroupsTbl SET ItemSortPos = @ItemSortPos
                WHERE GroupItemServiceTypeID = @GroupItemServiceTypeID AND ItemID = @ItemID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemSortPos", DataValue = sortPos, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemID", DataValue = itemId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool DeleteItemFromGroup(int groupReferenceItemId, int itemId)
        {
            const string sql = "DELETE FROM ItemGroupsTbl WHERE GroupItemServiceTypeID = @GroupItemServiceTypeID AND ItemID = @ItemID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemID", DataValue = itemId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public HashSet<int> GetItemIdsForGroup(int groupReferenceItemId)
        {
            var itemIds = new HashSet<int>();
            foreach (var groupItem in GetAllByGroupReferenceItemId(groupReferenceItemId))
            {
                if (groupItem?.ItemID != null && groupItem.ItemID.Value > 0)
                {
                    itemIds.Add(groupItem.ItemID.Value);
                }
            }

            return itemIds;
        }

        public List<ItemGroupGridRow> GetGridRowsByGroupReferenceItemId(int groupReferenceItemId, string sortBy = null)
        {
            var list = new List<ItemGroupGridRow>();
            if (groupReferenceItemId == SystemConstants.DatabaseConstants.InvalidID)
                return list;

            string orderBy = string.IsNullOrWhiteSpace(sortBy) ? "g.ItemSortPos" : sortBy;
            // Allow simple column names from ODS SortExpression without forcing table alias
            if (orderBy.IndexOf('.') < 0 && orderBy.IndexOf(',') < 0)
            {
                if (string.Equals(orderBy, "ItemTypeSortPos", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(orderBy, "ItemSortPos", StringComparison.OrdinalIgnoreCase))
                    orderBy = "g.ItemSortPos";
                else if (string.Equals(orderBy, "Enabled", StringComparison.OrdinalIgnoreCase))
                    orderBy = "g.Enabled";
                else if (string.Equals(orderBy, "ItemDesc", StringComparison.OrdinalIgnoreCase))
                    orderBy = "i.ItemDesc";
            }

            string sql = @"
                SELECT g.ItemGroupID, g.GroupItemServiceTypeID, g.ItemID, g.ItemSortPos, g.Enabled, g.Notes,
                       CASE WHEN i.ItemEnabled = 1 THEN i.ItemDesc ELSE '_' + ISNULL(i.ItemDesc, '') END AS ItemDesc
                FROM ItemGroupsTbl g
                LEFT JOIN ItemsTbl i ON i.ItemID = g.ItemID
                WHERE g.GroupItemServiceTypeID = @GroupItemServiceTypeID
                ORDER BY " + orderBy;

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new ItemGroupGridRow
                    {
                        ItemGroupID = rdr["ItemGroupID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemGroupID"]),
                        GroupItemTypeID = rdr["GroupItemServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["GroupItemServiceTypeID"]),
                        ItemTypeID = rdr["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemID"]),
                        ItemTypeSortPos = rdr["ItemSortPos"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemSortPos"]),
                        Enabled = rdr["Enabled"] == DBNull.Value || Convert.ToBoolean(rdr["Enabled"]),
                        Notes = rdr["Notes"] == DBNull.Value ? string.Empty : rdr["Notes"].ToString(),
                        ItemDesc = rdr["ItemDesc"] == DBNull.Value ? string.Empty : rdr["ItemDesc"].ToString()
                    });
                }
            }

            return list;
        }

        public bool InsertItemToGroup(int groupReferenceItemId, int itemId, string notes = null)
        {
            return InsertItemsToGroup(groupReferenceItemId, new[] { itemId }, notes) > 0;
        }

        public override int Insert(ItemGroup entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO ItemGroupsTbl
                    (GroupItemServiceTypeID, ItemID, ItemSortPos, Enabled, Notes)
                VALUES
                    (@GroupItemServiceTypeID, @ItemID, @ItemSortPos, @Enabled, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@GroupItemServiceTypeID",
                    DataValue = entity.GroupItemServiceTypeID,
                    DataDbType = DbType.Int32
                },
                new DBParameter
                {
                    ParamName = "@ItemID",
                    DataValue = FkOrDbNull(entity.ItemID),
                    DataDbType = DbType.Int32
                },
                new DBParameter
                {
                    ParamName = "@ItemSortPos",
                    DataValue = entity.ItemSortPos ?? (object)DBNull.Value,
                    DataDbType = DbType.Int32
                },
                new DBParameter
                {
                    ParamName = "@Enabled",
                    DataValue = entity.Enabled ?? true,
                    DataDbType = DbType.Boolean
                },
                new DBParameter
                {
                    ParamName = "@Notes",
                    DataValue = string.IsNullOrWhiteSpace(entity.Notes) ? (object)DBNull.Value : entity.Notes,
                    DataDbType = DbType.String
                }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        /// <summary>
        /// Adds multiple items with a single last-sort read, then incrementing ItemSortPos.
        /// </summary>
        public int InsertItemsToGroup(int groupReferenceItemId, IEnumerable<int> itemIds, string notes = null)
        {
            if (groupReferenceItemId <= 0 || itemIds == null)
                return 0;

            int sortPos = GetLastGroupItemSortPos(groupReferenceItemId);
            string noteText = notes ?? "added on ItemGroup form";
            int added = 0;

            foreach (int itemId in itemIds)
            {
                if (itemId <= 0)
                    continue;

                sortPos++;
                var entity = new ItemGroup
                {
                    GroupItemServiceTypeID = groupReferenceItemId,
                    ItemID = itemId,
                    ItemSortPos = sortPos,
                    Enabled = true,
                    Notes = noteText
                };

                if (Insert(entity) > 0)
                    added++;
            }

            return added;
        }

        public bool MoveItemSortUp(int groupReferenceItemId, int itemId, int currentSortPos)
        {
            if (currentSortPos <= 1) return false;

            var previous = GetPrevGroupItem(groupReferenceItemId, currentSortPos);
            if (previous?.ItemID == null) return false;

            int swapPos = previous.ItemSortPos ?? 0;
            UpdateItemSortPos(swapPos + 1, groupReferenceItemId, previous.ItemID.Value);
            return UpdateItemSortPos(swapPos, groupReferenceItemId, itemId);
        }

        public bool MoveItemSortDown(int groupReferenceItemId, int itemId, int currentSortPos)
        {
            int lastPos = GetLastGroupItemSortPos(groupReferenceItemId);
            if (currentSortPos >= lastPos) return false;

            var next = GetNextGroupItem(groupReferenceItemId, currentSortPos);
            if (next?.ItemID == null) return false;

            int swapPos = next.ItemSortPos ?? 0;
            UpdateItemSortPos(swapPos - 1, groupReferenceItemId, next.ItemID.Value);
            return UpdateItemSortPos(swapPos, groupReferenceItemId, itemId);
        }

        private ItemGroup GetSingleInGroup(int groupReferenceItemId, string sql, int? itemSortPos = null)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 }
            };

            if (itemSortPos.HasValue)
            {
                parameters.Add(new DBParameter { ParamName = "@ItemSortPos", DataValue = itemSortPos.Value, DataDbType = DbType.Int32 });
            }

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<ItemGroup>(rdr);
                }
            }

            return null;
        }

        private List<ItemGroup> QueryList(string sql, List<DBParameter> parameters)
        {
            var list = new List<ItemGroup>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(DbMapper.Map<ItemGroup>(rdr));
                }
            }

            return list;
        }
    }
}
