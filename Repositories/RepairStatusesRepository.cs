using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class RepairStatusesRepository : RepositoryBase<RepairStatus>
    {
        protected override string TableName => "RepairStatusesTbl";
        protected override string KeyColumn => "RepairStatusID";

        protected override string CoreColumns =>
            "RepairStatusID, RepairStatusDesc, EmailContact, SortOrder, Notes, StatusNote";

        protected override string LookupColumns =>
            "RepairStatusID, RepairStatusDesc";

        public string GetRepairStatusDesc(int repairStatusId)
        {
            return ExecuteScalar<string>(
                "SELECT RepairStatusDesc FROM RepairStatusesTbl WHERE RepairStatusID = @RepairStatusID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@RepairStatusID", DataValue = repairStatusId, DataDbType = DbType.Int32 }
                });
        }

        /// <summary>
        /// StatusNote for emails/order notes; falls back to RepairStatusDesc when StatusNote is blank
        /// (same behaviour as legacy RepairStatusesTbl.GetStatusNote).
        /// </summary>
        public string GetStatusNote(int repairStatusId)
        {
            string statusNote = ExecuteScalar<string>(
                "SELECT StatusNote FROM RepairStatusesTbl WHERE RepairStatusID = @RepairStatusID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@RepairStatusID", DataValue = repairStatusId, DataDbType = DbType.Int32 }
                });
            return string.IsNullOrWhiteSpace(statusNote)
                ? GetRepairStatusDesc(repairStatusId)
                : statusNote;
        }
    }
}