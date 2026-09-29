using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Small pill with an area code for the delivery sheet (System Preferences → General).
    /// Names of two or more words use the first letter of each word, up to three (East London → el).
    /// One-word names keep their first letter, then the next consonants, up to three letters
    /// (Constantia → cns, Athlone → ath); names with too few consonants are topped up with their other letters.
    /// An area's own ShortCode (Lookups → Areas) replaces it and is shown as typed (Durban → Dbn).
    /// The full area name is the tooltip. Screen only.
    /// </summary>
    public static class AreaPill
    {
        private const string Vowels = "aeiou";

        public static string Abbreviation(string areaName)
        {
            string name = (areaName ?? string.Empty).ToLowerInvariant();

            var words = Regex.Split(name, "[^\\p{L}]+").Where(w => w.Length > 0).ToList();
            if (words.Count >= 2)
                return new string(words.Take(3).Select(w => w[0]).ToArray());

            var letters = name.Where(char.IsLetter).ToList();
            var code = new StringBuilder(3);
            var used = new bool[letters.Count];

            if (letters.Count > 0)
            {
                code.Append(letters[0]);
                used[0] = true;
            }

            for (int i = 1; i < letters.Count && code.Length < 3; i++)
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

        /// <summary>The area's own short code when set, else the automatic abbreviation.</summary>
        public static string Code(string areaName, string shortCode)
        {
            return string.IsNullOrWhiteSpace(shortCode) ? Abbreviation(areaName) : shortCode.Trim();
        }

        /// <summary>Pill HTML, or empty when there is no area name or code.</summary>
        public static string Html(string areaName, string shortCode = null)
        {
            string code = Code(areaName, shortCode);
            if (code.Length == 0)
                return string.Empty;

            string title = HttpUtility.HtmlAttributeEncode("Area: " + (areaName ?? string.Empty).Trim());
            return "<span class=\"status-badge area-pill\" title=\"" + title + "\" aria-label=\"" + title + "\">"
                + HttpUtility.HtmlEncode(code) + "</span>";
        }
    }
}
