// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.ReoccuringOrderDetails
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using AjaxControlToolkit;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class ReoccuringOrderDetails : Page
    {
        private static string prevPage = string.Empty;
        protected Label lblReoccuringOrderID;
        protected ScriptManager smReoccuringOrderDetails;
        protected UpdateProgress uprgReoccuringOrderDetails;
        protected UpdatePanel upnlReoccuringOrderDetails;
        protected DropDownList ddlCompanyName;
        protected Label ReoccuringOrderIDLabel;
        protected TextBox ValueTextBox;
        protected DropDownList ddlReoccuranceType;
        protected DropDownList ddlItemType;
        protected TextBox QuantityTextBox;
        protected TextBox UntilDateTextBox;
        protected CalendarExtender UntilDateTextBox_CalendarExtender;
        protected TextBox LastDateTextBox;
        protected CalendarExtender LastDateTextBox_CalendarExtender;
        protected DropDownList ddlPackagingTypes;
        protected CheckBox EnabledCheckBox;
        protected TextBox NotesTextBox;
        protected Label NextDateLabel;
        protected Button btnUpdate;
        protected Button btnUpdateAndReturn;
        protected Button btnInsert;
        protected Button btnDelete;
        protected Button btnCancel;
        protected Literal ltrlStatus;
        protected SqlDataSource sdsCompanys;
        protected ObjectDataSource odsItems;
        protected ObjectDataSource odsReoccuranceTypes;
        protected ObjectDataSource odsPackagingTypes;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;
            ReoccuringOrderDetails.prevPage = !(this.Request.UrlReferrer == (Uri)null) ? this.Request.UrlReferrer.ToString() : string.Empty;
            if (this.Request.QueryString["ID"] != null)
            {
                this.PutDataFromForm(Convert.ToInt32(this.Request.QueryString["ID"]));
            }
            else
            {
                this.btnUpdate.Enabled = false;
                this.btnUpdateAndReturn.Enabled = false;
                this.btnInsert.Enabled = true;
                this.btnDelete.Enabled = false;
                this.EnabledCheckBox.Checked = true;
            }
            // set the date Fomrat of the ajax boxes
            UntilDateTextBox_CalendarExtender.Format = SystemConstants.FormatConstants.DateFormat;
            LastDateTextBox_CalendarExtender.Format = SystemConstants.FormatConstants.DateFormat;
        }

        private void PutDataFromForm(int pReoccuringOrderID)
        {
            ReoccuringOrderTbl reoccuringOrderById = new ReoccuringOrderDAL().GetByReoccuringOrderByID(pReoccuringOrderID);
            if (reoccuringOrderById == null)
                return;
            this.ReoccuringOrderIDLabel.Text = reoccuringOrderById.ReoccuringOrderID.ToString();
            this.ddlCompanyName.SelectedValue = reoccuringOrderById.CustomerID.ToString();
            this.ValueTextBox.Text = reoccuringOrderById.ReoccuranceValue.ToString();
            this.ddlReoccuranceType.SelectedValue = reoccuringOrderById.ReoccuranceTypeID.ToString();
            this.ddlItemType.SelectedValue = reoccuringOrderById.ItemRequiredID.ToString();
            this.QuantityTextBox.Text = reoccuringOrderById.QtyRequired.ToString();
            this.UntilDateTextBox.Text = $"{reoccuringOrderById.RequireUntilDate:d}";
            this.LastDateTextBox.Text = $"{reoccuringOrderById.DateLastDone:d}";
            this.NextDateLabel.Text = $"{reoccuringOrderById.NextDateRequired:d}";
            this.ddlPackagingTypes.SelectedValue = reoccuringOrderById.PackagingID.ToString();
            this.EnabledCheckBox.Checked = reoccuringOrderById.Enabled;
            this.NotesTextBox.Text = reoccuringOrderById.Notes;
        }

        private ReoccuringOrderTbl GetDataFromForm()
        {
            ReoccuringOrderTbl dataFromForm = new ReoccuringOrderTbl();
            if (!string.IsNullOrEmpty(this.ReoccuringOrderIDLabel.Text))
                dataFromForm.ReoccuringOrderID = Convert.ToInt32(this.ReoccuringOrderIDLabel.Text);
            dataFromForm.CustomerID = Convert.ToInt32(this.ddlCompanyName.SelectedValue);
            dataFromForm.ReoccuranceValue = Convert.ToInt32(this.ValueTextBox.Text);
            dataFromForm.ReoccuranceTypeID = Convert.ToInt32(this.ddlReoccuranceType.SelectedValue);
            dataFromForm.ItemRequiredID = Convert.ToInt32(this.ddlItemType.SelectedValue);
            dataFromForm.QtyRequired = Convert.ToDouble(this.QuantityTextBox.Text);
            dataFromForm.RequireUntilDate = TrackerTools.ParseUserDate(this.UntilDateTextBox.Text);
            dataFromForm.DateLastDone = TrackerTools.ParseUserDate(this.LastDateTextBox.Text);
            dataFromForm.NextDateRequired = TrackerTools.ParseUserDate(this.NextDateLabel.Text);
            dataFromForm.PackagingID = Convert.ToInt32(this.ddlPackagingTypes.SelectedValue);
            dataFromForm.Enabled = this.EnabledCheckBox.Checked;
            dataFromForm.Notes = this.NotesTextBox.Text;
            return dataFromForm;
        }

        private void UpdateRecord()
        {
            string str = new ReoccuringOrderDAL().UpdateReoccuringOrder(this.GetDataFromForm(), Convert.ToInt32(this.ReoccuringOrderIDLabel.Text));
            this.ltrlStatus.Text = str == "" ? "Reoccuring Item Updated" : str;
            showMessageBox showMessageBox = new showMessageBox(this.Page, "Reoccurring Order Update", this.ltrlStatus.Text);
        }

        private void ReturnToPrevPage() => this.ReturnToPrevPage(false);

        private void ReturnToPrevPage(bool GoToReoccuringOrders)
        {
            if (GoToReoccuringOrders || string.IsNullOrWhiteSpace(ReoccuringOrderDetails.prevPage))
                this.Response.Redirect("~/Pages/ReoccuringOrders.aspx");
            else
                this.Response.Redirect(ReoccuringOrderDetails.prevPage);
        }

        protected void btnUpdate_Click(object sender, EventArgs e) => this.UpdateRecord();

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            this.UpdateRecord();
            this.ReturnToPrevPage();
        }

        protected void btnCancel_Click(object sender, EventArgs e) => this.ReturnToPrevPage();

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            string str = new ReoccuringOrderDAL().InsertReoccuringOrder(this.GetDataFromForm());
            this.ltrlStatus.Text = str == "" ? "Reoccuring Item Inserted" : "Error inserting: " + str;
            showMessageBox showMessageBox = new showMessageBox(this.Page, "Reoccurring Order Insert", this.ltrlStatus.Text);
            this.ReturnToPrevPage(true);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string str = new ReoccuringOrderDAL().DeleteReoccuringOrder(Convert.ToInt32(this.ReoccuringOrderIDLabel.Text));
            this.ltrlStatus.Text = str == "" ? "Reoccuring Item Deleted" : str;
            showMessageBox showMessageBox = new showMessageBox(this.Page, "Reoccurring Order Deleted", this.ltrlStatus.Text);
            this.ReturnToPrevPage(true);
        }
    }
}