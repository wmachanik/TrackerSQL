using System;

namespace TrackerSQL.Models
{
    [Serializable]
    public class RecurringOrderItem
    {
        public int RecurringOrderItemID { get; set; }
        public int RecurringOrderID { get; set; }
    public int? RecurringTypeID { get; set; }
    public int? Value { get; set; }
        public int? ItemRequiredID { get; set; }
        public double? QtyRequired { get; set; }
    public DateTime? DateLastDone { get; set; }
    public DateTime? NextDateRequired { get; set; }
    public DateTime? RequireUntilDate { get; set; }
        public int? ItemPackagingID { get; set; }
    public string RecurringTypeDesc { get; set; }
    public string ItemDesc { get; set; }
    public string ItemPackagingDesc { get; set; }

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

    public string ItemDisplay
    {
        get
        {
            var parts = new[]
            {
                ItemDesc,
                QtyRequired.HasValue ? QtyRequired.Value.ToString("0.##") : string.Empty,
                ItemPackagingDesc
            };

            return string.Join(" ", parts).Trim();
        }
    }
    }
}
