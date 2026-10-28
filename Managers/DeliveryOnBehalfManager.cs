using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>Delivery on behalf: orders point at a contact deliver-to address; sheets and driver use its address/GPS.</summary>
    public static class DeliveryOnBehalfManager
    {
        public static bool IsActive =>
            OrdersRepository.HasDeliverToAddressIdColumn()
            && ContactDeliverToAddressRepository.TableSeen();

        public static string JoinClause =>
            IsActive
                ? " LEFT JOIN dbo.ContactDeliverToAddressTbl dob ON dob.DeliverToAddressID = o.DeliverToAddressID"
                : string.Empty;

        /// <summary>Order header driver, else the deliver-to address default (when on-behalf is active).</summary>
        public static string EffectiveDeliveredByIdSql(string orderAlias = "o", string deliverToAlias = "dob")
        {
            if (!IsActive)
                return orderAlias + ".ToBeDeliveredByID";
            return "COALESCE(NULLIF(" + orderAlias + ".ToBeDeliveredByID, 0), " + deliverToAlias + ".ToBeDeliveredByID)";
        }

        /// <summary>Requires <see cref="JoinClause"/> on the query before this join when <see cref="IsActive"/>.</summary>
        public static string AreaJoinOnEffectiveArea(string contactAlias, string areaAlias)
        {
            if (!IsActive)
                return $"LEFT JOIN AreasTbl {areaAlias} ON {areaAlias}.AreaID = {contactAlias}.AreaID";
            return $"LEFT JOIN AreasTbl {areaAlias} ON {areaAlias}.AreaID = COALESCE(dob.AreaID, {contactAlias}.AreaID)";
        }

        public static string EffectiveBillingAddressSql(string contactAlias = "c")
        {
            if (!IsActive)
                return contactAlias + ".BillingAddress";
            return @"CASE WHEN dob.DeliverToAddressID IS NOT NULL THEN
                LTRIM(RTRIM(
                    ISNULL(dob.Address1, N'')
                    + CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(dob.Address2, N''))), N'') IS NOT NULL THEN N', ' + dob.Address2 ELSE N'' END
                    + CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(dob.Address3, N''))), N'') IS NOT NULL THEN N', ' + dob.Address3 ELSE N'' END
                ))
            ELSE " + contactAlias + @".BillingAddress END";
        }

        public static string EffectivePostalCodeSql(string contactAlias = "c")
        {
            if (!IsActive)
                return contactAlias + ".PostalCode";
            return "COALESCE(dob.PostalCode, " + contactAlias + ".PostalCode)";
        }

        public static string EffectiveProvinceSql(string contactAlias = "c")
        {
            if (!IsActive)
                return contactAlias + ".StateOrProvince";
            return "COALESCE(dob.StateOrProvince, " + contactAlias + ".StateOrProvince)";
        }

        public static string EffectiveRouteHashSql(string defaultRouteHashSql)
        {
            if (!IsActive)
                return defaultRouteHashSql;
            return "COALESCE(NULLIF(dob.Geohash, ''), " + defaultRouteHashSql + ")";
        }

        public static string EffectiveGpsLatSql()
        {
            if (!IsActive)
                return "gl.Latitude";
            return "CASE WHEN dob.Latitude IS NOT NULL THEN CAST(dob.Latitude AS FLOAT) ELSE gl.Latitude END";
        }

        public static string EffectiveGpsLngSql()
        {
            if (!IsActive)
                return "gl.Longitude";
            return "CASE WHEN dob.Longitude IS NOT NULL THEN CAST(dob.Longitude AS FLOAT) ELSE gl.Longitude END";
        }

        public static string DeliverToDisplayName(string label, string deliverToPerson)
        {
            if (!string.IsNullOrWhiteSpace(label))
                return label.Trim();
            if (!string.IsNullOrWhiteSpace(deliverToPerson))
                return deliverToPerson.Trim();
            return string.Empty;
        }

        /// <summary>e.g. The Cape Coffee Co → TCCC</summary>
        public static string CustomerInitials(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return string.Empty;
            var sb = new StringBuilder();
            foreach (string word in companyName.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                char c = word[0];
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToUpperInvariant(c));
            }
            return sb.ToString();
        }

        /// <summary>List/sheet title: Vredehoek Spar [TCCC]</summary>
        public static string ListDisplayName(string deliverToName, string customerCompany)
        {
            if (string.IsNullOrWhiteSpace(deliverToName))
                return customerCompany ?? string.Empty;
            string initials = CustomerInitials(customerCompany);
            if (string.IsNullOrEmpty(initials))
                return deliverToName.Trim();
            return deliverToName.Trim() + " [" + initials + "]";
        }

        public static string NotesWithoutDeliverToPrefix(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return string.Empty;
            var lines = notes.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Where(l => !l.TrimStart().StartsWith("Deliver to:", StringComparison.OrdinalIgnoreCase))
                .ToList();
            string joined = string.Join("\n", lines).Trim();
            if (joined.StartsWith("Deliver to:", StringComparison.OrdinalIgnoreCase))
            {
                int dash = joined.IndexOf(" — ", StringComparison.Ordinal);
                joined = dash > 0 ? joined.Substring(dash + 3).Trim() : string.Empty;
            }
            return joined;
        }

        private static readonly Regex AutoPoPattern = new Regex(@"^[A-Za-z0-9]+-\d{8}-\d+$", RegexOptions.Compiled);

        public static string SanitizePoSlug(string deliverToName)
        {
            if (string.IsNullOrWhiteSpace(deliverToName))
                return "DeliverTo";
            var sb = new StringBuilder();
            foreach (char c in deliverToName.Trim())
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }
            return sb.Length > 0 ? sb.ToString() : "DeliverTo";
        }

        public static string FormatDeliverToPurchaseOrder(string deliverToName, DateTime requiredByDate, int sequence)
        {
            int seq = sequence < 1 ? 1 : sequence;
            return SanitizePoSlug(deliverToName) + "-" + requiredByDate.ToString("yyyyMMdd") + "-" + seq;
        }

        public static bool LooksLikeAutoDeliverToPo(string purchaseOrder)
        {
            return !string.IsNullOrWhiteSpace(purchaseOrder) && AutoPoPattern.IsMatch(purchaseOrder.Trim());
        }

        /// <summary>Order Detail deliver-to dropdown: Short Name only.</summary>
        public static string OrderPickerText(ContactDeliverToAddress row)
        {
            return row == null ? string.Empty : row.ShortNameForOrders;
        }

        /// <summary>Detail for orders.log when a new order uses an alternative delivery address.</summary>
        public static string AuditDeliverToSummary(int? deliverToAddressId)
        {
            if (!IsActive || !deliverToAddressId.HasValue || deliverToAddressId.Value <= 0)
                return null;

            ContactDeliverToAddress row = new ContactDeliverToAddressRepository().GetById(deliverToAddressId.Value);
            if (row == null)
                return "Delivery on behalf (alternative address id " + deliverToAddressId.Value + ").";

            var parts = new List<string> { "Delivery on behalf: " + row.ShortNameForOrders };
            string person = (row.DeliverToPerson ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(person)
                && !string.Equals(person, row.ShortNameForOrders, StringComparison.OrdinalIgnoreCase))
                parts.Add("sign POD " + person);
            string kind = row.AddressKindDisplay;
            if (!string.IsNullOrWhiteSpace(kind))
                parts.Add(kind);
            parts.Add("address id " + row.DeliverToAddressID);
            return string.Join("; ", parts) + ".";
        }

        /// <summary>
        /// When the order uses a deliver-to address with its own driver, set <see cref="OrderHeaderData.ToBeDeliveredBy"/>
        /// from that row (not the billing contact's preferred delivery person).
        /// </summary>
        public static void ApplyDeliverToDriver(OrderHeaderData header)
        {
            if (header == null || !IsActive)
                return;
            int? deliverToId = header.DeliverToAddressID;
            if (!deliverToId.HasValue || deliverToId.Value <= 0)
                return;
            ContactDeliverToAddress row = new ContactDeliverToAddressRepository().GetById(deliverToId.Value);
            if (row == null || row.ContactID != header.CustomerID || !row.Enabled)
                return;
            if (row.ToBeDeliveredByID is int personId && personId > 0)
                header.ToBeDeliveredBy = personId;
        }

        /// <summary>True when the order UI should offer main vs deliver-to (independent of OrdersTbl column — that is checked on save).</summary>
        public static bool ContactNeedsDeliverToChoice(int contactId)
        {
            if (contactId <= 0
                || !ContactsRepository.HasDeliveryOnBehalfColumn()
                || !ContactDeliverToAddressRepository.TableSeen())
                return false;

            Contact contact = new ContactsRepository().GetById(contactId);
            if (contact?.DeliveryOnBehalf != true)
                return false;

            return new ContactDeliverToAddressRepository().ListForContact(contactId, enabledOnly: true).Count > 0;
        }
    }
}
