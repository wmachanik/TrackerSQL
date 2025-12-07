using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class ClosureDatesRepository : RepositoryBase<ClosureDate>
    {
        protected override string TableName => "ClosureDatesTbl";
        protected override string KeyColumn => "ClosureDateID";
    }
}
