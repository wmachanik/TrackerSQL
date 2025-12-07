using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;
using TrackerSQL.Classes;

namespace TrackerDotNet.Classes.Sql
{
    public class ContactsUsage
    {
        public int ContactID { get; set; }
        public int LastCupCount { get; set; }
        public DateTime? NextCoffeeBy { get; set; }
        public DateTime? NextCleanOn { get; set; }
        public DateTime? NextFilterEst { get; set; }
        public DateTime? NextDescaleEst { get; set; }
        public DateTime? NextServiceEst { get; set; }
        public double? DailyConsumption { get; set; }
        public double? FilterAveCount { get; set; }
        public double? DescaleAveCount { get; set; }
        public double? ServiceAveCount { get; set; }
        public double? CleanAveCount { get; set; }
    }

    public class ContactsUsageRepository : RepositoryBase<ContactsUsage>
    {
        protected override string TableName => "ContactsUsageTbl";
        protected override string KeyColumn => "ContactID";
        protected override string CoreColumns => "ContactID, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount";

        public ContactsUsage GetByContactId(int id)
        {
            string sql = $"SELECT {CoreColumns} FROM {TableName} WHERE ContactID = @Id";
            var p = new List<DBParameter> { new DBParameter { ParamName = "@Id", DataDbType = DbType.Int32, DataValue = id } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read()) return DbMapper.Map<ContactsUsage>(rdr);
            }
            return null;
        }
    }
}
