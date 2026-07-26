using AjaxControlToolkit;
using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Pages
{
    public partial class RepairDetail : Page
    {
        public const string CONST_URL_REQUEST_REPAIRID = "RepairID";
        private const string CONST_SESSION_REPAIRSTATUSID = "RepairStatusID";
        private const string SESSION_RETURN_URL = "RepairDetailReturnUrl";
        private const string DefaultReturnUrl = "~/Pages/Repairs.aspx";

        private readonly RepairManager _repairManager = new RepairManager();

        protected ScriptManager scrmRepairDetail;
        protected UpdateProgress udtpRepairDetail;
        protected UpdatePanel upnlRepairDetail;
        protected HiddenField hdnRepairDirty;
        protected Panel pnlRepairShell;
        protected Panel pnlNewRepair;
        protected ComboBox cboNewCompany;
        protected Button btnInsert;
        protected Button btnCancelInsert;
        protected Panel pnlRepairDetail;
        protected ComboBox cboCompany;
        protected TextBox tbxContactName;
        protected TextBox tbxContactEmail;
        protected TextBox tbxJobCardNumber;
        protected DropDownList ddlEquipTypes;
        protected TextBox tbxMachineSerialNumber;
        protected DropDownList ddlSwopOutMachine;
        protected DropDownList ddlMachineCondtion;
        protected CheckBox cbxTakenFrother;
        protected CheckBox cbxTakenBeanLid;
        protected CheckBox cbxTakenWaterLid;
        protected CheckBox cbxBrokenFrother;
        protected CheckBox cbxBrokenBeanLid;
        protected CheckBox cbxBrokenWaterLid;
        protected DropDownList ddlRepairFault;
        protected TextBox tbxRepairFaultDesc;
        protected DropDownList ddlRepairStatuses;
        protected TextBox tbxNotes;
        protected Label lblRepairID;
        protected Label lblRelatedOrderLineID;
        protected Label lblDateLogged;
        protected Label lblLastChanged;
        protected Button btnUpdate;
        protected Button btnUpdateAndReturn;
        protected Button btnDelete;
        protected Button btnCancel;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatusMessage;
        protected Literal ltrlStatus;
        protected ObjectDataSource odsCompanys;
        protected ObjectDataSource odsCompanyDemos;
        protected ObjectDataSource odsEquipTypes;
        protected ObjectDataSource odsRepairFaults;
        protected ObjectDataSource odsRepairStatuses;
        protected ObjectDataSource odsMachineConditions;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;

            CaptureReturnUrlIfNeeded();

            if (this.Request.QueryString["RepairID"] != null)
            {
                this.pnlNewRepair.Visible = false;
                this.pnlRepairDetail.Visible = true;
                this.lblRepairID.Text = this.Request.QueryString["RepairID"].ToString();
                this.PutDataFromForm(Convert.ToInt32(this.lblRepairID.Text));
                this.btnDelete.Enabled = Membership.GetUser() != null
                    && Membership.GetUser().UserName.ToLower() == "warren";
                this.upnlRepairDetail.Update();
            }
            else
            {
                this.pnlNewRepair.Visible = true;
                this.pnlRepairDetail.Visible = false;
                this.upnlRepairDetail.Update();
            }
        }

        private void CaptureReturnUrlIfNeeded()
        {
            string qsReturn = Request.QueryString["ReturnUrl"];
            if (!string.IsNullOrWhiteSpace(qsReturn) && TryNormalizeLocalReturnUrl(qsReturn, out string fromQuery))
            {
                Session[SESSION_RETURN_URL] = fromQuery;
                return;
            }

            if (Request.UrlReferrer != null)
            {
                string referrer = Request.UrlReferrer.ToString();
                if (referrer.IndexOf("RepairDetail.aspx", StringComparison.OrdinalIgnoreCase) < 0
                    && IsSafeReturnUrl(referrer))
                {
                    Session[SESSION_RETURN_URL] = referrer;
                    return;
                }
            }

            if (Session[SESSION_RETURN_URL] == null)
                Session[SESSION_RETURN_URL] = ResolveUrl(DefaultReturnUrl);
        }

        private string GetReturnUrl()
        {
            string url = Session[SESSION_RETURN_URL] as string;
            if (string.IsNullOrWhiteSpace(url) || !IsSafeReturnUrl(url))
                url = ResolveUrl(DefaultReturnUrl);
            return url;
        }

        private void ReturnToCaller()
        {
            Response.Redirect(GetReturnUrl(), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private bool TryNormalizeLocalReturnUrl(string candidate, out string normalized)
        {
            normalized = null;
            if (string.IsNullOrWhiteSpace(candidate))
                return false;

            candidate = candidate.Trim();
            if (candidate.StartsWith("~/") || (candidate.StartsWith("/") && !candidate.StartsWith("//")))
            {
                normalized = ResolveUrl(candidate.StartsWith("~/") ? candidate : "~" + candidate);
                return IsSafeReturnUrl(normalized);
            }

            if (IsSafeReturnUrl(candidate))
            {
                normalized = candidate;
                return true;
            }

            return false;
        }

        private bool IsSafeReturnUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (url.StartsWith("~/") || (url.StartsWith("/") && !url.StartsWith("//")))
                return url.IndexOf("://", StringComparison.Ordinal) < 0;

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri absolute))
                return false;

            return Request.Url != null
                && string.Equals(absolute.Host, Request.Url.Host, StringComparison.OrdinalIgnoreCase);
        }

        private void SetStatus(string message, bool? isError = null)
        {
            ltrlStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);
            if (pnlStatusMessage == null)
                return;

            if (string.IsNullOrEmpty(message))
            {
                pnlStatusMessage.Attributes["class"] = "status-message";
                return;
            }

            if (isError == true)
                pnlStatusMessage.Attributes["class"] = "status-message status-error";
            else if (isError == false)
                pnlStatusMessage.Attributes["class"] = "status-message status-success";
            else
                pnlStatusMessage.Attributes["class"] = "status-message status-info";
        }

        private void ClearDirtyState()
        {
            if (hdnRepairDirty != null)
                hdnRepairDirty.Value = "0";

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                "repairDetailClearDirty",
                "if (window.TrackerUnsaved) { TrackerUnsaved.clearDirty(); } else if (window.repairDetailClearDirty) { repairDetailClearDirty(); }",
                true);
        }

        private void PutDataFromForm(int pRepairID)
        {
            RepairFormData repairById = _repairManager.GetRepairFormDataById(pRepairID);
            if (repairById == null)
                return;
            this.lblRepairID.Text = repairById.RepairID.ToString();
            this.cboCompany.DataBind();
            this.ddlEquipTypes.DataBind();
            this.ddlMachineCondtion.DataBind();
            this.ddlRepairFault.DataBind();
            this.ddlRepairStatuses.DataBind();
            this.ddlSwopOutMachine.DataBind();
            this.cboCompany.SelectedValue = repairById.CustomerID.ToString();
            this.tbxContactName.Text = repairById.ContactName;
            this.tbxContactEmail.Text = repairById.ContactEmail;
            this.tbxJobCardNumber.Text = repairById.JobCardNumber;
            this.ddlEquipTypes.SelectedValue = repairById.MachineTypeID.ToString();
            this.tbxMachineSerialNumber.Text = repairById.MachineSerialNumber;
            this.ddlSwopOutMachine.SelectedValue = repairById.SwopOutMachineID.ToString();
            this.ddlMachineCondtion.SelectedValue = repairById.MachineConditionID.ToString();
            this.cbxTakenFrother.Checked = repairById.TakenFrother;
            this.cbxTakenBeanLid.Checked = repairById.TakenBeanLid;
            this.cbxTakenWaterLid.Checked = repairById.TakenWaterLid;
            this.cbxBrokenFrother.Checked = repairById.BrokenFrother;
            this.cbxBrokenBeanLid.Checked = repairById.BrokenBeanLid;
            this.cbxBrokenWaterLid.Checked = repairById.BrokenWaterLid;
            this.ddlRepairFault.SelectedValue = repairById.RepairFaultID.ToString();
            this.tbxRepairFaultDesc.Text = repairById.RepairFaultDesc;
            this.ddlRepairStatuses.SelectedValue = repairById.RepairStatusID.ToString();
            this.tbxNotes.Text = repairById.Notes;
            this.lblDateLogged.Text = $"{repairById.DateLogged:d}";
            this.lblLastChanged.Text = $"{repairById.LastStatusChange:d}";
            this.lblRelatedOrderLineID.Text = repairById.RelatedOrderLineID.ToString();
            this.Session[CONST_SESSION_REPAIRSTATUSID] = (object)repairById.RepairStatusID;
            ClearDirtyState();
        }

        private RepairFormData GetDataFromForm()
        {
            return new RepairFormData
            {
                RepairID = Convert.ToInt32(this.lblRepairID.Text),
                CustomerID = Convert.ToInt32(this.cboCompany.SelectedValue),
                ContactName = this.tbxContactName.Text,
                ContactEmail = this.tbxContactEmail.Text,
                JobCardNumber = this.tbxJobCardNumber.Text,
                MachineTypeID = Convert.ToInt32(this.ddlEquipTypes.SelectedValue),
                MachineSerialNumber = this.tbxMachineSerialNumber.Text,
                SwopOutMachineID = Convert.ToInt32(this.ddlSwopOutMachine.SelectedValue),
                MachineConditionID = Convert.ToInt32(this.ddlMachineCondtion.SelectedValue),
                TakenFrother = this.cbxTakenFrother.Checked,
                TakenBeanLid = this.cbxTakenBeanLid.Checked,
                TakenWaterLid = this.cbxTakenWaterLid.Checked,
                BrokenFrother = this.cbxBrokenFrother.Checked,
                BrokenBeanLid = this.cbxBrokenBeanLid.Checked,
                BrokenWaterLid = this.cbxBrokenWaterLid.Checked,
                RepairFaultID = Convert.ToInt32(this.ddlRepairFault.SelectedValue),
                RepairFaultDesc = this.tbxRepairFaultDesc.Text,
                RepairStatusID = Convert.ToInt32(this.ddlRepairStatuses.SelectedValue),
                Notes = this.tbxNotes.Text,
                DateLogged = Convert.ToDateTime(this.lblDateLogged.Text).Date,
                LastStatusChange = Convert.ToDateTime(this.lblLastChanged.Text).Date,
                RelatedOrderLineID = Convert.ToInt32(this.lblRelatedOrderLineID.Text)
            };
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            if (this.cboNewCompany.SelectedIndex <= 0)
            {
                SetStatus("Select a customer before inserting.", true);
                upnlRepairDetail.Update();
                return;
            }

            int contactId = Convert.ToInt32(this.cboNewCompany.SelectedValue);
            int repairId = _repairManager.CreateRepairForContact(contactId);
            if (repairId <= 0)
            {
                SetStatus("Could not create repair.", true);
                upnlRepairDetail.Update();
                return;
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"New repair created for ContactID {contactId}, RepairID {repairId}");
            this.pnlNewRepair.Visible = false;
            this.pnlRepairDetail.Visible = true;
            this.PutDataFromForm(repairId);
            SetStatus("Repair created. Complete the details and save.", false);
            this.upnlRepairDetail.Update();
        }

        private bool TryUpdateRecord(out string message)
        {
            message = null;
            RepairFormData dataFromForm = this.GetDataFromForm();
            int previousStatusId = this.Session[CONST_SESSION_REPAIRSTATUSID] != null
                ? (int)this.Session[CONST_SESSION_REPAIRSTATUSID]
                : 0;

            string result = _repairManager.HandleStatusChange(dataFromForm);

            // Repo update failure returns ErrorUpdating before email; email failure returns a summary after save.
            string updateError = MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);
            if (!string.IsNullOrWhiteSpace(result)
                && !string.IsNullOrWhiteSpace(updateError)
                && string.Equals(result.Trim(), updateError.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"RepairID {dataFromForm.RepairID} update failed: {result}");
                message = result;
                return false;
            }

            if (dataFromForm.RepairStatusID != previousStatusId)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"RepairID {dataFromForm.RepairID} status changed from {previousStatusId} to {dataFromForm.RepairStatusID}");
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                    $"RepairID {dataFromForm.RepairID} updated (no status change)");
            }

            this.Session[CONST_SESSION_REPAIRSTATUSID] = dataFromForm.RepairStatusID;
            message = string.IsNullOrWhiteSpace(result) ? "Record updated." : result;
            return true;
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!TryUpdateRecord(out string message))
            {
                SetStatus(message ?? "Update failed.", true);
                upnlRepairDetail.Update();
                return;
            }

            ClearDirtyState();
            SetStatus(message, false);
            upnlRepairDetail.Update();
        }

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            if (!TryUpdateRecord(out string message))
            {
                SetStatus(message ?? "Update failed.", true);
                upnlRepairDetail.Update();
                return;
            }

            ClearDirtyState();
            if (!string.IsNullOrWhiteSpace(message) &&
                message.IndexOf("Record updated", StringComparison.OrdinalIgnoreCase) < 0)
            {
                new showMessageBox(this.Page, "Repair Status Update", message);
            }

            ReturnToCaller();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            _repairManager.DeleteRepair(Convert.ToInt32(this.lblRepairID.Text));
            Session[SESSION_RETURN_URL] = ResolveUrl(DefaultReturnUrl);
            ReturnToCaller();
        }

        protected void btnCancel_Click(object sender, EventArgs e) => ReturnToCaller();
    }
}
