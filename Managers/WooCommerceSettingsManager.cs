using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
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

        /// <summary>Run idempotent create/alter once per app domain (not every postback).</summary>
        public WooCommerceSchemaInstaller.EnsureResult EnsureSchemaOnce()
        {
            const string cacheKey = "WooCommerce.SchemaEnsured.v10";
            if (HttpRuntime.Cache[cacheKey] != null)
                return new WooCommerceSchemaInstaller.EnsureResult { Succeeded = true, Message = "Schema already ensured." };

            var result = EnsureSchema();
            if (result.Succeeded)
            {
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    true,
                    null,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration);
            }
            return result;
        }

        /// <summary>Force schema ensure on next page load (e.g. after XMLtoSQL or wizard).</summary>
        public static void InvalidateSchemaCache()
        {
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured");
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured.v2");
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured.v3");
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured.v4");
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured.v5");
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured.v6");
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured.v7");
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured.v8");
            HttpRuntime.Cache.Remove("WooCommerce.SchemaEnsured.v10");
            InvalidateIntegrationEnabledCache();
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
            const string cacheKey = "WooCommerce.IntegrationEnabled";
            try
            {
                object cached = HttpRuntime.Cache[cacheKey];
                if (cached is bool)
                    return (bool)cached;

                bool enabled = IsSchemaReady() && GetPreferencesHeader().WooCommerceEnabled;
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    enabled,
                    null,
                    DateTime.UtcNow.AddSeconds(60),
                    Cache.NoSlidingExpiration);
                return enabled;
            }
            catch
            {
                return false;
            }
        }

        public static void InvalidateIntegrationEnabledCache()
        {
            HttpRuntime.Cache.Remove("WooCommerce.IntegrationEnabled");
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

        public void SaveDispatchWaybillSettings(
            string dispatchDeliveryPersonIds,
            bool trackingNumberRequired,
            string updatedBy)
        {
            EnsureSchema();
            var settings = _settingsRepo.GetSettings();
            settings.DispatchDeliveryPersonIds = (dispatchDeliveryPersonIds ?? string.Empty).Trim();
            settings.TrackingNumberRequired = trackingNumberRequired;
            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo",
                "Dispatch/waybill settings saved: people="
                + (string.IsNullOrWhiteSpace(settings.DispatchDeliveryPersonIds)
                    ? "(none)"
                    : settings.DispatchDeliveryPersonIds)
                + "; trackingRequired=" + trackingNumberRequired,
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

        public void SaveImportAddressSettings(
            bool includeProvince,
            bool includeCountry,
            bool deduplicateSuburb,
            bool stripCapeTown,
            bool titleCase,
            bool replacePlus27,
            bool formatSaPhone,
            string updatedBy)
        {
            EnsureSchema();
            var settings = _settingsRepo.GetSettings();
            settings.ImportAddressIncludeProvince = includeProvince;
            settings.ImportAddressIncludeCountry = includeCountry;
            settings.ImportAddressDeduplicateSuburb = deduplicateSuburb;
            settings.ImportAddressStripCapeTown = stripCapeTown;
            settings.ImportAddressTitleCase = titleCase;
            settings.ImportPhoneReplacePlus27 = replacePlus27;
            settings.ImportPhoneFormatSa = formatSaPhone;
            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo",
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Import address settings: province={0}, country={1}, dedupe={2}, stripCape={3}, titleCase={4}, phone27={5}, phoneFmt={6}",
                    includeProvince, includeCountry, deduplicateSuburb, stripCapeTown, titleCase, replacePlus27, formatSaPhone),
                updatedBy);
        }

        public void SaveImportNotesItemId(int? notesItemId, string updatedBy)
        {
            EnsureSchema();
            var settings = _settingsRepo.GetSettings();
            settings.ImportNotesItemID = notesItemId.HasValue && notesItemId.Value > 0
                ? notesItemId
                : null;
            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo",
                "ImportNotesItemID=" + (settings.ImportNotesItemID.HasValue
                    ? settings.ImportNotesItemID.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : "cleared"),
                updatedBy);
        }

        public const string DefaultNotePartOrder =
            "Name,Gear,Address,WooPay,Email,CustomerNote,ContactCreated";

        public static readonly string[] NotePartKeys =
        {
            "Name", "Gear", "Address", "WooPay", "Email", "CustomerNote", "ContactCreated"
        };

        public void SaveGeneralImportSettings(
            string companyNameMode,
            string noteLineFormat,
            bool appendTrackingToOrderNotes,
            string autoPullMode,
            string notePartOrder,
            string updatedBy)
        {
            EnsureSchema();
            var settings = _settingsRepo.GetSettings();
            settings.ImportCompanyNameMode = NormalizeCompanyNameMode(companyNameMode);
            settings.ImportNoteLineFormat = NormalizeNoteLineFormat(noteLineFormat);
            settings.AppendTrackingToOrderNotes = appendTrackingToOrderNotes;
            settings.ImportAutoPullMode = NormalizeAutoPullMode(autoPullMode);
            settings.ImportNotePartOrder = NormalizeNotePartOrder(notePartOrder);
            _settingsRepo.SaveSettings(settings, updatedBy);
            AppLogger.WriteLog("woo",
                string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "General import settings: companyMode={0}, noteFmt={1}, trackNotes={2}, autoPull={3}, noteOrder={4}",
                    settings.ImportCompanyNameMode, settings.ImportNoteLineFormat,
                    appendTrackingToOrderNotes, settings.ImportAutoPullMode, settings.ImportNotePartOrder),
                updatedBy);
        }

        public static string NormalizeCompanyNameMode(string mode)
        {
            if (string.Equals(mode, "UpdateName", StringComparison.OrdinalIgnoreCase))
                return "UpdateName";
            return "CareOfPrefix";
        }

        public static string NormalizeNoteLineFormat(string format)
        {
            if (string.Equals(format, "SkuOnly", StringComparison.OrdinalIgnoreCase))
                return "SkuOnly";
            return "SkuAndName";
        }

        public static string NormalizeAutoPullMode(string mode)
        {
            if (string.Equals(mode, "None", StringComparison.OrdinalIgnoreCase))
                return "None";
            if (string.Equals(mode, "SinceLastSync", StringComparison.OrdinalIgnoreCase))
                return "SinceLastSync";
            if (string.Equals(mode, "ThisWeek", StringComparison.OrdinalIgnoreCase))
                return "ThisWeek";
            return "Today";
        }

        /// <summary>Returns a validated comma-separated note-part order (all known keys, no duplicates).</summary>
        public static string NormalizeNotePartOrder(string order)
        {
            var known = new HashSet<string>(NotePartKeys, StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(order))
            {
                foreach (string raw in order.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string key = raw.Trim();
                    string match = NotePartKeys.FirstOrDefault(k =>
                        string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                    if (match != null && !result.Contains(match))
                        result.Add(match);
                }
            }

            foreach (string key in NotePartKeys)
            {
                if (!result.Contains(key))
                    result.Add(key);
            }

            return string.Join(",", result);
        }

        public static List<string> ParseNotePartOrder(string order)
        {
            return NormalizeNotePartOrder(order)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList();
        }

        public static string NotePartDisplayLabel(string key)
        {
            switch ((key ?? string.Empty).Trim())
            {
                case "Name": return "Name (if available — keep first for ZZName / delivery sheet)";
                case "Gear": return "Gear / notes-mapped lines (if any)";
                case "Address": return "Shipping address (if available)";
                case "WooPay": return "Woo order + payment [#Woo#: pay] (if available)";
                case "Email": return "Email [#email#] (if available)";
                case "CustomerNote": return "Woo customer note (if available)";
                case "ContactCreated": return "Contact created (if a new contact was added)";
                default: return key;
            }
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
            InvalidateIntegrationEnabledCache();
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
            InvalidateIntegrationEnabledCache();
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
