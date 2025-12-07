using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class ItemPrepTypesRepository : RepositoryBase<ItemPrepType>
    {
        protected override string TableName => "ItemPrepTypesTbl";
        protected override string KeyColumn => "ItemPrepID";

        protected override string CoreColumns => "ItemPrepID, ItemPrepTypeName";
        protected override string LookupColumns => "ItemPrepID, ItemPrepTypeName";
    }
}
