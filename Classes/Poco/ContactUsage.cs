using System;

namespace TrackerDotNet.Classes.Poco
{
    public class ContactUsage
    {
        public int ContactID { get; set; }
        public int? LastCupCount { get; set; }
        public DateTime? NextCoffeeBy { get; set; }
        public DateTime? NextCleanOn { get; set; }
        public DateTime? NextFilterEst { get; set; }
        public DateTime? NextDescaleEst { get; set; }
        public DateTime? NextServiceEst { get; set; }
        public double? DailyConsumption { get; set; }
        public double? FilterAveCount { get; set; }
        public double? DescaleAveCount { get; set; }
        public double? ServiceAveCount { get; set; }
        public double? CleanAveCount { get; set; }
    }
}
