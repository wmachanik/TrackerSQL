using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Singleton header for optional system modules (companion — not SysDataTbl).
    /// </summary>
    public class SystemPreferencesHdr
    {
        public int PrefsID { get; set; } = 1;
        public bool WooCommerceEnabled { get; set; }
        public bool WooWizardCompleted { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
