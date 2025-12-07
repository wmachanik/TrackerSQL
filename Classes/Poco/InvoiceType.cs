namespace TrackerDotNet.Classes.Poco
{
    public class InvoiceType
    {
        public int InvoiceTypeID { get; set; }
        public string InvoiceTypeDesc { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
    }
}
