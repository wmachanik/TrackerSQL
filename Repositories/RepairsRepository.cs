using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class RepairsRepository : RepositoryBase<Repair>
    {
        public const int DoneStatusId = 7;

        private const string SelectColumns = @"
            RepairID, ContactID, ContactName, ContactEmail, JobCardNumber, DateLogged, LastStatusChange,
            EquipTypeID, EquipSerialNumber, SwopOutMachineID, EquipConditionID,
            TakenFrother, TakenBeanLid, TakenWaterLid, BrokenFrother, BrokenBeanLid, BrokenWaterLid,
            RepairFaultID, RepairFaultDesc, RepairStatusID, RelatedOrderLineID, Notes";

        private const string SelectColumnsAliased = @"
            r.RepairID, r.ContactID, r.ContactName, r.ContactEmail, r.JobCardNumber, r.DateLogged, r.LastStatusChange,
            r.EquipTypeID, r.EquipSerialNumber, r.SwopOutMachineID, r.EquipConditionID,
            r.TakenFrother, r.TakenBeanLid, r.TakenWaterLid, r.BrokenFrother, r.BrokenBeanLid, r.BrokenWaterLid,
            r.RepairFaultID, r.RepairFaultDesc, r.RepairStatusID, r.RelatedOrderLineID, r.Notes";

        protected override string TableName => "RepairsTbl";
        protected override string KeyColumn => "RepairID";

        protected override string CoreColumns => SelectColumns;

        public List<Repair> GetAllNotDone(string sortBy = null)
        {
            string sql = $"SELECT {SelectColumns} FROM RepairsTbl WHERE RepairStatusID <> {DoneStatusId}";
            return QueryRepairs(sql, null, sortBy);
        }

        public List<Repair> GetAllRepairsOfStatus(string repairStatus, string sortBy = null)
        {
            if (!int.TryParse(repairStatus, out int statusId) || repairStatus.Equals("OPEN", StringComparison.OrdinalIgnoreCase))
            {
                return GetAllNotDone(sortBy);
            }

            const string sql = "SELECT " + SelectColumns + " FROM RepairsTbl WHERE RepairStatusID = @RepairStatusID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RepairStatusID", DataValue = statusId, DataDbType = DbType.Int32 }
            };

            return QueryRepairs(sql, parameters, sortBy);
        }

        public List<Repair> GetRepairsByStatusAndDateRange(
            string sortBy, string repairStatus, object fromDate, object toDate, string filterBy, string filterText)
        {
            DateTime? fromDateNullable = TrackerTools.ConvertToNullableDateTime(fromDate);
            DateTime? toDateNullable = TrackerTools.ConvertToNullableDateTime(toDate);

            // Treat unset / sentinel dates as "no date filter"
            if (fromDateNullable.HasValue && fromDateNullable.Value <= SystemConstants.DatabaseConstants.SystemMinDate)
                fromDateNullable = null;
            if (toDateNullable.HasValue && toDateNullable.Value <= SystemConstants.DatabaseConstants.SystemMinDate)
                toDateNullable = null;

            bool hasTextFilter = !string.IsNullOrWhiteSpace(filterText)
                && !string.IsNullOrWhiteSpace(filterBy)
                && !string.Equals(filterBy, "DateLogged", StringComparison.OrdinalIgnoreCase);

            if (!fromDateNullable.HasValue && !toDateNullable.HasValue && !hasTextFilter)
            {
                return GetAllRepairsOfStatus(repairStatus, sortBy);
            }

            bool needsJoin = filterBy == "CompanyID" && !string.IsNullOrEmpty(filterText);
            string sql = needsJoin
                ? $"SELECT {SelectColumnsAliased} FROM RepairsTbl r INNER JOIN ContactsTbl c ON r.ContactID = c.ContactID"
                : $"SELECT {SelectColumns} FROM RepairsTbl";

            var parameters = new List<DBParameter>();
            var whereConditions = new List<string>();
            string prefix = needsJoin ? "r." : "RepairsTbl.";

            if (!string.IsNullOrEmpty(repairStatus) && int.TryParse(repairStatus, out int statusId) &&
                !repairStatus.Equals("OPEN", StringComparison.OrdinalIgnoreCase))
            {
                whereConditions.Add($"{prefix}RepairStatusID = @RepairStatusID");
                parameters.Add(new DBParameter { ParamName = "@RepairStatusID", DataValue = statusId, DataDbType = DbType.Int32 });
            }
            else if (repairStatus == "OPEN" || string.IsNullOrEmpty(repairStatus))
            {
                whereConditions.Add($"{prefix}RepairStatusID <> {DoneStatusId}");
            }

            if (fromDateNullable.HasValue)
            {
                whereConditions.Add($"{prefix}DateLogged >= @FromDate");
                parameters.Add(new DBParameter { ParamName = "@FromDate", DataValue = fromDateNullable.Value.Date, DataDbType = DbType.DateTime });
            }

            if (toDateNullable.HasValue)
            {
                whereConditions.Add($"{prefix}DateLogged < @ToDate");
                parameters.Add(new DBParameter { ParamName = "@ToDate", DataValue = toDateNullable.Value.Date.AddDays(1), DataDbType = DbType.DateTime });
            }

            if (!string.IsNullOrEmpty(filterText) && !string.IsNullOrEmpty(filterBy))
            {
                switch (filterBy)
                {
                    case "CompanyID":
                        string companySearchText = filterText.Contains("%") ? filterText : $"%{filterText}%";
                        whereConditions.Add("c.CompanyName LIKE @CompanyName");
                        parameters.Add(new DBParameter { ParamName = "@CompanyName", DataValue = companySearchText, DataDbType = DbType.String });
                        break;

                    case "MachineSerialNumber":
                        string serialSearchText = filterText.Contains("%") ? filterText : $"%{filterText}%";
                        whereConditions.Add($"{prefix}EquipSerialNumber LIKE @SerialNumber");
                        parameters.Add(new DBParameter { ParamName = "@SerialNumber", DataValue = serialSearchText, DataDbType = DbType.String });
                        break;
                }
            }

            if (whereConditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", whereConditions);
            }

            return QueryRepairs(sql, parameters, sortBy);
        }

        /// <summary>All repairs for a contact (any status), newest first.</summary>
        public List<Repair> GetByContactId(int contactId)
        {
            const string sql = "SELECT " + SelectColumns + @"
                FROM RepairsTbl
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            return QueryRepairs(sql, parameters, "DateLogged DESC");
        }

        /// <summary>All repairs for a contact logged on/after the given date, newest first.</summary>
        public List<Repair> GetByContactSince(int contactId, DateTime sinceDate)
        {
            const string sql = "SELECT " + SelectColumns + @"
                FROM RepairsTbl
                WHERE ContactID = @ContactID AND DateLogged >= @SinceDate";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SinceDate", DataValue = sinceDate.Date, DataDbType = DbType.DateTime }
            };

            return QueryRepairs(sql, parameters, "DateLogged DESC");
        }

        public Repair GetRepairById(int repairId)
        {
            if (repairId <= 0) return null;

            const string sql = "SELECT " + SelectColumns + " FROM RepairsTbl WHERE RepairID = @RepairID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RepairID", DataValue = repairId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<Repair>(rdr);
                }
            }

            return null;
        }

        public List<Repair> GetListOfRelatedTempOrders()
        {
            const string sql = "SELECT " + SelectColumns + " FROM RepairsTbl WHERE RelatedOrderLineID > 0";
            return QueryRepairs(sql, null, null);
        }

        public bool InsertRepair(Repair repair)
        {
            if (repair == null) throw new ArgumentNullException(nameof(repair));

            const string sql = @"
                INSERT INTO RepairsTbl
                (ContactID, ContactName, ContactEmail, JobCardNumber, DateLogged, LastStatusChange,
                 EquipTypeID, EquipSerialNumber, SwopOutMachineID, EquipConditionID,
                 TakenFrother, TakenBeanLid, TakenWaterLid, BrokenFrother, BrokenBeanLid, BrokenWaterLid,
                 RepairFaultID, RepairFaultDesc, RepairStatusID, RelatedOrderLineID, Notes)
                VALUES
                (@ContactID, @ContactName, @ContactEmail, @JobCardNumber, @DateLogged, @LastStatusChange,
                 @EquipTypeID, @EquipSerialNumber, @SwopOutMachineID, @EquipConditionID,
                 @TakenFrother, @TakenBeanLid, @TakenWaterLid, @BrokenFrother, @BrokenBeanLid, @BrokenWaterLid,
                 @RepairFaultID, @RepairFaultDesc, @RepairStatusID, @RelatedOrderLineID, @Notes)";

            return ExecNonQuery(sql, BuildParameters(repair, includeId: false)) > 0;
        }

        /// <summary>
        /// Inserts a repair and returns the new RepairID (0 on failure). Prefer this over
        /// InsertRepair + GetLastIdInserted: DateLogged is date-only, so two same-day repairs
        /// for one contact made GetLastIdInserted ambiguous (it returned the older repair).
        /// </summary>
        public int InsertRepairReturnId(Repair repair)
        {
            if (repair == null) throw new ArgumentNullException(nameof(repair));

            const string sql = @"
                INSERT INTO RepairsTbl
                (ContactID, ContactName, ContactEmail, JobCardNumber, DateLogged, LastStatusChange,
                 EquipTypeID, EquipSerialNumber, SwopOutMachineID, EquipConditionID,
                 TakenFrother, TakenBeanLid, TakenWaterLid, BrokenFrother, BrokenBeanLid, BrokenWaterLid,
                 RepairFaultID, RepairFaultDesc, RepairStatusID, RelatedOrderLineID, Notes)
                VALUES
                (@ContactID, @ContactName, @ContactEmail, @JobCardNumber, @DateLogged, @LastStatusChange,
                 @EquipTypeID, @EquipSerialNumber, @SwopOutMachineID, @EquipConditionID,
                 @TakenFrother, @TakenBeanLid, @TakenWaterLid, @BrokenFrother, @BrokenBeanLid, @BrokenWaterLid,
                 @RepairFaultID, @RepairFaultDesc, @RepairStatusID, @RelatedOrderLineID, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return ExecuteScalar<int>(sql, BuildParameters(repair, includeId: false));
        }

        public bool UpdateRepair(Repair repair, int originalRepairId = 0)
        {
            if (repair == null) throw new ArgumentNullException(nameof(repair));

            int repairId = originalRepairId > 0 ? originalRepairId : repair.RepairID;
            const string sql = @"
                UPDATE RepairsTbl SET
                    ContactID = @ContactID, ContactName = @ContactName, ContactEmail = @ContactEmail,
                    JobCardNumber = @JobCardNumber, DateLogged = @DateLogged, LastStatusChange = @LastStatusChange,
                    EquipTypeID = @EquipTypeID, EquipSerialNumber = @EquipSerialNumber,
                    SwopOutMachineID = @SwopOutMachineID, EquipConditionID = @EquipConditionID,
                    TakenFrother = @TakenFrother, TakenBeanLid = @TakenBeanLid, TakenWaterLid = @TakenWaterLid,
                    BrokenFrother = @BrokenFrother, BrokenBeanLid = @BrokenBeanLid, BrokenWaterLid = @BrokenWaterLid,
                    RepairFaultID = @RepairFaultID, RepairFaultDesc = @RepairFaultDesc,
                    RepairStatusID = @RepairStatusID, RelatedOrderLineID = @RelatedOrderLineID, Notes = @Notes
                WHERE RepairID = @RepairID";

            var parameters = BuildParameters(repair, includeId: true);
            parameters.Add(new DBParameter { ParamName = "@RepairID", DataValue = repairId, DataDbType = DbType.Int32 });

            return ExecNonQuery(sql, parameters) > 0;
        }

        public bool DeleteRepair(int repairId)
        {
            return Delete(repairId);
        }

        public int GetLastIdInserted(long contactId)
        {
            // DateLogged is date-only — RepairID breaks same-day ties (newest wins).
            const string sql = "SELECT TOP 1 RepairID FROM RepairsTbl WHERE ContactID = @ContactID ORDER BY DateLogged DESC, RepairID DESC";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        private List<Repair> QueryRepairs(string sql, List<DBParameter> parameters, string sortBy)
        {
            var list = new List<Repair>();
            sql += " ORDER BY " + (string.IsNullOrEmpty(sortBy) ? "DateLogged DESC" : sortBy);

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(DbMapper.Map<Repair>(rdr));
                }
            }

            return list;
        }

        private static List<DBParameter> BuildParameters(Repair repair, bool includeId)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = repair.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactName", DataValue = repair.ContactName ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ContactEmail", DataValue = repair.ContactEmail ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@JobCardNumber", DataValue = repair.JobCardNumber ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@DateLogged", DataValue = repair.DateLogged ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@LastStatusChange", DataValue = repair.LastStatusChange ?? (object)DBNull.Value, DataDbType = DbType.Date },
                new DBParameter { ParamName = "@EquipTypeID", DataValue = FkOrDbNull(repair.EquipTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@EquipSerialNumber", DataValue = repair.EquipSerialNumber ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SwopOutMachineID", DataValue = FkOrDbNull(repair.SwopOutMachineID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@EquipConditionID", DataValue = FkOrDbNull(repair.EquipConditionID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@TakenFrother", DataValue = repair.TakenFrother ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@TakenBeanLid", DataValue = repair.TakenBeanLid ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@TakenWaterLid", DataValue = repair.TakenWaterLid ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@BrokenFrother", DataValue = repair.BrokenFrother ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@BrokenBeanLid", DataValue = repair.BrokenBeanLid ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@BrokenWaterLid", DataValue = repair.BrokenWaterLid ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@RepairFaultID", DataValue = FkOrDbNull(repair.RepairFaultID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@RepairFaultDesc", DataValue = repair.RepairFaultDesc ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@RepairStatusID", DataValue = FkOrDbNull(repair.RepairStatusID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@RelatedOrderLineID", DataValue = FkOrDbNull(repair.RelatedOrderLineID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Notes", DataValue = repair.Notes ?? (object)DBNull.Value, DataDbType = DbType.String }
            };

            return parameters;
        }
    }
}
