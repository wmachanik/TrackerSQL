// Decompiled with JetBrains decompiler
// Type: TrackerSQL.DataSets.TrackerDataSet
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;
using static TrackerSQL.Classes.MessageKeys;

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
                    lblFilter.Text = Session[CONST_WHERECLAUSE_SESSIONVAR].ToString();
            }
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

                lblFilter.Text = string.IsNullOrEmpty(whereFilter) 
                    ? $"Showing {contacts.Count} contacts" 
                    : $"Filter: {whereFilter} ({contacts.Count} results)";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Contacts BindContactsGrid error: " + ex.Message);
                lblFilter.Text = "Error loading contacts: " + ex.Message;
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
                    lblFilter.Text = $"Filtered by ContactID={contactId}.";
                }
                else
                {
                    Session[CONST_WHERECLAUSE_SESSIONVAR] = "1=0";
                    lblFilter.Text = "Please enter a valid numeric Contact ID.";
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
                lblFilter.Text = $"Filtered by {filterField} LIKE '{filterValue}'";
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
            upnlSelection.Update();
        }

        protected void tbxFilterBy_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxFilterBy.Text) || ddlFilterBy.SelectedIndex != 0)
                return;
            ddlFilterBy.SelectedIndex = 1; // default to CompanyName
            upnlSelection.Update();
        }

        protected void ddlContactEnabled_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvContacts.PageIndex = 0;
            BindContactsGrid();
        }
    }
}
