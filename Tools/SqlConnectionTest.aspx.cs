//------------------------------------------------------------------------------
// TrackerSQL — SQL connection timing / diagnostics (anonymous-friendly).
//------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TrackerSQL.Tools
{
    public partial class SqlConnectionTest : Page
    {
        private const string ConnectionStringName = "TrackerDataSQL";
        private const string LastGoodConnectionViewStateKey = "SqlConnectionTest.LastGoodConnection";
        private const string RealConnectionViewStateKey = "SqlConnectionTest.RealConnection";
        private const string PasswordMask = "********";

        // Same shape as ContactsRepository.GetAllCompanyNames() (dropdown population).
        private const string ContactsDropdownSql = @"
                SELECT ContactID, CompanyName, Enabled
                FROM ContactsTbl
                ORDER BY Enabled DESC, CompanyName";

        protected Panel pnlSqlConnectionTest;
        protected TextBox txtConnectionString;
        protected CheckBox chkRunProbeQuery;
        protected Button btnTest;
        protected Button btnTestContacts;
        protected Button btnReloadConfig;
        protected ImageButton btnBack;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;
        protected Panel pnlResults;
        protected Literal ltrlResults;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadConfiguredConnectionString();
                SetContactsTestEnabled(false);
            }
            else
            {
                // Keep enabled across postbacks only after a successful Open().
                SetContactsTestEnabled(!string.IsNullOrEmpty(ViewState[LastGoodConnectionViewStateKey] as string));
            }
        }

        protected void btnReloadConfig_Click(object sender, EventArgs e)
        {
            LoadConfiguredConnectionString();
            ViewState.Remove(LastGoodConnectionViewStateKey);
            SetContactsTestEnabled(false);
            SetStatus("Reloaded TrackerDataSQL from Web.config.", isError: null);
            pnlResults.Visible = false;
            ltrlResults.Text = string.Empty;
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Tools/SystemTools.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnTest_Click(object sender, EventArgs e)
        {
            string connectionString = ResolveConnectionStringForUse();
            if (string.IsNullOrEmpty(connectionString))
            {
                ViewState.Remove(LastGoodConnectionViewStateKey);
                SetContactsTestEnabled(false);
                SetStatus("Enter a connection string first.", isError: true);
                pnlResults.Visible = false;
                return;
            }

            var html = new StringBuilder();
            html.Append("<table class=\"results-table\" style=\"width:auto;min-width:28em;\">");
            html.Append("<tr><th>Check</th><th>Result</th></tr>");

            long openMs = -1;
            bool opened = false;
            string openError = null;

            try
            {
                var swOpen = Stopwatch.StartNew();
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    swOpen.Stop();
                    openMs = swOpen.ElapsedMilliseconds;
                    opened = true;

                    AppendRow(html, "Open()", "OK — " + openMs + " ms");
                    AppendRow(html, "DataSource", HttpUtility.HtmlEncode(conn.DataSource ?? string.Empty));
                    AppendRow(html, "Database", HttpUtility.HtmlEncode(conn.Database ?? string.Empty));
                    AppendRow(html, "ServerVersion", HttpUtility.HtmlEncode(conn.ServerVersion ?? string.Empty));
                    AppendRow(html, "State", HttpUtility.HtmlEncode(conn.State.ToString()));

                    if (chkRunProbeQuery != null && chkRunProbeQuery.Checked)
                    {
                        var swQuery = Stopwatch.StartNew();
                        using (var cmd = new SqlCommand(
                            "SELECT DB_NAME() AS DbName, @@SERVERNAME AS ServerName, CAST(@@VERSION AS nvarchar(4000)) AS VersionText;",
                            conn))
                        {
                            cmd.CommandTimeout = 15;
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    swQuery.Stop();
                                    AppendRow(html, "Probe query", "OK — " + swQuery.ElapsedMilliseconds + " ms");
                                    AppendRow(html, "DB_NAME()", HttpUtility.HtmlEncode(Convert.ToString(reader["DbName"])));
                                    AppendRow(html, "@@SERVERNAME", HttpUtility.HtmlEncode(Convert.ToString(reader["ServerName"])));

                                    string version = Convert.ToString(reader["VersionText"]) ?? string.Empty;
                                    if (version.Length > 180)
                                        version = version.Substring(0, 180) + "...";
                                    AppendRow(html, "@@VERSION", HttpUtility.HtmlEncode(version));
                                }
                                else
                                {
                                    swQuery.Stop();
                                    AppendRow(html, "Probe query", "No rows — " + swQuery.ElapsedMilliseconds + " ms");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                openError = ex.Message;
                AppendRow(html, "Open()", "FAILED");
                AppendRow(html, "Error", HttpUtility.HtmlEncode(ex.GetType().Name + ": " + ex.Message));
            }

            html.Append("</table>");
            html.Append("<p class=\"small\" style=\"margin-top:8px;\">");
            html.Append("Tested at ").Append(HttpUtility.HtmlEncode(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            html.Append(". Connection string is not saved to Web.config.");
            if (opened)
                html.Append(" Contacts dropdown query test is now enabled.");
            html.Append("</p>");

            pnlResults.Visible = true;
            ltrlResults.Text = html.ToString();

            if (opened)
            {
                RememberConnectionString(connectionString);
                ViewState[LastGoodConnectionViewStateKey] = connectionString;
                SetContactsTestEnabled(true);
                SetStatus("SQL connect OK — " + openMs + " ms.", isError: false);
            }
            else
            {
                ViewState.Remove(LastGoodConnectionViewStateKey);
                SetContactsTestEnabled(false);
                SetStatus("SQL connect failed" + (string.IsNullOrEmpty(openError) ? "." : ": " + openError), isError: true);
            }
        }

        protected void btnTestContacts_Click(object sender, EventArgs e)
        {
            string connectionString = ViewState[LastGoodConnectionViewStateKey] as string;
            if (string.IsNullOrWhiteSpace(connectionString))
                connectionString = ResolveConnectionStringForUse();

            if (string.IsNullOrEmpty(connectionString))
            {
                SetContactsTestEnabled(false);
                SetStatus("Run Test Connection successfully first.", isError: true);
                return;
            }

            var html = new StringBuilder();
            html.Append("<table class=\"results-table\" style=\"width:auto;min-width:28em;\">");
            html.Append("<tr><th>Check</th><th>Result</th></tr>");

            try
            {
                var sw = Stopwatch.StartNew();
                int rowCount = 0;

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(ContactsDropdownSql, conn))
                    {
                        cmd.CommandTimeout = 60;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                rowCount++;
                        }
                    }
                }

                sw.Stop();
                AppendRow(html, "Contacts dropdown query", "OK — " + sw.ElapsedMilliseconds + " ms");
                AppendRow(html, "Rows read", rowCount.ToString());
                AppendRow(html, "SQL", HttpUtility.HtmlEncode(
                    "SELECT ContactID, CompanyName, Enabled FROM ContactsTbl ORDER BY Enabled DESC, CompanyName"));
                html.Append("</table>");
                html.Append("<p class=\"small\" style=\"margin-top:8px;\">");
                html.Append("Same query as company-name pulldowns. Names are not displayed — timing only.");
                html.Append("</p>");

                pnlResults.Visible = true;
                ltrlResults.Text = html.ToString();
                SetContactsTestEnabled(true);
                SetStatus("Contacts query OK — " + rowCount + " row(s) in " + sw.ElapsedMilliseconds + " ms.", isError: false);
            }
            catch (Exception ex)
            {
                AppendRow(html, "Contacts dropdown query", "FAILED");
                AppendRow(html, "Error", HttpUtility.HtmlEncode(ex.GetType().Name + ": " + ex.Message));
                html.Append("</table>");
                pnlResults.Visible = true;
                ltrlResults.Text = html.ToString();
                SetStatus("Contacts query failed: " + ex.Message, isError: true);
            }
        }

        private void SetContactsTestEnabled(bool enabled)
        {
            if (btnTestContacts != null)
                btnTestContacts.Enabled = enabled;
        }

        private void LoadConfiguredConnectionString()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            string raw = settings != null ? (settings.ConnectionString ?? string.Empty) : string.Empty;
            RememberConnectionString(raw);

            if (settings == null)
                SetStatus("Connection string '" + ConnectionStringName + "' was not found in Web.config.", isError: true);
        }

        /// <summary>
        /// Stores the real connection string and shows a password-masked copy in the text box.
        /// </summary>
        private void RememberConnectionString(string connectionString)
        {
            string raw = connectionString ?? string.Empty;
            ViewState[RealConnectionViewStateKey] = raw;
            txtConnectionString.Text = MaskConnectionStringPassword(raw);
        }

        /// <summary>
        /// Uses the text box value, but if Password is still the mask, restores the real password
        /// from ViewState (or Web.config as fallback).
        /// </summary>
        private string ResolveConnectionStringForUse()
        {
            string displayed = (txtConnectionString.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(displayed))
                return string.Empty;

            string stored = ViewState[RealConnectionViewStateKey] as string;
            if (string.IsNullOrEmpty(stored))
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];
                stored = settings != null ? (settings.ConnectionString ?? string.Empty) : string.Empty;
            }

            try
            {
                var displayedBuilder = new SqlConnectionStringBuilder(displayed);
                if (!IsMaskedPassword(displayedBuilder.Password))
                    return displayedBuilder.ConnectionString;

                if (string.IsNullOrEmpty(stored))
                    return displayed;

                var storedBuilder = new SqlConnectionStringBuilder(stored);
                displayedBuilder.Password = storedBuilder.Password;
                return displayedBuilder.ConnectionString;
            }
            catch (ArgumentException)
            {
                // Malformed string — if it still contains the mask, fall back to stored raw.
                if (displayed.IndexOf(PasswordMask, StringComparison.Ordinal) >= 0
                    && !string.IsNullOrEmpty(stored))
                    return stored;
                return displayed;
            }
        }

        private static string MaskConnectionStringPassword(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return connectionString ?? string.Empty;

            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                if (string.IsNullOrEmpty(builder.Password))
                    return builder.ConnectionString;

                builder.Password = PasswordMask;
                return builder.ConnectionString;
            }
            catch (ArgumentException)
            {
                return connectionString;
            }
        }

        private static bool IsMaskedPassword(string password)
        {
            return string.Equals(password, PasswordMask, StringComparison.Ordinal);
        }

        private static void AppendRow(StringBuilder html, string label, string valueHtml)
        {
            html.Append("<tr><td>")
                .Append(HttpUtility.HtmlEncode(label))
                .Append("</td><td>")
                .Append(valueHtml ?? string.Empty)
                .Append("</td></tr>");
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
