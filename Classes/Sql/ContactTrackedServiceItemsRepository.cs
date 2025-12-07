using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class ContactTrackedServiceItemsRepository : RepositoryBase<ContactTrackedServiceItem>
    {
        protected override string TableName => "ContactTrackedServiceItemsTbl";
        protected override string KeyColumn => "ContactTrackedServiceItemsID";
    }
}
