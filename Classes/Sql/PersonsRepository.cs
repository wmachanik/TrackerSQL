using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class PersonsRepository : RepositoryBase<Person>
    {
        protected override string TableName => "PeopleTbl";
        protected override string KeyColumn => "PersonID";

        // Removed alias; use corrected column/property name directly
        protected override string CoreColumns => "PersonID, Abbreviation, Enabled";
        protected override string LookupColumns => "PersonID, Abbreviation";
    }
}
