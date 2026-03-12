using AjaxControlToolkit;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using static TrackerSQL.Classes.MessageKeys;

namespace TrackerSQL.Pages
{
    public partial class Contacts : Page
    {
        // Session key used by page filter (aligned with ObjectDataSource SessionParameter)
        private const string CONST_WHERECLAUSE_SESSIONVAR = "ContactSummaryWhereFilter";
        protected ScriptManager smContactSummary;
        protected UpdateProgress uprgContactSummary;
        protected UpdatePanel upnlSelection;
        protected DropDownList ddlFilterBy;
        protected TextBox tbxFilterBy;
        protected Button btnGon;
        protected Button btnReset;
        protected DropDownList ddlContactEnabled;
        protected UpdatePanel upnlContactSummary;
        protected GridView gvContacts;
        protected ObjectDataSource odsContactSummaries;
        protected Label lblFilter;

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
                // Ensure initial bind
                odsContactSummaries.DataBind();
                gvContacts.DataBind();

                // Diagnostics: log bound row count
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Contacts Page_Load initial bind: gvContacts.Rows=" + gvContacts.Rows.Count);
                if (gvContacts.Rows.Count == 0)
                {
                    // Direct repository call to see if data exists ignoring filters
                    try
                    {
                        var repo = new TrackerDotNet.Classes.Sql.ContactSummariesRepository();
                        var rawList = repo.GetAllContactSummaries("CompanyName", -1, string.Empty); // no filter, both enabled states
                        AppLogger.WriteLog(SystemConstants.LogTypes.System, "Direct repository test (no filters) returned count=" + rawList.Count);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.System, "Direct repository test failed: " + ex.Message);
                    }
                }
            }
            else
            {
                if (Session[CONST_WHERECLAUSE_SESSIONVAR] != null)
                    lblFilter.Text = Session[CONST_WHERECLAUSE_SESSIONVAR].ToString();
            }
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
            odsContactSummaries.DataBind();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Session[CONST_WHERECLAUSE_SESSIONVAR] = string.Empty;
            ddlFilterBy.SelectedIndex = 0;
            tbxFilterBy.Text = string.Empty;
            odsContactSummaries.DataBind();
        }

        protected void tbxFilterBy_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbxFilterBy.Text) || ddlFilterBy.SelectedIndex != 0)
                return;
            ddlFilterBy.SelectedIndex = 1; // default to CompanyName
            upnlSelection.Update();
        }

        // Diagnostics for ODS
        protected void odsContactSummaries_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            string sort = e.InputParameters["SortBy"] as string;
            int enabled = e.InputParameters["IsEnabled"] != null ? Convert.ToInt32(e.InputParameters["IsEnabled"]) : -1;
            string where = e.InputParameters["WhereFilter"] as string;
            AppLogger.WriteLog(SystemConstants.LogTypes.System, $"ODS Selecting: SortBy={sort}, IsEnabled={enabled}, Where='{where}'");
        }

        protected void odsContactSummaries_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.Exception != null)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "ODS Selected error: " + e.Exception.Message);
                e.ExceptionHandled = true;
            }
            else
            {
                var result = e.ReturnValue as System.Collections.ICollection;
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"ODS Selected OK: count={(result != null ? result.Count : -1)}");
            }
        }
    }
}
