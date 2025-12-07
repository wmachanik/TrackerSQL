using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class ContactUsageLinesRepository : RepositoryBase<ContactUsageLine>
    {
        protected override string TableName => "ContactUsageLinesTbl";
        protected override string KeyColumn => "ContactUsageLineNo";
    }
}
