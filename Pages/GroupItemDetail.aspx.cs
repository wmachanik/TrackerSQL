using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class GroupItemDetail : Page
    {
        public const string QueryGroupItemId = "ItemTypeID";
        private const string SessionReturnUrl = "ReturnItemGroupURL";
        private const string DefaultReturnUrl = "~/Pages/ItemGroups.aspx";

        private readonly ItemsRepository _itemsRepository = new ItemsRepository();
        private readonly SysDataRepository _sysDataRepository = new SysDataRepository();

        protected System.Web.UI.ScriptManager scmGroupDetail;
        protected UpdateProgress uprgGroupDetail;
        protected UpdatePanel upnlGroupDetail;
        protected Panel pnlGroupDetail;
        protected Literal litPageTitle;
        protected TextBox tbxGroupItem;
        protected HiddenField hdnGroupItemID;
        protected Label lblGroupItemID;
        protected TextBox tbxGroupDesc;
        protected TextBox tbxGroupShortName;
        protected Button btnSave;
        protected Button btnCancel;
        protected HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;

        private bool TryGetEditItemId(out int itemId)
        {
            itemId = 0;
            return hdnGroupItemID != null
                && int.TryParse(hdnGroupItemID.Value, out itemId)
                && itemId > 0;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            CaptureReturnUrl();
            LoadExistingGroupIfRequested();
        }

        private void CaptureReturnUrl()
        {
            string referrer = Request.UrlReferrer != null ? Request.UrlReferrer.OriginalString : string.Empty;
            if (!string.IsNullOrEmpty(referrer) && IsSameSiteUrl(referrer))
                Session[SessionReturnUrl] = referrer;
            else if (Session[SessionReturnUrl] == null)
                Session[SessionReturnUrl] = ResolveUrl(DefaultReturnUrl);
        }

        private static bool IsSameSiteUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            try
            {
                if (url.StartsWith("~/", StringComparison.Ordinal) || url.StartsWith("/", StringComparison.Ordinal))
                    return true;
                var uri = new Uri(url, UriKind.Absolute);
                return string.Equals(uri.Host, HttpContext.Current.Request.Url.Host, StringComparison.OrdinalIgnoreCase);
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        private void LoadExistingGroupIfRequested()
        {
            string idText = Request.QueryString[QueryGroupItemId];
            if (string.IsNullOrEmpty(idText) || !int.TryParse(idText, out int itemId) || itemId <= 0)
            {
                litPageTitle.Text = "Add Group";
                SetStatus("Enter a name for the new group.", "status-info");
                return;
            }

            var item = _itemsRepository.GetById(itemId);
            if (item == null || item.ItemID == SystemConstants.DatabaseConstants.InvalidID)
            {
                litPageTitle.Text = "Add Group";
                SetStatus("Group not found — you can create a new one.", "status-warn");
                return;
            }

            litPageTitle.Text = "Edit Group";
            hdnGroupItemID.Value = item.ItemID.ToString();
            lblGroupItemID.Visible = true;
            lblGroupItemID.Text = "ID: " + item.ItemID;
            tbxGroupItem.Text = item.ItemDesc;
            tbxGroupDesc.Text = item.ItemDetail;
            tbxGroupShortName.Text = item.ItemShortName;
            SetStatus("Loaded " + item.ItemDesc + ".", "status-info");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string name = (tbxGroupItem.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
            {
                SetStatus("Please enter a group name.", "status-error");
                return;
            }

            int? groupServiceTypeId = _sysDataRepository.GetGroupItemServiceTypeId();
            if (!groupServiceTypeId.HasValue || groupServiceTypeId.Value <= 0)
            {
                SetStatus("System group service type is not configured.", "status-error");
                return;
            }

            if (TryGetEditItemId(out int itemId))
            {
                var item = new Item
                {
                    ItemID = itemId,
                    ItemDesc = name,
                    ItemDetail = tbxGroupDesc.Text,
                    ItemEnabled = true,
                    ItemsCharacteritics = "Group Item-update",
                    ItemShortName = tbxGroupShortName.Text,
                    ItemServiceTypeID = groupServiceTypeId,
                    SortOrder = 15
                };

                if (!_itemsRepository.UpdateGroupReferenceItem(item))
                {
                    SetStatus("Error updating " + name + ".", "status-error");
                    return;
                }

                Session[SessionReturnUrl] = ResolveUrl(DefaultReturnUrl);
                Response.Redirect(DefaultReturnUrl, false);
                return;
            }

            if (_itemsRepository.GroupNameExists(name))
            {
                SetStatus("Group name \"" + name + "\" already exists. Choose a different name.", "status-warn");
                return;
            }

            var newItem = new Item
            {
                ItemDesc = name,
                ItemDetail = tbxGroupDesc.Text,
                ItemEnabled = true,
                ItemsCharacteritics = "Group Item",
                ItemShortName = tbxGroupShortName.Text,
                ItemServiceTypeID = groupServiceTypeId,
                SortOrder = 15
            };

            if (_itemsRepository.InsertGroupReferenceItem(newItem) <= 0)
            {
                SetStatus("Error adding " + name + ".", "status-error");
                return;
            }

            Response.Redirect(DefaultReturnUrl, false);
        }

        protected void btnCancel_Click(object sender, EventArgs e) => ReturnToPrevPage();

        private void ReturnToPrevPage()
        {
            string url = Session[SessionReturnUrl] as string;
            if (string.IsNullOrEmpty(url) || !IsSameSiteUrl(url))
            {
                Response.Redirect(DefaultReturnUrl, false);
                return;
            }

            Response.Redirect(url, false);
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
