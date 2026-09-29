using System.Text.RegularExpressions;
using System.Web;
using TrackerSQL.Models;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Renders an order flag (InvoiceTypesTbl row) as a status-badge pill using the
    /// PillLabel / PillColour configured in Lookups → Invoice Types.
    /// </summary>
    public static class OrderFlagPill
    {
        private static readonly Regex SafeColour = new Regex(
            @"^(#[0-9a-fA-F]{3}|#[0-9a-fA-F]{6}|[a-zA-Z]{3,20})$", RegexOptions.Compiled);

        /// <summary>True when the flag should show a pill (it has a pill label).</summary>
        public static bool HasPill(InvoiceType invoiceType)
        {
            return invoiceType != null && !string.IsNullOrWhiteSpace(invoiceType.PillLabel);
        }

        /// <summary>
        /// Pill HTML, or empty when the flag has no pill label (e.g. Standard).
        /// The colour is only emitted when it is a plain hex / named colour.
        /// </summary>
        public static string Html(InvoiceType invoiceType)
        {
            if (!HasPill(invoiceType))
                return string.Empty;

            return Html(invoiceType.PillLabel, invoiceType.PillColour, invoiceType.InvoiceTypeDesc);
        }

        public static string Html(string label, string colour, string title = null)
        {
            if (string.IsNullOrWhiteSpace(label))
                return string.Empty;

            string style = string.Empty;
            string trimmedColour = colour?.Trim();
            if (!string.IsNullOrEmpty(trimmedColour) && SafeColour.IsMatch(trimmedColour))
                style = " style=\"background-color:" + trimmedColour + ";border-color:" + trimmedColour + "\"";

            string titleAttr = string.IsNullOrWhiteSpace(title)
                ? string.Empty
                : " title=\"" + HttpUtility.HtmlAttributeEncode(title) + "\"";

            return "<span class=\"status-badge order-flag-pill\"" + style + titleAttr + ">"
                + HttpUtility.HtmlEncode(label.Trim()) + "</span>";
        }

        public static bool IsValidColour(string colour)
        {
            return string.IsNullOrWhiteSpace(colour) || SafeColour.IsMatch(colour.Trim());
        }
    }
}
