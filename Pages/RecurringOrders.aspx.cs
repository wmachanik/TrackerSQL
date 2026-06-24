using System;
using System.Collections.Generic;
using System.Linq;
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

        protected ScriptManager smRecurringOrders;
        protected UpdateProgress uprgRecurringOrders;
        protected UpdatePanel upnlSelection;
        protected DropDownList ddlFilterBy;
        protected TextBox tbxFilterBy;
        protected Button btnGo;
        protected Button btnReset;
        protected Button btnCalcNextRequired;
        protected DropDownList ddlEnabledFilter;
        protected HyperLink hlAddRecurringOrder;
        protected UpdatePanel upnlRecurringOrdersSummary;
        protected GridView gvRecurringOrders;
        protected Label lblFilter;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = "CompanyName";

                if (!string.IsNullOrWhiteSpace(Request.QueryString["CompanyName"]))
                {
                    ddlFilterBy.SelectedValue = "CompanyName";
                    tbxFilterBy.Text = Request.QueryString["CompanyName"];
                }

                BindRecurringOrdersGrid();
            }
        }

        private void BindRecurringOrdersGrid()
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

                lblFilter.Text = $"<strong>Showing {groupedRecurringOrders.Count} recurring order header(s)</strong>";

                if (!string.IsNullOrWhiteSpace(companyNameFilter))
                {
                    lblFilter.Text += $"<br/><span style='color:#666;'>Company filter: {Server.HtmlEncode(companyNameFilter)}</span>";
                }

                string enabledDescription = enabledFilter == 1
                    ? "Enabled only"
                    : enabledFilter == 0
                        ? "Disabled only"
                        : "Enabled and disabled";
                lblFilter.Text += $"<br/><span style='color:#666;'>Status: {enabledDescription}</span>";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "RecurringOrders BindRecurringOrdersGrid error: " + ex.Message);
                lblFilter.Text = "<span style='color:red;'>Error loading data: " + Server.HtmlEncode(ex.Message) + "</span>";
            }
        }

        protected void gvRecurringOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            var recurringOrderGroupSummary = e.Row.DataItem as RecurringOrderGroupSummary;
            if (recurringOrderGroupSummary == null)
            {
                return;
            }

            var recurringOrdersGrid = e.Row.FindControl("gvRecurringOrdersForContact") as GridView;
            if (recurringOrdersGrid != null)
            {
                recurringOrdersGrid.DataSource = recurringOrderGroupSummary.RecurringOrderItems;
                recurringOrdersGrid.DataBind();
            }

            var editRecurringOrderLink = e.Row.FindControl("hlEditRecurringOrder") as HyperLink;
            if (editRecurringOrderLink != null)
            {
                editRecurringOrderLink.NavigateUrl = recurringOrderGroupSummary.DetailsNavigateUrl;
            }

            var contactDetailsLink = e.Row.FindControl("hlContactDetails") as HyperLink;
            var companyNameLabel = e.Row.FindControl("lblCompanyName") as Label;
            if (recurringOrderGroupSummary.HasContactLink)
            {
                if (contactDetailsLink != null)
                {
                    contactDetailsLink.Text = recurringOrderGroupSummary.CompanyNameDisplay;
                    contactDetailsLink.NavigateUrl = recurringOrderGroupSummary.ContactDetailsNavigateUrl;
                    contactDetailsLink.Visible = true;
                }

                if (companyNameLabel != null)
                {
                    companyNameLabel.Visible = false;
                }
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
            }

            var recurringOrderCountLabel = e.Row.FindControl("lblRecurringOrderCount") as Label;
            if (recurringOrderCountLabel != null)
            {
                recurringOrderCountLabel.Text = recurringOrderGroupSummary.RecurringLineCount + " line(s)";
            }
        }

        protected void gvRecurringOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRecurringOrders.PageIndex = e.NewPageIndex;
            BindRecurringOrdersGrid();
        }

        protected void gvRecurringOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DeleteRecurringOrder", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int recurringOrderId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out recurringOrderId) || recurringOrderId <= 0)
            {
                return;
            }

            try
            {
                var recurringOrdersRepository = new RecurringOrdersRepository();
                recurringOrdersRepository.Delete(recurringOrderId);
                BindRecurringOrdersGrid();
                lblFilter.Text = "<span style='color:#55624a;font-weight:bold;'>Recurring order deleted.</span><br/>" + lblFilter.Text;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "RecurringOrders gvRecurringOrders_RowCommand delete error: " + ex.Message);
                lblFilter.Text = "<span style='color:red;'>Error deleting recurring order: " + Server.HtmlEncode(ex.Message) + "</span>";
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

                BindRecurringOrdersGrid();
                lblFilter.Text = "<span style='color:#55624a;font-weight:bold;'>"
                    + statusMessage
                    + "</span><br/>"
                    + lblFilter.Text;
                new showMessageBox(Page, "Recurring Orders", statusMessage);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "RecurringOrders btnCalcNextRequired_Click error: " + ex.Message);
                lblFilter.Text = "<span style='color:red;'>Error recalculating next required dates: " + Server.HtmlEncode(ex.Message) + "</span>";
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
            {
                ddlFilterBy.SelectedValue = "CompanyName";
            }

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
