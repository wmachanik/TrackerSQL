// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.test.XMLtoSQL
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using TrackerDotNet.Classes;

namespace TrackerDotNet.test
{
    public partial class XMLtoSQL : Page
    {
        private const string CONST_DEFAULT_PREFIX = "SQLCommands";
        protected TextBox FileNameTextBox;
        protected Button GoButton;
        protected GridView gvSQLResults;
        protected Panel pnlSQLResults;

        protected Literal ltrlFileList;
        protected Panel pnlFileBrowser;
        protected Button RefreshFilesButton;

        private class SqlCommandResult
        {
            public string Type { get; set; }
            public string Sql { get; set; }
            public string Error { get; set; }
            public string Result { get; set; }
        }
        private void SetDefaultFileName()
        {
            string folderPath = Server.MapPath("~/App_Data/");
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    this.FileNameTextBox.Text = $"App_Data directory not found: {folderPath}";
                    return;
                }

                FileInfo[] files = new DirectoryInfo(folderPath).GetFiles("SQLCommands*.xml");
                int maxSuffix = -1;
                string selectedFile = "";

                foreach (FileInfo file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file.Name);
                    if (fileName.StartsWith("SQLCommands"))
                    {
                        string numberPart = fileName.Substring("SQLCommands".Length);
                        if (string.IsNullOrEmpty(numberPart))
                        {
                            if (maxSuffix < 0)
                            {
                                maxSuffix = 0;
                                selectedFile = file.FullName;
                            }
                        }
                        else if (int.TryParse(numberPart, out int suffix))
                        {
                            if (suffix > maxSuffix)
                            {
                                maxSuffix = suffix;
                                selectedFile = file.FullName;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(selectedFile))
                {
                    this.FileNameTextBox.Text = selectedFile;
                }
                else
                {
                    this.FileNameTextBox.Text = Path.Combine(folderPath, "SQLCommands001.xml");
                }

                // Load file browser
                LoadFileBrowser();
            }
            catch (Exception ex)
            {
                this.FileNameTextBox.Text = $"Error: {ex.Message}";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;
            this.SetDefaultFileName();
        }

        private void showMsgBox(string pTitle, string pMessage)
        {
            string script = $"showAppMessage('{pMessage.Replace("'", "\\'")}');";
            System.Web.UI.ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), pTitle, script, true);
        }

