using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class RepairFaultsRepository : RepositoryBase<RepairFault>
    {
        protected override string TableName => "RepairFaultsTbl";
        protected override string KeyColumn => "RepairFaultID";

        protected override string CoreColumns =>
            "RepairFaultID, RepairFaultDesc, SortOrder, Notes";

        protected override string LookupColumns =>
            "RepairFaultID, RepairFaultDesc";

        public string GetRepairFaultDesc(int repairFaultId)
        {
            if (repairFaultId <= 0) return string.Empty;

            return ExecuteScalar<string>(
                "SELECT RepairFaultDesc FROM RepairFaultsTbl WHERE RepairFaultID = @RepairFaultID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@RepairFaultID", DataValue = repairFaultId, DataDbType = DbType.Int32 }
                }) ?? string.Empty;
        }
    }
}
