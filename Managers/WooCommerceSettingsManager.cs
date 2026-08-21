using System;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Orchestrates Woo settings, encryption, schema ensure, and connection test.
    /// </summary>
    public class WooCommerceSettingsManager
    {
        private readonly SystemPreferencesHdrRepository _prefsRepo = new SystemPreferencesHdrRepository();
        private readonly WooCommerceSettingsRepository _settingsRepo = new WooCommerceSettingsRepository();
        private readonly WooCommerceSchemaInstaller _schema = new WooCommerceSchemaInstaller();
        private readonly WooCommerceApiClient _api = new WooCommerceApiClient();

        public bool IsSchemaReady()
        {
            return _schema.TablesExist() && _prefsRepo.TableExists() && _settingsRepo.TableExists();
        }

        public WooCommerceSchemaInstaller.EnsureResult EnsureSchema()
        {
            return _schema.EnsureSchema();
        }

        public SystemPreferencesHdr GetPreferencesHeader()
        {
            if (!IsSchemaReady())
                return new SystemPreferencesHdr { PrefsID = 1 };
            return _prefsRepo.GetHeader();
        }

        /// <summary>True when schema exists and WooCommerce integration is enabled.</summary>
        public bool IsIntegrationEnabled()
        {
            try
            {
                return IsSchemaReady() && GetPreferencesHeader().WooCommerceEnabled;
            }
            catch
            {
                return false;
            }
        }

        public WooCommerceSettings GetSettings()
        {
            if (!IsSchemaReady())
                return new WooCommerceSettings { SettingsID = 1 };
            return _settingsRepo.GetSettings();
        }

        public bool HasSavedConsumerKey()
        {
            var s = GetSettings();
            return !string.IsNullOrEmpty(s?.ConsumerKeyEncrypted);
        }

        public bool HasSavedConsumerSecret()
        {
            var s = GetSettings();
            return !string.IsNullOrEmpty(s?.ConsumerSecretEncrypted);
        }

        public void SaveConnectionSettings(
            string storeUrl,
            string adminUrl,
            string consumerKeyPlain,
            string consumerSecretPlain,
            bool replaceKey,
            bool replaceSecret,
            string updatedBy)
        {
            if (!WooCommerceSecretProtector.HasCryptoKeyConfigured())
                throw new InvalidOperationException("WooCommerceCryptoKey is not configured in appSettings.");

            EnsureSchema();
            var settings = _settingsRepo.GetSettings();
            settings.StoreBaseUrl = WooCommerceApiClient.NormalizeStoreBaseUrl(storeUrl);
            string admin = (adminUrl ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(admin) ||
                string.Equals(admin, settings.StoreBaseUrl, StringComparison.OrdinalIgnoreCase))
            {
                admin = WooCommerceApiClient.SuggestAdminUrl(settings.StoreBaseUrl);
            }
            settings.AdminBaseUrl = admin;

            if (replaceKey && !string.IsNullOrWhiteSpace(consumerKeyPlain))
                settings.ConsumerKeyEncrypted = WooCommerceSecretProtector.Encrypt(consumerKeyPlain.Trim());

            if (replaceSecret && !string.IsNullOrWhiteSpace(consumerSecretPlain))
                settings.ConsumerSecretEncrypted = WooCommerceSecretProtector.Encrypt(consumerSecretPlain.Trim());

            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo",
                "Connection settings saved (key replaced=" + replaceKey + ", secret replaced=" + replaceSecret + ")",
                updatedBy);
        }

        public void SaveBehaviourDefaults(
            string categoryFilterMode,
            string dispatchDeliveryPersonIds,
            bool trackingNumberRequired,
            string updatedBy)
        {
            EnsureSchema();
            var settings = _settingsRepo.GetSettings();
            if (!string.IsNullOrWhiteSpace(categoryFilterMode))
                settings.CategoryFilterMode = categoryFilterMode.Trim();
            if (!string.IsNullOrWhiteSpace(dispatchDeliveryPersonIds))
                settings.DispatchDeliveryPersonIds = dispatchDeliveryPersonIds.Trim();
            settings.TrackingNumberRequired = trackingNumberRequired;
            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo", "Behaviour defaults saved", updatedBy);
        }

        public WooCommerceApiClient.ConnectionTestResult TestConnection(
            string storeUrlOverride,
            string consumerKeyOverride,
            string consumerSecretOverride,
            string updatedBy)
        {
            EnsureSchema();
            var settings = _settingsRepo.GetSettings();

            if (!string.IsNullOrWhiteSpace(storeUrlOverride))
                settings.StoreBaseUrl = WooCommerceApiClient.NormalizeStoreBaseUrl(storeUrlOverride);

            if (string.IsNullOrWhiteSpace(settings.StoreBaseUrl))
            {
                var fail = new WooCommerceApiClient.ConnectionTestResult
                {
                    Succeeded = false,
                    Detail = "Store URL is required. Enter the store base URL (e.g. https://shop.example.com) and Save, then Test."
                };
                AppLogger.WriteLog("woo", "Test connection FAILED — " + fail.Detail, updatedBy);
                return fail;
            }

            string key = !string.IsNullOrWhiteSpace(consumerKeyOverride)
                ? consumerKeyOverride.Trim()
                : DecryptOrEmpty(settings.ConsumerKeyEncrypted);

            string secret = !string.IsNullOrWhiteSpace(consumerSecretOverride)
                ? consumerSecretOverride.Trim()
                : DecryptOrEmpty(settings.ConsumerSecretEncrypted);

            var result = _api.TestConnection(settings, key, secret);

            // Persist URL used for a successful/attempted test if it was only typed in the form.
            if (!string.IsNullOrWhiteSpace(storeUrlOverride))
                settings.StoreBaseUrl = WooCommerceApiClient.NormalizeStoreBaseUrl(storeUrlOverride);

            settings.LastConnectionTestUtc = DateTime.UtcNow;
            settings.LastConnectionTestOk = result.Succeeded;
            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo",
                "Test connection " + (result.Succeeded ? "OK" : "FAILED") + " — " + (result.Detail ?? string.Empty),
                updatedBy);
            return result;
        }

        /// <summary>Masked key for UI confirmation (never the full secret).</summary>
        public string GetMaskedConsumerKeyHint()
        {
            string key = DecryptOrEmpty(GetSettings()?.ConsumerKeyEncrypted);
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            if (key.Length <= 10)
                return "••••";
            return key.Substring(0, 6) + "…" + key.Substring(key.Length - 4);
        }

        public bool TryGetApiCredentials(out WooCommerceApiClient.ApiCredentials creds, out string error)
        {
            creds = null;
            error = null;
            if (!IsSchemaReady())
            {
                error = "WooCommerce tables are not ready.";
                return false;
            }
            var hdr = GetPreferencesHeader();
            if (!hdr.WooCommerceEnabled)
            {
                error = "WooCommerce integration is disabled.";
                return false;
            }
            var settings = GetSettings();
            string key = DecryptOrEmpty(settings.ConsumerKeyEncrypted);
            string secret = DecryptOrEmpty(settings.ConsumerSecretEncrypted);
            if (string.IsNullOrWhiteSpace(settings.StoreBaseUrl) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
            {
                error = "Store URL and API keys must be saved first.";
                return false;
            }
            creds = new WooCommerceApiClient.ApiCredentials
            {
                StoreBaseUrl = settings.StoreBaseUrl,
                ConsumerKey = key,
                ConsumerSecret = secret
            };
            return true;
        }

        public void SaveCategoryFilterMode(string mode, string updatedBy)
        {
            EnsureSchema();
            var settings = _settingsRepo.GetSettings();
            settings.CategoryFilterMode = string.IsNullOrWhiteSpace(mode) ? "All" : mode.Trim();
            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo", "CategoryFilterMode=" + settings.CategoryFilterMode, updatedBy);
        }

        public void CompleteWizard(string updatedBy)
        {
            EnsureSchema();
            var settings = _settingsRepo.GetSettings();
            if (settings.LastConnectionTestOk != true)
                throw new InvalidOperationException("Test Connection must succeed before finishing setup.");

            settings.IntegrationEnabled = true;
            settings.WizardCompleted = true;
            _settingsRepo.SaveSettings(settings, updatedBy);

            var hdr = _prefsRepo.GetHeader();
            hdr.WooCommerceEnabled = true;
            hdr.WooWizardCompleted = true;
            _prefsRepo.SaveHeader(hdr, updatedBy);
            AppLogger.WriteLog("woo", "Setup wizard completed; integration enabled", updatedBy);
        }

        public void SetWooEnabled(bool enabled, string updatedBy)
        {
            EnsureSchema();
            var hdr = _prefsRepo.GetHeader();
            hdr.WooCommerceEnabled = enabled;
            _prefsRepo.SaveHeader(hdr, updatedBy);

            var settings = _settingsRepo.GetSettings();
            settings.IntegrationEnabled = enabled;
            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo", "Integration enabled=" + enabled, updatedBy);
        }

        private static string DecryptOrEmpty(string cipher)
        {
            if (string.IsNullOrEmpty(cipher))
                return string.Empty;
            try
            {
                return WooCommerceSecretProtector.Decrypt(cipher) ?? string.Empty;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("woo", "Decrypt failed: " + ex.Message);
                return string.Empty;
            }
        }
    }
}
