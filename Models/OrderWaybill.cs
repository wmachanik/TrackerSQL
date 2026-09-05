using System;

namespace TrackerSQL.Models
{
    public class OrderWaybill
    {
        public int WaybillID { get; set; }
        public int OrderID { get; set; }
        public int? ContactID { get; set; }
        public string WaybillNumber { get; set; }
        public string Carrier { get; set; }
        public string DispatchStatus { get; set; }
        public DateTime DispatchedAt { get; set; }
        public long? WooOrderId { get; set; }
        public bool WooNotePosted { get; set; }
        public bool CustomerEmailSent { get; set; }
        public string CreatedBy { get; set; }

        public string OrderNavigateUrl => "~/Pages/OrderDetail.aspx?OrderID=" + OrderID;

        public string WooNoteDisplay => WooNotePosted ? "Y" : "";
        public string EmailSentDisplay => CustomerEmailSent ? "Y" : "";
    }
}
