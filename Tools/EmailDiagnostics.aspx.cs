using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;

namespace TrackerSQL.Tools
{
    public partial class EmailDiagnostics : System.Web.UI.Page
    {
        private const string ComboViewStateKey = "ComboTestState";

        private static readonly (int port, string option)[] ComboDefinitions =
        {
            (587, "StartTls"),
            (587, "None"),
            (587, "SslOnConnect"),
            (465, "SslOnConnect"),
            (465, "None"),
            (25, "StartTls"),
            (25, "None")
        };

        [Serializable]
        private sealed class ComboTestState
        {
            public int Index;
            public List<string> Results = new List<string>();
            public string SmtpHost;
            public string SmtpUser;
            public string SmtpPass;
            public string From;
            public string To;
        }

        private ComboTestState ComboState
        {
            get => ViewState[ComboViewStateKey] as ComboTestState;
            set => ViewState[ComboViewStateKey] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var config = ConfigurationManager.AppSettings;

                txtHost.Text = config["EMailSMTP"] ?? "";
                txtPort.Text = config["EMailPort"] ?? "";                // fixed key (was "EmailPort")
                txtUser.Text = config["EMailLogIn"] ?? "";
                txtPass.Text = config["EMailPassword"] ?? "";            // fixed key (was "EmailPassword")
                txtFrom.Text = config["SysEmailFrom"] ?? "";

                // Test recipient / CC defaults
                txtTo.Text = config["EmailTestRecipient"] ?? config["SysEmailFrom"] ?? "";
                // If the CC box is present on the page, populate it from config
                if (this.FindControl("txtCc") != null)
                {
                    txtCc.Text = config["SysCCEmailAddress"] ?? "";
                }

                // Test recipient / CC defaults
                txtTo.Text = config["EmailTestRecipient"] ?? config["SysEmailFrom"] ?? "";
                // If the CC box is present on the page, populate it from config
                if (this.FindControl("txtCc") != null)
                {
                    txtCc.Text = config["SysCCEmailAddress"] ?? "";
                }

                chkSSL.Checked = (config["EMailSSLEnabled"] ?? "false").ToLower() == "true";
                ddlSocketOption.SelectedValue = config["EmailSocketOption"] ?? "Auto";
                txtTimeout.Text = config["EmailTimeout"] ?? "10000";

                tmrComboProgress.Enabled = false;
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
            // Read UI fields, with sensible fallbacks to appSettings
            var config = ConfigurationManager.AppSettings;
            string defaultTo = config["EmailTestRecipient"] ?? config["SysEmailFrom"] ?? "";
            string defaultCc = config["SysCCEmailAddress"] ?? "";

            string smtpHost = txtHost.Text.Trim();
            int smtpPort = int.TryParse(txtPort.Text, out var p) ? p : ConfigHelper.GetInt("EMailPort", 25);
            string smtpUser = txtUser.Text.Trim();
            string smtpPass = txtPass.Text;
            bool enableSsl = chkSSL.Checked;
            string socketOption = ddlSocketOption.SelectedValue;
            int timeout = int.TryParse(txtTimeout.Text, out var t) ? t : 10000;
            string from = txtFrom.Text.Trim();
            string to = string.IsNullOrWhiteSpace(txtTo.Text) ? defaultTo : txtTo.Text.Trim();
            string cc = this.FindControl("txtCc") != null && !string.IsNullOrWhiteSpace(txtCc.Text)
                ? txtCc.Text.Trim()
                : defaultCc;

            return new EmailSettings(
                smtpHost,
                smtpPort,
                smtpUser,
                smtpPass,
                enableSsl,
                socketOption,
                timeout,
                from,
                to,
                cc
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

        protected void btnTestCombos_Click(object sender, EventArgs e)
        {
            string smtpHost = GetFieldOrConfig(txtHost.Text, "EMailSMTP");
            string smtpUser = GetFieldOrConfig(txtUser.Text, "EMailLogIn");
            string smtpPass = string.IsNullOrEmpty(txtPass.Text)
                ? (ConfigurationManager.AppSettings["EmailPassword"] ?? string.Empty)
                : txtPass.Text;

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser))
            {
                lblProgress.Text = "Enter SMTP host and username on the Test Email tab first.";
                return;
            }

            tmrComboProgress.Enabled = false;
            ComboState = new ComboTestState
            {
                Index = 0,
                Results = new List<string>(),
                SmtpHost = smtpHost.Trim(),
                SmtpUser = smtpUser.Trim(),
                SmtpPass = smtpPass,
                From = GetFieldOrConfig(txtFrom.Text, "SysEmailFrom"),
                To = GetFieldOrConfig(txtTo.Text, "EmailTestRecipient")
            };

            lblProgress.Text = "Starting tests...";
            litComboResults.Text = string.Empty;
            btnTestCombos.Enabled = false;

            ProcessNextCombo();
        }

