using System;
using System.Collections.Generic;
using System.Globalization;

namespace TrackerSQL.Models
{
    public enum WooOrderImportMode
    {
        Specific = 0,
        Last = 1,
        SinceLastSync = 2,
        DateRange = 3,
        /// <summary>Orders created today in app local time (SAST).</summary>
        Today = 4,
        /// <summary>Orders from Monday of the current week through end of Sunday (app local).</summary>
        ThisWeek = 5
    }

    [Serializable]
    public class WooOrderLinePreview
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public double WooQty { get; set; }
        public string MapType { get; set; }
        public int? TrackerItemId { get; set; }
        public string TrackerSku { get; set; }
        public double TrackerQty { get; set; }
        public int? PackagingId { get; set; }
        /// <summary>Tracker ItemPrepTypesTbl id from Woo Prep Type (e.g. Whole beans → beans).</summary>
        public int? PrepTypeId { get; set; }
        /// <summary>When true, do not copy packaging from a prior Tracker order (Woo already chose prep/pack).</summary>
        public bool SuppressPriorPackagingInfer { get; set; }
        /// <summary>Attribute-resolved note fragments (Prep Type, Size, etc.) for order Notes.</summary>
        public List<string> AttributeNoteParts { get; set; } = new List<string>();
        public bool IsGear { get; set; }
        public bool CanImport { get; set; }
        /// <summary>Line has no Woo map; qty/SKU appended to order notes on import.</summary>
        public bool IsUnmappedForNotes { get; set; }
        public string Note { get; set; }
    }

    [Serializable]
    public class WooOrderImportPreviewRow
    {
        public long WooOrderId { get; set; }
        public string WooOrderNumber { get; set; }
        public string WooStatus { get; set; }
        public DateTime? OrderDate { get; set; }
        public bool AlreadyImported { get; set; }
        public int? ExistingTrackerOrderId { get; set; }
        public bool IsGearOnly { get; set; }
        public bool UseZzName { get; set; }
        public int? MatchedContactId { get; set; }
        public bool IsNewContact { get; set; }
        public bool CanAddContact { get; set; }
        public bool CanUpdateContact { get; set; }
        public bool ContactHasShippingChanges { get; set; }
        /// <summary>Woo company differs from contact — show company-name mode prompt on Update contact.</summary>
        public bool NeedsCompanyNameDecision { get; set; }
        public string WooCompanyName { get; set; }
        public string ContactCompanyName { get; set; }
        /// <summary>New or Found — shown before contact name in preview.</summary>
        public string ContactStatusLabel { get; set; }
        public string ContactSummary { get; set; }
        public string ContactDisplayName { get; set; }
        /// <summary>When several contacts share the order email, notes how the match was chosen.</summary>
        public string ContactEmailConflict { get; set; }
        public string ShippingSummary { get; set; }
        public string PaymentAbbrev { get; set; }
        public string ShippingMethod { get; set; }
        public int? ResolvedAreaId { get; set; }
        public string ResolvedAreaName { get; set; }
        public int? DeliveryPersonId { get; set; }
        public bool CanImport { get; set; }
        public bool Selected { get; set; } = true;
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Conflicts { get; set; } = new List<string>();
        /// <summary>Short summary e.g. "1× order notes (8BehBra3)".</summary>
        public string LinesSummary { get; set; }
        public List<WooOrderLinePreview> Lines { get; set; } = new List<WooOrderLinePreview>();
        /// <summary>Not stored in ViewState/Session — reload from Woo at commit if needed.</summary>
        [NonSerialized]
        public string RawJson;
    }

    /// <summary>
    /// Maps / lookups loaded once per pull or import batch (avoids N+1 DB reads in BuildPreview).
    /// </summary>
    public class WooOrderImportPreviewContext
    {
        public HashSet<long> GearCategoryIds { get; set; } = new HashSet<long>();
        public List<WooPaymentMethodMap> PaymentMaps { get; set; } = new List<WooPaymentMethodMap>();
        public Dictionary<string, WooItemMapping> ItemMapsByKey { get; set; }
            = new Dictionary<string, WooItemMapping>(StringComparer.Ordinal);
        public Dictionary<int, Item> ItemsById { get; set; } = new Dictionary<int, Item>();
        public List<WooCategoryFilter> CategoryFilters { get; set; } = new List<WooCategoryFilter>();
        public string CategoryFilterMode { get; set; } = "All";
        public WooAttributeResolveCache AttributeCache { get; set; }

        public static string ItemMapKey(long productId, long? variationId)
        {
            long v = variationId.HasValue && variationId.Value > 0 ? variationId.Value : 0;
            return productId.ToString(CultureInfo.InvariantCulture) + ":" + v.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>Attribute maps + packaging lists for ResolveOrderLineAttributes without per-line DB hits.</summary>
    public class WooAttributeResolveCache
    {
        public List<WooAttributeMap> AttrMaps { get; set; } = new List<WooAttributeMap>();
        public Dictionary<string, WooAttributeParent> ParentByName { get; set; }
            = new Dictionary<string, WooAttributeParent>(StringComparer.OrdinalIgnoreCase);
        public List<ItemPackaging> AllPackagings { get; set; } = new List<ItemPackaging>();
        public Dictionary<int, HashSet<int>> AllowedPackagingByServiceType { get; set; }
            = new Dictionary<int, HashSet<int>>();
    }

    public class WooOrderImportBatchResult
    {
        public int Imported { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        /// <summary>Tracker OrderID from the most recent successful import/update in this batch.</summary>
        public int? LastTrackerOrderId { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }

    public class WooOrderImportConflictRow
    {
        public int OrderID { get; set; }
        public string WooOrderNumber { get; set; }
        public string ImportConflicts { get; set; }
        public DateTime? LastSyncedUtc { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Notes { get; set; }
        public string PurchaseOrder { get; set; }
    }

    /// <summary>Packaging copied from a prior Tracker order for the same contact + sort group.</summary>
    public class PriorOrderPackagingHint
    {
        public int PackagingId { get; set; }
        public int SourceOrderId { get; set; }
        public int SourceItemId { get; set; }
        public string SourceItemSku { get; set; }
        public string SourceItemDesc { get; set; }
        public int SortOrder { get; set; }
    }
}
