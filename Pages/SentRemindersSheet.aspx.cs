// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.SentRemindersSheet
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using AjaxControlToolkit;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Controls;
using System.Collections.Generic;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class SentRemindersSheet : Page
    {
        private const string CONST_URL_REQUEST_LASTSENTDATE = "LastSentDate";
        protected ScriptManager smSentRemindersSummary;
        protected UpdateProgress uprgSentRemindersSummary;
        protected UpdatePanel upnlSelection;
        protected DropDownList ddlFilterByDate;
        protected UpdatePanel upnlSentRemindersList;
        protected GridView gvSentReminders;
        protected ObjectDataSource odsSentRemindersSummarys;
        protected ObjectDataSource odsDatesSentReminder;
        protected UpdatePanel UpdatePanel1;
        protected Label lblFilter;
        protected GridView gvFailedEmails;
        protected System.Web.UI.HtmlControls.HtmlGenericControl divFailedEmails;
        
        // Add these control declarations
        //protected Literal ltrlReminderSummary;
        //protected Literal ltrlReminderFooter;
        //protected Button btnClearFailures;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadReminderData();
                LoadFailedEmails();
                UpdateReminderSummaryFromQueryString();
            }
        }

        /// <summary>
        /// MISSING METHOD: Load reminder data and bind to controls
        /// </summary>
        private void LoadReminderData()
        {
            try
            {
                // Force rebind of the date dropdown
                odsDatesSentReminder.DataBind();
                ddlFilterByDate.DataBind();
                
                // If there's a LastSentDate in query string, select it
                if (Request.QueryString[CONST_URL_REQUEST_LASTSENTDATE] != null)
                {
                    string queryDate = Request.QueryString[CONST_URL_REQUEST_LASTSENTDATE];
                    var listItem = ddlFilterByDate.Items.FindByValue(queryDate);
                    if (listItem != null)
                    {
                        ddlFilterByDate.SelectedValue = queryDate;
                    }
                }
                
                // Force rebind of the main grid
                gvSentReminders.DataBind();
                
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, "SentRemindersSheet: Reminder data loaded successfully");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Error loading reminder data: {ex.Message}");
            }
        }

        protected void ddlFilterByDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear query string data and recalculate from database
            LoadReminderData();
            UpdateReminderSummaryFromDatabase();
            // FORCE the UpdatePanel to refresh
            upnlSentRemindersList.Update();
        }

        private void UpdateReminderSummaryFromQueryString()
        {
            try
            {
                DateTime selectedDate = GetSelectedDate();
                
                // Try to get stats from query string first (from redirect)
                // But only if the selected date matches today (when redirect happened)
                DateTime today = TimeZoneUtils.Now().Date;
                bool useQueryString = selectedDate.Date == today && Request.QueryString["TotalReminders"] != null;
                
                int totalReminders = 0;
                int uniqueCustomers = 0;
                int successful = 0;
                int failed = 0;
                
                if (useQueryString)
                {
                    int.TryParse(Request.QueryString["TotalReminders"], out totalReminders);
                    int.TryParse(Request.QueryString["UniqueCustomers"], out uniqueCustomers);
                    int.TryParse(Request.QueryString["Successful"], out successful);
                    int.TryParse(Request.QueryString["Failed"], out failed);
                    
                    AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Using query string stats for {selectedDate:yyyy-MM-dd} - {uniqueCustomers} customers, {successful}/{totalReminders} successful");
                }
                else
                {
                    // Either no query string data or different date selected - query database
                    UpdateReminderSummaryFromDatabase();
                    return;
                }
                
                // Display the summary
                DisplayReminderSummary(selectedDate, totalReminders, uniqueCustomers, successful, failed);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Error updating summary from query string: {ex.Message}");
                UpdateReminderSummaryFromDatabase(); // Fallback
            }
        }

        private void UpdateReminderSummaryFromDatabase()
        {
            try
            {
                DateTime selectedDate = GetSelectedDate();
                
                var sentRemindersLogTbl = new SentRemindersLogTbl();
                int totalReminders = sentRemindersLogTbl.GetEntriesCountForDate(selectedDate);
                
                int uniqueCustomers = 0;
                int successful = 0;
                int failed = 0;
                
                if (totalReminders > 0)
                {
                    var dayResults = sentRemindersLogTbl.GetAllByDate(selectedDate, "CustomerID");
                    uniqueCustomers = dayResults.Select(r => r.CustomerID).Distinct().Count();
                    successful = dayResults.Count(r => r.ReminderSent);
                    failed = dayResults.Count(r => !r.ReminderSent);
                }
                
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"SentRemindersSheet: Using database stats for {selectedDate:yyyy-MM-dd} - {uniqueCustomers} customers, {successful}/{totalReminders} successful");
                
                // Display the summary
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
                // Update header summary
                if (ltrlReminderSummary != null)
                {
                    if (totalReminders == 0)
                    {
                        ltrlReminderSummary.Text = $"<div class='reminder-summary' style='padding: 10px; background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 5px; margin-bottom: 15px;'>" +
                                                 $"<h4>📊 Reminder Summary for {selectedDate:dddd, MMMM dd, yyyy}</h4>" +
                                                 $"<p><em>No reminders were sent on this date.</em></p>" +
                                                 "</div>";
                    }
                    else
                    {
                        string summaryText = $"<div class='reminder-summary' style='padding: 10px; background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 5px; margin-bottom: 15px;'>" +
                                           $"<h4>📊 Reminder Summary for {selectedDate:dddd, MMMM dd, yyyy}</h4>" +
                                           $"<div style='display: flex; gap: 20px; flex-wrap: wrap;'>" +
                                           $"<div><strong>👥 Customers:</strong> {uniqueCustomers}</div>" +
                                           $"<div><strong>📧 Total Reminders:</strong> {totalReminders}</div>" +
                                           $"<div><strong>✅ Successful:</strong> {successful}</div>" +
                                           $"<div><strong>❌ Failed:</strong> {failed}</div>";
                        
                        if (totalReminders > 0)
                        {
                            double successRate = (double)successful / totalReminders * 100;
                            summaryText += $"<div><strong>📈 Success Rate:</strong> {successRate:F1}%</div>";
                        }
                        
                        summaryText += "</div></div>";
                        ltrlReminderSummary.Text = summaryText;
                    }
                }
                
                // Update footer summary
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
                        string footerText = $"<div class='reminder-footer' style='padding: 10px; background-color: #f8f9fa; border-top: 1px solid #dee2e6; margin-top: 15px; text-align: center;'>" +
                                          $"<strong>Summary:</strong> {successful} of {totalReminders} reminders sent successfully to {uniqueCustomers} customers on {selectedDate:yyyy-MM-dd}" +
                                          "</div>";
                        
                        ltrlReminderFooter.Text = footerText;
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
            // Your existing method to get the selected date from dropdown
            // This should return the date from ddlFilterByDate.SelectedValue
            if (ddlFilterByDate.SelectedValue != null && DateTime.TryParse(ddlFilterByDate.SelectedValue, out DateTime selectedDate))
            {
                return selectedDate.Date;
            }
            
            // Default to today if nothing selected
            return TimeZoneUtils.Now().Date;
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (this.IsPostBack || this.Request.QueryString.Count <= 0 || this.Request.QueryString["LastSentDate"] == null)
                return;
            string str = $"{Convert.ToDateTime(this.Request.QueryString["LastSentDate"]):d}";
            if (this.ddlFilterByDate.Items.FindByValue(str) == null)
                return;
            this.ddlFilterByDate.SelectedValue = str;
            this.gvSentReminders.DataBind();
            this.upnlSentRemindersList.Update();
        }

        public string GetCompanyName(long pCompanyID)
        {
            return pCompanyID > 0L ? new CompanyNames().GetCompanyNameByCompanyID(pCompanyID) : string.Empty;
        }

        // Your existing LoadFailedEmails and related methods stay the same...
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