using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    public class ActivityPeriodSnapshot
    {
        public int Days { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int OrdersAdded { get; set; }
        public int OrdersCompleted { get; set; }
        public int WooOrdersImported { get; set; }

        public int DriverDeliveriesSuccessful { get; set; }
        public int DriverDeliveryAttempts { get; set; }
        public int DeliveryConfirmationEmails { get; set; }

        public int RepairsLogged { get; set; }
        public int RepairsCompleted { get; set; }

        public double CoffeeUnitsDelivered { get; set; }
        public int OrdersCouriered { get; set; }
        public int OrdersCollected { get; set; }
        public int OrdersOwnDriver { get; set; }

        public int ClientMailEmailsSent { get; set; }
        public int RemindersSent { get; set; }
    }

    public class ActivityCoffeeByItemRow
    {
        public string ItemDesc { get; set; }
        public double Qty { get; set; }
    }

    public class ActivityCoffeeByItemCompareRow
    {
        public string ItemDesc { get; set; }
        public double CurrentQty { get; set; }
        public double PreviousQty { get; set; }
        public string DeltaText { get; set; }
        public int BarCurrentPercent { get; set; }
        public int BarPreviousPercent { get; set; }
    }

    public class ActivityMetricPanel
    {
        public string MetricKey { get; set; }
        public string Label { get; set; }
        public string Hint { get; set; }
        public bool UseDecimalFormat { get; set; }

        public decimal CurrentValue { get; set; }
        public decimal PreviousValue { get; set; }
        public int BarCurrentPercent { get; set; }
        public int BarPreviousPercent { get; set; }
        public string DeltaText { get; set; }
    }

    public class ActivityDashboardViewModel
    {
        public DateTime GeneratedAt { get; set; }
        public int SelectedDays { get; set; }
        public bool IsCustomRange { get; set; }
        public string CurrentRangeText { get; set; }
        public string PreviousRangeText { get; set; }
        public string ForwardRangeText { get; set; }

        public ActivityPeriodSnapshot Current { get; set; }
        public ActivityPeriodSnapshot Previous { get; set; }

        public int ProjectedRecurringLines { get; set; }
        public int ProjectedPredictedOrders { get; set; }

        public List<ActivityMetricPanel> Panels { get; set; } = new List<ActivityMetricPanel>();
        public List<ActivityCoffeeByItemCompareRow> CoffeeByItemCompare { get; set; } = new List<ActivityCoffeeByItemCompareRow>();
    }
}