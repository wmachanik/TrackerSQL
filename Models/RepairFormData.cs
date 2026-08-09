using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Repair grid/form DTO — property names match legacy RepairsTbl bindings.
    /// </summary>
    public class RepairFormData
    {
        public int RepairID { get; set; }
        public long CustomerID { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string JobCardNumber { get; set; } = string.Empty;
        public DateTime DateLogged { get; set; }
        public DateTime LastStatusChange { get; set; }
        public int MachineTypeID { get; set; }
        public string MachineSerialNumber { get; set; } = string.Empty;
        public int SwopOutMachineID { get; set; }
        public int MachineConditionID { get; set; }
        public bool TakenFrother { get; set; }
        public bool TakenBeanLid { get; set; } = true;
        public bool TakenWaterLid { get; set; } = true;
        public bool BrokenFrother { get; set; }
        public bool BrokenBeanLid { get; set; }
        public bool BrokenWaterLid { get; set; }
        public int RepairFaultID { get; set; }
        public string RepairFaultDesc { get; set; } = string.Empty;
        public int RepairStatusID { get; set; }
        public int RelatedOrderLineID { get; set; }
        /// <summary>OrderID for RelatedOrderLineID (list/detail links). 0 when none.</summary>
        public int RelatedOrderID { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
