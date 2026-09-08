using System;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class WooCommerceSettingsRepository : RepositoryBase<WooCommerceSettings>
    {
        private const int SingletonId = 1;

        protected override string TableName => "WooCommerceSettingsTbl";
        protected override string KeyColumn => "SettingsID";

        protected override string CoreColumns =>
            "SettingsID, StoreBaseUrl, AdminBaseUrl, ConsumerKeyEncrypted, ConsumerSecretEncrypted, " +
            "IntegrationEnabled, WizardCompleted, LastConnectionTestUtc, LastConnectionTestOk, " +
            "LastItemsSyncUtc, LastOrdersSyncUtc, LastOrdersSyncOrderNumber, LastContactsSyncUtc, " +
            "PushEnabledStateToWoo, DisableScopeDefault, PullStockQtyEnabled, CategoryFilterMode, " +
            "GuestCheckoutContactMode, DispatchDeliveryPersonIds, TrackingNumberRequired, " +
            "DispatchedWooStatus, AutoCompleteOnWooCompleted, DefaultImportAreaID, " +
            "ImportAddressIncludeProvince, ImportAddressIncludeCountry, " +
            "ImportAddressDeduplicateSuburb, ImportAddressStripCapeTown, ImportAddressTitleCase, " +
            "ImportPhoneReplacePlus27, ImportPhoneFormatSa, " +
            "ImportNotesItemID, ImportCompanyNameMode, ImportNoteLineFormat, AppendTrackingToOrderNotes, " +
            "ImportAutoPullMode, ImportZzNameNoteLayout, ImportNotePartOrder, " +
            "UpdatedAt, UpdatedBy";

        public WooCommerceSettings GetSettings()
        {
            return GetById(SingletonId) ?? CreateDefault();
        }

        public bool SaveSettings(WooCommerceSettings settings, string updatedBy)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.SettingsID = SingletonId;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.UpdatedBy = updatedBy ?? string.Empty;

            var existing = GetById(SingletonId);
            if (existing == null)
                return Insert(settings) > 0;

            return Update(settings) > 0;
        }

        public bool TableExists()
        {
            try
            {
                using (var db = CreateDb())
                {
                    int n = db.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'WooCommerceSettingsTbl'");
                    return n > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static WooCommerceSettings CreateDefault()
        {
            return new WooCommerceSettings
            {
                SettingsID = SingletonId,
                PushEnabledStateToWoo = true,
                DisableScopeDefault = "MappedOnly",
                CategoryFilterMode = "All",
                GuestCheckoutContactMode = "ZZName",
                DispatchDeliveryPersonIds = "5,7",
                TrackingNumberRequired = true,
                DispatchedWooStatus = "processing",
                ImportNotesItemID = SystemConstants.ItemConstants.NoteItemTimeID,
                ImportCompanyNameMode = "CareOfPrefix",
                ImportNoteLineFormat = "SkuAndName",
                AppendTrackingToOrderNotes = false,
                ImportAutoPullMode = "Today",
                ImportZzNameNoteLayout = "NameGearAddressMeta",
                ImportNotePartOrder = "Name,Gear,Address,WooPay,Email,CustomerNote,ContactCreated",
                ImportAddressDeduplicateSuburb = true,
                ImportAddressStripCapeTown = true,
                ImportAddressTitleCase = true
            };
        }
    }
}
