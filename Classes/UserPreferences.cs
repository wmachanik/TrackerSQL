using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrackerDotNet.Classes
{
    public class UserPreferences
    {
        public Guid UserId { get; set; }
        public string TimeZoneId { get; set; } = "South Africa Standard Time";
        public string Language { get; set; } = "en-ZA";
        public DateTime LoadedOn { get; set; }

        public TimeZoneInfo GetTimeZoneInfo()
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
    }
}