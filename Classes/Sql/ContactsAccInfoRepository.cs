using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes.Poco;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ContactsAccInfoRepository : RepositoryBase<ContactsAccInfo>
    {
        protected override string TableName => "ContactsAccInfoTbl";
        protected override string KeyColumn => "ContactsAccInfoID";

        public ContactsAccInfo GetByCustomerId(int customerId)
        {
            string sql = $"SELECT * FROM {TableName} WHERE ContactID = @Id";
            var p = new List<DBParameter> { new DBParameter { DataValue = customerId, DataDbType = DbType.Int32, ParamName = "@Id" } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<ContactsAccInfo>(rdr);
                }
            }
            return null;
        }
    }
}
