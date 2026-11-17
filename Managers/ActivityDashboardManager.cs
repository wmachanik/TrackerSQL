using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class ActivityDashboardManager
    {
        public static readonly int[] AllowedDayWindows = { 7, 30, 90, 180, 360 };

        private readonly ActivityDashboardRepository _repository = new ActivityDashboardRepository();

        public const int MaxCustomRangeDays = 366;

        public ActivityDashboardViewModel Build(int selectedDays)
        {
            if (Array.IndexOf(AllowedDayWindows, selectedDays) < 0)
                selectedDays = 30;

            DateTime today = TimeZoneUtils.Now().Date;
            DateTime currentFrom = today.AddDays(-(selectedDays - 1));
            return BuildForRange(currentFrom, today, isCustomRange: false);
        }

        public ActivityDashboardViewModel BuildForRange(DateTime currentFrom, DateTime currentTo, bool isCustomRange = true)
        {
            DateTime today = TimeZoneUtils.Now().Date;
            currentFrom = currentFrom.Date;
            currentTo = currentTo.Date;

            if (currentTo < currentFrom)
            {
                DateTime swap = currentFrom;
                currentFrom = currentTo;
                currentTo = swap;
            }

            int selectedDays = (currentTo - currentFrom).Days + 1;
            if (selectedDays < 1)
                selectedDays = 1;

            DateTime previousTo = currentFrom.AddDays(-1);
            DateTime previousFrom = previousTo.AddDays(-(selectedDays - 1));
            DateTime forwardEnd = today.AddDays(selectedDays - 1);

            var vm = new ActivityDashboardViewModel
            {
                GeneratedAt = TimeZoneUtils.Now(),
                SelectedDays = selectedDays,
                IsCustomRange = isCustomRange,
                CurrentRangeText = FormatRange(currentFrom, currentTo),
                PreviousRangeText = FormatRange(previousFrom, previousTo),
                ForwardRangeText = FormatRange(today, forwardEnd),
                Current = _repository.LoadSnapshot(selectedDays, currentFrom, currentTo),
                Previous = _repository.LoadSnapshot(selectedDays, previousFrom, previousTo),
                ProjectedRecurringLines = _repository.CountRecurringDueBetween(today, forwardEnd),
                ProjectedPredictedOrders = _repository.CountPredictedOrdersBetween(today, forwardEnd)
            };

            var coffeeCurrent = _repository.CoffeeByItemDelivered(currentFrom, currentTo, 80);
            var coffeePrevious = _repository.CoffeeByItemDelivered(previousFrom, previousTo, 80);
            vm.CoffeeByItemCompare = BuildCoffeeItemCompare(coffeeCurrent, coffeePrevious, 30);

            vm.Panels = BuildPanels(vm.Current, vm.Previous);
            return vm;
        }

        private static List<ActivityCoffeeByItemCompareRow> BuildCoffeeItemCompare(
            List<ActivityCoffeeByItemRow> current,
            List<ActivityCoffeeByItemRow> previous,
            int maxRows)
        {
            var dict = new Dictionary<string, ActivityCoffeeByItemCompareRow>(StringComparer.OrdinalIgnoreCase);

            void AddQty(IEnumerable<ActivityCoffeeByItemRow> rows, bool isCurrent)
            {
                if (rows == null)
                    return;
                foreach (var r in rows)
                {
                    string key = string.IsNullOrWhiteSpace(r.ItemDesc) ? "(unknown)" : r.ItemDesc.Trim();
                    if (!dict.TryGetValue(key, out var row))
                    {
                        row = new ActivityCoffeeByItemCompareRow { ItemDesc = key };
                        dict[key] = row;
                    }

                    if (isCurrent)
                        row.CurrentQty = r.Qty;
                    else
                        row.PreviousQty = r.Qty;
                }
            }

            AddQty(current, true);
            AddQty(previous, false);

            var list = dict.Values
                .OrderByDescending(r => Math.Max(r.CurrentQty, r.PreviousQty))
                .ThenBy(r => r.ItemDesc, StringComparer.OrdinalIgnoreCase)
                .Take(maxRows)
                .ToList();

            foreach (var row in list)
            {
                row.DeltaText = FormatDelta((decimal)row.CurrentQty, (decimal)row.PreviousQty);
                double max = Math.Max(row.CurrentQty, row.PreviousQty);
                row.BarCurrentPercent = BarPercentDouble(row.CurrentQty, max);
                row.BarPreviousPercent = BarPercentDouble(row.PreviousQty, max);
            }

            return list;
        }

        private static int BarPercentDouble(double value, double max)
        {
            if (max <= 0 || value <= 0)
                return 0;
            return Math.Min(100, (int)Math.Round(100.0 * value / max));
        }

        private static string FormatRange(DateTime from, DateTime to)
        {
            return from.ToString("d MMM yyyy", CultureInfo.InvariantCulture)
                + " – "
                + to.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
        }

        private static List<ActivityMetricPanel> BuildPanels(ActivityPeriodSnapshot current, ActivityPeriodSnapshot previous)
        {
            var panels = new List<ActivityMetricPanel>
            {
                Panel("ordersAdded", "Orders added", "New orders by order date", current.OrdersAdded, previous.OrdersAdded, false),
                Panel("ordersDone", "Orders completed", "Orders marked done (required-by date)", current.OrdersCompleted, previous.OrdersCompleted, false),
                Panel("wooImport", "Woo imports", "Tracker orders linked to Woo", current.WooOrdersImported, previous.WooOrdersImported, false),
                Panel("driverDelivered", "Driver: delivered", "Mobile app proof marked Delivered", current.DriverDeliveriesSuccessful, previous.DriverDeliveriesSuccessful, false),
                Panel("driverAttempts", "Driver: attempts", "All mobile delivery proofs", current.DriverDeliveryAttempts, previous.DriverDeliveryAttempts, false),
                Panel("podEmail", "Delivery confirmation emails", "Proof-of-delivery emails (PodSentAt)", current.DeliveryConfirmationEmails, previous.DeliveryConfirmationEmails, false),
                Panel("repairsLogged", "Repairs logged", "New repair tickets", current.RepairsLogged, previous.RepairsLogged, false),
                Panel("repairsDone", "Repairs completed", "Moved to done status", current.RepairsCompleted, previous.RepairsCompleted, false),
                Panel("courier", "Couriered", "Completed via dispatch/courier", current.OrdersCouriered, previous.OrdersCouriered, false),
                Panel("collect", "Collected", "Completed collection orders", current.OrdersCollected, previous.OrdersCollected, false),
                Panel("ownDriver", "Own driver", "Completed, not collection/courier", current.OrdersOwnDriver, previous.OrdersOwnDriver, false),
                Panel("clientMail", "Client mail sent", "Bulk client mail emails", current.ClientMailEmailsSent, previous.ClientMailEmailsSent, false),
                Panel("reminders", "Checkup reminders", "Coffee checkup reminders sent", current.RemindersSent, previous.RemindersSent, false)
            };

            foreach (var p in panels)
                ApplyCompareBars(p);

            return panels;
        }

        public static ActivityMetricPanel BuildCoffeePanel(ActivityPeriodSnapshot current, ActivityPeriodSnapshot previous)
        {
            var panel = Panel("coffee", "Coffee delivered", "Coffee line qty on completed orders",
                (decimal)current.CoffeeUnitsDelivered, (decimal)previous.CoffeeUnitsDelivered, useDecimalFormat: true);
            ApplyCompareBars(panel);
            return panel;
        }

        private static ActivityMetricPanel Panel(string key, string label, string hint,
            decimal current, decimal previous, bool useDecimalFormat)
        {
            return new ActivityMetricPanel
            {
                MetricKey = key,
                Label = label,
                Hint = hint,
                UseDecimalFormat = useDecimalFormat,
                CurrentValue = current,
                PreviousValue = previous,
                DeltaText = FormatDelta(current, previous)
            };
        }

        private static ActivityMetricPanel Panel(string key, string label, string hint, int current, int previous, bool useDecimalFormat)
            => Panel(key, label, hint, (decimal)current, (decimal)previous, useDecimalFormat);

        private static void ApplyCompareBars(ActivityMetricPanel row)
        {
            decimal max = Math.Max(row.CurrentValue, row.PreviousValue);
            if (max <= 0)
            {
                row.BarCurrentPercent = 0;
                row.BarPreviousPercent = 0;
                return;
            }

            row.BarCurrentPercent = BarPercent(row.CurrentValue, max);
            row.BarPreviousPercent = BarPercent(row.PreviousValue, max);
        }

        private static int BarPercent(decimal value, decimal max)
        {
            if (max <= 0 || value <= 0)
                return 0;
            return Math.Min(100, (int)Math.Round(100m * value / max));
        }

        private static string FormatDelta(decimal current, decimal previous)
        {
            if (previous <= 0 && current <= 0)
                return "No change";
            if (previous <= 0)
                return "Up from none prior period";
            decimal diff = current - previous;
            decimal pct = 100m * diff / previous;
            string sign = diff >= 0 ? "+" : string.Empty;
            bool decimals = current != decimal.Truncate(current) || previous != decimal.Truncate(previous)
                || Math.Abs(diff) < 1m;
            string n = decimals ? "N2" : "N0";
            string p = decimals ? "N1" : "N0";
            return string.Format(CultureInfo.InvariantCulture, "{0}{1:" + n + "} ({2}{3:" + p + "}% vs prior)", sign, diff, sign, pct);
        }
    }
}