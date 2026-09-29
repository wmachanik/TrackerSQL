using System.Linq;
using System.Text;
using System.Web;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Small pill with a three-letter area code for the delivery sheet (System Preferences → General).
    /// The code is the first three consonants of the area name (Constantia → cns); names with fewer
    /// consonants are topped up with their other letters. The full area name is the tooltip.
    /// </summary>
    public static class AreaPill
    {
        private const string Vowels = "aeiou";

        public static string Abbreviation(string areaName)
        {
            var letters = (areaName ?? string.Empty).ToLowerInvariant().Where(char.IsLetter).ToList();
            var code = new StringBuilder(3);
            var used = new bool[letters.Count];

            for (int i = 0; i < letters.Count && code.Length < 3; i++)
            {
                if (Vowels.IndexOf(letters[i]) < 0)
                {
                    code.Append(letters[i]);
                    used[i] = true;
                }
            }

            for (int i = 0; i < letters.Count && code.Length < 3; i++)
            {
                if (!used[i])
                    code.Append(letters[i]);
            }

            return code.ToString();
        }

        /// <summary>Pill HTML, or empty when there is no area name.</summary>
        public static string Html(string areaName)
        {
            string code = Abbreviation(areaName);
            if (code.Length == 0)
                return string.Empty;

            string title = HttpUtility.HtmlAttributeEncode("Area: " + areaName.Trim());
            return "<span class=\"status-badge area-pill\" title=\"" + title + "\" aria-label=\"" + title + "\">"
                + HttpUtility.HtmlEncode(code) + "</span>";
        }
    }
}
