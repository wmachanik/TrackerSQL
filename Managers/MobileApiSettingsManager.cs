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
                NotesToOffice = s.AllowNotesToOffice && !string.IsNullOrWhiteSpace(OfficeEmail(s))
            };
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
