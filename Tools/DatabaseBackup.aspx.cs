//------------------------------------------------------------------------------
// TrackerSQL v3.x — Database Backup tool
// Creates timestamped SQL Server .bak files. Folder from Web.config
// DatabaseBackupFolder (absolute or ~/...), default ~/App_Data/Backup.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;

namespace TrackerSQL.Tools
{
    public partial class DatabaseBackup : Page
    {
        private const string BackupFolderSettingKey = "DatabaseBackupFolder";
        private const string DefaultBackupFolderVirtualPath = "~/App_Data/Backup";
        private const string ConnectionStringName = "TrackerDataSQL";

        protected ScriptManager smDatabaseBackup;
        protected UpdateProgress uprgDatabaseBackup;
        protected UpdatePanel upnlDatabaseBackup;
        protected Panel pnlDatabaseBackup;
        protected Button btnBackupNow;
        protected Button btnDownloadSelected;
        protected Button btnDeleteSelected;
        protected Button btnRefresh;
        protected ImageButton btnBack;
        protected GridView gvBackups;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;
        protected Literal ltrlBackupFolder;

        private sealed class BackupFileInfo
        {
            public string FileName { get; set; }
            public DateTime CreatedUtc { get; set; }
            public string CreatedDisplay { get; set; }
            public long SizeBytes { get; set; }
            public string SizeDisplay { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                EnsureBackupFolder();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "DatabaseBackup: could not create backup folder: " + ex.Message);
            }

            ltrlBackupFolder.Text = HttpUtility.HtmlEncode(GetBackupFolderPath());

            if (!IsPostBack)
            {
                try
                {
                    BindBackupList();
                }
                catch (Exception ex)
                {
                    SetStatus("Could not list backups: " + ex.Message, isError: true);
                }

                ApplyBackupAvailabilityHint();
            }
        }

        /// <summary>
        /// Warn when SQL is remote and folder is still the web App_Data default (SQL usually cannot see it).
        /// Custom absolute DatabaseBackupFolder (e.g. host www\db) stays enabled for try.
        /// </summary>
        private void ApplyBackupAvailabilityHint()
        {
            bool remote = IsRemoteSqlServer(GetConnectionStringBuilder());
            bool usingDefaultAppData = !HasCustomBackupFolderConfigured();

            if (remote && usingDefaultAppData)
            {
                SetStatus(
                    "SQL Server appears remote and DatabaseBackupFolder is not set. "
                    + "Set appSetting DatabaseBackupFolder in Web.config to a path SQL can write "
                    + "(on this host often h:\\root\\home\\…\\www\\db). "
                    + "Default App_Data\\Backup is only for local SQL Express.",
                    isError: null);
                btnBackupNow.Enabled = false;
                btnBackupNow.ToolTip = "Set DatabaseBackupFolder in Web.config to a SQL-visible path";
                return;
            }

            if (remote)
            {
                SetStatus(
                    "Using configured DatabaseBackupFolder. Path must be visible to the SQL Server service "
                    + "(same as your host manual backup folder).",
                    isError: null);
            }
        }

