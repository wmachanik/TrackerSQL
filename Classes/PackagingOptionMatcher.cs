using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrackerSQL.Models;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Matches Woo attribute options (e.g. "Plunger grind") to Tracker packaging
    /// rows whose desc/symbol is abbreviated (e.g. GrndPlnger).
    /// </summary>
    public static class PackagingOptionMatcher
    {
        private static readonly Dictionary<string, string> TokenAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "grind", "grnd" },
            { "ground", "grnd" },
            { "plunger", "plnger" },
            { "french", "plnger" },
            { "espresso", "espr" },
            { "filter", "filtr" },
            { "aeropress", "aero" },
            { "stove", "stove" },
            { "stovetop", "stove" },
            { "whole", "whole" },
            { "beans", "bean" },
            { "bean", "bean" },
            { "packet", "pkt" },
            { "pack", "pkt" },
            { "bag", "bag" },
            { "box", "bx" },
            { "bx", "bx" }
        };

        /// <summary>
        /// Best packaging ID for a Woo option text, or null when nothing scores well.
        /// </summary>
        public static int? SuggestPackagingId(string option, IList<ItemPackaging> packagings)
        {
            if (string.IsNullOrWhiteSpace(option) || packagings == null || packagings.Count == 0)
                return null;

            // Weight-only options (1kg Packet) must not steal packaging — qty channel owns those.
            if (PackagingWeightParser.TryParseQtyFactorFromOption(option).HasValue)
                return null;

            string opt = option.Trim();
            string optNorm = Normalize(opt);
            var optTokens = Tokenize(opt);

            int bestId = 0;
            int bestScore = 0;

            foreach (var p in packagings)
            {
                string desc = p.ItemPackagingDesc ?? string.Empty;
                string symbol = p.Symbol ?? string.Empty;
                if (string.IsNullOrWhiteSpace(desc) && string.IsNullOrWhiteSpace(symbol))
                    continue;

                int score = ScoreCandidate(opt, optNorm, optTokens, desc, symbol);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestId = p.ItemPackagingID;
                }
            }

            // Require a real token/abbrev hit (not a weak single-letter coincidence).
            return bestScore >= 40 ? (int?)bestId : null;
        }

        private static int ScoreCandidate(
            string opt,
            string optNorm,
            List<string> optTokens,
            string desc,
            string symbol)
        {
            int score = 0;
            string descNorm = Normalize(desc);
            string symNorm = Normalize(symbol);
            string haystack = desc + " " + symbol;
            string hayNorm = Normalize(haystack);

            if (!string.IsNullOrEmpty(desc)
                && string.Equals(desc.Trim(), opt, StringComparison.OrdinalIgnoreCase))
                score += 100;
            if (!string.IsNullOrEmpty(symbol)
                && string.Equals(symbol.Trim(), opt, StringComparison.OrdinalIgnoreCase))
                score += 100;

            if (!string.IsNullOrEmpty(optNorm) && optNorm.Length >= 3)
            {
                if (descNorm.IndexOf(optNorm, StringComparison.Ordinal) >= 0
                    || symNorm.IndexOf(optNorm, StringComparison.Ordinal) >= 0
                    || hayNorm.IndexOf(optNorm, StringComparison.Ordinal) >= 0)
                    score += 70;
                if (optNorm.IndexOf(descNorm, StringComparison.Ordinal) >= 0 && descNorm.Length >= 3)
                    score += 50;
                if (optNorm.IndexOf(symNorm, StringComparison.Ordinal) >= 0 && symNorm.Length >= 3)
                    score += 50;
            }

            if ((!string.IsNullOrEmpty(desc) && desc.IndexOf(opt, StringComparison.OrdinalIgnoreCase) >= 0)
                || (!string.IsNullOrEmpty(symbol) && symbol.IndexOf(opt, StringComparison.OrdinalIgnoreCase) >= 0))
                score += 60;

            var packTokens = Tokenize(desc + " " + symbol);
            if (optTokens.Count > 0 && packTokens.Count > 0)
            {
                int hits = optTokens.Count(t => packTokens.Any(pt => TokensMatch(t, pt)));
                if (hits > 0)
                    score += 25 * hits;
                // All meaningful option tokens covered (e.g. plunger+grind → grnd+plnger).
                if (hits == optTokens.Count && optTokens.Count >= 1)
                    score += 30;
            }

            return score;
        }

        private static bool TokensMatch(string a, string b)
        {
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                return true;
            if (a.Length >= 3 && b.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (b.Length >= 3 && a.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            return false;
        }

        private static List<string> Tokenize(string text)
        {
            var raw = (text ?? string.Empty)
                .Split(new[] { ' ', '/', '-', '_', ',', '.', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>();
            foreach (string r in raw)
            {
                string t = r.Trim().ToLowerInvariant();
                if (t.Length == 0)
                    continue;
                string alias;
                if (TokenAliases.TryGetValue(t, out alias))
                    t = alias;
                // Skip noise words.
                if (t == "type" || t == "the" || t == "and" || t == "or")
                    continue;
                if (!list.Contains(t))
                    list.Add(t);
            }
            return list;
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var sb = new StringBuilder(text.Length);
            foreach (char c in text.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }
            string compact = sb.ToString();

            // Apply multi-char alias replacements on the compact form.
            compact = compact.Replace("grind", "grnd");
            compact = compact.Replace("plunger", "plnger");
            compact = compact.Replace("espresso", "espr");
            compact = compact.Replace("filter", "filtr");
            compact = compact.Replace("aeropress", "aero");
            compact = compact.Replace("wholebeans", "wholebean");
            compact = compact.Replace("packet", "pkt");
            return compact;
        }
    }
}
