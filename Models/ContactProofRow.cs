using System;
using System.Globalization;

namespace TrackerSQL.Models
{
    /// <summary>
    /// One driver-app proof of delivery (MobileDeliveryProofTbl) for the Contact Details "Proof of Delivery" tab.
    /// </summary>
    public class ContactProofRow
    {
        public long ProofID { get; set; }
        public int OrderID { get; set; }
        public DateTime DeliveredAt { get; set; }
        public string Outcome { get; set; }
        public string Reason { get; set; }
        public string ReceivedByName { get; set; }
        public string DriverNote { get; set; }
        public string MissingItems { get; set; }
        public string Driver { get; set; }
        public string Status { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public byte[] SignatureImage { get; set; }
        public string SignatureContentType { get; set; }

        public string OutcomeDisplay =>
            string.Equals(Outcome, "NotDelivered", StringComparison.OrdinalIgnoreCase) ? "Not delivered" : "Delivered";

        public string OrderNavigateUrl => "~/Pages/OrderDetail.aspx?OrderID=" + OrderID;

        /// <summary>Signature as an inline data: URL (signatures are small PNG/JPEG images); empty when none.</summary>
        public string SignatureDataUrl
        {
            get
            {
                if (SignatureImage == null || SignatureImage.Length == 0)
                    return string.Empty;
                string type = string.IsNullOrWhiteSpace(SignatureContentType) ? "image/png" : SignatureContentType.Trim();
                return "data:" + type + ";base64," + Convert.ToBase64String(SignatureImage);
            }
        }

        public bool HasSignature => SignatureImage != null && SignatureImage.Length > 0;

        /// <summary>Where the phone was when the proof was captured (OpenStreetMap); empty when not recorded.</summary>
        public string MapUrl
        {
            get
            {
                if (!Latitude.HasValue || !Longitude.HasValue)
                    return string.Empty;
                string lat = Latitude.Value.ToString("0.######", CultureInfo.InvariantCulture);
                string lng = Longitude.Value.ToString("0.######", CultureInfo.InvariantCulture);
                return "https://www.openstreetmap.org/?mlat=" + lat + "&mlon=" + lng + "#map=18/" + lat + "/" + lng;
            }
        }

        public bool HasLocation => MapUrl.Length > 0;
    }
}
