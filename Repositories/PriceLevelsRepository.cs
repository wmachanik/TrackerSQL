using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class PriceLevelsRepository : RepositoryBase<PriceLevel>
    {
        protected override string TableName => "PriceLevelsTbl";
        protected override string KeyColumn => "PriceLevelID";

        protected override string CoreColumns =>
            "PriceLevelID, PriceLevelDesc, PricingFactor, Enabled, Notes";

        protected override string LookupColumns =>
            "PriceLevelID, PriceLevelDesc";
    }
}