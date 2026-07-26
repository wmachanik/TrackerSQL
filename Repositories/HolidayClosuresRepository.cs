using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// SQL access for HolidayClosuresTbl. Business date-shift rules live in HolidayClosureManager.
    /// </summary>
    public class HolidayClosuresRepository : RepositoryBase<HolidayClosure>
    {
        protected override string TableName => "HolidayClosuresTbl";
        protected override string KeyColumn => "HolidayClosureID";

        protected override string CoreColumns =>
            "HolidayClosureID, ClosureDate, DaysClosed, AppliesToPrep, AppliesToDelivery, ShiftStrategy, Description";

        public List<HolidayClosure> GetAllOrdered()
        {
            const string sql = @"
                SELECT HolidayClosureID, ClosureDate, DaysClosed, AppliesToPrep, AppliesToDelivery, ShiftStrategy, Description
                FROM HolidayClosuresTbl
                ORDER BY ClosureDate";

            var list = new List<HolidayClosure>();
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(MapRow(rdr));
            }

            return list;
        }

        /// <summary>
        /// Closures whose date span overlaps [rangeStart, rangeEnd] (inclusive).
        /// Pass excludeId &gt; 0 to ignore the row being edited.
        /// </summary>
        public List<HolidayClosure> GetOverlappingRange(DateTime rangeStart, DateTime rangeEnd, int excludeId = 0)
        {
            rangeStart = rangeStart.Date;
            rangeEnd = rangeEnd.Date;

            const string sql = @"
                SELECT HolidayClosureID, ClosureDate, DaysClosed, AppliesToPrep, AppliesToDelivery, ShiftStrategy, Description
                FROM HolidayClosuresTbl
                WHERE ClosureDate IS NOT NULL
                  AND ClosureDate <= @RangeEnd
                  AND DATEADD(day, CASE WHEN ISNULL(DaysClosed, 1) < 1 THEN 0 ELSE ISNULL(DaysClosed, 1) - 1 END, ClosureDate) >= @RangeStart
                  AND (@ExcludeId <= 0 OR HolidayClosureID <> @ExcludeId)
                ORDER BY ClosureDate";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RangeStart", DataValue = rangeStart, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@RangeEnd", DataValue = rangeEnd, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ExcludeId", DataValue = excludeId, DataDbType = DbType.Int32 }
            };

            var list = new List<HolidayClosure>();
            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                    list.Add(MapRow(rdr));
            }

            return list;
        }

        /// <summary>True when another closure already uses this exact start date.</summary>
        public bool ExistsWithStartDate(DateTime closureDate, int excludeId = 0)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM HolidayClosuresTbl
                WHERE ClosureDate = @ClosureDate
                  AND (@ExcludeId <= 0 OR HolidayClosureID <> @ExcludeId)";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ClosureDate", DataValue = closureDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ExcludeId", DataValue = excludeId, DataDbType = DbType.Int32 }
            };

            return ExecuteScalar<int>(sql, parameters) > 0;
        }

        public override HolidayClosure GetById(int id)
        {
            const string sql = @"
                SELECT HolidayClosureID, ClosureDate, DaysClosed, AppliesToPrep, AppliesToDelivery, ShiftStrategy, Description
                FROM HolidayClosuresTbl
                WHERE HolidayClosureID = @Id";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Id", DataValue = id, DataDbType = DbType.Int32 }
            };

            using (var db = CreateDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                    return MapRow(rdr);
            }

            return null;
        }

        public override int Insert(HolidayClosure entity)
        {
            if (entity == null)
                return -1;

            Normalize(entity);

            const string sql = @"
                INSERT INTO HolidayClosuresTbl
                    (ClosureDate, DaysClosed, AppliesToPrep, AppliesToDelivery, ShiftStrategy, Description)
                VALUES
                    (@ClosureDate, @DaysClosed, @AppliesToPrep, @AppliesToDelivery, @ShiftStrategy, @Description);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = BuildWriteParameters(entity);
            int newId = ExecuteScalar<int>(sql, parameters);
            return newId > 0 ? newId : -1;
        }

        public override int Update(HolidayClosure entity)
        {
            if (entity == null || entity.HolidayClosureID <= 0)
                return 0;

            Normalize(entity);

            const string sql = @"
                UPDATE HolidayClosuresTbl
                SET ClosureDate = @ClosureDate,
                    DaysClosed = @DaysClosed,
                    AppliesToPrep = @AppliesToPrep,
                    AppliesToDelivery = @AppliesToDelivery,
                    ShiftStrategy = @ShiftStrategy,
                    Description = @Description
                WHERE HolidayClosureID = @HolidayClosureID";

            var parameters = BuildWriteParameters(entity);
            parameters.Add(new DBParameter
            {
                ParamName = "@HolidayClosureID",
                DataValue = entity.HolidayClosureID,
                DataDbType = DbType.Int32
            });

            return ExecNonQuery(sql, parameters);
        }

        public override bool Delete(int id)
        {
            if (id <= 0)
                return false;

            const string sql = "DELETE FROM HolidayClosuresTbl WHERE HolidayClosureID = @Id";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Id", DataValue = id, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        private static List<DBParameter> BuildWriteParameters(HolidayClosure entity)
        {
            return new List<DBParameter>
            {
                new DBParameter { ParamName = "@ClosureDate", DataValue = entity.ClosureDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@DaysClosed", DataValue = entity.DaysClosed ?? 1, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@AppliesToPrep", DataValue = entity.AppliesToPrep ?? true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@AppliesToDelivery", DataValue = entity.AppliesToDelivery ?? true, DataDbType = DbType.Boolean },
                new DBParameter
                {
                    ParamName = "@ShiftStrategy",
                    DataValue = string.IsNullOrWhiteSpace(entity.ShiftStrategy) ? "Forward" : entity.ShiftStrategy.Trim(),
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@Description",
                    DataValue = string.IsNullOrWhiteSpace(entity.Description) ? (object)DBNull.Value : entity.Description.Trim(),
                    DataDbType = DbType.String
                }
            };
        }

        private static HolidayClosure MapRow(IDataRecord rdr)
        {
            var entity = new HolidayClosure
            {
                HolidayClosureID = rdr["HolidayClosureID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["HolidayClosureID"]),
                ClosureDate = rdr["ClosureDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["ClosureDate"]).Date,
                DaysClosed = rdr["DaysClosed"] == DBNull.Value ? 1 : Convert.ToInt32(rdr["DaysClosed"]),
                AppliesToPrep = rdr["AppliesToPrep"] != DBNull.Value && Convert.ToBoolean(rdr["AppliesToPrep"]),
                AppliesToDelivery = rdr["AppliesToDelivery"] != DBNull.Value && Convert.ToBoolean(rdr["AppliesToDelivery"]),
                ShiftStrategy = rdr["ShiftStrategy"] == DBNull.Value ? "Forward" : Convert.ToString(rdr["ShiftStrategy"]),
                Description = rdr["Description"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["Description"])
            };

            Normalize(entity);
            return entity;
        }

        private static void Normalize(HolidayClosure entity)
        {
            if (entity == null)
                return;

            entity.ClosureDate = entity.ClosureDate.Date;
            if (!entity.DaysClosed.HasValue || entity.DaysClosed.Value < 1)
                entity.DaysClosed = 1;
            if (!entity.AppliesToPrep.HasValue)
                entity.AppliesToPrep = true;
            if (!entity.AppliesToDelivery.HasValue)
                entity.AppliesToDelivery = true;
            if (string.IsNullOrWhiteSpace(entity.ShiftStrategy))
                entity.ShiftStrategy = "Forward";
            entity.Description = entity.Description ?? string.Empty;
        }
    }
}
