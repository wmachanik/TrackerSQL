using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class OrderEntry : Page
    {
        private const string DefaultReturnUrl = "~/Default.aspx";

        protected ScriptManager smOrderEntry;
        protected UpdateProgress uprgOrderEntry;
        protected UpdatePanel upnlOrderEntry;
        protected Panel pnlOrderEntry;
        protected CheckBox chkbxOrderDone;
        protected DropDownList ddlSearchFor;
        protected TextBox tbxSearchFor;
        protected Button btnGo;
        protected Button btnReset;
        protected HyperLink btnNewOrder;
        protected Button btnBack;
        protected GridView gvListOfOrders;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;
        protected ObjectDataSource odsDistinctOrders;

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPostBackControls();

            if (!IsPostBack)
                RefreshStatus();
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;

            scriptManager.RegisterAsyncPostBackControl(chkbxOrderDone);
            scriptManager.RegisterAsyncPostBackControl(btnGo);
            scriptManager.RegisterAsyncPostBackControl(btnReset);
            scriptManager.RegisterAsyncPostBackControl(tbxSearchFor);
            scriptManager.RegisterPostBackControl(btnBack);
        }

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);
            pnlStatus.Visible = !string.IsNullOrWhiteSpace(message);

            if (string.IsNullOrWhiteSpace(message))
            {
                pnlStatus.Attributes["class"] = "status-message";
                return;
            }

            if (isError == true)
                pnlStatus.Attributes["class"] = "status-message status-error";
            else if (isError == false)
                pnlStatus.Attributes["class"] = "status-message status-success";
            else
                pnlStatus.Attributes["class"] = "status-message status-info";
        }

        private void RefreshStatus(string statusPrefix = null)
        {
            try
            {
                string mode = chkbxOrderDone.Checked ? "done" : "open";
                string status = $"Showing {mode} orders.";

                if (ddlSearchFor.SelectedValue == "Company" && !string.IsNullOrWhiteSpace(tbxSearchFor.Text))
                    status += $" Company filter: {tbxSearchFor.Text.Trim()}.";
                else if (ddlSearchFor.SelectedValue == "PrepDate" && !string.IsNullOrWhiteSpace(tbxSearchFor.Text))
                    status += $" Prep date filter: {tbxSearchFor.Text.Trim()}.";

                if (!string.IsNullOrWhiteSpace(statusPrefix))
                    SetStatus(statusPrefix.Trim() + " " + status, isError: false);
                else
                    SetStatus(status, isError: null);
            }
            catch (Exception ex)
            {
                SetStatus("Could not refresh order list: " + ex.Message, isError: true);
            }
        }

        private void RebindOrders(string statusPrefix = null)
        {
            try
            {
                gvListOfOrders.DataBind();
                RefreshStatus(statusPrefix);
                upnlOrderEntry.Update();
            }
            catch (Exception ex)
            {
                SetStatus("Could not load orders: " + ex.Message, isError: true);
                upnlOrderEntry.Update();
            }
        }

        protected void chkbxOrderDone_CheckedChanged(object sender, EventArgs e)
        {
            gvListOfOrders.PageIndex = 0;
            RebindOrders();
        }

        protected void btnGo_Click(object sender, EventArgs e)
        {
            gvListOfOrders.PageIndex = 0;
            RebindOrders();
        }

        protected void tbxSearchFor_TextChanged(object sender, EventArgs e)
        {
            gvListOfOrders.PageIndex = 0;
            RebindOrders();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            tbxSearchFor.Text = string.Empty;
            ddlSearchFor.SelectedIndex = 0;
            gvListOfOrders.PageIndex = 0;
            RebindOrders("Filters cleared.");
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(DefaultReturnUrl, endResponse: false);
            Context.ApplicationInstance.CompleteRequest();
        }

        public string GetItemDesc(int pItemID)
        {
            return pItemID > 0 ? new ItemsRepository().GetItemDescById(pItemID) : string.Empty;
        }

        public string FormatQuantity(double quantity)
        {
            return SystemConstants.FormatConstants.FormatQuantity(quantity);
        }
    }
}
