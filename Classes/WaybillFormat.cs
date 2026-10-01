using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// A courier's usual waybill formats (CourierServicesTbl.WaybillFormats), one or more separated by ; or new lines.
    /// In a format: @ = a letter, # = a digit, ? = a letter or digit, * = anything; {n} after one of these repeats it
    /// n times and + means one or more; any other character must be there as typed (letters ignore case).
    /// Spaces in the waybill are ignored. Examples: PAR* (Pargo), @?#{10};#@#{10} (Fastway), @+ (letters only).
    /// </summary>
    public static class WaybillFormat
    {
        public const string Help = "@ = a letter, # = a digit, ? = a letter or digit, * = anything, {10} = ten of the one before, + = one or more. Several formats: separate with ;";

        private class Token
        {
            public char Kind;      // '@', '#', '?', '*' or 'L' (literal text)
            public string Text;    // literal text
            public int Count = 1;  // exact count; 0 with Many = one or more
            public bool Many;
        }

        public static List<string> Split(string formats)
        {
            return (formats ?? string.Empty)
                .Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => f.Length > 0)
                .ToList();
        }

        /// <summary>The waybill as compared: spaces removed, upper case.</summary>
        public static string Normalise(string waybill)
        {
            return Regex.Replace(waybill ?? string.Empty, @"\s+", string.Empty).ToUpperInvariant();
        }

        /// <summary>True when there are no formats or the waybill matches one of them.</summary>
        public static bool Matches(string waybill, string formats)
        {
            List<string> list = Split(formats);
            if (list.Count == 0)
                return true;
            string value = Normalise(waybill);
            foreach (string f in list)
            {
                if (TryParse(f, out List<Token> tokens, out _) && Regex.IsMatch(value, ToRegex(tokens), RegexOptions.CultureInvariant))
                    return true;
            }
            return false;
        }

        /// <summary>Null when every format can be read, otherwise what is wrong (for Lookups → Couriers).</summary>
        public static string Check(string formats)
        {
            foreach (string f in Split(formats))
            {
                if (!TryParse(f, out _, out string error))
                    return "'" + f + "': " + error;
            }
            return null;
        }

        /// <summary>The formats in words, e.g. "PAR, then anything" or "a letter, a letter or digit, then 10 digits".</summary>
        public static string Describe(string formats)
        {
            var parts = new List<string>();
            foreach (string f in Split(formats))
            {
                if (TryParse(f, out List<Token> tokens, out _))
                    parts.Add(DescribeTokens(tokens));
            }
            return string.Join("; or ", parts);
        }

        private static bool TryParse(string format, out List<Token> tokens, out string error)
        {
            tokens = new List<Token>();
            error = null;
            int i = 0;
            while (i < format.Length)
            {
                char c = format[i];
                if (c == '{' || c == '+')
                {
                    Token last = tokens.LastOrDefault();
                    if (last == null || last.Kind == '*')
                    {
                        error = c + " must follow @, #, ? or a character";
                        return false;
                    }
                    if (last.Kind == 'L' && last.Text.Length > 1)
                    {
                        string tail = last.Text.Substring(last.Text.Length - 1);
                        last.Text = last.Text.Substring(0, last.Text.Length - 1);
                        last = new Token { Kind = 'L', Text = tail };
                        tokens.Add(last);
                    }
                    if (c == '+')
                    {
                        last.Many = true;
                        i++;
                        continue;
                    }
                    int close = format.IndexOf('}', i);
                    if (close < 0 || !int.TryParse(format.Substring(i + 1, close - i - 1), NumberStyles.None, CultureInfo.InvariantCulture, out int n) || n < 1 || n > 50)
                    {
                        error = "write a count like {10}";
                        return false;
                    }
                    last.Count = n;
                    i = close + 1;
                    continue;
                }
                if (c == '@' || c == '#' || c == '?' || c == '*')
                {
                    tokens.Add(new Token { Kind = c });
                }
                else if (!char.IsWhiteSpace(c))
                {
                    Token last = tokens.LastOrDefault();
                    if (last != null && last.Kind == 'L' && last.Count == 1 && !last.Many)
                        last.Text += char.ToUpperInvariant(c);
                    else
                        tokens.Add(new Token { Kind = 'L', Text = char.ToUpperInvariant(c).ToString() });
                }
                i++;
            }
            if (tokens.Count == 0)
            {
                error = "empty";
                return false;
            }
            return true;
        }

        private static string ToRegex(List<Token> tokens)
        {
            var sb = new StringBuilder("^");
            foreach (Token t in tokens)
            {
                string unit;
                switch (t.Kind)
                {
                    case '@': unit = "[A-Z]"; break;
                    case '#': unit = "[0-9]"; break;
                    case '?': unit = "[A-Z0-9]"; break;
                    case '*': sb.Append(".*"); continue;
                    default: unit = t.Text.Length == 1 ? Regex.Escape(t.Text) : "(?:" + Regex.Escape(t.Text) + ")"; break;
                }
                sb.Append(unit);
                if (t.Many) sb.Append('+');
                else if (t.Count > 1) sb.Append('{').Append(t.Count).Append('}');
            }
            return sb.Append('$').ToString();
        }

        private static string DescribeTokens(List<Token> tokens)
        {
            var words = new List<string>();
            foreach (Token t in tokens)
            {
                string one, many;
                switch (t.Kind)
                {
                    case '@': one = "a letter"; many = "letters"; break;
                    case '#': one = "a digit"; many = "digits"; break;
                    case '?': one = "a letter or digit"; many = "letters or digits"; break;
                    case '*': words.Add("anything"); continue;
                    default: one = t.Text; many = t.Text + " (repeated)"; break;
                }
                if (t.Many) words.Add(many);
                else if (t.Count > 1) words.Add(t.Count + " " + (t.Kind == 'L' ? "× " + t.Text : many));
                else words.Add(one);
            }
            if (words.Count == 1)
                return words[0];
            return string.Join(", ", words.Take(words.Count - 1)) + ", then " + words[words.Count - 1];
        }
    }
}
