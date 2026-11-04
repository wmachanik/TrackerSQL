using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ClientMailRepository
    {
        private static readonly Regex DangerousSql = new Regex(
            @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|EXEC|EXECUTE|MERGE|TRUNCATE|CREATE|GRANT|REVOKE|DENY|UNION|OPENROWSET|OPENQUERY|XP_|WAITFOR|SHUTDOWN|BACKUP|RESTORE|INTO|SELECT)\b|--|/\*|\*/|;",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool TryValidateCustomWhere(string fragment, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(fragment))
                return true;

            if (fragment.Length > 2000)
            {
                error = "Custom Where is too long (max 2000 characters).";
                return false;
            }

            if (DangerousSql.IsMatch(fragment))
            {
                error = "Custom Where is not allowed: no SELECT/DML/DDL, comments, or semicolons. Use a filter expression only (for example c.AreaID IN (1,2)).";
                return false;
            }

            return true;
        }

        public List<ClientMailRecipient> Search(ClientMailFilter filter)
        {
            if (filter == null)
                filter = new ClientMailFilter();

            var sql = new StringBuilder(@"
SELECT
    c.ContactID,
    c.CompanyName,
    c.ContactFirstName,
    c.ContactLastName,
    c.EmailAddress,
    c.AltEmailAddress,
    ct.ContactTypeDesc,
    et.EquipTypeName,
    a.AreaName,
    lastOrd.RequiredByDate AS LastOrderDate,
    u.NextCoffeeBy,
    lastItems.LastItemsOrdered,
    lastTypes.LastItemTypes,
    c.PhoneNumber,
    c.CellNumber,
    CAST(CASE WHEN rec.ContactID IS NULL THEN 0 ELSE 1 END AS BIT) AS IsRecurring
FROM ContactsTbl c
LEFT JOIN ContactTypesTbl ct ON ct.ContactTypeID = c.ContactTypeID
LEFT JOIN EquipTypesTbl et ON et.EquipTypeID = c.EquipTypeID
LEFT JOIN AreasTbl a ON a.AreaID = c.AreaID
LEFT JOIN ContactsItemsPredictedTbl u ON u.ContactID = c.ContactID
OUTER APPLY (
    SELECT TOP 1 o.OrderID, o.RequiredByDate
    FROM OrdersTbl o
    WHERE o.ContactID = c.ContactID AND o.Done = 1
    ORDER BY o.RequiredByDate DESC, o.OrderID DESC
) lastOrd
OUTER APPLY (
    SELECT STUFF((
        SELECT ', ' + ISNULL(NULLIF(i.ItemShortName, ''), i.ItemDesc)
        FROM OrderLinesTbl ol
        INNER JOIN ItemsTbl i ON i.ItemID = ol.ItemID
        WHERE ol.OrderID = lastOrd.OrderID
        FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, '') AS LastItemsOrdered
) lastItems
OUTER APPLY (
    SELECT STUFF((
        SELECT ', ' + types.ItemServiceTypeName
        FROM (
            SELECT DISTINCT st.ItemServiceTypeName
            FROM OrderLinesTbl ol
            INNER JOIN ItemsTbl i ON i.ItemID = ol.ItemID
            INNER JOIN ItemServiceTypesTbl st ON st.ItemServiceTypeID = i.ItemServiceTypeID
            WHERE ol.OrderID = lastOrd.OrderID
        ) types
        FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, '') AS LastItemTypes
) lastTypes
OUTER APPLY (
    SELECT TOP 1 r.ContactID
    FROM RecurringOrdersTbl r
    WHERE r.ContactID = c.ContactID AND ISNULL(r.Enabled, 0) = 1
) rec
WHERE 1 = 1
");

            var parameters = new List<DBParameter>();

            if (filter.EnabledOnly)
                sql.AppendLine(" AND ISNULL(c.Enabled, 0) = 1");

            if (filter.MustHaveEmail)
            {
                sql.AppendLine(@" AND (
                    (c.EmailAddress IS NOT NULL AND LTRIM(RTRIM(c.EmailAddress)) <> '')
                 OR (c.AltEmailAddress IS NOT NULL AND LTRIM(RTRIM(c.AltEmailAddress)) <> '')
                )");
            }

            var rules = filter.Rules ?? new List<ClientMailRule>();
            int paramIndex = 0;
            string combined = CombineRules(rules, parameters, ref paramIndex);

            if (!string.IsNullOrEmpty(combined))
            {
                sql.Append(" AND ");
                sql.AppendLine(combined);
            }

            sql.AppendLine(" ORDER BY c.CompanyName, c.ContactFirstName");

            var list = new List<ClientMailRecipient>();
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql.ToString(), parameters))
            {
                while (rdr != null && rdr.Read())
                    list.Add(DbMapper.Map<ClientMailRecipient>(rdr));
            }

            return list;
        }

        private static string CombineRules(List<ClientMailRule> rules, List<DBParameter> parameters, ref int paramIndex)
        {
            var expr = new StringBuilder();
            int depth = 0;
            bool first = true;
            foreach (var rule in rules)
            {
                if (rule == null)
                    continue;
                string clause = BuildRuleClause(rule, parameters, ref paramIndex, out string ruleError);
                if (!string.IsNullOrEmpty(ruleError))
                    throw new InvalidOperationException(ruleError);
                if (string.IsNullOrWhiteSpace(clause))
                    continue;

                if (!first)
                {
                    expr.Append(string.Equals(rule.Join, "Or", StringComparison.OrdinalIgnoreCase) ? " OR " : " AND ");
                }
                first = false;

                if (rule.GroupOpen)
                {
                    expr.Append("(");
                    depth++;
                }
                expr.Append("(");
                expr.Append(clause);
                expr.Append(")");
                if (rule.GroupClose)
                {
                    if (depth <= 0)
                        throw new InvalidOperationException("A close bracket has no matching open bracket. Tick ( before the group and ) on its last condition.");
                    expr.Append(")");
                    depth--;
                }
            }

            if (depth != 0)
                throw new InvalidOperationException("Open and close brackets must match. Tick ( on the first condition in a group and ) on the last.");

            return expr.Length == 0 ? null : expr.ToString();
        }

        private static string BuildRuleClause(ClientMailRule rule, List<DBParameter> parameters, ref int paramIndex, out string error)
        {
            error = null;
            string field = rule.Field ?? string.Empty;
            string op = rule.Operator ?? string.Empty;

            if (field == ClientMailFields.Name)
            {
                ResolveTextCompare(op, rule.Value, out string cmp, out string likeValue, out bool negated);
                string p = AddParam(parameters, ref paramIndex, likeValue, DbType.String);
                string inner = $@"(c.CompanyName {cmp} {p}
 OR c.ContactFirstName {cmp} {p}
 OR c.ContactLastName {cmp} {p}
 OR CONCAT(ISNULL(c.ContactFirstName,''), ' ', ISNULL(c.ContactLastName,'')) {cmp} {p})";
                return negated ? "NOT " + inner : inner;
            }

            if (field == ClientMailFields.EmailPrimary || field == ClientMailFields.EmailAll)
            {
                ResolveTextCompare(op, rule.Value, out string cmp, out string likeValue, out bool negated);
                string p = AddParam(parameters, ref paramIndex, likeValue, DbType.String);
                string inner;
                if (field == ClientMailFields.EmailAll)
                {
                    inner = $@"(ISNULL(c.EmailAddress,'') {cmp} {p}
 OR ISNULL(c.AltEmailAddress,'') {cmp} {p})";
                }
                else
                {
                    inner = $"(ISNULL(c.EmailAddress,'') {cmp} {p})";
                }
                return negated ? "NOT " + inner : inner;
            }

            if (field == ClientMailFields.ContactType)
            {
                int id;
                if (!int.TryParse(rule.Value, out id) || id <= 0)
                {
                    error = "Choose a contact type.";
                    return null;
                }
                string p = AddParam(parameters, ref paramIndex, id, DbType.Int32);
                return op == ClientMailOps.IsNot ? "c.ContactTypeID <> " + p : "c.ContactTypeID = " + p;
            }

            if (field == ClientMailFields.Equipment)
            {
                int id;
                if (!int.TryParse(rule.Value, out id) || id <= 0)
                {
                    error = "Choose an equipment type.";
                    return null;
                }
                string p = AddParam(parameters, ref paramIndex, id, DbType.Int32);
                return op == ClientMailOps.IsNot ? "c.EquipTypeID <> " + p : "c.EquipTypeID = " + p;
            }

            if (field == ClientMailFields.Area)
            {
                int id;
                if (!int.TryParse(rule.Value, out id) || id <= 0)
                {
                    error = "Choose an area.";
                    return null;
                }
                string p = AddParam(parameters, ref paramIndex, id, DbType.Int32);
                return op == ClientMailOps.IsNot ? "c.AreaID <> " + p : "c.AreaID = " + p;
            }

            if (field == ClientMailFields.Recurring)
            {
                bool yes = rule.Value == "1" || string.Equals(rule.Value, "Yes", StringComparison.OrdinalIgnoreCase);
                return yes ? "rec.ContactID IS NOT NULL" : "rec.ContactID IS NULL";
            }

            if (field == ClientMailFields.LastItemType)
            {
                int id;
                if (!int.TryParse(rule.Value, out id) || id <= 0)
                {
                    error = "Choose a last item type.";
                    return null;
                }
                string p = AddParam(parameters, ref paramIndex, id, DbType.Int32);
                string exists = @"EXISTS (
                    SELECT 1 FROM OrderLinesTbl olx
                    INNER JOIN ItemsTbl ix ON ix.ItemID = olx.ItemID
                    WHERE olx.OrderID = lastOrd.OrderID AND ix.ItemServiceTypeID = " + p + ")";
                return op == ClientMailOps.IsNot ? "NOT " + exists : exists;
            }

            if (field == ClientMailFields.LastItem)
            {
                int id;
                if (!int.TryParse(rule.Value, out id) || id <= 0)
                {
                    error = "Choose a last item.";
                    return null;
                }
                string p = AddParam(parameters, ref paramIndex, id, DbType.Int32);
                string exists = "EXISTS (SELECT 1 FROM OrderLinesTbl olx WHERE olx.OrderID = lastOrd.OrderID AND olx.ItemID = " + p + ")";
                return op == ClientMailOps.IsNot ? "NOT " + exists : exists;
            }

            if (field == ClientMailFields.LastOrdered)
                return BuildDateClause("lastOrd.RequiredByDate", op, rule, parameters, ref paramIndex, out error);

            if (field == ClientMailFields.NextRequired)
                return BuildDateClause("u.NextCoffeeBy", op, rule, parameters, ref paramIndex, out error);

            if (field == ClientMailFields.CustomWhere)
            {
                if (!TryValidateCustomWhere(rule.Value, out error))
                    return null;
                if (string.IsNullOrWhiteSpace(rule.Value))
                {
                    error = "Enter a custom Where fragment.";
                    return null;
                }
                return "(" + rule.Value.Trim() + ")";
            }

            error = "Unknown filter field.";
            return null;
        }

        private static void ResolveTextCompare(string op, string value, out string cmp, out string likeValue, out bool negated)
        {
            string raw = (value ?? string.Empty).Trim();
            negated = op == ClientMailOps.NotContains || op == ClientMailOps.NotStartsWith || op == ClientMailOps.NotEquals;
            if (op == ClientMailOps.EqualsOp || op == ClientMailOps.NotEquals)
            {
                cmp = "=";
                likeValue = raw;
                return;
            }
            cmp = "LIKE";
            if (op == ClientMailOps.StartsWith || op == ClientMailOps.NotStartsWith)
                likeValue = raw + "%";
            else
                likeValue = "%" + raw + "%";
        }

        private static string BuildDateClause(string column, string op, ClientMailRule rule, List<DBParameter> parameters, ref int paramIndex, out string error)
        {
            error = null;
            if (op == ClientMailOps.InLastDays)
            {
                int days;
                if (!int.TryParse(rule.Value, out days) || days < 0 || days > 3650)
                {
                    error = "Enter days between 0 and 3650.";
                    return null;
                }
                string p = AddParam(parameters, ref paramIndex, days, DbType.Int32);
                return column + " >= DATEADD(day, -" + p + ", CAST(GETDATE() AS date))";
            }

            DateTime d1;
            if (op == ClientMailOps.Between)
            {
                if (!TryParseDate(rule.Value, out d1) || !TryParseDate(rule.Value2, out DateTime d2))
                {
                    error = "Enter both From and To dates.";
                    return null;
                }
                string p1 = AddParam(parameters, ref paramIndex, d1.Date, DbType.Date);
                string p2 = AddParam(parameters, ref paramIndex, d2.Date.AddDays(1), DbType.Date);
                return column + " >= " + p1 + " AND " + column + " < " + p2;
            }

            if (!TryParseDate(rule.Value, out d1))
            {
                error = "Enter a date.";
                return null;
            }

            if (op == ClientMailOps.OnOrBefore)
            {
                string p = AddParam(parameters, ref paramIndex, d1.Date.AddDays(1), DbType.Date);
                return column + " < " + p;
            }

            string pa = AddParam(parameters, ref paramIndex, d1.Date, DbType.Date);
            return column + " >= " + pa;
        }

        private static bool TryParseDate(string text, out DateTime d)
        {
            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out d))
                return true;
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
        }

        private static string AddParam(List<DBParameter> parameters, ref int paramIndex, object value, DbType type)
        {
            string name = "@p" + paramIndex;
            paramIndex++;
            parameters.Add(new DBParameter { ParamName = name, DataValue = value, DataDbType = type });
            return name;
        }
    }
}
