using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class ContactsAwayPeriodRepository : RepositoryBase<ContactsAwayPeriod>
    {
        protected override string TableName => "ContactsAwayPeriodTbl";
        protected override string KeyColumn => "AwayPeriodID";
    }
}
