using System;
using System.Configuration;

namespace TrackerDotNet.Classes
{
    /// <summary>
    /// Central helper for reading typed AppSettings values safely.
    /// </summary>
    public static class ConfigHelper
    {
        public static int GetInt(string key, int defaultValue)
        {
            try
            {
                var raw = ConfigurationManager.AppSettings[key];
                int v;
                if (int.TryParse(raw, out v))
                    return v;
            }
            catch { }
            return defaultValue;
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            try
            {
                var raw = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(raw)) return defaultValue;
                return raw.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                       raw.Equals("1") ||
                       raw.Equals("yes", StringComparison.OrdinalIgnoreCase);
            }
            catch { }
            return defaultValue;
        }

        public static string GetString(string key, string defaultValue = "")
        {
            try
            {
                var raw = ConfigurationManager.AppSettings[key];
                return string.IsNullOrEmpty(raw) ? defaultValue : raw;
            }
            catch { return defaultValue; }
        }
    }
}