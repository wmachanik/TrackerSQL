using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class TotalCountTrackerRepository : RepositoryBase<TotalCountTracker>
    {
        protected override string TableName => "TotalCountTrackerTbl";
        protected override string KeyColumn => "TotalCounterTrackerID";

        protected override string CoreColumns =>
            "TotalCounterTrackerID, CountDate, TotalCount, Comments";

        /// <summary>
        /// Gets the latest total count tracker record by CountDate.
        /// </summary>
        public TotalCountTracker GetLatest()
        {
            string sql = $@"
                SELECT TOP 1 {CoreColumns}
                FROM {TableName}
                ORDER BY CountDate DESC";

            using (var rdr = ExecReader(sql))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<TotalCountTracker>(rdr);
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a new total count tracker record.
        /// </summary>
        public int Add(int totalCount, string comments = null)
        {
            string sql = $@"
                INSERT INTO {TableName}
                (
                    CountDate,
                    TotalCount,
                    Comments
                )
                VALUES
                (
                    @CountDate,
                    @TotalCount,
                    @Comments
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@CountDate",
                    DataDbType = DbType.DateTime,
                    DataValue = DateTime.UtcNow
                },
                new DBParameter
                {
                    ParamName = "@TotalCount",
                    DataDbType = DbType.Int32,
                    DataValue = totalCount
                },
                new DBParameter
                {
                    ParamName = "@Comments",
                    DataDbType = DbType.String,
                    DataValue = comments ?? string.Empty
                }
            };

            return ExecuteScalar<int>(sql, parameters);
        }
    }
}