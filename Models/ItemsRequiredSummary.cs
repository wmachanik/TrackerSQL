using System;
using TrackerSQL.Classes;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Represents items required data for preparation/delivery summaries.
    /// Qty is kg ordered; PackQty is how many packs that equals for the packaging size.
    /// </summary>
    public class ItemsRequiredSummary
    {
        public DateTime? PrepDate { get; set; }

        public DateTime? RequiredByDate { get; set; }

        public string ItemDesc { get; set; }

        /// <summary>
        /// Packaging description (e.g. 250g bag). Missing packaging is treated as 1kg bag.
        /// </summary>
        public string ItemPackagingDesc { get; set; }

        public string Abbreviation { get; set; }

        /// <summary>
        /// Quantity ordered in kg (order-line total weight).
        /// </summary>
        public double Qty { get; set; }

        /// <summary>
        /// Pack size in kg parsed from <see cref="ItemPackagingDesc"/> (default 1kg).
        /// </summary>
        public double PackWeightKg
        {
            get { return PackagingWeightParser.ParsePackWeightKg(ItemPackagingDesc); }
        }

        /// <summary>
        /// Number of packs required: Qty (kg) / pack weight.
        /// Example: 0.5 kg ÷ 0.25 kg (250g) = 2 packs.
        /// </summary>
        public double PackQty
        {
            get { return PackagingWeightParser.ComputePackQty(Qty, ItemPackagingDesc); }
        }
    }
}
