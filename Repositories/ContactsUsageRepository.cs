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

        /// <summary>
        /// Home-page total: sum of each contact's last cup reading (legacy ClientUsageTbl SUM).
        /// </summary>
        public long GetSumOfLastCupCounts()
        {
            const string sql = @"
SELECT ISNULL(SUM(CAST(ISNULL(LastCupCount, 0) AS BIGINT)), 0)
FROM ContactsItemsPredictedTbl";
            return ExecuteScalar<long>(sql);
        }

        /// <summary>
        /// Live cup-count breakdown for home page / System Tools recalc.
        /// </summary>
        public CupCountTotalSummary GetCupCountSummary()
        {
            const string sql = @"
SELECT
    ISNULL(SUM(CAST(ISNULL(p.LastCupCount, 0) AS BIGINT)), 0) AS TotalCups,
    COUNT(*) AS PredictedRows,
    SUM(CASE WHEN ISNULL(p.LastCupCount, 0) > 0 THEN 1 ELSE 0 END) AS ContactsWithCount,
    ISNULL(SUM(CASE WHEN ISNULL(c.Enabled, 1) = 1 THEN CAST(ISNULL(p.LastCupCount, 0) AS BIGINT) ELSE 0 END), 0) AS EnabledTotalCups,
    SUM(CASE WHEN ISNULL(c.Enabled, 1) = 1 AND ISNULL(p.LastCupCount, 0) > 0 THEN 1 ELSE 0 END) AS EnabledContactsWithCount
FROM ContactsItemsPredictedTbl p
LEFT JOIN ContactsTbl c ON c.ContactID = p.ContactID";

            var summary = new CupCountTotalSummary();
            using (var rdr = ExecReader(sql))
            {
                if (rdr != null && rdr.Read())
                {
                    summary.TotalCups = rdr["TotalCups"] == DBNull.Value ? 0 : Convert.ToInt64(rdr["TotalCups"]);
                    summary.PredictedRows = rdr["PredictedRows"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PredictedRows"]);
                    summary.ContactsWithCount = rdr["ContactsWithCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ContactsWithCount"]);
                    summary.EnabledTotalCups = rdr["EnabledTotalCups"] == DBNull.Value ? 0 : Convert.ToInt64(rdr["EnabledTotalCups"]);
                    summary.EnabledContactsWithCount = rdr["EnabledContactsWithCount"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(rdr["EnabledContactsWithCount"]);
                }
            }
            return summary;
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
        /// Prediction rows for bulk Next Coffee / filter / clean / descale / service recalculation (enabled contacts only).
        /// </summary>
        public List<PredictionBulkRecalcCandidate> GetBulkRecalcCandidates(bool staleOnly)
        {
            int coffee = SystemConstants.ServiceTypeConstants.Coffee;
            int filter = SystemConstants.ServiceTypeConstants.Filter;
            int clean = SystemConstants.ServiceTypeConstants.Clean;
            int descale = SystemConstants.ServiceTypeConstants.Descale;
            int service = SystemConstants.ServiceTypeConstants.Service;
            string sql = @"
                SELECT
                    p.ContactID,
                    c.CompanyName,
                    p.NextCoffeeBy AS PreviousNextCoffeeBy,
                    p.NextFilterEst AS PreviousNextFilterEst,
                    p.NextCleanOn AS PreviousNextCleanOn,
                    p.NextDescaleEst AS PreviousNextDescaleEst,
                    p.NextServiceEst AS PreviousNextServiceEst,
                    p.DailyConsumption AS PreviousDailyConsumption,
                    p.LastCupCount,
                    coffee.LastCoffeeDate,
                    filt.LastFilterDate
                FROM ContactsItemsPredictedTbl p
                INNER JOIN ContactsTbl c ON c.ContactID = p.ContactID
                OUTER APPLY (
                    SELECT MAX(s.UsageDate) AS LastCoffeeDate
                    FROM ContactsItemSvcSummaryTbl s
                    WHERE s.ContactID = p.ContactID
                      AND s.ItemServiceTypeID = @CoffeeServiceType
                ) coffee
                OUTER APPLY (
                    SELECT MAX(s.UsageDate) AS LastFilterDate
                    FROM ContactsItemSvcSummaryTbl s
                    WHERE s.ContactID = p.ContactID
                      AND s.ItemServiceTypeID = @FilterServiceType
                ) filt
                OUTER APPLY (
                    SELECT MAX(s.UsageDate) AS LastCleanDate
                    FROM ContactsItemSvcSummaryTbl s
                    WHERE s.ContactID = p.ContactID
                      AND s.ItemServiceTypeID = @CleanServiceType
                ) clean
                OUTER APPLY (
                    SELECT MAX(s.UsageDate) AS LastDescaleDate
                    FROM ContactsItemSvcSummaryTbl s
                    WHERE s.ContactID = p.ContactID
                      AND s.ItemServiceTypeID = @DescaleServiceType
                ) dsc
                OUTER APPLY (
                    SELECT MAX(s.UsageDate) AS LastServiceDate
                    FROM ContactsItemSvcSummaryTbl s
                    WHERE s.ContactID = p.ContactID
                      AND s.ItemServiceTypeID = @ServiceServiceType
                ) svc
                WHERE ISNULL(c.Enabled, 1) = 1
                  AND ISNULL(c.PredictionDisabled, 0) = 0";

            if (staleOnly)
            {
                sql += @"
                  AND (
                        (coffee.LastCoffeeDate IS NOT NULL
                         AND (p.NextCoffeeBy IS NULL OR p.NextCoffeeBy < coffee.LastCoffeeDate))
                     OR (filt.LastFilterDate IS NOT NULL
                         AND (p.NextFilterEst IS NULL OR p.NextFilterEst < filt.LastFilterDate
                              OR p.NextFilterEst > DATEADD(month, 3, filt.LastFilterDate)))
                     OR (clean.LastCleanDate IS NOT NULL
                         AND (p.NextCleanOn IS NULL OR p.NextCleanOn < clean.LastCleanDate))
                     OR (dsc.LastDescaleDate IS NOT NULL
                         AND (p.NextDescaleEst IS NULL OR p.NextDescaleEst < dsc.LastDescaleDate))
                     OR (svc.LastServiceDate IS NOT NULL
                         AND (p.NextServiceEst IS NULL OR p.NextServiceEst < svc.LastServiceDate))
                  )";
            }

            sql += " ORDER BY c.CompanyName, p.ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@CoffeeServiceType", DataValue = coffee, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@FilterServiceType", DataValue = filter, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@CleanServiceType", DataValue = clean, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@DescaleServiceType", DataValue = descale, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ServiceServiceType", DataValue = service, DataDbType = DbType.Int32 }
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
                        PreviousNextCoffeeBy = ReadDate(rdr, "PreviousNextCoffeeBy"),
                        PreviousNextFilterEst = ReadDate(rdr, "PreviousNextFilterEst"),
                        PreviousNextCleanOn = ReadDate(rdr, "PreviousNextCleanOn"),
                        PreviousNextDescaleEst = ReadDate(rdr, "PreviousNextDescaleEst"),
                        PreviousNextServiceEst = ReadDate(rdr, "PreviousNextServiceEst"),
                        PreviousDailyConsumption = rdr["PreviousDailyConsumption"] == DBNull.Value
                            ? (double?)null
                            : Convert.ToDouble(rdr["PreviousDailyConsumption"]),
                        LastCupCount = rdr["LastCupCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["LastCupCount"]),
                        LastCoffeeDate = ReadDate(rdr, "LastCoffeeDate"),
                        LastFilterDate = ReadDate(rdr, "LastFilterDate")
                    });
                }
            }

            return list;
        }

        private static DateTime? ReadDate(IDataRecord rdr, string name)
        {
            return rdr[name] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr[name]).Date;
        }
    }

    public class PredictionBulkRecalcCandidate
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public DateTime? PreviousNextCoffeeBy { get; set; }
        public DateTime? PreviousNextFilterEst { get; set; }
        public DateTime? PreviousNextCleanOn { get; set; }
        public DateTime? PreviousNextDescaleEst { get; set; }
        public DateTime? PreviousNextServiceEst { get; set; }
        public double? PreviousDailyConsumption { get; set; }
        public int LastCupCount { get; set; }
        public DateTime? LastCoffeeDate { get; set; }
        public DateTime? LastFilterDate { get; set; }
    }

    public class CupCountTotalSummary
    {
        public long TotalCups { get; set; }
        public int PredictedRows { get; set; }
        public int ContactsWithCount { get; set; }
        public long EnabledTotalCups { get; set; }
        public int EnabledContactsWithCount { get; set; }
    }
}