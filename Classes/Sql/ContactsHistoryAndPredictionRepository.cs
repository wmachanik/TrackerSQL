using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class ContactsHistoryAndPredictionRepository : RepositoryBase<ContactsHistoryAndPrediction>
    {
        protected override string TableName => "ContactsHistoryAndPredictionTbl";
        protected override string KeyColumn => "HistoryID";
    }
}
