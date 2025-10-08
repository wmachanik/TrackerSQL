using System;
using System.Web;

namespace TrackerDotNet.Classes
{
    public static class UrlTextDecoder
    {
        /// <summary>
        /// Idempotent-ish decode: handles strings that may already be decoded or partially encoded.
        /// Also converts literal \r\n sequences to real newlines.
        /// </summary>
        public static string DecodePossiblyUrlEncoded(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // Fast path: if it contains '+' or %xx or literal \r\n, try decoding.
            bool looksEncoded = input.IndexOf('+') >= 0 || input.IndexOf('%') >= 0 || input.Contains("\\r\\n");

            string working = input;

            if (looksEncoded)
            {
                try
                {
                    // First change plus to space (UrlDecode would also do this but only if treated as encoded).
                    working = working.Replace("+", " ");
                    working = HttpUtility.UrlDecode(working);
                }
                catch { /* swallow; fall back to original */ }
            }

            // Convert literal escape sequences to real newlines if present
            if (working != null && working.Contains("\\r\\n"))
                working = working.Replace("\\r\\n", "\r\n");

            return working;
        }
    }
}