using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ItemPackagingsRepository : RepositoryBase<ItemPackaging>
    {
        protected override string TableName => "ItemPackagingsTbl";
        protected override string KeyColumn => "ItemPackagingID";
        protected override string CoreColumns => "ItemPackagingID, ItemPrepDescription AS ItemPackagingDesc, AdditionalNotes, Symbol, Colour, BGColour";
        protected override string LookupColumns => "ItemPackagingID, ItemPrepDescription AS ItemPackagingDesc";

        public override ItemPackaging GetById(int id)
        {
            string sql = "SELECT ItemPackagingID, ItemPrepDescription AS ItemPackagingDesc, AdditionalNotes, Symbol, Colour, BGColour FROM ItemPackagingsTbl WHERE ItemPackagingID = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<ItemPackaging>(rdr);
                }
            }
            return null;
        }

        public override List<ItemPackaging> GetAll(string SortBy)
        {
            var list = new List<ItemPackaging>();
            string sql = "SELECT ItemPackagingID, ItemPrepDescription AS ItemPackagingDesc, AdditionalNotes, Symbol, Colour, BGColour FROM ItemPackagingsTbl";
            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                string orderBy = SortBy.Equals("ItemPackagingDesc", System.StringComparison.OrdinalIgnoreCase) ? "ItemPrepDescription" : SortBy;
                sql += " ORDER BY " + orderBy;
            }
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<ItemPackaging>(rdr));
            }
            return list;
        }
    }
}
