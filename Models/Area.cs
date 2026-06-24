namespace TrackerSQL.Models
{
    public class Area
    {
        public int AreaID { get; set; }
        public int ID { get; set; } // Alias for AreaID (for GridView DataKeyNames)
        public string AreaName { get; set; }
        public int? PrepDayOfWeekID { get; set; }
        public int? DeliveryDelay { get; set; }
    }
}

