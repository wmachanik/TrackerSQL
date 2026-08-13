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
            if (contactId <= 0)
                return false;

            const string updateSql = @"
                UPDATE ContactsItemsPredictedTbl
                SET LastCupCount = @LastCupCount
                WHERE ContactID = @ContactID";

            var updateParameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@LastCupCount", DataValue = lastCupCount, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            if (ExecNonQuery(updateSql, updateParameters) > 0)
                return true;

            // New / never-predicted contacts have no row yet — create one with sensible defaults.
            DateTime nextCoffee = TimeZoneUtils.Now().Date.AddDays(20);
            const string insertSql = @"
                INSERT INTO ContactsItemsPredictedTbl
                    (ContactID, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst,
                     DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount)
                VALUES
                    (@ContactID, @LastCupCount, @NextCoffeeBy, @NextCleanOn, @NextFilterEst, @NextDescaleEst, @NextServiceEst,
                     @DailyConsumption, @FilterAveCount, @DescaleAveCount, @ServiceAveCount, @CleanAveCount)";

            var insertParameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@LastCupCount", DataValue = lastCupCount, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@NextCoffeeBy", DataValue = nextCoffee, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextCleanOn", DataValue = nextCoffee.AddDays(20), DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextFilterEst", DataValue = nextCoffee.AddDays(30), DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextDescaleEst", DataValue = nextCoffee.AddDays(30), DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextServiceEst", DataValue = nextCoffee.AddYears(1), DataDbType = DbType.Date },
                new DBParameter { ParamName = "@DailyConsumption", DataValue = SystemConstants.BusinessConstants.TypicalAverageConsumption, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@FilterAveCount", DataValue = 300.0, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@DescaleAveCount", DataValue = 500.0, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@ServiceAveCount", DataValue = 10000.0, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@CleanAveCount", DataValue = 200.0, DataDbType = DbType.Double }
            };

            return ExecNonQuery(insertSql, insertParameters) > 0;
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