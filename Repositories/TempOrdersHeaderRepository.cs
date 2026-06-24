using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class TempOrdersHeaderRepository : RepositoryBase<TempOrdersHeader>
    {
        private const string SelectColumns =
            "TOHeaderID, ContactID, OrderDate, RoastDate AS PrepDate, RequiredByDate, ToBeDeliveredByID, Confirmed, Done, Notes";

        protected override string TableName => "TempOrdersHeaderTbl";
        protected override string KeyColumn => "TOHeaderID";

        protected override string CoreColumns => SelectColumns;

        public TempOrdersHeader GetFirst()
        {
            const string sql = "SELECT TOP 1 " + SelectColumns + " FROM TempOrdersHeaderTbl ORDER BY TOHeaderID DESC";

            using (var rdr = ExecReader(sql))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<TempOrdersHeader>(rdr);
                }
            }

            return null;
        }

        public override TempOrdersHeader GetById(int headerId)
        {
            if (headerId <= 0) return null;

            const string sql = "SELECT " + SelectColumns + " FROM TempOrdersHeaderTbl WHERE TOHeaderID = @TOHeaderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = headerId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<TempOrdersHeader>(rdr);
                }
            }

            return null;
        }

        public int InsertHeader(TempOrdersHeader header)
        {
            return Insert(header);
        }

        public int GetCurrentHeaderId()
        {
            const string sql = "SELECT TOP 1 TOHeaderID FROM TempOrdersHeaderTbl ORDER BY TOHeaderID DESC";
            return ExecuteScalar<int>(sql);
        }

        public bool DeleteByHeaderId(int headerId)
        {
            if (headerId <= 0) return false;

            const string sql = "DELETE FROM TempOrdersHeaderTbl WHERE TOHeaderID = @TOHeaderID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = headerId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) >= 0;
        }

        public bool DeleteAllRecords()
        {
            return ExecNonQuery("DELETE FROM TempOrdersHeaderTbl") >= 0;
        }

        public OrderDoneHeaderView GetOrderDoneHeaderView(int headerId)
        {
            if (headerId <= 0) return null;

            const string sql = @"
                SELECT c.CompanyName, h.ContactID AS CustomerID, h.RequiredByDate
                FROM TempOrdersHeaderTbl h
                INNER JOIN ContactsTbl c ON h.ContactID = c.ContactID
                WHERE h.TOHeaderID = @TOHeaderID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@TOHeaderID", DataValue = headerId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return new OrderDoneHeaderView
                    {
                        CompanyName = rdr["CompanyName"] == DBNull.Value ? string.Empty : rdr["CompanyName"].ToString(),
                        CustomerID = rdr["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["CustomerID"]),
                        RequiredByDate = rdr["RequiredByDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["RequiredByDate"]).Date
                    };
                }
            }

            return null;
        }

        public override int Insert(TempOrdersHeader entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO TempOrdersHeaderTbl
                (ContactID, OrderDate, RoastDate, RequiredByDate, ToBeDeliveredByID, Confirmed, Done, Notes)
                VALUES
                (@ContactID, @OrderDate, @PrepDate, @RequiredByDate, @ToBeDeliveredByID, @Confirmed, @Done, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return ExecuteScalar<int>(sql, BuildParameters(entity));
        }

        private static List<DBParameter> BuildParameters(TempOrdersHeader entity)
        {
            return new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = entity.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@OrderDate", DataValue = entity.OrderDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@PrepDate", DataValue = entity.PrepDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@RequiredByDate", DataValue = entity.RequiredByDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ToBeDeliveredByID", DataValue = entity.ToBeDeliveredByID ?? (object)DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Confirmed", DataValue = entity.Confirmed ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Done", DataValue = entity.Done ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = entity.Notes ?? (object)DBNull.Value, DataDbType = DbType.String }
            };
        }
    }
}
