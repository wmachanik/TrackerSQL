using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    public partial class WooCommerceMapping : Page
    {
        private readonly WooCommerceMappingManager _manager = new WooCommerceMappingManager();
        private readonly WooCommerceAreaMappingManager _areaManager = new WooCommerceAreaMappingManager();
        private readonly WooCommerceOrderImportManager _orderImportManager = new WooCommerceOrderImportManager();
        private readonly WooCommerceSettingsManager _settings = new WooCommerceSettingsManager();
        private readonly ItemsRepository _itemsRepo = new ItemsRepository();
        private readonly ItemServiceTypesRepository _svcRepo = new ItemServiceTypesRepository();
        private readonly ItemPackagingsRepository _packRepo = new ItemPackagingsRepository();
        private readonly ItemSortOrdersRepository _sortOrdersRepo = new ItemSortOrdersRepository();
        private readonly AreasRepository _areasRepo = new AreasRepository();
        private readonly PersonsRepository _personsRepo = new PersonsRepository();
        private readonly OrderManager _orderManager = new OrderManager();

        private const string SessionPull = "WooMap.PullRows";
        private const string SessionPullFind = "WooMap.PullFind";
        private const string SessionPullNewOnly = "WooMap.PullNewOnly";
        private const string SessionSavedMapsFind = "WooMap.SavedMapsFind";
        private const string SessionSavedMapsSort = "WooMap.SavedMapsSort";
        private const string SessionSavedMapsSortDir = "WooMap.SavedMapsSortDir";
        private const string SessionPullDirty = "WooMap.PullDirty";
        private const string SessionMissingSku = "WooMap.MissingSkuRows";
        private const string SessionPullVer = "WooMap.PullVer";
        private const string SessionPendingSorts = "WooMap.PendingSorts";
        private const string SessionPendingImportModes = "WooMap.PendingImportModes";
        private const int PullRowsVersion = 29;
        private const string SessionAttrPull = "WooMap.AttrRows";
        private const string SessionPendingIncludes = "WooMap.PendingIncludes";
        private const string SessionEnabledPush = "WooMap.EnabledPushRows";
        private const string VsTab = "WooMap.Tab";
        private const string ItemLookupSort = "ItemEnabled DESC, SortOrder, ItemDesc";

        private Dictionary<int, bool> _dbIncludes;
        private Dictionary<int, int?> _dbSorts;
        private Dictionary<int, string> _dbImportModes;
        private Dictionary<int, bool> _dbAttrUseForVariants;
        private Dictionary<int, int> _dbAttrQtyRanks;
        private Dictionary<int, int> _dbAttrPackRanks;
        private Dictionary<int, int> _dbAttrNoteRanks;
        private List<ListItem> _itemDropdownTemplate;
        private List<ListItem> _personDropdownTemplate;
        private readonly Dictionary<int, List<ItemPackaging>> _packagingsByServiceType = new Dictionary<int, List<ItemPackaging>>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserCanManage())
            {
                pnlMain.Visible = false;
                pnlWooDisabled.Visible = false;
                pnlAccessDenied.Visible = true;
                lblAccessDenied.Text = MessageProvider.Get(MessageKeys.SystemPreferences.AccessDenied);
                return;
            }

            if (!_settings.IsIntegrationEnabled())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = false;
                pnlWooDisabled.Visible = true;
                litWooDisabled.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapNeedWooEnabled);
                btnStartWooWizard.Text = MessageProvider.Get(MessageKeys.WooCommerce.StartWizard);
                return;
            }

            pnlWooDisabled.Visible = false;
            _settings.EnsureSchemaOnce();

            if (!IsPostBack)
            {
                ClearPendingIncludes();
                BindLabels();
                if (!TryShowInitialTabFromQuery())
                {
                    ShowTab(0);
                    BindCategories();
                    LoadCatMode();
                }
                SyncAttrVariantTabAvailability();
            }
            else
            {
                ApplyTabHighlight(GetTab());
                SyncAttrVariantTabAvailability();
            }
        }

        protected void btnStartWooWizard_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Tools/SystemPreferences.aspx?section=woo&wizard=1");
        }

        private bool UserCanManage()
        {
            var user = Context?.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated &&
                   (user.IsInRole("Administrators") || user.IsInRole("Admin"));
        }

        private void BindLabels()
        {
            litTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapPageTitle);
            litSubtitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapPageSubtitle);
            btnTabCat.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabCategories);
            btnTabAttrParents.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabAttrParents);
            btnTabAttrVariants.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabAttrVariants);
            btnTabMap.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabMappings);
            btnTabAreas.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabAreas);
            btnTabShipping.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabShipping);
            btnTabPayment.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabPayment);
            btnTabSavedMaps.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabSavedMaps);
            btnTabMissingSku.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabMissingSku);
            btnTabSync.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabSync);
            litCatHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapCatHelp);
            litAttrParentsHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapAttrParentsHelp);
            litAttrOptionsHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapAttrOptionsHelp);
            litAttrVariantsLocked.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapAttrVariantsLocked);
            litMapHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapMapHelp);
            litAreasHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapAreasHelp);
            litAreaSectionTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapAreaSectionTitle);
            lblDefaultImportArea.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapDefaultImportAreaLbl);
            btnSaveDefaultArea.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveDefaultArea);
            lblImportNotesItem.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapImportNotesItemLbl);
            btnSaveImportNotesItem.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveImportNotesItem);
            litAddressConfigTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapAddressConfigTitle);
            litAddressConfigNote.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapAddressConfigNote);
            btnSaveAddressConfig.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveAddressConfig);
            litAreaDefaultsNote.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapAreaDefaultsNote);
            btnSaveAreaDefaults.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveAreaDefaults);
            litShippingMapsNote.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapShippingMapsNote);
            litShippingHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapShippingHelp);
            litShippingEmptyHint.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapShippingEmptyHint);
            btnSaveShippingMaps.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveShippingMaps);
            litDispatchWaybillTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapDispatchWaybillTitle);
            litDispatchWaybillNote.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapDispatchWaybillNote);
            lblDispatchPeople.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapDispatchPeopleLbl);
            btnSaveDispatchWaybill.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveDispatchWaybill);
            chkTrackingNumberRequired.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelTrackingRequired);
            litPaymentMapsNote.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapPaymentMapsNote);
            btnSavePaymentMaps.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSavePaymentMaps);
            litPaymentHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapPaymentHelp);
            lblTestPostal.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTestPostalLbl);
            lblTestSuburb.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTestSuburbLbl);
            btnTestResolve.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTestResolveBtn);
            litSavedMapsHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSavedMapsHelp);
            lblFindSavedMap.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapFindSku);
            btnFindSavedMap.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapFindSkuBtn);
            btnClearFindSavedMap.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapClearFindSku);
            litMissingSkuHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapMissingSkuHelp);
            lblFindSku.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapFindSku);
            btnFindSku.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapFindSkuBtn);
            btnClearFindSku.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapClearFindSku);
            chkNewSinceSync.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapNewSinceSync);
            btnSaveSelectedMaps.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveSelected);
            btnSaveSelectedMaps.ToolTip = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveSelectedTip);
            btnSaveSelectedMapsBottom.Text = btnSaveSelectedMaps.Text;
            btnSaveSelectedMapsBottom.ToolTip = btnSaveSelectedMaps.ToolTip;
            btnResetCatalog.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapResetCatalog);
            btnExpandAllGroups.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapExpandAll);
            btnCollapseAllGroups.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapCollapseAll);
            btnWriteMissingSkus.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapWriteMissingSkus);
            litSyncHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSyncHelp);
            litCatModeLbl.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelCategoryMode) + " ";
            btnSaveCatIncludes.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveIncludes);
            btnSaveAttrParents.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveAttrParents);
            btnSaveAttrMaps.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveAttributeMaps);
            btnDryPush.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapDryPush);
            btnPushEnabled.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapPushEnabled);
            hdnUnsavedLeave.Value = MessageProvider.Get(MessageKeys.WooCommerce.MapUnsavedLeave);
            btnBack.AlternateText = MessageProvider.Get(MessageKeys.WooCommerce.ButtonBack);
            btnBack.ToolTip = MessageProvider.Get(MessageKeys.WooCommerce.MapBackToPreferences);
            BindActionButtonTexts();
        }

        /// <summary>
        /// First run = Pull…; after data exists locally = Sync… (same action).
        /// </summary>
        private void BindActionButtonTexts()
        {
            bool catsSynced = (_manager.GetCategoryFilters() ?? new List<WooCategoryFilter>()).Count > 0;
            bool attrParentsSynced = (_manager.GetAttributeParents() ?? new List<WooAttributeParent>()).Count > 0;
            bool attrOptionsSynced = (_manager.GetAttributeMaps() ?? new List<WooAttributeMap>()).Count > 0
                || Session[SessionAttrPull] != null;
            bool productsSynced = _manager.HasCachedCatalog()
                || _settings.GetSettings().LastItemsSyncUtc.HasValue;

            btnPullCats.Text = MessageProvider.Get(catsSynced
                ? MessageKeys.WooCommerce.MapSyncCategories
                : MessageKeys.WooCommerce.MapPullCategories);
            btnPullAttrParents.Text = MessageProvider.Get(attrParentsSynced
                ? MessageKeys.WooCommerce.MapSyncAttrParents
                : MessageKeys.WooCommerce.MapPullAttrParents);
            btnPullAttributes.Text = MessageProvider.Get(attrOptionsSynced
                ? MessageKeys.WooCommerce.MapSyncAttributes
                : MessageKeys.WooCommerce.MapPullAttributes);
            btnPullProducts.Text = MessageProvider.Get(productsSynced
                ? MessageKeys.WooCommerce.MapSyncProducts
                : MessageKeys.WooCommerce.MapPullProducts);

            hdnPullCatsConfirm.Value = MessageProvider.Get(catsSynced
                ? MessageKeys.WooCommerce.MapSyncCatsConfirm
                : MessageKeys.WooCommerce.MapPullCatsConfirm);

            btnPullCats.Attributes["data-woo-op"] = "cats";
            btnPullAttrParents.Attributes["data-woo-op"] = "attrParents";
            btnPullAttributes.Attributes["data-woo-op"] = "attrOptions";
            btnPullProducts.Attributes["data-woo-op"] = "products";
            btnDryPush.Attributes["data-woo-op"] = "dryPush";
            btnPushEnabled.Attributes["data-woo-op"] = "push";
        }

        private void BindItemDropdowns()
        {
            // Display Tracker SKU (_ if disabled). Sort like item lookups: enabled, SortOrder, name.
            var items = (_itemsRepo.GetAll(ItemLookupSort) ?? new List<Item>())
                .OrderByDescending(i => i.ItemEnabled != false)
                .ThenBy(i => i.SortOrder ?? int.MaxValue)
                .ThenBy(i => i.ItemDesc ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
            ddlItemLookup.Items.Clear();
            ddlItemLookup.Items.Add(new ListItem(MessageProvider.Get(MessageKeys.WooCommerce.MapDestNotMapped), "0"));
            ddlItemLookup.Items.Add(new ListItem(MessageProvider.Get(MessageKeys.WooCommerce.MapDestCreateParent),
                WooProductMapRow.DestinationCreateParent.ToString()));
            ddlItemLookup.Items.Add(new ListItem(MessageProvider.Get(MessageKeys.WooCommerce.MapDestNotes),
                WooProductMapRow.DestinationNotesValue.ToString()));
            foreach (var i in items)
            {
                string sku = (i.SKU ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(sku))
                    sku = "-";
                string label = sku;
                string desc = (i.ItemDesc ?? string.Empty).Trim();
                if (desc.Length > 0 && !string.Equals(desc, sku, StringComparison.OrdinalIgnoreCase))
                    label = sku + " - " + desc;
                ddlItemLookup.Items.Add(new ListItem(
                    LookupFormatter.FormatLookupText(label, i.ItemEnabled),
                    i.ItemID.ToString()));
            }
            _itemDropdownTemplate = null;
        }

        /// <summary>Only load the full Tracker item list when the Mappings grid needs it.</summary>
        private void EnsureItemDropdowns()
        {
            if (ddlItemLookup.Items.Count > 0)
                return;
            BindItemDropdowns();
        }

        private List<ListItem> GetItemDropdownTemplate()
        {
            EnsureItemDropdowns();
            if (_itemDropdownTemplate == null)
            {
                _itemDropdownTemplate = ddlItemLookup.Items.Cast<ListItem>()
                    .Select(li => new ListItem(li.Text, li.Value))
                    .ToList();
            }
            return _itemDropdownTemplate;
        }

        private int GetTab()
        {
            object v = ViewState[VsTab];
            return v == null ? 0 : (int)v;
        }

        private void ShowTab(int index)
        {
            ViewState[VsTab] = index;
            mvTabs.ActiveViewIndex = index;
            ApplyTabHighlight(index);
            SyncAttrVariantTabAvailability();
        }

        /// <summary>Deep-link e.g. ?tab=payment from order import.</summary>
        private bool TryShowInitialTabFromQuery()
        {
            string tab = Request.QueryString["tab"];
            if (string.IsNullOrWhiteSpace(tab))
                return false;

            switch (tab.Trim().ToLowerInvariant())
            {
                case "payment":
                    ShowTab(6);
                    BindPaymentTab();
                    return true;
                case "shipping":
                    ShowTab(5);
                    BindShippingTab();
                    return true;
                case "areas":
                    ShowTab(4);
                    BindAreasTab();
                    return true;
                case "mappings":
                case "map":
                    ShowTab(3);
                    BindImportNotesItemDropdown();
                    EnsurePullRowsLoaded();
                    RebindPull();
                    SyncSaveSelectedButton();
                    return true;
                default:
                    return false;
            }
        }

        private void ApplyTabHighlight(int index)
        {
            btnTabCat.CssClass = index == 0 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabAttrParents.CssClass = index == 1 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabAttrVariants.CssClass = index == 2 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabMap.CssClass = index == 3 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabAreas.CssClass = index == 4 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabShipping.CssClass = index == 5 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabPayment.CssClass = index == 6 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabSavedMaps.CssClass = index == 7 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabMissingSku.CssClass = index == 8 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabSync.CssClass = index == 9 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
        }

        protected void btnTabCat_Click(object sender, EventArgs e) { PersistActiveTabEdits(); ShowTab(0); BindCategories(); LoadCatMode(); }
        protected void btnTabAttrParents_Click(object sender, EventArgs e) { PersistActiveTabEdits(); ShowTab(1); BindAttrParents(); }
        protected void btnTabAttrVariants_Click(object sender, EventArgs e) { PersistActiveTabEdits(); ShowTab(2); SyncAttrVariantsPanels(); RebindAttributes(); }
        protected void btnTabMap_Click(object sender, EventArgs e)
        {
            PersistActiveTabEdits();
            ShowTab(3);
            BindImportNotesItemDropdown();
            EnsurePullRowsLoaded();
            RebindPull();
            SyncSaveSelectedButton();
        }
        protected void btnTabAreas_Click(object sender, EventArgs e)
        {
            PersistActiveTabEdits();
            ShowTab(4);
            BindAreasTab();
        }
        protected void btnTabShipping_Click(object sender, EventArgs e)
        {
            PersistActiveTabEdits();
            ShowTab(5);
            BindShippingTab();
        }
        protected void btnTabPayment_Click(object sender, EventArgs e)
        {
            PersistActiveTabEdits();
            ShowTab(6);
            BindPaymentTab();
        }
        protected void btnTabSavedMaps_Click(object sender, EventArgs e)
        {
            PersistActiveTabEdits();
            ShowTab(7);
            BindExistingMaps();
        }
        protected void btnTabMissingSku_Click(object sender, EventArgs e)
        {
            PersistActiveTabEdits();
            ShowTab(8);
            EnsurePullRowsLoaded();
            RebindMissingSku();
            SyncWriteMissingButton();
        }
        protected void btnTabSync_Click(object sender, EventArgs e)
        {
            PersistActiveTabEdits();
            ShowTab(9);
            BindEnabledPushGrid();
        }

        private void PersistActiveTabEdits()
        {
            if (hdnDiscardLeave != null && hdnDiscardLeave.Value == "1")
            {
                hdnDiscardLeave.Value = "0";
                ClearPendingIncludes();
                ClearUnsavedFlag(hdnUnsavedCat);
                ClearUnsavedFlag(hdnUnsavedAttrParents);
                ClearUnsavedFlag(hdnUnsavedAttrMaps);
                ClearUnsavedFlag(hdnUnsavedPull);
                ClearUnsavedFlag(hdnUnsavedMissing);
                return;
            }

            int tab = GetTab();
            if (tab == 0)
                MergeVisibleIncludesToPending();
            else if (tab == 2)
                MergeAttrGridEditsIntoSession();
            else if (tab == 3)
                MergeVisiblePullEdits();
            else if (tab == 7)
                MergeVisibleMissingSkuEdits();
        }

        private static void ClearUnsavedFlag(HiddenField hdn)
        {
            if (hdn != null)
                hdn.Value = "0";
        }

        protected void gvCategories_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvCategories, e.Row);
        }

        protected void gvAttr_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvAttr, e.Row);
        }

        protected void gvPull_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvPull, e.Row);
            if (e.Row.RowType != DataControlRowType.Header)
                return;
            ApplyPullHeaderToolTips(e.Row);
        }

        private static void ApplyPullHeaderToolTips(GridViewRow headerRow)
        {
            if (headerRow == null)
                return;
            SetHeaderTip(headerRow, "Svd", MessageKeys.WooCommerce.MapColAppliedTip);
            SetHeaderTip(headerRow, "Impt", MessageKeys.WooCommerce.MapColImportTip);
            SetHeaderTip(headerRow, "Woo SKU", MessageKeys.WooCommerce.MapColSkuTip);
            SetHeaderTip(headerRow, "Variant", MessageKeys.WooCommerce.MapColVariantTip);
            SetHeaderTip(headerRow, "Pack cascade / notes", MessageKeys.WooCommerce.MapColNotesAttrTip);
            SetHeaderTip(headerRow, "Mode", MessageKeys.WooCommerce.MapColModeTip);
            SetHeaderTip(headerRow, "Match", MessageKeys.WooCommerce.MapColMatchTip);
            SetHeaderTip(headerRow, "Tracker SKU", MessageKeys.WooCommerce.MapColDestinationTip);
            SetHeaderTip(headerRow, "Item SKU", MessageKeys.WooCommerce.MapColItemSkuTip);
            SetHeaderTip(headerRow, "S/O", MessageKeys.WooCommerce.MapColSortTip);
            SetHeaderTip(headerRow, "Qty", MessageKeys.WooCommerce.MapColQtyTip);
            SetHeaderTip(headerRow, "Pack", MessageKeys.WooCommerce.MapColPackTip);
        }

        private static void SetHeaderTip(GridViewRow headerRow, string headerText, string messageKey)
        {
            string tip = MessageProvider.Get(messageKey);
            if (string.IsNullOrWhiteSpace(tip) || headerRow == null)
                return;
            foreach (TableCell cell in headerRow.Cells)
            {
                var fieldCell = cell as DataControlFieldCell;
                string fieldHeader = fieldCell != null && fieldCell.ContainingField != null
                    ? fieldCell.ContainingField.HeaderText
                    : null;
                if (!string.Equals(fieldHeader, headerText, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(cell.Text, headerText, StringComparison.OrdinalIgnoreCase))
                    continue;
                cell.ToolTip = tip;
                return;
            }
        }

        protected void gvMissingSku_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvMissingSku, e.Row);
        }

        private void LoadCatMode()
        {
            var s = _settings.GetSettings();
            string mode = s.CategoryFilterMode ?? "All";
            if (string.Equals(mode, "ExcludeList", StringComparison.OrdinalIgnoreCase))
                mode = "IncludeList";
            var item = ddlCatMode.Items.FindByValue(mode);
            if (item != null)
                ddlCatMode.SelectedValue = item.Value;
            ddlCatMode.Attributes["data-original"] = ddlCatMode.SelectedValue;
            SyncSaveIncludesButton();
        }

        private Dictionary<int, bool> GetPendingIncludes()
        {
            var pending = Session[SessionPendingIncludes] as Dictionary<int, bool>;
            if (pending == null)
            {
                pending = new Dictionary<int, bool>();
                Session[SessionPendingIncludes] = pending;
            }
            return pending;
        }

        private void ClearPendingIncludes()
        {
            Session[SessionPendingIncludes] = new Dictionary<int, bool>();
            Session[SessionPendingSorts] = new Dictionary<int, int?>();
            Session[SessionPendingImportModes] = new Dictionary<int, string>();
        }

        private Dictionary<int, int?> GetPendingSorts()
        {
            var pending = Session[SessionPendingSorts] as Dictionary<int, int?>;
            if (pending == null)
            {
                pending = new Dictionary<int, int?>();
                Session[SessionPendingSorts] = pending;
            }
            return pending;
        }

        private Dictionary<int, string> GetPendingImportModes()
        {
            var pending = Session[SessionPendingImportModes] as Dictionary<int, string>;
            if (pending == null)
            {
                pending = new Dictionary<int, string>();
                Session[SessionPendingImportModes] = pending;
            }
            return pending;
        }

        private static int? NormalizeSortValue(int? value)
        {
            if (!value.HasValue || value.Value <= 0)
                return null;
            return value;
        }

        private static string NormalizeImportModeValue(string value)
        {
            return WooCategoryFilterRepository.NormalizeImportModeOrNull(value) ?? string.Empty;
        }

        private void MergeVisibleIncludesToPending()
        {
            var dbList = _manager.GetCategoryFilters() ?? new List<WooCategoryFilter>();
            var dbInclude = dbList.ToDictionary(x => x.FilterID, x => x.IncludeInSync);
            var dbSort = dbList.ToDictionary(x => x.FilterID, x => x.DefaultSortValue);
            var dbMode = dbList.ToDictionary(x => x.FilterID, x => NormalizeImportModeValue(x.DefaultImportMode));
            var pendingInc = GetPendingIncludes();
            var pendingSort = GetPendingSorts();
            var pendingMode = GetPendingImportModes();
            bool remapDefaults = false;

            foreach (GridViewRow row in gvCategories.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int filterId = Convert.ToInt32(gvCategories.DataKeys[row.RowIndex].Value);
                var chk = row.FindControl("chkInclude") as CheckBox;
                var ddlSort = row.FindControl("ddlCatSort") as DropDownList;
                var ddlMode = row.FindControl("ddlCatImportMode") as DropDownList;

                bool dbVal;
                if (!dbInclude.TryGetValue(filterId, out dbVal))
                    dbVal = true;
                if (chk != null)
                {
                    if (chk.Checked != dbVal)
                        pendingInc[filterId] = chk.Checked;
                    else
                        pendingInc.Remove(filterId);
                }

                int? dbSo;
                if (!dbSort.TryGetValue(filterId, out dbSo))
                    dbSo = null;
                if (ddlSort != null)
                {
                    int parsed;
                    int? uiSo = int.TryParse(ddlSort.SelectedValue, out parsed) && parsed > 0
                        ? (int?)parsed
                        : null;
                    bool sortChanged = !Nullable.Equals(NormalizeSortValue(uiSo), NormalizeSortValue(dbSo));
                    if (sortChanged)
                    {
                        pendingSort[filterId] = uiSo;
                        remapDefaults = true;
                    }
                    else
                        pendingSort.Remove(filterId);
                }

                string dbImport;
                if (!dbMode.TryGetValue(filterId, out dbImport))
                    dbImport = string.Empty;
                if (ddlMode != null)
                {
                    string uiMode = NormalizeImportModeValue(ddlMode.SelectedValue);
                    bool modeChanged = !string.Equals(uiMode, dbImport, StringComparison.OrdinalIgnoreCase);
                    if (modeChanged)
                    {
                        pendingMode[filterId] = uiMode;
                        remapDefaults = true;
                    }
                    else
                        pendingMode.Remove(filterId);
                }
            }

            // Import / S/O defaults feed the Mappings grid — refresh only untouched products.
            if (remapDefaults)
                RebuildPullRowsPreservingUserModes();
        }

        /// <summary>DB category filters with unsaved Include / S/O / Import overlays (for mapping defaults).</summary>
        private List<WooCategoryFilter> GetCategoryFiltersForMapping()
        {
            var list = _manager.GetCategoryFilters() ?? new List<WooCategoryFilter>();
            var pendingInc = GetPendingIncludes();
            var pendingSort = GetPendingSorts();
            var pendingMode = GetPendingImportModes();
            foreach (var c in list)
            {
                bool overrideInclude;
                if (pendingInc.TryGetValue(c.FilterID, out overrideInclude))
                    c.IncludeInSync = overrideInclude;
                int? overrideSort;
                if (pendingSort.TryGetValue(c.FilterID, out overrideSort))
                    c.DefaultSortValue = overrideSort;
                string overrideMode;
                if (pendingMode.TryGetValue(c.FilterID, out overrideMode))
                    c.DefaultImportMode = string.IsNullOrEmpty(overrideMode) ? null : overrideMode;
            }
            return list;
        }

        /// <summary>
        /// Rebuild mapping rows from cache + category defaults, but keep Mode on parents the user
        /// already edited (or that already have a saved mapping in session edits).
        /// </summary>
        private void RebuildPullRowsPreservingUserModes()
        {
            var previous = Session[SessionPull] as List<WooProductMapRow>;
            try
            {
                var result = _manager.LoadCachedProductsForMapping(GetCategoryFiltersForMapping());
                var neu = result.MappingRows ?? new List<WooProductMapRow>();
                PreserveUserImportModes(previous, neu);
                Session[SessionPull] = neu;
                Session[SessionMissingSku] = result.MissingSkuRows ?? new List<WooProductMapRow>();
                Session[SessionPullVer] = PullRowsVersion;
            }
            catch (Exception ex)
            {
                if (previous == null)
                {
                    Session[SessionPull] = new List<WooProductMapRow>();
                    Session[SessionMissingSku] = new List<WooProductMapRow>();
                    Session[SessionPullVer] = PullRowsVersion;
                }
                SetStatus(ex.Message, true);
            }
        }

        private static void PreserveUserImportModes(List<WooProductMapRow> previous, List<WooProductMapRow> neu)
        {
            if (previous == null || previous.Count == 0 || neu == null || neu.Count == 0)
                return;

            var prevByKey = previous.Where(r => r != null && !string.IsNullOrEmpty(r.RowKey))
                .GroupBy(r => r.RowKey, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var prevParents = previous.Where(r => r != null && r.IsParentGroup)
                .GroupBy(r => r.WooProductId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var parent in neu.Where(r => r != null && r.IsParentGroup))
            {
                WooProductMapRow prev;
                if (!prevParents.TryGetValue(parent.WooProductId, out prev))
                    continue;

                parent.GroupExpanded = prev.GroupExpanded;

                // Saved maps are already applied from DB on rebuild. Keep unsaved user Mode edits.
                if (!prev.ImportModeUserSet)
                    continue;

                parent.ImportMode = prev.ImportMode;
                parent.ImportModeUserSet = true;
                parent.UsesCategoryImportDefault = false;
                parent.MappedItemID = prev.MappedItemID;
                parent.MapToNotes = prev.MapToNotes;
                parent.ApplySelected = prev.ApplySelected;
                parent.IncludeInImport = prev.IncludeInImport;
                parent.MatchReason = prev.MatchReason;
                parent.SuggestedItemID = prev.SuggestedItemID;
                parent.SuggestedItemDesc = prev.SuggestedItemDesc;
                parent.CreateSku = prev.CreateSku;
                parent.CreateSkuUserSet = prev.CreateSkuUserSet;
                parent.CreateSortOrder = prev.CreateSortOrder;
                parent.QtyFactor = prev.QtyFactor;
                parent.PackagingID = prev.PackagingID;
                parent.ExistingMappingID = prev.ExistingMappingID;
                parent.ChildrenHaveSavedMapping = prev.ChildrenHaveSavedMapping;
                parent.SavedSnapshot = prev.SavedSnapshot;
            }

            // Keep unsaved variant Item SKU / S/O / Qty / Pack / destination across rebuild.
            foreach (var row in neu.Where(r => r != null && !r.IsParentGroup))
            {
                WooProductMapRow prev;
                if (!prevByKey.TryGetValue(row.RowKey, out prev))
                    continue;
                if (!prev.CreateSkuUserSet && !prev.IsDirtyVsSaved && !prev.ImportModeUserSet)
                    continue;
                if (prev.CreateSkuUserSet || prev.IsDirtyVsSaved)
                {
                    row.CreateSku = prev.CreateSku;
                    row.CreateSkuUserSet = prev.CreateSkuUserSet;
                    row.CreateSortOrder = prev.CreateSortOrder;
                    row.QtyFactor = prev.QtyFactor;
                    row.PackagingID = prev.PackagingID;
                    row.MappedItemID = prev.MappedItemID;
                    row.MapToNotes = prev.MapToNotes;
                    row.IncludeInImport = prev.IncludeInImport;
                    row.ApplySelected = prev.ApplySelected;
                    if (!string.IsNullOrEmpty(prev.SavedSnapshot))
                        row.SavedSnapshot = prev.SavedSnapshot;
                }
            }

            WooCommerceMappingManager.ApplyParentImportModes(neu);
            // Refresh clean snapshots after structural sync; keep stale snapshot only for real user Mode edits.
            foreach (var r in neu.Where(x => x != null && x.HasSavedMapping))
            {
                if (!r.ImportModeUserSet || string.IsNullOrEmpty(r.SavedSnapshot))
                    r.CaptureSavedSnapshot();
            }
        }

        private void SyncSaveIncludesButton()
        {
            int pending = GetPendingIncludes().Count + GetPendingSorts().Count + GetPendingImportModes().Count;
            hdnPendingCount.Value = pending.ToString();
        }

        private void BindCategories()
        {
            var list = _manager.GetCategoryFilters() ?? new List<WooCategoryFilter>();
            _dbIncludes = list.ToDictionary(x => x.FilterID, x => x.IncludeInSync);
            _dbSorts = list.ToDictionary(x => x.FilterID, x => x.DefaultSortValue);
            _dbImportModes = list.ToDictionary(x => x.FilterID, x => NormalizeImportModeValue(x.DefaultImportMode));

            var pendingInc = GetPendingIncludes();
            var pendingSort = GetPendingSorts();
            var pendingMode = GetPendingImportModes();
            foreach (var c in list)
            {
                bool overrideInclude;
                if (pendingInc.TryGetValue(c.FilterID, out overrideInclude))
                    c.IncludeInSync = overrideInclude;
                int? overrideSort;
                if (pendingSort.TryGetValue(c.FilterID, out overrideSort))
                    c.DefaultSortValue = overrideSort;
                string overrideMode;
                if (pendingMode.TryGetValue(c.FilterID, out overrideMode))
                    c.DefaultImportMode = string.IsNullOrEmpty(overrideMode) ? null : overrideMode;
            }

            if (gvCategories.PageIndex > 0)
            {
                int pageCount = list.Count == 0 ? 1 : (int)Math.Ceiling(list.Count / (double)gvCategories.PageSize);
                if (gvCategories.PageIndex >= pageCount)
                    gvCategories.PageIndex = Math.Max(0, pageCount - 1);
            }

            gvCategories.DataSource = list;
            gvCategories.DataBind();
            SyncSaveIncludesButton();

            if (list.Count == 0)
            {
                litCatPageInfo.Text = string.Empty;
                return;
            }

            int page = gvCategories.PageIndex + 1;
            int pages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)gvCategories.PageSize));
            litCatPageInfo.Text = "<p class=\"woo-map-page-info\">"
                + Server.HtmlEncode(MessageProvider.Format(MessageKeys.WooCommerce.MapCatPageInfo, list.Count, page, pages))
                + "</p>";
        }

        protected void gvCategories_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            MergeVisibleIncludesToPending();
            gvCategories.PageIndex = e.NewPageIndex;
            BindCategories();
        }

        protected void gvCategories_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var row = e.Row.DataItem as WooCategoryFilter;
            if (row == null)
                return;

            var chk = e.Row.FindControl("chkInclude") as CheckBox;
            if (chk != null)
            {
                chk.InputAttributes["data-original"] = chk.Checked ? "1" : "0";
            }

            var ddlSort = e.Row.FindControl("ddlCatSort") as DropDownList;
            if (ddlSort != null)
            {
                int? selected = row.DefaultSortValue;
                int? pending;
                if (GetPendingSorts().TryGetValue(row.FilterID, out pending))
                    selected = pending;
                _sortOrdersRepo.FillDropDown(ddlSort, selected, includeBlank: true);
                ddlSort.Attributes["data-original"] = ddlSort.SelectedValue ?? string.Empty;
            }

            var ddlImport = e.Row.FindControl("ddlCatImportMode") as DropDownList;
            if (ddlImport != null)
            {
                string selectedMode = NormalizeImportModeValue(row.DefaultImportMode);
                string pendingMode;
                if (GetPendingImportModes().TryGetValue(row.FilterID, out pendingMode))
                    selectedMode = NormalizeImportModeValue(pendingMode);
                FillCatImportModeDropDown(ddlImport, selectedMode);
                ddlImport.Attributes["data-original"] = ddlImport.SelectedValue ?? string.Empty;
            }
        }

        private static void FillCatImportModeDropDown(DropDownList ddl, string selectedMode)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeInherit), string.Empty));
            ddl.Items.Add(new ListItem(
                MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeVariants),
                WooProductMapRow.ImportModeVariants));
            ddl.Items.Add(new ListItem(
                MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeParentItem),
                WooProductMapRow.ImportModeParentItem));
            ddl.Items.Add(new ListItem(
                MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeParentNotes),
                WooProductMapRow.ImportModeParentNotes));
            ddl.Items.Add(new ListItem(
                MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeExclude),
                WooProductMapRow.ImportModeExclude));
            string mode = selectedMode ?? string.Empty;
            if (ddl.Items.FindByValue(mode) != null)
                ddl.SelectedValue = mode;
            else
                ddl.SelectedIndex = 0;
        }

        protected void btnPullCats_Click(object sender, EventArgs e)
        {
            ClearPendingIncludes();
            var result = _manager.PullCategories(UserName());
            SetStatus(result.Message, !result.Succeeded);
            gvCategories.PageIndex = 0;
            BindCategories();
            BindActionButtonTexts();
        }

        protected void btnSaveCatIncludes_Click(object sender, EventArgs e)
        {
            MergeVisibleIncludesToPending();
            var pendingInc = GetPendingIncludes();
            var pendingSort = GetPendingSorts();
            var pendingMode = GetPendingImportModes();
            var dbList = _manager.GetCategoryFilters() ?? new List<WooCategoryFilter>();
            var ids = new HashSet<int>(pendingInc.Keys);
            foreach (int id in pendingSort.Keys)
                ids.Add(id);
            foreach (int id in pendingMode.Keys)
                ids.Add(id);

            int saved = 0;
            foreach (int filterId in ids)
            {
                var dbRow = dbList.Find(c => c.FilterID == filterId);
                bool include = dbRow != null && dbRow.IncludeInSync;
                int? sort = dbRow != null ? dbRow.DefaultSortValue : null;
                string importMode = dbRow != null ? NormalizeImportModeValue(dbRow.DefaultImportMode) : string.Empty;
                bool overrideInc;
                if (pendingInc.TryGetValue(filterId, out overrideInc))
                    include = overrideInc;
                int? overrideSort;
                if (pendingSort.TryGetValue(filterId, out overrideSort))
                    sort = overrideSort;
                string overrideMode;
                if (pendingMode.TryGetValue(filterId, out overrideMode))
                    importMode = NormalizeImportModeValue(overrideMode);
                _manager.SaveCategoryRow(filterId, include, sort, UserName(),
                    string.IsNullOrEmpty(importMode) ? null : importMode);
                saved++;
            }
            ClearPendingIncludes();

            string original = ddlCatMode.Attributes["data-original"];
            if (string.IsNullOrEmpty(original) || ddlCatMode.SelectedValue != original)
            {
                _manager.SaveCategoryMode(ddlCatMode.SelectedValue, UserName());
            }

            BindCategories();
            LoadCatMode();
            ClearUnsavedFlag(hdnUnsavedCat);
            RebuildPullRowsPreservingUserModes();
            SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapIncludesSaved, saved), false);
        }

        protected void btnPullAttrParents_Click(object sender, EventArgs e)
        {
            var result = _manager.PullAttributeParents(UserName());
            BindAttrParents();
            SyncAttrVariantTabAvailability();
            BindActionButtonTexts();
            SetStatus(result.Message, !result.Succeeded);
        }

        protected void btnSaveAttrParents_Click(object sender, EventArgs e)
        {
            var selections = new List<WooAttributeParent>();
            foreach (GridViewRow row in gvAttrParents.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int parentId = Convert.ToInt32(gvAttrParents.DataKeys[row.RowIndex].Value);
                var chk = (CheckBox)row.FindControl("chkUseForVariants");
                var ddlQty = (DropDownList)row.FindControl("ddlQtyRank");
                var ddlPack = (DropDownList)row.FindControl("ddlPackRank");
                var ddlNote = (DropDownList)row.FindControl("ddlNoteRank");
                selections.Add(new WooAttributeParent
                {
                    ParentID = parentId,
                    UseForVariants = chk != null && chk.Checked,
                    QtyRank = WooAttributeParent.ParseRank(ddlQty != null ? ddlQty.SelectedValue : null),
                    PackRank = WooAttributeParent.ParseRank(ddlPack != null ? ddlPack.SelectedValue : null),
                    NoteRank = WooAttributeParent.ParseRank(ddlNote != null ? ddlNote.SelectedValue : null)
                });
            }
            int saved = _manager.SaveAttributeParentVariants(selections, UserName());
            BindAttrParents();
            SyncAttrVariantTabAvailability();
            ClearUnsavedFlag(hdnUnsavedAttrParents);
            SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapAttrParentsSaved, saved), false);
        }

        private void BindAttrParents()
        {
            var list = _manager.GetAttributeParents() ?? new List<WooAttributeParent>();
            _dbAttrUseForVariants = list.ToDictionary(x => x.ParentID, x => x.UseForVariants);
            _dbAttrQtyRanks = list.ToDictionary(x => x.ParentID, x => x.QtyRank);
            _dbAttrPackRanks = list.ToDictionary(x => x.ParentID, x => x.PackRank);
            _dbAttrNoteRanks = list.ToDictionary(x => x.ParentID, x => x.NoteRank);
            gvAttrParents.DataSource = list;
            gvAttrParents.DataBind();
            SyncSaveAttrParentsButton(dirty: false);
            SyncAttrVariantTabAvailability();
        }

        private void SyncSaveAttrParentsButton(bool dirty)
        {
        }

        private bool HasPulledAttributeParents()
        {
            var list = _manager.GetAttributeParents();
            return list != null && list.Count > 0;
        }

        private void SyncAttrVariantTabAvailability()
        {
            bool ready = HasPulledAttributeParents();
            bool active = GetTab() == 2;
            string css = active ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            if (!ready)
                css += " is-disabled";
            btnTabAttrVariants.CssClass = css;
            btnTabAttrVariants.Enabled = ready;
            btnTabAttrVariants.ToolTip = ready
                ? string.Empty
                : MessageProvider.Get(MessageKeys.WooCommerce.MapAttrVariantsLocked);
        }

        private void SyncAttrVariantsPanels()
        {
            bool ready = HasPulledAttributeParents();
            pnlAttrVariantsLocked.Visible = !ready;
            pnlAttrVariants.Visible = ready;
            pnlAttrVariants.Enabled = ready;
        }

        protected void gvAttrParents_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var row = e.Row.DataItem as WooAttributeParent;
            if (row == null)
                return;

            var chk = e.Row.FindControl("chkUseForVariants") as CheckBox;
            if (chk != null)
            {
                bool dbVal = row.UseForVariants;
                if (_dbAttrUseForVariants != null && _dbAttrUseForVariants.ContainsKey(row.ParentID))
                    dbVal = _dbAttrUseForVariants[row.ParentID];
                chk.InputAttributes["data-original"] = dbVal ? "1" : "0";
            }

            BindRankDropdown(e.Row, "ddlQtyRank", row.QtyRank, _dbAttrQtyRanks, row.ParentID);
            BindRankDropdown(e.Row, "ddlPackRank", row.PackRank, _dbAttrPackRanks, row.ParentID);
            BindRankDropdown(e.Row, "ddlNoteRank", row.NoteRank, _dbAttrNoteRanks, row.ParentID);
        }

        private static void BindRankDropdown(
            GridViewRow gridRow,
            string controlId,
            int currentRank,
            Dictionary<int, int> dbRanks,
            int parentId)
        {
            var ddl = gridRow.FindControl(controlId) as DropDownList;
            if (ddl == null)
                return;
            int rank = currentRank;
            if (dbRanks != null && dbRanks.ContainsKey(parentId))
                rank = dbRanks[parentId];
            rank = WooAttributeParent.NormalizeRank(rank);
            string value = rank.ToString(CultureInfo.InvariantCulture);
            if (ddl.Items.FindByValue(value) != null)
                ddl.SelectedValue = value;
            ddl.Attributes["data-original"] = value;
        }

        protected void btnPullAttributes_Click(object sender, EventArgs e)
        {
            try
            {
                if (!HasPulledAttributeParents())
                {
                    SyncAttrVariantsPanels();
                    SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapAttrVariantsLocked), true);
                    return;
                }

                var rows = _manager.PullDistinctAttributes(UserName());
                Session[SessionAttrPull] = rows;
                gvAttr.PageIndex = 0;
                gvAttr.DataSource = rows;
                gvAttr.DataBind();
                SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapPullAttributesOk, rows.Count), false);
                BindActionButtonTexts();
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private List<WooAttributeMap> EnsureAttrRows()
        {
            var rows = Session[SessionAttrPull] as List<WooAttributeMap>;
            if (rows == null)
            {
                rows = (_manager.GetAttributeMaps() ?? new List<WooAttributeMap>())
                    .Where(m => m != null && m.ItemServiceTypeID == 0)
                    .ToList();
                Session[SessionAttrPull] = rows;
            }
            return rows;
        }

        private void RebindAttributes()
        {
            gvAttr.DataSource = EnsureAttrRows();
            gvAttr.DataBind();
        }

        protected void gvAttr_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Persist edits on the page being left so Save still covers paged rows.
            MergeAttrGridEditsIntoSession();
            gvAttr.PageIndex = e.NewPageIndex;
            RebindAttributes();
        }

        /// <summary>
        /// Copies current-page qty/packaging/service edits into the session list before paging away.
        /// </summary>
        private void MergeAttrGridEditsIntoSession()
        {
            var rows = EnsureAttrRows();
            if (rows.Count == 0 || gvAttr.Rows.Count == 0)
                return;

            var byKey = rows
                .GroupBy(r => r.RowKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (GridViewRow row in gvAttr.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;

                var hdnName = (HiddenField)row.FindControl("hdnAttrName");
                var hdnOption = (HiddenField)row.FindControl("hdnAttrOption");
                var hdnId = (HiddenField)row.FindControl("hdnAttrMapId");
                var hdnRole = (HiddenField)row.FindControl("hdnAttrRole");
                var txtQty = (TextBox)row.FindControl("txtAttrQty");
                var ddlPack = (DropDownList)row.FindControl("ddlAttrPack");
                var ddlSvc = (DropDownList)row.FindControl("ddlAttrSvc");
                if (txtQty == null || ddlPack == null)
                    continue;

                WooAttributeMap target = null;
                string boundKey = Convert.ToString(gvAttr.DataKeys[row.RowIndex].Value);
                if (!string.IsNullOrEmpty(boundKey))
                    byKey.TryGetValue(boundKey, out target);
                if (target == null && hdnId != null)
                {
                    int mapId;
                    if (int.TryParse(hdnId.Value, out mapId) && mapId > 0)
                        target = rows.FirstOrDefault(r => r.MapID == mapId);
                }
                if (target == null && hdnName != null && hdnOption != null)
                {
                    target = rows.FirstOrDefault(r =>
                        string.Equals(r.AttributeName, hdnName.Value, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(r.AttributeOption, hdnOption.Value, StringComparison.OrdinalIgnoreCase)
                        && r.ItemServiceTypeID == 0);
                }
                if (target == null)
                    continue;

                int svcId = 0;
                if (ddlSvc != null)
                    int.TryParse(ddlSvc.SelectedValue, out svcId);
                double qty;
                if (!double.TryParse(txtQty.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out qty))
                    qty = 1;
                int packId;
                int.TryParse(ddlPack.SelectedValue, out packId);

                target.QtyFactor = qty;
                target.PackagingID = packId > 0 ? (int?)packId : null;
                target.ItemServiceTypeID = svcId;
                if (hdnRole != null && !string.IsNullOrWhiteSpace(hdnRole.Value))
                    target.MapRole = WooAttributeMapRoles.Normalize(hdnRole.Value);
                if (hdnId != null)
                {
                    int mapId;
                    if (int.TryParse(hdnId.Value, out mapId))
                        target.MapID = mapId;
                }
            }

            Session[SessionAttrPull] = rows;
        }

        protected void gvAttr_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as WooAttributeMap;
            if (row == null) return;

            var ddlPack = e.Row.FindControl("ddlAttrPack") as DropDownList;
            var ddlSvc = e.Row.FindControl("ddlAttrSvc") as DropDownList;
            if (ddlPack != null)
            {
                FillPackagingDropdown(ddlPack, row.PackagingID ?? row.SuggestedPackagingID, null);
                MarkOriginal(ddlPack, ddlPack.SelectedValue);
            }
            if (ddlSvc != null)
            {
                FillServiceTypeDropdown(ddlSvc, row.ItemServiceTypeID);
                MarkOriginal(ddlSvc, ddlSvc.SelectedValue);
            }
            bool notesOnly = WooAttributeMapRoles.IsNotesOnly(row.MapRole);
            if (ddlPack != null)
                ddlPack.Enabled = !notesOnly && WooAttributeMapRoles.AppliesPackaging(row.MapRole);
            var txtQty = e.Row.FindControl("txtAttrQty") as TextBox;
            if (txtQty != null)
            {
                txtQty.Enabled = !notesOnly && WooAttributeMapRoles.AppliesQty(row.MapRole);
                MarkOriginal(txtQty, txtQty.Text);
            }
        }

        protected void btnSaveAttrMaps_Click(object sender, EventArgs e)
        {
            int saved = 0;
            try
            {
                MergeAttrGridEditsIntoSession();
                var rows = EnsureAttrRows();
                if (rows.Count == 0)
                {
                    SetStatus("Pull attributes first, then save.", true);
                    return;
                }

                foreach (var row in rows)
                {
                    if (row == null || string.IsNullOrWhiteSpace(row.AttributeName) || string.IsNullOrWhiteSpace(row.AttributeOption))
                        continue;

                    var map = new WooAttributeMap
                    {
                        MapID = row.MapID,
                        AttributeName = row.AttributeName,
                        AttributeOption = row.AttributeOption,
                        QtyFactor = row.QtyFactor <= 0 ? 1 : row.QtyFactor,
                        PackagingID = row.PackagingID,
                        MapRole = WooAttributeMapRoles.Normalize(row.MapRole),
                        ItemServiceTypeID = row.ItemServiceTypeID,
                        IsActive = true,
                        Notes = row.Notes
                    };
                    row.MapID = _manager.SaveAttributeMap(map, UserName());
                    saved++;
                }

                var refreshed = (_manager.GetAttributeMaps() ?? new List<WooAttributeMap>())
                    .Where(m => m != null && m.ItemServiceTypeID == 0)
                    .ToList();
                Session[SessionAttrPull] = refreshed;
                gvAttr.DataSource = refreshed;
                gvAttr.DataBind();
                ClearUnsavedFlag(hdnUnsavedAttrMaps);
                SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapAttrSaved, saved), false);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        protected void btnPullProducts_Click(object sender, EventArgs e)
        {
            try
            {
                var result = _manager.PullProductsForMapping(UserName());
                Session[SessionPull] = result.MappingRows;
                Session[SessionMissingSku] = result.MissingSkuRows;
                Session[SessionPullFind] = null;
                Session[SessionPullNewOnly] = null;
                Session[SessionPullVer] = PullRowsVersion;
                ClearPullDirty();
                txtFindSku.Text = string.Empty;
                chkNewSinceSync.Checked = false;
                gvPull.PageIndex = 0;
                gvMissingSku.PageIndex = 0;
                RebindPull();
                RebindMissingSku();
                SyncSaveSelectedButton();
                SyncWriteMissingButton();
                SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapPullProductsOk,
                    result.MappingRows.Count, result.MissingSkuRows.Count, result.ParentsScanned)
                    + (result.NewSinceLastSyncCount > 0
                        ? " " + result.NewSinceLastSyncCount + " new since previous pull."
                        : string.Empty)
                    + (result.HitCatalogCap
                        ? " " + MessageProvider.Get(MessageKeys.WooCommerce.MapPullProductsCapWarn)
                        : string.Empty), false);
                BindActionButtonTexts();
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        protected void btnResetCatalog_Click(object sender, EventArgs e)
        {
            try
            {
                _manager.ResetCatalogCache(UserName());
                Session[SessionPull] = new List<WooProductMapRow>();
                Session[SessionMissingSku] = new List<WooProductMapRow>();
                Session[SessionPullFind] = null;
                Session[SessionPullVer] = PullRowsVersion;
                ClearPullDirty();
                txtFindSku.Text = string.Empty;
                gvPull.PageIndex = 0;
                gvMissingSku.PageIndex = 0;
                RebindPull();
                RebindMissingSku();
                SyncSaveSelectedButton();
                SyncWriteMissingButton();
                BindActionButtonTexts();
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapResetCatalogOk), false);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        protected void btnFindSku_Click(object sender, EventArgs e)
        {
            MergeVisiblePullEdits();
            string needle = (txtFindSku.Text ?? string.Empty).Trim();
            if (needle.StartsWith("%", StringComparison.Ordinal))
                needle = needle.TrimStart('%').Trim();
            Session[SessionPullFind] = needle;
            gvPull.PageIndex = 0;
            RebindPull();
            SyncSaveSelectedButton();
        }

        protected void btnClearFindSku_Click(object sender, EventArgs e)
        {
            MergeVisiblePullEdits();
            txtFindSku.Text = string.Empty;
            Session[SessionPullFind] = null;
            gvPull.PageIndex = 0;
            RebindPull();
            SyncSaveSelectedButton();
        }

        private List<WooProductMapRow> GetAllPullRows()
        {
            EnsurePullRowsLoaded();
            return Session[SessionPull] as List<WooProductMapRow> ?? new List<WooProductMapRow>();
        }

        private List<WooProductMapRow> GetMissingSkuRows()
        {
            EnsurePullRowsLoaded();
            var rows = Session[SessionMissingSku] as List<WooProductMapRow> ?? new List<WooProductMapRow>();
            if (!IsPullNewOnlyFilter())
                return rows;
            return rows.Where(r => r != null && r.IsNewSinceLastSync).ToList();
        }

        private bool IsPullNewOnlyFilter()
        {
            object v = Session[SessionPullNewOnly];
            return v is bool && (bool)v;
        }

        protected void chkNewSinceSync_CheckedChanged(object sender, EventArgs e)
        {
            MergeVisiblePullEdits();
            Session[SessionPullNewOnly] = chkNewSinceSync.Checked;
            gvPull.PageIndex = 0;
            gvMissingSku.PageIndex = 0;
            RebindPull();
            RebindMissingSku();
            SyncSaveSelectedButton();
            SyncWriteMissingButton();
        }

        /// <summary>Session first; otherwise rebuild from the local Woo catalog cache (no API).</summary>
        private void EnsurePullRowsLoaded()
        {
            object ver = Session[SessionPullVer];
            bool versionOk = ver is int && (int)ver == PullRowsVersion;
            if (Session[SessionPull] != null && versionOk)
                return;
            RebuildPullRowsPreservingUserModes();
        }

        private static bool RowMatchesFind(WooProductMapRow r, string needle, HashSet<int> trackerItemIds = null)
        {
            if (r == null || string.IsNullOrEmpty(needle))
                return true;
            if (ContainsIgnoreCase(r.Sku, needle)
                || ContainsIgnoreCase(r.ParentSku, needle)
                || ContainsIgnoreCase(r.DisplaySku, needle)
                || ContainsIgnoreCase(r.Name, needle)
                || ContainsIgnoreCase(r.ParentName, needle)
                || ContainsIgnoreCase(r.DisplayName, needle)
                || ContainsIgnoreCase(r.AttributesLabel, needle)
                || ContainsIgnoreCase(r.SuggestedItemDesc, needle)
                || ContainsIgnoreCase(r.CreateSku, needle)
                || ContainsIgnoreCase(r.NewSku, needle)
                || ContainsIgnoreCase(r.MatchReason, needle))
                return true;

            if (trackerItemIds != null)
            {
                if (r.MappedItemID > 0 && trackerItemIds.Contains(r.MappedItemID))
                    return true;
                if (r.SuggestedItemID > 0 && trackerItemIds.Contains(r.SuggestedItemID))
                    return true;
            }

            return false;
        }

        private HashSet<int> FindTrackerItemIdsMatching(string needle)
        {
            if (string.IsNullOrWhiteSpace(needle))
                return new HashSet<int>();

            var ids = new HashSet<int>();
            foreach (Item item in _itemsRepo.GetAll() ?? new List<Item>())
            {
                if (item == null || item.ItemID <= 0)
                    continue;
                if (ContainsIgnoreCase(item.SKU, needle)
                    || ContainsIgnoreCase(item.ItemDesc, needle)
                    || ContainsIgnoreCase(item.ItemShortName, needle))
                    ids.Add(item.ItemID);
            }

            return ids;
        }

        private static bool ContainsIgnoreCase(string haystack, string needle)
        {
            return !string.IsNullOrEmpty(haystack)
                && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private List<WooProductMapRow> GetVisiblePullRows()
        {
            var rows = GetAllPullRows();
            if (IsPullNewOnlyFilter())
            {
                var newFamilies = new HashSet<long>(rows
                    .Where(r => r != null && r.IsNewSinceLastSync)
                    .Select(r => r.WooProductId));
                rows = rows.Where(r => r != null && newFamilies.Contains(r.WooProductId)).ToList();
            }

            string find = Session[SessionPullFind] as string;
            var parentIds = new HashSet<long>(rows.Where(r => r.IsParentGroup).Select(r => r.WooProductId));
            var expanded = new HashSet<long>(rows.Where(r => r.IsParentGroup && r.GroupExpanded).Select(r => r.WooProductId));

            if (!string.IsNullOrWhiteSpace(find))
            {
                string needle = find.Trim();
                HashSet<int> trackerItemIds = FindTrackerItemIdsMatching(needle);
                var matchedProducts = new HashSet<long>();
                foreach (var r in rows)
                {
                    if (!RowMatchesFind(r, needle, trackerItemIds))
                        continue;
                    matchedProducts.Add(r.WooProductId);
                }
                // Search shows the whole family so qty + pack variants are visible.
                return rows.Where(r => matchedProducts.Contains(r.WooProductId)).ToList();
            }

            return rows.Where(r =>
            {
                if (!parentIds.Contains(r.WooProductId) || r.IsParentGroup)
                    return true;
                return expanded.Contains(r.WooProductId);
            }).ToList();
        }

        private int GetPullListStart()
        {
            object v = ViewState["PullListStart"];
            if (v is int)
                return Math.Max(0, (int)v);
            return Math.Max(0, gvPull.PageIndex * gvPull.PageSize);
        }

        private void SetPullListStart(int start)
        {
            ViewState["PullListStart"] = Math.Max(0, start);
        }

        private void RebindPull()
        {
            var rows = GetVisiblePullRows();
            int pageSize = gvPull.PageSize > 0 ? gvPull.PageSize : 40;
            int start = GetPullListStart();
            if (rows.Count == 0)
                start = 0;
            else if (start >= rows.Count)
                start = Math.Max(0, ((rows.Count - 1) / pageSize) * pageSize);

            SetPullListStart(start);
            gvPull.AllowCustomPaging = true;
            gvPull.VirtualItemCount = rows.Count;
            // Expand can start mid-list (parent at top). Treat that as the following page
            // so the pager / "page N of M" label stay in sync with the jump.
            int pageCount = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)pageSize));
            int pageIndex = start / pageSize;
            if (start % pageSize != 0)
                pageIndex = Math.Min(pageIndex + 1, pageCount - 1);
            gvPull.PageIndex = pageIndex;
            gvPull.DataSource = rows.Skip(start).Take(pageSize).ToList();
            gvPull.DataBind();
            UpdatePullPageInfo(rows.Count);
            SyncPullDirtyHidden();
            SyncSaveSelectedButton();

            string find = Session[SessionPullFind] as string;
            if (!string.IsNullOrEmpty(find) && string.IsNullOrEmpty(txtFindSku.Text))
                txtFindSku.Text = find;
            if (chkNewSinceSync.Checked != IsPullNewOnlyFilter())
                chkNewSinceSync.Checked = IsPullNewOnlyFilter();
        }

        private void UpdatePullPageInfo(int visibleCount)
        {
            int total = GetAllPullRows().Count;
            if (total == 0)
            {
                litPullPageInfo.Text = string.Empty;
                return;
            }

            int page = gvPull.PageIndex + 1;
            int pages = Math.Max(1, (int)Math.Ceiling(visibleCount / (double)gvPull.PageSize));
            string find = Session[SessionPullFind] as string;
            string text;
            if (!string.IsNullOrWhiteSpace(find))
                text = MessageProvider.Format(MessageKeys.WooCommerce.MapPullPageInfoFiltered, visibleCount, total, page, pages, find);
            else if (IsPullNewOnlyFilter())
                text = MessageProvider.Format(MessageKeys.WooCommerce.MapPullPageInfoNewOnly, visibleCount, total, page, pages);
            else
                text = MessageProvider.Format(MessageKeys.WooCommerce.MapPullPageInfo, visibleCount, page, pages);
            litPullPageInfo.Text = "<p class=\"woo-map-page-info\">" + Server.HtmlEncode(text) + "</p>";
        }

        private void RebindMissingSku()
        {
            var rows = GetMissingSkuRows();
            if (gvMissingSku.PageIndex > 0)
            {
                int pageCount = rows.Count == 0 ? 1 : (int)Math.Ceiling(rows.Count / (double)gvMissingSku.PageSize);
                if (gvMissingSku.PageIndex >= pageCount)
                    gvMissingSku.PageIndex = Math.Max(0, pageCount - 1);
            }
            gvMissingSku.DataSource = rows;
            gvMissingSku.DataBind();
            if (rows.Count == 0)
            {
                litMissingSkuPageInfo.Text = string.Empty;
            }
            else
            {
                int page = gvMissingSku.PageIndex + 1;
                int pages = Math.Max(1, (int)Math.Ceiling(rows.Count / (double)gvMissingSku.PageSize));
                litMissingSkuPageInfo.Text = "<p class=\"woo-map-page-info\">"
                    + Server.HtmlEncode(MessageProvider.Format(MessageKeys.WooCommerce.MapMissingSkuPageInfo, rows.Count, page, pages))
                    + "</p>";
            }
            SyncWriteMissingButton();
        }

        protected void gvPull_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            MergeVisiblePullEdits();
            int pageSize = gvPull.PageSize > 0 ? gvPull.PageSize : 40;
            SetPullListStart(e.NewPageIndex * pageSize);
            gvPull.PageIndex = e.NewPageIndex;
            RebindPull();
        }

        protected void gvMissingSku_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            MergeVisibleMissingSkuEdits();
            gvMissingSku.PageIndex = e.NewPageIndex;
            RebindMissingSku();
        }

        private void SetPullDirty()
        {
            Session[SessionPullDirty] = true;
            SyncPullDirtyHidden();
        }

        private void ClearPullDirty()
        {
            Session[SessionPullDirty] = false;
            SyncPullDirtyHidden();
        }

        private void SyncPullDirtyHidden()
        {
            if (hdnPullDirty == null)
                return;
            // Client may set hdnPullDirty without Session (Qty/SKU edits). Any postback used to
            // overwrite that with Session=false while pencils still showed IsDirtyVsSaved.
            bool dirty = Session[SessionPullDirty] is bool && (bool)Session[SessionPullDirty];
            if (!dirty && GetAllPullRows().Any(r => r != null && r.IsDirtyVsSaved))
            {
                Session[SessionPullDirty] = true;
                dirty = true;
            }
            hdnPullDirty.Value = dirty ? "1" : "0";
        }

        private void SyncSaveSelectedButton()
        {
            SyncPullDirtyHidden();
        }

        private void SyncWriteMissingButton()
        {
        }

        private void MergeVisiblePullEdits()
        {
            var all = GetAllPullRows();
            if (all.Count == 0 || gvPull.Rows.Count == 0)
                return;

            var byKey = all.ToDictionary(r => r.RowKey, StringComparer.Ordinal);
            var editedKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (GridViewRow gvRow in gvPull.Rows)
            {
                if (gvRow.RowType != DataControlRowType.DataRow)
                    continue;
                string key = Convert.ToString(gvPull.DataKeys[gvRow.RowIndex].Value);
                WooProductMapRow row;
                if (string.IsNullOrEmpty(key) || !byKey.TryGetValue(key, out row))
                    continue;

                var chk = gvRow.FindControl("chkImport") as CheckBox;
                var ddl = gvRow.FindControl("ddlItem") as DropDownList;
                var ddlMode = gvRow.FindControl("ddlImportMode") as DropDownList;
                var txtQty = gvRow.FindControl("txtQty") as TextBox;
                var ddlPack = gvRow.FindControl("ddlPack") as DropDownList;

                string prevMode = row.ImportMode;
                int prevDest = row.MappedItemID;
                bool prevNotes = row.MapToNotes;
                bool prevInclude = row.IncludeInImport;
                double prevQty = row.QtyFactor;
                int? prevPack = row.PackagingID;
                string prevSku = row.CreateSku ?? string.Empty;
                int prevSort = row.CreateSortOrder;

                if (chk != null && chk.Enabled && (!row.IsParentGroup || row.ParentMapsAsDestination))
                    row.IncludeInImport = chk.Checked;

                // Mode dropdown is filled in RowDataBound; on Save postback it can reset to the
                // first item (Variants). Do not let that wipe ParentNotes / ParentItem / Exclude
                // already held in session unless Destination confirms Variants.
                if (ddlMode != null && ddlMode.Visible && !string.IsNullOrEmpty(ddlMode.SelectedValue))
                {
                    string postedMode = ddlMode.SelectedValue;
                    bool wouldRegressToVariants =
                        string.Equals(postedMode, WooProductMapRow.ImportModeVariants, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(row.ImportMode, WooProductMapRow.ImportModeVariants, StringComparison.OrdinalIgnoreCase)
                        && (row.ImportParentAsNotes || row.ImportParentAsItem || row.ImportParentExcluded);
                    if (!wouldRegressToVariants
                        && !string.Equals(row.ImportMode, postedMode, StringComparison.OrdinalIgnoreCase))
                    {
                        row.ImportMode = postedMode;
                        if (row.IsParentGroup)
                        {
                            row.ImportModeUserSet = true;
                            row.UsesCategoryImportDefault = false;
                        }
                    }
                    else if (!wouldRegressToVariants)
                        row.ImportMode = postedMode;
                }

                if (ddl != null && ddl.Visible)
                {
                    int dest;
                    if (int.TryParse(ddl.SelectedValue, out dest))
                    {
                        row.MapToNotes = dest == WooProductMapRow.DestinationNotesValue;
                        row.MappedItemID = dest;
                        if (dest == WooProductMapRow.DestinationCreateParent)
                        {
                            // Switching to Create: prefer Woo catalog SKU over mapped Tracker SKU.
                            if (prevDest != WooProductMapRow.DestinationCreateParent)
                                row.CreateSkuUserSet = false;
                            SeedCreateSkuIfNeeded(row, forceWooSku: prevDest != WooProductMapRow.DestinationCreateParent);
                        }
                        if (row.IsParentGroup)
                        {
                            if (dest == WooProductMapRow.DestinationNotesValue)
                            {
                                row.ImportMode = WooProductMapRow.ImportModeParentNotes;
                                row.ImportModeUserSet = true;
                                row.UsesCategoryImportDefault = false;
                            }
                            else if (dest == WooProductMapRow.DestinationCreateParent || dest > 0)
                            {
                                if (!row.ImportParentExcluded)
                                {
                                    row.ImportMode = WooProductMapRow.ImportModeParentItem;
                                    row.ImportModeUserSet = true;
                                    row.UsesCategoryImportDefault = false;
                                }
                            }
                        }
                        if (row.MapToNotes)
                            row.MatchReason = "Order notes";
                    }
                }

                if (txtQty != null && txtQty.Visible)
                {
                    double qty;
                    if (double.TryParse(txtQty.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out qty))
                        row.QtyFactor = qty;
                }

                if (ddlPack != null && ddlPack.Visible)
                {
                    int packId;
                    if (int.TryParse(ddlPack.SelectedValue, out packId))
                    {
                        // Do not clear a known pack when the dropdown posts 0 after a list rebuild glitch.
                        if (packId > 0)
                            row.PackagingID = packId;
                        else if (ddlPack.SelectedValue == "0" || string.IsNullOrEmpty(ddlPack.SelectedValue))
                        {
                            var sel = ddlPack.SelectedItem;
                            if (sel != null && string.Equals(sel.Value, "0", StringComparison.Ordinal))
                                row.PackagingID = null;
                        }
                    }
                }

                var txtItemSku = gvRow.FindControl("txtItemSku") as TextBox;
                bool destChanged = prevDest != row.MappedItemID;
                bool justSwitchedToCreate = row.MappedItemID == WooProductMapRow.DestinationCreateParent
                    && destChanged;
                // Textbox still shows the previous mapping's SKU until rebind. Do not copy it
                // onto a newly selected Tracker item (or a just-seeded Create SKU).
                if (txtItemSku != null && txtItemSku.Visible && !justSwitchedToCreate && !destChanged)
                {
                    string typed = txtItemSku.Text ?? string.Empty;
                    row.CreateSku = typed;
                    if (!string.Equals(typed, prevSku, StringComparison.Ordinal))
                        row.CreateSkuUserSet = true;
                }

                var ddlSort = gvRow.FindControl("ddlSortOrder") as DropDownList;
                if (ddlSort != null && ddlSort.Visible && !destChanged)
                {
                    int sort;
                    if (int.TryParse(ddlSort.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out sort))
                        row.CreateSortOrder = sort;
                }

                if (destChanged)
                    SyncItemSkuFromDestination(row);

                bool edited = !string.Equals(prevMode, row.ImportMode, StringComparison.OrdinalIgnoreCase)
                    || prevDest != row.MappedItemID
                    || prevNotes != row.MapToNotes
                    || prevInclude != row.IncludeInImport
                    || Math.Abs(prevQty - row.QtyFactor) > 0.000001
                    || !Nullable.Equals(prevPack, row.PackagingID)
                    || !string.Equals(prevSku, row.CreateSku ?? string.Empty, StringComparison.Ordinal)
                    || prevSort != row.CreateSortOrder;
                if (edited)
                {
                    row.ApplySelected = true;
                    // Only the parent row itself — never mark the whole group dirty from a variant edit.
                    if (row.IsParentGroup
                        && (!string.Equals(prevMode, row.ImportMode, StringComparison.OrdinalIgnoreCase)
                            || prevDest != row.MappedItemID
                            || prevNotes != row.MapToNotes
                            || prevInclude != row.IncludeInImport))
                    {
                        row.ImportModeUserSet = true;
                    }
                    editedKeys.Add(row.RowKey);
                }
            }

            // Snapshot dirtiness before ApplyParentImportModes — that sync can change IncludeInImport
            // and must not clear a real user S/O/Qty/Pack/SKU edit from a prior postback.
            var dirtyBefore = new HashSet<string>(StringComparer.Ordinal);
            foreach (var r in all)
            {
                if (r != null && r.IsDirtyVsSaved)
                    dirtyBefore.Add(r.RowKey);
            }

            WooCommerceMappingManager.ApplyParentImportModes(all);
            foreach (var editedKey in editedKeys)
            {
                WooProductMapRow editedRow;
                if (byKey.TryGetValue(editedKey, out editedRow))
                    editedRow.ApplySelected = true;
            }
            foreach (var r in all)
            {
                if (r == null || !r.HasSavedMapping)
                    continue;
                if (editedKeys.Contains(r.RowKey) || dirtyBefore.Contains(r.RowKey))
                    continue;
                r.CaptureSavedSnapshot();
            }
            Session[SessionPull] = all;
            if (editedKeys.Count > 0 || dirtyBefore.Count > 0
                || all.Any(r => r != null && r.IsDirtyVsSaved))
                SetPullDirty();
        }

        private static void MarkRowApplySelected(WooProductMapRow row)
        {
            if (row == null)
                return;
            row.ApplySelected = true;
        }

        /// <summary>
        /// When Tracker SKU mapping changes, Item SKU / S/O follow the selected item
        /// (Create still seeds from the Woo catalog SKU).
        /// </summary>
        private void SyncItemSkuFromDestination(WooProductMapRow row)
        {
            if (row == null)
                return;
            if (row.MapToNotes || row.MappedItemID == WooProductMapRow.DestinationNotesValue
                || row.ImportParentAsNotes || row.ImportParentExcluded)
                return;
            if (row.MappedItemID == WooProductMapRow.DestinationCreateParent)
            {
                row.CreateSkuUserSet = false;
                SeedCreateSkuIfNeeded(row, forceWooSku: true);
                return;
            }
            if (row.MappedItemID <= 0)
                return;

            var item = _itemsRepo.GetById(row.MappedItemID);
            if (item == null)
                return;
            row.CreateSku = (item.SKU ?? string.Empty).Trim();
            if (item.SortOrder.HasValue && item.SortOrder.Value > 0)
                row.CreateSortOrder = item.SortOrder.Value;
            row.CreateSkuUserSet = false;
        }

        /// <summary>
        /// Fills Item SKU from the Woo catalog SKU when creating a Tracker item.
        /// forceWooSku: destination just switched to Create — overwrite a leftover mapped Tracker SKU.
        /// </summary>
        private static void SeedCreateSkuIfNeeded(WooProductMapRow row, bool forceWooSku = false)
        {
            if (row == null)
                return;
            if (!forceWooSku && row.CreateSkuUserSet && !string.IsNullOrWhiteSpace(row.CreateSku))
                return;
            string def = row.WooCreateSkuDefault;
            if (string.IsNullOrWhiteSpace(def))
                return;
            if (forceWooSku
                || string.IsNullOrWhiteSpace(row.CreateSku)
                || !row.CreateSkuUserSet)
            {
                row.CreateSku = def;
                if (forceWooSku)
                    row.CreateSkuUserSet = false;
            }
        }

        protected void ddlPack_SelectedIndexChanged(object sender, EventArgs e)
        {
            MergeVisiblePullEdits();
            SetPullDirty();
            RebindPull();
        }

        protected void ddlSortOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            MergeVisiblePullEdits();
            SetPullDirty();
            RebindPull();
        }

        protected void chkImport_CheckedChanged(object sender, EventArgs e)
        {
            MergeVisiblePullEdits();
            var chk = sender as CheckBox;
            if (chk == null)
                return;
            var gvRow = chk.NamingContainer as GridViewRow;
            if (gvRow == null)
                return;
            string key = Convert.ToString(gvPull.DataKeys[gvRow.RowIndex].Value);
            var rows = GetAllPullRows();
            var parent = rows.FirstOrDefault(r => string.Equals(r.RowKey, key, StringComparison.Ordinal));
            if (parent == null || !parent.IsParentGroup)
                return;

            if (parent.ParentMapsAsDestination)
                parent.IncludeInImport = chk.Checked;
            else
                parent.ImportMode = chk.Checked
                    ? WooProductMapRow.ImportModeVariants
                    : WooProductMapRow.ImportModeExclude;

            parent.ImportModeUserSet = true;
            parent.UsesCategoryImportDefault = false;
            MarkRowApplySelected(parent);
            WooCommerceMappingManager.ApplyParentImportModes(rows);
            MarkRowApplySelected(parent);
            Session[SessionPull] = rows;
            SetPullDirty();
            RebindPull();
        }

        protected void ddlItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            MergeVisiblePullEdits();
            var ddl = sender as DropDownList;
            if (ddl == null)
                return;
            var gvRow = ddl.NamingContainer as GridViewRow;
            if (gvRow == null)
                return;
            string key = Convert.ToString(gvPull.DataKeys[gvRow.RowIndex].Value);
            var rows = GetAllPullRows();
            var row = rows.FirstOrDefault(r => string.Equals(r.RowKey, key, StringComparison.Ordinal));
            if (row == null)
                return;

            int dest;
            if (!int.TryParse(ddl.SelectedValue, out dest))
                return;

            int prevDest = row.MappedItemID;

            if (row.IsParentGroup)
            {
                if (dest == WooProductMapRow.DestinationNotesValue)
                    row.ImportMode = WooProductMapRow.ImportModeParentNotes;
                else if (dest == WooProductMapRow.DestinationCreateParent || dest > 0)
                {
                    row.ImportMode = WooProductMapRow.ImportModeParentItem;
                    row.MappedItemID = dest;
                }
                else if (!row.ImportParentExcluded)
                {
                    row.ImportMode = WooProductMapRow.ImportModeVariants;
                    row.MappedItemID = 0;
                }
                row.ImportModeUserSet = true;
                row.UsesCategoryImportDefault = false;
            }
            else
            {
                row.MappedItemID = dest;
                row.MapToNotes = dest == WooProductMapRow.DestinationNotesValue;
            }

            if (dest == WooProductMapRow.DestinationCreateParent)
            {
                if (prevDest != WooProductMapRow.DestinationCreateParent)
                    row.CreateSkuUserSet = false;
                SeedCreateSkuIfNeeded(row, forceWooSku: prevDest != WooProductMapRow.DestinationCreateParent);
            }
            else
                SyncItemSkuFromDestination(row);

            MarkRowApplySelected(row);
            WooCommerceMappingManager.ApplyParentImportModes(rows);
            MarkRowApplySelected(row);
            Session[SessionPull] = rows;
            SetPullDirty();
            RebindPull();
        }

        protected void ddlImportMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            MergeVisiblePullEdits();
            var ddl = sender as DropDownList;
            if (ddl == null)
                return;
            var gvRow = ddl.NamingContainer as GridViewRow;
            if (gvRow == null)
                return;
            string key = Convert.ToString(gvPull.DataKeys[gvRow.RowIndex].Value);
            var rows = GetAllPullRows();
            var parent = rows.FirstOrDefault(r => string.Equals(r.RowKey, key, StringComparison.Ordinal));
            if (parent == null || !parent.IsParentGroup)
                return;
            parent.ImportMode = ddl.SelectedValue;
            parent.ImportModeUserSet = true;
            parent.UsesCategoryImportDefault = false;
            MarkRowApplySelected(parent);
            WooCommerceMappingManager.ApplyParentImportModes(rows);
            if (parent.ImportParentAsItem == false && parent.ImportParentAsNotes == false && parent.ImportParentExcluded == false)
                _manager.RematchUnmappedChildren(rows, parent.WooProductId);
            MarkRowApplySelected(parent);
            Session[SessionPull] = rows;
            SetPullDirty();
            RebindPull();
        }

        protected void gvPull_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "ToggleGroup", StringComparison.Ordinal))
                return;
            MergeVisiblePullEdits();
            long productId;
            if (!long.TryParse(Convert.ToString(e.CommandArgument), NumberStyles.Integer, CultureInfo.InvariantCulture, out productId))
                return;
            var rows = GetAllPullRows();
            var parent = rows.FirstOrDefault(r => r.IsParentGroup && r.WooProductId == productId);
            if (parent == null)
                return;
            parent.GroupExpanded = !parent.GroupExpanded;
            Session[SessionPull] = rows;

            if (parent.GroupExpanded)
            {
                var visible = GetVisiblePullRows();
                int idx = visible.FindIndex(r => r.IsParentGroup && r.WooProductId == productId);
                int pageSize = gvPull.PageSize > 0 ? gvPull.PageSize : 40;
                int start = GetPullListStart();
                if (idx >= 0)
                {
                    int posOnPage = idx - start;
                    int childCount = visible.Count(r => !r.IsParentGroup && r.WooProductId == productId);
                    int room = pageSize - posOnPage - 1;
                    if (posOnPage < 0 || room < 1)
                        SetPullListStart(idx);
                    else if (childCount > room)
                        SetPullListStart(idx);
                }
            }

            RebindPull();
        }

        protected void btnExpandAllGroups_Click(object sender, EventArgs e)
        {
            SetAllGroupsExpanded(true);
        }

        protected void btnCollapseAllGroups_Click(object sender, EventArgs e)
        {
            SetAllGroupsExpanded(false);
        }

        private void SetAllGroupsExpanded(bool expanded)
        {
            MergeVisiblePullEdits();
            var rows = GetAllPullRows();
            foreach (var r in rows)
            {
                if (r.IsParentGroup)
                    r.GroupExpanded = expanded;
            }
            Session[SessionPull] = rows;
            SetPullListStart(0);
            gvPull.PageIndex = 0;
            RebindPull();
        }

        protected void gvPull_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as WooProductMapRow;
            if (row == null)
                return;

            if (row.IsParentGroup)
                e.Row.CssClass = (e.Row.CssClass + " woo-map-row-parent"
                    + (row.ImportParentExcluded ? " woo-map-row-excluded" : "")
                    + (row.HasSavedMapping ? " woo-map-row-saved" : "")).Trim();
            else if (row.IsVariation)
                e.Row.CssClass = (e.Row.CssClass + " woo-map-row-variant"
                    + (row.HasSavedMapping ? " woo-map-row-saved" : "")).Trim();
            else if (row.HasSavedMapping)
                e.Row.CssClass = (e.Row.CssClass + " woo-map-row-saved").Trim();

            var ddlSortOrder = e.Row.FindControl("ddlSortOrder") as DropDownList;
            if (ddlSortOrder != null && ddlSortOrder.Visible)
            {
                _sortOrdersRepo.FillDropDownCompact(ddlSortOrder, row.CreateSortOrder);
                SyncCompactSortOrderTooltip(ddlSortOrder);
            }

            var chkImport = e.Row.FindControl("chkImport") as CheckBox;
            if (chkImport != null)
            {
                chkImport.AutoPostBack = row.IsParentGroup;
                chkImport.ToolTip = MessageProvider.Get(MessageKeys.WooCommerce.MapImportCellTip);
            }

            var ddlMode = e.Row.FindControl("ddlImportMode") as DropDownList;
            if (ddlMode != null && row.IsParentGroup)
            {
                ddlMode.ToolTip = MessageProvider.Get(MessageKeys.WooCommerce.MapModeCellTip);
                // Populate once so ViewState keeps the user's selection across Save postbacks.
                if (ddlMode.Items.Count == 0)
                {
                    ddlMode.Items.Add(new ListItem(
                        MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeVariants),
                        WooProductMapRow.ImportModeVariants));
                    ddlMode.Items.Add(new ListItem(
                        MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeParentItem),
                        WooProductMapRow.ImportModeParentItem));
                    ddlMode.Items.Add(new ListItem(
                        MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeParentNotes),
                        WooProductMapRow.ImportModeParentNotes));
                    ddlMode.Items.Add(new ListItem(
                        MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeExclude),
                        WooProductMapRow.ImportModeExclude));
                }
                if (ddlMode.Items.FindByValue(row.ImportMode) != null)
                    ddlMode.SelectedValue = row.ImportMode;
            }

            var ddl = e.Row.FindControl("ddlItem") as DropDownList;
            if (ddl != null)
                ddl.ToolTip = MessageProvider.Get(MessageKeys.WooCommerce.MapDestCellTip);
            var ddlPack = e.Row.FindControl("ddlPack") as DropDownList;
            if (ddl != null && ddl.Visible)
            {
                ddl.AutoPostBack = true;
                ddl.Items.Clear();
                foreach (ListItem li in GetItemDropdownTemplate())
                    ddl.Items.Add(new ListItem(li.Text, li.Value));

                if (row.IsParentGroup)
                {
                    var variantsItem = ddl.Items.FindByValue("0");
                    if (variantsItem != null)
                        variantsItem.Text = "(variants)";
                }

                EnsureCreateDestinationItem(ddl, row);

                int selected = row.MapToNotes || row.ImportParentAsNotes
                    ? WooProductMapRow.DestinationNotesValue
                    : (row.MappedItemID == WooProductMapRow.DestinationCreateParent
                        ? WooProductMapRow.DestinationCreateParent
                        : (row.MappedItemID > 0 ? row.MappedItemID : row.SuggestedItemID));
                if (row.ImportParentAsItem && selected == 0)
                    selected = WooProductMapRow.DestinationCreateParent;
                // Notes is -1; do not treat it as unmapped or it snaps back to Create.
                if (!row.IsParentGroup
                    && selected == 0
                    && row.HasOwnSku
                    && row.SuggestedItemID <= 0
                    && row.ExistingMappingID <= 0
                    && !row.MapToNotes
                    && !row.ImportParentAsItem
                    && !row.ImportParentAsNotes
                    && !row.ImportParentExcluded)
                    selected = WooProductMapRow.DestinationCreateParent;
                if (row.IsParentGroup && !row.ParentMapsAsDestination && !row.ImportParentExcluded)
                {
                    selected = 0;
                    // Keep session MappedItemID aligned with the "(variants)" destination control.
                    if (row.MappedItemID != 0)
                        row.MappedItemID = 0;
                }
                if (ddl.Items.FindByValue(selected.ToString()) != null)
                    ddl.SelectedValue = selected.ToString();

                if (selected == WooProductMapRow.DestinationCreateParent)
                    SeedCreateSkuIfNeeded(row);
                else if (selected > 0 && !row.CreateSkuUserSet)
                    SyncItemSkuFromDestination(row);

                var txtCreate = e.Row.FindControl("txtItemSku") as TextBox;
                if (txtCreate != null && txtCreate.Visible)
                    txtCreate.Text = row.CreateSku ?? string.Empty;

                bool notes = selected == WooProductMapRow.DestinationNotesValue;
                int? serviceTypeId = null;
                if (!notes && selected > 0)
                {
                    var item = _itemsRepo.GetById(selected);
                    if (item != null)
                        serviceTypeId = item.ItemServiceTypeID;
                }
                if (ddlPack != null)
                {
                    FillPackagingDropdown(ddlPack, notes ? null : (row.PackagingID ?? row.SuggestedPackagingID), serviceTypeId, compact: true);
                    ddlPack.Enabled = !notes;
                    SyncCompactPackTooltip(ddlPack);
                }
                var txtQty = e.Row.FindControl("txtQty") as TextBox;
                if (txtQty != null)
                {
                    txtQty.Enabled = !notes;
                    txtQty.ToolTip = MessageProvider.Get(MessageKeys.WooCommerce.MapColQtyTip);
                }
            }

            var txtItemSku = e.Row.FindControl("txtItemSku") as TextBox;
            if (txtItemSku != null && txtItemSku.Visible)
                txtItemSku.ToolTip = MessageProvider.Get(MessageKeys.WooCommerce.MapColItemSkuTip);

            MarkPullRowOriginals(e.Row);
        }

        private static void MarkPullRowOriginals(GridViewRow row)
        {
            var chkImport = row.FindControl("chkImport") as CheckBox;
            if (chkImport != null)
                MarkOriginal(chkImport, chkImport.Checked);
            MarkOriginal(row.FindControl("ddlImportMode") as DropDownList);
            MarkOriginal(row.FindControl("ddlItem") as DropDownList);
            MarkOriginal(row.FindControl("txtItemSku") as TextBox);
            MarkOriginal(row.FindControl("ddlSortOrder") as DropDownList);
            MarkOriginal(row.FindControl("txtQty") as TextBox);
            MarkOriginal(row.FindControl("ddlPack") as DropDownList);
        }

        private static void EnsureCreateDestinationItem(DropDownList ddl, WooProductMapRow row)
        {
            if (ddl == null || row == null)
                return;

            string createVal = WooProductMapRow.DestinationCreateParent.ToString();
            var existing = ddl.Items.FindByValue(createVal);
            if (existing != null)
                ddl.Items.Remove(existing);

            if (!row.IsParentGroup && (row.ImportParentAsItem || row.ImportParentAsNotes || row.ImportParentExcluded))
                return;

            string sku;
            string createText;
            if (row.IsParentGroup)
            {
                sku = row.DisplaySku;
                if (string.IsNullOrWhiteSpace(sku) || string.Equals(sku, "(parent)", StringComparison.Ordinal))
                    return;
                createText = MessageProvider.Get(MessageKeys.WooCommerce.MapDestCreateParent);
                if (string.IsNullOrWhiteSpace(createText)
                    || string.Equals(createText, MessageKeys.WooCommerce.MapDestCreateParent, StringComparison.Ordinal))
                    createText = "Create Tracker item (parent SKU)";
            }
            else
            {
                if (!row.HasOwnSku)
                    return;
                sku = row.Sku.Trim();
                createText = MessageProvider.Format(MessageKeys.WooCommerce.MapDestCreateVariant, sku);
                if (string.IsNullOrWhiteSpace(createText)
                    || createText.IndexOf("{0}", StringComparison.Ordinal) >= 0
                    || string.Equals(createText, MessageKeys.WooCommerce.MapDestCreateVariant, StringComparison.Ordinal))
                    createText = "Create Tracker item (" + sku + ")";
            }

            ddl.Items.Insert(0, new ListItem(createText, createVal));
        }

        private void MergeVisibleMissingSkuEdits()
        {
            var all = GetMissingSkuRows();
            if (all.Count == 0 || gvMissingSku.Rows.Count == 0)
                return;

            var byKey = all.ToDictionary(r => r.RowKey, StringComparer.Ordinal);
            foreach (GridViewRow gvRow in gvMissingSku.Rows)
            {
                if (gvRow.RowType != DataControlRowType.DataRow)
                    continue;
                string key = Convert.ToString(gvMissingSku.DataKeys[gvRow.RowIndex].Value);
                WooProductMapRow row;
                if (string.IsNullOrEmpty(key) || !byKey.TryGetValue(key, out row))
                    continue;

                var chk = gvRow.FindControl("chkMissingApply") as CheckBox;
                var txt = gvRow.FindControl("txtNewSku") as TextBox;
                if (chk != null)
                    row.ApplySelected = chk.Checked;
                if (txt != null)
                {
                    row.NewSku = txt.Text;
                    // Typing a SKU is enough intent to write (Apply may have been left unticked).
                    if (!string.IsNullOrWhiteSpace(txt.Text))
                        row.ApplySelected = true;
                }
            }
            Session[SessionMissingSku] = all;
        }

        protected void gvMissingSku_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as WooProductMapRow;
            if (row != null && row.IsVariation)
                e.Row.CssClass = (e.Row.CssClass + " woo-map-row-variant").Trim();
            var chk = e.Row.FindControl("chkMissingApply") as CheckBox;
            if (chk != null)
                MarkOriginal(chk, chk.Checked);
            var txtSku = e.Row.FindControl("txtNewSku") as TextBox;
            if (txtSku != null)
                MarkOriginal(txtSku, txtSku.Text);
        }

        protected void btnSaveSelectedMaps_Click(object sender, EventArgs e)
        {
            if (hdnPullSaveOk != null)
                hdnPullSaveOk.Value = "0";
            MergeVisiblePullEdits();
            var rows = GetAllPullRows();
            var selected = rows.Where(r => r != null && r.NeedsPersist).ToList();
            if (selected.Count == 0)
            {
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapSaveSelectedNone), true);
                SyncSaveSelectedButton();
                return;
            }

            int saved = 0;
            int created = 0;
            int linkedExisting = 0;
            var errors = new List<string>();

            try
            {
                var createKeys = new HashSet<string>(StringComparer.Ordinal);
                foreach (var src in selected)
                {
                    if (src.ImportParentAsNotes)
                        continue;
                    if (src.ImportParentExcluded)
                        continue;
                    if (src.IsParentGroup && !src.ImportParentAsItem)
                        continue;
                    if (!src.IsParentGroup && src.ImportParentAsItem)
                        continue;
                    bool mapToNotes = src.MapToNotes || src.MappedItemID == WooProductMapRow.DestinationNotesValue;
                    if (mapToNotes)
                        continue;
                    if (src.MappedItemID > 0 || src.SuggestedItemID > 0)
                        continue;
                    if (!src.NeedsTrackerItem && !src.IsCreateParentDestination)
                        continue;
                    if (!createKeys.Add(src.RowKey))
                        continue;

                    try
                    {
                        bool wasCreated;
                        var item = _manager.CreateTrackerItemForWooRow(src, UserName(), out wasCreated);
                        if (wasCreated)
                            created++;
                        else
                            linkedExisting++;
                        src.SuggestedItemID = item.ItemID;
                        src.SuggestedItemDesc = item.ItemDesc;
                        src.MappedItemID = item.ItemID;
                        src.MatchReason = wasCreated
                            ? "Created Tracker item"
                            : "Linked existing Tracker item (same SKU)";
                        if (!src.IsParentGroup)
                            continue;
                        foreach (var r in rows)
                        {
                            if (r.WooProductId != src.WooProductId)
                                continue;
                            if (r.RowKey == src.RowKey)
                                continue;
                            if (r.MapToNotes || r.ImportParentAsNotes)
                                continue;
                            if (r.IsParentGroup && !r.ImportParentAsItem)
                                continue;
                            if (!r.IsParentGroup && r.ImportParentAsItem)
                                continue;
                            if (r.ExistingMappingID > 0 && r.MappedItemID > 0)
                                continue;
                            r.SuggestedItemID = item.ItemID;
                            r.SuggestedItemDesc = item.ItemDesc;
                            r.MappedItemID = item.ItemID;
                            r.MatchReason = src.MatchReason;
                        }
                    }
                    catch (Exception createEx)
                    {
                        errors.Add((src.DisplaySku ?? src.Sku) + ": " + createEx.Message);
                    }
                }

                if (created > 0 || linkedExisting > 0)
                    BindItemDropdowns();

                foreach (var src in selected)
                {
                    try
                    {
                        if (src.IsParentGroup && !src.ParentUsesApply && !src.ImportModeUserSet)
                        {
                            // Variants parent dirty only from UI/session drift — nothing to write.
                            if (src.IsDirtyVsSaved)
                                src.CaptureSavedSnapshot();
                            src.ApplySelected = false;
                            continue;
                        }
                        if (!src.IsParentGroup && (src.ImportParentAsNotes || src.ImportParentAsItem || src.ImportParentExcluded))
                            continue;

                        if (src.ImportParentExcluded)
                        {
                            int excludeId = _manager.SaveMapping(
                                src.WooProductId,
                                null,
                                0,
                                false,
                                false,
                                1,
                                null,
                                TruncateSkuPattern(src.DisplaySku),
                                UserName(),
                                exclude: true);
                            _manager.ExcludeProductFromImport(src.WooProductId);
                            src.ExistingMappingID = excludeId;
                            src.ImportMode = WooProductMapRow.ImportModeExclude;
                            src.ImportModeUserSet = false;
                            src.UsesCategoryImportDefault = false;
                            src.IncludeInImport = false;
                            src.MatchReason = "Do not import";
                            src.ApplySelected = false;
                            src.CaptureSavedSnapshot();
                            saved++;
                            continue;
                        }

                        if (src.IsParentGroup && !src.ParentMapsAsDestination)
                        {
                            // Persist Variants so a category default (e.g. Parent → notes) cannot restore on reload.
                            int variantsId = _manager.SaveMapping(
                                src.WooProductId,
                                null,
                                0,
                                false,
                                false,
                                1,
                                null,
                                TruncateSkuPattern(src.DisplaySku),
                                UserName(),
                                variantsParent: true);
                            src.ExistingMappingID = variantsId;
                            src.ImportMode = WooProductMapRow.ImportModeVariants;
                            src.ImportModeUserSet = false;
                            src.UsesCategoryImportDefault = false;
                            src.ApplySelected = false;
                            src.MappedItemID = 0;
                            src.MapToNotes = false;
                            src.IncludeInImport = false;
                            src.MatchReason = "Import variants";
                            src.CaptureSavedSnapshot();
                            saved++;
                            continue;
                        }

                        // Destination / Mode from Merge are authoritative. Do not infer Notes from a
                        // stale MappedItemID==-1 while MapToNotes is false (that skipped SKU/S/O writes).
                        bool mapToNotes;
                        if (src.IsParentGroup)
                            mapToNotes = src.ImportParentAsNotes;
                        else if (src.MappedItemID > 0 || src.IsCreateParentDestination)
                            mapToNotes = false;
                        else
                            mapToNotes = src.MapToNotes
                                || src.MappedItemID == WooProductMapRow.DestinationNotesValue;
                        if (mapToNotes && src.IsParentGroup)
                            src.IncludeInImport = true;

                        // Still on Create destination after create-phase? Try once more, then fail clearly.
                        if (!mapToNotes && src.IsCreateParentDestination)
                        {
                            try
                            {
                                bool wasCreated;
                                var item = _manager.CreateTrackerItemForWooRow(src, UserName(), out wasCreated);
                                if (wasCreated)
                                    created++;
                                else
                                    linkedExisting++;
                                src.SuggestedItemID = item.ItemID;
                                src.SuggestedItemDesc = item.ItemDesc;
                                src.MappedItemID = item.ItemID;
                                src.MatchReason = wasCreated
                                    ? "Created Tracker item"
                                    : "Linked existing Tracker item (same SKU)";
                            }
                            catch (Exception createEx)
                            {
                                errors.Add((src.DisplaySku ?? src.Sku) + ": " + createEx.Message);
                                continue;
                            }
                        }

                        int itemId = mapToNotes ? 0 : (src.MappedItemID > 0 ? src.MappedItemID : src.SuggestedItemID);
                        if (!mapToNotes && itemId <= 0)
                        {
                            errors.Add((src.DisplaySku ?? src.Sku) + ": no destination (choose a Tracker item, Create, or Parent → notes)");
                            continue;
                        }

                        string skuPattern = mapToNotes
                            ? TruncateSkuPattern(src.NotesDescription)
                            : TruncateSkuPattern(string.IsNullOrWhiteSpace(src.Sku) ? src.DisplaySku : src.Sku);
                        if (mapToNotes && string.IsNullOrWhiteSpace(skuPattern))
                            skuPattern = TruncateSkuPattern(src.DisplaySku);

                        int mappingId = _manager.SaveMapping(
                            src.WooProductId,
                            src.IsParentGroup ? null : src.WooVariationId,
                            itemId,
                            mapToNotes,
                            src.IncludeInImport,
                            src.QtyFactor,
                            src.PackagingID,
                            skuPattern,
                            UserName());

                        if (mappingId <= 0)
                        {
                            errors.Add((src.DisplaySku ?? src.Sku) + ": save returned no mapping id");
                            continue;
                        }

                        src.ExistingMappingID = mappingId;
                        src.MapToNotes = mapToNotes;
                        src.MappedItemID = mapToNotes ? WooProductMapRow.DestinationNotesValue : itemId;
                        src.ImportModeUserSet = false;
                        src.UsesCategoryImportDefault = false;
                        src.MatchReason = mapToNotes ? (src.IsParentGroup ? "Parent → order notes" : "Order notes") : "Saved mapping";
                        src.ApplySelected = false;
                        saved++;
                        // Write Item SKU / S/O after the map so a sort-only edit is not lost on rebuild.
                        if (!mapToNotes && itemId > 0)
                        {
                            try
                            {
                                _manager.ApplySkuAndSort(itemId, src.CreateSku, src.CreateSortOrder, UserName());
                                src.CreateSkuUserSet = false;
                            }
                            catch (Exception skuEx)
                            {
                                errors.Add((src.DisplaySku ?? src.Sku) + ": " + skuEx.Message);
                            }
                        }
                        else
                            src.CreateSkuUserSet = false;
                        src.CaptureSavedSnapshot();
                    }
                    catch (Exception mapEx)
                    {
                        errors.Add((src.DisplaySku ?? src.Sku) + ": " + mapEx.Message);
                    }
                }

                Session[SessionPull] = rows;
                if (!rows.Any(r => r != null && (r.IsDirtyVsSaved || r.ImportModeUserSet)))
                    ClearPullDirty();
                bool ok = saved > 0 || created > 0;
                if (ok)
                {
                    if (hdnPullSaveOk != null)
                        hdnPullSaveOk.Value = "1";
                    ClearUnsavedFlag(hdnUnsavedPull);
                    ClearPullDirty();
                    Session[SessionMissingSku] = null;
                    RebuildPullRowsPreservingUserModes();
                }
                else if (errors.Count == 0
                    && !rows.Any(r => r != null && (r.IsDirtyVsSaved || r.ImportModeUserSet)))
                {
                    // Pencils cleared with nothing to write (e.g. Variants parent drift).
                    ok = true;
                    if (hdnPullSaveOk != null)
                        hdnPullSaveOk.Value = "1";
                    ClearUnsavedFlag(hdnUnsavedPull);
                }
                RebindPull();
                BindExistingMaps();
                string msg = ok && saved == 0 && created == 0 && linkedExisting == 0 && errors.Count == 0
                    ? "Saved — mapping grid is up to date."
                    : MessageProvider.Format(MessageKeys.WooCommerce.MapSaveSelectedOk, saved, created);
                if (linkedExisting > 0)
                    msg += " Linked " + linkedExisting + " existing Tracker item(s) by SKU.";
                if (errors.Count > 0)
                    msg += " " + string.Join("; ", errors.Take(5));
                SetStatus(msg, errors.Count > 0 || !ok);
                WooCommerceUserLog.Write(
                    "Mapping save selected",
                    string.Format(CultureInfo.InvariantCulture,
                        "saved={0}, created={1}, linked={2}, errors={3}",
                        saved, created, linkedExisting, errors.Count),
                    UserName());
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private static string TruncateSkuPattern(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            const int max = 255;
            string t = value.Trim();
            return t.Length <= max ? t : t.Substring(0, max);
        }

        protected void btnWriteMissingSkus_Click(object sender, EventArgs e)
        {
            MergeVisibleMissingSkuEdits();
            var rows = GetMissingSkuRows();
            // Keep typed NewSku values so a failed write does not wipe the grid.
            var pendingSkus = rows
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.NewSku))
                .ToDictionary(r => r.RowKey, r => new { r.NewSku, r.ApplySelected }, StringComparer.Ordinal);

            var result = _manager.WriteMissingSkusToWoo(rows, UserName());
            Session[SessionPull] = null;
            Session[SessionMissingSku] = null;
            EnsurePullRowsLoaded();

            // Re-apply typed SKUs onto rebuilt missing list when write failed or partial.
            if (!result.Succeeded || result.Count == 0)
            {
                var missing = GetMissingSkuRows();
                foreach (var row in missing)
                {
                    if (row == null || string.IsNullOrEmpty(row.RowKey))
                        continue;
                    if (!pendingSkus.ContainsKey(row.RowKey))
                        continue;
                    var keep = pendingSkus[row.RowKey];
                    row.NewSku = keep.NewSku;
                    row.ApplySelected = keep.ApplySelected;
                }
                Session[SessionMissingSku] = missing;
            }

            gvPull.PageIndex = 0;
            gvMissingSku.PageIndex = 0;
            RebindMissingSku();
            RebindPull();
            SyncSaveSelectedButton();
            SyncWriteMissingButton();
            if (result.Succeeded)
                ClearUnsavedFlag(hdnUnsavedMissing);
            SetStatus(result.Message, result.Succeeded ? StatusKind.Success
                : (result.Count > 0 ? StatusKind.Warn : StatusKind.Error));
            if (!result.Succeeded && !string.IsNullOrWhiteSpace(result.Message))
            {
                string js = "window.setTimeout(function(){ window.alert("
                    + HttpUtility.JavaScriptStringEncode(result.Message, addDoubleQuotes: true)
                    + "); }, 50);";
                ScriptManager.RegisterStartupScript(this, GetType(), "wooMapSkuWriteErr", js, true);
            }
        }

        private void BindExistingMaps()
        {
            var maps = _manager.GetMappings() ?? new List<WooItemMapping>();
            string notesLabel = MessageProvider.Get(MessageKeys.WooCommerce.MapDestNotes);
            foreach (var m in maps)
            {
                if (m.IsVariantsMap)
                {
                    if (string.IsNullOrWhiteSpace(m.ItemSku))
                        m.ItemSku = "-";
                    m.ItemDesc = MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeVariants);
                    continue;
                }
                if (m.IsExcludeMap)
                {
                    if (string.IsNullOrWhiteSpace(m.ItemSku))
                        m.ItemSku = "-";
                    m.ItemDesc = MessageProvider.Get(MessageKeys.WooCommerce.MapImportModeExclude);
                    continue;
                }
                if (!m.IsNotesMap)
                    continue;
                if (string.IsNullOrWhiteSpace(m.ItemSku))
                    m.ItemSku = "-";
                m.ItemDesc = notesLabel;
            }

            int total = maps.Count;
            string find = Session[SessionSavedMapsFind] as string;
            if (!string.IsNullOrEmpty(find) && string.IsNullOrEmpty(txtFindSavedMap.Text))
                txtFindSavedMap.Text = find;
            find = (txtFindSavedMap.Text ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(find))
            {
                Session[SessionSavedMapsFind] = find;
                maps = FilterSavedMaps(maps, find);
            }
            else
                Session[SessionSavedMapsFind] = null;

            maps = SortSavedMaps(maps);

            if (gvMaps.PageIndex > 0 && maps.Count <= gvMaps.PageIndex * gvMaps.PageSize)
                gvMaps.PageIndex = 0;
            gvMaps.DataSource = maps;
            gvMaps.DataBind();
            if (litSavedMapsPageInfo != null)
            {
                int pageSize = gvMaps.PageSize > 0 ? gvMaps.PageSize : 40;
                int page = gvMaps.PageIndex + 1;
                int pages = maps.Count == 0 ? 1 : (int)Math.Ceiling(maps.Count / (double)pageSize);
                if (!string.IsNullOrEmpty(find))
                {
                    litSavedMapsPageInfo.Text = MessageProvider.Format(
                        MessageKeys.WooCommerce.MapSavedMapsFilteredInfo,
                        maps.Count, total, find, page, pages);
                }
                else
                {
                    litSavedMapsPageInfo.Text = MessageProvider.Format(
                        MessageKeys.WooCommerce.MapSavedMapsPageInfo, maps.Count, page, pages);
                }
            }
        }

        private static List<WooItemMapping> FilterSavedMaps(List<WooItemMapping> maps, string find)
        {
            if (maps == null || maps.Count == 0 || string.IsNullOrWhiteSpace(find))
                return maps ?? new List<WooItemMapping>();
            string needle = find.Trim();
            return maps.Where(m =>
            {
                if (m == null) return false;
                if (m.MappingID.ToString(CultureInfo.InvariantCulture).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (!string.IsNullOrEmpty(m.MapType) && m.MapType.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (!string.IsNullOrEmpty(m.ItemSku) && m.ItemSku.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (!string.IsNullOrEmpty(m.ItemDesc) && m.ItemDesc.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (!string.IsNullOrEmpty(m.SkuPattern) && m.SkuPattern.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (!string.IsNullOrEmpty(m.WooProductLabel) && m.WooProductLabel.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (!string.IsNullOrEmpty(m.WooVariationLabel) && m.WooVariationLabel.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (m.WooProductId.HasValue
                    && m.WooProductId.Value.ToString(CultureInfo.InvariantCulture).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (m.WooVariationId.HasValue
                    && m.WooVariationId.Value.ToString(CultureInfo.InvariantCulture).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                return false;
            }).ToList();
        }

        private List<WooItemMapping> SortSavedMaps(List<WooItemMapping> maps)
        {
            if (maps == null || maps.Count == 0)
                return maps ?? new List<WooItemMapping>();
            string expr = Session[SessionSavedMapsSort] as string;
            if (string.IsNullOrEmpty(expr))
                expr = "MappingID";
            bool desc = string.Equals(Session[SessionSavedMapsSortDir] as string, "DESC", StringComparison.OrdinalIgnoreCase);
            Func<WooItemMapping, object> key;
            switch (expr)
            {
                case "MapType":
                    key = m => m.MapType ?? string.Empty;
                    break;
                case "IncludeInImport":
                    key = m => m.IncludeInImport ? 1 : 0;
                    break;
                case "ItemSku":
                    key = m => m.ItemSku ?? string.Empty;
                    break;
                case "ItemDesc":
                    key = m => m.ItemDesc ?? string.Empty;
                    break;
                case "SkuPattern":
                    key = m => m.SkuPattern ?? string.Empty;
                    break;
                case "WooProductId":
                case "WooProductLabel":
                    key = m => m.WooProductLabel
                        ?? (m.WooProductId.HasValue ? m.WooProductId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                    break;
                case "WooVariationId":
                case "WooVariationLabel":
                    key = m => m.WooVariationLabel
                        ?? (m.WooVariationId.HasValue && m.WooVariationId.Value > 0
                            ? m.WooVariationId.Value.ToString(CultureInfo.InvariantCulture)
                            : string.Empty);
                    break;
                case "QtyFactor":
                    key = m => m.QtyFactor;
                    break;
                case "PackagingDesc":
                case "PackagingID":
                    key = m => m.PackagingDesc ?? (m.PackagingID.HasValue ? m.PackagingID.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                    break;
                case "LastWooStatus":
                    key = m => m.LastWooStatus ?? string.Empty;
                    break;
                default:
                    key = m => m.MappingID;
                    break;
            }
            return desc
                ? maps.OrderByDescending(key).ToList()
                : maps.OrderBy(key).ToList();
        }

        protected static string FormatWooProductCell(object dataItem)
        {
            var m = dataItem as WooItemMapping;
            if (m == null)
                return "-";
            if (!string.IsNullOrWhiteSpace(m.WooProductLabel))
                return m.WooProductLabel.Trim();
            if (m.WooProductId.HasValue && m.WooProductId.Value > 0)
                return "#" + m.WooProductId.Value.ToString(CultureInfo.InvariantCulture);
            return "-";
        }

        protected static string FormatWooVariationCell(object dataItem)
        {
            var m = dataItem as WooItemMapping;
            if (m == null)
                return "-";
            if (!m.WooVariationId.HasValue || m.WooVariationId.Value <= 0)
                return "-";
            if (!string.IsNullOrWhiteSpace(m.WooVariationLabel))
                return m.WooVariationLabel.Trim();
            return "#" + m.WooVariationId.Value.ToString(CultureInfo.InvariantCulture);
        }

        protected void btnFindSavedMap_Click(object sender, EventArgs e)
        {
            Session[SessionSavedMapsFind] = (txtFindSavedMap.Text ?? string.Empty).Trim();
            gvMaps.PageIndex = 0;
            BindExistingMaps();
        }

        protected void btnClearFindSavedMap_Click(object sender, EventArgs e)
        {
            txtFindSavedMap.Text = string.Empty;
            Session[SessionSavedMapsFind] = null;
            gvMaps.PageIndex = 0;
            BindExistingMaps();
        }

        protected void gvMaps_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvMaps, e.Row);
        }

        protected void gvMaps_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMaps.PageIndex = e.NewPageIndex;
            BindExistingMaps();
        }

        protected void gvMaps_Sorting(object sender, GridViewSortEventArgs e)
        {
            string current = Session[SessionSavedMapsSort] as string;
            string dir = Session[SessionSavedMapsSortDir] as string;
            if (string.Equals(current, e.SortExpression, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(dir, "DESC", StringComparison.OrdinalIgnoreCase))
                Session[SessionSavedMapsSortDir] = "DESC";
            else
                Session[SessionSavedMapsSortDir] = "ASC";
            Session[SessionSavedMapsSort] = e.SortExpression;
            gvMaps.PageIndex = 0;
            BindExistingMaps();
        }

        protected void gvMaps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DelMap")
                return;
            int id = Convert.ToInt32(e.CommandArgument);
            _manager.DeleteMapping(id, UserName());
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapDeleted), false);
            // Refresh configure grid too so Saved flags clear.
            Session[SessionPull] = null;
            Session[SessionMissingSku] = null;
            EnsurePullRowsLoaded();
            BindExistingMaps();
        }

        protected void btnDryPush_Click(object sender, EventArgs e)
        {
            var result = _manager.PushEnabledState(UserName(), dryRun: true);
            Session[SessionEnabledPush] = result.Rows;
            BindEnabledPushGrid();
            SetStatus(result.Message, !result.Succeeded);
        }

        protected void btnPushEnabled_Click(object sender, EventArgs e)
        {
            var result = _manager.PushEnabledState(UserName(), dryRun: false);
            Session[SessionEnabledPush] = result.Rows;
            BindEnabledPushGrid();
            SetStatus(result.Message, result.FailCount > 0);
            BindExistingMaps();
        }

        private void BindEnabledPushGrid()
        {
            var rows = Session[SessionEnabledPush] as List<WooEnabledPushPreviewRow>
                ?? new List<WooEnabledPushPreviewRow>();
            if (gvEnabledPush.PageIndex > 0)
            {
                int pages = rows.Count == 0 ? 1 : (int)Math.Ceiling(rows.Count / (double)gvEnabledPush.PageSize);
                if (gvEnabledPush.PageIndex >= pages)
                    gvEnabledPush.PageIndex = Math.Max(0, pages - 1);
            }
            gvEnabledPush.DataSource = rows;
            gvEnabledPush.DataBind();
            if (litEnabledPushSummary != null)
            {
                if (rows.Count == 0)
                    litEnabledPushSummary.Text = "<p class=\"woo-map-page-info\">Run dry-run to list mapped products and what Woo status would be set from Lookups → Items Enbld.</p>";
                else
                {
                    int willChange = rows.Count(r => r != null && r.NeedsChange);
                    int unchanged = rows.Count - willChange;
                    litEnabledPushSummary.Text = "<p class=\"woo-map-page-info\">"
                        + Server.HtmlEncode(rows.Count + " mapped row(s): "
                            + willChange + " would change Woo, "
                            + unchanged + " already match.")
                        + "</p>";
                }
            }
        }

        protected void gvEnabledPush_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEnabledPush.PageIndex = e.NewPageIndex;
            BindEnabledPushGrid();
        }

        protected void gvEnabledPush_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvEnabledPush, e.Row);
        }

        protected void gvEnabledPush_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as WooEnabledPushPreviewRow;
            if (row == null)
                return;
            if (row.NeedsChange)
                e.Row.CssClass = (e.Row.CssClass + " woo-map-row-dirty").Trim();
            else if (!row.TrackerEnabled)
                e.Row.CssClass = (e.Row.CssClass + " woo-map-row-disabled").Trim();
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Tools/SystemPreferences.aspx");
        }

        private void FillPackagingDropdown(DropDownList ddl, int? selectedId, int? itemServiceTypeId, bool compact = false)
        {
            ddl.Items.Clear();
            var none = new ListItem(compact ? "-" : "(none)", "0");
            if (compact)
            {
                none.Attributes["data-compact"] = "-";
                none.Attributes["data-full"] = "(none)";
            }
            ddl.Items.Add(none);
            var packs = GetPackagingsForServiceTypeCached(itemServiceTypeId);
            foreach (var p in packs)
            {
                string full = string.IsNullOrWhiteSpace(p.Symbol)
                    ? (p.ItemPackagingDesc ?? p.ItemPackagingID.ToString())
                    : (p.ItemPackagingDesc + " [" + p.Symbol + "]");
                string text = compact ? CompactPackagingLabel(p) : full;
                var item = new ListItem(text, p.ItemPackagingID.ToString());
                if (compact)
                {
                    item.Attributes["data-compact"] = text;
                    item.Attributes["data-full"] = full;
                }
                ddl.Items.Add(item);
            }
            if (selectedId.HasValue && selectedId.Value > 0
                && ddl.Items.FindByValue(selectedId.Value.ToString()) != null)
            {
                ddl.SelectedValue = selectedId.Value.ToString();
            }
        }

        private static string CompactPackagingLabel(ItemPackaging p)
        {
            if (p == null)
                return "-";
            if (!string.IsNullOrWhiteSpace(p.Symbol))
                return p.Symbol.Trim();
            string desc = (p.ItemPackagingDesc ?? string.Empty).Trim();
            if (desc.Length <= 6)
                return desc;
            return desc.Substring(0, 5) + "…";
        }

        private static void SyncCompactSortOrderTooltip(DropDownList ddl)
        {
            if (ddl?.SelectedItem == null)
                return;
            string title = ddl.SelectedItem.Attributes["data-full"];
            if (string.IsNullOrWhiteSpace(title))
                title = ddl.SelectedItem.Attributes["title"];
            ddl.ToolTip = string.IsNullOrWhiteSpace(title) ? ddl.SelectedItem.Text : title;
        }

        private static void SyncCompactPackTooltip(DropDownList ddl)
        {
            if (ddl?.SelectedItem == null)
                return;
            string title = ddl.SelectedItem.Attributes["data-full"];
            if (string.IsNullOrWhiteSpace(title))
                title = ddl.SelectedItem.Attributes["title"];
            ddl.ToolTip = string.IsNullOrWhiteSpace(title) ? ddl.SelectedItem.Text : title;
        }

        private List<ItemPackaging> GetPackagingsForServiceTypeCached(int? itemServiceTypeId)
        {
            int key = itemServiceTypeId.HasValue && itemServiceTypeId.Value > 0 ? itemServiceTypeId.Value : 0;
            List<ItemPackaging> cached;
            if (_packagingsByServiceType.TryGetValue(key, out cached))
                return cached;

            cached = _manager.GetPackagingsForServiceType(key > 0 ? (int?)key : null);
            _packagingsByServiceType[key] = cached;
            return cached;
        }

        private void FillServiceTypeDropdown(DropDownList ddl, int selectedId)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("(all types)", "0"));
            var types = _svcRepo.GetAll("ItemServiceTypeName") ?? new List<ItemServiceType>();
            foreach (var t in types)
            {
                ddl.Items.Add(new ListItem(t.ItemServiceTypeName ?? t.ItemServiceTypeID.ToString(), t.ItemServiceTypeID.ToString()));
            }
            if (ddl.Items.FindByValue(selectedId.ToString()) != null)
                ddl.SelectedValue = selectedId.ToString();
        }

        private static void MarkOriginal(CheckBox chk, bool value)
        {
            if (chk != null)
                chk.InputAttributes["data-original"] = value ? "1" : "0";
        }

        private static void MarkOriginal(ListControl ddl)
        {
            if (ddl != null)
                ddl.Attributes["data-original"] = ddl.SelectedValue ?? string.Empty;
        }

        private static void MarkOriginal(ListControl ddl, string value)
        {
            if (ddl != null)
                ddl.Attributes["data-original"] = value ?? string.Empty;
        }

        private static void MarkOriginal(TextBox txt)
        {
            if (txt != null)
                txt.Attributes["data-original"] = txt.Text ?? string.Empty;
        }

        private static void MarkOriginal(TextBox txt, string value)
        {
            if (txt != null)
                txt.Attributes["data-original"] = value ?? string.Empty;
        }

        #region Areas / Shipping tabs

        private const string VsWooAreaRows = "WooMap.AreaRows";
        private const string VsWooAreaFilter = "WooMap.AreaFilter";

        private void BindAreasTab()
        {
            _personDropdownTemplate = null;
            BindDefaultImportAreaDropdown();
            BindAddressConfigPanel();
            ReloadAreaWorkingRows();
            BindAreaDefaultsGrid();
            BindSystemDefaultPersonHint();
            litTestResolveResult.Text = string.Empty;
        }

        private void BindShippingTab()
        {
            _personDropdownTemplate = null;
            var maps = _areaManager.GetShippingMethodMaps() ?? new List<WooShippingMethodMap>();
            bool empty = maps.Count == 0;
            if (pnlShippingEmptyHint != null)
                pnlShippingEmptyHint.Visible = empty;
            // GridView hides the footer when there are no rows — seed a blank draft so the user can add the first map.
            if (empty)
            {
                maps.Add(new WooShippingMethodMap
                {
                    MapID = 0,
                    MethodMatch = string.Empty,
                    IsActive = true
                });
            }
            gvShippingMaps.DataSource = maps;
            gvShippingMaps.DataBind();
            BindDispatchWaybillPanel();
        }

        private const string VsDispatchSelected = "WooMap.DispatchSelectedIds";

        private void BindDispatchWaybillPanel()
        {
            var settings = _settings.GetSettings() ?? new WooCommerceSettings();
            EnsureDispatchSelectedLoaded(settings);

            var selected = GetDispatchSelectedIds();
            var rows = new List<DispatchPersonRow>();
            foreach (var person in _orderManager.GetDeliveryPersons() ?? new List<Person>())
            {
                string abbr = (person.Abbreviation ?? string.Empty).Trim();
                string name = (person.PersonName ?? string.Empty).Trim();
                string label;
                if (!string.IsNullOrEmpty(abbr) && !string.IsNullOrEmpty(name)
                    && !string.Equals(abbr, name, StringComparison.OrdinalIgnoreCase))
                    label = abbr + " — " + name;
                else if (!string.IsNullOrEmpty(abbr))
                    label = abbr;
                else if (!string.IsNullOrEmpty(name))
                    label = name;
                else
                    label = "#" + person.PersonID.ToString(CultureInfo.InvariantCulture);

                rows.Add(new DispatchPersonRow
                {
                    PersonID = person.PersonID,
                    DisplayName = label,
                    UseWaybill = selected.Contains(person.PersonID)
                });
            }

            gvDispatchPeople.DataSource = rows;
            gvDispatchPeople.DataBind();
            chkTrackingNumberRequired.Checked = settings.TrackingNumberRequired;
        }

        private void EnsureDispatchSelectedLoaded(WooCommerceSettings settings)
        {
            if (ViewState[VsDispatchSelected] != null)
                return;

            var selected = new List<int>();
            string ids = settings?.DispatchDeliveryPersonIds ?? string.Empty;
            foreach (string part in ids.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int id;
                if (int.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id) && id > 0)
                    selected.Add(id);
            }
            ViewState[VsDispatchSelected] = selected;
        }

        private List<int> GetDispatchSelectedIds()
        {
            return ViewState[VsDispatchSelected] as List<int> ?? new List<int>();
        }

        private void MergeDispatchGridChecks()
        {
            var selected = new HashSet<int>(GetDispatchSelectedIds());
            foreach (GridViewRow row in gvDispatchPeople.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int id = Convert.ToInt32(gvDispatchPeople.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                var chk = row.FindControl("chkUseWaybill") as CheckBox;
                if (chk != null && chk.Checked)
                    selected.Add(id);
                else
                    selected.Remove(id);
            }
            ViewState[VsDispatchSelected] = selected.OrderBy(x => x).ToList();
        }

        protected void gvDispatchPeople_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            MergeDispatchGridChecks();
            gvDispatchPeople.PageIndex = e.NewPageIndex;
            BindDispatchWaybillPanel();
        }

        protected void gvDispatchPeople_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvDispatchPeople, e.Row);
        }

        protected void btnSaveDispatchWaybill_Click(object sender, EventArgs e)
        {
            MergeDispatchGridChecks();
            string joined = string.Join(",", GetDispatchSelectedIds());
            _settings.SaveDispatchWaybillSettings(joined, chkTrackingNumberRequired.Checked, UserName());
            ViewState[VsDispatchSelected] = null;
            BindDispatchWaybillPanel();
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapDispatchWaybillSaved), false);
            WooCommerceUserLog.Write("Mapping dispatch/waybill",
                "people=" + (joined.Length > 0 ? joined : "(none)")
                + "; trackingRequired=" + chkTrackingNumberRequired.Checked,
                UserName());
        }

        private void BindPaymentTab()
        {
            gvPaymentMaps.DataSource = _orderImportManager.GetPaymentMethodMaps() ?? new List<WooPaymentMethodMap>();
            gvPaymentMaps.DataBind();
        }

        private void ReloadAreaWorkingRows()
        {
            ViewState[VsWooAreaRows] = _areaManager.GetAreaDeliveryDefaults() ?? new List<WooAreaDeliveryDefault>();
        }

        private List<WooAreaDeliveryDefault> GetAreaWorkingRows()
        {
            return ViewState[VsWooAreaRows] as List<WooAreaDeliveryDefault>
                ?? new List<WooAreaDeliveryDefault>();
        }

        private void MergeAreaGridEdits()
        {
            var working = GetAreaWorkingRows();
            var byId = working.ToDictionary(r => r.AreaID);
            foreach (GridViewRow row in gvAreaDefaults.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int areaId = Convert.ToInt32(gvAreaDefaults.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                if (!byId.TryGetValue(areaId, out var target))
                    continue;
                var ddl = (DropDownList)row.FindControl("ddlDefaultPerson");
                var txt = (TextBox)row.FindControl("txtPostalRanges");
                int personId = ParseIntOrZero(ddl != null ? ddl.SelectedValue : null);
                target.DefaultPreferredAgentID = personId > 0 ? personId : (int?)null;
                target.PostalRanges = txt != null ? txt.Text : target.PostalRanges;
            }
            ViewState[VsWooAreaRows] = working;
        }

        private void BindAreaDefaultsGrid()
        {
            var working = GetAreaWorkingRows();
            string filter = (ViewState[VsWooAreaFilter] as string) ?? string.Empty;
            if (txtAreaFilter != null)
                txtAreaFilter.Text = filter;
            IEnumerable<WooAreaDeliveryDefault> query = working;
            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = filter.Trim();
                query = working.Where(r =>
                    !string.IsNullOrWhiteSpace(r.AreaName)
                    && r.AreaName.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            var list = query.ToList();
            if (gvAreaDefaults.PageIndex > 0
                && gvAreaDefaults.PageIndex * gvAreaDefaults.PageSize >= Math.Max(list.Count, 1))
                gvAreaDefaults.PageIndex = 0;
            gvAreaDefaults.DataSource = list;
            gvAreaDefaults.DataBind();
        }

        protected void gvAreaDefaults_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvAreaDefaults, e.Row);
        }

        protected void gvAreaDefaults_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            MergeAreaGridEdits();
            gvAreaDefaults.PageIndex = e.NewPageIndex;
            BindAreaDefaultsGrid();
        }

        protected void btnAreaFilter_Click(object sender, EventArgs e)
        {
            MergeAreaGridEdits();
            ViewState[VsWooAreaFilter] = (txtAreaFilter.Text ?? string.Empty).Trim();
            gvAreaDefaults.PageIndex = 0;
            BindAreaDefaultsGrid();
        }

        protected void btnAreaFilterClear_Click(object sender, EventArgs e)
        {
            MergeAreaGridEdits();
            ViewState[VsWooAreaFilter] = string.Empty;
            txtAreaFilter.Text = string.Empty;
            gvAreaDefaults.PageIndex = 0;
            BindAreaDefaultsGrid();
        }

        private void BindSystemDefaultPersonHint()
        {
            int id = WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId();
            var person = _personsRepo.GetById(id);
            string label = !string.IsNullOrWhiteSpace(person?.Abbreviation)
                ? person.Abbreviation
                : (!string.IsNullOrWhiteSpace(person?.PersonName) ? person.PersonName : id.ToString(CultureInfo.InvariantCulture));
            litSystemDefaultPersonHint.Text = MessageProvider.Format(
                MessageKeys.WooCommerce.MapSystemDefaultPersonHint,
                label,
                id);
        }

        private int GetSystemDefaultDeliveryPersonId()
        {
            return WooCommerceAreaMappingManager.GetSystemDefaultDeliveryPersonId();
        }

        private void BindDefaultImportAreaDropdown()
        {
            ddlDefaultImportArea.Items.Clear();
            ddlDefaultImportArea.Items.Add(new ListItem("(none)", "0"));
            foreach (var area in (_areasRepo.GetAll("AreaName") ?? new List<Area>()))
            {
                ddlDefaultImportArea.Items.Add(new ListItem(
                    area.AreaName ?? area.AreaID.ToString(CultureInfo.InvariantCulture),
                    area.AreaID.ToString(CultureInfo.InvariantCulture)));
            }
            int? def = _areaManager.GetDefaultImportAreaId();
            ddlDefaultImportArea.SelectedValue = def.HasValue && def.Value > 0
                ? def.Value.ToString(CultureInfo.InvariantCulture)
                : "0";
            MarkOriginal(ddlDefaultImportArea);
        }

        private void BindImportNotesItemDropdown()
        {
            var settings = _settings.GetSettings();
            int? selected = settings.ImportNotesItemID;
            if (!selected.HasValue || selected.Value <= 0)
            {
                var byLegacy = _itemsRepo.GetById(SystemConstants.ItemConstants.NoteItemTimeID);
                if (byLegacy != null)
                    selected = byLegacy.ItemID;
                else
                {
                    var byName = _itemsRepo.FindFirstByDescription("Notes");
                    if (byName != null && byName.ItemID > 0)
                        selected = byName.ItemID;
                }
            }

            ddlImportNotesItem.Items.Clear();
            ddlImportNotesItem.Items.Add(new ListItem("(none)", "0"));
            foreach (var item in _itemsRepo.GetNotesItemChoices(selected) ?? new List<OrderItemLookup>())
            {
                string idText = item.ItemTypeID.ToString(CultureInfo.InvariantCulture);
                string label = (item.ItemDesc ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(label))
                    label = "Item #" + idText;
                else
                    label = label + " (#" + idText + ")";
                ddlImportNotesItem.Items.Add(new ListItem(label, idText));
            }

            string sel = selected.HasValue && selected.Value > 0
                ? selected.Value.ToString(CultureInfo.InvariantCulture)
                : "0";
            if (ddlImportNotesItem.Items.FindByValue(sel) != null)
                ddlImportNotesItem.SelectedValue = sel;
            else
                ddlImportNotesItem.SelectedValue = "0";
            MarkOriginal(ddlImportNotesItem);
        }

        protected void btnSaveImportNotesItem_Click(object sender, EventArgs e)
        {
            int itemId = ParseIntOrZero(ddlImportNotesItem.SelectedValue);
            _settings.SaveImportNotesItemId(itemId > 0 ? itemId : (int?)null, UserName());
            BindImportNotesItemDropdown();
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapImportNotesItemSaved), false);
            WooCommerceUserLog.Write("Mapping import Notes item",
                itemId > 0 ? "itemId=" + itemId : "cleared", UserName());
        }

        private List<ListItem> GetPersonDropdownItems()
        {
            if (_personDropdownTemplate == null)
            {
                _personDropdownTemplate = new List<ListItem>();
                foreach (var person in _orderManager.GetDeliveryPersons() ?? new List<Person>())
                {
                    string label = !string.IsNullOrWhiteSpace(person.Abbreviation)
                        ? person.Abbreviation
                        : person.PersonName;
                    if (string.IsNullOrWhiteSpace(label))
                        label = person.PersonID.ToString(CultureInfo.InvariantCulture);
                    _personDropdownTemplate.Add(new ListItem(
                        label,
                        person.PersonID.ToString(CultureInfo.InvariantCulture)));
                }
            }
            return _personDropdownTemplate;
        }

        private void PopulatePersonDropdown(DropDownList ddl, int? selectedId, bool includeNone = true)
        {
            if (ddl == null)
                return;
            ddl.Items.Clear();
            if (includeNone)
                ddl.Items.Add(new ListItem("(none)", "0"));
            foreach (var li in GetPersonDropdownItems())
                ddl.Items.Add(new ListItem(li.Text, li.Value));
            if (selectedId.HasValue && selectedId.Value > 0)
            {
                var item = ddl.Items.FindByValue(selectedId.Value.ToString(CultureInfo.InvariantCulture));
                if (item != null)
                    item.Selected = true;
            }
            else if (includeNone)
                ddl.SelectedValue = "0";
        }

        protected void btnSaveDefaultArea_Click(object sender, EventArgs e)
        {
            int areaId = ParseIntOrZero(ddlDefaultImportArea.SelectedValue);
            _areaManager.SaveDefaultImportArea(areaId > 0 ? areaId : (int?)null, UserName());
            BindDefaultImportAreaDropdown();
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapDefaultAreaSaved), false);
            WooCommerceUserLog.Write("Mapping default import area",
                areaId > 0 ? "areaId=" + areaId : "cleared", UserName());
        }

        private void BindAddressConfigPanel()
        {
            var settings = _settings.GetSettings();
            chkImportAddressIncludeProvince.Checked = settings.ImportAddressIncludeProvince;
            chkImportAddressIncludeCountry.Checked = settings.ImportAddressIncludeCountry;
            chkImportPhoneReplacePlus27.Checked = settings.ImportPhoneReplacePlus27;
            chkImportPhoneFormatSa.Checked = settings.ImportPhoneFormatSa;
        }

        protected void btnSaveAddressConfig_Click(object sender, EventArgs e)
        {
            bool includeProvince = chkImportAddressIncludeProvince.Checked;
            bool includeCountry = chkImportAddressIncludeCountry.Checked;
            bool replacePlus27 = chkImportPhoneReplacePlus27.Checked;
            bool formatSa = chkImportPhoneFormatSa.Checked;
            _settings.SaveImportAddressSettings(
                includeProvince,
                includeCountry,
                replacePlus27,
                formatSa,
                UserName());
            BindAddressConfigPanel();
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapAddressConfigSaved), false);
            WooCommerceUserLog.Write("Mapping address config saved",
                string.Format(CultureInfo.InvariantCulture,
                    "province={0}, country={1}, phone27={2}, phoneFmt={3}",
                    includeProvince, includeCountry, replacePlus27, formatSa),
                UserName());
        }

        protected void btnSaveAreaDefaults_Click(object sender, EventArgs e)
        {
            MergeAreaGridEdits();
            int saved = _areaManager.SaveAreaDeliveryDefaults(GetAreaWorkingRows(), UserName());
            BindAreasTab();
            SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapAreaDefaultsSaved, saved), false);
            WooCommerceUserLog.Write("Mapping area defaults saved", "rows=" + saved, UserName());
        }

        protected void btnSaveShippingMaps_Click(object sender, EventArgs e)
        {
            var rows = CollectShippingMapsFromGrid(includeFooter: true);
            int saved = _areaManager.SaveShippingMethodMaps(rows, UserName());
            BindShippingTab();
            SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapShippingMapsSaved, saved), false);
            WooCommerceUserLog.Write("Mapping shipping maps saved", "rows=" + saved, UserName());
        }

        protected void btnTestResolve_Click(object sender, EventArgs e)
        {
            var result = _areaManager.ResolveArea(txtTestPostal.Text, txtTestSuburb.Text);
            if (result.IsAmbiguous)
            {
                litTestResolveResult.Text = MessageProvider.Format(
                    MessageKeys.WooCommerce.MapTestResolveAmbiguous,
                    result.Reason ?? string.Empty);
            }
            else if (!result.AreaID.HasValue)
            {
                litTestResolveResult.Text = MessageProvider.Format(
                    MessageKeys.WooCommerce.MapTestResolveFailed,
                    result.Reason ?? string.Empty);
            }
            else
            {
                string defFlag = result.UsedDefaultArea ? " (default area)" : string.Empty;
                string person = result.DefaultPreferredAgentID.HasValue
                    ? result.DefaultPreferredAgentID.Value.ToString(CultureInfo.InvariantCulture)
                    : "(none)";
                litTestResolveResult.Text = MessageProvider.Format(
                    MessageKeys.WooCommerce.MapTestResolveResult,
                    result.AreaName ?? "?",
                    result.AreaID.Value,
                    defFlag,
                    person,
                    result.Reason ?? string.Empty);
            }
        }

        protected void gvAreaDefaults_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as WooAreaDeliveryDefault;
            if (row == null)
                return;
            int personId = row.DefaultPreferredAgentID.HasValue && row.DefaultPreferredAgentID.Value > 0
                ? row.DefaultPreferredAgentID.Value
                : GetSystemDefaultDeliveryPersonId();
            PopulatePersonDropdown((DropDownList)e.Row.FindControl("ddlDefaultPerson"), personId, includeNone: false);
            MarkOriginal((DropDownList)e.Row.FindControl("ddlDefaultPerson"));
            MarkOriginal((TextBox)e.Row.FindControl("txtPostalRanges"));
        }

        protected void gvShippingMaps_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var map = e.Row.DataItem as WooShippingMethodMap;
                if (map != null)
                    PopulatePersonDropdown((DropDownList)e.Row.FindControl("ddlShipPerson"), map.ToBeDeliveredByID, includeNone: false);
                MarkOriginal((DropDownList)e.Row.FindControl("ddlShipPerson"));
                MarkOriginal((TextBox)e.Row.FindControl("txtMethodMatch"));
                MarkOriginal((TextBox)e.Row.FindControl("txtShipNotes"));
                var chk = (CheckBox)e.Row.FindControl("chkShipActive");
                if (chk != null)
                    MarkOriginal(chk, chk.Checked);
                var btn = (LinkButton)e.Row.FindControl("btnDeleteShipping");
                if (btn != null)
                {
                    btn.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapDeleteRow);
                    btn.Visible = map != null && map.MapID > 0;
                }
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                PopulatePersonDropdown((DropDownList)e.Row.FindControl("ddlNewShipPerson"), null, includeNone: false);
                MarkOriginal((TextBox)e.Row.FindControl("txtNewMethodMatch"));
                MarkOriginal((TextBox)e.Row.FindControl("txtNewShipNotes"));
                MarkOriginal((DropDownList)e.Row.FindControl("ddlNewShipPerson"));
            }
        }

        protected void gvShippingMaps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DeleteShipping", StringComparison.OrdinalIgnoreCase))
                return;
            int mapId = ParseIntOrZero(e.CommandArgument?.ToString());
            if (mapId <= 0)
                return;
            _areaManager.DeleteShippingMethodMap(mapId, UserName());
            BindShippingTab();
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapDeleted), false);
        }

        protected void btnSavePaymentMaps_Click(object sender, EventArgs e)
        {
            var rows = CollectPaymentMapsFromGrid(includeFooter: true);
            _orderImportManager.SavePaymentMethodMaps(rows, UserName());
            BindPaymentTab();
            SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapPaymentMapsSaved, rows.Count), false);
            WooCommerceUserLog.Write("Mapping payment maps saved", "rows=" + rows.Count, UserName());
        }

        protected void gvPaymentMaps_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                MarkOriginal((TextBox)e.Row.FindControl("txtPayMethodMatch"));
                MarkOriginal((TextBox)e.Row.FindControl("txtPayAbbrev"));
                var chk = (CheckBox)e.Row.FindControl("chkPayActive");
                if (chk != null)
                    MarkOriginal(chk, chk.Checked);
                var btn = (LinkButton)e.Row.FindControl("btnDeletePayment");
                if (btn != null)
                    btn.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapDeleteRow);
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                MarkOriginal((TextBox)e.Row.FindControl("txtNewPayMethodMatch"));
                MarkOriginal((TextBox)e.Row.FindControl("txtNewPayAbbrev"));
            }
        }

        protected void gvPaymentMaps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DeletePayment", StringComparison.OrdinalIgnoreCase))
                return;
            int mapId = ParseIntOrZero(e.CommandArgument?.ToString());
            if (mapId <= 0)
                return;
            _orderImportManager.DeletePaymentMethodMap(mapId, UserName());
            BindPaymentTab();
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapDeleted), false);
        }

        private List<WooPaymentMethodMap> CollectPaymentMapsFromGrid(bool includeFooter)
        {
            var list = new List<WooPaymentMethodMap>();
            foreach (GridViewRow row in gvPaymentMaps.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int mapId = Convert.ToInt32(gvPaymentMaps.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                list.Add(ReadPaymentMapRow(row, mapId));
            }
            if (includeFooter && gvPaymentMaps.FooterRow != null)
            {
                var footer = ReadPaymentFooterRow(gvPaymentMaps.FooterRow);
                if (footer != null)
                    list.Add(footer);
            }
            return list;
        }

        private static WooPaymentMethodMap ReadPaymentMapRow(GridViewRow row, int mapId)
        {
            var txtMatch = (TextBox)row.FindControl("txtPayMethodMatch");
            var txtAbbrev = (TextBox)row.FindControl("txtPayAbbrev");
            var chk = (CheckBox)row.FindControl("chkPayActive");
            return new WooPaymentMethodMap
            {
                MapID = mapId,
                MethodMatch = txtMatch?.Text,
                PaymentAbbrev = txtAbbrev?.Text,
                IsActive = chk == null || chk.Checked
            };
        }

        private static WooPaymentMethodMap ReadPaymentFooterRow(GridViewRow row)
        {
            var txtMatch = (TextBox)row.FindControl("txtNewPayMethodMatch");
            var txtAbbrev = (TextBox)row.FindControl("txtNewPayAbbrev");
            if (string.IsNullOrWhiteSpace(txtMatch?.Text) || string.IsNullOrWhiteSpace(txtAbbrev?.Text))
                return null;
            var chk = (CheckBox)row.FindControl("chkNewPayActive");
            return new WooPaymentMethodMap
            {
                MapID = 0,
                MethodMatch = txtMatch.Text.Trim(),
                PaymentAbbrev = txtAbbrev.Text.Trim(),
                IsActive = chk == null || chk.Checked
            };
        }

        private List<WooShippingMethodMap> CollectShippingMapsFromGrid(bool includeFooter)
        {
            var list = new List<WooShippingMethodMap>();
            foreach (GridViewRow row in gvShippingMaps.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int mapId = Convert.ToInt32(gvShippingMaps.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                list.Add(ReadShippingMapRow(row, mapId));
            }
            if (includeFooter && gvShippingMaps.FooterRow != null)
            {
                var footer = ReadShippingFooterRow(gvShippingMaps.FooterRow);
                if (footer != null)
                    list.Add(footer);
            }
            return list;
        }

        private WooShippingMethodMap ReadShippingMapRow(GridViewRow row, int mapId)
        {
            var txtMethod = (TextBox)row.FindControl("txtMethodMatch");
            var ddlPerson = (DropDownList)row.FindControl("ddlShipPerson");
            var chkActive = (CheckBox)row.FindControl("chkShipActive");
            var txtNotes = (TextBox)row.FindControl("txtShipNotes");
            return new WooShippingMethodMap
            {
                MapID = mapId,
                MethodMatch = txtMethod?.Text,
                ToBeDeliveredByID = ParseIntOrZero(ddlPerson?.SelectedValue),
                IsActive = chkActive == null || chkActive.Checked,
                Notes = txtNotes?.Text
            };
        }

        private WooShippingMethodMap ReadShippingFooterRow(GridViewRow row)
        {
            var txtMethod = (TextBox)row.FindControl("txtNewMethodMatch");
            var ddlPerson = (DropDownList)row.FindControl("ddlNewShipPerson");
            int personId = ParseIntOrZero(ddlPerson?.SelectedValue);
            if (string.IsNullOrWhiteSpace(txtMethod?.Text) || personId <= 0)
                return null;
            var chkActive = (CheckBox)row.FindControl("chkNewShipActive");
            var txtNotes = (TextBox)row.FindControl("txtNewShipNotes");
            return new WooShippingMethodMap
            {
                MapID = 0,
                MethodMatch = txtMethod.Text.Trim(),
                ToBeDeliveredByID = personId,
                IsActive = chkActive == null || chkActive.Checked,
                Notes = txtNotes?.Text
            };
        }

        private static int ParseIntOrZero(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;
            int n;
            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out n) ? n : 0;
        }

        #endregion

        private string UserName()
        {
            return Context?.User?.Identity?.Name ?? "system";
        }

        private enum StatusKind
        {
            Success,
            Warn,
            Error
        }

        private void SetStatus(string message, bool isError)
        {
            SetStatus(message, isError ? StatusKind.Error : StatusKind.Success);
        }

        private void SetStatus(string message, StatusKind kind)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                lblMessage.Visible = false;
                return;
            }
            lblMessage.Visible = true;
            lblMessage.Text = message;
            string tone = kind == StatusKind.Error
                ? "status-error"
                : (kind == StatusKind.Warn ? "status-warn" : "status-success");
            lblMessage.CssClass = "status-message " + tone;
        }
    }
}
