using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using TrackerSQL.Models;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Renders an order flag (InvoiceTypesTbl row) as a status-badge pill using the
    /// PillLabel / PillColour / PillIcon configured in Lookups → Invoice Types.
    /// With an icon the pill is a small round badge and the label is its tooltip.
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

            return Html(invoiceType.PillLabel, invoiceType.PillColour, invoiceType.PillIcon);
        }

        public static string Html(string label, string colour, string icon = null)
        {
            if (string.IsNullOrWhiteSpace(label))
                return string.Empty;

            string trimmedColour = colour?.Trim();
            bool hasColour = !string.IsNullOrEmpty(trimmedColour) && SafeColour.IsMatch(trimmedColour);
            string encodedLabel = HttpUtility.HtmlEncode(label.Trim());

            if (!string.IsNullOrWhiteSpace(icon))
            {
                string iconStyle = hasColour ? " style=\"border-color:" + trimmedColour + "\"" : string.Empty;
                return "<span class=\"status-badge order-flag-pill is-icon\"" + iconStyle
                    + " title=\"" + HttpUtility.HtmlAttributeEncode(label.Trim()) + "\" aria-label=\"" + HttpUtility.HtmlAttributeEncode(label.Trim()) + "\">"
                    + HttpUtility.HtmlEncode(icon.Trim()) + "</span>";
            }

            string style = hasColour
                ? " style=\"background-color:" + trimmedColour + ";border-color:" + trimmedColour + "\""
                : string.Empty;
            return "<span class=\"status-badge order-flag-pill\"" + style + ">" + encodedLabel + "</span>";
        }

        /// <summary>Key for the delivery sheet: each flag that has an icon, as "icon label".</summary>
        public static string LegendHtml(IEnumerable<InvoiceType> invoiceTypes)
        {
            var withIcons = (invoiceTypes ?? Enumerable.Empty<InvoiceType>())
                .Where(t => HasPill(t) && !string.IsNullOrWhiteSpace(t.PillIcon) && (t.Enabled ?? true))
                .OrderBy(t => t.InvoiceTypeID)
                .ToList();
            if (withIcons.Count == 0)
                return string.Empty;

            var html = new StringBuilder("<span class=\"order-flag-legend\">");
            foreach (var invoiceType in withIcons)
            {
                html.Append("<span class=\"order-flag-legend-item\">")
                    .Append(Html(invoiceType))
                    .Append(HttpUtility.HtmlEncode(invoiceType.PillLabel.Trim()))
                    .Append("</span>");
            }
            html.Append("</span>");
            return html.ToString();
        }

        public static bool IsValidColour(string colour)
        {
            return string.IsNullOrWhiteSpace(colour) || SafeColour.IsMatch(colour.Trim());
        }
    }
}
