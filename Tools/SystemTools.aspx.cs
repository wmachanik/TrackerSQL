using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    public partial class SystemTools : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();

            if (!IsPostBack)
            {
                pnlToolResults.Visible = false;
                gvResults.Visible = false;
                ResultsTitleLabel.Visible = false;
                pnlResultsSection.Visible = false;
                SetStatus(string.Empty, null);
            }

            SetMessagesEditorButtonVisibility();
            SyncWooMappingToolCard();

            sw.Stop();
            RequestTiming.Write("SYSTEM TOOLS PAGE", sw.ElapsedMilliseconds + " ms");
        }

        private void SyncWooMappingToolCard()
        {
            if (btnWooMapping == null)
                return;

            bool wooOn = false;
            try
            {
                wooOn = new WooCommerceSettingsManager().IsIntegrationEnabled();
            }
            catch
            {
                wooOn = false;
            }

            if (wooOn)
            {
                btnWooMapping.Text = "Open";
                if (litWooMappingToolBlurb != null)
                    litWooMappingToolBlurb.Text = "Categories, item SKU maps, and enabled sync";
            }
            else
            {
                btnWooMapping.Text = MessageProvider.Get(MessageKeys.WooCommerce.StartWizard);
                if (litWooMappingToolBlurb != null)
                    litWooMappingToolBlurb.Text = MessageProvider.Get(MessageKeys.WooCommerce.MapNeedWooEnabled);
            }

        }

        protected void btnWooMapping_Click(object sender, EventArgs e)
        {
            bool wooOn = false;
            try
            {
                wooOn = new WooCommerceSettingsManager().IsIntegrationEnabled();
            }
            catch
            {
                wooOn = false;
            }

            if (wooOn)
                Response.Redirect("~/Tools/WooCommerceMapping.aspx");
            else
                Response.Redirect("~/Tools/SystemPreferences.aspx?section=woo&wizard=1");
        }

        private void SetMessagesEditorButtonVisibility()
        {
            try
            {
                if (btnMessagesEditor == null)
                    return;

                var user = Context?.User;
                bool canSee =
                    user != null &&
                    (user.IsInRole("Administrators") ||
                     user.IsInRole("Admin") ||
                     user.IsInRole("AgentManager"));

                btnMessagesEditor.Visible = canSee;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: Error setting MessagesEditor button visibility: " + ex.Message);
            }
        }

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = message ?? string.Empty;

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

        private void ShowResultsSection(bool showPrepGrid, bool showResultsGrid)
        {
            pnlResultsSection.Visible = true;
            pnlResetPrepDate.Visible = showPrepGrid;
            pnlToolResults.Visible = showResultsGrid || !string.IsNullOrWhiteSpace(ResultsTitleLabel.Text);
            gvResults.Visible = showResultsGrid;
            ResultsTitleLabel.Visible = !string.IsNullOrWhiteSpace(ResultsTitleLabel.Text);
        }

        protected void btnResetPrepDates_Click(object sender, EventArgs e)
        {
            try
            {
                ResultsTitleLabel.Text = "Area prep / delivery schedule";
                ShowResultsSection(showPrepGrid: true, showResultsGrid: false);

                int areasUpdated = new TrackerTools().SetNextPreparationDateByArea();

                if (areasUpdated < 0)
                {
                    SetStatus("Failed to reset prep dates. Check App_Data/ErrorLog.txt.", isError: true);
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Reset prep dates failed");
                }
                else if (areasUpdated == 0)
                {
                    SetStatus("No areas were updated. Check AreaPrepDaysTbl and App_Data/ErrorLog.txt.", isError: true);
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Reset prep dates - no areas updated");
                }
                else
                {
                    SetStatus("Prep/Delivery dates reset for " + areasUpdated + " area(s).", isError: false);
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Reset prep dates for " + areasUpdated + " areas");
                }

                BindAreaPrepDatesGrid();
                upnlSystemToolsButtons.Update();
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, isError: true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: Reset prep dates error: " + ex.Message);
                upnlSystemToolsButtons.Update();
            }
        }

        private void BindAreaPrepDatesGrid()
        {
            try
            {
                var rows = new NextPrepDateByAreaRepository().GetAreaPrepDateGrid()
                    ?? new List<AreaPrepDateRow>();

                gvAreaPrepDates.Visible = true;
                gvAreaPrepDates.DataSource = rows;
                gvAreaPrepDates.DataBind();

                if (rows.Count == 0)
                {
                    string current = ltrlStatus.Text ?? string.Empty;
                    SetStatus(
                        (string.IsNullOrWhiteSpace(current) ? string.Empty : current + " ")
                        + "(No rows in NextPreparationDateByAreasTbl)",
                        isError: true);
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database,
                    "BindAreaPrepDatesGrid error: " + ex.Message);
                SetStatus("Error loading grid: " + ex.Message, isError: true);
            }
        }

        protected void btnSetLastOrderDate_Click(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.System, "SystemTools: btnSetLastOrderDate_Click / SetLastRecurringOrderDate started.");

            try
            {
                var recurringOrdersRepository = new RecurringOrdersRepository();
                // All enabled lines: DateLastDone from ContactsItemUsageTbl when matching usage exists
                var usageUpdates = recurringOrdersRepository.SetLastRecurringOrderDate();

                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: Processed " + usageUpdates.Count + " enabled recurring item(s).");

                var results = new List<RecurringOrderUpdateResult>();
                int updatedCount = 0;
                int fromUsageCount = 0;
                int skippedCount = 0;

                foreach (var update in usageUpdates)
                {
                    string updateResult = update.UpdateResult ?? string.Empty;
                    bool fromUsage = update.LastUsageDate.HasValue
                        && update.AppliedLastDate.HasValue
                        && update.AppliedLastDate.Value.Date == update.LastUsageDate.Value.Date
                        && (!update.PreviousDateLastDone.HasValue
                            || update.LastUsageDate.Value.Date >= update.PreviousDateLastDone.Value.Date);

                    if (fromUsage)
                    {
                        fromUsageCount++;
                    }

                    if (updateResult.StartsWith("Skipped", StringComparison.OrdinalIgnoreCase))
                    {
                        skippedCount++;
                    }
                    else if (string.IsNullOrEmpty(updateResult)
                        || !updateResult.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                    {
                        updatedCount++;
                    }

                    results.Add(new RecurringOrderUpdateResult
                    {
                        RecurringOrderID = update.RecurringOrderID,
                        ContactName = update.CompanyName,
                        Item = update.ItemDesc,
                        LastOrderDate = update.AppliedLastDate.HasValue
                            ? update.AppliedLastDate.Value.ToString("yyyy-MM-dd")
                            : "(none)",
                        PreviousLastDate = update.PreviousDateLastDone.HasValue
                            ? update.PreviousDateLastDone.Value.ToString("yyyy-MM-dd")
                            : "(none)",
                        UpdateResult = string.IsNullOrWhiteSpace(updateResult) ? "Updated" : updateResult
                    });
                }

                ResultsTitleLabel.Text = usageUpdates.Count == 0
                    ? "Set Last Recurring Order Date: no enabled recurring order lines found."
                    : "Set Last Recurring Order Date Results: " + usageUpdates.Count
                        + " enabled line(s); " + updatedCount + " updated ("
                        + fromUsageCount + " from usage"
                        + (skippedCount > 0 ? ", " + skippedCount + " skipped" : string.Empty)
                        + ").";
                gvResults.DataSource = results;
                gvResults.DataBind();
                ShowResultsSection(showPrepGrid: false, showResultsGrid: true);
                SetStatus(
                    usageUpdates.Count > 0
                        ? "Processed " + usageUpdates.Count + " enabled line(s); updated " + updatedCount
                            + " (" + fromUsageCount + " from ContactsItemUsageTbl)."
                        : "No enabled recurring order lines found.",
                    isError: updatedCount > 0 ? false : (bool?)null);

                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: SetLastRecurringOrderDate processed " + usageUpdates.Count
                        + ", updated " + updatedCount + ", fromUsage " + fromUsageCount + ".");
                upnlSystemToolsButtons.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: SetLastRecurringOrderDate error: " + ex.Message);
                ResultsTitleLabel.Text = "Set Last Recurring Order Date: Error";
                ShowResultsSection(showPrepGrid: false, showResultsGrid: false);
                SetStatus(ex.Message, isError: true);
                upnlSystemToolsButtons.Update();
            }
        }

        protected void btnRecalcPredictions_Click(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.System, "SystemTools: btnRecalcPredictions_Click started.");

            try
            {
                bool staleOnly = chkRecalcPredictionsStaleOnly == null || chkRecalcPredictionsStaleOnly.Checked;
                var results = new PredictionManager().RecalculateBulk(staleOnly)
                    ?? new List<PredictionBulkRecalcResult>();

                int changed = 0;
                int unchanged = 0;
                int failed = 0;
                var changedNames = new List<string>();
                foreach (var row in results)
                {
                    if (row.Changed)
                    {
                        changed++;
                        if (!string.IsNullOrWhiteSpace(row.CompanyName))
                            changedNames.Add(row.CompanyName.Trim());
                        else
                            changedNames.Add("Contact " + row.ContactID);
                    }
                    else if (string.Equals(row.Result, "Unchanged", StringComparison.OrdinalIgnoreCase))
                    {
                        unchanged++;
                    }
                    else
                    {
                        failed++;
                    }
                }

                string scope = staleOnly ? "stale prediction contacts" : "all enabled prediction contacts";
                ResultsTitleLabel.Text = results.Count == 0
                    ? "Recalc Prediction Averages: no " + scope + " found."
                    : "Recalc Prediction Averages (" + scope + "): " + results.Count
                        + " processed — " + changed + " changed, " + unchanged + " unchanged"
                        + (failed > 0 ? ", " + failed + " failed" : string.Empty) + ".";

                // Grid: who changed first (already sorted), with before/after next coffee + daily.
                gvResults.DataSource = results;
                gvResults.DataBind();
                ShowResultsSection(showPrepGrid: false, showResultsGrid: results.Count > 0);

                string whoChanged;
                if (changedNames.Count == 0)
                {
                    whoChanged = "No next-date / average values changed.";
                }
                else if (changedNames.Count <= 25)
                {
                    whoChanged = "Changed: " + string.Join("; ", changedNames) + ".";
                }
                else
                {
                    whoChanged = "Changed (" + changedNames.Count + "): "
                        + string.Join("; ", changedNames.GetRange(0, 25))
                        + "; … and " + (changedNames.Count - 25) + " more (see grid).";
                }

                SetStatus(
                    results.Count > 0
                        ? whoChanged
                        : "No matching prediction contacts to recalculate.",
                    isError: failed > 0 ? true : (changed > 0 ? false : (bool?)null));

                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: RecalcPredictions staleOnly=" + staleOnly
                    + " count=" + results.Count + " changed=" + changed
                    + " unchanged=" + unchanged + " failed=" + failed
                    + (changedNames.Count > 0 ? " names=" + string.Join(", ", changedNames) : string.Empty));
                upnlSystemToolsButtons.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: RecalcPredictions error: " + ex.Message);
                ResultsTitleLabel.Text = "Recalc Prediction Averages: Error";
                ShowResultsSection(showPrepGrid: false, showResultsGrid: false);
                SetStatus(ex.Message, isError: true);
                upnlSystemToolsButtons.Update();
            }
        }

        protected void btnRecalcTotalCups_Click(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.System, "SystemTools: btnRecalcTotalCups_Click started.");

            try
            {
                var usageRepo = new ContactsUsageRepository();
                var trackerRepo = new TotalCountTrackerRepository();
                var previous = trackerRepo.GetLatest();
                var summary = usageRepo.GetCupCountSummary() ?? new CupCountTotalSummary();

                long total = summary.TotalCups;
                int snapshotValue = total > int.MaxValue ? int.MaxValue : (int)total;
                string userName = Context?.User?.Identity?.Name ?? "system";
                string comments = "System Tools: Recalc total cups by " + userName;

                int snapshotId = trackerRepo.Add(snapshotValue, comments);
                bool saved = snapshotId > 0;

                string previousText = previous?.TotalCount.HasValue == true
                    ? previous.TotalCount.Value.ToString("n0")
                    : "(none)";
                string previousDateText = previous?.CountDate.HasValue == true
                    ? previous.CountDate.Value.ToString("yyyy-MM-dd HH:mm")
                    : "—";

                var rows = new List<object>
                {
                    new { Item = "Previous tracker total", Value = previousText },
                    new { Item = "Previous tracker date", Value = previousDateText },
                    new { Item = "Recalculated total (home page)", Value = total.ToString("n0") },
                    new { Item = "Contacts with a cup reading", Value = summary.ContactsWithCount.ToString("n0") },
                    new { Item = "Prediction rows", Value = summary.PredictedRows.ToString("n0") },
                    new { Item = "Enabled contacts total", Value = summary.EnabledTotalCups.ToString("n0") },
                    new { Item = "Enabled contacts with a reading", Value = summary.EnabledContactsWithCount.ToString("n0") },
                    new { Item = "Tracker snapshot saved", Value = saved ? "Yes" : "No" }
                };

                ResultsTitleLabel.Text = saved
                    ? "Recalc Total Cups: " + total.ToString("n0") + " cups"
                    : "Recalc Total Cups: calculated " + total.ToString("n0") + " but snapshot was not saved.";
                gvResults.DataSource = rows;
                gvResults.DataBind();
                ShowResultsSection(showPrepGrid: false, showResultsGrid: true);

                SetStatus(
                    saved
                        ? "Home page total is now " + total.ToString("n0") + " cups (sum of each contact’s last cup count)."
                        : "Calculated " + total.ToString("n0") + " cups but could not write TotalCountTrackerTbl.",
                    isError: saved ? false : true);

                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: RecalcTotalCups previous=" + previousText
                    + " new=" + total
                    + " contactsWithCount=" + summary.ContactsWithCount
                    + " snapshotSaved=" + saved);
                upnlSystemToolsButtons.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: RecalcTotalCups error: " + ex.Message);
                ResultsTitleLabel.Text = "Recalc Total Cups: Error";
                ShowResultsSection(showPrepGrid: false, showResultsGrid: false);
                SetStatus(ex.Message, isError: true);
                upnlSystemToolsButtons.Update();
            }
        }

        protected void btnDisableInactiveClients_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime cutoff = TimeZoneUtils.Now().AddYears(-3).Date;
                var repo = new ContactsRepository();
                var inactiveContacts = repo.GetInactiveContacts(cutoff);

                if (inactiveContacts.Count == 0)
                {
                    ResultsTitleLabel.Text = "Disable Inactive Clients: No eligible contacts found.";
                    gvResults.DataSource = null;
                    gvResults.DataBind();
                    ShowResultsSection(showPrepGrid: false, showResultsGrid: false);
                    SetStatus("No active contacts are older than 3 years without orders.", isError: null);
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Disable Inactive Clients - no eligible contacts found.");
                    upnlSystemToolsButtons.Update();
                    return;
                }

                int disabledCount = repo.DisableInactiveContacts(cutoff);
                if (disabledCount < 0)
                {
                    ResultsTitleLabel.Text = "Disable Inactive Clients: Update failed";
                    ShowResultsSection(showPrepGrid: false, showResultsGrid: false);
                    SetStatus("An error occurred while disabling inactive clients.", isError: true);
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Disable Inactive Clients - UPDATE operation failed.");
                    upnlSystemToolsButtons.Update();
                    return;
                }

                ResultsTitleLabel.Text = "Disabled " + disabledCount
                    + " contacts (no orders since before " + cutoff.ToString("yyyy-MM-dd") + ").";
                gvResults.DataSource = inactiveContacts;
                gvResults.DataBind();
                ShowResultsSection(showPrepGrid: false, showResultsGrid: true);
                SetStatus("Disable Inactive Contacts completed.", isError: false);

                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: Disabled " + disabledCount + " inactive contacts.");
                upnlSystemToolsButtons.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: Disable inactive contacts error: " + ex.Message);
                ResultsTitleLabel.Text = "Disable Inactive Clients: Error";
                ShowResultsSection(showPrepGrid: false, showResultsGrid: false);
                SetStatus(ex.Message, isError: true);
                upnlSystemToolsButtons.Update();
            }
        }

        public class RecurringOrderUpdateResult
        {
            public int RecurringOrderID { get; set; }
            public string ContactName { get; set; }
            public string Item { get; set; }
            public string PreviousLastDate { get; set; }
            public string LastOrderDate { get; set; }
            public string UpdateResult { get; set; }
        }
    }
}
