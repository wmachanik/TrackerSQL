using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class RecurringTypeRepository : RepositoryBase<RecurringType>
    {
        protected override string TableName => "RecurringTypesTbl";
        protected override string KeyColumn => "RecurringTypeID";
    }
}
