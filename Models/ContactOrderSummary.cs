using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// One OrdersTbl row for Contact Details, with a short first-line preview.
    /// </summary>
    public class ContactOrderSummary
    {
        public int OrderID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? PrepDate { get; set; }
        public DateTime? RequiredByDate { get; set; }
        public bool Confirmed { get; set; }
        public bool Done { get; set; }
        public bool InvoiceDone { get; set; }
        public string Notes { get; set; }
        public int FirstItemID { get; set; }
        public string FirstItemDesc { get; set; }
        public double FirstQty { get; set; }
        public int LineCount { get; set; }

        public bool IsEditable => !Done;

        public string EditNavigateUrl =>
            "~/Pages/OrderDetail.aspx?OrderID=" + OrderID;

        /// <summary>
        /// First line item text; appends " ..." when the order has more lines.
        /// </summary>
        public string ItemsDisplay
        {
            get
            {
                string item = string.IsNullOrWhiteSpace(FirstItemDesc)
                    ? (FirstItemID > 0 ? ("Item #" + FirstItemID) : "(no items)")
                    : FirstItemDesc.Trim();

                string qty = FirstQty > 0 ? (" × " + FirstQty.ToString("0.###")) : string.Empty;
                string more = LineCount > 1 ? " ..." : string.Empty;
                return item + qty + more;
            }
        }

        public ContactOrderSummary()
        {
            Notes = string.Empty;
            FirstItemDesc = string.Empty;
        }
    }
}
