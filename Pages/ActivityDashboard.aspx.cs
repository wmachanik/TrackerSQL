using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Pages
{
    public partial class ActivityDashboard : Page
    {
        private const string CustomPeriodValue = "custom";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SelectDays("30");
                InitCustomDateFields();
            }

            SyncCustomPanelVisibility();
            BindDashboard();
        }

        protected void ddlDays_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsCustomMode())
                EnsureCustomDatesDefault();
            SyncCustomPanelVisibility();
            BindDashboard();
        }

        protected void btnApplyCustomRange_Click(object sender, EventArgs e)
        {
            SyncCustomPanelVisibility();
            BindDashboard();
        }

        private void SyncCustomPanelVisibility()
        {
            pnlCustomRange.Visible = IsCustomMode();
        }

        private bool IsCustomMode()
        {
            return string.Equals(ddlDays.SelectedValue, CustomPeriodValue, StringComparison.OrdinalIgnoreCase);
        }

        private void SelectDays(string value)
        {
            var item = ddlDays.Items.FindByValue(value);
            if (item != null)
            {
                ddlDays.ClearSelection();
                item.Selected = true;
            }
        }

        private void InitCustomDateFields()
        {
            DateTime today = TimeZoneUtils.Now().Date;
            tbxCustomFrom.Text = today.AddDays(-29).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            tbxCustomTo.Text = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private void EnsureCustomDatesDefault()
        {
            if (string.IsNullOrWhiteSpace(tbxCustomFrom.Text) || string.IsNullOrWhiteSpace(tbxCustomTo.Text))
                InitCustomDateFields();
        }

        private int GetSelectedDays()
        {
            if (int.TryParse(ddlDays.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int days))
                return days;
            return 30;
        }

        private void BindDashboard()
        {
            var manager = new ActivityDashboardManager();
            ActivityDashboardViewModel vm;
            string rangeError = null;

            if (IsCustomMode())
            {
                if (!TryParseCustomRange(out DateTime from, out DateTime to, out rangeError))
                {
                    ShowRangeStatus(rangeError, isError: true);
                    return;
                }

                int span = (to - from).Days + 1;
                if (span > ActivityDashboardManager.MaxCustomRangeDays)
                {
                    ShowRangeStatus(
                        "Date range cannot be longer than " + ActivityDashboardManager.MaxCustomRangeDays + " days.",
                        isError: true);
                    return;
                }

                vm = manager.BuildForRange(from, to, isCustomRange: true);
            }
            else
            {
                vm = manager.Build(GetSelectedDays());
            }

            ShowRangeStatus(null, isError: false);

            litAsOf.Text = vm.GeneratedAt.ToString("dddd d MMM yyyy", CultureInfo.InvariantCulture);
            litCurrentRange.Text = vm.CurrentRangeText;
            litPreviousRange.Text = vm.PreviousRangeText;
            litForwardRange.Text = vm.ForwardRangeText;

            litProjectedRecurring.Text = vm.ProjectedRecurringLines.ToString(CultureInfo.InvariantCulture);
            litProjectedPredicted.Text = vm.ProjectedPredictedOrders.ToString(CultureInfo.InvariantCulture);

            var coffee = ActivityDashboardManager.BuildCoffeePanel(vm.Current, vm.Previous);
            litCoffeeDelta.Text = coffee.DeltaText;
            litCoffeeCurrent.Text = FormatValue(coffee.CurrentValue, true);
            litCoffeePrevious.Text = FormatValue(coffee.PreviousValue, true);
            barCoffeeCurrent.Style["width"] = coffee.BarCurrentPercent + "%";
            barCoffeePrior.Style["width"] = coffee.BarPreviousPercent + "%";

            bool hasCoffeeRows = vm.CoffeeByItemCompare != null && vm.CoffeeByItemCompare.Count > 0;
            pnlCoffeeStripsEmpty.Visible = !hasCoffeeRows;
            if (hasCoffeeRows)
            {
                rptCoffeeByItem.DataSource = vm.CoffeeByItemCompare;
                rptCoffeeByItem.DataBind();
            }
            else
            {
                rptCoffeeByItem.DataSource = null;
                rptCoffeeByItem.DataBind();
            }

            rptPanels.DataSource = vm.Panels;
            rptPanels.DataBind();
        }

        private bool TryParseCustomRange(out DateTime from, out DateTime to, out string error)
        {
            from = DateTime.MinValue;
            to = DateTime.MinValue;
            error = null;

            if (!DateTime.TryParseExact(tbxCustomFrom.Text?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out from))
            {
                error = "Enter a valid From date (yyyy-MM-dd).";
                return false;
            }

            if (!DateTime.TryParseExact(tbxCustomTo.Text?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out to))
            {
                error = "Enter a valid To date (yyyy-MM-dd).";
                return false;
            }

            from = from.Date;
            to = to.Date;

            if (to < from)
            {
                error = "To date must be on or after From date.";
                return false;
            }

            return true;
        }

        private void ShowRangeStatus(string message, bool isError)
        {
            if (pnlRangeStatus == null)
                return;

            bool show = !string.IsNullOrWhiteSpace(message);
            pnlRangeStatus.Visible = show;
            if (litRangeStatus != null)
                litRangeStatus.Text = show ? message : string.Empty;
            pnlRangeStatus.CssClass = isError ? "activity-range-status is-error" : "activity-range-status";
        }

        protected void rptPanels_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            if (!(e.Item.DataItem is ActivityMetricPanel panel))
                return;

            var litCurrent = e.Item.FindControl("litCurrentVal") as Literal;
            var litPrevious = e.Item.FindControl("litPreviousVal") as Literal;
            if (litCurrent != null)
                litCurrent.Text = "<span class=\"activity-panel-value activity-panel-value-sm\">"
                    + FormatValue(panel.CurrentValue, panel.UseDecimalFormat) + "</span>";
            if (litPrevious != null)
                litPrevious.Text = "<span class=\"activity-panel-value activity-panel-value-sm\">"
                    + FormatValue(panel.PreviousValue, panel.UseDecimalFormat) + "</span>";
        }

        private static string FormatValue(decimal value, bool useDecimal)
        {
            return useDecimal
                ? value.ToString("N2", CultureInfo.InvariantCulture)
                : value.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}