using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

            if (!TryValidateCustomWhere(filter.CustomWhere, out string customError))
                throw new InvalidOperationException(customError);

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

            if (!string.IsNullOrWhiteSpace(filter.NameContains))
            {
                sql.AppendLine(@" AND (
                    c.CompanyName LIKE @Name
                 OR c.ContactFirstName LIKE @Name
                 OR c.ContactLastName LIKE @Name
                 OR CONCAT(ISNULL(c.ContactFirstName,''), ' ', ISNULL(c.ContactLastName,'')) LIKE @Name
                )");
                parameters.Add(new DBParameter
                {
                    ParamName = "@Name",
                    DataValue = "%" + filter.NameContains.Trim() + "%",
                    DataDbType = DbType.String
                });
            }

            if (filter.ContactTypeID.GetValueOrDefault() > 0)
            {
                sql.AppendLine(" AND c.ContactTypeID = @ContactTypeID");
                parameters.Add(new DBParameter
                {
                    ParamName = "@ContactTypeID",
                    DataValue = filter.ContactTypeID.Value,
                    DataDbType = DbType.Int32
                });
            }

            if (filter.EquipTypeID.GetValueOrDefault() > 0)
            {
                sql.AppendLine(" AND c.EquipTypeID = @EquipTypeID");
                parameters.Add(new DBParameter
                {
                    ParamName = "@EquipTypeID",
                    DataValue = filter.EquipTypeID.Value,
                    DataDbType = DbType.Int32
                });
            }

            if (filter.LastOrderedFrom.HasValue)
            {
                sql.AppendLine(" AND lastOrd.RequiredByDate >= @LastFrom");
                parameters.Add(new DBParameter
                {
                    ParamName = "@LastFrom",
                    DataValue = filter.LastOrderedFrom.Value.Date,
                    DataDbType = DbType.Date
                });
            }

            if (filter.LastOrderedTo.HasValue)
            {
                sql.AppendLine(" AND lastOrd.RequiredByDate < @LastTo");
                parameters.Add(new DBParameter
                {
                    ParamName = "@LastTo",
                    DataValue = filter.LastOrderedTo.Value.Date.AddDays(1),
                    DataDbType = DbType.Date
                });
            }

            if (filter.NextRequiredFrom.HasValue)
            {
                sql.AppendLine(" AND u.NextCoffeeBy >= @NextFrom");
                parameters.Add(new DBParameter
                {
                    ParamName = "@NextFrom",
                    DataValue = filter.NextRequiredFrom.Value.Date,
                    DataDbType = DbType.Date
                });
            }

            if (filter.NextRequiredTo.HasValue)
            {
                sql.AppendLine(" AND u.NextCoffeeBy < @NextTo");
                parameters.Add(new DBParameter
                {
                    ParamName = "@NextTo",
                    DataValue = filter.NextRequiredTo.Value.Date.AddDays(1),
                    DataDbType = DbType.Date
                });
            }

            if (filter.AreaIds != null && filter.AreaIds.Count > 0)
            {
                var ids = filter.AreaIds.Where(id => id > 0).Distinct().ToList();
                if (ids.Count > 0)
                {
                    sql.Append(" AND c.AreaID IN (");
                    sql.Append(string.Join(",", ids.Select(id => id.ToString())));
                    sql.AppendLine(")");
                }
            }

            if (filter.IsRecurring == true)
                sql.AppendLine(" AND rec.ContactID IS NOT NULL");
            else if (filter.IsRecurring == false)
                sql.AppendLine(" AND rec.ContactID IS NULL");

            if (filter.LastItemIds != null && filter.LastItemIds.Count > 0)
            {
                var ids = filter.LastItemIds.Where(id => id > 0).Distinct().ToList();
                if (ids.Count > 0)
                {
                    sql.Append(" AND EXISTS (SELECT 1 FROM OrderLinesTbl olx WHERE olx.OrderID = lastOrd.OrderID AND olx.ItemID IN (");
                    sql.Append(string.Join(",", ids.Select(id => id.ToString())));
                    sql.AppendLine("))");
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.CustomWhere))
            {
                sql.Append(" AND (");
                sql.Append(filter.CustomWhere.Trim());
                sql.AppendLine(")");
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
    }
}
