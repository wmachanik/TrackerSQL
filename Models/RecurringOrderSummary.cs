using System.Collections.Generic;
using System.Linq;
using System;
using TrackerSQL.Classes;

namespace TrackerSQL.Models
{
    public class RecurringOrderSummary
    {
        public int RecurringOrderID { get; set; }
        public int RecurringOrderItemID { get; set; }
        public int? ContactID { get; set; }
        public string CompanyName { get; set; }
        public int? DeliveryByID { get; set; }
        public string DeliveryByDisplay { get; set; }
        public int? RecurringTypeID { get; set; }
        public string RecurringTypeDesc { get; set; }
        public int? Value { get; set; }
        public System.DateTime? DateLastDone { get; set; }
        public System.DateTime? NextDateRequired { get; set; }
        public System.DateTime? RequireUntilDate { get; set; }
        public int? ItemRequiredID { get; set; }
        public string ItemDesc { get; set; }
        public double? QtyRequired { get; set; }
        public int? ItemPackagingID { get; set; }
        public string ItemPackagingDesc { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }

        public string DetailsNavigateUrl
        {
            get { return SystemConstants.PageUrls.RecurringOrderDetailsUrl(RecurringOrderID); }
        }

        public string EnabledDisplay
        {
            get { return Enabled == false ? "disabled" : "enabled"; }
        }

        public string RecurringPatternDisplay
        {
            get
            {
                if (!Value.HasValue && string.IsNullOrWhiteSpace(RecurringTypeDesc))
                {
                    return string.Empty;
                }

                if (!Value.HasValue)
                {
                    return RecurringTypeDesc ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(RecurringTypeDesc))
                {
                    return Value.Value.ToString();
                }

                return Value.Value + " " + RecurringTypeDesc;
            }
        }

        public string ItemsDisplay
        {
            get
            {
                string itemDesc = ItemDesc ?? string.Empty;
                string quantityText = QtyRequired.HasValue
                    ? SystemConstants.FormatConstants.FormatQuantity(QtyRequired.Value)
                    : string.Empty;
                string packagingDescription = ItemPackagingDesc ?? string.Empty;

                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(itemDesc))
                {
                    parts.Add(itemDesc);
                }
                if (!string.IsNullOrWhiteSpace(quantityText))
                {
                    parts.Add(quantityText);
                }
                if (!string.IsNullOrWhiteSpace(packagingDescription))
                {
                    parts.Add(packagingDescription);
                }

                return string.Join(" ", parts.ToArray());
            }
        }
    }

    public class RecurringOrderContactSummary
    {
        public int? ContactID { get; set; }
        public string CompanyName { get; set; }
        public List<RecurringOrderSummary> RecurringOrders { get; set; } = new List<RecurringOrderSummary>();

        public string CompanyNameDisplay
        {
            get { return string.IsNullOrWhiteSpace(CompanyName) ? "(no company)" : CompanyName; }
        }

        public bool HasContactLink
        {
            get { return ContactID.HasValue && ContactID.Value > 0; }
        }

        public bool ShowCompanyLabel
        {
            get { return !HasContactLink; }
        }

        public string ContactDetailsNavigateUrl
        {
            get { return HasContactLink ? "~/Pages/ContactDetails.aspx?ID=" + ContactID.Value : string.Empty; }
        }

        public DateTime? NextDateRequired
        {
            get
            {
                var nextDates = RecurringOrders == null
                    ? new List<DateTime>()
                    : RecurringOrders.Where(recurringOrder => recurringOrder != null && recurringOrder.NextDateRequired.HasValue)
                        .Select(recurringOrder => recurringOrder.NextDateRequired.Value)
                        .ToList();

                return nextDates.Count == 0 ? (DateTime?)null : nextDates.Min();
            }
        }

        public int RecurringOrderCount
        {
            get
            {
                return RecurringOrders == null
                    ? 0
                    : RecurringOrders.Select(recurringOrder => recurringOrder.RecurringOrderID).Distinct().Count();
            }
        }
    }

    public class RecurringOrderGroupSummary
    {
        public int RecurringOrderID { get; set; }
        public int? ContactID { get; set; }
        public string CompanyName { get; set; }
        public int? DeliveryByID { get; set; }
        public string DeliveryByDisplay { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
        public List<RecurringOrderSummary> RecurringOrderItems { get; set; } = new List<RecurringOrderSummary>();

        public string CompanyNameDisplay
        {
            get { return string.IsNullOrWhiteSpace(CompanyName) ? "(no company)" : CompanyName; }
        }

        public bool HasContactLink
        {
            get { return ContactID.HasValue && ContactID.Value > 0; }
        }

        public bool ShowCompanyLabel
        {
            get { return !HasContactLink; }
        }

        public string ContactDetailsNavigateUrl
        {
            get { return HasContactLink ? "~/Pages/ContactDetails.aspx?ID=" + ContactID.Value : string.Empty; }
        }

        public string DetailsNavigateUrl
        {
            get { return SystemConstants.PageUrls.RecurringOrderDetailsUrl(RecurringOrderID); }
        }

        public string EnabledDisplay
        {
            get { return Enabled == false ? "disabled" : "enabled"; }
        }

        public int RecurringLineCount
        {
            get { return RecurringOrderItems == null ? 0 : RecurringOrderItems.Count; }
        }

        public DateTime? NextDateRequired
        {
            get
            {
                var nextDates = RecurringOrderItems == null
                    ? new List<DateTime>()
                    : RecurringOrderItems.Where(recurringOrder => recurringOrder != null && recurringOrder.NextDateRequired.HasValue)
                        .Select(recurringOrder => recurringOrder.NextDateRequired.Value)
                        .ToList();

                return nextDates.Count == 0 ? (DateTime?)null : nextDates.Min();
            }
        }

        public DateTime? DateLastDone
        {
            get
            {
                var lastDates = RecurringOrderItems == null
                    ? new List<DateTime>()
                    : RecurringOrderItems.Where(recurringOrder => recurringOrder != null && recurringOrder.DateLastDone.HasValue)
                        .Select(recurringOrder => recurringOrder.DateLastDone.Value)
                        .ToList();

                return lastDates.Count == 0 ? (DateTime?)null : lastDates.Min();
            }
        }
    }
}
