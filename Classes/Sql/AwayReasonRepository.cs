using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class AwayReasonRepository : RepositoryBase<AwayReason>
    {
        protected override string TableName => "AwayReasonTbl";
        protected override string KeyColumn => "AwayReasonID";

        protected override string CoreColumns => "AwayReasonID, ReasonDesc";
        protected override string LookupColumns => "AwayReasonID, ReasonDesc";
    }
}
