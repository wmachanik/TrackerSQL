using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    public partial class ContactPortalAdmin : Page
    {
        private readonly ContactPortalManager _portal = new ContactPortalManager();

        protected Panel pnlAccessDenied;
        protected Panel pnlMain;
        protected Panel pnlFields;
        protected Panel pnlRequests;
        protected CheckBoxList cblFields;
        protected DropDownList ddlStatus;
        protected GridView gvRequests;
        protected Literal litMessage;
        protected Button btnSaveFields;
        protected Button btnEnsureSchema;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SecurityManager.IsAdmin())
            {
                pnlMain.Visible = false;
                pnlAccessDenied.Visible = true;
                return;
            }

            _portal.EnsureReady();

            if (!IsPostBack)
            {
                BindFields();
                BindRequests();
            }
        }

        private void BindFields()
        {
            var settings = _portal.GetSettings();
            var selected = ContactPortalSettingsRepository.ParseFields(settings.EditableContactFields);
            cblFields.Items.Clear();
            foreach (string field in ContactPortalSettingsRepository.AllowedFieldCatalog)
            {
                cblFields.Items.Add(new ListItem(ContactPortalSettingsRepository.FieldLabel(field), field)
                {
                    Selected = selected.Contains(field)
                });
            }
        }

        private void BindRequests()
        {
            gvRequests.DataSource = _portal.ListRequests(ddlStatus.SelectedValue);
            gvRequests.DataBind();
        }

        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvRequests.PageIndex = 0;
            BindRequests();
        }

        protected void gvRequests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRequests.PageIndex = e.NewPageIndex;
            BindRequests();
        }

        protected void gvRequests_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvRequests, e.Row);
        }

        protected void btnSaveFields_Click(object sender, EventArgs e)
        {
            string csv = string.Join(",",
                cblFields.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value));
            if (_portal.SaveSettings(csv, User.Identity.Name))
                SetMsg("Field settings saved.", false);
            else
                SetMsg("Could not save the field settings.", true);
            BindFields();
        }

        protected void btnEnsureSchema_Click(object sender, EventArgs e)
        {
            var result = new ContactPortalSchemaInstaller().EnsureSchema();
            SetMsg(result.Message ?? (result.Succeeded ? "Portal tables are in place." : "Could not check the portal tables."), !result.Succeeded);
        }

        protected void gvRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string status = e.CommandName == "MarkDone"
                ? ContactPortalChangeStatuses.Done
                : e.CommandName == "MarkReject"
                    ? ContactPortalChangeStatuses.Rejected
                    : null;
            if (status == null || !long.TryParse(e.CommandArgument?.ToString(), out long id) || id <= 0)
                return;

            var row = (e.CommandSource as Control)?.NamingContainer as GridViewRow;
            string note = (row?.FindControl("txtNote") as TextBox)?.Text;

            bool ok = _portal.ResolveRequest(id, status, note, User.Identity.Name, out string message);
            SetMsg(message, !ok);
            BindRequests();
        }

        protected string AboutText(object dataItem)
        {
            var r = dataItem as ContactPortalChangeRequest;
            return r == null ? string.Empty : ContactPortalManager.DescribeRequestSubject(r.Kind, r.RelatedId);
        }

        protected bool IsOpen(object dataItem)
        {
            return (dataItem as ContactPortalChangeRequest)?.Status == ContactPortalChangeStatuses.Open;
        }

        /// <summary>Status badge, plus who resolved it, when, and the note sent to the contact.</summary>
        protected string StatusHtml(object dataItem)
        {
            var r = dataItem as ContactPortalChangeRequest;
            if (r == null)
                return string.Empty;

            string css = r.Status == ContactPortalChangeStatuses.Done ? "is-done"
                : r.Status == ContactPortalChangeStatuses.Rejected ? "is-disabled"
                : "is-enabled";
            string html = "<span class=\"status-badge " + css + "\">" + HttpUtility.HtmlEncode(r.Status) + "</span>";
            if (r.ResolvedAt.HasValue)
            {
                html += "<div class=\"sys-prefs-help\">" + HttpUtility.HtmlEncode(r.ResolvedAt.Value.ToString("dd MMM yyyy HH:mm")
                    + (string.IsNullOrWhiteSpace(r.ResolvedBy) ? string.Empty : " by " + r.ResolvedBy)) + "</div>";
            }
            if (!string.IsNullOrWhiteSpace(r.StaffNote))
                html += "<div class=\"sys-prefs-help\">Note: " + HttpUtility.HtmlEncode(r.StaffNote) + "</div>";
            return html;
        }

        private void SetMsg(string text, bool error)
        {
            litMessage.Text = "<p class=\"status-message "
                + (error ? "status-error" : "status-info")
                + "\">" + HttpUtility.HtmlEncode(text) + "</p>";
        }
    }
}
