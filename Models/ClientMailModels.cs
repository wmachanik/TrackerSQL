using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    public class ClientMailRecipient
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string EmailAddress { get; set; }
        public string AltEmailAddress { get; set; }
        public string ContactTypeDesc { get; set; }
        public string EquipTypeName { get; set; }
        public string AreaName { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime? NextCoffeeBy { get; set; }
        public string LastItemsOrdered { get; set; }
        public string LastItemTypes { get; set; }
        public string PhoneNumber { get; set; }
        public string CellNumber { get; set; }
        public bool IsRecurring { get; set; }

        public string ToEmail
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EmailAddress))
                    return EmailAddress.Trim();
                return (AltEmailAddress ?? string.Empty).Trim();
            }
        }

        public string DisplayName
        {
            get
            {
                string first = (ContactFirstName ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(first))
                    return first;
                return CompanyName ?? string.Empty;
            }
        }
    }

    public class ClientMailRule
    {
        public int Id { get; set; }
        public string Join { get; set; }
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string Value2 { get; set; }
        public bool GroupOpen { get; set; }
        public bool GroupClose { get; set; }
        public string Display { get; set; }
    }

    public class ClientMailFilter
    {
        public bool EnabledOnly { get; set; } = true;
        public bool MustHaveEmail { get; set; } = true;
        public List<ClientMailRule> Rules { get; set; }
    }

    public static class ClientMailFields
    {
        public const string Name = "Name";
        public const string ContactType = "ContactType";
        public const string LastOrdered = "LastOrdered";
        public const string NextRequired = "NextRequired";
        public const string Equipment = "Equipment";
        public const string Area = "Area";
        public const string Recurring = "Recurring";
        public const string LastItem = "LastItem";
        public const string LastItemType = "LastItemType";
        public const string EmailPrimary = "EmailPrimary";
        public const string EmailAll = "EmailAll";
        public const string CustomWhere = "CustomWhere";
    }

    public class ClientMailTemplate
    {
        public int ClientMailTemplateID { get; set; }
        public string TemplateName { get; set; }
        public string Subject { get; set; }
        public string HeaderText { get; set; }
        public string BodyText { get; set; }
        public string FooterText { get; set; }
        public DateTime? DateLastChange { get; set; }
    }

    public static class ClientMailOps
    {
        public const string Contains = "Contains";
        public const string NotContains = "NotContains";
        public const string StartsWith = "StartsWith";
        public const string NotStartsWith = "NotStartsWith";
        public const string EqualsOp = "Equals";
        public const string NotEquals = "NotEquals";
        public const string Is = "Is";
        public const string IsNot = "IsNot";
        public const string OnOrAfter = "OnOrAfter";
        public const string OnOrBefore = "OnOrBefore";
        public const string Between = "Between";
        public const string InLastDays = "InLastDays";
        public const string Matches = "Matches";
    }
}
