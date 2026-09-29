namespace TrackerSQL.Models
{
    public class InvoiceType
    {
        public int InvoiceTypeID { get; set; }
        public string InvoiceTypeDesc { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
        /// <summary>Short text of the pill shown on the delivery sheet; empty = no pill (e.g. standard).</summary>
        public string PillLabel { get; set; }
        /// <summary>Pill background, a CSS colour such as #6f42c1.</summary>
        public string PillColour { get; set; }
    }
}
