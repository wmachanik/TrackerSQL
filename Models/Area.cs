namespace TrackerSQL.Models
{
    public class Area
    {
        public int AreaID { get; set; }

        /// <summary>Read-only alias for any leftover ID bindings. Not mapped to SQL.</summary>
        public int ID => AreaID;

        public string AreaName { get; set; }
        public int? PrepDayOfWeekID { get; set; }
        public int? DeliveryDelay { get; set; }
    }
}
