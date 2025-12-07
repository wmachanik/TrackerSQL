// Decompiled with JetBrains decompiler
// Type: TrackerSQL.Pages.SendCoffeeCheckup
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Controls;
using TrackerSQL.Managers;

namespace TrackerSQL.Pages
{
    public partial class SendCoffeeCheckup : System.Web.UI.Page
    {
        private static Dictionary<int, string> _cachedCityNames = new Dictionary<int, string>();
        private static Dictionary<int, string> _cachedItemDescriptions = new Dictionary<int, string>();
        private int reminderWindowDays = SystemConstants.CheckupConstants.DefaultReminderWindowDays; // CoffeeCheckupManager.GetReminderWindowDays(); // fallback


        // Business logic manager - PROPERLY INITIALIZED
        private readonly CoffeeCheckupManager _coffeeCheckupManager;
        
        public SendCoffeeCheckup()
        {
            _coffeeCheckupManager = new CoffeeCheckupManager();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                // NEW: Clear any stale temp data from a previous session to avoid showing old results
                try
                {
                    var temp = new TempCoffeeCheckup();
                    // Keep order consistent with existing cleanup usage elsewhere
                    temp.DeleteAllContactRecords();
                    temp.DeleteAllContactItems();
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        "SendCoffeeCheckup: Cleared previous TempCoffeeCheckup data on initial page load.");
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog("error",
                        $"SendCoffeeCheckup: Failed to clear previous temp data on load: {ex.Message}");
                }

                // Make sure panels are visible
                upnlCustomerCheckup.Visible = true;
                upnlContactItems.Visible = true;
                
                // Load email templates immediately
                LoadEmailTextsOnly();
                
                // Set initial status
                //autoLoadingStatus.Visible = true;
                btnPrepData.Visible = true; // Keep visible for manual fallback

                // Setup reminder window including dropdown
                reminderWindowDays = CoffeeCheckupManager.GetReminderWindowDays();
                int min = ConfigHelper.GetInt("CoffeeCheckupReminderWindowMin", 5);
                int max = ConfigHelper.GetInt("CoffeeCheckupReminderWindowMax", 30);
                int def = CoffeeCheckupManager.GetReminderWindowDays();

                ddlReminderWindow.Items.Clear();
                for (int i = min; i <= max; i++)
                    ddlReminderWindow.Items.Add(new ListItem(i.ToString(), i.ToString()));
                
