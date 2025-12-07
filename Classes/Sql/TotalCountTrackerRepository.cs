using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes; // for DBParameter and TrackerSQLDb

namespace TrackerDotNet.Classes.Sql
{
    public class TotalCountTrackerRepository : RepositoryBase<TotalCountTracker>
    {
        protected override string TableName => "TotalCountTrackerTbl";
        protected override string KeyColumn => "TotalCounterTrackerID";

        public TotalCountTracker GetLatest()
        {
            string sql = "SELECT TOP 1 * FROM " + TableName + " ORDER BY CountDate DESC";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<TotalCountTracker>(rdr);
                }
            }
            return null;
        }

        public void Add(int totalCount, string comments = null)
        {
            string sql = "INSERT INTO " + TableName + " (CountDate, TotalCount, Comments) VALUES (@CountDate, @TotalCount, @Comments)";
            var parameters = new List<DBParameter>
            {
                new DBParameter{ ParamName = "@CountDate", DataDbType = DbType.DateTime, DataValue = System.DateTime.UtcNow},
                new DBParameter{ ParamName = "@TotalCount", DataDbType = DbType.Int32, DataValue = totalCount},
                new DBParameter{ ParamName = "@Comments", DataDbType = DbType.String, DataValue = comments ?? string.Empty}
            };
            using (var db = new TrackerSQLDb())
            {
                db.ExecuteNonQuery(sql, parameters);
            }
        }
    }
}
