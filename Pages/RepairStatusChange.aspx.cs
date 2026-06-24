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
        private static string prevPage = string.Empty;
        private readonly RepairManager _repairManager = new RepairManager();

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
            RepairStatusChange.prevPage = !(this.Request.UrlReferrer == (Uri)null) ? this.Request.UrlReferrer.ToString() : string.Empty;
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
            if (string.IsNullOrWhiteSpace(RepairStatusChange.prevPage))
                this.Response.Redirect("~/Pages/Repairs.aspx");
            else
                this.Response.Redirect(RepairStatusChange.prevPage);
        }

        private void UpdateRecord()
        {
            RepairFormData pRepair = (RepairFormData)this.Session["RepairDataUsed"];
            int int32 = Convert.ToInt32(this.ddlRepairStatuses.SelectedValue);
            if (pRepair.RepairStatusID == int32)
                return;
            
            pRepair.RepairStatusID = int32;
            
            string result = _repairManager.HandleStatusChange(pRepair);
            
            if (!string.IsNullOrWhiteSpace(result))
                this.ltrlStatus.Text = result;
            else
                this.ltrlStatus.Text = MessageProvider.Get(MessageKeys.Repairs.StatusUpdateSuccess);
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

        protected void btnCancel_Click(object sender, EventArgs e) => this.ReturnToPrevPage();
    }
}
