using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Models;
using TrackerSQL.Classes;

namespace TrackerSQL.Repositories
{
        public class AreaPrepDaysRepository : RepositoryBase<AreaPrepDays>
        {
        protected override string TableName => "AreaPrepDaysTbl";
        protected override string KeyColumn => "AreaPrepDaysID";

        public override List<AreaPrepDays> GetAll(string sortBy)
        {
            var list = new List<AreaPrepDays>();
            string sql = "SELECT AreaPrepDaysID, AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder FROM AreaPrepDaysTbl";
            if (!string.IsNullOrWhiteSpace(sortBy))
                sql += " ORDER BY " + sortBy;

            using (var rdr = ExecReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(DbMapper.Map<AreaPrepDays>(rdr));
            }

            return list;
        }

        public override int Update(AreaPrepDays entity)
        {
            const string sql = @"UPDATE AreaPrepDaysTbl 
                                 SET AreaID = @AreaID, 
                                     PrepDayOfWeekID = @PrepDayOfWeekID, 
                                     DeliveryDelayDays = @DeliveryDelayDays, 
                                     DeliveryOrder = @DeliveryOrder 
                                 WHERE AreaPrepDaysID = @AreaPrepDaysID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@AreaID", DataValue = entity.AreaID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PrepDayOfWeekID", DataValue = entity.PrepDayOfWeekID.HasValue ? (object)entity.PrepDayOfWeekID.Value : System.DBNull.Value, DataDbType = DbType.Byte },
                new DBParameter { ParamName = "@DeliveryDelayDays", DataValue = entity.DeliveryDelayDays.HasValue ? (object)entity.DeliveryDelayDays.Value : System.DBNull.Value, DataDbType = DbType.Int16 },
                new DBParameter { ParamName = "@DeliveryOrder", DataValue = entity.DeliveryOrder.HasValue ? (object)entity.DeliveryOrder.Value : System.DBNull.Value, DataDbType = DbType.Int16 },
                new DBParameter { ParamName = "@AreaPrepDaysID", DataValue = entity.AreaPrepDaysID, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters);
        }

        /// <summary>
        /// Sets the area delivery order on every prep-day row of each area, in one batch.
        /// An area has one delivery order: all of its prep-day rows carry the same value.
        /// </summary>
        public int UpdateAreaDeliveryOrders(IList<KeyValuePair<int, short>> deliveryOrdersByAreaId)
        {
            if (deliveryOrdersByAreaId == null || deliveryOrdersByAreaId.Count == 0)
                return 0;

            var sql = new System.Text.StringBuilder();
            var parameters = new List<DBParameter>();
            for (int i = 0; i < deliveryOrdersByAreaId.Count; i++)
            {
                sql.Append("UPDATE AreaPrepDaysTbl SET DeliveryOrder = @o").Append(i)
                   .Append(" WHERE AreaID = @a").Append(i).Append(";\n");
                parameters.Add(new DBParameter { ParamName = "@o" + i, DataValue = deliveryOrdersByAreaId[i].Value, DataDbType = DbType.Int16 });
                parameters.Add(new DBParameter { ParamName = "@a" + i, DataValue = deliveryOrdersByAreaId[i].Key, DataDbType = DbType.Int32 });
            }

            return ExecNonQuery(sql.ToString(), parameters);
        }

        /// <summary>
        /// Delivery order for a new prep-day row: the area's existing order, or after every other area
        /// when it is the area's first prep day.
        /// </summary>
        public short GetDeliveryOrderForNewPrepDay(int areaId)
        {
            const string sql = @"
                SELECT COALESCE(
                    (SELECT MIN(DeliveryOrder) FROM AreaPrepDaysTbl WHERE AreaID = @AreaID),
                    (SELECT ISNULL(MAX(DeliveryOrder), 0) + 10 FROM AreaPrepDaysTbl))";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@AreaID", DataValue = areaId, DataDbType = DbType.Int32 }
            };
            using (var db = CreateDb())
            {
                object result = db.ExecuteScalar(sql, parameters);
                return result == null || result == DBNull.Value ? (short)10 : Convert.ToInt16(result);
            }
        }

        /// <summary>
        /// Deletes a AreaPrepDays record by ID
        /// </summary>
        public override bool Delete(int id)
        {
            string sql = $"DELETE FROM {TableName} WHERE {KeyColumn} = @Id";
            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            int result = ExecNonQuery(sql, parameters);
            return result > 0;
        }
        /// <summary>
        /// Inserts a new AreaPrepDays record
        /// TODO: Fill in column names and parameters
        /// </summary>
        public override int Insert(AreaPrepDays entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            string sql = @"
                INSERT INTO AreaPrepDaysTbl ( AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder )
                VALUES (@AreaID, @PrepDayOfWeekID, @DeliveryDelayDays, @DeliveryOrder);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                // Add parameters for each property of AreaPrepDays except AreaPrepDaysID (which is identity)
                new DBParameter { ParamName = "@AreaID", DataValue = entity.AreaID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PrepDayOfWeekID", DataValue = entity.PrepDayOfWeekID.HasValue ? (object)entity.PrepDayOfWeekID.Value : System.DBNull.Value, DataDbType = DbType.Byte },
                new DBParameter { ParamName = "@DeliveryDelayDays", DataValue = entity.DeliveryDelayDays.HasValue ? (object)entity.DeliveryDelayDays.Value : System.DBNull.Value, DataDbType = DbType.Int16 },
                new DBParameter { ParamName = "@DeliveryOrder", DataValue = entity.DeliveryOrder.HasValue ? (object)entity.DeliveryOrder.Value : System.DBNull.Value, DataDbType = DbType.Int16 }
            };
            using (var db = CreateDb())
            {
                var result = db.ExecuteScalar(sql, parameters);
                return Convert.ToInt32(result);  //return the new ID of the inserted record
            }
            //return ExecuteScalar<int>(sql, parameters);
        }
    }
}

