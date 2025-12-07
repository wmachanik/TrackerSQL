using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    // Simplified generic version; keep existing ContactsRepository for custom mapping.
    public class ContactsGenericRepository : RepositoryBase<Contact>
    {
        protected override string TableName => "ContactsTbl";
        protected override string KeyColumn => "ContactID";
    }
}
