// Decompiled with JetBrains decompiler
// Type: TrackerSQL.Tools.SystemTools
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using AjaxControlToolkit;
using AjaxControlToolkit.HtmlEditor.ToolbarButtons;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

//- only form later versions #nullable disable
namespace TrackerSQL.Tools
{
    public partial class SystemTools : System.Web.UI.Page
    {
        // private const int CONST_MINMONTHS = 3;
        //private StreamWriter _ColsStream;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlSetClientType.Visible = false;
                gvResults.Visible = false;
                ltrlStatus.Visible = false;
                ResultsTitleLabel.Visible = false;
                pnlResultsSection.Visible = false;
            }
            // Ensure Messages Editor button only shows for allowed roles every request
            SetMessagesEditorButtonVisibility();
        }

        private void SetMessagesEditorButtonVisibility()
        {
            try
            {
                // Defensive: control may not exist if markup not deployed yet
                if (btnMessagesEditor == null) return;

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

        /// <summary>
        /// DEPRECATED: Set Client Type handler - Uses entirely legacy code
        /// Button should be hidden or removed from UI
        /// TODO: Rewrite using modern repositories or delete feature entirely
        /// </summary>
        [Obsolete("This handler uses legacy Controls classes. Needs rewrite with modern repositories.")]
        protected void btnSetClientType_Click(object sender, EventArgs e)
        {
            // DEPRECATED - DO NOT USE
            // This uses 100% legacy code:
            // - ContactType (Controls folder legacy class)
            // - ClientUsageLinesTbl (legacy)
            // - ItemUsageTbl (legacy)
            // 
            // To rewrite: Would need modern repositories for:
            // - ContactsRepository
            // - ContactsItemSvcSummaryRepository  
            // - ContactsItemUsageRepository
            //
            // For now, show message that this feature is disabled
            new showMessageBox(this.Page, "Info",
                "This feature is currently disabled during modernization. Please contact support.");
            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "SystemTools: btnSetClientType_Click called - feature disabled");
        }

        protected void btnResetPrepDates_Click(object sender, EventArgs e)
        {
            try
            {
                pnlResultsSection.Visible = true;
                this.pnlResetPrepDate.Visible = true;

                // Direct SQL Server approach - no legacy code
                int areasUpdated = ResetPrepDatesDirectSQL();

                if (areasUpdated < 0)
                {
                    this.ltrlStatus.Text = "ERROR: Failed to reset prep dates. Check App_Data/ErrorLog.txt.";
                    this.ltrlStatus.Visible = true;
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Reset prep dates failed");
                }
                else if (areasUpdated == 0)
                {
                    this.ltrlStatus.Text = "Warning: No areas were updated. Check AreaPrepDaysTbl and App_Data/ErrorLog.txt.";
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Reset prep dates - no areas updated");
                }
                else
                {
                    this.ltrlStatus.Text = $"Prep/Delivery dates reset for {areasUpdated} area(s).";
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        $"SystemTools: Reset prep dates for {areasUpdated} areas");
                }
                this.ltrlStatus.Visible = true;
                ResultsTitleLabel.Text = "Area prep / delivery schedule";
                ResultsTitleLabel.Visible = true;

                BindAreaPrepDatesGrid();
            }
            catch (Exception ex)
            {
                this.ltrlStatus.Text = $"ERROR: {ex.Message}";
                this.ltrlStatus.Visible = true;
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    $"SystemTools: Reset prep dates error: {ex.Message}");
            }
        }

        /// <summary>
        /// Simplified direct SQL approach to reset prep dates
        /// </summary>
        private int ResetPrepDatesDirectSQL()
        {
            try
            {
                var tools = new TrackerTools();
                return tools.SetNextPreperationDateByArea();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database,
                    $"ResetPrepDatesDirectSQL error: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Bind the area prep dates grid using modern SQL instead of legacy SqlDataSource
        /// </summary>
        private void BindAreaPrepDatesGrid()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    string sql = @"
                        SELECT 
                            a.AreaName AS Area,
                            n.PreperationDate,
                            n.DeliveryDate,
                            n.NextPreperationDate,
                            n.NextDeliveryDate
                        FROM NextPreperationDateByAreasTbl n
                        LEFT OUTER JOIN AreasTbl a ON n.AreaID = a.AreaID
                        ORDER BY a.AreaName";

                    AppLogger.WriteLog(SystemConstants.LogTypes.Database,
                        $"BindAreaPrepDatesGrid: Executing SQL query");

                    var dt = db.ReturnDataTable(sql);

                    if (dt != null)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Database,
                            $"BindAreaPrepDatesGrid: Retrieved {dt.Rows.Count} rows");

                        this.gvAreaPrepDates.Visible = true;
                        this.gvAreaPrepDates.DataSource = dt;
                        this.gvAreaPrepDates.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                                "BindAreaPrepDatesGrid: No rows returned from query");
                            this.ltrlStatus.Text += " (No rows in NextPreperationDateByAreasTbl)";
                        }
                        else
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                                $"BindAreaPrepDatesGrid: Grid bound successfully with {dt.Rows.Count} rows");
                        }
                    }
                    else
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Database,
                            "BindAreaPrepDatesGrid: DataTable is null");
                        this.ltrlStatus.Text += " (Database query returned null)";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database,
                    $"BindAreaPrepDatesGrid error: {ex.Message}\n{ex.StackTrace}");
                this.ltrlStatus.Text = $"ERROR loading grid: {ex.Message}";
            }
        }

        protected void btnSetLastOrderDate_Click(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.System, "SystemTools: btnSetLastOrderDate_Click started.");

            System.Threading.Thread.Sleep(2000);

            var recurringOrdersRepository = new RecurringOrdersRepository();
            var summaries = recurringOrdersRepository.GetSummaries("RecurringOrderItemID", string.Empty, 1);

            AppLogger.WriteLog(SystemConstants.LogTypes.System, $"SystemTools: Found {summaries.Count} enabled recurring order items.");

            var results = new List<RecurringOrderUpdateResult>();
            int updatedCount = 0;

            foreach (var summary in summaries)
            {
                if (!summary.DateLastDone.HasValue || summary.RecurringOrderItemID <= 0)
                {
                    continue;
                }

                DateTime lastOrderDate = summary.DateLastDone.Value;
                string updateResult = recurringOrdersRepository.SetRecurringOrderItemDates(
                    lastOrderDate,
                    summary.RecurringOrderItemID,
                    true);

                results.Add(new RecurringOrderUpdateResult
                {
                    OrderID = summary.RecurringOrderID,
                    ContactName = summary.CompanyName,
                    Item = summary.ItemDesc,
                    LastOrderDate = lastOrderDate.ToString("yyyy-MM-dd"),
                    UpdateResult = updateResult
                });

                if (string.IsNullOrEmpty(updateResult) || !updateResult.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                {
                    updatedCount++;
                }
            }

            ResultsTitleLabel.Text = $"Set Last Order Date Results: {updatedCount} updated.";
            gvResults.DataSource = results;
            gvResults.DataBind();

            pnlResultsSection.Visible = true;
            pnlSetClientType.Visible = true;

            AppLogger.WriteLog(SystemConstants.LogTypes.System, $"SystemTools: SetLastOrderDate updated {updatedCount} recurring orders.");

            // Show message box to user
            string msg = updatedCount > 0
                ? $"A Total of {updatedCount} recurring orders were updated."
                : "No recurring orders were updated.";
            showMessageBox showMessageBox1 = new showMessageBox(this.Page, "Info", msg);
        }

        protected void btnDisableInactiveClients_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime cutoff = TimeZoneUtils.Now().AddYears(-3).Date;
                var repo = new ContactsRepository();

                // Step 1: Get inactive contacts
                var inactiveContacts = repo.GetInactiveContacts(cutoff);

                // If none, show message and exit
                if (inactiveContacts.Count == 0)
                {
                    pnlResultsSection.Visible = true;
                    pnlSetClientType.Visible = true;
                    ResultsTitleLabel.Text = "Disable Inactive Clients: No eligible contacts found.";
                    ltrlStatus.Text = "No active contacts are older than 3 years without orders.";
                    ltrlStatus.Visible = true;
                    gvResults.DataSource = null;
                    gvResults.DataBind();
                    new showMessageBox(this.Page, "Info", "No eligible contacts found.");
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Disable Inactive Clients - no eligible contacts found.");
                    return;
                }

                // Step 2: Disable them in bulk
                int disabledCount = repo.DisableInactiveContacts(cutoff);

                // If error during update, display message
                if (disabledCount < 0)
                {
                    pnlResultsSection.Visible = true;
                    pnlSetClientType.Visible = true;
                    gvResults.Visible = false;
                    ResultsTitleLabel.Visible = true;
                    ltrlStatus.Visible = true;

                    ResultsTitleLabel.Text = "Disable Inactive Clients: Update failed";
                    ltrlStatus.Text = "An error occurred while disabling inactive clients.";
                    new showMessageBox(this.Page, "Error",
                        "An error occurred disabling inactive contacts. Check logs for details.");
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "SystemTools: Disable Inactive Clients - UPDATE operation failed.");
                    return;
                }

                // Success: show results
                pnlResultsSection.Visible = true;
                pnlSetClientType.Visible = true;
                gvResults.Visible = true;
                ltrlStatus.Visible = true;
                ResultsTitleLabel.Visible = true;

                ResultsTitleLabel.Text = $"Disabled {disabledCount} contacts (no orders since before {cutoff:yyyy-MM-dd}).";
                ltrlStatus.Text = "Disable Inactive Contacts completed.";
                gvResults.DataSource = inactiveContacts;
                gvResults.DataBind();

                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    $"SystemTools: Disabled {disabledCount} inactive contacts.");
                new showMessageBox(this.Page, "Info", $"Disabled {disabledCount} inactive contacts.");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "SystemTools: Disable inactive contacts error: " + ex.Message);
                new showMessageBox(this.Page, "Error",
                    "An error occurred disabling inactive contacts: " + ex.Message);
            }
        }

        public class RecurringOrderUpdateResult
        {
            public int OrderID { get; set; }
            public string ContactName { get; set; }
            public string Item { get; set; }
            public string LastOrderDate { get; set; }
            public string UpdateResult { get; set; }
        }
    }
}
