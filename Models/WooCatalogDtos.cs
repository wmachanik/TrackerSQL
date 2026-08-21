using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TrackerSQL.Models
{
    public class WooCategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long Parent { get; set; }
        public int Count { get; set; }
    }

    public class WooAttributeValue
    {
        public string Name { get; set; }
        public string Option { get; set; }
        /// <summary>Woo term usage count when loaded from attributes/terms API; otherwise 0.</summary>
        public int SampleCount { get; set; }
    }

    /// <summary>Global Woo product attribute (parent) from /products/attributes.</summary>
    public class WooGlobalAttributeDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int TermCount { get; set; }
    }

    public class WooProductDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public long? ParentId { get; set; }
        public long? VariationId { get; set; }
        /// <summary>Parent product SKU when this row is a variation.</summary>
        public string ParentSku { get; set; }
        /// <summary>Parent product name when this row is a variation.</summary>
        public string ParentName { get; set; }
        public string StockStatus { get; set; }
        public bool ManageStock { get; set; }
        /// <summary>True when this DTO is a variable-product header (variations follow).</summary>
        public bool IsParentGroup { get; set; }
        /// <summary>All Woo variations (including out of stock). Parent groups only.</summary>
        public int VariationTotalCount { get; set; }
        /// <summary>Publish + in-stock variations kept for mapping. Parent groups only.</summary>
        public int VariationInStockCount { get; set; }
        public List<long> CategoryIds { get; set; } = new List<long>();
        public string CategoriesLabel { get; set; }
        public List<WooAttributeValue> Attributes { get; set; } = new List<WooAttributeValue>();

        public string AttributesLabel
        {
            get
            {
                if (Attributes == null || Attributes.Count == 0)
                    return string.Empty;
                return string.Join(", ",
                    Attributes
                        .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Name))
                        .Select(a => a.Name.Trim() + ": " + (a.Option ?? string.Empty).Trim()));
            }
        }

        public bool HasOwnSku
        {
            get { return !string.IsNullOrWhiteSpace(Sku); }
        }
    }

    /// <summary>Result of a products sync: mappable rows vs products missing a Woo SKU.</summary>
    public class WooProductPullResult
    {
        public List<WooProductMapRow> MappingRows { get; set; } = new List<WooProductMapRow>();
        public List<WooProductMapRow> MissingSkuRows { get; set; } = new List<WooProductMapRow>();
        /// <summary>Parent/simple products scanned from Woo (before variation expand).</summary>
        public int ParentsScanned { get; set; }
        public bool HitCatalogCap { get; set; }
    }

    /// <summary>Row for the mapping pull grid (session-backed).</summary>
    public class WooProductMapRow
    {
        public const int DestinationNotesValue = -1;
        /// <summary>Create an enabled Tracker item from the Woo parent SKU on Save.</summary>
        public const int DestinationCreateParent = -2;
        public const string ImportModeVariants = "Variants";
        public const string ImportModeParentNotes = "ParentNotes";
        /// <summary>Map the parent Tracker SKU only; variation attributes go to order notes.</summary>
        public const string ImportModeParentItem = "ParentItem";
        /// <summary>Skip this Woo product and all its variants on order import.</summary>
        public const string ImportModeExclude = "Exclude";

        public long WooProductId { get; set; }
        public long? WooVariationId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        /// <summary>Parent Woo product SKU (variations); used to match Tracker item.</summary>
        public string ParentSku { get; set; }
        public string ParentName { get; set; }
        public string Status { get; set; }
        public string CategoriesLabel { get; set; }
        /// <summary>Short variant options only (e.g. "250g Packet · Fine").</summary>
        public string AttributesLabel { get; set; }
        public string MatchReason { get; set; }
        public int SuggestedItemID { get; set; }
        public string SuggestedItemDesc { get; set; }
        public int MappedItemID { get; set; }
        public int ExistingMappingID { get; set; }
        public double QtyFactor { get; set; } = 1;
        public int? PackagingID { get; set; }
        public int? SuggestedPackagingID { get; set; }
        public bool IsVariation { get; set; }
        /// <summary>Variable product header row (children are variations).</summary>
        public bool IsParentGroup { get; set; }
        /// <summary>Variants = map children; ParentItem = one parent SKU, variants → notes; ParentNotes = whole product to notes.</summary>
        public string ImportMode { get; set; } = ImportModeVariants;
        /// <summary>True when ImportMode came from a category default (safe to refresh when category defaults change).</summary>
        public bool UsesCategoryImportDefault { get; set; }
        /// <summary>True when the user changed Mode / destination / import on this parent in the UI.</summary>
        public bool ImportModeUserSet { get; set; }
        public int Depth { get; set; }
        /// <summary>Parent groups start collapsed; + expands variant rows.</summary>
        public bool GroupExpanded { get; set; }
        /// <summary>Woo variation count including out of stock (parent groups).</summary>
        public int VariationTotalCount { get; set; }
        /// <summary>In-stock publish variations shown under this parent.</summary>
        public int VariationInStockCount { get; set; }
        /// <summary>When true, order import will include this line (default for publish).</summary>
        public bool IncludeInImport { get; set; } = true;
        /// <summary>Route line to order Notes instead of an ItemsTbl SKU (guest / walk-in).</summary>
        public bool MapToNotes { get; set; }
        /// <summary>Include this row in the next bulk Save selected.</summary>
        public bool ApplySelected { get; set; }
        /// <summary>New SKU entered on the Missing SKUs tab (before write-back).</summary>
        public string NewSku { get; set; }
        /// <summary>Tracker SKU to create or rename to (defaults to Woo SKU).</summary>
        public string CreateSku { get; set; }
        /// <summary>ItemsTbl.SortOrder when creating or updating the Tracker item.</summary>
        public int CreateSortOrder { get; set; } = 1;

        public bool ImportParentAsNotes
        {
            get { return string.Equals(ImportMode, ImportModeParentNotes, StringComparison.OrdinalIgnoreCase); }
        }

        public bool ImportParentAsItem
        {
            get { return string.Equals(ImportMode, ImportModeParentItem, StringComparison.OrdinalIgnoreCase); }
        }

        public bool ImportParentExcluded
        {
            get { return string.Equals(ImportMode, ImportModeExclude, StringComparison.OrdinalIgnoreCase); }
        }

        public bool ParentMapsAsDestination
        {
            get { return ImportParentAsNotes || ImportParentAsItem; }
        }

        /// <summary>Parent row can be saved (notes / parent SKU / exclude / clear leftover parent map) without expanding variants.</summary>
        public bool ParentUsesApply
        {
            get { return ParentMapsAsDestination || ImportParentExcluded || ExistingMappingID > 0; }
        }

        /// <summary>True when this row (or a child map for a Variants parent) is in WooItemMappingsTbl.</summary>
        public bool HasSavedMapping
        {
            get { return ExistingMappingID > 0 || ChildrenHaveSavedMapping; }
        }

        /// <summary>Variants parent: at least one variation has a saved map.</summary>
        public bool ChildrenHaveSavedMapping { get; set; }

        /// <summary>Fingerprint of last loaded/saved values; used to show dirty after edits.</summary>
        public string SavedSnapshot { get; set; }

        public bool IsDirtyVsSaved
        {
            get
            {
                if (!HasSavedMapping || string.IsNullOrEmpty(SavedSnapshot))
                    return false;
                return !string.Equals(SavedSnapshot, BuildSavedSnapshot(), StringComparison.Ordinal);
            }
        }

        /// <summary>✓ saved · ✎ saved but edited · — not saved.</summary>
        public string SavedFlagLabel
        {
            get
            {
                if (!HasSavedMapping)
                    return "—";
                return IsDirtyVsSaved ? "✎" : "✓";
            }
        }

        public string SavedFlagCss
        {
            get
            {
                if (!HasSavedMapping)
                    return "woo-map-saved-no";
                return IsDirtyVsSaved ? "woo-map-saved-dirty" : "woo-map-saved-yes";
            }
        }

        public string SavedFlagTitle
        {
            get
            {
                if (!HasSavedMapping)
                    return "Rule not applied yet — Save changes to store it";
                return IsDirtyVsSaved
                    ? "Rule was applied — you have unsaved edits"
                    : "Rule applied in Tracker";
            }
        }

        public void CaptureSavedSnapshot()
        {
            SavedSnapshot = BuildSavedSnapshot();
        }

        public string BuildSavedSnapshot()
        {
            return string.Join("|",
                ImportMode ?? string.Empty,
                MappedItemID.ToString(CultureInfo.InvariantCulture),
                MapToNotes ? "1" : "0",
                IncludeInImport ? "1" : "0",
                QtyFactor.ToString("0.####", CultureInfo.InvariantCulture),
                PackagingID.HasValue ? PackagingID.Value.ToString(CultureInfo.InvariantCulture) : "",
                (CreateSku ?? string.Empty).Trim(),
                CreateSortOrder.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>True when Save changes should write this row (dirty, or pending parent/variant map).</summary>
        public bool NeedsPersist
        {
            get
            {
                if (IsDirtyVsSaved)
                    return true;
                if (ImportModeUserSet)
                    return true;
                if (IsParentGroup)
                {
                    if (ExistingMappingID > 0)
                        return false;
                    return ImportParentExcluded || ImportParentAsNotes || ImportParentAsItem;
                }
                if (ImportParentAsNotes || ImportParentAsItem || ImportParentExcluded)
                    return false;
                if (ExistingMappingID > 0)
                    return false;
                return MapToNotes
                    || MappedItemID == DestinationNotesValue
                    || MappedItemID == DestinationCreateParent
                    || MappedItemID > 0
                    || SuggestedItemID > 0;
            }
        }

        public string ParentDestPlaceholder
        {
            get { return ImportParentExcluded ? "(do not import)" : "(variants)"; }
        }

        /// <summary>Import column: parent means the whole group; variants mean that line.</summary>
        public bool ShowIncludeInImport
        {
            get
            {
                if (!IsParentGroup)
                    return IncludeInImport;
                if (ImportParentExcluded)
                    return false;
                if (ParentMapsAsDestination)
                    return IncludeInImport;
                return true;
            }
        }

        /// <summary>Item SKU + sort order boxes: create/rename Tracker SKU, not the Woo catalog SKU.</summary>
        public bool ShowItemCreateFields
        {
            get
            {
                if (MapToNotes || ImportParentAsNotes || ImportParentExcluded)
                    return false;
                if (IsParentGroup)
                    return ImportParentAsItem || IsCreateParentDestination;
                if (ImportParentAsItem)
                    return false;
                return HasOwnSku || !string.IsNullOrWhiteSpace(CreateSku);
            }
        }

        /// <summary>True when the user typed Item SKU (do not reset from Woo on rebind).</summary>
        public bool CreateSkuUserSet { get; set; }

        /// <summary>Woo catalog SKU to use when Destination is Create Tracker item.</summary>
        public string WooCreateSkuDefault
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Sku))
                    return Sku.Trim();
                // Never use parent SKU for a variation Create — each variant needs its own Tracker SKU.
                if (IsVariation)
                    return string.Empty;
                if (!string.IsNullOrWhiteSpace(ParentSku))
                    return ParentSku.Trim();
                return string.Empty;
            }
        }

        public string DisplaySku
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Sku))
                    return Sku.Trim();
                if (!string.IsNullOrWhiteSpace(ParentSku))
                    return ParentSku.Trim();
                return IsParentGroup ? "(parent)" : string.Empty;
            }
        }

        /// <summary>
        /// SKU that owns the group. When the parent has no SKU, use a product-id key
        /// so variants still sort under that parent (not by the variant's own SKU).
        /// </summary>
        public string GroupSku
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ParentSku))
                    return ParentSku.Trim();
                if (!string.IsNullOrWhiteSpace(Sku) && (IsParentGroup || !IsVariation))
                    return Sku.Trim();
                return "#p" + WooProductId;
            }
        }

        /// <summary>Sort key shared by parent + variants so a blank parent SKU does not split the family.</summary>
        public string FamilySortKey
        {
            get
            {
                string sku = null;
                if (!string.IsNullOrWhiteSpace(ParentSku))
                    sku = ParentSku.Trim();
                else if (IsParentGroup && !string.IsNullOrWhiteSpace(Sku))
                    sku = Sku.Trim();

                if (!string.IsNullOrWhiteSpace(sku))
                    return "0|" + sku.ToUpperInvariant() + "|" + WooProductId.ToString("D12");

                return "1|" + WooProductId.ToString("D12");
            }
        }

        /// <summary>SKU shown in the grid: parent = group SKU; variant = own SKU only.</summary>
        public string ListSku
        {
            get
            {
                if (IsParentGroup)
                {
                    if (!string.IsNullOrWhiteSpace(Sku))
                        return Sku.Trim();
                    if (!string.IsNullOrWhiteSpace(ParentSku))
                        return ParentSku.Trim();
                    return "(no parent SKU)";
                }
                if (IsVariation)
                    return string.IsNullOrWhiteSpace(Sku) ? "—" : Sku.Trim();
                return DisplaySku;
            }
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Name))
                    return Name.Trim();
                if (!string.IsNullOrWhiteSpace(ParentName))
                    return ParentName.Trim();
                return string.Empty;
            }
        }

        /// <summary>Name shown in the grid: variants omit the repeated product title.</summary>
        public string ListName
        {
            get
            {
                if (IsVariation)
                    return string.Empty;
                return DisplayName;
            }
        }

        public bool HasOwnSku
        {
            get { return !string.IsNullOrWhiteSpace(Sku); }
        }

        public bool IsCreateParentDestination
        {
            get { return MappedItemID == DestinationCreateParent; }
        }

        public static string FormatInStockReason(int inStock, int total)
        {
            if (inStock < 0)
                inStock = 0;
            if (total < inStock)
                total = inStock;
            int oos = total - inStock;
            string stock = inStock == 1 ? "1 in stock" : (inStock.ToString() + " in stock");
            if (oos > 0)
                stock += " (" + oos + " out of stock)";
            return stock;
        }

        /// <summary>Text stored on Notes maps (parent + variant) for order import later.</summary>
        public string NotesDescription
        {
            get
            {
                string name = DisplayName;
                string attrs = AttributesLabel;
                if (string.IsNullOrWhiteSpace(attrs) || attrs == "—")
                    return name;
                if (string.IsNullOrWhiteSpace(name))
                    return attrs;
                return name + " (" + attrs + ")";
            }
        }

        /// <summary>True when Woo status is publish-like, not notes-bound, and no Tracker item suggested.</summary>
        public bool NeedsTrackerItem
        {
            get
            {
                if (MapToNotes || ImportParentAsNotes || SuggestedItemID > 0 || ExistingMappingID > 0)
                    return false;
                if (!IsWooEnabledStatus(Status))
                    return false;
                if (IsParentGroup)
                    return ImportParentAsItem;
                return !ImportParentAsItem;
            }
        }

        public string RowKey
        {
            get { return WooProductId + ":" + (WooVariationId ?? 0); }
        }

        public string SkuCssClass
        {
            get
            {
                if (IsParentGroup) return "woo-map-sku woo-map-sku-parent";
                if (IsVariation) return "woo-map-sku woo-map-sku-variant";
                return "woo-map-sku woo-map-sku-simple";
            }
        }

        public string NameCssClass
        {
            get { return "woo-map-name woo-map-depth-" + Depth; }
        }

        public static bool IsWooEnabledStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;
            return string.Equals(status.Trim(), "publish", StringComparison.OrdinalIgnoreCase);
        }
    }
}
