//------------------------------------------------------------------------------
// TrackerSQL v3.x — SendCoffeeCheckup
// WebForms page code-behind for SendCoffeeCheckup.
//------------------------------------------------------------------------------

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
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Pages
{
    public partial class SendCoffeeCheckup : System.Web.UI.Page
    {
        private const string DefaultReturnUrl = "~/Default.aspx";

        private static Dictionary<int, string> _cachedAreaNames = new Dictionary<int, string>();
        private static Dictionary<int, string> _cachedItemDescriptions = new Dictionary<int, string>();
        private int reminderWindowDays = SystemConstants.CheckupConstants.DefaultReminderWindowDays;

        private readonly CoffeeCheckupManager _coffeeCheckupManager;

        public SendCoffeeCheckup()
        {
            _coffeeCheckupManager = new CoffeeCheckupManager();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPostBackControls();
            ApplyEmailTestModeIndicator();

            if (!IsPostBack)
            {
                try
                {
                    _coffeeCheckupManager.ClearTempCheckupData();
                    AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                        "SendCoffeeCheckup: Cleared previous TempCoffeeCheckup data on initial page load.");
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog("error",
                        $"SendCoffeeCheckup: Failed to clear previous temp data on load: {ex.Message}");
                }

                upnlCustomerCheckup.Visible = true;

                LoadEmailTextsOnly();
                btnPrepData.Visible = true;

                reminderWindowDays = CoffeeCheckupManager.GetReminderWindowDays();
                int min = ConfigHelper.GetInt("CoffeeCheckupReminderWindowMin", 5);
                int max = ConfigHelper.GetInt("CoffeeCheckupReminderWindowMax", 30);
                int def = CoffeeCheckupManager.GetReminderWindowDays();

                ddlReminderWindow.Items.Clear();
                for (int i = min; i <= max; i++)
                    ddlReminderWindow.Items.Add(new ListItem(i.ToString(), i.ToString()));

                string lastUsed = Session["CoffeeCheckupReminderWindowDays"] as string;
                if (!string.IsNullOrEmpty(lastUsed) && ddlReminderWindow.Items.FindByValue(lastUsed) != null)
                    ddlReminderWindow.SelectedValue = lastUsed;
                else
                    ddlReminderWindow.SelectedValue = def.ToString();

                SetStatus("Preparing contact list...", isError: null);
            }
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;

            scriptManager.RegisterAsyncPostBackControl(btnPrepData);
            scriptManager.RegisterAsyncPostBackControl(btnRefreshCustomerCheckupList);
            scriptManager.RegisterAsyncPostBackControl(btnUpdate);
            scriptManager.RegisterAsyncPostBackControl(btnReload);
            scriptManager.RegisterAsyncPostBackControl(btnClearTodaysData);
            scriptManager.RegisterAsyncPostBackControl(ddlReminderWindow);
            scriptManager.RegisterAsyncPostBackControl(btnSend);
            scriptManager.RegisterPostBackControl(btnBack);
            scriptManager.RegisterAsyncPostBackControl(imgBtnEmailTestMode);
        }

        /// <summary>
        /// Shows alert icon beside Send when Web.config EmailTestMode is true.
        /// </summary>
        private void ApplyEmailTestModeIndicator()
        {
            bool testMode = ConfigHelper.GetBool("EmailTestMode", false);
            string testRecipient = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");

            imgBtnEmailTestMode.Visible = testMode;
            if (!testMode)
                return;

            string tip = $"Email TEST MODE is ON (Web.config EmailTestMode=true). " +
                         $"All checkup emails are redirected to: {testRecipient}";
            imgBtnEmailTestMode.ToolTip = tip;
            btnSend.ToolTip = "TEST MODE: emails go to " + testRecipient + " — then open reminder results";
        }

        private static string AppendEmailTestModeNote(string status)
        {
            if (!ConfigHelper.GetBool("EmailTestMode", false))
                return status;

            string testRecipient = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");
            return status + $" Email TEST MODE is ON — messages go to {testRecipient}.";
        }

        protected void imgBtnEmailTestMode_Click(object sender, ImageClickEventArgs e)
        {
            string testRecipient = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");
            new showMessageBox(this.Page, "Email Test Mode",
                "EmailTestMode is ON in Web.config.\n\n" +
                "Checkup emails will NOT go to contacts.\n" +
                "They are redirected to:\n" + testRecipient + "\n\n" +
                "Set EmailTestMode to false for production sends.");
        }

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(message))
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, "SendCoffeeCheckup STATUS: " + message);

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
                SetStatus("Warning: " + ex.Message, isError: true);
            }
        }

        protected void btnPrepData_Click(object sender, EventArgs e)
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                SetStatus("Preparing contact data...", isError: null);
                upnlSendEmail.Update();

                if (ddlReminderWindow.SelectedItem != null)
                    int.TryParse(ddlReminderWindow.SelectedValue, out reminderWindowDays);

                _coffeeCheckupManager.PrepareCustomerReminderData(reminderWindowDays);

                int adjustedCount = _coffeeCheckupManager.PostAdjustPreparedReminderData(reminderWindowDays);
                ViewState["AdjustedCount"] = adjustedCount;
                bool holidayInWindow = adjustedCount != -1;

                gvCustomerCheckup.SelectedIndex = -1;
                BindCheckupGrids();

                int contactCount = GetCustomerCount();
                stopwatch.Stop();

                btnPrepData.Text = "Refresh Data";
                btnPrepData.Visible = true;

                string status = $"Success — {contactCount} contact(s) prepared in {stopwatch.ElapsedMilliseconds / 1000.0:F1}s. You can send reminders.";
                if (holidayInWindow)
                {
                    string msg = adjustedCount > 0
                        ? $"Upcoming holiday detected. Adjusted {adjustedCount} prep/delivery date(s)."
                        : "Upcoming holiday detected. Dates were verified against closures; no changes were required.";

                    new showMessageBox(this.Page, "Holiday Notice", msg);
                    status += " " + msg;
                }

                var prepNotices = _coffeeCheckupManager.LastPrepNotices;
                if (prepNotices != null && prepNotices.Count > 0)
                    status += " " + string.Join(" ", prepNotices);

                SetStatus(AppendEmailTestModeNote(status), isError: false);
                upnlCustomerCheckup.Update();
                upnlSendEmail.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("error", $"SendCoffeeCheckup: Error in btnPrepData_Click: {ex.Message}");
                btnPrepData.Visible = true;
                btnPrepData.Text = "Retry Data Prep";
                SetStatus("Error: " + ex.Message + " Check the logs for more details.", isError: true);
                upnlCustomerCheckup.Update();
                upnlSendEmail.Update();
            }
        }
        private void BindCheckupGrids()
        {
            gvCustomerCheckup.DataSource = _coffeeCheckupManager.GetPreparedContacts("CompanyName");
            gvCustomerCheckup.DataBind();
            BindContactItemsGrid();
            upnlCustomerCheckup.Update();
        }

        private void BindContactItemsGrid()
        {
            if (gvCustomerCheckup.SelectedIndex < 0 || gvCustomerCheckup.SelectedDataKey == null)
            {
                gvItemsToConfirm.DataSource = null;
                gvItemsToConfirm.DataBind();
                ltrlSelectedContact.Text = string.Empty;
                return;
            }

            long contactId = Convert.ToInt64(gvCustomerCheckup.SelectedDataKey.Value);
            string company = string.Empty;
            if (gvCustomerCheckup.SelectedRow != null)
            {
                // 0=actions, 1=hidden id, 2=company hyperlink
                var link = gvCustomerCheckup.SelectedRow.Cells[2].Controls.OfType<HyperLink>().FirstOrDefault();
                company = link != null ? link.Text : gvCustomerCheckup.SelectedRow.Cells[2].Text;
            }

            if (string.IsNullOrWhiteSpace(company))
                company = _coffeeCheckupManager.GetPreparedContactDisplayName(contactId);

            var items = _coffeeCheckupManager.GetPreparedContactItems(contactId);
            gvItemsToConfirm.DataSource = items;
            gvItemsToConfirm.DataBind();

            ltrlSelectedContact.Text = string.IsNullOrWhiteSpace(company)
                ? $"<div class='small' style='margin-bottom:6px;font-size:x-small;'>{items.Count} item(s)</div>"
                : $"<div class='small' style='margin-bottom:6px;font-size:x-small;'><strong>{HttpUtility.HtmlEncode(company)}</strong> — {items.Count} item(s)</div>";
        }

        protected void gvCustomerCheckup_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindContactItemsGrid();
            upnlCustomerCheckup.Update();
        }

        protected void gvCustomerCheckup_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCustomerCheckup.PageIndex = e.NewPageIndex;
            gvCustomerCheckup.SelectedIndex = -1;
            BindCheckupGrids();
        }

        /// <summary>App-standard pager (Previous / squares / Next) — see Classes/GridPager.cs.</summary>
        protected void gvCustomerCheckup_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvCustomerCheckup, e.Row);
        }

        protected void gvCustomerCheckup_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "ExcludeThisTime", StringComparison.OrdinalIgnoreCase))
                return;

            if (!long.TryParse(Convert.ToString(e.CommandArgument), out long contactId) || contactId <= 0)
            {
                SetStatus("Could not exclude that contact from this run.", isError: true);
                upnlSendEmail.Update();
                return;
            }

            string companyName = _coffeeCheckupManager.GetPreparedContactDisplayName(contactId);
            ExcludeContactThisTime(contactId, companyName);
        }

        private void ExcludeContactThisTime(long contactId, string companyName = null)
        {
            if (_coffeeCheckupManager.ExcludePreparedContactThisTime(contactId))
            {
                gvCustomerCheckup.SelectedIndex = -1;
                BindCheckupGrids();
                int remaining = GetCustomerCount();
                string who = string.IsNullOrWhiteSpace(companyName) ? "Contact" : companyName.Trim();
                SetStatus(
                    $"Excluded {who} from this run. {remaining} contact(s) remain. Warning: Prep Data or Refresh List will add them back if still due.",
                    isError: null);
                upnlSendEmail.Update();
            }
            else
            {
                SetStatus("Could not exclude that contact from this run.", isError: true);
                upnlSendEmail.Update();
            }
        }

        private int GetCustomerCount()
        {
            try
            {
                return _coffeeCheckupManager.GetPreparedContactCount();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("error", $"SendCoffeeCheckup: Error getting contact count: {ex.Message}");
                return 0;
            }
        }

        // FIXED: Remove duplicate and use cached versions
        protected string GetAreaName(int AreaId)
        {
            if (!_cachedAreaNames.ContainsKey(AreaId))
            {
                _cachedAreaNames[AreaId] = _coffeeCheckupManager.GetCachedAreaName(AreaId);
            }
            return _cachedAreaNames[AreaId];
        }

        protected string GetItemDesc(int itemId)
        {
            if (!_cachedItemDescriptions.ContainsKey(itemId))
            {
                _cachedItemDescriptions[itemId] = _coffeeCheckupManager.GetCachedItemDescription(itemId);
            }
            return _cachedItemDescriptions[itemId];
        }

        protected string FormatItemQty(object qtyValue)
        {
            if (qtyValue == null || qtyValue == DBNull.Value)
                return SystemConstants.FormatConstants.FormatQuantity(0);

            double qty = Convert.ToDouble(qtyValue);
            return SystemConstants.FormatConstants.FormatQuantity(qty);
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                SetStatus("Starting coffee checkup process...", isError: null);
                
                var emailData = new SendCheckEmailTexts {
                    Header = this.tbxEmailIntro.Text,
                    Body = this.tbxEmailBody.Text,
                    Footer = this.tbxEmailFooter.Text
                };
                if (_coffeeCheckupManager.IsHolidayComingInWindow(reminderWindowDays))
                {
                    string holidayNote = MessageProvider.Get(MessageKeys.CoffeeCheckup.HolidayClosureEmailNote);
                    if (string.IsNullOrWhiteSpace(holidayNote))
                        holidayNote = "Please note: upcoming public holidays may affect delivery timing. Thanks for your understanding.";

                    emailData.Footer = (emailData.Footer ?? string.Empty) +
                       $"<p style='margin:8px 0'>{HttpUtility.HtmlEncode(holidayNote)}</p>";
                }

                SetStatus("Processing contacts...", isError: null);
                upnlSendEmail.Update();

                var batchResult = _coffeeCheckupManager.ProcessCoffeeCheckupReminders(
                    emailData,
                    includeOrdersEmailCc: chkCcOrdersEmail.Checked);

                string status = $"Complete — sent: {batchResult.TotalSent}, failed: {batchResult.TotalFailed}.";
                if (!chkCcOrdersEmail.Checked)
                    status += " (orders email CC off)";
                if (!string.IsNullOrWhiteSpace(batchResult.ErrorMessage))
                    status += " " + batchResult.ErrorMessage;
                SetStatus(status, isError: batchResult.TotalFailed > 0);
                upnlSendEmail.Update();

                string statusMessage = $"Coffee checkup process completed!\n\n" +
                                     $"Emails sent successfully: {batchResult.TotalSent}\n" +
                                     $"Failed / skipped: {batchResult.TotalFailed}\n" +
                                     $"Total contacts processed: {batchResult.TotalSent + batchResult.TotalFailed}";

                if (!chkCcOrdersEmail.Checked)
                    statusMessage += "\n\nOrders email was NOT CC'd on this send.";

                if (batchResult.TotalFailed > 0)
                {
                    statusMessage += "\n\nDetails:\n" + (batchResult.ErrorMessage ?? "See the results page for failed emails.");
                }

                RedirectToResultsPage(statusMessage);
            }
            catch (Exception ex)
            {
                SetStatus("Error: " + ex.Message, isError: true);
                upnlSendEmail.Update();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Error in btnSend_Click: {ex.Message}");
                new showMessageBox(this.Page, "Email Sending Error", ex.Message);
            }
        }

        protected void btnTestSingleCustomer_Click(object sender, EventArgs e)
        {
            try
            {
                SetStatus("Starting test mode...", isError: null);

                var allContacts = _coffeeCheckupManager.GetPreparedContactsWithItems();
                if (!allContacts.Any())
                {
                    SetStatus("No test contacts available. Run Prep Data first.", isError: true);
                    return;
                }

                var testContact = allContacts.First();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"TEST: Using customer {testContact.CompanyName} (ID: {testContact.CustomerID})");

                if (!_coffeeCheckupManager.ValidateCustomerEligibility(testContact))
                {
                    SetStatus($"Test customer {testContact.CompanyName} is not eligible for reminders.", isError: true);
                    return;
                }

                var emailData = new SendCheckEmailTexts
                {
                    Header = this.tbxEmailIntro.Text,
                    Body = this.tbxEmailBody.Text,
                    Footer = this.tbxEmailFooter.Text
                };

                var testResult = _coffeeCheckupManager.ProcessCoffeeCheckupReminders(emailData);

                SetStatus($"Test completed — sent: {testResult.TotalSent}, failed: {testResult.TotalFailed}.", isError: testResult.TotalFailed > 0);
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"TEST: Completed - Sent: {testResult.TotalSent}, Failed: {testResult.TotalFailed}");
            }
            catch (Exception ex)
            {
                SetStatus("Test failed: " + ex.Message, isError: true);
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"TEST ERROR: {ex.Message}");
            }
        }

        protected void btnClearTodaysData_Click(object sender, EventArgs e)
        {
            try
            {
                SetStatus("Clearing today's reminder data...", isError: null);

                int deletedCount = _coffeeCheckupManager.ClearTodaysSentReminderEntries();

                SetStatus($"Cleared {deletedCount} reminder entries from today.", isError: false);
                upnlSendEmail.Update();

                new showMessageBox(this.Page,
                    "Data Cleared",
                    $"Successfully removed {deletedCount} reminder log entries from today ({TimeZoneUtils.Now().Date:yyyy-MM-dd}).\n\nYou can now test again with clean data.");

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Cleared {deletedCount} today's reminder entries");
            }
            catch (Exception ex)
            {
                SetStatus("Error clearing data: " + ex.Message, isError: true);
                upnlSendEmail.Update();
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Error clearing today's data: {ex.Message}");

                new showMessageBox(this.Page,
                    "Clear Data Error",
                    $"Error clearing today's data: {ex.Message}");
            }
        }

        private void RedirectToResultsPage(string completionMessage = null)
        {
            try
            {
                DateTime sentDate = TimeZoneUtils.Now().Date;
                var stats = _coffeeCheckupManager.GetSentReminderDayStats(sentDate);

                string redirectUrl = $"{this.ResolveUrl("~/Pages/SentRemindersSheet.aspx")}" +
                                   $"?LastSentDate={stats.SentDate:yyyy-MM-dd}" +
                                   $"&TotalReminders={stats.TotalReminders}" +
                                   $"&UniqueCustomers={stats.UniqueCustomers}" +
                                   $"&Successful={stats.Successful}" +
                                   $"&Failed={stats.Failed}";

                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup,
                    $"SendCoffeeCheckup: Redirecting with stats - {stats.UniqueCustomers} customers, {stats.Successful}/{stats.TotalReminders} successful");

                var scriptManager = ScriptManager.GetCurrent(Page);
                if (scriptManager != null && scriptManager.IsInAsyncPostBack)
                {
                    // One script block: optional alert (properly encoded), then navigate.
                    // Register on the UpdatePanel so the async response includes the script.
                    var script = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(completionMessage))
                    {
                        script.Append("try{showAppMessage(");
                        script.Append(HttpUtility.JavaScriptStringEncode(completionMessage, true));
                        script.Append(");}catch(e){}");
                    }
                    script.Append("window.location.href=");
                    script.Append(HttpUtility.JavaScriptStringEncode(redirectUrl, true));
                    script.Append(";");

                    ScriptManager.RegisterStartupScript(
                        upnlSendEmail,
                        upnlSendEmail.GetType(),
                        "redirectSentReminders",
                        script.ToString(),
                        true);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(completionMessage))
                        new showMessageBox(this.Page, "Coffee Checkup Status", completionMessage);

                    Response.Redirect(redirectUrl, false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception redirectEx)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.SendCheckup, $"SendCoffeeCheckup: Redirect failed: {redirectEx.Message}");
                SetStatus("Process completed, but redirect to results failed.", isError: true);
                upnlSendEmail.Update();
            }
        }

        private void LoadEmailTexts()
        {
            var texts = _coffeeCheckupManager.GetEmailTexts();
            if (texts == null || texts.SCEMTID <= 0)
                return;
            this.ltrlEmailTextID.Text = texts.SCEMTID.ToString();
            this.tbxEmailIntro.Text = HttpUtility.HtmlDecode(texts.Header);
            this.tbxEmailBody.Text = HttpUtility.HtmlDecode(texts.Body);
            this.tbxEmailFooter.Text = texts.Footer;
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.ltrlEmailTextID.Text))
            {
                SetStatus("No email text ID loaded — cannot update.", isError: true);
                upnlSendEmail.Update();
                return;
            }
            var pEmailTextsData = new SendCheckEmailTexts
            {
                Header = HttpUtility.HtmlEncode(this.tbxEmailIntro.Text),
                Body = HttpUtility.HtmlEncode(this.tbxEmailBody.Text),
                Footer = HttpUtility.HtmlEncode(this.tbxEmailFooter.Text)
            };
            string result = _coffeeCheckupManager.UpdateEmailTexts(pEmailTextsData, Convert.ToInt32(this.ltrlEmailTextID.Text));
            bool failed = string.IsNullOrWhiteSpace(result)
                || result.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0
                || result.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0;
            SetStatus(string.IsNullOrWhiteSpace(result) ? "Email text updated." : result, isError: failed ? true : false);
            upnlSendEmail.Update();
        }

        protected void btnReload_Click(object sender, EventArgs e)
        {
            LoadEmailTexts();
            SetStatus("Email text reloaded.", isError: false);
            upnlSendEmail.Update();
        }

        // Helper methods for compatibility
        public string GetItemSKU(int pItemID) => _coffeeCheckupManager.GetCachedItemSKU(pItemID);
        public string GetPackagingDesc(int pPackagingID) => _coffeeCheckupManager.GetCachedPackagingDescription(pPackagingID);
        public string GetItemUoM(int pItemID) => _coffeeCheckupManager.GetCachedItemUoM(pItemID);

        protected void ddlReminderWindow_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["CoffeeCheckupReminderWindowDays"] = ddlReminderWindow.SelectedValue;
            btnPrepData_Click(sender, e);
        }
    }
}
