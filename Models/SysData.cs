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
    /// - GroupReferenceItemID → GroupReferenceItemID
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
        /// Group reference item ID — links to ItemsTbl (legacy Access GroupItemTypeID).
        /// Column name in database: GroupReferenceItemID
        /// </summary>
        public int? GroupReferenceItemID { get; set; }

        /// <summary>
        /// Internal Contact IDs
        /// Column name in database: InternalContactIDs
        /// </summary>
        public string InternalContactIDs { get; set; }
    }
}