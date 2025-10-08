using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrackerDotNet.Controls;

namespace TrackerDotNet.Classes
{
    public static class EmailUtils
    {
        public static string GetFriendlyContactName(ContactEmailDetails details)
        {
            var names = new List<string>();

            if (!string.IsNullOrWhiteSpace(details.FirstName))
                names.Add(details.FirstName.Trim());

            if (!string.IsNullOrWhiteSpace(details.altFirstName))
                names.Add(details.altFirstName.Trim());

            return names.Count > 0 ? string.Join(" and ", names) : "Coffee Lover";
        }

        public static string CleanNoteText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";

            // Strip prefix before colon
            if (raw.Contains(":"))
                raw = raw.Substring(raw.IndexOf(":") + 1).Trim();

            // Remove embedded marker [#...] if present
            int start = raw.IndexOf("[#");
            int end = raw.IndexOf("#]");

            if (start >= 0 && end > start)
            {
                string before = raw.Substring(0, start).Trim();
                string after = raw.Substring(end + 2).Trim();

                raw = string.IsNullOrEmpty(before) ? after :
                      string.IsNullOrEmpty(after) ? before :
                      $"{before}; {after}";
            }

            return raw;
        }
    }

}