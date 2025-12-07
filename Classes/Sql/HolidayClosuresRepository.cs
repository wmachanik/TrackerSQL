using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class HolidayClosuresRepository : RepositoryBase<HolidayClosure>
    {
        protected override string TableName => "HolidayClosuresTbl";
        protected override string KeyColumn => "HolidayClosureID";
    }
}
