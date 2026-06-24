using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ItemServiceTypesRepository : RepositoryBase<ItemServiceType>
    {
        protected override string TableName => "ItemServiceTypesTbl";
        protected override string KeyColumn => "ItemServiceTypeID";

        protected override string CoreColumns =>
            "ItemServiceTypeID, ItemServiceTypeName, Description, ItemPackagingID, ItemPrepTypeID";

        protected override string LookupColumns =>
            "ItemServiceTypeID, ItemServiceTypeName";
    }
}