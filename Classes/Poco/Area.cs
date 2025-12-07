namespace TrackerDotNet.Classes.Poco
{
    public class Area
    {
        public int AreaID { get; set; }
        // Real DB column name expected: AreaName
        public string AreaName { get; set; }
        public int? PrepDayOfWeekID { get; set; }
        public int? DeliveryDelay { get; set; }
    }
}
