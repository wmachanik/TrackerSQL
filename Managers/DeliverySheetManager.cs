using System;
using System.Collections.Generic;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Managers
{
    public class DeliverySheetManager
    {
        private const string CONST_ZZNAME_PREFIX = "_*:";

        private readonly Func<long, int> _getContactInvoiceType;

        public DeliverySheetManager(Func<long, int> getContactInvoiceType)
        {
            _getContactInvoiceType = getContactInvoiceType ?? (contactId => 0);
        }

        public DeliverySheetBuildResult Build(List<DeliverySheetOrderRow> rows, bool includeDeliveryPeople)
        {
            var result = new DeliverySheetBuildResult();

            var deliveryItems = new List<DeliverySheetDisplayItem>();
            var deliveryPeople = new SortedDictionary<string, string>();
            var itemTotals = new Dictionary<string, DeliverySheetTotal>();

            if (rows == null)
            {
                result.Items = deliveryItems;
                result.Totals = new List<DeliverySheetTotal>();
                result.DeliveryPeople = new List<DeliveryPersonOption>();
                return result;
            }

            foreach (var row in rows)
            {
                var item = BuildDisplayItem(row, includeDeliveryPeople, deliveryPeople);

                AddItemHtmlAndTotals(row, item, itemTotals);

                deliveryItems.Add(item);
            }

            ReorderSundryItems(deliveryItems);

            result.Items = deliveryItems;

            result.Totals = itemTotals
                .OrderBy(x => x.Value.ItemOrder)
                .Select(x => x.Value)
                .ToList();

            result.DeliveryPeople = deliveryPeople
                .Select(x => new DeliveryPersonOption
                {
                    PersonID = x.Key,
                    Abbreviation = x.Value
                })
                .ToList();

            return result;
        }

        private DeliverySheetDisplayItem BuildDisplayItem(
            DeliverySheetOrderRow row,
            bool includeDeliveryPeople,
            SortedDictionary<string, string> deliveryPeople)
        {
            string[] invoiceTypePrefixes = new string[8]
            {
                "",
                "dN",
                "d#",
                "g$",
                "cS",
                "s@",
                "!!",
                "??"
            };

            var item = new DeliverySheetDisplayItem
            {
                OrderID = row.OrderID,
                ContactID = row.ContactID.ToString(),
                ContactIDValue = row.ContactID,
                ContactName = row.ContactName ?? string.Empty,
                Details = string.Format("{0:d}, {1}", row.RequiredByDate, row.DeliveryByAbbreviation),
                InvoiceDone = row.InvoiceDone,
                Done = row.Done,
                PurchaseOrder = row.PurchaseOrder ?? string.Empty,
                Items = string.Empty,
                RequiredByDate = row.RequiredByDate,
                Notes = row.Notes ?? string.Empty
            };

            if (item.ContactName.StartsWith(SystemConstants.CustomerConstants.SundryCustomerName))
            {
                item.ContactID = SystemConstants.CustomerConstants.SundryCustomerNamePrefix;

                string notes = row.Notes ?? string.Empty;

                if (notes.Contains(":"))
                    notes = notes.Remove(notes.IndexOf(":")).Trim();

                string strippedNotes = StripEmailOut(notes);

                item.ContactName = CONST_ZZNAME_PREFIX + " " + strippedNotes;
            }

            if ((row.Notes ?? string.Empty).StartsWith("+"))
            {
                item.ContactName = string.Format("{0}[{1}]", item.ContactName, row.Notes);
            }

            if (!item.ContactID.Equals(SystemConstants.CustomerConstants.SundryCustomerNamePrefix))
            {
                long contactId;

                if (long.TryParse(item.ContactID, out contactId))
                {
                    int invoiceType = _getContactInvoiceType(contactId);

                    if (invoiceType > 1 && invoiceType - 1 < invoiceTypePrefixes.Length)
                    {
                        item.ContactName = string.Format(
                            "{0}]> {1}",
                            invoiceTypePrefixes[invoiceType - 1],
                            item.ContactName);
                    }
                    }
            }

            // Done status is rendered as a status-badge next to the name in the page UI
            // (same pattern as RecurringOrders enabled/disabled) — do not prefix the name here.

            if (includeDeliveryPeople)
            {
                string deliveryByKey = row.ToBeDeliveredByID.HasValue
                    ? row.ToBeDeliveredByID.Value.ToString()
                    : string.Empty;

                if (!deliveryPeople.ContainsKey(deliveryByKey))
                {
                    deliveryPeople[deliveryByKey] = row.DeliveryByAbbreviation ?? string.Empty;
                }
            }

            return item;
        }

        private void AddItemHtmlAndTotals(
            DeliverySheetOrderRow row,
            DeliverySheetDisplayItem displayItem,
            Dictionary<string, DeliverySheetTotal> itemTotals)
        {
            bool isSundry = displayItem.ContactID == SystemConstants.CustomerConstants.SundryCustomerNamePrefix;

            string key = row.ItemID.ToString();

            string itemDescription = !string.IsNullOrEmpty(row.ItemShortName)
                ? row.ItemShortName
                : row.ItemDesc;

            string totalDescription = itemDescription;

            if (!row.ItemEnabled)
            {
                itemDescription = "<span style='background-color: RED; color: WHITE'>SOLD OUT</span> " + itemDescription;
                totalDescription = string.Format(">{0}<", totalDescription);
            }

            if (row.SortOrder == 10)
            {
                string notes = row.Notes ?? string.Empty;

                if (isSundry && notes.Contains(":"))
                    notes = notes.Substring(notes.IndexOf(":") + 1).Trim();

                string strippedNotes = StripEmailOut(notes);

                itemDescription = string.Format("{0}: {1}", itemDescription, strippedNotes);
            }

            string bgColour = string.IsNullOrEmpty(row.BGColour)
                ? "transparent"
                : row.BGColour;

            if (!string.IsNullOrEmpty(row.PackDesc))
            {
                displayItem.Items += string.Format(
                    "<span style='background-color:{0}; padding-top: 1px; padding-bottom:2px'>{1}X{2} ({3})</span>",
                    bgColour,
                    SystemConstants.FormatConstants.FormatQuantity(row.QtyOrdered),
                    itemDescription,
                    row.PackDesc);
            }
            else
            {
                displayItem.Items += string.Format(
                    "<span style='background-color:{0}'>{1}X{2}</span>",
                    bgColour,
                    SystemConstants.FormatConstants.FormatQuantity(row.QtyOrdered),
                    itemDescription);
            }

            if (row.SortOrder != 10)
            {
                if (itemTotals.ContainsKey(key))
                {
                    itemTotals[key].TotalQty += row.QtyOrdered;
                }
                else
                {
                    itemTotals[key] = new DeliverySheetTotal
                    {
                        ItemID = key,
                        ItemDesc = totalDescription,
                        TotalQty = row.QtyOrdered,
                        ItemOrder = row.SortOrder
                    };
                }
            }
        }

        private void ReorderSundryItems(List<DeliverySheetDisplayItem> deliveryItems)
        {
            int count = deliveryItems.Count;

            for (int index1 = 0; index1 < count; index1++)
            {
                if (deliveryItems[index1].ContactName.StartsWith(CONST_ZZNAME_PREFIX))
                {
                    for (int index2 = index1 + 2; index2 < count; index2++)
                    {
                        if (deliveryItems[index2].ContactName.Equals(deliveryItems[index1].ContactName))
                        {
                            DeliverySheetDisplayItem item = deliveryItems[index2];

                            deliveryItems.RemoveAt(index2);
                            deliveryItems.Insert(index1 + 1, item);
                        }
                    }
                }
            }
        }

        private string StripEmailOut(string notes)
        {
            if (string.IsNullOrEmpty(notes))
                return string.Empty;

            int start = notes.IndexOf("[#");

            if (start >= 0)
            {
                int end = notes.IndexOf("#]");

                if (end >= 0)
                {
                    notes = string.Format(
                        "{0};{1}",
                        notes.Substring(0, start),
                        notes.Substring(end + 2));
                }
            }

            return notes;
        }
    }
}