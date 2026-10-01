using System;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// System → Driver App settings. Until they are saved, "run Done" follows the old
    /// MobileApi.AutoCompleteDeliveries appSetting and the other options use their defaults.
    /// </summary>
    public static class MobileApiSettingsManager
    {
        private static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(1);
        private static readonly object Sync = new object();
        private static MobileApiSettings _cached;
        private static DateTime _cachedAt;

        public static MobileApiSettings Current
        {
            get
            {
                lock (Sync)
                {
                    if (_cached == null || DateTime.UtcNow - _cachedAt > CacheFor)
                    {
                        _cached = Load(out _);
                        _cachedAt = DateTime.UtcNow;
                    }
                    return _cached;
                }
            }
        }

        /// <summary>Reads the settings from the database; <paramref name="saved"/> is false when defaults are returned.</summary>
        public static MobileApiSettings Load(out bool saved)
        {
            saved = false;
            try
            {
                MobileApiSchemaInstaller.EnsureReady();
                var row = new MobileApiSettingsRepository().Get();
                if (row != null)
                {
                    saved = true;
                    return row;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver App settings could not be read: " + ex.Message);
            }
            return Defaults();
        }

        public static bool Save(MobileApiSettings settings, string updatedBy)
        {
            MobileApiSchemaInstaller.EnsureReady();
            bool ok = new MobileApiSettingsRepository().Save(settings, updatedBy);
            lock (Sync)
                _cached = null;
            return ok;
        }

        /// <summary>Where driver notes are emailed: the configured office address, else the orders address.</summary>
        public static string OfficeEmail(MobileApiSettings settings)
        {
            string email = settings?.OfficeEmail;
            return string.IsNullOrWhiteSpace(email) ? OrdersEmail : email.Trim();
        }

        public static string OrdersEmail => ConfigHelper.GetString("SysCCEmailAddress", string.Empty);

        public static MobileDriverSettings ForApp()
        {
            var s = Current;
            return new MobileDriverSettings
            {
                RunDoneOnDelivery = s.RunDoneOnDelivery,
                SendConfirmation = s.SendDeliveryConfirmation,
                NotesToOffice = s.AllowNotesToOffice && !string.IsNullOrWhiteSpace(OfficeEmail(s)),
                LandlinePrefixes = ParsePrefixes(s.LandlinePrefixes ?? DefaultLandlinePrefixes)
            };
        }

        /// <summary>Used until the setting is saved: Cape Town landlines.</summary>
        public const string DefaultLandlinePrefixes = "021";

        /// <summary>
        /// "021, +27 21; 2711" → ["021", "011"]: digits only, a leading +27 / 0027 / 27 becomes 0, duplicates dropped.
        /// </summary>
        public static System.Collections.Generic.List<string> ParsePrefixes(string text)
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (string raw in (text ?? string.Empty).Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string p = LocalNumber(raw);
                if (p.Length >= 2 && !list.Contains(p))
                    list.Add(p);
            }
            return list;
        }

        /// <summary>A phone number or prefix in local form: "+27 21 555" → "021555", "0027 82" → "082".</summary>
        public static string LocalNumber(string number)
        {
            string s = (number ?? string.Empty).Trim();
            bool plus = s.StartsWith("+");
            string digits = new string(Array.FindAll(s.ToCharArray(), char.IsDigit));
            if (digits.StartsWith("0027"))
                return "0" + digits.Substring(4);
            if ((plus || digits.Length != 0 && !digits.StartsWith("0")) && digits.StartsWith("27"))
                return "0" + digits.Substring(2);
            return digits;
        }

        private static MobileApiSettings Defaults()
        {
            return new MobileApiSettings
            {
                RunDoneOnDelivery = ConfigHelper.GetBool("MobileApi.AutoCompleteDeliveries", false),
                SendDeliveryConfirmation = false,
                AllowNotesToOffice = true
            };
        }
    }
}
