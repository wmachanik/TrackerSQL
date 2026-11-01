//------------------------------------------------------------------------------
// TrackerSQL v3.x — XMLtoSQL
// System tools page: run <command> batches from App_Data XML against SQL Server.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Newtonsoft.Json;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

namespace TrackerSQL.Tools
{
    public partial class XMLtoSQL : Page
    {
        private const string DefaultReturnUrl = "~/Tools/SystemTools.aspx";
        private const string LogName = "xmltosql";
        private const string VsModeTab = "XmlToSql.ModeTab";
        private const string VsFileSort = "XmlToSql.FileSort";
        private const string ArchiveFolderName = "Archive";
        private const string RunLogFileName = "xmltosql-run-log.json";
        private const string SmokeTestFileName = "SQLCommands_Test_SQLServer.xml";
        private const int ArchiveAfterDays = 13;
        private const string FolderAppData = "App_Data";
        private const string FolderArchive = "Archive";
        private static readonly object RunLogLock = new object();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetDefaultFileName();
                ShowModeTab(0);
                SetStatus("Select an XML command file, then click Execute. Prefer SQLCommands_Test_SQLServer.xml for a safe smoke test.", isError: null);
            }
            else
            {
                ApplyTabHighlight(GetModeTab());
            }

            // Belt-and-suspenders: Tools/Web.config already restricts to Administrators.
            if (!SecurityManager.IsAdmin())
            {
                Response.Redirect("~/Account/Login.aspx", true);
            }
        }

        private int GetModeTab()
        {
            object v = ViewState[VsModeTab];
            if (v is int i)
                return i;
            return 0;
        }

        private void ShowModeTab(int index)
        {
            if (index < 0) index = 0;
            if (index > 1) index = 1;
            ViewState[VsModeTab] = index;
            if (mvModes != null)
                mvModes.ActiveViewIndex = index;
            ApplyTabHighlight(index);
        }

        private void ApplyTabHighlight(int index)
        {
            if (btnTabXml != null)
                btnTabXml.CssClass = index == 0 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            if (btnTabManual != null)
                btnTabManual.CssClass = index == 1 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
        }

        protected void btnTabXml_Click(object sender, EventArgs e)
        {
            ShowModeTab(0);
            SetStatus("XML packs mode — pick a file and Execute.", isError: null);
            upnlXmlToSql.Update();
        }

        protected void btnTabManual_Click(object sender, EventArgs e)
        {
            ShowModeTab(1);
            BindManualHistory();
            SetStatus("Manual SQL mode — enter a single statement, then Execute SQL.", isError: null);
            upnlXmlToSql.Update();
        }

        private void ClearResults()
        {
            pnlSQLResults.Controls.Clear();
            gvSQLResults.DataSource = null;
            gvSQLResults.DataBind();
        }

        private void BindManualHistory()
        {
            if (gvManualHistory == null)
                return;
            gvManualHistory.DataSource = ManualSqlHistoryStore.LoadAll();
            gvManualHistory.DataBind();
        }

        private static string CurrentUserName()
        {
            try
            {
                string name = System.Web.HttpContext.Current?.User?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(name))
                    return "(unknown)";
                if (name.Contains("\\"))
                    name = name.Substring(name.LastIndexOf('\\') + 1);
                return name;
            }
            catch
            {
                return "(unknown)";
            }
        }

        private void ShowSelectResult(string sql, DataTable table, string heading, bool truncatedNote = false)
        {
            if (table == null)
                return;

            string note = truncatedNote
                ? " <em>(history snapshot truncated)</em>"
                : string.Empty;
            var title = new Literal
            {
                Text = "<h4>" + Server.HtmlEncode(heading ?? "SELECT result") + "</h4>"
                    + "<pre style='white-space:pre-wrap;'>" + Server.HtmlEncode(sql ?? string.Empty) + "</pre>"
                    + "<p><em>Rows: " + table.Rows.Count + "</em>" + note + "</p>"
            };
            var grid = new GridView
            {
                CssClass = "results-table",
                AutoGenerateColumns = true
            };
            grid.DataSource = table;
            grid.DataBind();

            pnlSQLResults.Controls.Add(title);
            pnlSQLResults.Controls.Add(grid);
            pnlSQLResults.Controls.Add(new Literal { Text = "<hr />" });
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

        private void SetDefaultFileName()
        {
            BindFolderDropdown();
            ApplySortHighlight();
            string folderPath = AppDataFolderPath();
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    FileNameTextBox.Text = string.Empty;
                    SetStatus("App_Data directory not found: " + folderPath, isError: true);
                    return;
                }

                // Prefer the SQL Server smoke-test file when present.
                string testFile = Path.Combine(folderPath, SmokeTestFileName);
                if (File.Exists(testFile))
                    FileNameTextBox.Text = testFile;
                else
                {
                    FileInfo newest = new DirectoryInfo(folderPath)
                        .GetFiles("SQLCommands*.xml")
                        .OrderByDescending(f => f.LastWriteTimeUtc)
                        .FirstOrDefault();

                    FileNameTextBox.Text = newest != null
                        ? newest.FullName
                        : Path.Combine(folderPath, SmokeTestFileName);
                }

                LoadFileBrowser();
            }
            catch (Exception ex)
            {
                SetStatus("Error locating XML files: " + ex.Message, isError: true);
                AppLogger.WriteLog(LogName, "SetDefaultFileName: " + ex.Message);
            }
        }

        private void BindFolderDropdown()
        {
            if (ddlXmlFolder == null)
                return;
            if (ddlXmlFolder.Items.Count == 0)
            {
                ddlXmlFolder.Items.Add(new ListItem(FolderAppData, FolderAppData));
                ddlXmlFolder.Items.Add(new ListItem(FolderAppData + "\\" + ArchiveFolderName, FolderArchive));
            }
            if (ddlXmlFolder.SelectedIndex < 0)
                ddlXmlFolder.SelectedValue = FolderAppData;
        }

        private string CurrentSort()
        {
            string sort = ViewState[VsFileSort] as string;
            return string.Equals(sort, "date", StringComparison.OrdinalIgnoreCase) ? "date" : "name";
        }

        private void ApplySortHighlight()
        {
            bool byDate = CurrentSort() == "date";
            if (btnSortName != null)
                btnSortName.CssClass = byDate ? "sys-prefs-tab" : "sys-prefs-tab is-active";
            if (btnSortDate != null)
                btnSortDate.CssClass = byDate ? "sys-prefs-tab is-active" : "sys-prefs-tab";
        }

        protected void btnSortName_Click(object sender, EventArgs e)
        {
            ViewState[VsFileSort] = "name";
            ApplySortHighlight();
            LoadFileBrowser();
            upnlXmlToSql.Update();
        }

        protected void btnSortDate_Click(object sender, EventArgs e)
        {
            ViewState[VsFileSort] = "date";
            ApplySortHighlight();
            LoadFileBrowser();
            upnlXmlToSql.Update();
        }

        protected void ddlXmlFolder_Changed(object sender, EventArgs e)
        {
            LoadFileBrowser();
            SetStatus("Showing folder " + GetBrowseFolderDisplayName() + ".", isError: null);
            upnlXmlToSql.Update();
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect(DefaultReturnUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void RefreshFilesButton_Click(object sender, EventArgs e)
        {
            BindFolderDropdown();
            ApplySortHighlight();
            LoadFileBrowser();
            SetStatus("File list refreshed.", isError: null);
            upnlXmlToSql.Update();
        }

        protected void btnArchive_Click(object sender, EventArgs e)
        {
            int moved = ArchiveDueXmlFiles();
            LoadFileBrowser();
            RedirectTextBoxIfArchived();
            SetStatus(
                moved > 0
                    ? "Moved " + moved + " pack(s) older than " + ArchiveAfterDays + " days to Archive. Choose Folder → App_Data\\Archive to see them."
                    : "Nothing to archive — no App_Data command packs older than " + ArchiveAfterDays + " days.",
                isError: null);
            upnlXmlToSql.Update();
        }

        protected void btnUnarchive_Click(object sender, EventArgs e)
        {
            string filePath = (FileNameTextBox.Text ?? string.Empty).Trim();
            string archive = Path.GetFullPath(ArchiveFolderPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string appData = Path.GetFullPath(AppDataFolderPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                SetStatus("Choose a file in Archive first.", isError: true);
                upnlXmlToSql.Update();
                return;
            }

            string full = Path.GetFullPath(filePath);
            string dir = Path.GetDirectoryName(full);
            if (string.IsNullOrEmpty(dir)
                || !string.Equals(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), archive, StringComparison.OrdinalIgnoreCase))
            {
                SetStatus("Unarchive only moves a file that is in App_Data\\Archive.", isError: true);
                upnlXmlToSql.Update();
                return;
            }

            string dest = Path.Combine(appData, Path.GetFileName(full));
            if (File.Exists(dest))
            {
                SetStatus("App_Data already has " + Path.GetFileName(full) + ".", isError: true);
                upnlXmlToSql.Update();
                return;
            }

            try
            {
                File.Move(full, dest);
                FileNameTextBox.Text = dest;
                if (ddlXmlFolder != null && ddlXmlFolder.Items.FindByValue(FolderAppData) != null)
                    ddlXmlFolder.SelectedValue = FolderAppData;
                LoadFileBrowser();
                SetStatus("Moved " + Path.GetFileName(dest) + " back to App_Data.", isError: false);
                AppLogger.WriteLog(LogName, "Unarchived " + Path.GetFileName(dest));
            }
            catch (Exception ex)
            {
                SetStatus("Unarchive failed: " + ex.Message, isError: true);
                AppLogger.WriteLog(LogName, "Unarchive failed: " + ex.Message);
            }

            upnlXmlToSql.Update();
        }

        protected void GoButton_Click(object sender, EventArgs e)
        {
            ClearResults();

            string filePath = FileNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(filePath))
            {
                SetStatus("Please specify a file path.", isError: true);
                upnlXmlToSql.Update();
                return;
            }

            if (!File.Exists(filePath))
            {
                SetStatus("File not found: " + filePath, isError: true);
                LoadFileBrowser();
                AppLogger.WriteLog(LogName, "File not found: " + filePath);
                upnlXmlToSql.Update();
                return;
            }

            List<SqlCommandResult> commands;
            try
            {
                commands = LoadCommandsFromXml(filePath);
            }
            catch (Exception ex)
            {
                SetStatus("XML processing error: " + ex.Message, isError: true);
                AppLogger.WriteLog(LogName, "XML processing error: " + ex.Message);
                upnlXmlToSql.Update();
                return;
            }

            if (commands.Count == 0)
            {
                SetStatus("No <command> elements found in the XML file.", isError: true);
                upnlXmlToSql.Update();
                return;
            }

            AppLogger.WriteLog(LogName, "Starting XML processing: " + filePath + " (" + commands.Count + " commands)");

            for (int index = 0; index < commands.Count; index++)
            {
                SqlCommandResult cmd = commands[index];
                try
                {
                    ExecuteCommand(cmd, index + 1);
                }
                catch (Exception cmdEx)
                {
                    cmd.Succeeded = false;
                    cmd.Error = "Exception: " + cmdEx.Message;
                    AppLogger.WriteLog(LogName, "Exception in command " + (index + 1) + ": " + cmdEx.Message);
                }
            }

            BindSummary(commands);

            try
            {
                RecordXmlRun(filePath);
                LoadFileBrowser();
                RedirectTextBoxIfArchived();
            }
            catch (Exception archEx)
            {
                AppLogger.WriteLog(LogName, "After execute: " + archEx.Message);
            }

            upnlXmlToSql.Update();
        }

        protected void btnManualExecute_Click(object sender, EventArgs e)
        {
            ClearResults();
            ShowModeTab(1);

            string sql = (tbxManualSql.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(sql))
            {
                BindManualHistory();
                SetStatus("Enter a SQL statement first.", isError: true);
                upnlXmlToSql.Update();
                return;
            }

            string typeChoice = (ddlManualType.SelectedValue ?? "auto").Trim().ToLowerInvariant();
            string type = typeChoice == "auto" ? DetectSqlType(sql) : typeChoice;

            var cmd = new SqlCommandResult
            {
                Type = type,
                Sql = sql
            };

            AppLogger.WriteLog(LogName, "Manual SQL (" + type + "): " + cmd.SqlPreview);

            DataTable selectTable = null;
            try
            {
                selectTable = ExecuteCommand(cmd, 1);
            }
            catch (Exception ex)
            {
                cmd.Succeeded = false;
                cmd.Error = "Exception: " + ex.Message;
                AppLogger.WriteLog(LogName, "Manual SQL exception: " + ex.Message);
            }

            string historyMessage = cmd.Succeeded
                ? (string.IsNullOrWhiteSpace(cmd.Error) ? "OK" : cmd.Error)
                : (cmd.Error ?? "Failed");

            try
            {
                ManualSqlHistoryStore.Add(
                    cmd.Type,
                    sql,
                    cmd.Succeeded,
                    historyMessage,
                    selectTable,
                    CurrentUserName());
            }
            catch (Exception histEx)
            {
                AppLogger.WriteLog(LogName, "Manual SQL history save failed: " + histEx.Message);
            }

            BindSummary(new List<SqlCommandResult> { cmd });
            BindManualHistory();
            upnlXmlToSql.Update();
        }

        protected void btnManualClear_Click(object sender, EventArgs e)
        {
            ShowModeTab(1);
            tbxManualSql.Text = string.Empty;
            ddlManualType.SelectedValue = "auto";
            ClearResults();
            BindManualHistory();
            SetStatus("Cleared.", isError: null);
            upnlXmlToSql.Update();
        }

        protected void gvManualHistory_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "OpenHistory", StringComparison.OrdinalIgnoreCase))
                return;

            ShowModeTab(1);
            string id = Convert.ToString(e.CommandArgument);
            ManualSqlHistoryEntry entry = ManualSqlHistoryStore.GetById(id);
            if (entry == null)
            {
                BindManualHistory();
                SetStatus("History entry not found.", isError: true);
                upnlXmlToSql.Update();
                return;
            }

            tbxManualSql.Text = entry.Sql ?? string.Empty;
            string type = (entry.Type ?? "auto").Trim().ToLowerInvariant();
            if (ddlManualType.Items.FindByValue(type) != null)
                ddlManualType.SelectedValue = type;
            else
                ddlManualType.SelectedValue = "auto";

            ClearResults();
            var summary = new SqlCommandResult
            {
                Type = entry.Type,
                Sql = entry.Sql,
                Succeeded = entry.Succeeded,
                Error = entry.Message
            };
            BindSummary(new List<SqlCommandResult> { summary });

            if (entry.ResultSnapshot != null && entry.ResultSnapshot.Columns != null
                && entry.ResultSnapshot.Columns.Count > 0)
            {
                DataTable table = ManualSqlHistoryStore.SnapshotToDataTable(entry.ResultSnapshot);
                ShowSelectResult(
                    entry.Sql,
                    table,
                    "Saved SELECT result (" + entry.RanAtDisplay + ")",
                    truncatedNote: entry.ResultSnapshot.Truncated);
            }

            BindManualHistory();
            SetStatus(
                "Loaded history from " + entry.RanAtDisplay
                + (entry.Succeeded ? " (OK)." : " (failed).")
                + " Edit and Execute SQL to run again.",
                isError: entry.Succeeded ? (bool?)false : true);
            upnlXmlToSql.Update();
        }

        private void BindSummary(List<SqlCommandResult> commands)
        {
            gvSQLResults.DataSource = commands;
            gvSQLResults.DataBind();

            int skippedCount = commands.Count(c =>
                string.Equals(c.Type, "disabled", StringComparison.OrdinalIgnoreCase));
            int successCount = commands.Count(c =>
                c.Succeeded && !string.Equals(c.Type, "disabled", StringComparison.OrdinalIgnoreCase));
            int failureCount = commands.Count(c => !c.Succeeded);

            SetStatus(
                "Execution completed. Total: " + commands.Count
                + " — ok: " + successCount
                + " — failed: " + failureCount
                + " — skipped: " + skippedCount
                + ".",
                isError: failureCount > 0 ? true : (bool?)false);

            AppLogger.WriteLog(LogName,
                "Execution completed. Total=" + commands.Count
                + " OK=" + successCount
                + " Fail=" + failureCount
                + " Skip=" + skippedCount);
        }

        private static string DetectSqlType(string sql)
        {
            string s = StripInlineComments(sql ?? string.Empty).TrimStart();
            if (string.IsNullOrEmpty(s))
                return "unknown";

            // Skip leading parentheses / WITH CTE → treat as SELECT when it queries.
            if (s.StartsWith("with", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("select", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("(select", StringComparison.OrdinalIgnoreCase))
                return "select";
            if (s.StartsWith("insert", StringComparison.OrdinalIgnoreCase))
                return "insert";
            if (s.StartsWith("update", StringComparison.OrdinalIgnoreCase))
                return "update";
            if (s.StartsWith("delete", StringComparison.OrdinalIgnoreCase))
                return "delete";
            if (s.StartsWith("create", StringComparison.OrdinalIgnoreCase))
                return "create";
            if (s.StartsWith("alter", StringComparison.OrdinalIgnoreCase))
                return "alter";
            if (s.StartsWith("drop", StringComparison.OrdinalIgnoreCase))
                return "drop";
            if (s.StartsWith("exec", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("execute", StringComparison.OrdinalIgnoreCase))
                return "exec";

            return "exec";
        }

        /// <summary>
        /// Runs one command. For SELECT, returns the result table (also rendered into pnlSQLResults).
        /// </summary>
        private DataTable ExecuteCommand(SqlCommandResult cmd, int ordinal)
        {
            string type = (cmd.Type ?? string.Empty).Trim().ToLowerInvariant();
            AppLogger.WriteLog(LogName, "Executing command " + ordinal + ": " + type);

            if (type == "select")
            {
                DataTable table = RunSelect(cmd.Sql);
                cmd.Succeeded = table != null;

                if (!cmd.Succeeded)
                {
                    cmd.Error = "SELECT failed or returned no result set.";
                    return null;
                }

                cmd.Error = "Rows: " + table.Rows.Count;
                ShowSelectResult(cmd.Sql, table, "SELECT result " + ordinal);
                return table;
            }

            if (type == "disabled")
            {
                cmd.Succeeded = true;
                cmd.Error = "Skipped (disabled)";
                return null;
            }

            if (type == "update" || type == "insert" || type == "delete"
                || type == "create" || type == "alter" || type == "drop"
                || type == "exec" || type == "execute")
            {
                if (type == "create" || type == "alter")
                    cmd.Sql = StripInlineComments(cmd.Sql);

                string err;
                int rowsAffected = RunCommand(cmd.Sql, out err);
                cmd.Succeeded = string.IsNullOrWhiteSpace(err);
                cmd.Error = cmd.Succeeded
                    ? (rowsAffected >= 0 ? "Rows affected: " + rowsAffected : "OK")
                    : err;

                if (cmd.Succeeded && type == "create"
                    && cmd.Sql.Trim().StartsWith("create table", StringComparison.OrdinalIgnoreCase))
                {
                    string created = ExtractCreatedTableName(cmd.Sql);
                    if (!string.IsNullOrEmpty(created) && !TableExists(created))
                    {
                        cmd.Succeeded = false;
                        cmd.Error = "CREATE reported success but table not found: " + created;
                    }
                }

                return null;
            }

            cmd.Succeeded = false;
            cmd.Error = "Unknown command type: " + cmd.Type;
            return null;
        }

        private static List<SqlCommandResult> LoadCommandsFromXml(string filePath)
        {
            var list = new List<SqlCommandResult>();

            var settings = new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader reader = XmlReader.Create(filePath, settings))
            {
                // Important: ReadElementContentAsString() leaves the reader on the *next* node.
                // Do not call Read() again in that case, or every other <command> is skipped.
                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.Element
                        && string.Equals(reader.LocalName, "command", StringComparison.OrdinalIgnoreCase))
                    {
                        var cmd = new SqlCommandResult
                        {
                            Type = reader.GetAttribute("type") ?? "unknown"
                        };

                        cmd.Sql = reader.ReadElementContentAsString().Trim();
                        list.Add(cmd);
                        continue;
                    }

                    if (!reader.Read())
                        break;
                }
            }

            return list;
        }

        private string AppDataFolderPath()
        {
            return Server.MapPath("~/App_Data/");
        }

        private string ArchiveFolderPath()
        {
            return Path.Combine(AppDataFolderPath(), ArchiveFolderName);
        }

        private void EnsureArchiveFolder()
        {
            string dir = ArchiveFolderPath();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private string RunLogPath()
        {
            return Path.Combine(AppDataFolderPath(), RunLogFileName);
        }

        private Dictionary<string, DateTime> LoadRunLog()
        {
            lock (RunLogLock)
            {
                string path = RunLogPath();
                if (!File.Exists(path))
                    return new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    string json = File.ReadAllText(path);
                    var loaded = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(json);
                    var log = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
                    if (loaded != null)
                    {
                        foreach (KeyValuePair<string, DateTime> kv in loaded)
                            log[kv.Key] = kv.Value;
                    }
                    return log;
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(LogName, "Load run log: " + ex.Message);
                    return new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        private void SaveRunLog(Dictionary<string, DateTime> log)
        {
            lock (RunLogLock)
            {
                string json = JsonConvert.SerializeObject(log ?? new Dictionary<string, DateTime>(), Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(RunLogPath(), json);
            }
        }

        private static bool IsArchivableCommandPack(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;
            if (string.Equals(fileName, SmokeTestFileName, StringComparison.OrdinalIgnoreCase))
                return false;
            return fileName.StartsWith("SQLCommands", StringComparison.OrdinalIgnoreCase)
                && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
        }

        private void RecordXmlRun(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string full = Path.GetFullPath(filePath);
            string name = Path.GetFileName(full);
            if (!IsArchivableCommandPack(name))
                return;

            string appData = Path.GetFullPath(AppDataFolderPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!full.StartsWith(appData, StringComparison.OrdinalIgnoreCase))
                return;

            string fileDir = Path.GetDirectoryName(full);
            if (string.IsNullOrEmpty(fileDir)
                || !string.Equals(fileDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), appData, StringComparison.OrdinalIgnoreCase))
                return;

            var log = LoadRunLog();
            log[name] = DateTime.UtcNow;
            SaveRunLog(log);
        }

        /// <summary>
        /// Moves SQLCommands packs sitting in App_Data whose file date is older than
        /// <see cref="ArchiveAfterDays"/> days. A run in the last 13 days keeps the file.
        /// </summary>
        private int ArchiveDueXmlFiles()
        {
            EnsureArchiveFolder();

            string appData = Path.GetFullPath(AppDataFolderPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string archive = Path.GetFullPath(ArchiveFolderPath());
            DateTime cutoff = DateTime.UtcNow.AddDays(-ArchiveAfterDays);
            var log = LoadRunLog();
            int moved = 0;

            if (!Directory.Exists(appData))
                return 0;

            foreach (FileInfo file in new DirectoryInfo(appData).GetFiles("SQLCommands*.xml"))
            {
                if (!IsArchivableCommandPack(file.Name))
                    continue;

                DateTime stamp = file.LastWriteTimeUtc;
                DateTime ran;
                if (log.TryGetValue(file.Name, out ran) && ran.ToUniversalTime() > stamp)
                    stamp = ran.ToUniversalTime();
                if (stamp > cutoff)
                    continue;

                string dest = Path.Combine(archive, file.Name);
                if (File.Exists(dest))
                {
                    string stem = Path.GetFileNameWithoutExtension(file.Name);
                    string ext = Path.GetExtension(file.Name);
                    dest = Path.Combine(archive, stem + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ext);
                }

                try
                {
                    File.Move(file.FullName, dest);
                    log.Remove(file.Name);
                    moved++;
                    AppLogger.WriteLog(LogName, "Archived " + file.Name + " → " + dest);
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(LogName, "Archive move failed for " + file.Name + ": " + ex.Message);
                }
            }

            if (moved > 0)
                SaveRunLog(log);

            return moved;
        }

        private void RedirectTextBoxIfArchived()
        {
            string current = (FileNameTextBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(current) || File.Exists(current))
                return;

            string archived = Path.Combine(ArchiveFolderPath(), Path.GetFileName(current));
            if (File.Exists(archived))
                FileNameTextBox.Text = archived;
        }

        private string GetBrowseFolderDisplayName()
        {
            BindFolderDropdown();
            string key = ddlXmlFolder?.SelectedValue ?? FolderAppData;
            if (string.Equals(key, FolderArchive, StringComparison.OrdinalIgnoreCase))
                return FolderAppData + "\\" + ArchiveFolderName;
            return FolderAppData;
        }

        private string ResolveBrowseFolderPath()
        {
            BindFolderDropdown();
            string key = ddlXmlFolder?.SelectedValue ?? FolderAppData;
            if (string.Equals(key, FolderArchive, StringComparison.OrdinalIgnoreCase))
            {
                EnsureArchiveFolder();
                return ArchiveFolderPath();
            }
            return AppDataFolderPath();
        }

        private List<FileInfo> OrderFiles(IEnumerable<FileInfo> files)
        {
            if (CurrentSort() == "date")
            {
                return files
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            return files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private void LoadFileBrowser()
        {
            BindFolderDropdown();
            ApplySortHighlight();
            string folderPath = ResolveBrowseFolderPath();
            try
            {
                if (ltrlFileBrowserTitle != null)
                    ltrlFileBrowserTitle.Text = "XML Files — " + GetBrowseFolderDisplayName();

                if (!Directory.Exists(folderPath))
                {
                    ltrlFileList.Text = "<em>Folder not found: " + Server.HtmlEncode(GetBrowseFolderDisplayName()) + "</em>";
                    return;
                }

                FileInfo[] allFiles = new DirectoryInfo(folderPath).GetFiles("*.xml");
                var html = new StringBuilder();

                if (allFiles.Length == 0)
                {
                    html.Append("<em>No XML files in this folder</em>");
                }
                else
                {
                    string selectedPath = (FileNameTextBox.Text ?? string.Empty).Trim();
                    var sqlCommandFiles = OrderFiles(allFiles
                        .Where(f => f.Name.StartsWith("SQLCommands", StringComparison.OrdinalIgnoreCase)));
                    var otherFiles = OrderFiles(allFiles
                        .Where(f => !f.Name.StartsWith("SQLCommands", StringComparison.OrdinalIgnoreCase)));

                    AppendFileGroup(html, "Migration / Command Files", sqlCommandFiles, highlight: true, selectedPath);
                    AppendFileGroup(html, "Other XML Files", otherFiles, highlight: false, selectedPath);
                }

                ltrlFileList.Text = html.ToString();
            }
            catch (Exception ex)
            {
                ltrlFileList.Text = "<em>Error loading files: " + Server.HtmlEncode(ex.Message) + "</em>";
            }
        }

        private static void AppendFileGroup(StringBuilder html, string title, List<FileInfo> files, bool highlight, string selectedPath)
        {
            if (files == null || files.Count == 0)
                return;

            html.Append("<strong>").Append(title).Append(":</strong>");
            string css = highlight ? "file-item xml-file" : "file-item";

            foreach (FileInfo file in files)
            {
                string fullPath = file.FullName;
                string jsPath = fullPath.Replace("\\", "\\\\").Replace("'", "\\'");
                string attrPath = System.Web.HttpUtility.HtmlAttributeEncode(fullPath);
                bool selected = !string.IsNullOrEmpty(selectedPath)
                    && string.Equals(fullPath, selectedPath, StringComparison.OrdinalIgnoreCase);
                string itemCss = selected ? css + " selected" : css;

                html.Append("<div class='").Append(itemCss)
                    .Append("' data-path=\"").Append(attrPath)
                    .Append("\" onclick=\"selectFile('")
                    .Append(jsPath).Append("')\">");
                html.Append(System.Web.HttpUtility.HtmlEncode(file.Name));
                html.Append(" <small>(")
                    .Append(file.LastWriteTime.ToString("yyyy-MM-dd HH:mm"))
                    .Append(", ")
                    .Append(file.Length)
                    .Append(" bytes)</small></div>");
            }

            html.Append("<br/>");
        }

        private static string StripInlineComments(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return sql;

            string[] lines = sql.Replace("\r", string.Empty).Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int idx = lines[i].IndexOf("--", StringComparison.Ordinal);
                if (idx >= 0)
                    lines[i] = lines[i].Substring(0, idx);
            }

            return string.Join(" ", lines).Trim();
        }

        private static string ExtractCreatedTableName(string sql)
        {
            try
            {
                string s = sql.Trim();
                int tblIdx = s.IndexOf("TABLE", StringComparison.OrdinalIgnoreCase);
                if (tblIdx < 0)
                    return null;

                string after = s.Substring(tblIdx + 5).Trim();
                if (after.StartsWith("IF", StringComparison.OrdinalIgnoreCase))
                    return null;

                // Skip optional dbo. / schema
                int paren = after.IndexOf('(');
                if (paren < 0)
                    return null;

                string name = after.Substring(0, paren).Trim();
                if (name.StartsWith("[") && name.EndsWith("]"))
                    name = name.Substring(1, name.Length - 2);
                if (name.Contains("."))
                    name = name.Substring(name.LastIndexOf('.') + 1).Trim('[', ']');

                return name;
            }
            catch
            {
                return null;
            }
        }

        private static bool TableExists(string tableName)
        {
            using (var db = new TrackerSQLDb())
            {
                object result = db.ExecuteScalar(
                    "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@TableName", DataValue = tableName, DataDbType = DbType.String }
                    });
                return result != null && result != DBNull.Value;
            }
        }

        private DataTable RunSelect(string sql)
        {
            using (var db = new TrackerSQLDb())
            {
                return db.ReturnDataTable(sql);
            }
        }

        private static int RunCommand(string sql, out string error)
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    int rows = db.ExecuteNonQuery(sql);
                    error = null;
                    return rows;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return -1;
            }
        }

        private sealed class SqlCommandResult
        {
            public string Type { get; set; }
            public string Sql { get; set; }
            public string Error { get; set; }
            public bool Succeeded { get; set; }

            public string SqlPreview
            {
                get
                {
                    string s = (Sql ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
                    while (s.Contains("  "))
                        s = s.Replace("  ", " ");
                    return s.Length <= 120 ? s : s.Substring(0, 117) + "...";
                }
            }
        }
    }
}
