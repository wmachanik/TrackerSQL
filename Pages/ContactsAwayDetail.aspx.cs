using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;
using TrackerSQL.Managers;

namespace TrackerSQL.Pages
{
    public partial class ContactsAwayDetail : Page
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
                // Bind dropdowns using SQL Server repositories
                BindCustomerDropdown();
                BindReasonDropdown();

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

        private void BindCustomerDropdown()
        {
            try
            {
                var repo = new ContactsRepository();
                var contacts = repo.GetAllCompanyNames();
                cboCustomer.DataSource = contacts;
                cboCustomer.DataBind();
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = $"<span style='color:red'>Error loading customers: {ex.Message}</span>";
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Error loading customers dropdown: {ex.Message}");
            }
        }

        private void BindReasonDropdown()
        {
            try
            {
                var repo = new AwayReasonRepository();
                var reasons = repo.GetAll("ReasonDesc");
                ddlReason.DataSource = reasons;
                ddlReason.DataBind();
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = $"<span style='color:red'>Error loading reasons: {ex.Message}</span>";
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Error loading reasons dropdown: {ex.Message}");
            }
        }

        private void LoadAwayPeriod(int awayPeriodId)
        {
            try
            {
                var repo = new ContactsAwayPeriodRepository();
                var period = repo.GetById(awayPeriodId);
                if (period != null)
                {
                    cboCustomer.SelectedValue = period.ContactID.ToString();
                    tbxAwayStartDate.Text = period.AwayStartDate.ToString("yyyy-MM-dd");
                    tbxAwayEndDate.Text = period.AwayEndDate.ToString("yyyy-MM-dd");
                    if (period.ReasonID.HasValue)
                    {
                        ddlReason.SelectedValue = period.ReasonID.Value.ToString();
                    }
                }
                else
                {
                    ltrlStatus.Text = "<span style='color:red'>Could not load away period.</span>";
                }
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = $"<span style='color:red'>Error loading away period: {ex.Message}</span>";
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Error loading away period {awayPeriodId}: {ex.Message}");
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
                var repo = new ContactsAwayPeriodRepository();
                var awayPeriod = new ContactsAwayPeriod
                {
                    ContactID = customerId,
                    AwayStartDate = startDate,
                    AwayEndDate = endDate,
                    ReasonID = reasonId
                };

                if (isEdit)
                {
                    awayPeriod.AwayPeriodID = awayPeriodId;
                    repo.Update(awayPeriod);
                }
                else
                {
                    repo.Insert(awayPeriod);
                }

                string logMsg = $"{(isEdit ? "Updated" : "Added")} away period for ContactID={customerId}: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}, ReasonID={reasonId}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, logMsg);

                // Send confirmation email using helper
                try
                {
                    new CustomerManager().SendAwayPeriodConfirmationEmail(customerId, startDate, endDate);
                }
                catch (Exception emailEx)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Email send failed (non-critical): {emailEx.Message}");
                }

                Response.Redirect("ContactsAway.aspx");
            }
            catch (Exception ex)
            {
                ltrlStatus.Text = $"<span style='color:red'>Exception: {ex.Message}</span>";
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Exception saving away period for ContactID={customerId}: {ex.Message}");
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int awayPeriodId;
            if (int.TryParse(Request.QueryString["AwayPeriodID"], out awayPeriodId))
            {
                try
                {
                    var repo = new ContactsAwayPeriodRepository();
                    repo.Delete(awayPeriodId);

                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"Deleted away period ID={awayPeriodId}");
                    Response.Redirect("ContactsAway.aspx");
                }
                catch (Exception ex)
                {
                    ltrlStatus.Text = $"<span style='color:red'>Error deleting: {ex.Message}</span>";
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Error deleting away period {awayPeriodId}: {ex.Message}");
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("ContactsAway.aspx");
        }
    }
}