        protected void tmrComboProgress_Tick(object sender, EventArgs e)
        {
            ProcessNextCombo();
        }

        private void ProcessNextCombo()
        {
            var state = ComboState;
            if (state == null)
            {
                lblProgress.Text = "No combo test in progress. Click Test All Combos.";
                tmrComboProgress.Enabled = false;
                btnTestCombos.Enabled = true;
                return;
            }

            if (state.Index >= ComboDefinitions.Length)
            {
                FinishComboTest(state);
                return;
            }

            var combo = ComboDefinitions[state.Index];
            state.Results.Add(RunSingleComboTest(state, combo.port, combo.option));
            state.Index++;
            ComboState = state;

            int total = ComboDefinitions.Length;
            int percent = (int)((state.Index / (double)total) * 100);
            lblProgress.Text = $"Progress: {percent}% ({state.Index}/{total})";
            litComboResults.Text = BuildComboTableHtml(state.Results);

            if (state.Index >= total)
                FinishComboTest(state);
            else
                tmrComboProgress.Enabled = true;
        }

        private void FinishComboTest(ComboTestState state)
        {
            tmrComboProgress.Enabled = false;
            btnTestCombos.Enabled = true;
            lblProgress.Text = $"Completed: {ComboDefinitions.Length}/{ComboDefinitions.Length} combinations tested.";
            litComboResults.Text = BuildComboTableHtml(state.Results);
            ComboState = null;
        }

        private static string RunSingleComboTest(ComboTestState state, int port, string option)
        {
            try
            {
                var comboSettings = new EmailSettings(
                    state.SmtpHost,
                    port,
                    state.SmtpUser,
                    state.SmtpPass ?? string.Empty,
                    enableSSL: (option != "None"),
                    socketOption: option,
                    timeout: 12000,
                    state.From ?? string.Empty,
                    state.To ?? string.Empty,
                    null);

                var email = new EmailMailKitCls(comboSettings);
                bool connected = email.TryConnectAndAuthenticate(out string errorMessage);

                return connected
                    ? $"<tr><td>{port}</td><td>{HttpUtility.HtmlEncode(option)}</td><td>OK Connect + Auth</td><td><button type='button' onclick=\"applyCombo('{port}', '{option}')\">Apply This</button></td></tr>"
                    : $"<tr><td>{port}</td><td>{HttpUtility.HtmlEncode(option)}</td><td>Failed - {HttpUtility.HtmlEncode(errorMessage ?? "Unknown error")}</td><td>&mdash;</td></tr>";
            }
            catch (Exception ex)
            {
                return $"<tr><td colspan='4'>Error for port {port}, option {HttpUtility.HtmlEncode(option)}: {HttpUtility.HtmlEncode(ex.Message)}</td></tr>";
            }
        }

        private static string GetFieldOrConfig(string fieldValue, string configKey)
        {
            if (!string.IsNullOrWhiteSpace(fieldValue))
                return fieldValue.Trim();

            return ConfigurationManager.AppSettings[configKey] ?? string.Empty;
        }

