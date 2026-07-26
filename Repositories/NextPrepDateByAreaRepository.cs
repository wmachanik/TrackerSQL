using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class NextPrepDateByAreaRepository : RepositoryBase<NextPreparationDateByArea>
    {
        protected override string TableName => "NextPreparationDateByAreasTbl";
        protected override string KeyColumn => "NextPrepDayID";

        protected override string CoreColumns =>
            "NextPrepDayID, AreaID, PreparationDate, DeliveryDate, DeliveryOrder, NextPreparationDate, NextDeliveryDate";

        public NextPreparationDateByArea GetPrepDataForContact(int contactId)
        {
            const string sql = @"
                SELECT n.NextPrepDayID, n.AreaID, n.PreparationDate, n.DeliveryDate, n.DeliveryOrder,
                       n.NextPreparationDate, n.NextDeliveryDate
                FROM NextPreparationDateByAreasTbl n
                RIGHT OUTER JOIN ContactsTbl c ON n.AreaID = c.AreaID
                WHERE c.ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<NextPreparationDateByArea>(rdr);
                }
            }

            return new NextPreparationDateByArea();
        }

        public List<DateTime> GetAllDeliveryDates()
        {
            var list = new List<DateTime>();
            const string sql = "SELECT DISTINCT DeliveryDate FROM NextPreparationDateByAreasTbl ORDER BY DeliveryDate";

            using (var rdr = ExecReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    if (rdr["DeliveryDate"] != DBNull.Value)
                    {
                        list.Add(Convert.ToDateTime(rdr["DeliveryDate"]).Date);
                    }
                }
            }

            return list;
        }

        public List<int> GetIdsByDeliveryDate(DateTime deliveryDate)
        {
            var list = new List<int>();
            const string sql = "SELECT NextPrepDayID FROM NextPreparationDateByAreasTbl WHERE DeliveryDate = @DeliveryDate";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@DeliveryDate", DataValue = deliveryDate.Date, DataDbType = DbType.Date }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(Convert.ToInt32(rdr["NextPrepDayID"]));
                }
            }

            return list;
        }

        public DateTime? GetNextDeliveryDateForContact(int contactId)
        {
            const string sql = @"
                SELECT n.DeliveryDate
                FROM ContactsTbl c
                INNER JOIN NextPreparationDateByAreasTbl n ON c.AreaID = n.AreaID
                WHERE c.ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read() && rdr["DeliveryDate"] != DBNull.Value)
                {
                    return Convert.ToDateTime(rdr["DeliveryDate"]).Date;
                }
            }

            return null;
        }

        public int UpdatePrepDataForArea(NextPreparationDateByArea data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            const string sql = @"
                UPDATE NextPreparationDateByAreasTbl
                SET PreparationDate = @PreparationDate,
                    DeliveryDate = @DeliveryDate,
                    DeliveryOrder = @DeliveryOrder,
                    NextDeliveryDate = @NextDeliveryDate,
                    NextPreparationDate = @NextPreparationDate
                WHERE AreaID = @AreaID";

            return ExecNonQuery(sql, BuildAreaParameters(data));
        }

        public int InsertPrepDataForArea(NextPreparationDateByArea data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            const string sql = @"
                INSERT INTO NextPreparationDateByAreasTbl
                (AreaID, PreparationDate, DeliveryDate, DeliveryOrder, NextDeliveryDate, NextPreparationDate)
                VALUES
                (@AreaID, @PreparationDate, @DeliveryDate, @DeliveryOrder, @NextDeliveryDate, @NextPreparationDate);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return ExecuteScalar<int>(sql, BuildAreaParameters(data));
        }

        public int MoveDeliveryDate(DateTime oldDeliveryDate, DateTime newDeliveryDate)
        {
            const string sql = @"
                UPDATE NextPreparationDateByAreasTbl
                SET DeliveryDate = @NewDeliveryDate
                WHERE DeliveryDate = @OldDeliveryDate";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@NewDeliveryDate", DataValue = newDeliveryDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@OldDeliveryDate", DataValue = oldDeliveryDate.Date, DataDbType = DbType.Date }
            };

            return ExecNonQuery(sql, parameters);
        }

        public int UpdateDeliveryDateById(int nextPrepDayId, DateTime deliveryDate)
        {
            const string sql = @"
                UPDATE NextPreparationDateByAreasTbl
                SET DeliveryDate = @DeliveryDate
                WHERE NextPrepDayID = @NextPrepDayID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@DeliveryDate", DataValue = deliveryDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextPrepDayID", DataValue = nextPrepDayId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters);
        }

        public List<AreaPrepDateRow> GetAreaPrepDateGrid()
        {
            var list = new List<AreaPrepDateRow>();
            const string sql = @"
                SELECT a.AreaName AS Area, n.PreparationDate, n.DeliveryDate, n.NextPreparationDate, n.NextDeliveryDate
                FROM NextPreparationDateByAreasTbl n
                LEFT OUTER JOIN AreasTbl a ON n.AreaID = a.AreaID
                ORDER BY n.DeliveryDate, a.AreaName";

            using (var rdr = ExecReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new AreaPrepDateRow
                    {
                        Area = rdr["Area"] == DBNull.Value ? string.Empty : rdr["Area"].ToString(),
                        PreparationDate = rdr["PreparationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["PreparationDate"]),
                        DeliveryDate = rdr["DeliveryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["DeliveryDate"]),
                        NextPreparationDate = rdr["NextPreparationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["NextPreparationDate"]),
                        NextDeliveryDate = rdr["NextDeliveryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["NextDeliveryDate"])
                    });
                }
            }

            return list;
        }

        private static List<DBParameter> BuildAreaParameters(NextPreparationDateByArea data)
        {
            return new List<DBParameter>
            {
                new DBParameter { ParamName = "@AreaID", DataValue = data.AreaID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@PreparationDate", DataValue = data.PreparationDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@DeliveryDate", DataValue = data.DeliveryDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@DeliveryOrder", DataValue = data.DeliveryOrder ?? (object)DBNull.Value, DataDbType = DbType.Int16 },
                new DBParameter { ParamName = "@NextDeliveryDate", DataValue = data.NextDeliveryDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextPreparationDate", DataValue = data.NextPreparationDate ?? (object)DBNull.Value, DataDbType = DbType.Date }
            };
        }
    }
}
