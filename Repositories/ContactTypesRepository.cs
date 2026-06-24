using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactTypesRepository : RepositoryBase<ContactType>
    {
        protected override string TableName => "ContactTypesTbl";
        protected override string KeyColumn => "ContactTypeID";

        protected override string CoreColumns =>
            "ContactTypeID, ContactTypeDesc";

        protected override string LookupColumns =>
            "ContactTypeID, ContactTypeDesc";
    }
}