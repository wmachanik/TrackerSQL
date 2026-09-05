using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// WooCommerce integration settings (companion singleton). Secrets stored encrypted.
    /// </summary>
    public class WooCommerceSettings
    {
        public int SettingsID { get; set; } = 1;
        public string StoreBaseUrl { get; set; }
        public string AdminBaseUrl { get; set; }
        public string ConsumerKeyEncrypted { get; set; }
        public string ConsumerSecretEncrypted { get; set; }
        public bool IntegrationEnabled { get; set; }
        public bool WizardCompleted { get; set; }
        public DateTime? LastConnectionTestUtc { get; set; }
        public bool? LastConnectionTestOk { get; set; }
        public DateTime? LastItemsSyncUtc { get; set; }
        public DateTime? LastOrdersSyncUtc { get; set; }
        public string LastOrdersSyncOrderNumber { get; set; }
        public DateTime? LastContactsSyncUtc { get; set; }
        public bool PushEnabledStateToWoo { get; set; } = true;
        public string DisableScopeDefault { get; set; } = "MappedOnly";
        public bool PullStockQtyEnabled { get; set; }
        public string CategoryFilterMode { get; set; } = "All";
        public string GuestCheckoutContactMode { get; set; } = "ZZName";
        public string DispatchDeliveryPersonIds { get; set; } = "5,7";
        public bool TrackingNumberRequired { get; set; } = true;
        public string DispatchedWooStatus { get; set; } = "processing";
        public bool AutoCompleteOnWooCompleted { get; set; }
        /// <summary>When postcode is not in any configured range (order import).</summary>
        public int? DefaultImportAreaID { get; set; }
        /// <summary>Include province/state in semicolon billing address on import.</summary>
        public bool ImportAddressIncludeProvince { get; set; }
        /// <summary>Include country in semicolon billing address on import.</summary>
        public bool ImportAddressIncludeCountry { get; set; }
        /// <summary>Replace +27 with 0 on imported phone numbers.</summary>
        public bool ImportPhoneReplacePlus27 { get; set; } = true;
        /// <summary>Format SA phones as aaa bbb-cccc.</summary>
        public bool ImportPhoneFormatSa { get; set; } = true;
        /// <summary>
        /// Tracker ItemsTbl.ItemID for the Notes line added when Woo SKUs are written to order notes.
        /// </summary>
        public int? ImportNotesItemID { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
