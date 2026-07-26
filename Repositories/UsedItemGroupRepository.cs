using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class UsedItemGroupRepository : RepositoryBase<UsedItemGroup>
    {
        private const string SelectColumns = @"
            UsedItemGroupID, ContactID,
            GroupItemServiceTypeID,
            LastItemID, LastItemSortPos, LastItemDateChanged, Notes";

        protected override string TableName => "UsedItemGroupsTbl";
        protected override string KeyColumn => "UsedItemGroupID";

        protected override string CoreColumns => SelectColumns;

        public UsedItemGroup GetContactLastGroupItem(long contactId, int groupReferenceItemId)
        {
            const string sql = @"
                SELECT UsedItemGroupID, LastItemID, LastItemSortPos, LastItemDateChanged, Notes,
                       GroupItemServiceTypeID, ContactID
                FROM UsedItemGroupsTbl
                WHERE ContactID = @ContactID AND GroupItemServiceTypeID = @GroupItemServiceTypeID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = groupReferenceItemId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    var entity = DbMapper.Map<UsedItemGroup>(rdr);
                    entity.ContactID = (int)contactId;
                    entity.GroupItemServiceTypeID = groupReferenceItemId;
                    return entity;
                }
            }

            return new UsedItemGroup
            {
                UsedItemGroupID = SystemConstants.DatabaseConstants.InvalidID,
                ContactID = (int)contactId,
                GroupItemServiceTypeID = groupReferenceItemId
            };
        }

        public UsedItemGroup GetLastUsedItemId(long contactId, int itemId, DateTime deliveryDate)
        {
            const string sql = @"
                SELECT UsedItemGroupID, GroupItemServiceTypeID,
                       LastItemSortPos, Notes, ContactID, LastItemID, LastItemDateChanged
                FROM UsedItemGroupsTbl
                WHERE ContactID = @ContactID AND LastItemID = @LastItemID AND LastItemDateChanged = @LastItemDateChanged";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@LastItemID", DataValue = itemId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@LastItemDateChanged", DataValue = deliveryDate.Date, DataDbType = DbType.Date }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    var entity = DbMapper.Map<UsedItemGroup>(rdr);
                    entity.ContactID = (int)contactId;
                    entity.LastItemID = itemId;
                    entity.LastItemDateChanged = deliveryDate.Date;
                    return entity;
                }
            }

            return new UsedItemGroup { UsedItemGroupID = SystemConstants.DatabaseConstants.InvalidID };
        }

        public int ChangeItemIdToGroupIfItWas(long contactId, int itemId, DateTime deliveryDate)
        {
            var row = GetLastUsedItemId(contactId, itemId, deliveryDate);
            if (row.UsedItemGroupID != SystemConstants.DatabaseConstants.InvalidID && row.GroupItemServiceTypeID.HasValue)
            {
                return row.GroupItemServiceTypeID.Value;
            }

            return itemId;
        }

        public bool UpdateIfGroupItem(long contactId, int itemId, DateTime oldDeliveryDate, DateTime newDeliveryDate)
        {
            if (newDeliveryDate.Date.Equals(oldDeliveryDate.Date))
                return false;

            var row = GetLastUsedItemId(contactId, itemId, oldDeliveryDate);
            if (row.UsedItemGroupID == SystemConstants.DatabaseConstants.InvalidID)
                return false;

            return UpdateLastItemDateChanged(row.UsedItemGroupID, newDeliveryDate);
        }

        public int GetNextGroupItemId(long contactId, int groupReferenceItemId, DateTime deliveryDate)
        {
            var itemGroupsRepository = new ItemGroupsRepository();
            var existing = GetContactLastGroupItem(contactId, groupReferenceItemId);
            ItemGroup nextItem;

            if (existing.UsedItemGroupID == SystemConstants.DatabaseConstants.InvalidID)
            {
                nextItem = itemGroupsRepository.GetFirstGroupItem(groupReferenceItemId);
                if (nextItem?.ItemID == null)
                {
                    return groupReferenceItemId;
                }

                Insert(new UsedItemGroup
                {
                    ContactID = (int)contactId,
                    GroupItemServiceTypeID = groupReferenceItemId,
                    LastItemID = nextItem.ItemID,
                    LastItemSortPos = nextItem.ItemSortPos,
                    LastItemDateChanged = deliveryDate.Date
                });
                return nextItem.ItemID.Value;
            }

            nextItem = itemGroupsRepository.GetNextGroupItem(groupReferenceItemId, existing.LastItemSortPos ?? 0);
            if (nextItem?.ItemID == null)
            {
                return groupReferenceItemId;
            }

            existing.LastItemID = nextItem.ItemID;
            existing.LastItemSortPos = nextItem.ItemSortPos;
            existing.LastItemDateChanged = deliveryDate.Date;
            Update(existing);
            return nextItem.ItemID.Value;
        }

        public bool UpdateLastItemDateChanged(int usedItemGroupId, DateTime newDeliveryDate)
        {
            const string sql = @"
                UPDATE UsedItemGroupsTbl SET LastItemDateChanged = @LastItemDateChanged
                WHERE UsedItemGroupID = @UsedItemGroupID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@LastItemDateChanged", DataValue = newDeliveryDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@UsedItemGroupID", DataValue = usedItemGroupId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public override int Insert(UsedItemGroup entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO UsedItemGroupsTbl
                (ContactID, GroupItemServiceTypeID, LastItemID, LastItemSortPos, LastItemDateChanged, Notes)
                VALUES
                (@ContactID, @GroupItemServiceTypeID, @LastItemID, @LastItemSortPos, @LastItemDateChanged, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return ExecuteScalar<int>(sql, BuildParameters(entity));
        }

        public override int Update(UsedItemGroup entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                UPDATE UsedItemGroupsTbl SET
                    ContactID = @ContactID,
                    GroupItemServiceTypeID = @GroupItemServiceTypeID,
                    LastItemID = @LastItemID,
                    LastItemSortPos = @LastItemSortPos,
                    LastItemDateChanged = @LastItemDateChanged,
                    Notes = @Notes
                WHERE UsedItemGroupID = @UsedItemGroupID";

            return ExecNonQuery(sql, BuildParameters(entity, includeId: true));
        }

        private static List<DBParameter> BuildParameters(UsedItemGroup entity, bool includeId = false)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = entity.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@GroupItemServiceTypeID", DataValue = entity.GroupItemServiceTypeID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@LastItemID", DataValue = entity.LastItemID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@LastItemSortPos", DataValue = entity.LastItemSortPos ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@LastItemDateChanged", DataValue = entity.LastItemDateChanged ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@Notes", DataValue = entity.Notes ?? (object)DBNull.Value, DataDbType = DbType.String }
            };

            if (includeId)
            {
                parameters.Add(new DBParameter { ParamName = "@UsedItemGroupID", DataValue = entity.UsedItemGroupID, DataDbType = DbType.Int32 });
            }

            return parameters;
        }
    }
}
