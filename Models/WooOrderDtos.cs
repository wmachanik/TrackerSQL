using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackerSQL.Models
{
    public class WooAddressDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string FullName =>
            string.Join(" ", new[] { FirstName, LastName }.WhereNonEmpty()).Trim();

        /// <summary>Best suburb/city hint from shipping lines (Woo often uses address_2 for suburb).</summary>
        public string Suburb
        {
            get
            {
                foreach (string p in new[] { City, Address2, Address1 })
                {
                    if (!string.IsNullOrWhiteSpace(p))
                        return p.Trim();
                }
                return string.Empty;
            }
        }

        public string FormattedAddress =>
            string.Join(", ", new[] { Address1, Address2, City, Postcode, State }.WhereNonEmpty());

        /// <summary>Tracker-style address: one line per part, separated by semicolons (postcode stored separately).</summary>
        public string SemicolonFormattedAddress =>
            string.Join("; ", new[] { Address1, Address2, City, State, Country }.WhereNonEmpty());
    }

    public class WooOrderLineDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public long ProductId { get; set; }
        public long VariationId { get; set; }
        public double Quantity { get; set; }
        public List<WooMetaDto> MetaData { get; set; } = new List<WooMetaDto>();
    }

    public class WooShippingLineDto
    {
        public string MethodTitle { get; set; }
        public string MethodId { get; set; }
    }

    public class WooMetaDto
    {
        public string Key { get; set; }
        /// <summary>Woo display_key (e.g. "Prep Type") — preferred for attribute matching.</summary>
        public string DisplayKey { get; set; }
        public string Value { get; set; }
        public string DisplayValue { get; set; }
    }

    /// <summary>Qty / packaging / notes resolved from Woo line attributes at order import.</summary>
    public class WooLineAttributeResolveResult
    {
        public double QtyFactor { get; set; } = 1;
        public int? PackagingId { get; set; }
        public int? PrepTypeId { get; set; }
        public string Reason { get; set; }
        public List<string> NoteParts { get; set; } = new List<string>();
        public bool Applied { get; set; }
    }

    public class WooOrderDto
    {
        public long Id { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public DateTime? DatePaid { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentMethodTitle { get; set; }
        public string TransactionId { get; set; }
        public long CustomerId { get; set; }
        public string CustomerNote { get; set; }
        public WooAddressDto Billing { get; set; } = new WooAddressDto();
        public WooAddressDto Shipping { get; set; } = new WooAddressDto();
        public List<WooOrderLineDto> LineItems { get; set; } = new List<WooOrderLineDto>();
        public List<WooShippingLineDto> ShippingLines { get; set; } = new List<WooShippingLineDto>();
        public List<WooMetaDto> MetaData { get; set; } = new List<WooMetaDto>();
        public string RawJson { get; set; }
    }

    internal static class WooStringJoin
    {
        public static IEnumerable<string> WhereNonEmpty(this IEnumerable<string> parts)
        {
            if (parts == null)
                yield break;
            foreach (string p in parts)
            {
                if (!string.IsNullOrWhiteSpace(p))
                    yield return p.Trim();
            }
        }
    }
}
