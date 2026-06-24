using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactUsageLinesRepository : RepositoryBase<ContactUsageLine>
    {
        protected override string TableName => "ContactsItemSvcSummaryTbl";
        protected override string KeyColumn => "ContactsItemSvcSummaryId";

        protected override string CoreColumns =>
            "ContactsItemSvcSummaryId, ContactID, UsageDate, CupCount, ItemServiceTypeID, Qty, Notes";

        public bool InsertUsageLine(ContactUsageLine line)
        {
            return Insert(line) > 0;
        }

        public override int Insert(ContactUsageLine entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO ContactsItemSvcSummaryTbl
                (ContactID, UsageDate, CupCount, ItemServiceTypeID, Qty, Notes)
                VALUES
                (@ContactID, @UsageDate, @CupCount, @ItemServiceTypeID, @Qty, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = entity.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@UsageDate", DataValue = entity.UsageDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@CupCount", DataValue = entity.CupCount ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ItemServiceTypeID", DataValue = entity.ItemServiceTypeID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Qty", DataValue = entity.Qty ?? (object)DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@Notes", DataValue = entity.Notes ?? (object)DBNull.Value, DataDbType = DbType.String }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public ContactUsageLine GetLatestUsageLine(int contactId, int serviceTypeId)
        {
            string sql = $@"
                SELECT TOP 1 {CoreColumns}
                FROM {TableName}
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            if (serviceTypeId > 0)
            {
                sql += " AND ItemServiceTypeID = @ItemServiceTypeID";
                parameters.Add(new DBParameter { ParamName = "@ItemServiceTypeID", DataValue = serviceTypeId, DataDbType = DbType.Int32 });
            }

            sql += " ORDER BY UsageDate DESC";

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<ContactUsageLine>(rdr);
                }
            }

            return null;
        }
    }
}