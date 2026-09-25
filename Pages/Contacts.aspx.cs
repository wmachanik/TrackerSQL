//------------------------------------------------------------------------------
// TrackerSQL v3.x — Contacts
// WebForms page code-behind for Contacts.
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class Contacts : Page
    {
        // Session key used by page filter
        private const string CONST_WHERECLAUSE_SESSIONVAR = "ContactSummaryWhereFilter";
        private const string CONST_SORTEXPRESSION_VIEWSTATE = "ContactsSortExpression";
        private const string CONST_FILTER_COOKIE = "TrackerContactsFilter";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["CompanyName"] != null)
                {
                    tbxFilterBy.Text = Request.QueryString["CompanyName"].ToString();
                    ddlFilterBy.SelectedValue = "CompanyName";
                    Session[CONST_WHERECLAUSE_SESSIONVAR] = $"CompanyName LIKE '{tbxFilterBy.Text.Replace("'", "''")}%'";
                }
                else
                {
                    Session[CONST_WHERECLAUSE_SESSIONVAR] = string.Empty;
                    RestoreFilterFromCookie();
                }

                ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = "CompanyName";
                BindContactsGrid();
            }
            else
            {
                if (Session[CONST_WHERECLAUSE_SESSIONVAR] != null)
                    SetFilterStatus(Session[CONST_WHERECLAUSE_SESSIONVAR].ToString(), null);
            }
        }

        private void SetFilterStatus(string message, bool? isError)
        {
            lblFilter.Text = message ?? string.Empty;

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

        /// <summary>
        /// Binds the contacts grid using the repository pattern
        /// </summary>
        private void BindContactsGrid()
        {
            try
            {
                var repo = new ContactSummariesRepository();
                string sortBy = ViewState[CONST_SORTEXPRESSION_VIEWSTATE] as string ?? "CompanyName";
                int isEnabled = int.TryParse(ddlContactEnabled.SelectedValue, out int val) ? val : 1;
                string whereFilter = Session[CONST_WHERECLAUSE_SESSIONVAR] as string ?? string.Empty;

                var contacts = repo.GetAllContactSummaries(sortBy, isEnabled, whereFilter);
                gvContacts.DataSource = contacts;
                gvContacts.DataBind();

                if (string.IsNullOrEmpty(whereFilter))
                    SetFilterStatus($"Showing {contacts.Count} contacts", null);
                else
                    SetFilterStatus($"Filter: {whereFilter} ({contacts.Count} results)", null);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Contacts BindContactsGrid error: " + ex.Message);
                SetFilterStatus("Error loading contacts: " + ex.Message, true);
            }
        }

        protected void gvContacts_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvContacts.PageIndex = e.NewPageIndex;
            BindContactsGrid();
        }

        /// <summary>App-standard pager (Previous / squares / Next) — see Classes/GridPager.cs.</summary>
        protected void gvContacts_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvContacts, e.Row);
        }

        protected void gvContacts_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = e.SortExpression;
            BindContactsGrid();
        }

        protected void btnGon_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxFilterBy.Text))
            {
                SetFilterStatus("Type something to search for, then click Go.", true);
                return;
            }

            // No field chosen -> assume Company Name (and show that in the dropdown).
            if (ddlFilterBy.SelectedValue == "0")
                ddlFilterBy.SelectedValue = "CompanyName";

            string where = BuildWhereClause(ddlFilterBy.SelectedValue, tbxFilterBy.Text);
            if (where == null)
            {
                Session[CONST_WHERECLAUSE_SESSIONVAR] = "1=0";
                SetFilterStatus("Please enter a valid numeric Contact ID.", true);
                new showMessageBox(Page, "Input Error", "Please enter a valid numeric Contact ID.");
                BindContactsGrid();
                upnlContactSummary.Update();
                return;
            }

            Session[CONST_WHERECLAUSE_SESSIONVAR] = where;
            SaveFilterCookie();
            gvContacts.PageIndex = 0;
            BindContactsGrid();
        }

        /// <summary>SQL filter for the chosen field and text; null when a Contact ID search is not numeric.</summary>
        private static string BuildWhereClause(string filterField, string text)
        {
            // Escape single quotes — names like O'Brien must not break the LIKE clause.
            string filterValue = (text ?? string.Empty).Trim().Replace("'", "''");

            if (filterField == "ContactID")
                return int.TryParse(filterValue, out int contactId) ? $"ContactID = {contactId}" : null;

            if (!filterValue.StartsWith("%"))
                filterValue = "%" + filterValue + "%";
            return $"{filterField} LIKE '{filterValue}'";
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Session[CONST_WHERECLAUSE_SESSIONVAR] = string.Empty;
            ddlFilterBy.SelectedIndex = 0;
            tbxFilterBy.Text = string.Empty;
            SaveFilterCookie();
            gvContacts.PageIndex = 0;
            BindContactsGrid();
            upnlContactSummary.Update();
        }

        /// <summary>Remembers enabled/disabled/both plus the last search (field and text) in this browser.</summary>
        private void SaveFilterCookie()
        {
            string raw = ddlContactEnabled.SelectedValue + "|"
                + (string.IsNullOrWhiteSpace(tbxFilterBy.Text) ? "0" : ddlFilterBy.SelectedValue) + "|"
                + (tbxFilterBy.Text ?? string.Empty).Trim();
            string encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw))
                .Replace('+', '-').Replace('/', '_').Replace('=', '.');

            var cookie = new HttpCookie(CONST_FILTER_COOKIE, encoded)
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(90),
                Path = "/"
            };
            if (Request.IsSecureConnection)
                cookie.Secure = true;
            Response.Cookies.Set(cookie);
        }

        private void RestoreFilterFromCookie()
        {
            HttpCookie cookie = Request.Cookies[CONST_FILTER_COOKIE];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
                return;

            string[] parts;
            try
            {
                string base64 = cookie.Value.Replace('-', '+').Replace('_', '/').Replace('.', '=');
                parts = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64)).Split(new[] { '|' }, 3);
            }
            catch (FormatException)
            {
                return;
            }
            if (parts.Length < 3)
                return;

            if (ddlContactEnabled.Items.FindByValue(parts[0]) != null)
                ddlContactEnabled.SelectedValue = parts[0];

            // Field name goes straight into SQL — only accept values offered in the dropdown.
            string field = parts[1];
            string text = parts[2];
            if (field == "0" || ddlFilterBy.Items.FindByValue(field) == null || string.IsNullOrWhiteSpace(text))
                return;

            string where = BuildWhereClause(field, text);
            if (where == null)
                return;

            ddlFilterBy.SelectedValue = field;
            tbxFilterBy.Text = text;
            Session[CONST_WHERECLAUSE_SESSIONVAR] = where;
        }

        protected void tbxFilterBy_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxFilterBy.Text) || ddlFilterBy.SelectedIndex != 0)
                return;
            ddlFilterBy.SelectedIndex = 1; // default to CompanyName
            upnlContactSummary.Update();
        }

        protected void ddlContactEnabled_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveFilterCookie();
            gvContacts.PageIndex = 0;
            BindContactsGrid();
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
