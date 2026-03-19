using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class AreasRepository : RepositoryBase<Area>
    {
        protected override string TableName => "AreasTbl";
        protected override string KeyColumn => "AreaID";

        protected override string CoreColumns => "AreaID, Area AS AreaName, PrepDayOfWeekID, DeliveryDelay";
        protected override string LookupColumns => "AreaID, Area AS AreaName";

        public override Area GetById(int id)
        {
            string sql = "SELECT AreaID, Area AS AreaName, PrepDayOfWeekID, DeliveryDelay FROM AreasTbl WHERE AreaID = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<Area>(rdr);
                }
            }
            return null;
        }

        public override List<Area> GetAll(string SortBy)
        {
            var list = new List<Area>();
            string sql = "SELECT AreaID, Area AS AreaName, PrepDayOfWeekID, DeliveryDelay FROM AreasTbl";
            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                string orderBy = SortBy.Equals("AreaName", System.StringComparison.OrdinalIgnoreCase) ? "Area" : SortBy;
                sql += " ORDER BY " + orderBy;
            }
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<Area>(rdr));
            }
            return list;
        }
    }
}
