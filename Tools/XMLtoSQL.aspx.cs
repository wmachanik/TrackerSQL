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
using TrackerSQL.Classes;

namespace TrackerSQL.Tools
{
    public partial class XMLtoSQL : Page
    {
        private const string DefaultReturnUrl = "~/Tools/SystemTools.aspx";
        private const string LogName = "xmltosql";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;

            SetDefaultFileName();
            SetStatus("Select an XML command file, then click Execute. Prefer SQLCommands_Test_SQLServer.xml for a safe smoke test.", isError: null);
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
            string folderPath = Server.MapPath("~/App_Data/");
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    FileNameTextBox.Text = string.Empty;
                    SetStatus("App_Data directory not found: " + folderPath, isError: true);
                    return;
                }

                // Prefer the SQL Server smoke-test file when present.
                string testFile = Path.Combine(folderPath, "SQLCommands_Test_SQLServer.xml");
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
                        : Path.Combine(folderPath, "SQLCommands_Test_SQLServer.xml");
                }

                LoadFileBrowser();
            }
            catch (Exception ex)
            {
                SetStatus("Error locating XML files: " + ex.Message, isError: true);
                AppLogger.WriteLog(LogName, "SetDefaultFileName: " + ex.Message);
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(DefaultReturnUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void RefreshFilesButton_Click(object sender, EventArgs e)
        {
            LoadFileBrowser();
            SetStatus("File list refreshed.", isError: null);
            upnlXmlToSql.Update();
        }

        protected void GoButton_Click(object sender, EventArgs e)
        {
            pnlSQLResults.Controls.Clear();
            gvSQLResults.DataSource = null;
            gvSQLResults.DataBind();

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

            upnlXmlToSql.Update();
        }

        private void ExecuteCommand(SqlCommandResult cmd, int ordinal)
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
                    return;
                }

                var title = new Literal
                {
                    Text = "<h4>SELECT result " + ordinal + "</h4>"
                        + "<pre style='white-space:pre-wrap;'>" + Server.HtmlEncode(cmd.Sql) + "</pre>"
                        + "<p><em>Rows: " + table.Rows.Count + "</em></p>"
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
                return;
            }

            if (type == "disabled")
            {
                cmd.Succeeded = true;
                cmd.Error = "Skipped (disabled)";
                return;
            }

            if (type == "update" || type == "insert" || type == "delete"
                || type == "create" || type == "alter" || type == "drop"
                || type == "exec" || type == "execute")
            {
                if (type == "create" || type == "alter")
                    cmd.Sql = StripInlineComments(cmd.Sql);

                string err = RunCommand(cmd.Sql);
                cmd.Succeeded = string.IsNullOrWhiteSpace(err);
                cmd.Error = err;

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

                return;
            }

            cmd.Succeeded = false;
            cmd.Error = "Unknown command type: " + cmd.Type;
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

        private void LoadFileBrowser()
        {
            string folderPath = Server.MapPath("~/App_Data/");
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    ltrlFileList.Text = "<em>App_Data directory not found</em>";
                    return;
                }

                FileInfo[] allFiles = new DirectoryInfo(folderPath).GetFiles("*.xml");
                if (allFiles.Length == 0)
                {
                    ltrlFileList.Text = "<em>No XML files found</em>";
                    return;
                }

                var sqlCommandFiles = allFiles
                    .Where(f => f.Name.StartsWith("SQLCommands", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var otherFiles = allFiles
                    .Where(f => !f.Name.StartsWith("SQLCommands", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var html = new StringBuilder();
                AppendFileGroup(html, "Migration / command files", sqlCommandFiles, highlight: true);
                AppendFileGroup(html, "Other XML files", otherFiles, highlight: false);
                ltrlFileList.Text = html.ToString();
            }
            catch (Exception ex)
            {
                ltrlFileList.Text = "<em>Error loading files: " + Server.HtmlEncode(ex.Message) + "</em>";
            }
        }

        private static void AppendFileGroup(StringBuilder html, string title, List<FileInfo> files, bool highlight)
        {
            if (files == null || files.Count == 0)
                return;

            html.Append("<strong>").Append(title).Append(":</strong><br/>");
            string css = highlight ? "file-item xml-file" : "file-item";

            foreach (FileInfo file in files)
            {
                string jsPath = file.FullName.Replace("\\", "\\\\").Replace("'", "\\'");
                html.Append("<div class='").Append(css).Append("' onclick=\"selectFile('")
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

        private static string RunCommand(string sql)
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    db.ExecuteNonQuery(sql);
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
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
