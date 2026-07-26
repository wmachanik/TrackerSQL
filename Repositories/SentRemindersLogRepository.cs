using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class SentRemindersLogRepository : RepositoryBase<SentRemindersLog>
    {
        // Canonical DB column: HadRecurringItems (never HadReoccurItems / HadRecurrItems / Reoccur*).
        private const string SelectColumns =
            "ReminderID, ContactID, DateSentReminder, NextPreparationDate, ReminderSent, HadAutoFulfilItem, HadRecurringItems";

        protected override string TableName => "SentRemindersLogTbl";
        protected override string KeyColumn => "ReminderID";

        protected override string CoreColumns => SelectColumns;

        public List<SentRemindersLog> GetAllByDate(DateTime dateSent, string sortBy = null)
        {
            string sql = $"SELECT {SelectColumns} FROM SentRemindersLogTbl WHERE DateSentReminder = @DateSentReminder";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@DateSentReminder", DataValue = dateSent.Date, DataDbType = DbType.Date }
            };

            if (!string.IsNullOrEmpty(sortBy))
            {
                sql += " ORDER BY " + sortBy;
            }

            return QueryList(sql, parameters);
        }

        public List<DateTime> GetLast20DatesReminderSent()
        {
            var list = new List<DateTime>();
            const string sql = "SELECT DISTINCT TOP 20 DateSentReminder FROM SentRemindersLogTbl ORDER BY DateSentReminder DESC";

            using (var rdr = ExecReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    if (rdr["DateSentReminder"] != DBNull.Value)
                    {
                        list.Add(Convert.ToDateTime(rdr["DateSentReminder"]).Date);
                    }
                }
            }

            return list;
        }

        public bool InsertLogItem(SentRemindersLog entry)
        {
            return Insert(entry) > 0;
        }

        public bool UpdateLogItem(SentRemindersLog entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            const string sql = @"
                UPDATE SentRemindersLogTbl SET
                    ContactID = @ContactID, DateSentReminder = @DateSentReminder,
                    NextPreparationDate = @NextPreparationDate, ReminderSent = @ReminderSent,
                    HadAutoFulfilItem = @HadAutoFulfilItem, HadRecurringItems = @HadRecurringItems
                WHERE ReminderID = @ReminderID";

            return ExecNonQuery(sql, BuildParameters(entry, includeId: true)) > 0;
        }

        public int DeleteEntriesForDate(DateTime targetDate)
        {
            int deletedCount = GetEntriesCountForDate(targetDate);

            const string sql = "DELETE FROM SentRemindersLogTbl WHERE DateSentReminder = @DateSentReminder";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@DateSentReminder", DataValue = targetDate.Date, DataDbType = DbType.Date }
            };

            int result = ExecNonQuery(sql, parameters);
            if (result < 0)
            {
                throw new Exception($"Failed to delete reminder entries for {targetDate:yyyy-MM-dd}");
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                $"SentRemindersLogRepository: Deleted {deletedCount} entries for {targetDate:yyyy-MM-dd}");

            return deletedCount;
        }

        public int DeleteTodaysEntries()
        {
            return DeleteEntriesForDate(TimeZoneUtils.Now().Date);
        }

        public DateTime GetLastSuccessfulCheckupDate()
        {
            DateTime lastCheckupDate = DateTime.MinValue;

            try
            {
                const string sql = "SELECT MAX(DateSentReminder) AS LastCheckupDate FROM SentRemindersLogTbl WHERE ReminderSent = 1";

                using (var rdr = ExecReader(sql))
                {
                    if (rdr != null && rdr.Read() && rdr["LastCheckupDate"] != DBNull.Value)
                    {
                        lastCheckupDate = Convert.ToDateTime(rdr["LastCheckupDate"]).Date;
                    }
                }

                if (lastCheckupDate == DateTime.MinValue)
                {
                    lastCheckupDate = new SysDataRepository().GetMinReminderDate();
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        $"SentRemindersLogRepository: No previous checkup found, using MinReminderDate: {lastCheckupDate:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"SentRemindersLogRepository: Error getting last checkup date: {ex.Message}. Using MinReminderDate.");
                lastCheckupDate = new SysDataRepository().GetMinReminderDate();
            }

            return lastCheckupDate;
        }

        public int GetEntriesCountForDate(DateTime targetDate)
        {
            const string sql = "SELECT COUNT(*) FROM SentRemindersLogTbl WHERE DateSentReminder = @DateSentReminder";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@DateSentReminder", DataValue = targetDate.Date, DataDbType = DbType.Date }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public override int Insert(SentRemindersLog entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO SentRemindersLogTbl
                (ContactID, DateSentReminder, NextPreparationDate, ReminderSent, HadAutoFulfilItem, HadRecurringItems)
                VALUES
                (@ContactID, @DateSentReminder, @NextPreparationDate, @ReminderSent, @HadAutoFulfilItem, @HadRecurringItems);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return ExecuteScalar<int>(sql, BuildParameters(entity, includeId: false));
        }

        private List<SentRemindersLog> QueryList(string sql, List<DBParameter> parameters)
        {
            var list = new List<SentRemindersLog>();
            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(DbMapper.Map<SentRemindersLog>(rdr));
                }
            }

            return list;
        }

        private static List<DBParameter> BuildParameters(SentRemindersLog entity, bool includeId)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = entity.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@DateSentReminder", DataValue = entity.DateSentReminder ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@NextPreparationDate", DataValue = entity.NextPreparationDate ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@ReminderSent", DataValue = entity.ReminderSent ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@HadAutoFulfilItem", DataValue = entity.HadAutoFulfilItem ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@HadRecurringItems", DataValue = entity.HadRecurringItems ?? (object)DBNull.Value, DataDbType = DbType.Boolean }
            };

            if (includeId)
            {
                parameters.Add(new DBParameter { ParamName = "@ReminderID", DataValue = entity.ReminderID, DataDbType = DbType.Int32 });
            }

            return parameters;
        }
    }
}
