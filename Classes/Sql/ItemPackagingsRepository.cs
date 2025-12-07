using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ItemPackagingsRepository : RepositoryBase<ItemPackaging>
    {
        protected override string TableName => "PackagingTbl"; // legacy actual table
        protected override string KeyColumn => "PackagingID";
        protected override string CoreColumns => "PackagingID AS ItemPackagingID, Description AS ItemPackagingDesc, AdditionalNotes, Symbol, Colour, BGColour";
        protected override string LookupColumns => "PackagingID AS ItemPackagingID, Description AS ItemPackagingDesc";

        public override List<ItemPackaging> GetAll(string SortBy)
        {
            var list = new List<ItemPackaging>();
            string sql = $"SELECT {CoreColumns} FROM {TableName}";
            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                if (SortBy == "ItemPackagingDesc") SortBy = "Description";
                if (SortBy == "ItemPackagingID") SortBy = "PackagingID";
                sql += " ORDER BY " + SortBy;
            }
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<ItemPackaging>(rdr));
            }
            return list;
        }

        public override ItemPackaging GetById(int id)
        {
            string sql = $"SELECT {CoreColumns} FROM {TableName} WHERE {KeyColumn} = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read()) return DbMapper.Map<ItemPackaging>(rdr);
            }
            return null;
        }
    }
}
