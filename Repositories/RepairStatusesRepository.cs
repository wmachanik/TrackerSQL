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

        public string GetStatusNote(int repairStatusId)
        {
            return ExecuteScalar<string>(
                "SELECT StatusNote FROM RepairStatusesTbl WHERE RepairStatusID = @RepairStatusID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@RepairStatusID", DataValue = repairStatusId, DataDbType = DbType.Int32 }
                });
        }
    }
}