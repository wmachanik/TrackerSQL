namespace TrackerDotNet.Classes.Poco
{
    public class PaymentTerm
    {
        public int PaymentTermID { get; set; }
        public string PaymentTermDesc { get; set; }
        public int? PaymentDays { get; set; }
        public int? DayOfMonth { get; set; }
        public bool? UseDays { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
    }
}
