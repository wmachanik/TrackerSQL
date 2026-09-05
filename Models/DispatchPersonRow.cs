using System;

namespace TrackerSQL.Models
{
    /// <summary>One Delivered-by person row for Woo Shipping dispatch/waybill settings.</summary>
    [Serializable]
    public class DispatchPersonRow
    {
        public int PersonID { get; set; }
        public string DisplayName { get; set; }
        public bool UseWaybill { get; set; }
    }
}
