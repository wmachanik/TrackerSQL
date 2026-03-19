using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerDotNet.Classes;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ItemPrepTypesRepository : RepositoryBase<ItemPrepType>
    {
        protected override string TableName => "ItemPrepTypesTbl";
        protected override string KeyColumn => "ItemPrepID";

        protected override string CoreColumns => "ItemPrepID, ItemPrepType AS ItemPrepTypeName";
        protected override string LookupColumns => "ItemPrepID, ItemPrepType AS ItemPrepTypeName";

        public override ItemPrepType GetById(int id)
        {
            string sql = "SELECT ItemPrepID, ItemPrepType AS ItemPrepTypeName, IdentifyingChar FROM ItemPrepTypesTbl WHERE ItemPrepID = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<ItemPrepType>(rdr);
                }
            }
            return null;
        }

        public override List<ItemPrepType> GetAll(string SortBy)
        {
            var list = new List<ItemPrepType>();
            string sql = "SELECT ItemPrepID, ItemPrepType AS ItemPrepTypeName, IdentifyingChar FROM ItemPrepTypesTbl";
            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                string orderBy = SortBy.Equals("ItemPrepTypeName", System.StringComparison.OrdinalIgnoreCase) ? "ItemPrepType" : SortBy;
                sql += " ORDER BY " + orderBy;
            }
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<ItemPrepType>(rdr));
            }
            return list;
        }
    }
}
