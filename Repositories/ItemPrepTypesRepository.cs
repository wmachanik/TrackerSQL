using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ItemPrepTypesRepository : RepositoryBase<ItemPrepType>
    {
        protected override string TableName => "ItemPrepTypesTbl";
        protected override string KeyColumn => "ItemPrepID";

        protected override string CoreColumns =>
            "ItemPrepID, ItemPrepTypeDesc, IdentifyingChar";

        protected override string LookupColumns =>
            "ItemPrepID, ItemPrepTypeDesc";
    }
}
