using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Tools
{
    public partial class LogViewer : System.Web.UI.Page
    {
        private string SortExpression
        {
            get => ViewState["SortExpression"] as string ?? "Date";
            set => ViewState["SortExpression"] = value;
        }
        private SortDirection SortDirection
        {
            get => ViewState["SortDirection"] is SortDirection dir ? dir : SortDirection.Descending;
            set => ViewState["SortDirection"] = value;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindLogFiles();
                ddlDateRange.SelectedValue = "ThisWeek";
                ddlDateRange_SelectedIndexChanged(null, null);
               // cbShowTimezone.Checked = false;
                BindLogEntries();
            }
        }

        private void BindLogFiles()
        {
            string logFolder = Server.MapPath("~/App_Data/");
            var logFiles = Directory.GetFiles(logFolder, "*.log")
                .Select(f => Path.GetFileName(f))
                .OrderByDescending(f => f)
                .ToList();

            ddlLogFile.DataSource = logFiles;
            ddlLogFile.DataBind();

            var defaultFile = logFiles.FirstOrDefault(f => f.Equals("orders.log", StringComparison.OrdinalIgnoreCase)) ?? logFiles.FirstOrDefault();
            if (defaultFile != null)
                ddlLogFile.SelectedValue = defaultFile;
        }

        private void SetDefaultDates()
        {
            tbxToDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            tbxFromDate.Text = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
        }

        protected void btnGo_Click(object sender, EventArgs e) => BindLogEntries();
        protected void btnReset_Click(object sender, EventArgs e)
        {
            tbxSearchText.Text = "";
            SetDefaultDates();
            BindLogEntries();
        }
        protected void ddlLogFile_SelectedIndexChanged(object sender, EventArgs e) => BindLogEntries();
        protected void btnApplyDateFilter_Click(object sender, EventArgs e) => BindLogEntries();
        protected void cbShowTimezone_CheckedChanged(object sender, EventArgs e) { /* JS handles column visibility */ }

        private void BindLogEntries()
        {
            string logFile = ddlLogFile.SelectedValue;
            string logFolder = Server.MapPath("~/App_Data/");
            string filePath = Path.Combine(logFolder, logFile);

            var entries = new List<LogEntry>();
            if (File.Exists(filePath))
            {
                string searchText = tbxSearchText.Text.Trim();
                DateTime.TryParse(tbxFromDate.Text, out DateTime fromDate);
                DateTime.TryParse(tbxToDate.Text, out DateTime toDate);

                foreach (var line in File.ReadLines(filePath))
                {
                    var entry = ParseLogLine(line);
                    if (entry == null) continue;

                    if (entry.Date < fromDate || entry.Date > toDate.AddDays(1).AddTicks(-1))
                        continue;

                    if (!string.IsNullOrEmpty(searchText) && (entry.Message == null || entry.Message.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) < 0))
                        continue;

                    entries.Add(entry);
                }
            }

            // Sorting
            if (SortDirection == SortDirection.Ascending)
                entries = entries.OrderBy(e => GetSortValue(e, SortExpression)).ToList();
            else
                entries = entries.OrderByDescending(e => GetSortValue(e, SortExpression)).ToList();

            gvLogResults.DataSource = entries;
            gvLogResults.DataBind();
            lblStatus.Text = $"{entries.Count} entries found.";
        }

        private object GetSortValue(LogEntry entry, string sortExpression)
        {
            switch (sortExpression)
            {
                case "Date": return entry.Date;
                case "User": return entry.User;
                case "Message": return entry.Message;
                default: return entry.Date;
            }
        }
        protected void gvLogResults_Sorting(object sender, GridViewSortEventArgs e)
        {
            if (SortExpression == e.SortExpression)
            {
                // Toggle direction
                SortDirection = SortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                SortExpression = e.SortExpression;
                SortDirection = SortDirection.Descending; // Default to newest first
            }
            BindLogEntries();
        }
        private LogEntry ParseLogLine(string line)
        {
            // Format: [date] [User: name] [Zone: zone] message
            // zone excluded for now
            try
            {
                int idxDateStart = line.IndexOf('[');
                int idxDateEnd = line.IndexOf(']');
                int idxUserStart = line.IndexOf("[User:", idxDateEnd + 1);
                int idxUserEnd = line.IndexOf(']', idxUserStart + 1);
                int idxZoneStart = line.IndexOf("[Zone:", idxUserEnd + 1);
                int idxZoneEnd = line.IndexOf(']', idxZoneStart + 1);

                if (idxDateStart != 0 || idxDateEnd < 0 || idxUserStart < 0 || idxUserEnd < 0 || idxZoneStart < 0 || idxZoneEnd < 0)
                    return null;

                string dateStr = line.Substring(idxDateStart + 1, idxDateEnd - idxDateStart - 1).Trim();
                string userStr = line.Substring(idxUserStart + 6, idxUserEnd - idxUserStart - 6).Trim();
                string zoneStr = line.Substring(idxZoneStart + 6, idxZoneEnd - idxZoneStart - 6).Trim();
                string msgStr = line.Substring(idxZoneEnd + 1).Trim();

                DateTime date;
                if (!DateTime.TryParse(dateStr, out date)) return null;

                return new LogEntry
                {
                    Date = date,
                    User = userStr,
                    //Timezone = zoneStr,
                    Message = msgStr
                };
            }
            catch
            {
                return null;
            }
        }
        protected void btnDeleteLog_Click(object sender, EventArgs e)
        {
            string logFile = ddlLogFile.SelectedValue;
            string logFolder = Server.MapPath("~/App_Data/");
            string filePath = Path.Combine(logFolder, logFile);

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    lblStatus.Text = $"Log file '{logFile}' has been deleted.";
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Log file '{logFile}' was deleted by user.");
                }
                else
                {
                    lblStatus.Text = $"Log file '{logFile}' not found.";
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Log file '{logFile}' not found.");
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error deleting log file: {ex.Message}";
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Error deleting log file: {ex.Message}");
            }

            BindLogFiles();      // Rebind dropdown
            BindLogEntries();    // Rebind log entries
            upnlSelection.Update(); // Update the UI
        }
        protected void ddlDateRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Use app time zone for all date calculations
            var today = TimeZoneUtils.Now().Date;

            switch (ddlDateRange.SelectedValue)
            {
                case "ThisWeek":
                    // Last 7 days (including today) in app time zone
                    var sevenDaysAgo = today.AddDays(-6);
                    tbxFromDate.Text = sevenDaysAgo.ToString("yyyy-MM-dd");
                    tbxToDate.Text = today.ToString("yyyy-MM-dd");
                    tbxFromDate.Enabled = false;
                    tbxToDate.Enabled = false;
                    break;
                case "ThisMonth":
                    var firstDay = new DateTime(today.Year, today.Month, 1);
                    tbxFromDate.Text = firstDay.ToString("yyyy-MM-dd");
                    tbxToDate.Text = today.ToString("yyyy-MM-dd");
                    tbxFromDate.Enabled = false;
                    tbxToDate.Enabled = false;
                    break;
                case "LastMonth":
                    var lastMonth = today.AddMonths(-1);
                    var firstLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                    var lastLastMonth = firstLastMonth.AddMonths(1).AddDays(-1);
                    tbxFromDate.Text = firstLastMonth.ToString("yyyy-MM-dd");
                    tbxToDate.Text = lastLastMonth.ToString("yyyy-MM-dd");
                    tbxFromDate.Enabled = false;
                    tbxToDate.Enabled = false;
                    break;
                case "Custom":
                    tbxFromDate.Enabled = true;
                    tbxToDate.Enabled = true;
                    break;
            }
            BindLogEntries();
        }
        public class LogEntry
        {
            public DateTime Date { get; set; }
            public string User { get; set; }
            //public string Timezone { get; set; }
            public string Message { get; set; }
        }

        protected void gvLogResults_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvLogResults.PageIndex = e.NewPageIndex;
            BindLogEntries();
        }
    }
}