using AjaxControlToolkit;
using System;
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
        private static string prevPage = string.Empty;
        private readonly RepairManager _repairManager = new RepairManager();

        protected ScriptManager scrmRepairDetail;
        protected UpdateProgress udtpRepairDetail;
        protected UpdatePanel upnlRepairDetail;
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
        protected Label lblRelatedOrderID;
        protected Label lblDateLogged;
        protected Label lblLastChanged;
        protected Button btnUpdateAndReturn;
        protected Button btnDelete;
        protected Button btnCancel;
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
            RepairDetail.prevPage = !(this.Request.UrlReferrer == (Uri)null) ? this.Request.UrlReferrer.ToString() : string.Empty;
            if (this.Request.QueryString["RepairID"] != null)
            {
                this.pnlNewRepair.Visible = false;
                this.pnlRepairDetail.Visible = true;
                this.lblRepairID.Text = this.Request.QueryString["RepairID"].ToString();
                this.PutDataFromForm(Convert.ToInt32(this.lblRepairID.Text));
                this.upnlRepairDetail.Update();
                this.btnDelete.Enabled = Membership.GetUser().UserName.ToLower() == "warren";
            }
            else
            {
                this.pnlNewRepair.Visible = true;
                this.pnlRepairDetail.Visible = false;
                this.upnlRepairDetail.Update();
            }
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
            this.lblRelatedOrderID.Text = repairById.RelatedOrderID.ToString();
            this.Session["RepairStatusID"] = (object)repairById.RepairStatusID;
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
                RelatedOrderID = Convert.ToInt32(this.lblRelatedOrderID.Text)
            };
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            if (this.cboNewCompany.SelectedIndex <= 0)
                return;

            int contactId = Convert.ToInt32(this.cboNewCompany.SelectedValue);
            int repairId = _repairManager.CreateRepairForContact(contactId);
            if (repairId <= 0)
                return;

            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"New repair created for ContactID {contactId}, RepairID {repairId}");
            this.pnlNewRepair.Visible = false;
            this.pnlRepairDetail.Visible = true;
            this.PutDataFromForm(repairId);
            this.upnlRepairDetail.Update();
        }

        private void UpdateRecord()
        {
            RepairFormData dataFromForm = this.GetDataFromForm();
            int previousStatusId = this.Session["RepairStatusID"] != null ? (int)this.Session["RepairStatusID"] : 0;

            string result = _repairManager.HandleStatusChange(dataFromForm);

            if (string.IsNullOrWhiteSpace(result))
            {
                if (dataFromForm.RepairStatusID != previousStatusId)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"RepairID {dataFromForm.RepairID} status changed from {previousStatusId} to {dataFromForm.RepairStatusID}");
                }
                else
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"RepairID {dataFromForm.RepairID} updated (no status change)");
                }
            }
            else
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"RepairID {dataFromForm.RepairID} update failed: {result}");
            }

            upnlRepairDetail.Update();
        }

        private void ReturnToPrevPage() => this.ReturnToPrevPage(false);

        private void ReturnToPrevPage(bool pGoToRepairs)
        {
            if (pGoToRepairs || string.IsNullOrWhiteSpace(RepairDetail.prevPage))
                this.Response.Redirect("~/Pages/Repairs.aspx");
            else
                this.Response.Redirect(RepairDetail.prevPage);
        }

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            this.UpdateRecord();
            string status = this.ltrlStatus.Text;

            if (!string.IsNullOrWhiteSpace(status) && !status.Contains("Record Updated"))
            {
                showMessageBox msgBox = new showMessageBox(this.Page, "Repair Status Update", status);
            }
            this.ReturnToPrevPage();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            _repairManager.DeleteRepair(Convert.ToInt32(this.lblRepairID.Text));
            this.ReturnToPrevPage(true);
        }

        protected void btnCancel_Click(object sender, EventArgs e) => this.ReturnToPrevPage();
    }
}
