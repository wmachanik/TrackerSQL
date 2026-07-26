using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class ItemGroups : Page
    {
        private const string SessionLastGroupId = "LastGroupIDSelected";
        private readonly ItemGroupsRepository _itemGroupsRepository = new ItemGroupsRepository();
        private readonly ItemsRepository _itemsRepository = new ItemsRepository();

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
        protected HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;
        protected ObjectDataSource odsItemsNotInGroup;
        protected ObjectDataSource odsItemInGroup;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Rebind when empty (first load, or ViewState lost the list)
            if (!IsPostBack || ddlGroupItems.Items.Count <= 1)
                BindGroupDropDown();

            if (!IsPostBack)
            {
                RestoreLastGroupSelection();
                UpdateDualListVisibility();
                if (!TryGetSelectedGroupId(out _))
                {
                    if (ddlGroupItems.Items.Count > 1)
                        SetStatus("Select a group.", "status-info");
                }
            }
        }

        private void BindGroupDropDown()
        {
            string keepSelected = ddlGroupItems.SelectedValue;
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

        private void RestoreLastGroupSelection()
        {
            if (Session[SessionLastGroupId] == null)
                return;

            string lastId = Session[SessionLastGroupId] as string;
            if (string.IsNullOrEmpty(lastId) || ddlGroupItems.Items.FindByValue(lastId) == null)
                return;

            ddlGroupItems.SelectedValue = lastId;
            UpdateDualListVisibility();
            if (pnlDualList != null && pnlDualList.Visible)
            {
                gvItemsInList.DataBind();
                gvItemsNotInGroup.DataBind();
            }
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
                return;
            }

            var itemIds = new List<int>();
            foreach (GridViewRow row in gvItemsNotInGroup.Rows)
            {
                var cbx = row.FindControl("cbxAddItem") as CheckBox;
                if (cbx == null || !cbx.Checked)
                    continue;

                int itemId = Convert.ToInt32(gvItemsNotInGroup.DataKeys[row.RowIndex].Value);
                if (itemId > 0)
                    itemIds.Add(itemId);
            }

            if (itemIds.Count == 0)
            {
                SetStatus("Check one or more items on the right, then click Add.", "status-warn");
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
                return;
            }

            int removed = 0;
            foreach (GridViewRow row in gvItemsInList.Rows)
            {
                var cbx = row.FindControl("cbxRemoveItem") as CheckBox;
                if (cbx == null || !cbx.Checked)
                    continue;

                int itemId = Convert.ToInt32(gvItemsInList.DataKeys[row.RowIndex].Value);
                if (itemId > 0 && _itemGroupsRepository.DeleteItemFromGroup(groupId, itemId))
                    removed++;
            }

            if (removed == 0)
            {
                SetStatus("Check one or more items on the left, then click Remove.", "status-warn");
                return;
            }

            RefreshGrids();
            SetStatus(removed == 1
                ? "Removed 1 item from the group."
                : $"Removed {removed} items from the group.", "status-success");
        }

        protected void ddlGroupItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!TryGetSelectedGroupId(out _))
            {
                Session.Remove(SessionLastGroupId);
                UpdateDualListVisibility();
                SetStatus("Select a group.", "status-info");
                upnlItemGroups.Update();
                return;
            }

            Session[SessionLastGroupId] = ddlGroupItems.SelectedValue;
            UpdateDualListVisibility();
            RefreshGrids();
            SetStatus("Loaded group members.", "status-info");
        }

        protected void gvItemsInList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals("MoveDown") && !e.CommandName.Equals("MoveUp"))
                return;

            if (!TryGetSelectedGroupId(out int groupId))
                return;

            int rowIndex = Convert.ToInt32(e.CommandArgument);
            if (rowIndex < 0 || rowIndex >= gvItemsInList.Rows.Count)
                return;

            int itemId = Convert.ToInt32(gvItemsInList.DataKeys[rowIndex].Value);
            var row = gvItemsInList.Rows[rowIndex];
            var lblPos = row.FindControl("lblItemSortPos") as Label;
            if (lblPos == null || !int.TryParse(lblPos.Text, out int sortPos))
            {
                SetStatus("Could not read sort position for that item.", "status-error");
                return;
            }

            bool moved = e.CommandName.Equals("MoveUp")
                ? _itemGroupsRepository.MoveItemSortUp(groupId, itemId, sortPos)
                : _itemGroupsRepository.MoveItemSortDown(groupId, itemId, sortPos);

            RefreshGrids();
            SetStatus(moved ? "Updated item order." : "Item is already at the end of the list.",
                moved ? "status-success" : "status-info");
        }

        private bool TryGetSelectedGroupId(out int groupId)
        {
            groupId = 0;
            if (ddlGroupItems == null || string.IsNullOrEmpty(ddlGroupItems.SelectedValue)
                || ddlGroupItems.SelectedValue == "-1")
                return false;

            return int.TryParse(ddlGroupItems.SelectedValue, out groupId) && groupId > 0;
        }

        private void UpdateDualListVisibility()
        {
            bool show = TryGetSelectedGroupId(out _);
            if (pnlDualList != null)
                pnlDualList.Visible = show;
        }

        private void RefreshGrids()
        {
            UpdateDualListVisibility();
            if (pnlDualList != null && pnlDualList.Visible)
            {
                gvItemsInList.DataBind();
                gvItemsNotInGroup.DataBind();
            }
            upnlItemGroups.Update();
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
