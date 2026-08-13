using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class ContactsAwayDetail : Page
    {
        private const string DefaultReturnUrl = "~/Pages/ContactsAway.aspx";
        private const string ViewStateAwayPeriodId = "ContactsAwayPeriodId";

        protected ScriptManager scrmContactsAwayDetail;
        protected UpdateProgress udtpContactsAwayDetail;
        protected UpdatePanel upnlContactsAwayDetail;
        protected Panel pnlAwayDetail;
        protected AjaxControlToolkit.ComboBox cboCustomer;
        protected TextBox tbxAwayStartDate;
        protected TextBox tbxAwayEndDate;
        protected DropDownList ddlReason;
        protected Button btnUpdate;
        protected Button btnUpdateAndReturn;
        protected Button btnDelete;
        protected ImageButton btnCancel;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindCustomerDropdown();
                BindReasonDropdown();

                int awayPeriodId = GetAwayPeriodId();
                if (awayPeriodId > 0)
                {
                    btnDelete.Visible = true;
                    LoadAwayPeriod(awayPeriodId);
                }
                else
                {
                    btnDelete.Visible = false;
                    tbxAwayStartDate.Text = TimeZoneUtils.Now().ToString("yyyy-MM-dd");
                    tbxAwayEndDate.Text = TimeZoneUtils.Now().ToString("yyyy-MM-dd");
                    if (int.TryParse(Request.QueryString["CustomerID"], out int customerId) && customerId > 0)
                    {
                        var item = cboCustomer.Items.FindByValue(customerId.ToString());
                        if (item != null)
                            cboCustomer.SelectedValue = customerId.ToString();
                    }
                }
            }
        }

        private int GetAwayPeriodId()
        {
            if (ViewState[ViewStateAwayPeriodId] is int fromView && fromView > 0)
                return fromView;

            if (int.TryParse(Request.QueryString["AwayPeriodID"], out int fromQuery) && fromQuery > 0)
            {
                ViewState[ViewStateAwayPeriodId] = fromQuery;
                return fromQuery;
            }

            return 0;
        }

        private void SetAwayPeriodId(int id)
        {
            ViewState[ViewStateAwayPeriodId] = id;
        }

        private void SetStatus(string message, bool? isError = null)
        {
            ltrlStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);

            if (pnlStatus == null)
                return;

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
                SetStatus("Error loading customers: " + ex.Message, true);
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
                SetStatus("Error loading reasons: " + ex.Message, true);
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
                    var item = cboCustomer.Items.FindByValue(period.ContactID.ToString());
                    if (item != null)
                        cboCustomer.SelectedValue = period.ContactID.ToString();

                    tbxAwayStartDate.Text = period.AwayStartDate.ToString("yyyy-MM-dd");
                    tbxAwayEndDate.Text = period.AwayEndDate.ToString("yyyy-MM-dd");
                    if (period.ReasonID.HasValue)
                    {
                        var reasonItem = ddlReason.Items.FindByValue(period.ReasonID.Value.ToString());
                        if (reasonItem != null)
                            ddlReason.SelectedValue = period.ReasonID.Value.ToString();
                    }
                }
                else
                {
                    SetStatus("Could not load away period.", true);
                }
            }
            catch (Exception ex)
            {
                SetStatus("Error loading away period: " + ex.Message, true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Error loading away period {awayPeriodId}: {ex.Message}");
            }
        }

        private bool TrySave(out string message)
        {
            message = null;
            int awayPeriodId = GetAwayPeriodId();
            bool isEdit = awayPeriodId > 0;

            if (!int.TryParse(cboCustomer.SelectedValue, out int customerId) || customerId == 0)
            {
                message = "Please select a customer.";
                return false;
            }
            if (!DateTime.TryParse(tbxAwayStartDate.Text, out DateTime startDate))
            {
                message = "Please enter a valid start date.";
                return false;
            }
            if (!DateTime.TryParse(tbxAwayEndDate.Text, out DateTime endDate))
            {
                message = "Please enter a valid end date.";
                return false;
            }
            if (endDate < startDate)
            {
                message = "End date cannot be before start date.";
                return false;
            }
            if (!int.TryParse(ddlReason.SelectedValue, out int reasonId) || reasonId == 0)
            {
                message = "Please select a reason.";
                return false;
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
                    int newId = repo.Insert(awayPeriod);
                    if (newId <= 0)
                    {
                        message = "Insert failed — away period was not created.";
                        return false;
                    }

                    SetAwayPeriodId(newId);
                    btnDelete.Visible = true;
                }

                string logMsg = $"{(isEdit ? "Updated" : "Added")} away period for ContactID={customerId}: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}, ReasonID={reasonId}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Customers, logMsg);

                try
                {
                    new CustomerManager().SendAwayPeriodConfirmationEmail(customerId, startDate, endDate);
                }
                catch (Exception emailEx)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Email send failed (non-critical): {emailEx.Message}");
                }

                message = isEdit ? "Away period updated." : "Away period saved.";
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Exception saving away period for ContactID={customerId}: {ex.Message}");
                message = "Exception: " + ex.Message;
                return false;
            }
        }

        private void ReturnToList()
        {
            Response.Redirect(DefaultReturnUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (TrySave(out string message))
            {
                SetStatus(message ?? "Away period saved.", false);
                upnlContactsAwayDetail.Update();
            }
            else
            {
                SetStatus(message ?? "Save failed.", true);
                upnlContactsAwayDetail.Update();
            }
        }

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            if (!TrySave(out string message))
            {
                SetStatus(message ?? "Save failed.", true);
                upnlContactsAwayDetail.Update();
                return;
            }

            ReturnToList();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int awayPeriodId = GetAwayPeriodId();
            if (awayPeriodId > 0)
            {
                try
                {
                    var repo = new ContactsAwayPeriodRepository();
                    repo.Delete(awayPeriodId);
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, $"Deleted away period ID={awayPeriodId}");
                    ReturnToList();
                }
                catch (Exception ex)
                {
                    SetStatus("Error deleting: " + ex.Message, true);
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Error deleting away period {awayPeriodId}: {ex.Message}");
                    upnlContactsAwayDetail.Update();
                }
            }
        }

        protected void btnCancel_Click(object sender, ImageClickEventArgs e)
        {
            ReturnToList();
        }
    }
}
