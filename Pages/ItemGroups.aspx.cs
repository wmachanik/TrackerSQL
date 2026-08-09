using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class ItemGroups : Page
    {
        private const string SessionLastGroupId = "LastGroupIDSelected";
        private readonly ItemGroupsRepository _itemGroupsRepository = new ItemGroupsRepository();
        private readonly ItemsRepository _itemsRepository = new ItemsRepository();

        /// <summary>When true, ddlGroupItems_SelectedIndexChanged ignores programmatic selection changes.</summary>
        private bool _suppressGroupSelectedIndexChanged;

        protected System.Web.UI.ScriptManager scrmngItemGroups;
        protected UpdateProgress uprgItemGroups;
        protected UpdatePanel upnlItemGroups;
        protected Panel pnlItemGroups;
        protected DropDownList ddlGroupItems;
        protected ImageButton imgbtnAddGroup;
        protected ImageButton imgbtnEditGroup;
        protected ImageButton imgbtnBack;
        protected Panel pnlDualList;
        protected GridView gvItemsInList;
        protected Button btnAddToGroup;
        protected Button btnRemoveFromGroup;
        protected GridView gvItemsNotInGroup;
        protected Label lblAvailFilter;
        protected TextBox tbxAvailFilter;
        protected ImageButton btnApplyAvailFilter;
        protected ImageButton btnClearAvailFilter;
        protected HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Rebind when empty (first load, or ViewState lost the list)
            if (!IsPostBack || ddlGroupItems.Items.Count <= 1)
                BindGroupDropDown();

            if (!IsPostBack)
            {
                RestoreLastGroupSelection();
                if (!TryGetSelectedGroupId(out _))
                {
                    UpdateDualListVisibility();
                    if (ddlGroupItems.Items.Count > 1)
                        SetStatus("Select a group.", "status-info");
                }
            }
        }

        private void BindGroupDropDown()
        {
            string keepSelected = ddlGroupItems.SelectedValue;
            // Prefer session if post data was lost with the item list
            if ((string.IsNullOrEmpty(keepSelected) || keepSelected == "-1")
                && Session[SessionLastGroupId] != null)
            {
                keepSelected = Session[SessionLastGroupId] as string;
            }

            _suppressGroupSelectedIndexChanged = true;
            try
            {
                ddlGroupItems.Items.Clear();
                ddlGroupItems.Items.Add(new ListItem("--Please select or add group--", "-1"));

                List<OrderItemLookup> groups;
                try
                {
                    groups = _itemsRepository.GetAllGroupTypeItems() ?? new List<OrderItemLookup>();
                }
                catch (Exception ex)
                {
                    SetStatus("Could not load groups: " + ex.Message, "status-error");
                    return;
                }

                foreach (OrderItemLookup group in groups)
                {
                    if (group == null || group.ItemTypeID <= 0)
                        continue;
                    ddlGroupItems.Items.Add(new ListItem(
                        string.IsNullOrWhiteSpace(group.ItemDesc) ? ("Group #" + group.ItemTypeID) : group.ItemDesc,
                        group.ItemTypeID.ToString()));
                }

                if (!string.IsNullOrEmpty(keepSelected) && ddlGroupItems.Items.FindByValue(keepSelected) != null)
                    ddlGroupItems.SelectedValue = keepSelected;
                else
                    ddlGroupItems.SelectedValue = "-1";

                if (groups.Count == 0)
                    SetStatus("No groups found. Use Add to create one.", "status-warn");
            }
            finally
            {
                _suppressGroupSelectedIndexChanged = false;
            }
        }

        private void RestoreLastGroupSelection()
        {
            if (Session[SessionLastGroupId] == null)
                return;

            string lastId = Session[SessionLastGroupId] as string;
            if (string.IsNullOrEmpty(lastId) || ddlGroupItems.Items.FindByValue(lastId) == null)
                return;

            _suppressGroupSelectedIndexChanged = true;
            try
            {
                ddlGroupItems.SelectedValue = lastId;
            }
            finally
            {
                _suppressGroupSelectedIndexChanged = false;
            }

            RefreshGrids();
            SetStatus("Loaded group members.", "status-info");
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        protected void btnAddGroup_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Pages/GroupItemDetail.aspx");
        }

        protected void btnEditGroup_Click(object sender, ImageClickEventArgs e)
        {
            if (!TryGetSelectedGroupId(out int groupId))
            {
                UpdateDualListVisibility();
                SetStatus("Select a group.", "status-warn");
                return;
            }

            Response.Redirect("~/Pages/GroupItemDetail.aspx?ItemTypeID=" + groupId);
        }

        protected void btnAddToGroup_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedGroupId(out int groupId))
            {
                UpdateDualListVisibility();
                SetStatus("Select a group.", "status-warn");
                upnlItemGroups.Update();
                return;
            }

            var itemIds = GetCheckedItemIds(gvItemsNotInGroup, "cbxAddItem", "hdnAvailItemId");
            if (itemIds.Count == 0)
            {
                SetStatus("Tick items on the right (Sel), then click Add.", "status-warn");
                upnlItemGroups.Update();
                return;
            }

            int added;
            try
            {
                added = _itemGroupsRepository.InsertItemsToGroup(groupId, itemIds);
            }
            catch (Exception ex)
            {
                SetStatus("Could not add items: " + ex.Message, "status-error");
                upnlItemGroups.Update();
                return;
            }

            RefreshGrids();
            SetStatus(added == 1
                ? "Added 1 item to the group."
                : $"Added {added} items to the group.", "status-success");
        }

        protected void btnRemoveFromGroup_Click(object sender, EventArgs e)
        {
            if (!TryGetSelectedGroupId(out int groupId))
            {
                UpdateDualListVisibility();
                SetStatus("Select a group.", "status-warn");
                upnlItemGroups.Update();
                return;
            }

            var itemIds = GetCheckedItemIds(gvItemsInList, "cbxRemoveItem", "hdnInGroupItemId");
            if (itemIds.Count == 0)
            {
                SetStatus("Tick Sel next to the item, then click Remove.", "status-warn");
                upnlItemGroups.Update();
                return;
            }

            int removed = 0;
            try
            {
                foreach (int itemId in itemIds)
                {
                    if (_itemGroupsRepository.DeleteItemFromGroup(groupId, itemId))
                        removed++;
                }
            }
            catch (Exception ex)
            {
                SetStatus("Could not remove items: " + ex.Message, "status-error");
                upnlItemGroups.Update();
                return;
            }

            if (removed == 0)
            {
                SetStatus("No matching group rows were deleted.", "status-warn");
                RefreshGrids();
                return;
            }

            RefreshGrids();
            SetStatus(removed == 1
                ? "Removed 1 item from the group."
                : $"Removed {removed} items from the group.", "status-success");
        }

        protected void ddlGroupItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressGroupSelectedIndexChanged)
                return;

            if (!TryGetSelectedGroupId(out _))
            {
                Session.Remove(SessionLastGroupId);
                ClearAvailFilter(rebind: false);
                gvItemsInList.DataSource = null;
                gvItemsInList.DataBind();
                gvItemsNotInGroup.DataSource = null;
                gvItemsNotInGroup.DataBind();
                UpdateDualListVisibility();
                SetStatus("Select a group.", "status-info");
                upnlItemGroups.Update();
                return;
            }

            Session[SessionLastGroupId] = ddlGroupItems.SelectedValue;
            ClearAvailFilter(rebind: false);
            gvItemsInList.PageIndex = 0;
            gvItemsNotInGroup.PageIndex = 0;
            RefreshGrids();
        }

        protected void tbxAvailFilter_TextChanged(object sender, EventArgs e)
        {
            ViewState["AvailItemFilter"] = tbxAvailFilter != null ? tbxAvailFilter.Text.Trim() : string.Empty;
            gvItemsNotInGroup.PageIndex = 0;
            RefreshGrids(preserveStatus: true);
        }

        protected void btnApplyAvailFilter_Click(object sender, ImageClickEventArgs e)
        {
            tbxAvailFilter_TextChanged(sender, EventArgs.Empty);
        }

        protected void btnClearAvailFilter_Click(object sender, EventArgs e)
        {
            ClearAvailFilter(rebind: true);
        }

        protected void gvItemsInList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvItemsInList.PageIndex = e.NewPageIndex;
            RefreshGrids();
        }

        protected void gvItemsNotInGroup_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvItemsNotInGroup.PageIndex = e.NewPageIndex;
            RefreshGrids();
        }

        /// <summary>App-standard pager (Previous / squares / Next) — see Classes/GridPager.cs.</summary>
        protected void gvItemsInList_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvItemsInList, e.Row);
        }

        protected void gvItemsNotInGroup_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvItemsNotInGroup, e.Row);
        }

        protected void gvItemsInList_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["InGroupSort"] = e.SortExpression;
            RefreshGrids();
        }

        protected void gvItemsInList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals("MoveDown") && !e.CommandName.Equals("MoveUp"))
                return;

            if (!TryGetSelectedGroupId(out int moveGroupId))
                return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            if (rowIndex < 0 || rowIndex >= gvItemsInList.Rows.Count)
                return;

            int moveItemId = ResolveRowItemId(gvItemsInList.Rows[rowIndex], "hdnInGroupItemId");
            if (moveItemId <= 0 && gvItemsInList.DataKeys != null && rowIndex < gvItemsInList.DataKeys.Count)
                int.TryParse(Convert.ToString(gvItemsInList.DataKeys[rowIndex].Value), out moveItemId);

            var row = gvItemsInList.Rows[rowIndex];
            var lblPos = row.FindControl("lblItemSortPos") as Label;
            if (lblPos == null || !int.TryParse(lblPos.Text, out int sortPos))
            {
                SetStatus("Could not read sort position for that item.", "status-error");
                return;
            }

            bool moved = e.CommandName.Equals("MoveUp")
                ? _itemGroupsRepository.MoveItemSortUp(moveGroupId, moveItemId, sortPos)
                : _itemGroupsRepository.MoveItemSortDown(moveGroupId, moveItemId, sortPos);

            RefreshGrids();
            SetStatus(moved ? "Updated item order." : "Item is already at the end of the list.",
                moved ? "status-success" : "status-info");
        }

        private List<int> GetCheckedItemIds(GridView grid, string checkBoxId, string hiddenId)
        {
            var ids = new List<int>();
            if (grid == null)
                return ids;

            foreach (GridViewRow row in grid.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                    continue;

                if (!IsCheckBoxChecked(row, checkBoxId))
                    continue;

                int itemId = ResolveRowItemId(row, hiddenId);
                if (itemId <= 0 && grid.DataKeys != null && row.RowIndex >= 0 && row.RowIndex < grid.DataKeys.Count)
                {
                    object key = grid.DataKeys[row.RowIndex].Value;
                    if (key != null)
                        int.TryParse(key.ToString(), out itemId);
                }

                if (itemId > 0 && !ids.Contains(itemId))
                    ids.Add(itemId);
            }

            return ids;
        }

        private bool IsCheckBoxChecked(GridViewRow row, string checkBoxId)
        {
            var cbx = row.FindControl(checkBoxId) as CheckBox;
            if (cbx == null)
                return false;

            // Prefer posted form value — survives a rebind that cleared CheckBox.Checked
            string posted = Request.Form[cbx.UniqueID];
            if (!string.IsNullOrEmpty(posted))
                return posted.Equals("on", StringComparison.OrdinalIgnoreCase)
                    || posted.Equals("true", StringComparison.OrdinalIgnoreCase)
                    || posted == "1";

            return cbx.Checked;
        }

        private static int ResolveRowItemId(GridViewRow row, string hiddenId)
        {
            var hdn = row.FindControl(hiddenId) as HiddenField;
            if (hdn != null && int.TryParse(hdn.Value, out int id) && id > 0)
                return id;
            return 0;
        }

        private bool TryGetSelectedGroupId(out int groupId)
        {
            groupId = 0;
            if (ddlGroupItems != null
                && !string.IsNullOrEmpty(ddlGroupItems.SelectedValue)
                && ddlGroupItems.SelectedValue != "-1"
                && int.TryParse(ddlGroupItems.SelectedValue, out groupId)
                && groupId > 0)
            {
                return true;
            }

            // Fallback if dropdown was rebuilt mid-postback
            string sessionId = Session[SessionLastGroupId] as string;
            return !string.IsNullOrEmpty(sessionId)
                && int.TryParse(sessionId, out groupId)
                && groupId > 0;
        }

        private void UpdateDualListVisibility()
        {
            bool show = TryGetSelectedGroupId(out _);
            if (pnlDualList != null)
                pnlDualList.Visible = show;
        }

        private void ClearAvailFilter(bool rebind)
        {
            ViewState["AvailItemFilter"] = string.Empty;
            if (tbxAvailFilter != null)
                tbxAvailFilter.Text = string.Empty;
            if (rebind)
            {
                gvItemsNotInGroup.PageIndex = 0;
                RefreshGrids(preserveStatus: true);
            }
        }

        private void RefreshGrids(bool preserveStatus = false)
        {
            UpdateDualListVisibility();

            if (!TryGetSelectedGroupId(out int groupId))
            {
                upnlItemGroups.Update();
                return;
            }

            // Keep session in sync for remove/add postbacks
            Session[SessionLastGroupId] = groupId.ToString();

            try
            {
                string sortBy = ViewState["InGroupSort"] as string;
                var inGroup = _itemGroupsRepository.GetGridRowsByGroupReferenceItemId(groupId, sortBy)
                    ?? new List<ItemGroupGridRow>();
                var notInGroup = _itemsRepository.GetItemsNotInGroup(groupId)
                    ?? new List<OrderItemLookup>();

                string filter = (tbxAvailFilter != null ? tbxAvailFilter.Text : null) ?? string.Empty;
                filter = filter.Trim();
                ViewState["AvailItemFilter"] = filter;
                if (tbxAvailFilter != null)
                    tbxAvailFilter.Text = filter;

                if (!string.IsNullOrEmpty(filter))
                {
                    notInGroup = notInGroup
                        .Where(i => i != null
                            && !string.IsNullOrEmpty(i.ItemDesc)
                            && i.ItemDesc.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                gvItemsInList.DataSource = inGroup;
                gvItemsInList.DataBind();

                gvItemsNotInGroup.DataSource = notInGroup;
                gvItemsNotInGroup.DataBind();

                if (!preserveStatus)
                {
                    string filterNote = string.IsNullOrEmpty(filter)
                        ? string.Empty
                        : $" (filtered by \"{filter}\")";
                    SetStatus(
                        $"Group loaded: {inGroup.Count} in group, {notInGroup.Count} available to add{filterNote}.",
                        "status-info");
                }
            }
            catch (Exception ex)
            {
                SetStatus("Could not load group members: " + ex.Message, "status-error");
            }

            upnlItemGroups.Update();

            if (tbxAvailFilter != null && pnlDualList != null && pnlDualList.Visible
                && !string.IsNullOrEmpty(tbxAvailFilter.Text))
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "itemGroupsAvailFilterFocus",
                    "try{var t=document.getElementById('" + tbxAvailFilter.ClientID
                    + "');if(t){t.focus();var v=t.value;t.value='';t.value=v;}}catch(ex){}",
                    true);
            }
        }

        private void SetStatus(string message, string cssModifier)
        {
            if (ltrlStatus == null || pnlStatus == null)
                return;

            ltrlStatus.Text = message ?? string.Empty;
            pnlStatus.Attributes["class"] = string.IsNullOrEmpty(cssModifier)
                ? "status-message"
                : "status-message " + cssModifier;
        }
    }
}
