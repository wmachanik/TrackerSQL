using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Caching;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Writes durable contact field history to ContactChangeLogTbl (independent of Notes).
    /// </summary>
    public static class ContactChangeLogManager
    {
        public const string SourceContactDetails = "ContactDetails";
        public const string SourceWooImport = "WooImport";
        public const string SourceWooUpdate = "WooUpdate";
        public const string SourceSystem = "System";
        public const string SourceMerge = "Merge";
        public const string SourceDisable = "Disable";
        public const string SourcePortal = "Portal";

        private const int ValueMaxLen = 400;

        public static void EnsureSchemaOnce()
        {
            const string cacheKey = "ContactChangeLog.SchemaEnsured.v1";
            if (HttpRuntime.Cache[cacheKey] != null)
                return;

            var result = new ContactChangeLogSchemaInstaller().EnsureSchema();
            if (result.Succeeded)
            {
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    true,
                    null,
                    Cache.NoAbsoluteExpiration,
                    Cache.NoSlidingExpiration);
            }
        }

        public static string CurrentUserName()
        {
            try
            {
                string name = HttpContext.Current?.User?.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                    return name.Trim();
            }
            catch
            {
                // ignore
            }
            return "system";
        }

        public static void LogSummary(int contactId, string source, string summary, string changedBy = null)
        {
            if (contactId <= 0 || string.IsNullOrWhiteSpace(summary))
                return;

            try
            {
                EnsureSchemaOnce();
                new ContactChangeLogRepository().Insert(new ContactChangeLogEntry
                {
                    ContactID = contactId,
                    ChangedAt = TimeZoneUtils.Now(),
                    ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? CurrentUserName() : changedBy.Trim(),
                    Source = string.IsNullOrWhiteSpace(source) ? SourceSystem : source.Trim(),
                    Summary = summary.Trim()
                });
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("system", "ContactChangeLog.LogSummary failed: " + ex.Message);
            }
        }

        public static void LogDiff(
            int contactId,
            Contact before,
            Contact after,
            string source,
            string summary = null,
            string changedBy = null)
        {
            if (contactId <= 0 || before == null || after == null)
                return;

            try
            {
                EnsureSchemaOnce();
                var changes = Diff(before, after);
                if (changes.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(summary))
                        LogSummary(contactId, source, summary, changedBy);
                    return;
                }

                string by = string.IsNullOrWhiteSpace(changedBy) ? CurrentUserName() : changedBy.Trim();
                string src = string.IsNullOrWhiteSpace(source) ? SourceSystem : source.Trim();
                DateTime when = TimeZoneUtils.Now();
                var repo = new ContactChangeLogRepository();

                foreach (var c in changes)
                {
                    repo.Insert(new ContactChangeLogEntry
                    {
                        ContactID = contactId,
                        ChangedAt = when,
                        ChangedBy = by,
                        Source = src,
                        Summary = summary,
                        FieldName = c.FieldName,
                        OldValue = c.OldValue,
                        NewValue = c.NewValue
                    });
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog("system", "ContactChangeLog.LogDiff failed: " + ex.Message);
            }
        }

        public static List<ContactChangeLogEntry> Diff(Contact before, Contact after)
        {
            var list = new List<ContactChangeLogEntry>();
            if (before == null || after == null)
                return list;

            Add(list, "CompanyName", before.CompanyName, after.CompanyName);
            Add(list, "ContactTitle", before.ContactTitle, after.ContactTitle);
            Add(list, "ContactFirstName", before.ContactFirstName, after.ContactFirstName);
            Add(list, "ContactLastName", before.ContactLastName, after.ContactLastName);
            Add(list, "ContactAltFirstName", before.ContactAltFirstName, after.ContactAltFirstName);
            Add(list, "ContactAltLastName", before.ContactAltLastName, after.ContactAltLastName);
            Add(list, "Department", before.Department, after.Department);
            Add(list, "BillingAddress", before.BillingAddress, after.BillingAddress);
            Add(list, "AreaID", FormatInt(before.AreaID), FormatInt(after.AreaID));
            Add(list, "StateOrProvince", before.StateOrProvince, after.StateOrProvince);
            Add(list, "PostalCode", before.PostalCode, after.PostalCode);
            Add(list, "CountryOrRegion", before.CountryOrRegion, after.CountryOrRegion);
            Add(list, "PhoneNumber", before.PhoneNumber, after.PhoneNumber);
            Add(list, "Extension", before.Extension, after.Extension);
            Add(list, "FaxNumber", before.FaxNumber, after.FaxNumber);
            Add(list, "CellNumber", before.CellNumber, after.CellNumber);
            Add(list, "EmailAddress", before.EmailAddress, after.EmailAddress);
            Add(list, "AltEmailAddress", before.AltEmailAddress, after.AltEmailAddress);
            Add(list, "ContractNo", before.ContractNo, after.ContractNo);
            Add(list, "ContactTypeID", FormatInt(before.ContactTypeID), FormatInt(after.ContactTypeID));
            Add(list, "EquipTypeID", FormatInt(before.EquipTypeID), FormatInt(after.EquipTypeID));
            Add(list, "ItemPrefID", FormatInt(before.ItemPrefID), FormatInt(after.ItemPrefID));
            Add(list, "PriPrefQty", FormatDouble(before.PriPrefQty), FormatDouble(after.PriPrefQty));
            Add(list, "PrefItemPrepTypeID", FormatInt(before.PrefItemPrepTypeID), FormatInt(after.PrefItemPrepTypeID));
            Add(list, "PrefItemPackagingID", FormatInt(before.PrefItemPackagingID), FormatInt(after.PrefItemPackagingID));
            Add(list, "SecondaryItemPrefID", FormatInt(before.SecondaryItemPrefID), FormatInt(after.SecondaryItemPrefID));
            Add(list, "SecPrefQty", FormatDouble(before.SecPrefQty), FormatDouble(after.SecPrefQty));
            Add(list, "TypicallySecToo", FormatBool(before.TypicallySecToo), FormatBool(after.TypicallySecToo));
            Add(list, "PreferredAgentID", FormatInt(before.PreferredAgentID), FormatInt(after.PreferredAgentID));
            Add(list, "PreferredCourierServiceID", FormatInt(before.PreferredCourierServiceID), FormatInt(after.PreferredCourierServiceID));
            Add(list, "SalesAgentID", FormatInt(before.SalesAgentID), FormatInt(after.SalesAgentID));
            Add(list, "EquipentSN", before.EquipentSN, after.EquipentSN);
            Add(list, "UsesFilter", FormatBool(before.UsesFilter), FormatBool(after.UsesFilter));
            Add(list, "AutoFulfill", FormatBool(before.AutoFulfill), FormatBool(after.AutoFulfill));
            Add(list, "Enabled", FormatBool(before.Enabled), FormatBool(after.Enabled));
            Add(list, "PredictionDisabled", FormatBool(before.PredictionDisabled), FormatBool(after.PredictionDisabled));
            Add(list, "AlwaysSendChkUp", FormatBool(before.AlwaysSendChkUp), FormatBool(after.AlwaysSendChkUp));
            Add(list, "NormallyResponds", FormatBool(before.NormallyResponds), FormatBool(after.NormallyResponds));
            Add(list, "SendDeliveryConfirmation", FormatBool(before.SendDeliveryConfirmation), FormatBool(after.SendDeliveryConfirmation));
            // Notes: log only when changed, truncated (full text still lives on the contact)
            Add(list, "Notes", Truncate(before.Notes), Truncate(after.Notes));
            return list;
        }

        private static void Add(List<ContactChangeLogEntry> list, string field, string oldVal, string newVal)
        {
            string a = Normalize(oldVal);
            string b = Normalize(newVal);
            if (string.Equals(a, b, StringComparison.Ordinal))
                return;
            list.Add(new ContactChangeLogEntry
            {
                FieldName = field,
                OldValue = string.IsNullOrEmpty(a) ? null : a,
                NewValue = string.IsNullOrEmpty(b) ? null : b
            });
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return value.Trim();
        }

        private static string FormatInt(int? value)
        {
            return value.HasValue && value.Value > 0
                ? value.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string FormatDouble(double? value)
        {
            return value.HasValue
                ? value.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static string FormatBool(bool? value)
        {
            if (!value.HasValue)
                return string.Empty;
            return value.Value ? "Yes" : "No";
        }

        private static string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            value = value.Trim();
            if (value.Length <= ValueMaxLen)
                return value;
            return value.Substring(0, ValueMaxLen - 1) + "…";
        }
    }
}