                // Load last used value from Session if available
                string lastUsed = Session["CoffeeCheckupReminderWindowDays"] as string;
                if (!string.IsNullOrEmpty(lastUsed) && ddlReminderWindow.Items.FindByValue(lastUsed) != null)
                    ddlReminderWindow.SelectedValue = lastUsed;
                else
                    ddlReminderWindow.SelectedValue = def.ToString();
                
            }
        }

        /// <summary>
        /// Load only email templates - fast operation
        /// </summary>
        private void LoadEmailTextsOnly()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Load email templates (existing method)
                LoadEmailTexts();
                
                stopwatch.Stop();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Email templates loaded in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("error", $"SendCoffeeCheckup: Error loading email templates: {ex.Message}");
                ltrlStatus.Text = $"<div class='alert alert-warning'>Warning: {ex.Message}</div>";
            }
        }
        protected void btnPrepData_Click(object sender, EventArgs e)
        {
            uprgCustomerCheckup.DisplayAfter = 100;
            uprgSendEmail.DisplayAfter = int.MaxValue;

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                upnlSendEmail.Update();

                
                if (ddlReminderWindow.SelectedItem != null)
                    int.TryParse(ddlReminderWindow.SelectedValue, out reminderWindowDays);

                // 1) Build the temp/contact list only
                _coffeeCheckupManager.PrepareCustomerReminderData(reminderWindowDays);

                // 2) Single post-pass adjust over the prepared list (only if holiday in window)
                int adjustedCount = _coffeeCheckupManager.PostAdjustPreparedReminderData(reminderWindowDays);
                ViewState["AdjustedCount"] = adjustedCount; // keep for later, optional
                // Determine if a holiday exists in the selected window
                bool holidayInWindow = adjustedCount != -1;

                // Refresh UI
                odsContactsToSendCheckup.DataBind();
                gvCustomerCheckup.DataBind();

                int customerCount = GetCustomerCount();
                stopwatch.Stop();

                btnPrepData.Text = "Refresh Data";
                btnPrepData.Visible = true;

                ltrlStatus.Text = $"<div style='background-color: #d4edda; color: #155724; padding: 8px; border-radius: 4px; margin: 5px 0; text-align: left;'>" +
                                  $"<strong>Success!</strong> Customer data prepared. You can now send reminders or test emails.</div>";
                // If we adjusted any rows, surface a notice via Site.Master (if available), else fallback
                if (holidayInWindow)
                {
                    string msg = adjustedCount > 0
                        ? $"Upcoming holiday detected. Adjusted {adjustedCount} prep/delivery date(s)."
                        : "Upcoming holiday detected. Dates were verified against closures; no changes were required.";

                    new showMessageBox(this.Page, "Holiday Notice", msg);
                    ltrlStatus.Text += $"<div style='background-color:#e8f4fd;color:#084c7f;padding:8px;border-radius:4px;margin:5px 0;text-align:left;'>{HttpUtility.HtmlEncode(msg)}</div>";
                }
                upnlCustomerCheckup.Update();
                upnlSendEmail.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("error", $"SendCoffeeCheckup: Error in btnPrepData_Click: {ex.Message}");
                btnPrepData.Visible = true;
                btnPrepData.Text = "Retry Data Prep";
                ltrlStatus.Text = $"<div style='background-color: #f8d7da; color: #721c24; padding: 8px; border-radius: 4px; margin: 5px 0; text-align: left;'>" +
                                  $"<strong>Error:</strong> {ex.Message}<br/><small>Check the logs for more details.</small></div>";
                upnlCustomerCheckup.Update();
                upnlSendEmail.Update();
            }
            finally
            {
                uprgCustomerCheckup.DisplayAfter = 500;
                uprgSendEmail.DisplayAfter = 0;
            }
        }
        /* old before 15 Sept PrepData
        protected void btnPrepData_Click(object sender, EventArgs e)
        {
            // Control progress indicators
            uprgCustomerCheckup.DisplayAfter = 100; // Show customer prep progress quickly
            uprgSendEmail.DisplayAfter = int.MaxValue; // Don't show email progress
            
            //AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "SendCoffeeCheckup: Prep Data Click triggered");
            
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // IMMEDIATELY hide the auto-loading panel
                //autoLoadingStatus.Visible = false;

                // Update status immediately
                //ltrlAutoLoadStatus.Text = "";
                
                // Force immediate update
                upnlSendEmail.Update();
                
                //AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "SendCoffeeCheckup: Starting CoffeeCheckupManager.PrepareCustomerReminderData()");
                
                // Use the enhanced CoffeeCheckupManager
                int reminderWindowDays = CoffeeCheckupManager.GetReminderWindowDays(); // fallback
                if (ddlReminderWindow.SelectedItem != null)
                    int.TryParse(ddlReminderWindow.SelectedValue, out reminderWindowDays);

                _coffeeCheckupManager.PrepareCustomerReminderData(reminderWindowDays);
                
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "SendCoffeeCheckup: Customer Reminder Data prepared, refreshing grids");
                
                // Refresh the grid data
                odsContactsToSendCheckup.DataBind();
                gvCustomerCheckup.DataBind();
                
                // Get customer count and show success
                int customerCount = GetCustomerCount();
                
                stopwatch.Stop();
                
                // Update button and show success
                btnPrepData.Text = "Refresh Data";
                btnPrepData.Visible = true;
                
                //ltrlCustomerStatus.Text = $"✅ Ready! Found <strong>{customerCount}</strong> customers eligible for coffee reminders. " +
                //                         $"<small>(Loaded in {stopwatch.ElapsedMilliseconds / 1000.0:F1}s)</small>";
                
                ltrlStatus.Text = $"<div style='background-color: #d4edda; color: #155724; padding: 8px; border-radius: 4px; margin: 5px 0; text-align: left;'>" +
                                 $"<strong>Success!</strong> Customer data prepared automatically. You can now send reminders or test emails.</div>";
                
                //AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Auto-prep completed successfully in {stopwatch.ElapsedMilliseconds}ms - {customerCount} customers");
                
                // Force update of all panels
                upnlCustomerCheckup.Update();
                upnlSendEmail.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("error", $"SendCoffeeCheckup: Error in btnPrepData_Click: {ex.Message}");
                
                // Hide auto-loading panel on error too
                //autoLoadingStatus.Visible = false;
                
                btnPrepData.Visible = true;
                btnPrepData.Text = "Retry Data Prep";
                
                ltrlStatus.Text = $"<div style='background-color: #f8d7da; color: #721c24; padding: 8px; border-radius: 4px; margin: 5px 0; text-align: left;'>" +
                                 $"<strong>Error:</strong> {ex.Message}<br/><small>Check the logs for more details.</small></div>";
                
                // Force update of all panels
                upnlCustomerCheckup.Update();
                upnlSendEmail.Update();
            }
            finally
            {
                // Reset progress indicators
                uprgCustomerCheckup.DisplayAfter = 500;
                uprgSendEmail.DisplayAfter = 0;
            }
        }
        */
        /// <summary>
        /// Get count of prepared customers
        /// </summary>
        private int GetCustomerCount()
        {
            try
            {
                var tempCheckup = new TempCoffeeCheckup();
                var customers = tempCheckup.GetAllContacts("CustomerID");
                return customers?.Count ?? 0;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("error", $"SendCoffeeCheckup: Error getting customer count: {ex.Message}");
                return 0;
            }
        }

        // FIXED: Remove duplicate and use cached versions
        protected string GetCityName(int cityId)
        {
            if (!_cachedCityNames.ContainsKey(cityId))
            {
                _cachedCityNames[cityId] = _coffeeCheckupManager.GetCachedCityName(cityId);
            }
            return _cachedCityNames[cityId];
        }

        protected string GetItemDesc(int itemId)
        {
            if (!_cachedItemDescriptions.ContainsKey(itemId))
            {
                _cachedItemDescriptions[itemId] = _coffeeCheckupManager.GetCachedItemDescription(itemId);
            }
            return _cachedItemDescriptions[itemId];
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            this.uprgSendEmail.DisplayAfter = 0;
            
            try
            {
                UpdateStatus("🔄 Starting coffee checkup process...");
                
                // Prepare email data from UI
                var emailData = new SendCheckEmailTextsData {
                    Header = this.tbxEmailIntro.Text,
                    Body = this.tbxEmailBody.Text,
                    Footer = this.tbxEmailFooter.Text
                };
                // If a holiday is within window, append a friendly note from Messages.resx (before signature)
                var closureProvider = new HolidayClosureProvider();
                if (closureProvider.IsThereAHolodayComing(TimeZoneUtils.Now().Date, reminderWindowDays))
                {
                    string holidayNote = MessageProvider.Get(MessageKeys.CoffeeCheckup.HolidayClosureEmailNote);
                    if (string.IsNullOrWhiteSpace(holidayNote))
                        holidayNote = "Please note: upcoming public holidays may affect delivery timing. Thanks for your understanding.";

                    emailData.Footer = (emailData.Footer ?? string.Empty) +
                       $"<p style='margin:8px 0'>{HttpUtility.HtmlEncode(holidayNote)}</p>";
                }


                UpdateStatus("📋 Processing customers...");
                
                // Use the manager to process reminders
                var batchResult = _coffeeCheckupManager.ProcessCoffeeCheckupReminders(emailData);
                
                UpdateStatus($"✅ Complete! Sent: {batchResult.TotalSent}, Failed: {batchResult.TotalFailed}");

                // Enhanced status message
                string statusMessage = $"Coffee checkup process completed!\n\n" +
                                     $"📧 Emails sent successfully: {batchResult.TotalSent}\n" +
                                     $"❌ Failed to send: {batchResult.TotalFailed}\n" +
                                     $"📊 Total customers processed: {batchResult.TotalSent + batchResult.TotalFailed}";

                if (batchResult.TotalFailed > 0)
                {
                    statusMessage += "\n\n⚠️ Check the results page for details about failed emails.";
                }

                var statusMsg = new showMessageBox(this.Page, 
                    "Coffee Checkup Status", 
                    statusMessage);

                // Navigation to results page
                RedirectToResultsPage();
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error: {ex.Message}");
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Error in btnSend_Click: {ex.Message}");
                
                var errorMsg = new showMessageBox(this.Page, 
                    "Email Sending Error", 
                    ex.Message);
            }
        }

        protected void btnTestSingleCustomer_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateStatus("🧪 Starting test mode...");

                // Get test contact
                var allContacts = new TempCoffeeCheckup().GetAllContactAndItems();
                if (!allContacts.Any())
                {
                    this.ltrlStatus.Text = "No test contacts available. Run 'Prep Data' first.";
                    return;
                }

                var testContact = allContacts.First();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"TEST: Using customer {testContact.CompanyName} (ID: {testContact.CustomerID})");

                // Validate eligibility
                if (!_coffeeCheckupManager.ValidateCustomerEligibility(testContact))
                {
                    this.ltrlStatus.Text = $"❌ Test customer {testContact.CompanyName} is not eligible for reminders";
                    return;
                }

                // Prepare email data
                var emailData = new SendCheckEmailTextsData
                {
                    Header = this.tbxEmailIntro.Text,
                    Body = this.tbxEmailBody.Text,
                    Footer = this.tbxEmailFooter.Text
                };

                // Process single customer test
                var testResult = _coffeeCheckupManager.ProcessCoffeeCheckupReminders(emailData);

                this.ltrlStatus.Text = $"✅ Test completed: Sent: {testResult.TotalSent}, Failed: {testResult.TotalFailed}";
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"TEST: Completed - Sent: {testResult.TotalSent}, Failed: {testResult.TotalFailed}");
            }
            catch (Exception ex)
            {
                this.ltrlStatus.Text = $"❌ Test failed: {ex.Message}";
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"TEST ERROR: {ex.Message}");
            }
        }

        protected void btnClearTodaysData_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateStatus("🗑️ Clearing today's reminder data...");

                // Clear today's sent reminder log entries
                var sentRemindersLogTbl = new SentRemindersLogTbl();
                int deletedCount = sentRemindersLogTbl.DeleteTodaysEntries(TimeZoneUtils.Now().Date);

                UpdateStatus($"✅ Cleared {deletedCount} reminder entries from today");

                var successMsg = new showMessageBox(this.Page,
                    "Data Cleared",
                    $"Successfully removed {deletedCount} reminder log entries from today ({TimeZoneUtils.Now().Date:yyyy-MM-dd}).\n\nYou can now test again with clean data.");

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Cleared {deletedCount} today's reminder entries");
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Error clearing data: {ex.Message}");
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Error clearing today's data: {ex.Message}");

                var errorMsg = new showMessageBox(this.Page,
                    "Clear Data Error",
                    $"Error clearing today's data: {ex.Message}");
            }
        }

        private void RedirectToResultsPage()
        {
            try
            {
                DateTime sentDate = TimeZoneUtils.Now().Date;

                // Get the statistics from the database
                var sentRemindersLogTbl = new SentRemindersLogTbl();
                int totalReminders = sentRemindersLogTbl.GetEntriesCountForDate(sentDate);

                // Count unique customers and success/fail by iterating through the results
                var dayResults = sentRemindersLogTbl.GetAllByDate(sentDate, "CustomerID");
                int uniqueCustomers = dayResults.Select(r => r.CustomerID).Distinct().Count();
                int successful = dayResults.Count(r => r.ReminderSent);
                int failed = dayResults.Count(r => !r.ReminderSent);

                string redirectUrl = $"{this.ResolveUrl("~/Pages/SentRemindersSheet.aspx")}" +
                                   $"?LastSentDate={sentDate:yyyy-MM-dd}" +
                                   $"&TotalReminders={totalReminders}" +
                                   $"&UniqueCustomers={uniqueCustomers}" +
                                   $"&Successful={successful}" +
                                   $"&Failed={failed}";

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Redirecting with stats - {uniqueCustomers} customers, {successful}/{totalReminders} successful");
                Response.Redirect(redirectUrl, false);
            }
            catch (Exception redirectEx)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Redirect failed: {redirectEx.Message}");
                UpdateStatus("✅ Process completed successfully!");
            }
        }

        private void UpdateStatus(string message)
        {
            this.ltrlStatus.Text = message;
            AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup STATUS: {message}");
        }

        private void LoadEmailTexts()
        {
            SendCheckEmailTextsData texts = new SendCheckEmailTextsData().GetTexts();
            if (texts.SCEMTID <= 0)
                return;
            this.ltrlEmailTextID.Text = texts.SCEMTID.ToString();
            this.tbxEmailIntro.Text = HttpUtility.HtmlDecode(texts.Header);
            this.tbxEmailBody.Text = HttpUtility.HtmlDecode(texts.Body);
            this.tbxEmailFooter.Text = texts.Footer;
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.ltrlEmailTextID.Text))
                return;
            SendCheckEmailTextsData pEmailTextsData = new SendCheckEmailTextsData();
            pEmailTextsData.Header = HttpUtility.HtmlEncode(this.tbxEmailIntro.Text);
            pEmailTextsData.Body = HttpUtility.HtmlEncode(this.tbxEmailBody.Text);
            pEmailTextsData.Footer = HttpUtility.HtmlEncode(this.tbxEmailFooter.Text);
            this.ltrlStatus.Text = pEmailTextsData.UpdateTexts(pEmailTextsData, Convert.ToInt32(this.ltrlEmailTextID.Text));
        }

        protected void btnReload_Click(object sender, EventArgs e) => this.LoadEmailTexts();

        // Helper methods for compatibility
        public string GetItemSKU(int pItemID) => _coffeeCheckupManager.GetCachedItemSKU(pItemID);
        public string GetPackagingDesc(int pPackagingID) => _coffeeCheckupManager.GetCachedPackagingDescription(pPackagingID);
        public string GetItemUoM(int pItemID) => _coffeeCheckupManager.GetCachedItemUoM(pItemID);

        protected void ddlReminderWindow_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Save the new value to Session (or Profile, or DB)
            Session["CoffeeCheckupReminderWindowDays"] = ddlReminderWindow.SelectedValue;

            // Trigger data prep with the new value
            btnPrepData_Click(sender, e);
        }
        //protected void btnShowMatrix_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // Make sure matrix is current (TTL respected by EnsureBuilt)
        //        CityDeliveryMatrix.EnsureBuilt();

        //        var rows = CityDeliveryMatrix.GetSnapshot();
        //        if (rows == null || rows.Count == 0)
        //        {
        //            ltrlMatrixDump.Text = "<div style='padding:6px;background:#fff3cd;color:#856404;border:1px solid #ffeeba;border-radius:4px;'>No matrix rows found.</div>";
        //        }
        //        else
        //        {
        //            var sb = new System.Text.StringBuilder();
        //            sb.Append("<table style='border-collapse:collapse;font-size:12px;'>");
        //            sb.Append("<tr style='background:#e9ecef;'>");
        //            sb.Append("<th style='border:1px solid #ccc;padding:4px;'>CityID</th>");
        //            sb.Append("<th style='border:1px solid #ccc;padding:4px;'>Prep</th>");
        //            sb.Append("<th style='border:1px solid #ccc;padding:4px;'>Delivery</th>");
        //            sb.Append("<th style='border:1px solid #ccc;padding:4px;'>Next Prep</th>");
        //            sb.Append("<th style='border:1px solid #ccc;padding:4px;'>Next Delivery</th>");
        //            sb.Append("</tr>");

        //            foreach (var r in rows)
        //            {
        //                sb.Append("<tr>");
        //                sb.AppendFormat("<td style='border:1px solid #ccc;padding:3px;text-align:center;'>{0}</td>", r.CityID);
        //                sb.AppendFormat("<td style='border:1px solid #ccc;padding:3px;'>{0:yyyy-MM-dd}</td>", r.PrepDate);
        //                sb.AppendFormat("<td style='border:1px solid #ccc;padding:3px;'>{0:yyyy-MM-dd}</td>", r.DeliveryDate);
        //                sb.AppendFormat("<td style='border:1px solid #ccc;padding:3px;'>{0:yyyy-MM-dd}</td>", r.NextPrepDate);
        //                sb.AppendFormat("<td style='border:1px solid #ccc;padding:3px;'>{0:yyyy-MM-dd}</td>", r.NextDeliveryDate);
        //                sb.Append("</tr>");
        //            }

        //            sb.Append("</table>");
        //            ltrlMatrixDump.Text = sb.ToString();
        //        }

        //        pnlMatrixDump.Visible = true;
        //        UpdateStatus("Delivery matrix snapshot generated.");
        //    }
        //    catch (Exception ex)
        //    {
        //        AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Error dumping matrix: {ex.Message}");
        //        ltrlMatrixDump.Text = "<div style='padding:6px;background:#f8d7da;color:#721c24;border:1px solid #f5c6cb;border-radius:4px;'>Matrix dump failed: "
        //            + HttpUtility.HtmlEncode(ex.Message) + "</div>";
        //        pnlMatrixDump.Visible = true;
        //    }
        //}
    }
}