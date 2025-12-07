using TrackerDotNet.Classes.Poco;
namespace TrackerDotNet.Classes.Sql
{
    public class EquipTypesRepository : RepositoryBase<EquipType>
    {
        protected override string TableName => "EquipTypesTbl";
        protected override string KeyColumn => "EquipTypeID";

        protected override string CoreColumns => "EquipTypeID, EquipTypeName";
        protected override string LookupColumns => "EquipTypeID, EquipTypeName";
    }
}
