using AjaxControlToolkit;
using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

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
        protected ImageButton btnCancelInsert;
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
        protected HiddenField hdnRelatedOrderLineID;
        protected System.Web.UI.HtmlControls.HtmlAnchor lnkRelatedOrder;
        protected Label lblDateLogged;
        protected Label lblLastChanged;
        protected Button btnUpdate;
        protected Button btnUpdateAndReturn;
        protected Button btnDelete;
        protected ImageButton btnCancel;
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
                if (this.Request.QueryString["new"] == "1")
                    SetStatus("Repair created. Complete the details and save.", false);
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
            ReturnUrlHelper.CaptureIfNeeded(this, SESSION_RETURN_URL, DefaultReturnUrl, "RepairDetail.aspx");
        }

        private string GetReturnUrl()
        {
            return ReturnUrlHelper.Get(this, SESSION_RETURN_URL, DefaultReturnUrl);
        }

        private void ReturnToCaller()
        {
            Response.Redirect(GetReturnUrl(), false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void SetStatus(string message, bool? isError = null)
        {
            StatusMessageHelper.Set(pnlStatusMessage, ltrlStatus, HttpUtility.HtmlEncode(message ?? string.Empty), isError);
            if (pnlStatusMessage != null && !string.IsNullOrWhiteSpace(message))
                pnlStatusMessage.Visible = true;
        }

        /// <summary>
        /// Writes a repairs.log audit line. AppLogger already prefixes the logged-in user.
        /// </summary>
        private void LogRepairAudit(string action, string details = null, int? repairIdOverride = null, int? contactIdOverride = null)
        {
            int repairId = repairIdOverride
                ?? (int.TryParse(lblRepairID?.Text, out int parsedId) ? parsedId : 0);

            int contactId = contactIdOverride ?? 0;
            if (contactId <= 0 && cboCompany != null && cboCompany.SelectedIndex > 0)
                int.TryParse(cboCompany.SelectedValue, out contactId);
            if (contactId <= 0 && cboNewCompany != null && cboNewCompany.SelectedIndex > 0)
                int.TryParse(cboNewCompany.SelectedValue, out contactId);

            string company = string.Empty;
            if (cboCompany?.SelectedItem != null && cboCompany.SelectedIndex > 0)
                company = cboCompany.SelectedItem.Text?.Trim() ?? string.Empty;
            else if (cboNewCompany?.SelectedItem != null && cboNewCompany.SelectedIndex > 0)
                company = cboNewCompany.SelectedItem.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(company) && contactId > 0)
                company = new ContactsRepository().GetContactNameById(contactId) ?? string.Empty;

            string repairPart = repairId > 0 ? $"Repair {repairId}" : "New repair";
            string contactPart = contactId > 0
                ? (string.IsNullOrWhiteSpace(company)
                    ? $"Contact={contactId}"
                    : $"Contact={contactId} ({company})")
                : "Contact=(none)";

            string line = $"{repairPart} | {contactPart} | {action}";
            if (!string.IsNullOrWhiteSpace(details))
                line += $" | {details}";

            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, line);
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
            SetRelatedOrderLineLink(repairById.RelatedOrderLineID);
            this.Session[CONST_SESSION_REPAIRSTATUSID] = (object)repairById.RepairStatusID;
            ClearDirtyState();
        }

        /// <summary>
        /// Shows the related order-line id as a real &lt;a&gt; that opens Order Detail.
        /// (asp:HyperLink with an empty NavigateUrl renders as a &lt;span&gt; — looks like
        /// text and is not clickable, which is what users were seeing.)
        /// </summary>
        private void SetRelatedOrderLineLink(int relatedOrderLineId)
        {
            hdnRelatedOrderLineID.Value = relatedOrderLineId.ToString();
            lnkRelatedOrder.InnerText = relatedOrderLineId > 0 ? relatedOrderLineId.ToString() : "—";
            lnkRelatedOrder.HRef = string.Empty;
            lnkRelatedOrder.Title = string.Empty;
            lnkRelatedOrder.Attributes["class"] = "repair-related-order-link is-disabled";

            if (relatedOrderLineId <= 0)
                return;

            var ordersRepo = new OrdersRepository();
            int? orderId = ordersRepo.GetOrderIdByLineId(relatedOrderLineId);

            // Legacy repairs may still store an OrderID in RelatedOrderLineID
            if ((!orderId.HasValue || orderId.Value <= 0)
                && ordersRepo.OrderExists(relatedOrderLineId))
            {
                orderId = relatedOrderLineId;
            }

            if (!orderId.HasValue || orderId.Value <= 0)
            {
                lnkRelatedOrder.Title = "No matching order found for line " + relatedOrderLineId;
                return;
            }

            lnkRelatedOrder.HRef = ResolveUrl(
                "~/Pages/OrderDetail.aspx?" + OrderDetail.CONST_QRYSTR_ORDERID + "=" + orderId.Value);
            lnkRelatedOrder.Title = "Open order " + orderId.Value;
            lnkRelatedOrder.Attributes["class"] = "repair-related-order-link";
            lnkRelatedOrder.InnerText = relatedOrderLineId + " → order " + orderId.Value;
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
                RelatedOrderLineID = ParseRelatedOrderLineId()
            };
        }

        private int ParseRelatedOrderLineId()
        {
            int lineId;
            return int.TryParse(hdnRelatedOrderLineID.Value, out lineId) ? lineId : 0;
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            if (this.cboNewCompany.SelectedIndex <= 0)
            {
                SetStatus("Select a customer before inserting.", true);
                if (btnInsert != null)
                {
                    btnInsert.Enabled = true;
                    btnInsert.Text = "Insert";
                }
                upnlRepairDetail.Update();
                return;
            }

            int contactId = Convert.ToInt32(this.cboNewCompany.SelectedValue);
            int repairId = _repairManager.CreateRepairForContact(contactId);
            if (repairId <= 0)
            {
                SetStatus("Could not create repair.", true);
                if (btnInsert != null)
                {
                    btnInsert.Enabled = true;
                    btnInsert.Text = "Insert";
                }
                upnlRepairDetail.Update();
                return;
            }

            LogRepairAudit("Repair created", null, repairIdOverride: repairId, contactIdOverride: contactId);

            // Full redirect (PostBackTrigger) so refresh does not re-post Insert and create a second repair.
            Response.Redirect("RepairDetail.aspx?RepairID=" + repairId + "&new=1", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private bool TryUpdateRecord(out string message)
        {
            message = null;
            RepairFormData dataFromForm = this.GetDataFromForm();
            int previousStatusId = this.Session[CONST_SESSION_REPAIRSTATUSID] != null
                ? (int)this.Session[CONST_SESSION_REPAIRSTATUSID]
                : 0;

            // ZZName must have a walk-in name before any save (repair row or related order).
            string sundryError = _repairManager.ValidateSundryContactName(dataFromForm);
            if (!string.IsNullOrEmpty(sundryError))
            {
                message = sundryError;
                return false;
            }

            // A repair needs a related order (repair-check line, delivery in 7 days). Create it
            // before saving so the new RelatedOrderLineID is stored with the repair. For ZZName
            // this also writes "contact name: [RepairStatus: …]" into the order notes.
            string orderNote = _repairManager.EnsureRelatedOrder(dataFromForm);
            SetRelatedOrderLineLink(dataFromForm.RelatedOrderLineID);

            string result = _repairManager.HandleStatusChange(dataFromForm);

            // Keep Name: / [RepairStatus: …] on the related order in sync when saving from Repair Detail
            // (HandleStatusChange itself does not touch order notes — status-change page stays lean).
            if (dataFromForm.RelatedOrderLineID > 0)
                _repairManager.SyncRelatedOrderNotes(dataFromForm);

            // Repo update failure returns ErrorUpdating before email; email failure returns a summary after save.
            string updateError = MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);
            if (!string.IsNullOrWhiteSpace(result)
                && !string.IsNullOrWhiteSpace(updateError)
                && string.Equals(result.Trim(), updateError.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                LogRepairAudit("Update failed", result, contactIdOverride: (int)dataFromForm.CustomerID);
                message = result;
                return false;
            }

            if (dataFromForm.RepairStatusID != previousStatusId)
            {
                LogRepairAudit(
                    "Status changed",
                    $"from={previousStatusId} to={dataFromForm.RepairStatusID}",
                    contactIdOverride: (int)dataFromForm.CustomerID);
            }
            else
            {
                LogRepairAudit("Repair saved", null, contactIdOverride: (int)dataFromForm.CustomerID);
            }

            // Status email success/fail is logged in RepairManager.SendStatusNotification → repairs.log

            this.Session[CONST_SESSION_REPAIRSTATUSID] = dataFromForm.RepairStatusID;
            message = string.IsNullOrWhiteSpace(result) ? "Record updated." : result;
            if (!string.IsNullOrWhiteSpace(orderNote))
                message += " " + orderNote;
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
            int repairId = Convert.ToInt32(this.lblRepairID.Text);
            LogRepairAudit("Repair deleted");
            _repairManager.DeleteRepair(repairId);
            Session[SESSION_RETURN_URL] = ResolveUrl(DefaultReturnUrl);
            ReturnToCaller();
        }

        protected void btnCancel_Click(object sender, ImageClickEventArgs e) => ReturnToCaller();
    }
}
