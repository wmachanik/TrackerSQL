using System;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class AwayReasonRepository : RepositoryBase<AwayReason>
    {
        protected override string TableName => "AwayReasonTbl";
        protected override string KeyColumn => "AwayReasonID";

        protected override string CoreColumns => "AwayReasonID, ReasonDesc";
        protected override string LookupColumns => "AwayReasonID, ReasonDesc";

        /// <summary>Finds a reason by description, or inserts it when missing.</summary>
        public int ResolveOrCreateReasonId(string reasonDesc)
        {
            if (string.IsNullOrWhiteSpace(reasonDesc))
                return 0;

            string trimmed = reasonDesc.Trim();
            var existing = GetAll("ReasonDesc")
                ?.FirstOrDefault(r => string.Equals(r.ReasonDesc?.Trim(), trimmed, StringComparison.OrdinalIgnoreCase));
            if (existing != null && existing.AwayReasonID > 0)
                return existing.AwayReasonID;

            int newId = Insert(new AwayReason { ReasonDesc = trimmed });
            if (newId > 0)
                return newId;

            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "AwayReasonRepository.ResolveOrCreateReasonId: insert failed for \"" + trimmed + "\".");
            return 0;
        }
    }
}
