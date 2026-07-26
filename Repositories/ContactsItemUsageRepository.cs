using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactsItemUsageRepository : RepositoryBase<ContactsItemUsage>
    {
        protected override string TableName => "ContactsItemUsageTbl";
        protected override string KeyColumn => "ContactItemUsageLineNo";

        protected override string CoreColumns =>
            "ContactItemUsageLineNo, ContactID, DeliveryDate, ItemProvidedID, QtyProvided, ItemPrepTypeID, ItemPackagingID, Notes";

        /// <summary>
        /// Gets item usage records for a contact.
        /// </summary>
        public List<ContactsItemUsage> GetByContactId(int contactId, string sortBy = "DeliveryDate DESC")
        {
            string sql = $@"
                SELECT {CoreColumns}
                FROM {TableName}
                WHERE ContactID = @ContactID
                ORDER BY {MapSortColumnOrDefault(sortBy)}";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@ContactID",
                    DataValue = contactId,
                    DataDbType = DbType.Int32
                }
            };

            var list = new List<ContactsItemUsage>();

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(DbMapper.Map<ContactsItemUsage>(rdr));
                }
            }

            return list;
        }

        public bool InsertUsageLine(ContactsItemUsage usage)
        {
            return Insert(usage) > 0;
        }

        public override int Insert(ContactsItemUsage entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO ContactsItemUsageTbl
                (ContactID, DeliveryDate, ItemProvidedID, QtyProvided, ItemPrepTypeID, ItemPackagingID, Notes)
                VALUES
                (@ContactID, @DeliveryDate, @ItemProvidedID, @QtyProvided, @ItemPrepTypeID, @ItemPackagingID, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = entity.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@DeliveryDate", DataValue = entity.DeliveryDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ItemProvidedID", DataValue = FkOrDbNull(entity.ItemProvidedID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@QtyProvided", DataValue = entity.QtyProvided ?? (object)DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@ItemPrepTypeID", DataValue = FkOrDbNull(entity.ItemPrepTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemPackagingID", DataValue = FkOrDbNull(entity.ItemPackagingID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Notes", DataValue = entity.Notes ?? (object)DBNull.Value, DataDbType = DbType.String }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public override int Update(ContactsItemUsage entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.ContactItemUsageLineNo <= 0) return 0;

            const string sql = @"
                UPDATE ContactsItemUsageTbl SET
                    DeliveryDate = @DeliveryDate,
                    ItemProvidedID = @ItemProvidedID,
                    QtyProvided = @QtyProvided,
                    ItemPrepTypeID = @ItemPrepTypeID,
                    ItemPackagingID = @ItemPackagingID,
                    Notes = @Notes
                WHERE ContactItemUsageLineNo = @ContactItemUsageLineNo
                  AND ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@DeliveryDate", DataValue = entity.DeliveryDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ItemProvidedID", DataValue = FkOrDbNull(entity.ItemProvidedID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@QtyProvided", DataValue = entity.QtyProvided ?? (object)DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@ItemPrepTypeID", DataValue = FkOrDbNull(entity.ItemPrepTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemPackagingID", DataValue = FkOrDbNull(entity.ItemPackagingID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Notes", DataValue = entity.Notes ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactItemUsageLineNo", DataValue = entity.ContactItemUsageLineNo, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = entity.ContactID, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters);
        }

        public bool DeleteUsageLine(int contactItemUsageLineNo, int contactId)
        {
            const string sql = @"
                DELETE FROM ContactsItemUsageTbl
                WHERE ContactItemUsageLineNo = @ContactItemUsageLineNo
                  AND ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactItemUsageLineNo", DataValue = contactItemUsageLineNo, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public List<ContactsItemUsage> GetLastItemsUsed(int contactId, int itemServiceTypeId)
        {
            const string sql = @"
                SELECT u.ContactItemUsageLineNo, u.ContactID, u.DeliveryDate, u.ItemProvidedID,
                       u.QtyProvided, u.ItemPrepTypeID, u.ItemPackagingID, u.Notes
                FROM ItemsTbl
                INNER JOIN ContactsItemUsageTbl u ON ItemsTbl.ItemID = u.ItemProvidedID
                WHERE u.ContactID = @ContactID
                  AND ItemsTbl.ItemServiceTypeID = @ItemServiceTypeID
                  AND u.DeliveryDate = (
                      SELECT MAX(u2.DeliveryDate)
                      FROM ItemsTbl i2
                      INNER JOIN ContactsItemUsageTbl u2 ON i2.ItemID = u2.ItemProvidedID
                      WHERE u2.ContactID = @ContactID AND i2.ItemServiceTypeID = @ItemServiceTypeID)";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemServiceTypeID", DataValue = itemServiceTypeId, DataDbType = DbType.Int32 }
            };

            var list = new List<ContactsItemUsage>();
            var usedItemGroupRepository = new UsedItemGroupRepository();

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    var usage = DbMapper.Map<ContactsItemUsage>(rdr);
                    int groupItemId = usedItemGroupRepository.ChangeItemIdToGroupIfItWas(
                        contactId,
                        usage.ItemProvidedID ?? 0,
                        usage.DeliveryDate ?? TimeZoneUtils.Now().Date);

                    if (groupItemId != usage.ItemProvidedID)
                    {
                        usage.ItemProvidedID = groupItemId;
                        usage.Notes = "Last item was a group item";
                    }

                    list.Add(usage);
                }
            }

            return list;
        }

        private static string MapSortColumnOrDefault(string sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return "DeliveryDate DESC";
            }

            string trimmed = sortBy.Trim();

            if (trimmed.Equals("DeliveryDate", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("DeliveryDate ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "DeliveryDate ASC";
            }

            if (trimmed.Equals("DeliveryDate DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "DeliveryDate DESC";
            }

            if (trimmed.Equals("ContactItemUsageLineNo", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("ContactItemUsageLineNo ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "ContactItemUsageLineNo ASC";
            }

            if (trimmed.Equals("ContactItemUsageLineNo DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "ContactItemUsageLineNo DESC";
            }

            if (trimmed.Equals("QtyProvided", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("QtyProvided ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "QtyProvided ASC";
            }

            if (trimmed.Equals("QtyProvided DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "QtyProvided DESC";
            }

            return "DeliveryDate DESC";
        }
    }
}