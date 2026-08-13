//------------------------------------------------------------------------------
// TrackerSQL v3.x — SentRemindersSheet
// WebForms page code-behind for SentRemindersSheet.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class SentRemindersSheet : Page
    {
        private const string ConstUrlRequestLastSentDate = "LastSentDate";
        private const string DefaultReturnUrl = "~/Default.aspx";

        private readonly SentRemindersLogRepository _sentRemindersLogRepository = new SentRemindersLogRepository();
        private readonly ContactsRepository _contactsRepository = new ContactsRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPostBackControls();

            if (!IsPostBack)
            {
                BindDateDropdown();
                ApplyQueryStringDateSelection();
                BindRemindersGrid();
                LoadFailedEmails();
                UpdateReminderSummaryFromQueryString();
                SetStatus("Select a date to review reminders sent that day.", isError: null);
            }
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;

            scriptManager.RegisterAsyncPostBackControl(ddlFilterByDate);
            scriptManager.RegisterAsyncPostBackControl(btnRefresh);
            scriptManager.RegisterAsyncPostBackControl(btnClearFailures);
            scriptManager.RegisterPostBackControl(btnBack);
        }

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);

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

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect(DefaultReturnUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            ReloadForSelectedDate("Reminder list refreshed.");
        }

        private void BindDateDropdown()
        {
            var dates = _sentRemindersLogRepository.GetLast20DatesReminderSent();
            ddlFilterByDate.DataSource = dates.ConvertAll(d => new { Date = d });
            ddlFilterByDate.DataTextField = "Date";
            ddlFilterByDate.DataValueField = "Date";
            ddlFilterByDate.DataBind();

            if (ddlFilterByDate.Items.Count == 0)
            {
                DateTime today = TimeZoneUtils.Now().Date;
                ddlFilterByDate.Items.Insert(0, new ListItem(today.ToString("d"), today.ToString("o")));
            }
        }

        private void ApplyQueryStringDateSelection()
        {
            string queryDate = Request.QueryString[ConstUrlRequestLastSentDate];
            if (string.IsNullOrWhiteSpace(queryDate))
                return;

            if (!DateTime.TryParse(queryDate, out DateTime parsed))
                return;

            string shortDate = parsed.Date.ToString("d");
            ListItem match = ddlFilterByDate.Items.FindByValue(queryDate)
                ?? ddlFilterByDate.Items.FindByText(shortDate);

            if (match == null)
            {
                foreach (ListItem item in ddlFilterByDate.Items)
                {
                    if (DateTime.TryParse(item.Value, out DateTime itemDate) && itemDate.Date == parsed.Date)
                    {
                        match = item;
                        break;
                    }
                }
            }

            if (match != null)
                ddlFilterByDate.SelectedValue = match.Value;
        }

        private void BindRemindersGrid()
        {
            DateTime selectedDate = GetSelectedDate();
            var rows = _sentRemindersLogRepository.GetAllByDate(selectedDate, "ContactID");
            gvSentReminders.DataSource = rows.ConvertAll(r => new
            {
                r.ReminderID,
                CustomerID = (long)r.ContactID,
                r.DateSentReminder,
                r.NextPreparationDate,
                r.ReminderSent,
                r.HadAutoFulfilItem,
                r.HadRecurringItems
            });
            gvSentReminders.DataBind();
        }

        private void ReloadForSelectedDate(string statusMessage)
        {
            try
            {
                BindRemindersGrid();
                UpdateReminderSummaryFromDatabase();
                LoadFailedEmails();
                SetStatus(statusMessage, isError: false);
                upnlSentRemindersList.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"SentRemindersSheet: Error reloading: {ex.Message}");
                SetStatus("Unable to load reminders: " + ex.Message, isError: true);
                upnlSentRemindersList.Update();
            }
        }

        protected void ddlFilterByDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ReloadForSelectedDate("Showing reminders for " + GetSelectedDate().ToString("yyyy-MM-dd") + ".");
        }

        protected void gvSentReminders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSentReminders.PageIndex = e.NewPageIndex;
            BindRemindersGrid();
            upnlSentRemindersList.Update();
        }

        private void UpdateReminderSummaryFromQueryString()
        {
            try
            {
                DateTime selectedDate = GetSelectedDate();
                DateTime today = TimeZoneUtils.Now().Date;
                bool useQueryString = selectedDate.Date == today && Request.QueryString["TotalReminders"] != null;

                if (!useQueryString)
                {
                    UpdateReminderSummaryFromDatabase();
                    return;
                }

                int.TryParse(Request.QueryString["TotalReminders"], out int totalReminders);
                int.TryParse(Request.QueryString["UniqueCustomers"], out int uniqueCustomers);
                int.TryParse(Request.QueryString["Successful"], out int successful);
                int.TryParse(Request.QueryString["Failed"], out int failed);

                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"SentRemindersSheet: Using query string stats for {selectedDate:yyyy-MM-dd} - {uniqueCustomers} customers, {successful}/{totalReminders} successful");
                DisplayReminderSummary(selectedDate, totalReminders, uniqueCustomers, successful, failed);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"SentRemindersSheet: Error updating summary from query string: {ex.Message}");
                UpdateReminderSummaryFromDatabase();
            }
        }

        private void UpdateReminderSummaryFromDatabase()
        {
            try
            {
                DateTime selectedDate = GetSelectedDate();
                int totalReminders = _sentRemindersLogRepository.GetEntriesCountForDate(selectedDate);

                int uniqueCustomers = 0;
                int successful = 0;
                int failed = 0;

                if (totalReminders > 0)
                {
                    var dayResults = _sentRemindersLogRepository.GetAllByDate(selectedDate, "ContactID");
                    uniqueCustomers = dayResults.Select(r => r.ContactID).Distinct().Count();
                    successful = dayResults.Count(r => r.ReminderSent == true);
                    failed = dayResults.Count(r => r.ReminderSent != true);
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"SentRemindersSheet: Using database stats for {selectedDate:yyyy-MM-dd} - {uniqueCustomers} customers, {successful}/{totalReminders} successful");
                DisplayReminderSummary(selectedDate, totalReminders, uniqueCustomers, successful, failed);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"SentRemindersSheet: Error updating summary from database: {ex.Message}");
                ltrlReminderSummary.Text = "<div class='status-message status-warn'>Unable to load reminder statistics.</div>";
                ltrlReminderFooter.Text = string.Empty;
                SetStatus("Unable to load reminder statistics.", isError: true);
            }
        }

        private void DisplayReminderSummary(DateTime selectedDate, int totalReminders, int uniqueCustomers, int successful, int failed)
        {
            try
            {
                string dateLabel = HttpUtility.HtmlEncode(selectedDate.ToString("dddd, MMMM dd, yyyy"));

                if (totalReminders == 0)
                {
                    ltrlReminderSummary.Text =
                        "<div class='status-message status-info' style='margin: 12px 0;'>" +
                        "<strong>Reminder Summary for " + dateLabel + "</strong><br />" +
                        "<em>No reminders were sent on this date.</em></div>";
                    ltrlReminderFooter.Text =
                        "<div class='status-message' style='margin-top: 12px; text-align: center;'>" +
                        "<em>No reminder data available for " + HttpUtility.HtmlEncode(selectedDate.ToString("yyyy-MM-dd")) + "</em></div>";
                    return;
                }

                double successRate = (double)successful / totalReminders * 100;
                ltrlReminderSummary.Text =
                    "<div class='status-message status-info' style='margin: 12px 0;'>" +
                    "<strong>Reminder Summary for " + dateLabel + "</strong>" +
                    "<div class='reminder-summary-stats'>" +
                    "<div><strong>Customers:</strong> " + uniqueCustomers + "</div>" +
                    "<div><strong>Total Reminders:</strong> " + totalReminders + "</div>" +
                    "<div><strong>Successful:</strong> " + successful + "</div>" +
                    "<div><strong>Failed:</strong> " + failed + "</div>" +
                    "<div><strong>Success Rate:</strong> " + successRate.ToString("F1") + "%</div>" +
                    "</div></div>";

                ltrlReminderFooter.Text =
                    "<div class='status-message' style='margin-top: 12px; text-align: center;'>" +
                    "<strong>Summary:</strong> " + successful + " of " + totalReminders +
                    " reminders sent successfully to " + uniqueCustomers + " customers on " +
                    HttpUtility.HtmlEncode(selectedDate.ToString("yyyy-MM-dd")) + "</div>";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                    $"SentRemindersSheet: Error displaying summary: {ex.Message}");
            }
        }

        private DateTime GetSelectedDate()
        {
            if (ddlFilterByDate.SelectedValue != null
                && DateTime.TryParse(ddlFilterByDate.SelectedValue, out DateTime selectedDate))
            {
                return selectedDate.Date;
            }

            return TimeZoneUtils.Now().Date;
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (IsPostBack || Request.QueryString.Count <= 0 || Request.QueryString[ConstUrlRequestLastSentDate] == null)
                return;

            ApplyQueryStringDateSelection();
            BindRemindersGrid();
            UpdateReminderSummaryFromQueryString();
            upnlSentRemindersList.Update();
        }

        public string GetCompanyName(long contactId)
        {
            return contactId > 0 ? _contactsRepository.GetContactNameById((int)contactId) : string.Empty;
        }

        private void LoadFailedEmails()
        {
            try
            {
                var failedContacts = Session["CoffeeCheckupFailures"] as List<string>;
                var failureDate = Session["CoffeeCheckupFailureDate"] as DateTime?;

                if (failedContacts != null && failedContacts.Any() && failureDate.HasValue
                    && failureDate.Value.Date == TimeZoneUtils.Now().Date)
                {
                    var failureData = failedContacts.Select(failure => new
                    {
                        CustomerName = ExtractCustomerName(failure),
                        FailureReason = ExtractFailureReason(failure)
                    }).ToList();

                    if (failureData.Any())
                    {
                        gvFailedEmails.DataSource = failureData;
                        gvFailedEmails.DataBind();
                        pnlFailedEmails.Visible = true;
                        AppLogger.WriteLog(SystemConstants.LogTypes.Email,
                            $"Displaying {failureData.Count} failed email attempts on SentRemindersSheet");
                        return;
                    }
                }

                pnlFailedEmails.Visible = false;
                if (failureDate.HasValue && failureDate.Value.Date != TimeZoneUtils.Now().Date)
                    ClearFailureSession();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Error loading failed emails: {ex.Message}");
                pnlFailedEmails.Visible = false;
            }
        }

        private static string ExtractCustomerName(string failureString)
        {
            try
            {
                int dashIndex = failureString.IndexOf(" - ", StringComparison.Ordinal);
                if (dashIndex > 0)
                    return failureString.Substring(0, dashIndex).Trim();
                return failureString;
            }
            catch
            {
                return failureString ?? "Unknown Customer";
            }
        }

        private static string ExtractFailureReason(string failureString)
        {
            try
            {
                int dashIndex = failureString.IndexOf(" - ", StringComparison.Ordinal);
                if (dashIndex > 0 && dashIndex + 3 < failureString.Length)
                    return failureString.Substring(dashIndex + 3).Trim();
                return "Unknown error";
            }
            catch
            {
                return "Unknown error";
            }
        }

        protected void btnClearFailures_Click(object sender, EventArgs e)
        {
            ClearFailureSession();
            pnlFailedEmails.Visible = false;
            SetStatus("Failed email list cleared.", isError: false);
            upnlSentRemindersList.Update();
            AppLogger.WriteLog(SystemConstants.LogTypes.Email, "Failed email list cleared by user");
        }

        private void ClearFailureSession()
        {
            Session.Remove("CoffeeCheckupFailures");
            Session.Remove("CoffeeCheckupFailureDate");
        }
    }
}
