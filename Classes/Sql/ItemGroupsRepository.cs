using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class ItemGroupsRepository : RepositoryBase<ItemGroup>
    {
        protected override string TableName => "ItemGroupsTbl";
        protected override string KeyColumn => "ItemGroupID";

        protected override string CoreColumns => "ItemGroupID, GroupReferenceItemID, ItemID, ItemSortPos, Enabled";
        protected override string LookupColumns => "ItemGroupID, GroupReferenceItemID";
    }
}
