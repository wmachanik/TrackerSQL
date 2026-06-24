using AjaxControlToolkit;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class SentRemindersSheet : Page
    {
        private const string CONST_URL_REQUEST_LASTSENTDATE = "LastSentDate";

        private readonly SentRemindersLogRepository _sentRemindersLogRepository = new SentRemindersLogRepository();
        private readonly ContactsRepository _contactsRepository = new ContactsRepository();

        protected ScriptManager smSentRemindersSummary;
        protected UpdateProgress uprgSentRemindersSummary;
        protected UpdatePanel upnlSelection;
        protected DropDownList ddlFilterByDate;
        protected UpdatePanel upnlSentRemindersList;
        protected GridView gvSentReminders;
        protected UpdatePanel UpdatePanel1;
        protected Label lblFilter;
        protected GridView gvFailedEmails;
        protected System.Web.UI.HtmlControls.HtmlGenericControl divFailedEmails;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDateDropdown();
                BindRemindersGrid();
                LoadFailedEmails();
                UpdateReminderSummaryFromQueryString();
            }
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
                ddlFilterByDate.Items.Insert(0, new ListItem(TimeZoneUtils.Now().Date.ToString("d"), TimeZoneUtils.Now().Date.ToString("d")));
            }
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
                r.NextPreperationDate,
                r.ReminderSent,
                r.HadAutoFulfilItem,
                HadReoccurItems = r.HadRecurrItems
            });
            gvSentReminders.DataBind();
        }

        private void LoadReminderData()
        {
            try
            {
                BindDateDropdown();

                if (Request.QueryString[CONST_URL_REQUEST_LASTSENTDATE] != null)
                {
                    string queryDate = Request.QueryString[CONST_URL_REQUEST_LASTSENTDATE];
                    var listItem = ddlFilterByDate.Items.FindByValue(queryDate);
                    if (listItem != null)
                    {
                        ddlFilterByDate.SelectedValue = queryDate;
                    }
                }

                BindRemindersGrid();
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, "SentRemindersSheet: Reminder data loaded successfully");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Error loading reminder data: {ex.Message}");
            }
        }

        protected void ddlFilterByDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReminderData();
            UpdateReminderSummaryFromDatabase();
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

                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Using query string stats for {selectedDate:yyyy-MM-dd} - {uniqueCustomers} customers, {successful}/{totalReminders} successful");
                DisplayReminderSummary(selectedDate, totalReminders, uniqueCustomers, successful, failed);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Error updating summary from query string: {ex.Message}");
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

                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Using database stats for {selectedDate:yyyy-MM-dd} - {uniqueCustomers} customers, {successful}/{totalReminders} successful");
                DisplayReminderSummary(selectedDate, totalReminders, uniqueCustomers, successful, failed);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Error updating summary from database: {ex.Message}");

                if (ltrlReminderSummary != null)
                {
                    ltrlReminderSummary.Text = "<div class='alert alert-warning'>Unable to load reminder statistics.</div>";
                }
                if (ltrlReminderFooter != null)
                {
                    ltrlReminderFooter.Text = "";
                }
            }
        }

        private void DisplayReminderSummary(DateTime selectedDate, int totalReminders, int uniqueCustomers, int successful, int failed)
        {
            try
            {
                if (ltrlReminderSummary != null)
                {
                    if (totalReminders == 0)
                    {
                        ltrlReminderSummary.Text = $"<div class='reminder-summary' style='padding: 10px; background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 5px; margin-bottom: 15px;'>" +
                                                 $"<h4>Reminder Summary for {selectedDate:dddd, MMMM dd, yyyy}</h4>" +
                                                 $"<p><em>No reminders were sent on this date.</em></p>" +
                                                 "</div>";
                    }
                    else
                    {
                        string summaryText = $"<div class='reminder-summary' style='padding: 10px; background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 5px; margin-bottom: 15px;'>" +
                                           $"<h4>Reminder Summary for {selectedDate:dddd, MMMM dd, yyyy}</h4>" +
                                           $"<div style='display: flex; gap: 20px; flex-wrap: wrap;'>" +
                                           $"<div><strong>Customers:</strong> {uniqueCustomers}</div>" +
                                           $"<div><strong>Total Reminders:</strong> {totalReminders}</div>" +
                                           $"<div><strong>Successful:</strong> {successful}</div>" +
                                           $"<div><strong>Failed:</strong> {failed}</div>";

                        if (totalReminders > 0)
                        {
                            double successRate = (double)successful / totalReminders * 100;
                            summaryText += $"<div><strong>Success Rate:</strong> {successRate:F1}%</div>";
                        }

                        summaryText += "</div></div>";
                        ltrlReminderSummary.Text = summaryText;
                    }
                }

                if (ltrlReminderFooter != null)
                {
                    if (totalReminders == 0)
                    {
                        ltrlReminderFooter.Text = $"<div class='reminder-footer' style='padding: 10px; background-color: #f8f9fa; border-top: 1px solid #dee2e6; margin-top: 15px; text-align: center;'>" +
                                                $"<em>No reminder data available for {selectedDate:yyyy-MM-dd}</em>" +
                                                "</div>";
                    }
                    else
                    {
                        ltrlReminderFooter.Text = $"<div class='reminder-footer' style='padding: 10px; background-color: #f8f9fa; border-top: 1px solid #dee2e6; margin-top: 15px; text-align: center;'>" +
                                          $"<strong>Summary:</strong> {successful} of {totalReminders} reminders sent successfully to {uniqueCustomers} customers on {selectedDate:yyyy-MM-dd}" +
                                          "</div>";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Error displaying summary: {ex.Message}");
            }
        }

        private DateTime GetSelectedDate()
        {
            if (ddlFilterByDate.SelectedValue != null && DateTime.TryParse(ddlFilterByDate.SelectedValue, out DateTime selectedDate))
            {
                return selectedDate.Date;
            }

            return TimeZoneUtils.Now().Date;
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (IsPostBack || Request.QueryString.Count <= 0 || Request.QueryString["LastSentDate"] == null)
                return;

            string str = $"{Convert.ToDateTime(Request.QueryString["LastSentDate"]):d}";
            if (ddlFilterByDate.Items.FindByValue(str) == null)
                return;

            ddlFilterByDate.SelectedValue = str;
            BindRemindersGrid();
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

                if (failedContacts != null && failedContacts.Any() && failureDate.HasValue)
                {
                    if (failureDate.Value.Date == TimeZoneUtils.Now().Date)
                    {
                        var failureData = failedContacts.Select(failure => new
                        {
                            CustomerName = ExtractCustomerName(failure),
                            FailureReason = ExtractFailureReason(failure)
                        }).ToList();

                        if (failureData.Any() && gvFailedEmails != null && divFailedEmails != null)
                        {
                            gvFailedEmails.DataSource = failureData;
                            gvFailedEmails.DataBind();
                            divFailedEmails.Visible = true;
                            AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Displaying {failureData.Count} failed email attempts on SentRemindersSheet");
                        }
                    }
                    else
                    {
                        ClearFailureSession();
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"Error loading failed emails: {ex.Message}");
                if (divFailedEmails != null)
                {
                    divFailedEmails.Visible = false;
                }
            }
        }

        private string ExtractCustomerName(string failureString)
        {
            try
            {
                int dashIndex = failureString.IndexOf(" - ");
                if (dashIndex > 0)
                {
                    return failureString.Substring(0, dashIndex).Trim();
                }
                return failureString;
            }
            catch
            {
                return failureString ?? "Unknown Customer";
            }
        }

        private string ExtractFailureReason(string failureString)
        {
            try
            {
                int dashIndex = failureString.IndexOf(" - ");
                if (dashIndex > 0 && dashIndex + 3 < failureString.Length)
                {
                    return failureString.Substring(dashIndex + 3).Trim();
                }
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
            if (divFailedEmails != null)
            {
                divFailedEmails.Visible = false;
            }
            AppLogger.WriteLog(SystemConstants.LogTypes.Email, "Failed email list cleared by user");
        }

        private void ClearFailureSession()
        {
            Session.Remove("CoffeeCheckupFailures");
            Session.Remove("CoffeeCheckupFailureDate");
        }
    }
}
