using System.Web;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// HTML status icons for diagnostics UI (uses imgButtons PNGs — no emoji fonts).
    /// </summary>
    public static class DiagnosticsIconHelper
    {
        private const string IconBase = "~/images/imgButtons/";

        public static string Success => Image("Yes.png", "Success");
        public static string Error => Image("Stop sign.png", "Error");
        public static string Warning => Image("Warning.png", "Warning");
        public static string Info => Image("View.png", "Info");

        public static string Image(string fileName, string alt)
        {
            string url = VirtualPathUtility.ToAbsolute(IconBase + fileName);
            return $"<img src=\"{url}\" alt=\"{HttpUtility.HtmlAttributeEncode(alt)}\" class=\"diag-icon\" />";
        }

        public static string FormatDiagnosticsHtml(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return string.Empty;

            var lines = plainText.Replace("\r\n", "\n").Split('\n');
            var html = new System.Text.StringBuilder();

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();
                if (line.Length == 0)
                {
                    html.Append("<br />");
                    continue;
                }

                string icon;
                string text = line;

                if (line.StartsWith("[OK]"))
                {
                    icon = Success;
                    text = line.Substring(4).TrimStart();
                }
                else if (line.StartsWith("[FAIL]"))
                {
                    icon = Error;
                    text = line.Substring(6).TrimStart();
                }
                else if (line.StartsWith("[WARN]"))
                {
                    icon = Warning;
                    text = line.Substring(6).TrimStart();
                }
                else
                {
                    icon = string.Empty;
                }

                html.Append(icon);
                if (!string.IsNullOrEmpty(icon))
                    html.Append(' ');
                html.Append(HttpUtility.HtmlEncode(text));
                html.Append("<br />");
            }

            return html.ToString();
        }
    }
}
