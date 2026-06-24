using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class HolidayClosuresRepository : RepositoryBase<HolidayClosure>
    {
        protected override string TableName => "HolidayClosuresTbl";
        protected override string KeyColumn => "HolidayClosureID";
    }
}