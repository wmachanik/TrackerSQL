using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ContactsItemUsage
    {
        public int ContactItemUsageLineNo { get; set; }
        public int ContactID { get; set; }
        public System.DateTime? DeliveryDate { get; set; }
        public int? ItemProvidedID { get; set; }
        public double? QtyProvided { get; set; }
        public int? ItemPrepTypeID { get; set; }
        public int? ItemPackagingID { get; set; }
        public string Notes { get; set; }
    }

    public class ContactsItemUsageRepository : RepositoryBase<ContactsItemUsage>
    {
        protected override string TableName => "ContactsItemUsageTbl";
        protected override string KeyColumn => "ContactItemUsageLineNo";
        protected override string CoreColumns => "ContactItemUsageLineNo, ContactID, DeliveryDate, ItemProvidedID, QtyProvided, ItemPrepTypeID, ItemPackagingID, Notes";
        protected override string LookupColumns => null;

        public List<ContactsItemUsage> GetByContactId(int contactId, string sortBy = "DeliveryDate DESC")
        {
            var list = new List<ContactsItemUsage>();
            string sql = $"SELECT {CoreColumns} FROM {TableName} WHERE ContactID = @ContactID";
            if (!string.IsNullOrWhiteSpace(sortBy)) sql += " ORDER BY " + sortBy;
            var p = new List<DBParameter> { new DBParameter { ParamName = "@ContactID", DataDbType = DbType.Int32, DataValue = contactId } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                while (rdr.Read()) list.Add(DbMapper.Map<ContactsItemUsage>(rdr));
            }
            return list;
        }
    }
}
