namespace TrackerSQL.Models
{
    /// <summary>
    /// Represents a prep/delivery rule for an area (formerly "Area").
    /// Used for calculating optimal delivery dates based on area schedules.
    /// </summary>
    public class PrepRule
    {
        /// <summary>Day of week ID (0=Sunday, 6=Saturday) when roasting occurs</summary>
        public int PrepDayOfWeekID { get; set; }

        /// <summary>Number of days between roast and delivery</summary>
        public int DeliveryDelayDays { get; set; }

        /// <summary>Display order for this rule</summary>
        public int DeliveryOrder { get; set; }
    }
}