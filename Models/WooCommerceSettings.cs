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
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
