using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class RecurranceTypeRepository : RepositoryBase<RecurranceType>
    {
        protected override string TableName => "RecurranceTypeTbl";
        protected override string KeyColumn => "RecurringTypeID";
    }
}