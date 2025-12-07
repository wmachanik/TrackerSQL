using System;
namespace TrackerDotNet.Classes.Poco
{
    public class TempOrdersHeader
    {
        public int TOHeaderID { get; set; }
        public int ContactID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? RoastDate { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public int? ToBeDeliveredByID { get; set; }
        public bool? Confirmed { get; set; }
        public bool? Done { get; set; }
        public string Notes { get; set; }
    }
}
