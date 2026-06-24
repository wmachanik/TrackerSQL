using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Represents items required data for preparation/delivery summaries
    /// </summary>
    public class ItemsRequiredSummary
    {
        /// <summary>
        /// Preparation (roasting) date or required-by date
        /// </summary>
        public DateTime? PrepDate { get; set; }

        /// <summary>
        /// Required-by date (for delivery-based summary)
        /// </summary>
        public DateTime? RequiredByDate { get; set; }

        /// <summary>
        /// Item description
        /// </summary>
        public string ItemDesc { get; set; }

        /// <summary>
        /// Abbreviation of delivery person (for delivery-based summary)
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Quantity required
        /// </summary>
        public double Qty { get; set; }
    }
}