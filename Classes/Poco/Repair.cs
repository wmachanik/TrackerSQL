using System;
namespace TrackerDotNet.Classes.Poco
{
    public class Repair
    {
        public int RepairID { get; set; }
        public int ContactID { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string JobCardNumber { get; set; }
        public DateTime? DateLogged { get; set; }
        public DateTime? LastStatusChange { get; set; }
        public int? EquipTypeID { get; set; }
        public string EquipSerialNumber { get; set; }
        public int? SwopOutMachineID { get; set; }
        public int? EquipConditionID { get; set; }
        public bool? TakenFrother { get; set; }
        public bool? TakenBeanLid { get; set; }
        public bool? TakenWaterLid { get; set; }
        public bool? BrokenFrother { get; set; }
        public bool? BrokenBeanLid { get; set; }
        public bool? BrokenWaterLid { get; set; }
        public int? RepairFaultID { get; set; }
        public string RepairFaultDesc { get; set; }
        public int? RepairStatusID { get; set; }
        public int? RelatedOrderID { get; set; }
        public string Notes { get; set; }
    }
}