        private static string BuildComboTableHtml(IEnumerable<string> rows)
        {
            return "<table class='results-table' style='width:100%; border-collapse: collapse;'>"
                 + "<thead><tr><th>Port</th><th>Socket Option</th><th>Status</th><th>Apply</th></tr></thead><tbody>"
                 + string.Join(string.Empty, rows)
                 + "</tbody></table>";
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
        protected void btnSendCcTest_Click(object sender, EventArgs e)
        {
            lblCcResult.Text = "";
            lblGlobalStatus.Text = "Sending CC test...";

            string smtpHost = txtHost.Text.Trim();
            int smtpPort = int.TryParse(txtPort.Text, out var p) ? p : ConfigHelper.GetInt("EMailPort", 587);
            string smtpUser = txtUser.Text.Trim();
            string smtpPass = txtPass.Text;
            string socketOption = ddlSocketOption.SelectedValue ?? "Auto";
            int timeout = int.TryParse(txtTimeout.Text, out var t) ? t : 10000;

            string from = string.IsNullOrWhiteSpace(txtFrom.Text) ? smtpUser : txtFrom.Text.Trim();
            string to = txtTo.Text.Trim();
            string ccRaw = txtCc.Text.Trim();
            string subject = txtSubject.Text;
            string body = txtBody.Text;

            try
            {
                var msg = new MimeMessage();

                // Use the configured From (or smtp user as fallback)
                msg.From.Add(MailboxAddress.Parse(from));

                // Add To
                msg.To.Add(MailboxAddress.Parse(to));

                // Add Cc addresses (support comma or semicolon separated)
                if (!string.IsNullOrWhiteSpace(ccRaw))
                {
                    var ccList = ccRaw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(a => a.Trim())
                                      .Where(a => !string.IsNullOrEmpty(a));
                    foreach (var cc in ccList)
                    {
                        try
                        {
                            msg.Cc.Add(MailboxAddress.Parse(cc));
                        }
                        catch (Exception exCc)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics: Invalid CC address skipped: {cc} - {exCc.Message}");
                        }
                    }
                }

                msg.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                msg.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Timeout = timeout;
                    client.CheckCertificateRevocation = false;
                    client.ServerCertificateValidationCallback = (s, c, h, eArgs) => true;

                    SecureSocketOptions option = SecureSocketOptions.Auto;
                    switch (socketOption)
                    {
                        case "None": option = SecureSocketOptions.None; break;
                        case "SslOnConnect": option = SecureSocketOptions.SslOnConnect; break;
                        case "StartTls": option = SecureSocketOptions.StartTls; break;
                        case "StartTlsWhenAvailable": option = SecureSocketOptions.StartTlsWhenAvailable; break;
                    }

                    client.Connect(smtpHost, smtpPort, option);

                    // If SMTP user is provided, authenticate
                    if (!string.IsNullOrEmpty(smtpUser))
                        client.Authenticate(smtpUser, smtpPass);

                    AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics: Sending CC test. Host={smtpHost}:{smtpPort} From={from} To={to} Cc={ccRaw}");

                    client.Send(msg);
                    client.Disconnect(true);
                }

                lblCcResult.ForeColor = System.Drawing.Color.Green;
                lblCcResult.Text = "✅ CC test sent — check To and CC inboxes (and spam/quarantine).";
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics: CC test sent successfully. To={to} Cc={ccRaw}");
            }
            catch (Exception ex)
            {
                lblCcResult.ForeColor = System.Drawing.Color.Red;
                lblCcResult.Text = "❌ Send failed: " + ex.GetType().Name + ": " + ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics: CC test failed: {ex}");
            }
            finally
            {
                lblGlobalStatus.Text = "CC test completed.";
            }
        }
        /// <summary>
        /// Send a test using the EmailMailKitCls wrapper and return a short result string.
        /// </summary>
        private string SendUsingWrapper()
        {
            try
            {
                var settings = GetEmailSettings();
                var email = new EmailMailKitCls(settings);

                // Ensure wrapper uses UI From/To/CC
                string from = string.IsNullOrWhiteSpace(txtFrom.Text) ? settings.FromAddress : txtFrom.Text.Trim();
                string to = string.IsNullOrWhiteSpace(txtTo.Text) ? settings.ToAddress : txtTo.Text.Trim();

                // Update settings in the wrapper (SetEmailFromTo will update emailConfig internal values)
                email.SetEmailFromTo(from, to);

                // Add subject/body
                email.SetEmailSubject(string.IsNullOrWhiteSpace(txtSubject.Text) ? "Wrapper Test" : txtSubject.Text);
                email.AddToBody(string.IsNullOrWhiteSpace(txtBody.Text) ? "Wrapper test body" : txtBody.Text);

                // Log exact recipients the wrapper will attempt to send to (includes CC from settings)
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics (Wrapper): From={from} To={to} Cc={settings.CcAddress}");

                bool ok = email.SendEmail();
                string formatted = email.GetFormattedResultMessage(ok);

                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics (Wrapper): Result: {formatted}");
                return $"Wrapper: {formatted}";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics (Wrapper): Exception: {ex}");
                return $"Wrapper: ❌ Exception: {ex.GetType().Name}: {ex.Message}";
            }
        }
        /// <summary>
        /// Send a test using MailKit SmtpClient directly (To + optional Cc) and return a short result string.
        /// </summary>
        private string SendUsingSmtpDirect()
        {
            string smtpHost = txtHost.Text.Trim();
            int smtpPort = int.TryParse(txtPort.Text, out var p) ? p : ConfigHelper.GetInt("EMailPort", 587);
            string smtpUser = txtUser.Text.Trim();
            string smtpPass = txtPass.Text;
            string socketOption = ddlSocketOption.SelectedValue ?? "Auto";
            int timeout = int.TryParse(txtTimeout.Text, out var t) ? t : 10000;

            string from = string.IsNullOrWhiteSpace(txtFrom.Text) ? smtpUser : txtFrom.Text.Trim();
            string to = txtTo.Text.Trim();
            string ccRaw = txtCc?.Text?.Trim() ?? "";
            string subject = string.IsNullOrWhiteSpace(txtSubject.Text) ? "Direct SMTP Test" : txtSubject.Text;
            string body = string.IsNullOrWhiteSpace(txtBody.Text) ? "Direct SMTP test body" : txtBody.Text;

            try
            {
                var msg = new MimeMessage();
                msg.From.Add(MailboxAddress.Parse(from));
                msg.To.Add(MailboxAddress.Parse(to));

                if (!string.IsNullOrWhiteSpace(ccRaw))
                {
                    var ccList = ccRaw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(a => a.Trim())
                                      .Where(a => !string.IsNullOrEmpty(a));

                    foreach (var cc in ccList)
                    {
                        try { msg.Cc.Add(MailboxAddress.Parse(cc)); }
                        catch (Exception exCc)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics: Invalid CC skipped: {cc} - {exCc.Message}");
                        }
                    }
                }

                msg.Subject = subject;
                msg.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Timeout = timeout;
                    client.CheckCertificateRevocation = false;
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    SecureSocketOptions option = SecureSocketOptions.Auto;
                    switch (socketOption)
                    {
                        case "None": option = SecureSocketOptions.None; break;
                        case "SslOnConnect": option = SecureSocketOptions.SslOnConnect; break;
                        case "StartTls": option = SecureSocketOptions.StartTls; break;
                        case "StartTlsWhenAvailable": option = SecureSocketOptions.StartTlsWhenAvailable; break;
                    }

                    client.Connect(smtpHost, smtpPort, option);

                    if (!string.IsNullOrEmpty(smtpUser))
                        client.Authenticate(smtpUser, smtpPass);

                    AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics: Direct SMTP send. Host={smtpHost}:{smtpPort} From={from} To={to} Cc={ccRaw}");

                    client.Send(msg);
                    client.Disconnect(true);
                }

                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics: Direct SMTP send succeeded. To={to} Cc={ccRaw}");
                return $"Direct SMTP: ✅ Sent to {to}" + (string.IsNullOrWhiteSpace(ccRaw) ? "" : $" (Cc: {ccRaw})");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"EmailDiagnostics: Direct SMTP send failed: {ex}");
                return $"Direct SMTP: ❌ {ex.GetType().Name}: {ex.Message}";
            }
        }

        /// <summary>
        /// New handler: run both tests (wrapper and direct SMTP) and show combined result.
        /// Add a button to the .aspx with OnClick="btnSendBothTest_Click" to invoke this.
        /// </summary>
        protected void btnSendBothTest_Click(object sender, EventArgs e)
        {
            lblGlobalStatus.Text = "Running both wrapper and direct SMTP tests...";

            string wrapperResult = SendUsingWrapper();
            string smtpResult = SendUsingSmtpDirect();

            // Build a readable combined result
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(wrapperResult.Replace("\n", "<br/>"));
            sb.AppendLine("<br/>");
            sb.AppendLine(smtpResult.Replace("\n", "<br/>"));

            // Show in UI (use lblCcResult or lblResult as appropriate)
            if (lblCcResult != null)
            {
                lblCcResult.ForeColor = System.Drawing.Color.Black;
                lblCcResult.Text = sb.ToString();
            }
            else
            {
                lblResult.ForeColor = System.Drawing.Color.Black;
                lblResult.Text = sb.ToString();
            }

            lblGlobalStatus.Text = "Both tests completed.";
        }
    }
}
