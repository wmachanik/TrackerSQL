using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class GroupItemDetail : Page
    {
        public const string CONST_QRYSTR_GROUPITEMID = "ItemTypeID";
        private const string CONST_SESSION_RETURNURL = "ReturnItemGroupURL";
        private readonly ItemsRepository _itemsRepository = new ItemsRepository();
        private readonly SysDataRepository _sysDataRepository = new SysDataRepository();

        protected System.Web.UI.ScriptManager scmGroupDetail;
        protected UpdatePanel upnlGroupDetail;
        protected Label lblAddOrEditItem;
        protected TextBox tbxGroupItem;
        protected Label lblGroupItemID;
        protected TextBox tbxGroupDesc;
        protected TextBox tbxGroupShortName;
        protected Button btnAdd;
        protected Button btnUpdate;
        protected Button btnCancel;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;
            this.Session[CONST_SESSION_RETURNURL] = this.Request.UrlReferrer == (Uri)null ? (object)"" : (object)this.Request.UrlReferrer.OriginalString.ToString();
            if (this.Request.QueryString[CONST_QRYSTR_GROUPITEMID] == null)
                return;

            var item = _itemsRepository.GetById(Convert.ToInt32(this.Request.QueryString[CONST_QRYSTR_GROUPITEMID]));
            if (item == null || item.ItemID == SystemConstants.DatabaseConstants.InvalidID)
                return;

            this.lblGroupItemID.Visible = true;
            this.btnAdd.Visible = false;
            this.btnUpdate.Visible = true;
            this.lblGroupItemID.Text = item.ItemID.ToString();
            this.tbxGroupItem.Text = item.ItemDesc;
            this.tbxGroupDesc.Text = item.ItemDetail;
            this.tbxGroupShortName.Text = item.ItemShortName;
        }

        protected void ReturnToPrevPage()
        {
            string url = this.Session[CONST_SESSION_RETURNURL].ToString();
            if (url.Length <= 0)
                return;
            this.Response.Redirect(url);
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (this.tbxGroupItem.Text.Equals(string.Empty))
            {
                new showMessageBox(this.Page, "Error no item", "Please enter a Group Item Name");
                return;
            }

            if (_itemsRepository.GroupNameExists(this.tbxGroupItem.Text))
            {
                new showMessageBox(this.Page, "name exists", $"Group Name: {this.tbxGroupItem.Text} Exists. Please enter a different Group Item Name");
                return;
            }

            int? groupServiceTypeId = _sysDataRepository.GetGroupItemServiceTypeId();
            var newItem = new Item
            {
                ItemDesc = this.tbxGroupItem.Text,
                ItemDetail = this.tbxGroupDesc.Text,
                ItemEnabled = true,
                ItemsCharacteritics = "Group Item",
                ItemShortName = this.tbxGroupShortName.Text,
                ItemServiceTypeID = groupServiceTypeId,
                SortOrder = 15
            };

            bool success = _itemsRepository.InsertGroupReferenceItem(newItem) > 0;
            new showMessageBox(this.Page, "Status", success ? "Group item added" : "Error adding group item");
            if (success)
                this.ReturnToPrevPage();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (this.tbxGroupItem.Text.Equals(string.Empty))
            {
                new showMessageBox(this.Page, "Error no item", "Please enter a Group Item Name");
                return;
            }

            int? groupServiceTypeId = _sysDataRepository.GetGroupItemServiceTypeId();
            var item = new Item
            {
                ItemID = Convert.ToInt32(this.lblGroupItemID.Text),
                ItemDesc = this.tbxGroupItem.Text,
                ItemDetail = this.tbxGroupDesc.Text,
                ItemEnabled = true,
                ItemsCharacteritics = "Group Item-update",
                ItemShortName = this.tbxGroupShortName.Text,
                ItemServiceTypeID = groupServiceTypeId,
                SortOrder = 15
            };

            bool success = _itemsRepository.UpdateGroupReferenceItem(item);
            new showMessageBox(this.Page, "Status", success ? "Group item update" : "Error updating group item");
            if (success)
                this.ReturnToPrevPage();
        }

        protected void btnCancel_Click(object sender, EventArgs e) => this.ReturnToPrevPage();
    }
}
