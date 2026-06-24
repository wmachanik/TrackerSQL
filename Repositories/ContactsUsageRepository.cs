using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactsUsageRepository : RepositoryBase<ContactsUsage>
    {
        protected override string TableName => "ContactsItemsPredictedTbl";
        protected override string KeyColumn => "ContactsItemsPredictedId";

        protected override string CoreColumns =>
            "ContactsItemsPredictedId, ContactID, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount";

        /// <summary>
        /// Gets the predicted usage record for a contact.
        /// </summary>
        public ContactsUsage GetByContactId(int contactId)
        {
            string sql = $@"
                SELECT {CoreColumns}
                FROM {TableName}
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@ContactID",
                    DataValue = contactId,
                    DataDbType = DbType.Int32
                }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<ContactsUsage>(rdr);
                }
            }

            return null;
        }

        public bool UpdateLastCupCount(int contactId, int lastCupCount)
        {
            const string sql = @"
                UPDATE ContactsItemsPredictedTbl
                SET LastCupCount = @LastCupCount
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@LastCupCount", DataValue = lastCupCount, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool ForceNextCoffeeDate(int contactId, DateTime nextDate)
        {
            const string sql = @"
                UPDATE ContactsItemsPredictedTbl
                SET NextCoffeeBy = @NextCoffeeBy
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@NextCoffeeBy", DataValue = nextDate.Date, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }
    }
}