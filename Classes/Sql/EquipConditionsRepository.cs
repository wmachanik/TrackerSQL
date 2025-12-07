using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class EquipConditionsRepository : RepositoryBase<EquipCondition>
    {
        protected override string TableName => "EquipConditionsTbl";
        protected override string KeyColumn => "EquipConditionID";

        protected override string CoreColumns => "EquipConditionID, ConditionDesc";
        protected override string LookupColumns => "EquipConditionID, ConditionDesc";
    }
}
