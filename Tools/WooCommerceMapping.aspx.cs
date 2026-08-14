using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        private readonly WooCommerceSettingsManager _settings = new WooCommerceSettingsManager();
        private readonly ItemsRepository _itemsRepo = new ItemsRepository();

        private const string SessionPull = "WooMap.PullRows";
        private const string SessionPendingIncludes = "WooMap.PendingIncludes";
        private const string VsTab = "WooMap.Tab";

        /// <summary>DB Include values for the current bind (before pending overrides).</summary>
        private Dictionary<int, bool> _dbIncludes;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserCanManage())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = true;
                lblAccessDenied.Text = MessageProvider.Get(MessageKeys.SystemPreferences.AccessDenied);
                return;
            }

            if (!IsPostBack)
            {
                BindLabels();
                BindItemDropdowns();
                ShowTab(0);
                BindCategories();
                BindExistingMaps();
                BindWildcards();
                LoadCatMode();
            }
            else
            {
                ApplyTabHighlight(GetTab());
            }
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
            btnTabMap.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabMappings);
            btnTabWild.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabWildcards);
            btnTabSync.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapTabSync);
            litCatHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapCatHelp);
            litMapHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapMapHelp);
            litWildHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapWildHelp);
            litSyncHelp.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSyncHelp);
            litCatModeLbl.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelCategoryMode) + " ";
            btnSaveCatMode.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveCatMode);
            btnSaveCatIncludes.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveIncludes);
            btnPullCats.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapPullCategories);
            btnPullProducts.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapPullProducts);
            litExistingMaps.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapExistingTitle);
            litWildPrefix.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapWildPrefix);
            litWildSuffix.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapWildSuffix);
            litWildQty.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapWildQty);
            litWildItem.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapWildItem);
            litWildNotes.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapWildNotes);
            btnSaveWild.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapSaveWildcard);
            btnDryPush.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapDryPush);
            btnPushEnabled.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapPushEnabled);
            hdnUnsavedLeave.Value = MessageProvider.Get(MessageKeys.WooCommerce.MapUnsavedLeave);
            hdnPullCatsConfirm.Value = MessageProvider.Get(MessageKeys.WooCommerce.MapPullCatsConfirm);
            btnBack.AlternateText = MessageProvider.Get(MessageKeys.WooCommerce.ButtonBack);
            btnBack.ToolTip = MessageProvider.Get(MessageKeys.WooCommerce.MapBackToPreferences);
        }

        private void BindItemDropdowns()
        {
            var items = _itemsRepo.GetAll("ItemDesc") ?? new List<Item>();
            ddlWildItem.Items.Clear();
            ddlWildItem.Items.Add(new ListItem("(select item)", "0"));
            foreach (var i in items)
            {
                string text = (i.SKU ?? "") + " — " + (i.ItemDesc ?? "");
                ddlWildItem.Items.Add(new ListItem(text, i.ItemID.ToString()));
            }
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
        }

        private void ApplyTabHighlight(int index)
        {
            btnTabCat.CssClass = index == 0 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabMap.CssClass = index == 1 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabWild.CssClass = index == 2 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnTabSync.CssClass = index == 3 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
        }

        protected void btnTabCat_Click(object sender, EventArgs e) { ShowTab(0); BindCategories(); LoadCatMode(); }
        protected void btnTabMap_Click(object sender, EventArgs e) { ShowTab(1); BindExistingMaps(); RebindPull(); }
        protected void btnTabWild_Click(object sender, EventArgs e) { ShowTab(2); BindWildcards(); }
        protected void btnTabSync_Click(object sender, EventArgs e) { ShowTab(3); }

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
            btnSaveCatMode.Attributes["disabled"] = "disabled";
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
        }

        /// <summary>Capture Include ticks from the current page into session (keeps edits across paging).</summary>
        private void MergeVisibleIncludesToPending()
        {
            var dbList = _manager.GetCategoryFilters() ?? new List<WooCategoryFilter>();
            var db = dbList.ToDictionary(x => x.FilterID, x => x.IncludeInSync);
            var pending = GetPendingIncludes();

            foreach (GridViewRow row in gvCategories.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;
                int filterId = Convert.ToInt32(gvCategories.DataKeys[row.RowIndex].Value);
                var chk = row.FindControl("chkInclude") as CheckBox;
                if (chk == null)
                    continue;

                bool dbVal;
                if (!db.TryGetValue(filterId, out dbVal))
                    dbVal = true;

                if (chk.Checked != dbVal)
                    pending[filterId] = chk.Checked;
                else
                    pending.Remove(filterId);
            }
        }

        private void SyncSaveIncludesButton()
        {
            int pending = GetPendingIncludes().Count;
            hdnPendingCount.Value = pending.ToString();
            if (pending == 0)
                btnSaveCatIncludes.Attributes["disabled"] = "disabled";
            else
                btnSaveCatIncludes.Attributes.Remove("disabled");
        }

        private void BindCategories()
        {
            var list = _manager.GetCategoryFilters() ?? new List<WooCategoryFilter>();
            _dbIncludes = list.ToDictionary(x => x.FilterID, x => x.IncludeInSync);

            var pending = GetPendingIncludes();
            foreach (var c in list)
            {
                bool overrideInclude;
                if (pending.TryGetValue(c.FilterID, out overrideInclude))
                    c.IncludeInSync = overrideInclude;
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

            var chk = e.Row.FindControl("chkInclude") as CheckBox;
            var row = e.Row.DataItem as WooCategoryFilter;
            if (chk == null || row == null)
                return;

            bool dbInclude = row.IncludeInSync;
            if (_dbIncludes != null && _dbIncludes.ContainsKey(row.FilterID))
                dbInclude = _dbIncludes[row.FilterID];

            chk.InputAttributes["data-original"] = dbInclude ? "1" : "0";
        }

        protected void btnPullCats_Click(object sender, EventArgs e)
        {
            ClearPendingIncludes();
            var result = _manager.PullCategories(UserName());
            SetStatus(result.Message, !result.Succeeded);
            gvCategories.PageIndex = 0;
            BindCategories();
        }

        protected void btnSaveCatMode_Click(object sender, EventArgs e)
        {
            _manager.SaveCategoryMode(ddlCatMode.SelectedValue, UserName());
            LoadCatMode();
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapCatModeSaved), false);
        }

        protected void btnSaveCatIncludes_Click(object sender, EventArgs e)
        {
            MergeVisibleIncludesToPending();
            var pending = GetPendingIncludes();
            int saved = 0;
            foreach (var kv in pending.ToList())
            {
                _manager.SaveCategoryRow(kv.Key, kv.Value, UserName());
                saved++;
            }
            ClearPendingIncludes();
            BindCategories();
            SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapIncludesSaved, saved), false);
        }

        protected void btnPullProducts_Click(object sender, EventArgs e)
        {
            try
            {
                var rows = _manager.PullProductsForMapping(UserName());
                Session[SessionPull] = rows;
                gvPull.DataSource = rows;
                gvPull.DataBind();
                SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.MapPullProductsOk, rows.Count), false);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void RebindPull()
        {
            var rows = Session[SessionPull] as List<WooProductMapRow>;
            if (rows == null) return;
            gvPull.DataSource = rows;
            gvPull.DataBind();
        }

        protected void gvPull_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;
            var row = e.Row.DataItem as WooProductMapRow;
            var ddl = e.Row.FindControl("ddlItem") as DropDownList;
            if (ddl == null || row == null)
                return;

            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("(not mapped)", "0"));
            foreach (ListItem li in ddlWildItem.Items)
            {
                if (li.Value == "0") continue;
                ddl.Items.Add(new ListItem(li.Text, li.Value));
            }
            int selected = row.MappedItemID > 0 ? row.MappedItemID : row.SuggestedItemID;
            if (selected > 0 && ddl.Items.FindByValue(selected.ToString()) != null)
                ddl.SelectedValue = selected.ToString();
        }

        protected void gvPull_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "SaveMap")
                return;
            int index = Convert.ToInt32(e.CommandArgument);
            var rows = Session[SessionPull] as List<WooProductMapRow>;
            if (rows == null || index < 0 || index >= rows.Count)
                return;

            GridViewRow gvRow = gvPull.Rows[index];
            var ddl = (DropDownList)gvRow.FindControl("ddlItem");
            var txtQty = (TextBox)gvRow.FindControl("txtQty");
            int itemId = Convert.ToInt32(ddl.SelectedValue);
            double qty;
            if (!double.TryParse(txtQty.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out qty))
                qty = 1;

            var src = rows[index];
            try
            {
                _manager.SaveMapping(src.WooProductId, src.WooVariationId, itemId, qty, src.Sku, UserName());
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapSaved), false);
                BindExistingMaps();
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void BindExistingMaps()
        {
            gvMaps.DataSource = _manager.GetMappings();
            gvMaps.DataBind();
        }

        protected void gvMaps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DelMap")
                return;
            int id = Convert.ToInt32(e.CommandArgument);
            _manager.DeleteMapping(id, UserName());
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapDeleted), false);
            BindExistingMaps();
        }

        private void BindWildcards()
        {
            gvWild.DataSource = _manager.GetWildcardRules();
            gvWild.DataBind();
        }

        protected void btnSaveWild_Click(object sender, EventArgs e)
        {
            try
            {
                double qty;
                if (!double.TryParse(txtWildQty.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out qty))
                    qty = 1;
                var rule = new WooSkuWildcardRule
                {
                    RuleID = Convert.ToInt32(hdnWildId.Value),
                    SkuPrefixPattern = txtWildPrefix.Text,
                    SuffixToken = txtWildSuffix.Text,
                    QtyFactor = qty,
                    ItemID = Convert.ToInt32(ddlWildItem.SelectedValue),
                    Notes = txtWildNotes.Text,
                    IsActive = true
                };
                _manager.SaveWildcard(rule, UserName());
                hdnWildId.Value = "0";
                txtWildPrefix.Text = string.Empty;
                txtWildSuffix.Text = string.Empty;
                txtWildNotes.Text = string.Empty;
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapWildSaved), false);
                BindWildcards();
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        protected void gvWild_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DelWild")
                return;
            _manager.DeleteWildcard(Convert.ToInt32(e.CommandArgument), UserName());
            SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.MapWildDeleted), false);
            BindWildcards();
        }

        protected void btnDryPush_Click(object sender, EventArgs e)
        {
            var result = _manager.PushEnabledState(UserName(), dryRun: true);
            SetStatus(result.Message, !result.Succeeded);
        }

        protected void btnPushEnabled_Click(object sender, EventArgs e)
        {
            var result = _manager.PushEnabledState(UserName(), dryRun: false);
            SetStatus(result.Message, !result.Succeeded);
            BindExistingMaps();
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Tools/SystemPreferences.aspx");
        }

        private string UserName()
        {
            return Context?.User?.Identity?.Name ?? "system";
        }

        private void SetStatus(string message, bool isError)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                lblMessage.Visible = false;
                return;
            }
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = isError ? "status-message status-error" : "status-message status-success";
        }
    }
}
