namespace TrackerSQL.Models
{
    public class EquipCondition
    {
        public int EquipConditionID { get; set; }
        public string ConditionDesc { get; set; }
        public int? SortOrder { get; set; }
        public string Notes { get; set; }
    }
}
