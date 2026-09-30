using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrackerSQL.Classes
{
    public class UserPreferences
    {
        public Guid UserId { get; set; }
        public string TimeZoneId { get; set; } = "South Africa Standard Time";
        public string Language { get; set; } = "en-ZA";
        public DateTime LoadedOn { get; set; }
        /// <summary>Delivery Sheet shows the Seq / Area / ETA / Status columns (off by default).</summary>
        public bool ShowDeliveryRouteEta { get; set; }
        /// <summary>Show driver positions (Driver Tracking page / map) (off by default).</summary>
        public bool DriverTracking { get; set; }

        public TimeZoneInfo GetTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
    }
}
