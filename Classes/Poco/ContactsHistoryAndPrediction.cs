using System;
namespace TrackerDotNet.Classes.Poco
{
    public class ContactsHistoryAndPrediction
    {
        public int HistoryID { get; set; }
        public int ContactID { get; set; }
        public DateTime? ItemDate { get; set; }
        public int? LastCount { get; set; }
        public DateTime? NextConsumableBy { get; set; }
        public DateTime? NextCleanOn { get; set; }
        public DateTime? NextFilterEst { get; set; }
        public DateTime? NextDescaleEst { get; set; }
        public DateTime? NextServiceEst { get; set; }
        public double? DailyConsumption { get; set; }
        public double? FilterAveCount { get; set; }
        public double? DescaleAveCount { get; set; }
        public double? ServiceAveCount { get; set; }
    }
}
