using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// Repository for ClosureDatesTbl.
    ///
    /// Standard CRUD operations are inherited from RepositoryBase<ClosureDate>:
    /// GetById, GetKeyColsById, GetAll, GetLookupValues, GetLookupList,
    /// GetAllEnabled, Insert, Update and Delete.
    /// </summary>
    public class ClosureDatesRepository : RepositoryBase<ClosureDate>
    {
        protected override string TableName => "ClosureDatesTbl";
        protected override string KeyColumn => "ClosureDateID";
    }
}
