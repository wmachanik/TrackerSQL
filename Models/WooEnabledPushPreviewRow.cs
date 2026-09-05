namespace TrackerSQL.Models
{
    /// <summary>One saved map row for Enabled sync dry-run / push preview.</summary>
    public class WooEnabledPushPreviewRow
    {
        public int MappingID { get; set; }
        public int ItemID { get; set; }
        public string ItemDesc { get; set; }
        public string ItemSku { get; set; }
        public bool TrackerEnabled { get; set; }
        public long WooProductId { get; set; }
        public long? WooVariationId { get; set; }
        public string LastWooStatus { get; set; }
        /// <summary>Status in Woo now (from last Pull products, or last push if no cache).</summary>
        public string CurrentWooStatus { get; set; }
        public string NewWooStatus { get; set; }
        public bool NeedsChange { get; set; }
        /// <summary>Dry-run: Planned. Push: OK or error detail.</summary>
        public string Result { get; set; }
    }
}
