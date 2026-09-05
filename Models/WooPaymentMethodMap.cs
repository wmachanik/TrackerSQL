namespace TrackerSQL.Models
{
    public class WooPaymentMethodMap
    {
        public int MapID { get; set; }
        /// <summary>Woo payment method id or title matched case-insensitively (contains).</summary>
        public string MethodMatch { get; set; }
        /// <summary>Max 4 characters written to order Notes prefix.</summary>
        public string PaymentAbbrev { get; set; }
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; }
    }
}
