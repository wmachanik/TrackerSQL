//------------------------------------------------------------------------------
// TrackerSQL v3.x — Contacts
// WebForms page code-behind for Contacts.
//------------------------------------------------------------------------------

using System;
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["CompanyName"] != null)
                {
                    tbxFilterBy.Text = Request.QueryString["CompanyName"].ToString();
                    ddlFilterBy.SelectedValue = "CompanyName";
                    Session[CONST_WHERECLAUSE_SESSIONVAR] = $"CompanyName LIKE '{tbxFilterBy.Text}%'";
                }
                else
                {
                    Session[CONST_WHERECLAUSE_SESSIONVAR] = string.Empty;
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

        protected void gvContacts_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = e.SortExpression;
            BindContactsGrid();
        }

        protected void btnGon_Click(object sender, EventArgs e)
        {
            if (ddlFilterBy.SelectedValue == "0" || string.IsNullOrWhiteSpace(tbxFilterBy.Text))
                return;

            string filterField = ddlFilterBy.SelectedValue;
            string filterValue = tbxFilterBy.Text.Trim();

            if (filterField == "ContactID")
            {
                if (int.TryParse(filterValue, out int contactId))
                {
                    Session[CONST_WHERECLAUSE_SESSIONVAR] = $"ContactID = {contactId}";
                }
                else
                {
                    Session[CONST_WHERECLAUSE_SESSIONVAR] = "1=0";
                    SetFilterStatus("Please enter a valid numeric Contact ID.", true);
                    new showMessageBox(Page, "Input Error", "Please enter a valid numeric Contact ID.");
                    BindContactsGrid();
                    upnlContactSummary.Update();
                    return;
                }
            }
            else
            {
                if (!filterValue.StartsWith("%"))
                    filterValue = "%" + filterValue + "%";
                Session[CONST_WHERECLAUSE_SESSIONVAR] = $"{filterField} LIKE '{filterValue}'";
            }
            gvContacts.PageIndex = 0;
            BindContactsGrid();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Session[CONST_WHERECLAUSE_SESSIONVAR] = string.Empty;
            ddlFilterBy.SelectedIndex = 0;
            tbxFilterBy.Text = string.Empty;
            gvContacts.PageIndex = 0;
            BindContactsGrid();
            upnlContactSummary.Update();
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
            gvContacts.PageIndex = 0;
            BindContactsGrid();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
