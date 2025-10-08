using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Tools
{
    public partial class EmailDiagnostics : System.Web.UI.Page
    {


        public class ComboProgress
        {
            public int Total { get; set; }
            public int Completed { get; set; }
            public List<string> Results { get; set; } = new List<string>();
            public bool IsDone => Completed >= Total;
        }

        private static ConcurrentDictionary<string, ComboProgress> ProgressStore = new ConcurrentDictionary<string, ComboProgress>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var config = ConfigurationManager.AppSettings;

                txtHost.Text = config["EMailSMTP"] ?? "";
                txtPort.Text = config["EmailPort"] ?? "";
                txtUser.Text = config["EMailLogIn"] ?? "";
                txtPass.Text = config["EmailPassword"] ?? "";
                txtFrom.Text = config["SysEmailFrom"] ?? "";

                chkSSL.Checked = (config["EMailSSLEnabled"] ?? "false").ToLower() == "true";
                ddlSocketOption.SelectedValue = config["EmailSocketOption"] ?? "Auto";
                txtTimeout.Text = config["EmailTimeout"] ?? "10000";
            }

            // Retain password field value across postback
            txtPass.Attributes["value"] = txtPass.Text;
        }

        protected void chkShowPwd_CheckedChanged(object sender, EventArgs e)
        {
            txtPass.TextMode = chkShowPwd.Checked ? TextBoxMode.SingleLine : TextBoxMode.Password;
            txtPass.Attributes["value"] = txtPass.Text;
        }
        private EmailSettings GetEmailSettings()
        {
            return new EmailSettings(
                txtHost.Text.Trim(),
                int.TryParse(txtPort.Text, out var p) ? p : 25,
                txtUser.Text.Trim(),
                txtPass.Text,
                chkSSL.Checked,
                ddlSocketOption.SelectedValue,
                int.TryParse(txtTimeout.Text, out var t) ? t : 10000,
                txtFrom.Text.Trim(),
                txtTo.Text.Trim(),
                null
            );
        }
        protected void btnSend_Click(object sender, EventArgs e)
        {
            lblGlobalStatus.Text = "";
            lblResult.Text = "";
            upGlobal.Update(); // force the update panel to refresh early

            lblGlobalStatus.Text = "Sending test email... ⏳";

            var emailSettings = GetEmailSettings();
            var email = new EmailMailKitCls(emailSettings);
            //email.IsTestMode = true; // Set test mode to avoid sending real emails

            if (!email.SetEmailFromTo(txtFrom.Text, txtTo.Text))
            {
                lblResult.Text = "⚠️ Invalid email addresses.";
                return;
            }

            email.SetEmailSubject(txtSubject.Text);
            email.AddToBody(txtBody.Text);

            bool success = email.SendEmail(); // No parameters needed

            lblResult.Text = email.GetFormattedResultMessage(success);

            lblGlobalStatus.Text = "Test email completed. ✅ ";
        }
        void SetOrAdd(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (settings[key] == null)
                settings.Add(key, value);
            else
                settings[key].Value = value;
        }
        protected void btnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                var settingsObj = GetEmailSettings();
                var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(Request.ApplicationPath);
                var appSettings = config.AppSettings.Settings;

                SetOrAdd(appSettings, "EMailSMTP", settingsObj.SmtpHost);
                SetOrAdd(appSettings, "EmailPort", settingsObj.SmtpPort.ToString());
                SetOrAdd(appSettings, "EMailLogIn", settingsObj.SmtpUser);
                SetOrAdd(appSettings, "EmailPassword", settingsObj.SmtpPass);
                SetOrAdd(appSettings, "EMailSSLEnabled", settingsObj.EnableSSL.ToString().ToLower());
                SetOrAdd(appSettings, "EmailSocketOption", settingsObj.SocketOption);
                SetOrAdd(appSettings, "EmailTimeout", settingsObj.Timeout.ToString());
                SetOrAdd(appSettings, "SysEmailFrom", settingsObj.FromAddress);

                config.Save();
                lblResult.Text = "✅ Settings saved to web.config.";
            }
            catch (Exception ex)
            {
                lblResult.Text = "❌ Failed to save: " + ex.Message;
            }
        }
        protected void btnDiagnostics_Click(object sender, EventArgs e)
        {
            lblGlobalStatus.Text = "Running Diagnostics...";
            var emailSettings = GetEmailSettings();
            var email = new EmailMailKitCls(emailSettings);
            string result = email.GetServerDiagnostics();
            // parameters not needed as part of enmailsetytings

            lblDiagnostics.Text = result.Replace("\n", "<br />");
            lblGlobalStatus.Text = "Diagnostics Run.";
        }

        protected void btnViewLog_Click(object sender, EventArgs e)
        {
            lblGlobalStatus.Text = "View Log click...";
            string logPath = Server.MapPath("~/App_Data/smtp_diagnostics.log");

            if (!File.Exists(logPath))
            {
                litLogOutput.Text = "<div style='color:red;'>❌ Log file not found.</div>";
                return;
            }

            string[] lines = File.ReadAllLines(logPath);
            var html = new System.Text.StringBuilder();
            html.Append("<div style='font-family: monospace; max-height: 400px; overflow-y: auto; background: #f9f9f9; border: 1px solid #ccc; padding: 10px;'>");

            foreach (string line in lines)
            {
                html.Append(System.Web.HttpUtility.HtmlEncode(line) + "<br />");
            }

            html.Append("</div>");
            litLogOutput.Text = html.ToString();
            lblGlobalStatus.Text = "View Log click done...";
        }
        [WebMethod]
        [ScriptMethod]
        public static void StartComboTest(string key, string smtpHost, string smtpUser, string smtpPass, string from, string to)
        {
            var combos = new List<(int port, string option)>  {
                (25, "None"),
                (25, "StartTls"),
                (465, "SslOnConnect"),
                (465, "None"),
                (587, "StartTls"),
                (587, "None"),
                (587, "SslOnConnect")
            };

            var progress = new ComboProgress
            {
                Total = combos.Count,
                Completed = 0,
                Results = new List<string>()
            };

            ProgressStore[key] = progress;

            // Package static fields for reuse
            var staticSettings = new
            {
                smtpHost,
                smtpUser,
                smtpPass,
                from,
                to
            };

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    foreach (var combo in combos)
                    {
                        try
                        {
                            int port = combo.port;
                            string option = combo.option;

                            var comboSettings = new EmailSettings();
                            comboSettings.LoadFromParameters(
                                staticSettings.smtpHost,
                                port,
                                staticSettings.smtpUser,
                                staticSettings.smtpPass,
                                enableSSL: (option != "None"),
                                socketOption: option,
                                timeout: 15000,
                                staticSettings.from,
                                staticSettings.to,
                                null
                            );

                            var email = new EmailMailKitCls(comboSettings);
                            email.SetEmailSubject("Combo Test");
                            email.AddToBody("Testing configuration");

                            bool sent = email.SendEmail();

                            string resultLine = sent
                                ? $"<tr><td>{port}</td><td>{option}</td><td>✅ Success</td><td><button onclick=\"applyCombo('{port}', '{option}')\">Apply This</button></td></tr>"
                                : $"<tr><td>{port}</td><td>{option}</td><td>❌ Failed - {System.Web.HttpUtility.HtmlEncode(email.LastErrorSummary ?? "Unknown error")}</td><td>—</td></tr>";

                            progress.Results.Add(resultLine);
                        }
                        catch (Exception exCombo)
                        {
                            progress.Results.Add($"<tr><td colspan='4'>⚠️ Error for port {combo.port}, option {combo.option}: {System.Web.HttpUtility.HtmlEncode(exCombo.Message)}</td></tr>");
                        }

                        progress.Completed++;
                        System.Threading.Thread.Sleep(300);
                    }
                }
                catch (Exception exThread)
                {
                    string errorHtml = $"<tr><td colspan='4'>🚨 Fatal error: {System.Web.HttpUtility.HtmlEncode(exThread.Message)}</td></tr>";
                    progress.Results.Add(errorHtml);
                    progress.Completed = progress.Total;
                }
            });
        }

        [WebMethod]
        [ScriptMethod]
        public static object GetComboProgress(string key)
        {
            if (!ProgressStore.TryGetValue(key, out var progress))
                return new { percent = 0, completed = false, html = "", completedCount = 0, total = 0 };

            int percent = (int)((progress.Completed / (double)progress.Total) * 100);
            string html = "<table class='results-table' style='width:100%; border-collapse: collapse;'>"
                        + "<thead><tr><th>Port</th><th>Socket Option</th><th>Status</th><th>Apply</th></tr></thead><tbody>"
                        + string.Join("", progress.Results)
                        + "</tbody></table>";

            return new
            {
                percent,
                completed = progress.Completed >= progress.Total,
                html,
                completedCount = progress.Completed,
                total = progress.Total
            };
        }


        //protected void btnTestCombos_Click(object sender, EventArgs e)
        //{
        //    lblGlobalStatus.Text = "Testing combos - start...";

        //    var ports = new[] { 25, 465, 587 };
        //    var options = new[] { "None", "StartTls", "SslOnConnect" };
        //    var results = new List<Tuple<int, string, string>>();
        //    lblProgress.Text = "Testing combinations...";

        //    var emailSettings = GetEmailSettings();
        //    foreach (var port in ports)
        //    {
        //        foreach (var option in options)
        //        {
        //            var email = new EmailClsMailKit();
        //            email.SetEmailFromTo(txtFrom.Text, txtTo.Text);
        //            email.SetEmailSubject("Combo Test");
        //            email.AddToBody("Test config");

        //            bool sent = email.SendEmail(
        //                smtpHost: emailSettings.SmtpHost,
        //                smtpPort: port,
        //                smtpUser: emailSettings.SmtpUser,
        //                smtpPass: emailSettings.SmtpPass,
        //                enableSSL: (option != "None"),
        //                socketOption: option,
        //                timeout: 15000
        //            );

        //            string status = sent ? "✅ Success" : $"❌ Failed - {email.LastErrorSummary}";
        //            results.Add(Tuple.Create(port, option, status));
        //        }
        //    }
        //    lblNoResults.Text = results.Count == 0 ? "⚠️ No results to display." : "";
        //    phComboResults.Visible = results.Count > 0;
        //    var summary = string.Join(Environment.NewLine, results.Select(r => $"Port {r.Item1}, Option {r.Item2}: {r.Item3}"));
        //    File.AppendAllText(Server.MapPath("~/App_Data/smtp_combos.log"), $"[{TimeZoneUtils.Now()}]\r\n{summary}\r\n----------------------\r\n");


        //    BuildComboResultsTable(results);
        //    lblProgress.Text = $"✅ Completed. {results.Count} combinations tested.";
        //    lblGlobalStatus.Text = "Testing combos - done...";
        //    ScriptManager.RegisterStartupScript(this, GetType(), "hideSpinner", "document.getElementById('spinner').style.display='none';", true);

        //}

        //private void BuildComboResultsTable(List<Tuple<int, string, string>> combos)
        //{
        //    var table = new Table { CssClass = "results-table", Width = Unit.Percentage(100) };
        //    table.Rows.Add(new TableHeaderRow
        //    {
        //        Cells =
        //        {
        //            new TableHeaderCell { Text = "Port" },
        //            new TableHeaderCell { Text = "Socket Option" },
        //            new TableHeaderCell { Text = "Status" },
        //            new TableHeaderCell { Text = "Apply" }
        //        }
        //    });

        //    foreach (var combo in combos)
        //    {
        //        var row = new TableRow();

        //        row.Cells.Add(new TableCell { Text = combo.Item1.ToString() });
        //        row.Cells.Add(new TableCell { Text = combo.Item2 });
        //        row.Cells.Add(new TableCell { Text = combo.Item3 });

        //        var btn = new Button
        //        {
        //            Text = "Use This",
        //            CommandName = "ApplyCombo",
        //            CommandArgument = $"{combo.Item1}|{combo.Item2}"
        //        };
        //        btn.Command += ApplyCombo_Command;
        //        row.Cells.Add(new TableCell { Controls = { btn } });

        //        table.Rows.Add(row);
        //    }

        //    phComboResults.Controls.Clear();
        //    phComboResults.Controls.Add(table);
        //}

        private void ApplyCombo_Command(object sender, CommandEventArgs e)
        {
            var parts = ((string)e.CommandArgument).Split('|');

            txtPort.Text = parts[0];
            ddlSocketOption.SelectedValue = parts[1];
            chkSSL.Checked = (parts[1] != "None");
            lblResult.Text = $"✅ Applied Port {parts[0]} + {parts[1]}";
        }

        protected void btnClearCombosLog_Click(object sender, EventArgs e)
        {
            string path = Server.MapPath("~/App_Data/smtp_combos.log");
            try
            {
                if (File.Exists(path))
                {
                    File.WriteAllText(path, "");
                    lblClearLogStatus.Text = "✅ Log file cleared.";
                }
                else
                {
                    lblClearLogStatus.Text = "⚠️ Log file not found.";
                }
            }
            catch (Exception ex)
            {
                lblClearLogStatus.Text = "❌ Error clearing log: " + ex.Message;
            }
        }
    }
}
