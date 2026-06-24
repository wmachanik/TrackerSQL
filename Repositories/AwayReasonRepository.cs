using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class AwayReasonRepository : RepositoryBase<AwayReason>
    {
        protected override string TableName => "AwayReasonTbl";
        protected override string KeyColumn => "AwayReasonID";

        protected override string CoreColumns => "AwayReasonID, ReasonDesc";
        protected override string LookupColumns => "AwayReasonID, ReasonDesc";
    }
}
