using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes; // For SystemConstants and AppLogger
using TrackerSQL.Controls;
using TrackerSQL.Managers; // For CustomersAwayTbl

namespace TrackerSQL.Pages
{
    public partial class CustomersAwayDetail : Page
    {
        protected global::System.Web.UI.WebControls.TextBox tbxAwayStartDate;
        protected global::System.Web.UI.WebControls.TextBox tbxAwayEndDate;
        protected global::System.Web.UI.WebControls.Literal ltrlStatus;
        protected global::AjaxControlToolkit.ComboBox cboCustomer;
        protected global::System.Web.UI.WebControls.DropDownList ddlReason;
        protected global::System.Web.UI.WebControls.Button btnInsert;
        protected global::System.Web.UI.WebControls.Button btnDelete;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int awayPeriodId;
                int customerId;
                if (int.TryParse(Request.QueryString["AwayPeriodID"], out awayPeriodId))
                {
                    btnInsert.Text = "Update";
                    btnDelete.Visible = true;
                    LoadAwayPeriod(awayPeriodId);
                }
                else
                {
                    btnInsert.Text = "Insert";
                    btnDelete.Visible = false;
                    tbxAwayStartDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    tbxAwayEndDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    if (int.TryParse(Request.QueryString["CustomerID"], out customerId))
                    {
                        cboCustomer.SelectedValue = customerId.ToString();
                    }
                }
            }
        }

        private void LoadAwayPeriod(int awayPeriodId)
        {
            var dal = new CustomersAwayTbl();
            var period = dal.GetAwayPeriodById(awayPeriodId);
            if (period != null)
            {
                cboCustomer.SelectedValue = period.ClientID.ToString();
                tbxAwayStartDate.Text = period.AwayStartDate.ToString("yyyy-MM-dd");
                tbxAwayEndDate.Text = period.AwayEndDate.ToString("yyyy-MM-dd");
                ddlReason.SelectedValue = period.ReasonID.ToString();
            }
            else
            {
                ltrlStatus.Text = "<span style='color:red'>Could not load away period.</span>";
            }
        }
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            ltrlStatus.Text = "";
            int customerId, reasonId, awayPeriodId;
            DateTime startDate, endDate;
            bool isEdit = int.TryParse(Request.QueryString["AwayPeriodID"], out awayPeriodId);

            if (!int.TryParse(cboCustomer.SelectedValue, out customerId) || customerId == 0)
            {
                ltrlStatus.Text = "<span style='color:red'>Please select a customer.</span>";
                return;
            }
            if (!DateTime.TryParse(tbxAwayStartDate.Text, out startDate))
            {
                ltrlStatus.Text = "<span style='color:red'>Please enter a valid start date.</span>";
                return;
            }
            if (!DateTime.TryParse(tbxAwayEndDate.Text, out endDate))
            {
                ltrlStatus.Text = "<span style='color:red'>Please enter a valid end date.</span>";
                return;
            }
            if (endDate < startDate)
            {
                ltrlStatus.Text = "<span style='color:red'>End date cannot be before start date.</span>";
                return;
            }
            if (!int.TryParse(ddlReason.SelectedValue, out reasonId) || reasonId == 0)
            {
                ltrlStatus.Text = "<span style='color:red'>Please select a reason.</span>";
                return;
            }

            try
            {
                var dal = new CustomersAwayTbl();
                string result;
                if (isEdit)
                {
                    result = dal.UpdateAwayPeriod(awayPeriodId, customerId, startDate, endDate, reasonId);
                }
                else
                {
                    result = dal.InsertAwayPeriod(customerId, startDate, endDate, reasonId);
                }

                if (string.IsNullOrEmpty(result))
                {
                    string logMsg = $"{(isEdit ? "Updated" : "Added")} away period for CustomerID={customerId}: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}, ReasonID={reasonId}";
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, logMsg);

                    // Send confirmation email using helper
                    new CustomerManager().SendAwayPeriodConfirmationEmail(customerId, startDate, endDate);

                    // Show confirmation and redirect using showMessageBox (or JS as fallback)
                    new showMessageBox(this.Page, "Status", "Away period saved successfully.");
                    Response.Redirect("CustomersAway.aspx");
                }
                else
                {
                    ltrlStatus.Text = $"<span style='color:red'>Error: {result}</span>";
                }
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = $"<span style='color:red'>Exception: {ex.Message}</span>";
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Exception saving away period for a Customer: {ex.Message}");
            }
        }
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int awayPeriodId;
            if (int.TryParse(Request.QueryString["AwayPeriodID"], out awayPeriodId))
            {
                var dal = new CustomersAwayTbl();
                // You need to implement DeleteAwayPeriod in CustomersAwayTbl if not present
                string result = dal.DeleteAwayPeriod(awayPeriodId);
                if (string.IsNullOrEmpty(result))
                {
                    // Show confirmation and redirect using showMessageBox (or JS as fallback)
                    new showMessageBox(this.Page, "Status", "Away period deleted successfully.");
                    Response.Redirect("CustomersAway.aspx");
                }
                else
                {
                    new showMessageBox(this.Page, "Status", "Away period deletiopn failed check logs for reason.");
                    ltrlStatus.Text = $"<span style='color:red'>Error deleting: {result}</span>";
                }
                Response.Redirect("CustomersAway.aspx");
            }
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("CustomersAway.aspx");
        }
    }
}