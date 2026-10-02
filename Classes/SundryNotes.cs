using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// ZZName (sundry walk-in) orders keep the person in the order notes:
    /// "Name: address, goods [#email#] [#41405: Yoco] [RepairStatus: …]".
    /// The name is before the first colon; the address / goods follow it; [#…#] tags hold the email and Woo references.
    /// </summary>
    public static class SundryNotes
    {
        private static readonly Regex TagPattern = new Regex(@"\[#.*?#\]", RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex EmailTagPattern = new Regex(@"\[#\s*([^\s\[\]#]+@[^\s\[\]#]+?)\s*#\]", RegexOptions.Compiled);
        private static readonly Regex Spaces = new Regex(@"[ \t]{2,}", RegexOptions.Compiled);

        /// <summary>The walk-in's name: the notes before the first colon (or before [RepairStatus: …]), without [#…#] tags.</summary>
        public static string Name(string notes)
        {
            string text = WithoutTags(notes);
            int status = StatusIndex(text);
            if (status >= 0)
                text = text.Substring(0, status);
            int colon = text.IndexOf(':');
            if (colon >= 0)
                text = text.Substring(0, colon);
            return Tidy(text);
        }

        /// <summary>What follows the name (address, goods, references) without [#…#] tags, lines joined with ", ".</summary>
        public static string Details(string notes)
        {
            string text = WithoutTags(notes);
            int status = StatusIndex(text);
            int colon = text.IndexOf(':');
            if (status >= 0 && (colon < 0 || status < colon))
                text = text.Substring(status);
            else if (colon >= 0)
                text = text.Substring(colon + 1);
            else
                return string.Empty;

            string joined = string.Join(", ", text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Tidy)
                .Where(line => line.Length > 0));
            return Tidy(joined);
        }

        /// <summary>The email in a [#name@example.com#] tag, else empty ([#41405: Yoco] style references are skipped).</summary>
        public static string Email(string notes)
        {
            if (string.IsNullOrEmpty(notes))
                return string.Empty;
            Match match = EmailTagPattern.Match(notes);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private static string WithoutTags(string notes)
        {
            return string.IsNullOrEmpty(notes) ? string.Empty : TagPattern.Replace(notes, " ");
        }

        private static int StatusIndex(string text)
        {
            return text.IndexOf(SystemConstants.RepairConstants.OrderNotesRepairStatusStartTag, StringComparison.OrdinalIgnoreCase);
        }

        private static string Tidy(string text)
        {
            return Spaces.Replace(text ?? string.Empty, " ").Trim().Trim(',', ';', '|', '-').Trim();
        }
    }
}
