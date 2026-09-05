using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>Shared Woo → Tracker contact + account bootstrap helpers.</summary>
    internal static class WooContactBootstrap
    {
        private static readonly TextInfo TitleCaser = CultureInfo.CurrentCulture.TextInfo;

        public static void ApplyCapitalization(Contact contact)
        {
            if (contact == null)
                return;

            contact.CompanyName = CapitalizeWords(contact.CompanyName);
            contact.ContactFirstName = CapitalizeWords(contact.ContactFirstName);
            contact.ContactLastName = CapitalizeWords(contact.ContactLastName);
            contact.ContactAltFirstName = CapitalizeWords(contact.ContactAltFirstName);
            contact.ContactAltLastName = CapitalizeWords(contact.ContactAltLastName);
            contact.StateOrProvince = CapitalizeWords(contact.StateOrProvince);
        }

        public static void ApplyCoffeePreferences(
            Contact contact,
            WooOrderImportPreviewRow preview,
            ItemsRepository itemsRepo)
        {
            if (contact == null || preview == null || itemsRepo == null)
                return;

            WooOrderLinePreview coffeeLine = FindFirstCoffeeLine(preview, itemsRepo);
            if (coffeeLine == null)
                return;

            contact.ContactTypeID = SystemConstants.CustomerTypeConstants.CoffeeOnly;
            contact.ItemPrefID = coffeeLine.TrackerItemId;
            contact.PriPrefQty = coffeeLine.TrackerQty > 0 ? coffeeLine.TrackerQty : (double?)null;
            if (coffeeLine.PackagingId.HasValue && coffeeLine.PackagingId.Value > 0)
                contact.PrefItemPackagingID = coffeeLine.PackagingId;
        }

        public static ContactsAccInfo BuildAccInfo(Contact contact, WooOrderDto order, int contactId)
        {
            var acc = new ContactsAccInfo
            {
                ContactID = contactId,
                FullCoName = CapitalizeWords(contact?.CompanyName),
                AccFirstName = CapitalizeWords(contact?.ContactFirstName),
                AccLastName = CapitalizeWords(contact?.ContactLastName),
                AltAccFirstName = CapitalizeWords(contact?.ContactAltFirstName),
                AltAccLastName = CapitalizeWords(contact?.ContactAltLastName),
                AccEmail = contact?.EmailAddress?.Trim(),
                AltAccEmail = contact?.AltEmailAddress?.Trim(),
                ContactVATNo = ExtractVatNumber(order),
                InvoiceTypeID = SystemConstants.InvoiceTypeConstants.Standard,
                Enabled = true
            };

            string billing = contact?.BillingAddress ?? string.Empty;
            string[] parts = billing.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            acc.BillAddr1 = parts.Length > 0 ? CapitalizeWords(parts[0].Trim()) : string.Empty;
            acc.BillAddr2 = parts.Length > 1 ? CapitalizeWords(parts[1].Trim()) : string.Empty;
            acc.BillAddr3 = parts.Length > 2 ? CapitalizeWords(parts[2].Trim()) : string.Empty;
            if (parts.Length > 3)
            {
                var tail = new List<string>();
                for (int i = 3; i < parts.Length; i++)
                    tail.Add(CapitalizeWords(parts[i].Trim()));
                acc.BillAddr4 = string.Join(", ", tail.Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            acc.BillAddr5 = NormalizePostcode(contact?.PostalCode);
            acc.ShipAddr1 = acc.BillAddr1;
            acc.ShipAddr2 = acc.BillAddr2;
            acc.ShipAddr3 = acc.BillAddr3;
            acc.ShipAddr4 = acc.BillAddr4;
            acc.ShipAddr5 = acc.BillAddr5;
            return acc;
        }

        public static void EnsureAccInfo(ContactsAccInfoRepository accRepo, Contact contact, WooOrderDto order, int contactId)
        {
            if (accRepo == null || contactId <= 0)
                return;

            if (accRepo.GetByContactId(contactId) != null)
                return;

            ContactsAccInfo acc = BuildAccInfo(contact, order, contactId);
            accRepo.Insert(acc);
        }

        public static void UpdateAccInfoAddresses(ContactsAccInfoRepository accRepo, Contact contact, WooOrderDto order, int contactId)
        {
            if (accRepo == null || contactId <= 0)
                return;

            ContactsAccInfo acc = accRepo.GetByContactId(contactId);
            if (acc == null)
            {
                EnsureAccInfo(accRepo, contact, order, contactId);
                return;
            }

            ContactsAccInfo fresh = BuildAccInfo(contact, order, contactId);
            acc.BillAddr1 = fresh.BillAddr1;
            acc.BillAddr2 = fresh.BillAddr2;
            acc.BillAddr3 = fresh.BillAddr3;
            acc.BillAddr4 = fresh.BillAddr4;
            acc.BillAddr5 = fresh.BillAddr5;
            acc.ShipAddr1 = fresh.ShipAddr1;
            acc.ShipAddr2 = fresh.ShipAddr2;
            acc.ShipAddr3 = fresh.ShipAddr3;
            acc.ShipAddr4 = fresh.ShipAddr4;
            acc.ShipAddr5 = fresh.ShipAddr5;
            if (string.IsNullOrWhiteSpace(acc.FullCoName))
                acc.FullCoName = fresh.FullCoName;
            if (string.IsNullOrWhiteSpace(acc.AccFirstName))
                acc.AccFirstName = fresh.AccFirstName;
            if (string.IsNullOrWhiteSpace(acc.AccLastName))
                acc.AccLastName = fresh.AccLastName;
            if (string.IsNullOrWhiteSpace(acc.AccEmail))
                acc.AccEmail = fresh.AccEmail;
            if (string.IsNullOrWhiteSpace(acc.ContactVATNo))
                acc.ContactVATNo = fresh.ContactVATNo;
            accRepo.Update(acc);
        }

        private static WooOrderLinePreview FindFirstCoffeeLine(WooOrderImportPreviewRow preview, ItemsRepository itemsRepo)
        {
            foreach (WooOrderLinePreview line in preview.Lines ?? Enumerable.Empty<WooOrderLinePreview>())
            {
                if (line == null || !line.CanImport || line.IsGear)
                    continue;
                if (string.Equals(line.MapType, "Notes", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!line.TrackerItemId.HasValue || line.TrackerItemId.Value <= 0)
                    continue;

                Item item = itemsRepo.GetById(line.TrackerItemId.Value);
                if (item?.ItemServiceTypeID == SystemConstants.ServiceTypeConstants.Coffee)
                    return line;
            }

            return null;
        }

        public static string CapitalizeWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value ?? string.Empty;

            string trimmed = value.Trim();
            if (trimmed.IndexOf('@') >= 0)
                return trimmed;

            return TitleCaser.ToTitleCase(trimmed.ToLower(CultureInfo.CurrentCulture));
        }

        public static string ExtractVatNumber(WooOrderDto order)
        {
            if (order?.MetaData == null)
                return null;

            string[] keys =
            {
                "vat_number", "billing_vat", "_billing_vat", "VAT Number", "vat",
                "eu_vat_number", "billing_vat_number", "_vat_number"
            };

            foreach (WooMetaDto meta in order.MetaData)
            {
                if (meta == null || string.IsNullOrWhiteSpace(meta.Key))
                    continue;
                foreach (string key in keys)
                {
                    if (string.Equals(meta.Key.Trim(), key, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(meta.Value))
                        return meta.Value.Trim();
                }
            }

            return null;
        }

        private static string NormalizePostcode(string postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                return string.Empty;
            var digits = System.Text.RegularExpressions.Regex.Replace(postalCode.Trim(), @"[^\d]", string.Empty);
            if (digits.Length >= 4)
                return digits.Substring(0, 4);
            return digits;
        }
    }
}
