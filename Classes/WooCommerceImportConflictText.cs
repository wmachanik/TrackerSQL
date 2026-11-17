using System;

namespace TrackerSQL.Classes
{
    /// <summary>Parses Woo import conflict snippets for product names (mapping search, Needs Mapping list).</summary>
    public static class WooCommerceImportConflictText
    {
        public static string ProductNameFromConflict(string part)
        {
            string text = (part ?? string.Empty).Trim();
            string prefix = null;
            if (text.StartsWith(SystemConstants.WooCommerceConstants.SkuMissingConflictPrefix, StringComparison.OrdinalIgnoreCase))
                prefix = SystemConstants.WooCommerceConstants.SkuMissingConflictPrefix;
            else if (text.StartsWith(SystemConstants.WooCommerceConstants.UnmappedSkuConflictPrefix, StringComparison.OrdinalIgnoreCase))
                prefix = SystemConstants.WooCommerceConstants.UnmappedSkuConflictPrefix;
            if (prefix == null)
                return null;

            string rest = text.Substring(prefix.Length).Trim();
            if (rest.StartsWith("\"", StringComparison.Ordinal))
            {
                int end = rest.IndexOf('"', 1);
                if (end > 1)
                    return rest.Substring(1, end - 1).Trim();
            }

            int stop = rest.IndexOf('(');
            if (stop < 0)
                stop = rest.IndexOf('.');
            if (stop >= 0)
                rest = rest.Substring(0, stop);
            return rest.Trim().Trim('"');
        }
    }
}