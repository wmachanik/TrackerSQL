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

        /// <summary>
        /// Writes recalculated next-service dates and averages after Order Done.
        /// Only known prediction columns are updated (whitelist).
        /// </summary>
        public bool UpdatePredictedServiceFields(int contactId, ContactsUsage predicted)
        {
            if (contactId <= 0 || predicted == null)
                return false;

            const string sql = @"
                UPDATE ContactsItemsPredictedTbl
                SET NextCoffeeBy = @NextCoffeeBy,
                    NextCleanOn = @NextCleanOn,
                    NextFilterEst = @NextFilterEst,
                    NextDescaleEst = @NextDescaleEst,
                    NextServiceEst = @NextServiceEst,
                    DailyConsumption = @DailyConsumption,
                    CleanAveCount = @CleanAveCount,
                    FilterAveCount = @FilterAveCount,
                    DescaleAveCount = @DescaleAveCount,
                    ServiceAveCount = @ServiceAveCount
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@NextCoffeeBy", DataValue = (object)predicted.NextCoffeeBy ?? DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextCleanOn", DataValue = (object)predicted.NextCleanOn ?? DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextFilterEst", DataValue = (object)predicted.NextFilterEst ?? DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextDescaleEst", DataValue = (object)predicted.NextDescaleEst ?? DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextServiceEst", DataValue = (object)predicted.NextServiceEst ?? DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@DailyConsumption", DataValue = (object)predicted.DailyConsumption ?? DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@CleanAveCount", DataValue = (object)predicted.CleanAveCount ?? DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@FilterAveCount", DataValue = (object)predicted.FilterAveCount ?? DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@DescaleAveCount", DataValue = (object)predicted.DescaleAveCount ?? DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@ServiceAveCount", DataValue = (object)predicted.ServiceAveCount ?? DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        /// <summary>
        /// Prediction rows for bulk average / NextCoffeeBy recalculation (enabled contacts only).
        /// </summary>
        public List<PredictionBulkRecalcCandidate> GetBulkRecalcCandidates(bool staleOnly)
        {
            int coffee = SystemConstants.ServiceTypeConstants.Coffee;
            string sql = @"
                SELECT
                    p.ContactID,
                    c.CompanyName,
                    p.NextCoffeeBy AS PreviousNextCoffeeBy,
                    p.DailyConsumption AS PreviousDailyConsumption,
                    p.LastCupCount,
                    coffee.LastCoffeeDate
                FROM ContactsItemsPredictedTbl p
                INNER JOIN ContactsTbl c ON c.ContactID = p.ContactID
                OUTER APPLY (
                    SELECT MAX(s.UsageDate) AS LastCoffeeDate
                    FROM ContactsItemSvcSummaryTbl s
                    WHERE s.ContactID = p.ContactID
                      AND s.ItemServiceTypeID = @CoffeeServiceType
                ) coffee
                WHERE ISNULL(c.Enabled, 1) = 1
                  AND ISNULL(c.PredictionDisabled, 0) = 0";

            if (staleOnly)
            {
                sql += @"
                  AND coffee.LastCoffeeDate IS NOT NULL
                  AND (p.NextCoffeeBy IS NULL OR p.NextCoffeeBy < coffee.LastCoffeeDate)";
            }

            sql += " ORDER BY c.CompanyName, p.ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@CoffeeServiceType", DataValue = coffee, DataDbType = DbType.Int32 }
            };

            var list = new List<PredictionBulkRecalcCandidate>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new PredictionBulkRecalcCandidate
                    {
                        ContactID = Convert.ToInt32(rdr["ContactID"]),
                        CompanyName = rdr["CompanyName"] == DBNull.Value ? string.Empty : Convert.ToString(rdr["CompanyName"]),
                        PreviousNextCoffeeBy = rdr["PreviousNextCoffeeBy"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(rdr["PreviousNextCoffeeBy"]).Date,
                        PreviousDailyConsumption = rdr["PreviousDailyConsumption"] == DBNull.Value
                            ? (double?)null
                            : Convert.ToDouble(rdr["PreviousDailyConsumption"]),
                        LastCupCount = rdr["LastCupCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["LastCupCount"]),
                        LastCoffeeDate = rdr["LastCoffeeDate"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(rdr["LastCoffeeDate"]).Date
                    });
                }
            }

            return list;
        }
    }

    public class PredictionBulkRecalcCandidate
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public DateTime? PreviousNextCoffeeBy { get; set; }
        public double? PreviousDailyConsumption { get; set; }
        public int LastCupCount { get; set; }
        public DateTime? LastCoffeeDate { get; set; }
    }
}