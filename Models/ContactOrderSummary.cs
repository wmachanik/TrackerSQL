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
        public string PurchaseOrder { get; set; }
        public string WaybillNumber { get; set; }
        public string DispatchStatus { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public int FirstItemID { get; set; }
        public string FirstItemDesc { get; set; }
        public double FirstQty { get; set; }
        public int LineCount { get; set; }

        public bool IsEditable => !Done;

        /// <summary>All lines, when loaded (Contact Portal). Empty otherwise — use ItemsDisplay.</summary>
        public System.Collections.Generic.List<string> ItemLines { get; set; }
            = new System.Collections.Generic.List<string>();

        public string StatusDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(WaybillNumber))
                    return string.IsNullOrWhiteSpace(DispatchStatus) ? "Dispatched" : DispatchStatus;
                return Done ? "Done" : string.Empty;
            }
        }

        /// <summary>Customer-facing status — never blank (Contact Portal).</summary>
        public string CustomerStatusDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(WaybillNumber))
                    return string.IsNullOrWhiteSpace(DispatchStatus)
                        ? "Dispatched"
                        : ContactPortalDisplay.TitleCase(DispatchStatus);
                if (Done)
                    return "Completed";
                return Confirmed ? "Confirmed" : "Received";
            }
        }

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
