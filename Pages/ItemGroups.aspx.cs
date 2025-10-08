// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.ItemGroups
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Controls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class ItemGroups : Page
    {
        private const string CONST_SESSION_LASTIDSELECTED = "LastGroupIDSelected";
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

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
        }

        protected void btnAddGroup_Click(object sender, EventArgs e)
        {
            this.Response.Redirect("GroupItemDetail.aspx");
            this.ddlGroupItems.DataBind();
        }

        protected void btnAddItem_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in this.gvItemsNotInGroup.Rows)
            {
                CheckBox control1 = (CheckBox)row.FindControl("cbxAddItem");
                if (control1 != null && control1.Checked)
                {
                    DropDownList control2 = (DropDownList)row.FindControl("ddlItemTypeDesc");
                    ItemGroupTbl pItemGroupTbl = new ItemGroupTbl()
                    {
                        GroupItemTypeID = Convert.ToInt32(this.ddlGroupItems.SelectedValue),
                        ItemTypeID = Convert.ToInt32(control2.SelectedValue)
                    };
                    pItemGroupTbl.ItemTypeSortPos = pItemGroupTbl.GetLastGroupItemSortPos(pItemGroupTbl.GroupItemTypeID) + 1;
                    pItemGroupTbl.Enabled = true;
                    pItemGroupTbl.Notes = "added on ItemGroup form";
                    pItemGroupTbl.InsertItemGroup(pItemGroupTbl);
                }
            }
            this.gvItemsInList.DataBind();
            this.gvItemsNotInGroup.DataBind();
            this.updtPnlItemsInList.Update();
        }

        protected void btnRemove_Click(object sender, EventArgs e)
        {
            foreach (GridViewRow row in this.gvItemsInList.Rows)
            {
                CheckBox control1 = (CheckBox)row.FindControl("cbxRemoveItem");
                if (control1 != null && control1.Checked)
                {
                    DropDownList control2 = (DropDownList)row.FindControl("ddlItemDesc");
                    new ItemGroupTbl().DeleteGroupItemFromGroup(Convert.ToInt32(this.ddlGroupItems.SelectedValue), Convert.ToInt32(control2.SelectedValue));
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
            ItemGroupTbl pItemGroupTbl = new ItemGroupTbl();
            pItemGroupTbl.GroupItemTypeID = Convert.ToInt32(this.ddlGroupItems.SelectedValue);
            pItemGroupTbl.ItemTypeID = Convert.ToInt32(control1.SelectedValue);
            pItemGroupTbl.ItemTypeSortPos = Convert.ToInt32(control2.Text);
            if (e.CommandName.Equals("MoveUp"))
                pItemGroupTbl.DecItemSortPos(pItemGroupTbl);
            else
                pItemGroupTbl.IncItemSortPos(pItemGroupTbl);
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