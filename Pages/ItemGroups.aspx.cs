using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class ItemGroups : Page
    {
        private const string CONST_SESSION_LASTIDSELECTED = "LastGroupIDSelected";
        private readonly ItemGroupsRepository _itemGroupsRepository = new ItemGroupsRepository();

        protected System.Web.UI.ScriptManager scrmngItemGroups;
        protected UpdatePanel updtPnlItems;
        protected DropDownList ddlGroupItems;
        protected ImageButton imgbtnAddGroup;
        protected ImageButton imgbtnEditGroup;
        protected UpdateProgress uprgItemGroups;
        protected UpdatePanel updtPnlItemsInList;
        protected GridView gvItemsInList;
        protected Button btnAddItem;
        protected Button btnRemove;
        protected GridView gvItemsNotInGroup;
        protected ObjectDataSource odsItemGroups;
        protected ObjectDataSource odsItemsNotInGroup;
        protected ObjectDataSource odsItemInGroup;
        protected ObjectDataSource odsItemTypes;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack || this.Session["LastGroupIDSelected"] == null)
                return;
            this.ddlGroupItems.DataBind();
            string str = (string)this.Session["LastGroupIDSelected"];
            if (this.ddlGroupItems.Items.FindByValue(str) == null)
                return;
            this.ddlGroupItems.SelectedValue = str;
        }

        protected void btnAddGroup_Click(object sender, EventArgs e)
        {
            this.Response.Redirect("GroupItemDetail.aspx");
            this.ddlGroupItems.DataBind();
        }

        protected void btnAddItem_Click(object sender, EventArgs e)
        {
            int groupId = Convert.ToInt32(this.ddlGroupItems.SelectedValue);
            foreach (GridViewRow row in this.gvItemsNotInGroup.Rows)
            {
                CheckBox control1 = (CheckBox)row.FindControl("cbxAddItem");
                if (control1 != null && control1.Checked)
                {
                    DropDownList control2 = (DropDownList)row.FindControl("ddlItemTypeDesc");
                    _itemGroupsRepository.InsertItemToGroup(groupId, Convert.ToInt32(control2.SelectedValue));
                }
            }
            this.gvItemsInList.DataBind();
            this.gvItemsNotInGroup.DataBind();
            this.updtPnlItemsInList.Update();
        }

        protected void btnRemove_Click(object sender, EventArgs e)
        {
            int groupId = Convert.ToInt32(this.ddlGroupItems.SelectedValue);
            foreach (GridViewRow row in this.gvItemsInList.Rows)
            {
                CheckBox control1 = (CheckBox)row.FindControl("cbxRemoveItem");
                if (control1 != null && control1.Checked)
                {
                    DropDownList control2 = (DropDownList)row.FindControl("ddlItemDesc");
                    _itemGroupsRepository.DeleteItemFromGroup(groupId, Convert.ToInt32(control2.SelectedValue));
                }
            }
            this.gvItemsInList.DataBind();
            this.gvItemsNotInGroup.DataBind();
        }

        protected void ddlGroupItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ddlGroupItems.SelectedValue.Equals("-1"))
                return;
            DropDownList dropDownList = (DropDownList)sender;
            if (dropDownList != null)
                this.Session["LastGroupIDSelected"] = (object)dropDownList.SelectedValue;
            this.gvItemsInList.DataBind();
            this.gvItemsNotInGroup.DataBind();
            this.updtPnlItemsInList.Update();
        }

        protected string GiveInStatus()
        {
            return !(this.ddlGroupItems.SelectedValue == "-1") ? "Please add an item to the group" : "Please select a group";
        }

        protected void gvItemsInList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals("MoveDown") && !e.CommandName.Equals("MoveUp"))
                return;
            GridViewRow row = this.gvItemsInList.Rows[Convert.ToInt32(e.CommandArgument)];
            DropDownList control1 = (DropDownList)row.FindControl("ddlItemDesc");
            Label control2 = (Label)row.FindControl("lblItemSortPos");
            int groupId = Convert.ToInt32(this.ddlGroupItems.SelectedValue);
            int itemId = Convert.ToInt32(control1.SelectedValue);
            int sortPos = Convert.ToInt32(control2.Text);

            if (e.CommandName.Equals("MoveUp"))
                _itemGroupsRepository.MoveItemSortUp(groupId, itemId, sortPos);
            else
                _itemGroupsRepository.MoveItemSortDown(groupId, itemId, sortPos);

            this.gvItemsInList.DataBind();
        }

        protected void btnEditGroup_Click(object sender, EventArgs e)
        {
            if (this.ddlGroupItems.SelectedValue.Equals("-1"))
                return;
            this.Response.Redirect("GroupItemDetail.aspx?ItemTypeID=" + this.ddlGroupItems.SelectedValue);
            this.ddlGroupItems.DataBind();
        }
    }
}