        private string StripInlineComments(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return sql;
            var lines = sql.Replace("\r", "").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int idx = lines[i].IndexOf("--", StringComparison.Ordinal);
                if (idx >= 0)
                    lines[i] = lines[i].Substring(0, idx);
            }
            return string.Join(" ", lines).Trim();
        }
        private string ExtractCreatedTableName(string sql)
        {
            try
            {
                // Expect form: CREATE TABLE <name> (
                string s = sql.Trim();
                int tblIdx = s.IndexOf("TABLE", StringComparison.OrdinalIgnoreCase);
                if (tblIdx < 0) return null;
                string after = s.Substring(tblIdx + 5).Trim();
                int paren = after.IndexOf('(');
                if (paren < 0) return null;
                string name = after.Substring(0, paren).Trim();
                if (name.StartsWith("[") && name.EndsWith("]"))
                    name = name.Substring(1, name.Length - 2);
                return name;
            }
            catch { return null; }
        }
        protected void GoButton_Click(object sender, EventArgs e)
        {
            List<XMLtoSQL.SQLCommand> sqlCommandList = new List<XMLtoSQL.SQLCommand>();

            // Enhanced file validation
            string filePath = this.FileNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(filePath))
            {
                showMsgBox("Error", "Please specify a file path.");
                return;
            }

            if (!File.Exists(filePath))
            {
                string appDataPath = Server.MapPath("~/App_Data/");
                string errorMsg = $"File not found: {filePath}\n\nApp_Data folder: {appDataPath}";

                // List available XML files
                try
                {
                    var xmlFiles = new DirectoryInfo(appDataPath).GetFiles("*.xml");
                    if (xmlFiles.Length > 0)
                    {
                        errorMsg += "\n\nAvailable XML files:";
                        foreach (var file in xmlFiles)
                        {
                            errorMsg += $"\n- {file.Name}";
                        }
                    }
                    else
                    {
                        errorMsg += "\n\nNo XML files found in App_Data directory.";
                    }
                }
                catch (Exception dirEx)
                {
                    errorMsg += $"\n\nError reading App_Data directory: {dirEx.Message}";
                }

                showMsgBox("File Not Found", errorMsg);
                AppLogger.WriteLog("xmltosql", $"File not found: {filePath}");
                return;
            }

            XmlReader xmlReader = null;
            try
            {
                AppLogger.WriteLog("xmltosql", $"Starting XML processing: {filePath}");

                xmlReader = XmlReader.Create(filePath);
                int commandCount = 0;

                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "command")
                    {
                        XMLtoSQL.SQLCommand sqlCommand = new XMLtoSQL.SQLCommand();
                        sqlCommand.type = xmlReader.GetAttribute("type");

                        if (string.IsNullOrEmpty(sqlCommand.type))
                        {
                            sqlCommand.type = "unknown";
                            sqlCommand.errString = "Missing type attribute";
                            sqlCommand.result = false;
                        }

                        xmlReader.Read();
                        sqlCommand.sql = xmlReader.Value?.Replace("\n", "").Trim() ?? "";

                        if (string.IsNullOrEmpty(sqlCommand.sql))
                        {
                            sqlCommand.errString = "Empty SQL command";
                            sqlCommand.result = false;
                        }

                        sqlCommandList.Add(sqlCommand);
                        commandCount++;

                        AppLogger.WriteLog("xmltosql", $"Command {commandCount}: type='{sqlCommand.type}', sql='{sqlCommand.sql.Substring(0, Math.Min(50, sqlCommand.sql.Length))}...'");
                    }
                }
                xmlReader.Close();
                xmlReader = null;

                AppLogger.WriteLog("xmltosql", $"Found {commandCount} commands to execute");

                // Execute commands
                for (int index = 0; index < sqlCommandList.Count; ++index)
                {
                    var cmd = sqlCommandList[index];
                    AppLogger.WriteLog("xmltosql", $"Executing command {index + 1}: {cmd.type}");

                    try
                    {
                        if (cmd.type == "select")
                        {
                            GridView child = new GridView();
                            child.CssClass = "table table-striped";
                            child.HeaderStyle.BackColor = System.Drawing.Color.LightBlue;

                            DataSet dataSet = this.RunSelect(cmd.sql);
                            cmd.result = dataSet != null && dataSet.Tables.Count > 0;

                            if (cmd.result)
                            {
                                child.DataSource = dataSet;
                                child.DataBind();

                                // Add a label before each grid
                                Label lblCommand = new Label();
                                lblCommand.Text = $"<h4>SELECT Result {index + 1}:</h4><pre>{cmd.sql}</pre>";
                                lblCommand.Text += $"<p><em>Rows returned: {dataSet.Tables[0].Rows.Count}</em></p>";
                                this.pnlSQLResults.Controls.Add(lblCommand);
                                this.pnlSQLResults.Controls.Add(child);
                                this.pnlSQLResults.Controls.Add(new Literal { Text = "<br/><hr/><br/>" });

                                AppLogger.WriteLog("xmltosql", $"SELECT command {index + 1} returned {dataSet.Tables[0].Rows.Count} rows");
                            }
                            else
                            {
                                cmd.errString = "No data returned or query failed";
                                AppLogger.WriteLog("xmltosql", $"SELECT command {index + 1} failed or returned no data");
                            }
                        }
                        else if (cmd.type == "disabled")
                        {
                            cmd.result = true;
                            cmd.errString = "Skipped (disabled)";
                            AppLogger.WriteLog("xmltosql", $"Command {index + 1} skipped (disabled)");
                        }
                        else if (cmd.type == "update" || cmd.type == "insert" || cmd.type == "delete" || cmd.type == "create" || cmd.type == "alter" || cmd.type == "drop")
                        {
                            // Strip inline '--' comments for Access DDL safety
                            if (cmd.type == "create" || cmd.type == "alter")
                                cmd.sql = StripInlineComments(cmd.sql);

                            cmd.errString = this.RunCommand(cmd.sql);
                            cmd.result = string.IsNullOrWhiteSpace(cmd.errString);

                            // Post‑verification for CREATE TABLE
                            if (cmd.result && cmd.type == "create" && cmd.sql.Trim().ToLower().StartsWith("create table"))
                            {
                                string created = ExtractCreatedTableName(cmd.sql);
                                if (!string.IsNullOrEmpty(created))
                                {
                                    using (var verify = new TrackerDb())
                                    {
                                        if (!verify.TableExists(created))
                                        {
                                            cmd.errString = "CREATE reported success but table not found: " + created;
                                            cmd.result = false;
                                        }
                                    }
                                }
                            }

                            if (cmd.result)
                                AppLogger.WriteLog("xmltosql", $"{cmd.type.ToUpper()} command {index + 1} executed successfully");
                            else
                                AppLogger.WriteLog("xmltosql", $"{cmd.type.ToUpper()} command {index + 1} failed: {cmd.errString}");
                        }
                        else
                        {
                            cmd.errString = $"Unknown command type: {cmd.type}";
                            cmd.result = false;
                            AppLogger.WriteLog("xmltosql", $"Unknown command type {index + 1}: {cmd.type}");
                        }
                    }
                    catch (Exception cmdEx)
                    {
                        cmd.errString = $"Exception: {cmdEx.Message}";
                        cmd.result = false;
                        AppLogger.WriteLog("xmltosql", $"Exception in command {index + 1}: {cmdEx.Message}");
                    }

                    // Check for TrackerDb errors
                    TrackerTools trackerTools = new TrackerTools();
                    string sessionErrorString = trackerTools.GetTrackerSessionErrorString();
                    if (!string.IsNullOrEmpty(sessionErrorString))
                    {
                        showMsgBox("Database Error", sessionErrorString);
                        AppLogger.WriteLog("xmltosql", $"TrackerDb error: {sessionErrorString}");
                        trackerTools.SetTrackerSessionErrorString("");
                    }
                }

                // Show command summary
                this.gvSQLResults.DataSource = sqlCommandList;
                this.gvSQLResults.DataBind();

                int successCount = 0;
                int failureCount = 0;
                foreach (var cmd in sqlCommandList)
                {
                    if (cmd.result) successCount++; else failureCount++;
                }

                string summaryMsg = $"Execution completed!\n\nTotal commands: {sqlCommandList.Count}\nSuccessful: {successCount}\nFailed: {failureCount}";
                showMsgBox("Execution Summary", summaryMsg);
                AppLogger.WriteLog("xmltosql", summaryMsg.Replace("\n", " "));
            }
            catch (Exception ex)
            {
                string errorMsg = $"XML processing error: {ex.Message}";
                showMsgBox("Error", errorMsg);
                AppLogger.WriteLog("xmltosql", $"XML processing error: {ex.Message}");
            }
            finally
            {
                xmlReader?.Close();
            }
        }

        protected void RefreshFilesButton_Click(object sender, EventArgs e)
        {
            LoadFileBrowser();
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

                var allFiles = new DirectoryInfo(folderPath).GetFiles("*.xml");
                if (allFiles.Length == 0)
                {
                    ltrlFileList.Text = "<em>No XML files found</em>";
                    return;
                }

                var html = new System.Text.StringBuilder();

                // Group files by type
                var sqlCommandFiles = new List<FileInfo>();
                var otherFiles = new List<FileInfo>();

                foreach (var file in allFiles)
                {
                    if (file.Name.StartsWith("SQLCommands"))
                        sqlCommandFiles.Add(file);
                    else
                        otherFiles.Add(file);
                }

                // SQL Command files first
                if (sqlCommandFiles.Count > 0)
                {
                    html.AppendLine("<strong>Migration Files:</strong><br/>");
                    Array.Sort(sqlCommandFiles.ToArray(), (f1, f2) => string.Compare(f1.Name, f2.Name));

                    foreach (var file in sqlCommandFiles)
                    {
                        html.AppendLine($"<div class='file-item xml-file' onclick=\"selectFile('{file.FullName.Replace("\\", "\\\\")}')\">");
                        html.AppendLine($"📄 {file.Name} <small>({file.LastWriteTime:yyyy-MM-dd HH:mm}, {file.Length} bytes)</small>");
                        html.AppendLine("</div>");
                    }
                    html.AppendLine("<br/>");
                }

                // Other XML files
                if (otherFiles.Count > 0)
                {
                    html.AppendLine("<strong>Other XML Files:</strong><br/>");
                    Array.Sort(otherFiles.ToArray(), (f1, f2) => string.Compare(f1.Name, f2.Name));

                    foreach (var file in otherFiles)
                    {
                        html.AppendLine($"<div class='file-item' onclick=\"selectFile('{file.FullName.Replace("\\", "\\\\")}')\">");
                        html.AppendLine($"📄 {file.Name} <small>({file.LastWriteTime:yyyy-MM-dd HH:mm})</small>");
                        html.AppendLine("</div>");
                    }
                }

                ltrlFileList.Text = html.ToString();
            }
            catch (Exception ex)
            {
                ltrlFileList.Text = $"<em>Error loading files: {ex.Message}</em>";
            }
        }

        private DataSet RunSelect(string pSQL) => new TrackerDb().ReturnDataSet(pSQL);

        private string RunCommand(string pSQL)
        {
            var db = new TrackerDb();
            string err = db.ExecuteNonQuerySQL(pSQL);
            if (string.IsNullOrEmpty(err) && !string.IsNullOrEmpty(db.ErrorResult))
                err = db.ErrorResult;
            db.Close();
            return err;
        }

        private class SQLCommand
        {
            private string _type;
            private string _sql;
            private string _errString;
            private bool _result;

            public SQLCommand()
            {
                this._type = "";
                this._sql = "";
                this._errString = "";
                this._result = false;
            }

            public string type
            {
                get => this._type;
                set => this._type = value;
            }

            public string sql
            {
                get => this._sql;
                set => this._sql = value;
            }

            public string errString
            {
                get => this._errString;
                set => this._errString = value;
            }

            public bool result
            {
                get => this._result;
                set => this._result = value;
            }
        }
    }
}