        protected void btnBackupNow_Click(object sender, EventArgs e)
        {
            try
            {
                string backupPath = CreateBackup();
                string fileName = Path.GetFileName(backupPath);
                BindBackupList();
                SetStatus("Backup created: " + fileName, isError: false);
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "DatabaseBackup: created " + backupPath);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "DatabaseBackup failed: " + ex.Message);
                SetStatus("Backup failed: " + ex.Message, isError: true);
                try { BindBackupList(); } catch { /* ignore list errors after backup fail */ }
            }
        }

        protected void btnDownloadSelected_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedFileNames();
            if (selected.Count == 0)
            {
                SetStatus("Select one backup to download.", isError: null);
                return;
            }

            if (selected.Count > 1)
            {
                SetStatus("Select only one backup to download (or use the row Download link).", isError: true);
                return;
            }

            try
            {
                TransmitBackupFile(selected[0]);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "DatabaseBackup download failed for " + selected[0] + ": " + ex.Message);
                SetStatus("Download failed: " + ex.Message, isError: true);
                BindBackupList();
            }
        }

        protected void gvBackups_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DownloadBackup", StringComparison.OrdinalIgnoreCase))
                return;

            string fileName = Convert.ToString(e.CommandArgument);
            try
            {
                TransmitBackupFile(fileName);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "DatabaseBackup download failed for " + fileName + ": " + ex.Message);
                SetStatus("Download failed: " + ex.Message, isError: true);
                BindBackupList();
            }
        }

        protected void gvBackups_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var sm = ScriptManager.GetCurrent(Page);
            var btnDownloadRow = e.Row.FindControl("btnDownloadRow") as LinkButton;
            if (sm != null && btnDownloadRow != null)
                sm.RegisterPostBackControl(btnDownloadRow);
        }

        protected void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            int deleted = 0;
            var errors = new List<string>();

            foreach (string fileName in GetSelectedFileNames())
            {
                try
                {
                    DeleteBackupFile(fileName);
                    deleted++;
                }
                catch (Exception ex)
                {
                    errors.Add(fileName + ": " + ex.Message);
                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "DatabaseBackup delete failed for " + fileName + ": " + ex.Message);
                }
            }

            BindBackupList();

            if (deleted == 0 && errors.Count == 0)
            {
                SetStatus("No backups were selected.", isError: null);
                return;
            }

            if (errors.Count == 0)
            {
                SetStatus("Deleted " + deleted + " backup file(s).", isError: false);
                return;
            }

            SetStatus(
                "Deleted " + deleted + " file(s). Some failed: " + string.Join("; ", errors),
                isError: true);
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            BindBackupList();
            SetStatus("Backup list refreshed.", isError: null);
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Tools/SystemTools.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private string CreateBackup()
        {
            EnsureBackupFolder();

            var builder = GetConnectionStringBuilder();
            string databaseName = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new InvalidOperationException("Connection string has no Initial Catalog / database name.");

            string stamp = TimeZoneUtils.Now().ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            string safeDbName = SanitizeFileToken(databaseName);
            string fileName = safeDbName + "_" + stamp + ".bak";
            string backupPath = Path.Combine(GetBackupFolderPath(), fileName);

            // SQL Server writes the file as its own service account — path must exist for SQL, not only IIS.
            string sql = "BACKUP DATABASE [" + databaseName.Replace("]", "]]") + "] TO DISK = @BackupPath "
                + "WITH COPY_ONLY, INIT, NAME = @BackupName, STATS = 10";

            using (var conn = new SqlConnection(builder.ConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.CommandTimeout = 600;
                cmd.Parameters.AddWithValue("@BackupPath", backupPath);
                cmd.Parameters.AddWithValue("@BackupName", databaseName + " backup " + stamp);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            if (!File.Exists(backupPath))
                throw new FileNotFoundException(
                    "SQL Server reported success but the web app cannot see the .bak file yet "
                    + "(folder permissions or SQL wrote elsewhere). Check: " + backupPath,
                    backupPath);

            return backupPath;
        }

        private List<string> GetSelectedFileNames()
        {
            var selected = new List<string>();
            foreach (GridViewRow row in gvBackups.Rows)
            {
                var chk = row.FindControl("chkSelect") as CheckBox;
                if (chk == null || !chk.Checked)
                    continue;

                string fileName = Convert.ToString(gvBackups.DataKeys[row.RowIndex].Value);
                if (!string.IsNullOrWhiteSpace(fileName))
                    selected.Add(fileName);
            }
            return selected;
        }

        private void TransmitBackupFile(string fileName)
        {
            string resolved = ResolveBackupFilePath(fileName);
            var fileInfo = new FileInfo(resolved);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("Backup file not found.", resolved);

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/octet-stream";
            Response.AddHeader("Content-Disposition",
                "attachment; filename=\"" + fileInfo.Name.Replace("\"", string.Empty) + "\"");
            Response.AddHeader("Content-Length", fileInfo.Length.ToString(CultureInfo.InvariantCulture));
            Response.TransmitFile(resolved);
            Response.Flush();
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private void DeleteBackupFile(string fileName)
        {
            string resolved = ResolveBackupFilePath(fileName);
            if (!File.Exists(resolved))
                throw new FileNotFoundException("Backup file not found.", resolved);

            File.Delete(resolved);
        }

        /// <summary>
        /// Resolves a .bak file name under App_Data\Backup and blocks path traversal.
        /// </summary>
        private string ResolveBackupFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)
                || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || fileName.Contains("..")
                || !fileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid backup file name.");
            }

            string fullPath = Path.Combine(GetBackupFolderPath(), fileName);
            string backupRoot = Path.GetFullPath(GetBackupFolderPath());
            string resolved = Path.GetFullPath(fullPath);
            if (!resolved.StartsWith(backupRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Backup path is outside the Backup folder.");

            return resolved;
        }

        private void BindBackupList()
        {
            EnsureBackupFolder();
            var files = Directory.GetFiles(GetBackupFolderPath(), "*.bak")
                .Select(path => new FileInfo(path))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Select(f => new BackupFileInfo
                {
                    FileName = f.Name,
                    CreatedUtc = f.LastWriteTimeUtc,
                    CreatedDisplay = f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    SizeBytes = f.Length,
                    SizeDisplay = FormatFileSize(f.Length)
                })
                .ToList();

            gvBackups.DataSource = files;
            gvBackups.DataBind();

            if (string.IsNullOrWhiteSpace(ltrlStatus.Text))
                SetStatus("Showing " + files.Count + " backup file(s).", isError: null);
        }

        private void EnsureBackupFolder()
        {
            string path = GetBackupFolderPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Resolves backup directory from Web.config DatabaseBackupFolder.
        /// Absolute path as-is; ~/virtual path via MapPath; empty → ~/App_Data/Backup.
        /// </summary>
        private string GetBackupFolderPath()
        {
            string configured = ConfigHelper.GetString(BackupFolderSettingKey, string.Empty);
            if (string.IsNullOrWhiteSpace(configured))
                return Path.GetFullPath(Server.MapPath(DefaultBackupFolderVirtualPath));

            configured = configured.Trim();
            if (configured.StartsWith("~/", StringComparison.Ordinal)
                || configured.StartsWith("~\\", StringComparison.Ordinal))
                return Path.GetFullPath(Server.MapPath(configured));

            return Path.GetFullPath(configured);
        }

        private static bool HasCustomBackupFolderConfigured()
        {
            return !string.IsNullOrWhiteSpace(ConfigHelper.GetString(BackupFolderSettingKey, string.Empty));
        }

        private static SqlConnectionStringBuilder GetConnectionStringBuilder()
        {
            var cs = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new ConfigurationErrorsException("Connection string '" + ConnectionStringName + "' was not found.");

            return new SqlConnectionStringBuilder(cs.ConnectionString);
        }

        /// <summary>
        /// True when Data Source is not this machine.
        /// </summary>
        private static bool IsRemoteSqlServer(SqlConnectionStringBuilder builder)
        {
            if (builder == null)
                return true;

            string dataSource = (builder.DataSource ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(dataSource))
                return true;

            string host = dataSource;
            int slash = host.IndexOf('\\');
            if (slash >= 0)
                host = host.Substring(0, slash);
            int comma = host.IndexOf(',');
            if (comma >= 0)
                host = host.Substring(0, comma);
            host = host.Trim().TrimStart('(').TrimEnd(')');

            if (string.IsNullOrEmpty(host))
                return true;

            return !(host.Equals(".", StringComparison.OrdinalIgnoreCase)
                || host.Equals("(local)", StringComparison.OrdinalIgnoreCase)
                || host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                || host.Equals("::1", StringComparison.OrdinalIgnoreCase));
        }

        private static string SanitizeFileToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Database";

            char[] invalid = Path.GetInvalidFileNameChars();
            var chars = value.Trim().Select(c => Array.IndexOf(invalid, c) >= 0 || c == ' ' ? '_' : c).ToArray();
            return new string(chars);
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return bytes + " B";
            double kb = bytes / 1024.0;
            if (kb < 1024)
                return kb.ToString("0.#") + " KB";
            double mb = kb / 1024.0;
            if (mb < 1024)
                return mb.ToString("0.#") + " MB";
            return (mb / 1024.0).ToString("0.##") + " GB";
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
    }
}
