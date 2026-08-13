using AjaxControlToolkit;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class RepairStatusChange : Page
    {
        private const string CONST_SESSION_REPAIRDATA = "RepairDataUsed";
        private const string CONST_VIEWSTATE_PREV_PAGE = "RepairStatusChangePreviousPage";
        private const string DEFAULT_RETURN_URL = "~/Pages/Repairs.aspx";
        private readonly RepairManager _repairManager = new RepairManager();

        private string ReturnPageUrl
        {
            get { return this.ViewState[CONST_VIEWSTATE_PREV_PAGE] as string ?? string.Empty; }
            set { this.ViewState[CONST_VIEWSTATE_PREV_PAGE] = value; }
        }

        protected ScriptManager scrmRepairStaus;
        protected UpdatePanel upnlRepairStaus;
        protected HtmlTable tblRepairStatus;
        protected Literal ltrlComapny;
        protected Literal ltrlMachine;
        protected Literal ltrlMachineSerialNumber;
        protected DropDownList ddlRepairStatuses;
        protected Label lblRepairID;
        protected Button btnUpdateAndReturn;
        protected Button btnCancel;
        protected Literal ltrlStatus;
        protected UpdateProgress udtpRepairStaus;
        protected ObjectDataSource odsEquipTypes;
        protected ObjectDataSource odsRepairStatuses;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;

            string requestedReturnUrl = this.Request.QueryString["ReturnUrl"];
            if (IsSafeReturnUrl(requestedReturnUrl)
                && requestedReturnUrl.IndexOf("RepairStatusChange.aspx", StringComparison.OrdinalIgnoreCase) < 0)
            {
                this.ReturnPageUrl = requestedReturnUrl;
            }
            else
            {
                string referrer = this.Request.UrlReferrer != null
                    ? this.Request.UrlReferrer.ToString()
                    : string.Empty;
                this.ReturnPageUrl = IsSafeReturnUrl(referrer)
                    && referrer.IndexOf("RepairStatusChange.aspx", StringComparison.OrdinalIgnoreCase) < 0
                    ? referrer
                    : this.ResolveUrl(DEFAULT_RETURN_URL);
            }

            if (this.Request.QueryString["RepairID"] == null)
                return;
            this.lblRepairID.Text = this.Request.QueryString["RepairID"].ToString();
            this.PutDataFromForm(Convert.ToInt32(this.lblRepairID.Text));
        }

        public string GetCompanyName(long pCompanyID)
        {
            return pCompanyID > 0L ? new ContactsRepository().GetContactNameById((int)pCompanyID) : string.Empty;
        }

        public string GetMachineDesc(int pEquipID)
        {
            return pEquipID > 0 ? new EquipTypesRepository().GetEquipTypeName(pEquipID) : string.Empty;
        }

        private void PutDataFromForm(int pRepairID)
        {
            RepairFormData repairById = _repairManager.GetRepairFormDataById(pRepairID);
            if (repairById == null)
                return;
            this.lblRepairID.Text = repairById.RepairID.ToString();
            this.ltrlComapny.Text = this.GetCompanyName(repairById.CustomerID);
            this.ltrlMachine.Text = this.GetMachineDesc(repairById.MachineTypeID);
            this.ddlRepairStatuses.DataBind();
            this.ltrlMachineSerialNumber.Text = repairById.MachineSerialNumber;
            this.ddlRepairStatuses.SelectedValue = repairById.RepairStatusID.ToString();
            this.Session["RepairDataUsed"] = repairById;
        }

        private void ReturnToPrevPage()
        {
            string returnUrl = IsSafeReturnUrl(this.ReturnPageUrl)
                && this.ReturnPageUrl.IndexOf("RepairStatusChange.aspx", StringComparison.OrdinalIgnoreCase) < 0
                ? this.ReturnPageUrl
                : this.ResolveUrl(DEFAULT_RETURN_URL);

            this.Response.Redirect(returnUrl, false);
            this.Context.ApplicationInstance.CompleteRequest();
        }

        private bool IsSafeReturnUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (url.StartsWith("~/") || (url.StartsWith("/") && !url.StartsWith("//")))
                return url.IndexOf("://", StringComparison.Ordinal) < 0;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri absolute))
                return false;

            return this.Request.Url != null
                && string.Equals(absolute.Host, this.Request.Url.Host, StringComparison.OrdinalIgnoreCase);
        }

        private bool UpdateRecord()
        {
            if (!int.TryParse(this.lblRepairID.Text, out int repairId) || repairId <= 0)
            {
                this.ltrlStatus.Text = "The repair ID is invalid. Reopen the repair and try again.";
                return false;
            }

            // Session state can expire or be cleared while this page is open. Always reload
            // the current database row so status changes do not depend on a cached form DTO.
            RepairFormData repair = _repairManager.GetRepairFormDataById(repairId);
            if (repair == null)
            {
                this.ltrlStatus.Text = "The repair record could not be loaded. Reopen the repair and try again.";
                return false;
            }

            if (!int.TryParse(this.ddlRepairStatuses.SelectedValue, out int selectedStatusId)
                || selectedStatusId <= 0)
            {
                this.ltrlStatus.Text = "Select a valid repair status.";
                return false;
            }

            if (repair.RepairStatusID == selectedStatusId)
                return true;

            int previousStatusId = repair.RepairStatusID;
            repair.RepairStatusID = selectedStatusId;
            repair.LastStatusChange = TimeZoneUtils.Now();

            string result = _repairManager.HandleStatusChange(repair);
            string updateError = MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);
            if (!string.IsNullOrWhiteSpace(result)
                && !string.IsNullOrWhiteSpace(updateError)
                && string.Equals(result.Trim(), updateError.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                this.ltrlStatus.Text = result;
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"Repair {repair.RepairID} | Contact={repair.CustomerID} | Update failed | {result}");
                return false;
            }

            this.Session[CONST_SESSION_REPAIRDATA] = repair;
            string company = new ContactsRepository().GetContactNameById((int)repair.CustomerID) ?? string.Empty;
            string contactPart = string.IsNullOrWhiteSpace(company)
                ? $"Contact={repair.CustomerID}"
                : $"Contact={repair.CustomerID} ({company})";
            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                $"Repair {repair.RepairID} | {contactPart} | Status changed | from={previousStatusId} to={selectedStatusId}");
            // Status email success/fail logged in RepairManager.SendStatusNotification
            this.ltrlStatus.Text = string.IsNullOrWhiteSpace(result)
                ? MessageProvider.Get(MessageKeys.Repairs.StatusUpdateSuccess)
                : result;
            return true;
        }

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.UpdateRecord())
                {
                    this.upnlRepairStaus.Update();
                    return;
                }

                this.ReturnToPrevPage();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"RepairID {this.lblRepairID.Text} status update failed: {ex.Message}");
                this.ltrlStatus.Text = "The repair status could not be updated. Please try again or check the repair log.";
                this.upnlRepairStaus.Update();
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e) => this.ReturnToPrevPage();
    }
}
