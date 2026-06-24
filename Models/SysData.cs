using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// POCO model for SysDataTbl - System configuration and runtime settings
    /// 
    /// Property names MUST match database column names exactly (case-insensitive for mapping):
    /// - ID → ID
    /// - LastRecurringDate → LastRecurringDate  
    /// - DoRecurringOrders → DoRecurringOrders
    /// - DateLastPrepDateCalcd → DateLastPrepDateCalcd
    /// - MinReminderDate → MinReminderDate
    /// - GroupItemServiceTypeID → GroupItemServiceTypeID
    /// - InternalContactIDs → InternalContactIDs
    /// </summary>
    public class SysData
    {
        public int ID { get; set; }

        public DateTime? LastRecurringDate { get; set; }

        public bool? DoRecurringOrders { get; set; }

        public DateTime? DateLastPrepDateCalcd { get; set; }

        public DateTime? MinReminderDate { get; set; }

        /// <summary>
        /// Group Item Service Type ID - links to ItemServiceTypesTbl
        /// Used to group items by their service type (Coffee, Cleaning, Group, etc.)
        /// Column name in database: GroupItemServiceTypeID
        /// </summary>
        public int? GroupItemServiceTypeID { get; set; }

        /// <summary>
        /// Internal Contact IDs
        /// Column name in database: InternalContactIDs
        /// </summary>
        public string InternalContactIDs { get; set; }
    }
}