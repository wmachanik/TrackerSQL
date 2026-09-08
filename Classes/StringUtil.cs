using System;

namespace TrackerSQL.Classes
{
    public static class StringUtil
    {
        public static string Truncate(string value, int maxLen, bool appendEllipsis = false)
        {
            if (string.IsNullOrEmpty(value) || maxLen <= 0)
                return value ?? string.Empty;
            value = value.Replace("\r", " ").Replace("\n", " ").Trim();
            if (value.Length <= maxLen)
                return value;
            if (!appendEllipsis)
                return value.Substring(0, maxLen);
            int keep = Math.Max(0, maxLen - 1);
            return value.Substring(0, keep) + "…";
        }

        public static bool NormEquals(string a, string b)
        {
            return string.Equals(
                (a ?? string.Empty).Trim(),
                (b ?? string.Empty).Trim(),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
