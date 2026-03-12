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

        public override List<ItemPrepType> GetAll(string SortBy)
        {
            var list = new List<ItemPrepType>();
            string sql = $"SELECT ItemPrepID, ItemPrepType AS ItemPrepTypeName, IdentifyingChar FROM {TableName}";
            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                sql += " ORDER BY " + SortBy;
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
