using System;
namespace TrackerSQL.Models
{
    public class TempOrdersHeader
    {
        public int TOHeaderID { get; set; }
        public int ContactID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? PrepDate { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public int? ToBeDeliveredByID { get; set; }
        public bool? Confirmed { get; set; }
        public bool? Done { get; set; }
        public string Notes { get; set; }
    }
}
