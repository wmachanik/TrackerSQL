using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class AreaPrepDaysRepository : RepositoryBase<AreaPrepDays>
    {
        protected override string TableName => "AreaPrepDaysTbl";
        protected override string KeyColumn => "AreaPrepDaysID";
    }
}
