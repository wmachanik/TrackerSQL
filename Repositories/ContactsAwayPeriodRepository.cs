using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// Repository for ContactsAwayPeriodTbl.
    ///
    /// Standard CRUD operations are inherited from RepositoryBase<ContactsAwayPeriod>:
    /// GetById, GetKeyColsById, GetAll, Insert, Update and Delete.
    ///
    /// Custom summary/list queries remain here because they join ContactsTbl and AwayReasonTbl.
    /// </summary>
    public class ContactsAwayPeriodRepository : RepositoryBase<ContactsAwayPeriod>
    {
        protected override string TableName => "ContactsAwayPeriodTbl";
        protected override string KeyColumn => "AwayPeriodID";

        /// <summary>
        /// Gets away periods with contact name and reason description for list view.
        /// </summary>
        public RepositoryListResult<ContactsAwaySummary> GetAwaySummaries(string sortBy = null)
        {
            return GetAwaySummaries(sortBy, null);
        }

        public RepositoryListResult<ContactsAwaySummary> GetAwaySummaries(string sortBy, string whereFilter)
        {
            string sql = GetAwaySummaryBaseSql();
            if (!string.IsNullOrWhiteSpace(whereFilter))
            {
                sql += " WHERE " + whereFilter;
            }

            sql += " ORDER BY " + MapSortColumnOrDefault(sortBy);
            return ExecuteAwaySummaryReader(sql, null, "ContactsAwayPeriodRepository.GetAwaySummaries");
        }

        /// <summary>
        /// Gets currently away contacts where today falls between start and end date.
        /// </summary>
        public List<ContactsAwaySummary> GetCurrentlyAway(string sortBy = "ContactName")
        {
            string sql = GetAwaySummaryBaseSql() + @"
                WHERE a.AwayStartDate <= @Today
                  AND a.AwayEndDate >= @Today
                ORDER BY " + MapSortColumnOrDefault(sortBy);

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@Today",
                    DataValue = TimeZoneUtils.Now().Date,
                    DataDbType = DbType.Date
                }
            };

            var result = ExecuteAwaySummaryReader(sql, parameters, "ContactsAwayPeriodRepository.GetCurrentlyAway");
            return result.Success ? result.Items : new List<ContactsAwaySummary>();
        }

        private static string GetAwaySummaryBaseSql()
        {
            return @"
                SELECT
                    a.AwayPeriodID,
                    a.ContactID,
                    c.CompanyName,
                    a.AwayStartDate,
                    a.AwayEndDate,
                    a.ReasonID,
                    r.ReasonDesc
                FROM ContactsAwayPeriodTbl a
                INNER JOIN ContactsTbl c ON a.ContactID = c.ContactID
                LEFT OUTER JOIN AwayReasonTbl r ON a.ReasonID = r.AwayReasonID";
        }

        private RepositoryListResult<ContactsAwaySummary> ExecuteAwaySummaryReader(
            string sql,
            List<DBParameter> parameters,
            string methodName)
        {
            var list = new List<ContactsAwaySummary>();

            try
            {
                using (var rdr = ExecReader(sql, parameters))
                {
                    while (rdr != null && rdr.Read())
                    {
                        list.Add(DbMapper.Map<ContactsAwaySummary>(rdr));
                    }
                }

                return RepositoryListResult<ContactsAwaySummary>.Ok(list);
            }
            catch (Exception ex)
            {
                return RepositoryListResult<ContactsAwaySummary>.Fail("ContactsAwayPeriodRepository." + methodName, ex);
            }
        }

        private static string MapSortColumnOrDefault(string sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return "a.AwayStartDate DESC";
            }

            string trimmed = sortBy.Trim();
            string direction = string.Empty;

            if (trimmed.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase))
            {
                direction = " DESC";
                trimmed = trimmed.Substring(0, trimmed.Length - 5).Trim();
            }
            else if (trimmed.EndsWith(" ASC", StringComparison.OrdinalIgnoreCase))
            {
                direction = " ASC";
                trimmed = trimmed.Substring(0, trimmed.Length - 4).Trim();
            }

            if (trimmed.Equals("CompanyName", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("ContactName", StringComparison.OrdinalIgnoreCase))
            {
                return "c.CompanyName" + direction;
            }

            if (trimmed.Equals("AwayStartDate", StringComparison.OrdinalIgnoreCase))
            {
                return "a.AwayStartDate" + direction;
            }

            if (trimmed.Equals("AwayEndDate", StringComparison.OrdinalIgnoreCase))
            {
                return "a.AwayEndDate" + direction;
            }

            if (trimmed.Equals("ReasonDesc", StringComparison.OrdinalIgnoreCase))
            {
                return "r.ReasonDesc" + direction;
            }

            if (trimmed.Equals("ReasonID", StringComparison.OrdinalIgnoreCase))
            {
                return "a.ReasonID" + direction;
            }

            if (trimmed.Equals("ContactID", StringComparison.OrdinalIgnoreCase))
            {
                return "a.ContactID" + direction;
            }

            if (trimmed.Equals("AwayPeriodID", StringComparison.OrdinalIgnoreCase))
            {
                return "a.AwayPeriodID" + direction;
            }

            return "a.AwayStartDate DESC";
        }

        public bool IsContactAwayOnDate(long contactId, DateTime date)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM ContactsAwayPeriodTbl
                WHERE ContactID = @ContactID
                  AND AwayStartDate <= @CheckDate
                  AND AwayEndDate >= @CheckDate";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 },
                new DBParameter { ParamName = "@CheckDate", DataValue = date.Date, DataDbType = DbType.Date }
            };

            return ExecuteScalar<int>(sql, parameters) > 0;
        }

        public HashSet<long> GetAwayContactIds(DateTime windowStart, DateTime windowEnd)
        {
            var result = new HashSet<long>();
            const string sql = @"
                SELECT DISTINCT ContactID
                FROM ContactsAwayPeriodTbl
                WHERE AwayStartDate <= @WindowEnd AND AwayEndDate >= @WindowStart";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@WindowEnd", DataValue = windowEnd, DataDbType = DbType.DateTime },
                new DBParameter { ParamName = "@WindowStart", DataValue = windowStart, DataDbType = DbType.DateTime }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    if (rdr["ContactID"] != DBNull.Value)
                    {
                        result.Add(Convert.ToInt64(rdr["ContactID"]));
                    }
                }
            }

            return result;
        }
    }
}
