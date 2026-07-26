using System;

namespace TrackerSQL.Models
{
    public class ClosureDate
    {
        public int ClosureDateID { get; set; }
        public DateTime DateClosed { get; set; }
        public DateTime? DateReopen { get; set; }
        public DateTime? NextPreparationDate { get; set; }
        public string Comments { get; set; }
    }
}
