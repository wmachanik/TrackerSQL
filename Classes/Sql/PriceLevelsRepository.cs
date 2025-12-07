using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class PriceLevelsRepository : RepositoryBase<PriceLevel>
    {
        protected override string TableName => "PriceLevelsTbl";
        protected override string KeyColumn => "PriceLevelID";

        protected override string CoreColumns => "PriceLevelID, PriceLevelDesc, Enabled";
        protected override string LookupColumns => "PriceLevelID, PriceLevelDesc";
    }
}
