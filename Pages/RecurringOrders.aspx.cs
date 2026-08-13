using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class RecurringOrders : Page
    {
        private const string CONST_SORTEXPRESSION_VIEWSTATE = "RecurringOrdersSortExpression";
        private const string DefaultReturnUrl = "~/Default.aspx";
        private const string FlashStatusSessionKey = "RecurringOrders.FlashStatus";

        protected ScriptManager smRecurringOrders;
        protected UpdateProgress uprgRecurringOrders;
        protected UpdatePanel upnlRecurringOrders;
        protected Panel pnlRecurringOrders;
        protected DropDownList ddlFilterBy;
        protected TextBox tbxFilterBy;
        protected Button btnGo;
        protected Button btnReset;
        protected Button btnCalcNextRequired;
        protected DropDownList ddlEnabledFilter;
        protected HyperLink hlAddRecurringOrder;
        protected ImageButton btnBack;
        protected GridView gvRecurringOrders;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPostBackControls();

            if (hlAddRecurringOrder != null)
                hlAddRecurringOrder.NavigateUrl = SystemConstants.PageUrls.RecurringOrderDetails;

            if (!IsPostBack)
            {
                ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = "CompanyName";

                if (!string.IsNullOrWhiteSpace(Request.QueryString["CompanyName"]))
                {
                    ddlFilterBy.SelectedValue = "CompanyName";
                    tbxFilterBy.Text = Request.QueryString["CompanyName"];
                }

                // Bind only on first load. Rebinding on every postback destroys the delete
                // LinkButtons before RowCommand runs (confirm fires, progress shows, but
                // nothing is deleted). Pager buttons are rebuilt in RowCreated from ViewState
                // (see GridPager); event handlers rebind after paging / filter / delete.
                BindRecurringOrdersGrid();
                ShowFlashStatusIfAny();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // GridView template LinkButtons are not re-bound on every postback — re-register
            // full postbacks each request so ScriptManager does not treat Delete as async.
            RegisterDeletePostBackControls();
        }

        /// <summary>
        /// Shows a one-shot status (and alert) after redirects from Recurring Order Details
        /// delete / save flows — startup scripts do not survive Response.Redirect.
        /// </summary>
        private void ShowFlashStatusIfAny()
        {
            string flash = Session[FlashStatusSessionKey] as string;
            if (string.IsNullOrWhiteSpace(flash))
                return;

            Session.Remove(FlashStatusSessionKey);
            SetStatus(flash, isError: false);
            new showMessageBox(Page, "Recurring Orders", flash);
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;

            scriptManager.RegisterAsyncPostBackControl(btnGo);
            scriptManager.RegisterAsyncPostBackControl(btnReset);
            scriptManager.RegisterAsyncPostBackControl(btnCalcNextRequired);
            scriptManager.RegisterAsyncPostBackControl(ddlEnabledFilter);
            scriptManager.RegisterAsyncPostBackControl(tbxFilterBy);
            scriptManager.RegisterPostBackControl(btnBack);
        }

        private void RegisterDeletePostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null || gvRecurringOrders == null)
                return;

            foreach (GridViewRow row in gvRecurringOrders.Rows)
            {
                var btnDelete = row.FindControl("btnDeleteRecurringOrder") as LinkButton;
                if (btnDelete != null)
                    scriptManager.RegisterPostBackControl(btnDelete);
            }
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

        private void BindRecurringOrdersGrid(string statusPrefix = null)
        {
            try
            {
                var recurringOrdersRepository = new RecurringOrdersRepository();
                string sortBy = ViewState[CONST_SORTEXPRESSION_VIEWSTATE] as string ?? "CompanyName";
                string companyNameFilter = ddlFilterBy.SelectedValue == "CompanyName"
                    ? tbxFilterBy.Text.Trim()
                    : string.Empty;
                int enabledFilter = Convert.ToInt32(ddlEnabledFilter.SelectedValue);

                var recurringOrders = recurringOrdersRepository.GetSummaries(sortBy, companyNameFilter, enabledFilter);
                var groupedRecurringOrders = BuildRecurringOrderGroupSummaries(recurringOrders, sortBy);

                gvRecurringOrders.DataSource = groupedRecurringOrders;
                gvRecurringOrders.DataBind();

                string enabledDescription = enabledFilter == 1
                    ? "enabled only"
                    : enabledFilter == 0
                        ? "disabled only"
                        : "enabled and disabled";

                string status = $"Showing {groupedRecurringOrders.Count} recurring order header(s) ({enabledDescription}).";
                if (!string.IsNullOrWhiteSpace(companyNameFilter))
                    status += $" Company filter: {companyNameFilter}.";

                if (!string.IsNullOrWhiteSpace(statusPrefix))
                {
                    SetStatus(statusPrefix.Trim() + " " + status, isError: false);
                    return;
                }

                SetStatus(status, isError: null);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "RecurringOrders BindRecurringOrdersGrid error: " + ex.Message);
                SetStatus("Error loading data: " + ex.Message, isError: true);
            }
        }

        protected void gvRecurringOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            var recurringOrderGroupSummary = e.Row.DataItem as RecurringOrderGroupSummary;
            if (recurringOrderGroupSummary == null)
                return;

            var recurringOrdersGrid = e.Row.FindControl("gvRecurringOrdersForContact") as GridView;
            if (recurringOrdersGrid != null)
            {
                recurringOrdersGrid.DataSource = recurringOrderGroupSummary.RecurringOrderItems;
                recurringOrdersGrid.DataBind();
            }

            var editRecurringOrderLink = e.Row.FindControl("hlEditRecurringOrder") as HyperLink;
            if (editRecurringOrderLink != null)
                editRecurringOrderLink.NavigateUrl = recurringOrderGroupSummary.DetailsNavigateUrl;

            var contactDetailsLink = e.Row.FindControl("hlContactDetails") as HyperLink;
            var companyNameLabel = e.Row.FindControl("lblCompanyName") as Label;
            if (recurringOrderGroupSummary.HasContactLink)
            {
                if (contactDetailsLink != null)
                {
                    contactDetailsLink.Text = recurringOrderGroupSummary.CompanyNameDisplay;
                    contactDetailsLink.NavigateUrl = recurringOrderGroupSummary.ContactDetailsNavigateUrl;
                    contactDetailsLink.ToolTip = "Open contact details";
                    contactDetailsLink.Visible = true;
                }

                if (companyNameLabel != null)
                    companyNameLabel.Visible = false;
            }
            else if (companyNameLabel != null)
            {
                companyNameLabel.Text = recurringOrderGroupSummary.CompanyNameDisplay;
                companyNameLabel.Visible = true;
            }

            var recurringOrderStatusLabel = e.Row.FindControl("lblRecurringOrderStatus") as Label;
            if (recurringOrderStatusLabel != null)
            {
                recurringOrderStatusLabel.Text = recurringOrderGroupSummary.EnabledDisplay;
                recurringOrderStatusLabel.CssClass = recurringOrderGroupSummary.Enabled == false
                    ? "status-badge is-disabled"
                    : "status-badge is-enabled";
            }

            var recurringOrderCountLabel = e.Row.FindControl("lblRecurringOrderCount") as Label;
            if (recurringOrderCountLabel != null)
                recurringOrderCountLabel.Text = recurringOrderGroupSummary.RecurringLineCount + " line(s)";

            var btnDeleteRecurringOrder = e.Row.FindControl("btnDeleteRecurringOrder") as LinkButton;
            if (btnDeleteRecurringOrder != null)
            {
                string companyName = recurringOrderGroupSummary.CompanyNameDisplay;
                int lineCount = recurringOrderGroupSummary.RecurringLineCount;
                string confirmMessage = "Delete the complete recurring order for '" + companyName + "' ("
                    + lineCount + " line(s))? This cannot be undone.";

                // Capture-phase JS + OnClientClick both call beginRecurringOrderListDelete.
                btnDeleteRecurringOrder.Attributes["data-confirm-delete"] = confirmMessage;
                btnDeleteRecurringOrder.OnClientClick =
                    "return beginRecurringOrderListDelete(this, "
                    + HttpUtility.JavaScriptStringEncode(confirmMessage, true)
                    + ");";

                // Full postback so the list always re-renders cleanly after delete.
                var scriptManager = ScriptManager.GetCurrent(Page);
                if (scriptManager != null)
                    scriptManager.RegisterPostBackControl(btnDeleteRecurringOrder);
            }
        }

        protected void gvRecurringOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.System,
                "RecurringOrders paging to page " + (e.NewPageIndex + 1) + " of " + gvRecurringOrders.PageCount);
            gvRecurringOrders.PageIndex = e.NewPageIndex;
            BindRecurringOrdersGrid();
        }

        /// <summary>App-standard pager (Previous / squares / Next) — see Classes/GridPager.cs.</summary>
        protected void gvRecurringOrders_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvRecurringOrders, e.Row);
        }

        protected void gvRecurringOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DeleteRecurringOrder", StringComparison.OrdinalIgnoreCase))
                return;

            int recurringOrderId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out recurringOrderId) || recurringOrderId <= 0)
            {
                SetStatus("Could not delete: missing recurring order id.", isError: true);
                upnlRecurringOrders.Update();
                return;
            }

            try
            {
                var recurringOrdersRepository = new RecurringOrdersRepository();
                var existing = recurringOrdersRepository.GetById(recurringOrderId);
                int contactId = existing?.ContactID ?? 0;
                string company = contactId > 0
                    ? (new ContactsRepository().GetContactNameById(contactId) ?? string.Empty)
                    : string.Empty;
                string contactPart = contactId > 0
                    ? (string.IsNullOrWhiteSpace(company)
                        ? $"Contact={contactId}"
                        : $"Contact={contactId} ({company})")
                    : "Contact=(none)";

                AppLogger.WriteLog(SystemConstants.LogTypes.Recurring,
                    $"RecurringOrder {recurringOrderId} | {contactPart} | Recurring order deleted | via=list");

                string contactNote = recurringOrdersRepository.Delete(recurringOrderId);
                string statusMessage = "Recurring order deleted."
                    + (string.IsNullOrWhiteSpace(contactNote) ? string.Empty : " " + contactNote);

                // Full-page reload clears the stuck "Deleting..." client UI.
                Session[FlashStatusSessionKey] = statusMessage;
                Response.Redirect(SystemConstants.PageUrls.RecurringOrders, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Recurring,
                    $"RecurringOrder {recurringOrderId} | Contact=(unknown) | Delete failed | via=list; {ex.Message}");
                SetStatus("Error deleting recurring order: " + ex.Message, isError: true);
                upnlRecurringOrders.Update();
            }
        }

        protected void btnCalcNextRequired_Click(object sender, EventArgs e)
        {
            try
            {
                var recurringOrdersRepository = new RecurringOrdersRepository();
                int updatedCount = recurringOrdersRepository.RecalculateNextDatesRequiredForEnabledHeaders();
                string statusMessage = "Calc Next Required recalculated "
                    + updatedCount
                    + " enabled recurring line(s).";

                BindRecurringOrdersGrid(statusMessage);
                new showMessageBox(Page, "Recurring Orders", statusMessage);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "RecurringOrders btnCalcNextRequired_Click error: " + ex.Message);
                SetStatus("Error recalculating next required dates: " + ex.Message, isError: true);
            }
        }

        protected void gvRecurringOrders_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = e.SortExpression;
            BindRecurringOrdersGrid();
        }

        protected void tbxFilterBy_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbxFilterBy.Text) && ddlFilterBy.SelectedValue == "0")
                ddlFilterBy.SelectedValue = "CompanyName";

            ApplyFilters();
        }

        protected void btnGo_Click(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ddlFilterBy.SelectedIndex = 0;
            tbxFilterBy.Text = string.Empty;
            ddlEnabledFilter.SelectedValue = "1";
            gvRecurringOrders.PageIndex = 0;
            BindRecurringOrdersGrid();
        }

        protected void ddlEnabledFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect(DefaultReturnUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Blank / SystemMinDate / far-future sentinel means "forever" — show infinity.
        /// </summary>
        protected string FormatRequireUntilDate(object value)
        {
            if (value == null || value == DBNull.Value)
                return "∞";

            DateTime untilDate;
            if (value is DateTime)
                untilDate = ((DateTime)value).Date;
            else if (!DateTime.TryParse(Convert.ToString(value), out untilDate))
                return "∞";
            else
                untilDate = untilDate.Date;

            if (untilDate <= SystemConstants.DatabaseConstants.SystemMinDate)
                return "∞";

            // Legacy / sentinel "open-ended" dates
            if (untilDate.Year >= 2099)
                return "∞";

            return untilDate.ToString("yyyy-MM-dd");
        }

        protected string FormatQty(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            double qty;
            if (value is double)
                qty = (double)value;
            else if (value is float)
                qty = (float)value;
            else if (value is decimal)
                qty = (double)(decimal)value;
            else if (!double.TryParse(Convert.ToString(value), out qty))
                return string.Empty;

            return SystemConstants.FormatConstants.FormatQuantity(qty);
        }

        private void ApplyFilters()
        {
            gvRecurringOrders.PageIndex = 0;
            BindRecurringOrdersGrid();
        }

        private List<RecurringOrderGroupSummary> BuildRecurringOrderGroupSummaries(List<RecurringOrderSummary> recurringOrders, string sortBy)
        {
            var groupedRecurringOrders = recurringOrders
                .GroupBy(recurringOrder => new
                {
                    recurringOrder.RecurringOrderID,
                    recurringOrder.ContactID,
                    CompanyName = recurringOrder.CompanyName ?? string.Empty,
                    recurringOrder.DeliveryByID,
                    DeliveryByDisplay = recurringOrder.DeliveryByDisplay ?? string.Empty,
                    recurringOrder.Enabled,
                    Notes = recurringOrder.Notes ?? string.Empty
                })
                .Select(group => new RecurringOrderGroupSummary
                {
                    RecurringOrderID = group.Key.RecurringOrderID,
                    ContactID = group.Key.ContactID,
                    CompanyName = group.Key.CompanyName,
                    DeliveryByID = group.Key.DeliveryByID,
                    DeliveryByDisplay = group.Key.DeliveryByDisplay,
                    Enabled = group.Key.Enabled,
                    Notes = group.Key.Notes,
                    RecurringOrderItems = SortRecurringOrdersForDisplay(group.ToList(), sortBy)
                })
                .ToList();

            return SortRecurringOrderGroupsForDisplay(groupedRecurringOrders, sortBy);
        }

        private List<RecurringOrderGroupSummary> SortRecurringOrderGroupsForDisplay(List<RecurringOrderGroupSummary> recurringOrders, string sortBy)
        {
            switch (sortBy)
            {
                case "DateLastDone":
                    return recurringOrders
                        .OrderBy(recurringOrder => recurringOrder.DateLastDone ?? DateTime.MaxValue)
                        .ThenBy(recurringOrder => recurringOrder.CompanyNameDisplay)
                        .ThenByDescending(recurringOrder => recurringOrder.Enabled ?? false)
                        .ToList();

                case "Enabled":
                    return recurringOrders
                        .OrderByDescending(recurringOrder => recurringOrder.Enabled ?? false)
                        .ThenBy(recurringOrder => recurringOrder.CompanyNameDisplay)
                        .ThenBy(recurringOrder => recurringOrder.NextDateRequired ?? DateTime.MaxValue)
                        .ToList();

                case "NextDateRequired":
                    return recurringOrders
                        .OrderBy(group => group.NextDateRequired ?? DateTime.MaxValue)
                        .ThenBy(group => group.CompanyNameDisplay)
                        .ThenByDescending(group => group.Enabled ?? false)
                        .ToList();

                default:
                    return recurringOrders
                        .OrderBy(group => group.CompanyNameDisplay)
                        .ThenByDescending(group => group.Enabled ?? false)
                        .ThenBy(group => group.NextDateRequired ?? DateTime.MaxValue)
                        .ToList();
            }
        }

        private List<RecurringOrderSummary> SortRecurringOrdersForDisplay(List<RecurringOrderSummary> recurringOrders, string sortBy)
        {
            switch (sortBy)
            {
                case "DateLastDone":
                    return recurringOrders
                        .OrderBy(recurringOrder => recurringOrder.DateLastDone ?? DateTime.MaxValue)
                        .ThenBy(recurringOrder => recurringOrder.NextDateRequired ?? DateTime.MaxValue)
                        .ToList();

                case "RequireUntilDate":
                    return recurringOrders
                        .OrderBy(recurringOrder => recurringOrder.RequireUntilDate ?? DateTime.MaxValue)
                        .ThenBy(recurringOrder => recurringOrder.NextDateRequired ?? DateTime.MaxValue)
                        .ToList();

                case "Enabled":
                    return recurringOrders
                        .OrderBy(recurringOrder => recurringOrder.Enabled ?? false)
                        .ThenBy(recurringOrder => recurringOrder.NextDateRequired ?? DateTime.MaxValue)
                        .ToList();

                case "Notes":
                    return recurringOrders
                        .OrderBy(recurringOrder => recurringOrder.Notes ?? string.Empty)
                        .ThenBy(recurringOrder => recurringOrder.NextDateRequired ?? DateTime.MaxValue)
                        .ToList();

                case "RecurringTypeDesc":
                    return recurringOrders
                        .OrderBy(recurringOrder => recurringOrder.RecurringPatternDisplay)
                        .ThenBy(recurringOrder => recurringOrder.NextDateRequired ?? DateTime.MaxValue)
                        .ToList();

                case "CompanyName":
                case "NextDateRequired":
                default:
                    return recurringOrders
                        .OrderBy(recurringOrder => recurringOrder.NextDateRequired ?? DateTime.MaxValue)
                        .ThenBy(recurringOrder => recurringOrder.RecurringPatternDisplay)
                        .ThenBy(recurringOrder => recurringOrder.ItemDesc ?? string.Empty)
                        .ToList();
            }
        }
    }
}
