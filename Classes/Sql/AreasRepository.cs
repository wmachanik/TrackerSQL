using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class AreasRepository : RepositoryBase<Area>
    {
        protected override string TableName => "AreasTbl";
        protected override string KeyColumn => "AreaID";

        // Use real column names; no aliases
        protected override string CoreColumns => "AreaID, AreaName";
        protected override string LookupColumns => "AreaID, AreaName";
    }
}
