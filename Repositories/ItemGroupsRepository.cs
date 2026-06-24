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
            "ItemGroupID, GroupReferenceItemID AS GroupItemServiceTypeID, ItemID, ItemSortPos, Enabled, Notes";

        protected override string TableName => "ItemGroupsTbl";
        protected override string KeyColumn => "ItemGroupID";

        protected override string CoreColumns =>
            "ItemGroupID, GroupReferenceItemID AS GroupItemServiceTypeID, ItemID, ItemSortPos, Enabled";

        protected override string LookupColumns =>
            "ItemGroupID, GroupReferenceItemID AS GroupItemServiceTypeID";

        public List<ItemGroup> GetAllByGroupReferenceItemId(int groupReferenceItemId, string sortBy = null)
        {
            if (groupReferenceItemId == SystemConstants.DatabaseConstants.InvalidID) return new List<ItemGroup>();

            string sql = $"SELECT {SelectColumns} FROM ItemGroupsTbl WHERE GroupReferenceItemID = @GroupReferenceItemID";
            sql += " ORDER BY " + (string.IsNullOrEmpty(sortBy) ? "ItemSortPos" : sortBy);

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@GroupReferenceItemID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 }
            };

            return QueryList(sql, parameters);
        }

        public ItemGroup GetFirstGroupItem(int groupReferenceItemId)
        {
            return GetSingleInGroup(groupReferenceItemId,
                "SELECT TOP 1 " + SelectColumns + " FROM ItemGroupsTbl WHERE GroupReferenceItemID = @GroupReferenceItemID ORDER BY ItemSortPos");
        }

        public ItemGroup GetNextGroupItem(int groupReferenceItemId, int lastItemSortPos)
        {
            var item = GetSingleInGroup(groupReferenceItemId,
                "SELECT TOP 1 " + SelectColumns + " FROM ItemGroupsTbl WHERE GroupReferenceItemID = @GroupReferenceItemID AND ItemSortPos > @ItemSortPos ORDER BY ItemSortPos",
                lastItemSortPos);
            return item ?? GetFirstGroupItem(groupReferenceItemId);
        }

        public ItemGroup GetPrevGroupItem(int groupReferenceItemId, int itemSortPos)
        {
            var item = GetSingleInGroup(groupReferenceItemId,
                "SELECT TOP 1 " + SelectColumns + " FROM ItemGroupsTbl WHERE GroupReferenceItemID = @GroupReferenceItemID AND ItemSortPos < @ItemSortPos ORDER BY ItemSortPos DESC",
                itemSortPos);
            return item ?? GetLastGroupItem(groupReferenceItemId);
        }

        public ItemGroup GetLastGroupItem(int groupReferenceItemId)
        {
            return GetSingleInGroup(groupReferenceItemId,
                "SELECT TOP 1 " + SelectColumns + " FROM ItemGroupsTbl WHERE GroupReferenceItemID = @GroupReferenceItemID ORDER BY ItemSortPos DESC");
        }

        public int GetLastGroupItemSortPos(int groupReferenceItemId)
        {
            var item = GetLastGroupItem(groupReferenceItemId);
            return item?.ItemSortPos ?? -1;
        }

        public int GetItemIdAtSortPos(int groupReferenceItemId, int sortPos)
        {
            const string sql = "SELECT ItemID FROM ItemGroupsTbl WHERE GroupReferenceItemID = @GroupReferenceItemID AND ItemSortPos = @ItemSortPos";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@GroupReferenceItemID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemSortPos", DataValue = sortPos, DataDbType = DbType.Int32 }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public bool UpdateItemSortPos(int sortPos, int groupReferenceItemId, int itemId)
        {
            const string sql = @"
                UPDATE ItemGroupsTbl SET ItemSortPos = @ItemSortPos
                WHERE GroupReferenceItemID = @GroupReferenceItemID AND ItemID = @ItemID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ItemSortPos", DataValue = sortPos, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@GroupReferenceItemID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemID", DataValue = itemId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool DeleteItemFromGroup(int groupReferenceItemId, int itemId)
        {
            const string sql = "DELETE FROM ItemGroupsTbl WHERE GroupReferenceItemID = @GroupReferenceItemID AND ItemID = @ItemID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@GroupReferenceItemID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 },
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
            foreach (var group in GetAllByGroupReferenceItemId(groupReferenceItemId, sortBy))
            {
                list.Add(new ItemGroupGridRow
                {
                    ItemGroupID = group.ItemGroupID,
                    GroupItemTypeID = group.GroupItemServiceTypeID,
                    ItemTypeID = group.ItemID ?? 0,
                    ItemTypeSortPos = group.ItemSortPos ?? 0,
                    Enabled = group.Enabled ?? true,
                    Notes = group.Notes ?? string.Empty,
                    ItemDesc = group.ItemID.HasValue ? new ItemsRepository().GetItemDescById(group.ItemID.Value) : string.Empty
                });
            }

            return list;
        }

        public bool InsertItemToGroup(int groupReferenceItemId, int itemId, string notes = null)
        {
            var entity = new ItemGroup
            {
                GroupItemServiceTypeID = groupReferenceItemId,
                ItemID = itemId,
                ItemSortPos = GetLastGroupItemSortPos(groupReferenceItemId) + 1,
                Enabled = true,
                Notes = notes ?? "added on ItemGroup form"
            };

            return Insert(entity) > 0;
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
                new DBParameter { ParamName = "@GroupReferenceItemID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 }
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
