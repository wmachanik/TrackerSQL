// Decompiled with JetBrains decompiler
// Type: TrackerSQL.Pages.RepairDetail
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using AjaxControlToolkit;
using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Controls;
using TrackerSQL.Managers;

//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public partial class RepairDetail : Page
    {
        public const string CONST_URL_REQUEST_REPAIRID = "RepairID";
        private const string CONST_SESSION_REPAIRSTATUSID = "RepairStatusID";
        private static string prevPage = string.Empty;
        protected ScriptManager scrmRepairDetail;
        protected UpdateProgress udtpRepairDetail;
        protected UpdatePanel upnlRepairDetail;
        protected Panel pnlNewRepair;
        //protected DropDownList ddlNewCompany;
        protected ComboBox cboNewCompany;
        protected Button btnInsert;
        protected Button btnCancelInsert;
        protected Panel pnlRepairDetail;
        //protected DropDownList ddlCompany;
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

        protected void Page_PreInit(object sender, EventArgs e)
        {
            //if (new CheckBrowser().fBrowserIsMobile())
            //    this.MasterPageFile = "~/MobileSite.master";
            //else
            //    this.MasterPageFile = "~/Site.master";
        }

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
            RepairsTbl repairById = new RepairsTbl().GetRepairById(pRepairID);
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

        private RepairsTbl GetDataFromForm()
        {
            return new RepairsTbl()
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
            RepairsTbl DataItem = new RepairsTbl();
            if (this.cboNewCompany.SelectedIndex <= 0)
                return;
            DataItem.CustomerID = Convert.ToInt32(this.cboNewCompany.SelectedValue);
            DataItem.DateLogged = TimeZoneUtils.Now().Date;
            CustomersTbl customersByCustomerID = new CustomersTbl().GetCustomerByCustomerID(DataItem.CustomerID);
            DataItem.ContactName = customersByCustomerID.ContactFirstName;
            DataItem.ContactEmail = customersByCustomerID.EmailAddress;
            DataItem.MachineTypeID = customersByCustomerID.EquipType;
            DataItem.MachineSerialNumber = customersByCustomerID.MachineSN;
            DataItem.DateLogged = TimeZoneUtils.Now().Date;
            DataItem.InsertRepair(DataItem);
            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"New repair created for CustomerID {DataItem.CustomerID}, RepairID {DataItem.GetLastIDInserted(DataItem.CustomerID)}");
            this.pnlNewRepair.Visible = false;
            this.pnlRepairDetail.Visible = true;
            DataItem.RepairID = DataItem.GetLastIDInserted(DataItem.CustomerID);
            this.PutDataFromForm(DataItem.RepairID);
            this.upnlRepairDetail.Update();
        }

        private void UpdateRecord()
        {
            var repairManager = new RepairManager();
            RepairsTbl dataFromForm = this.GetDataFromForm();
            int previousStatusId = this.Session["RepairStatusID"] != null ? (int)this.Session["RepairStatusID"] : 0;

            string result = repairManager.HandleStatusChange(dataFromForm);

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

            // Show popup only if there's a relevant message
            if (!string.IsNullOrWhiteSpace(status) && !status.Contains("Record Updated"))
            {
                showMessageBox msgBox = new showMessageBox(this.Page, "Repair Status Update", status);
                AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"Repair Status Update: {status}");
            }
            this.ReturnToPrevPage();
        }

        protected void btnCancel_Click(object sender, EventArgs e) => this.ReturnToPrevPage();

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int repairId = Convert.ToInt32(this.lblRepairID.Text);
            new RepairsTbl().DeleteRepair(repairId);
            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs, $"RepairID {repairId} deleted");
            this.ReturnToPrevPage();
        }

        public void RepairUpdating(object source, ObjectDataSourceMethodEventArgs e)
        {
        }

        public void RowUpdated(object source, ObjectDataSourceStatusEventArgs e)
        {
            if (e.AffectedRows != 0)
                return;
            showMessageBox showMessageBox = new showMessageBox(this.Page, "nothing updated", "no records updated");
        }
    }
}