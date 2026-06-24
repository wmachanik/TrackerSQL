using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// Repository for AreasTbl.
    ///
    /// Standard CRUD operations are inherited from RepositoryBase<Area> because
    /// AreasTbl now uses column names that match the Area POCO:
    /// AreaID, AreaName, PrepDayOfWeekID and DeliveryDelay.
    ///
    /// Custom methods remain here only for area/contact prep-rule lookups.
    /// </summary>
    public class AreasRepository : RepositoryBase<Area>
    {
        protected override string TableName => "AreasTbl";
        protected override string KeyColumn => "AreaID";

        protected override string CoreColumns =>
            "AreaID, AreaName, PrepDayOfWeekID, DeliveryDelay";

        protected override string LookupColumns =>
            "AreaID, AreaName";

        /// <summary>
        /// Gets all prep/delivery rules for a given area, used for calculating delivery dates.
        /// Returns rules in delivery order for proper scheduling.
        /// </summary>
        public List<PrepRule> GetPrepRulesForArea(int areaId)
        {
            const string sql = @"
                SELECT
                    AreaID,
                    PrepDayOfWeekID,
                    DeliveryDelay,
                    DeliveryOrder
                FROM AreasTbl
                WHERE AreaID = @AreaID
                ORDER BY DeliveryOrder, PrepDayOfWeekID";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    DataValue = areaId,
                    DataDbType = DbType.Int32,
                    ParamName = "@AreaID"
                }
            };

            var list = new List<PrepRule>();

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new PrepRule
                    {
                        PrepDayOfWeekID = GetValue<int>(rdr, "PrepDayOfWeekID"),
                        DeliveryDelayDays = GetValue<int>(rdr, "DeliveryDelay"),
                        DeliveryOrder = GetValue<int>(rdr, "DeliveryOrder")
                    });
                }
            }

            return list;
        }

        /// <summary>
        /// Gets prep rules for a contact by joining to the contact's assigned area.
        /// This is the SQL-based replacement for the legacy AreaTblDAL approach.
        /// </summary>
        public List<PrepRule> GetPrepRulesForContact(int contactId)
        {
            const string sql = @"
                SELECT
                    a.AreaID,
                    a.PrepDayOfWeekID,
                    a.DeliveryDelay,
                    1 AS DeliveryOrder
                FROM ContactsTbl c
                INNER JOIN AreasTbl a ON c.AreaID = a.AreaID
                WHERE c.ContactID = @ContactID
                ORDER BY a.PrepDayOfWeekID";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    DataValue = contactId,
                    DataDbType = DbType.Int32,
                    ParamName = "@ContactID"
                }
            };

            var list = new List<PrepRule>();

            using (var rdr = ExecReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new PrepRule
                    {
                        PrepDayOfWeekID = GetValue<int>(rdr, "PrepDayOfWeekID"),
                        DeliveryDelayDays = GetValue<int>(rdr, "DeliveryDelay"),
                        DeliveryOrder = GetValue<int>(rdr, "DeliveryOrder")
                    });
                }
            }

            return list;
        }

        public string GetAreaName(int areaId)
        {
            var area = GetKeyColsById(areaId);
            return area?.AreaName ?? string.Empty;
        }

        public int GetAreaIdByContactId(long contactId)
        {
            const string sql = "SELECT AreaID FROM ContactsTbl WHERE ContactID = @ContactID";
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int64 }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        private static TValue GetValue<TValue>(IDataRecord record, string name)
        {
            if (!HasColumn(record, name))
            {
                return default(TValue);
            }

            object value = record[name];

            if (value == null || value == DBNull.Value)
            {
                return default(TValue);
            }

            try
            {
                Type targetType = typeof(TValue);
                Type underlyingType = Nullable.GetUnderlyingType(targetType);

                if (underlyingType != null)
                {
                    targetType = underlyingType;
                }

                if (targetType == typeof(string))
                {
                    return (TValue)(object)Convert.ToString(value);
                }

                if (targetType.IsEnum)
                {
                    if (value is string)
                    {
                        return (TValue)Enum.Parse(targetType, value.ToString());
                    }

                    return (TValue)Enum.ToObject(targetType, value);
                }

                if (targetType == typeof(Guid))
                {
                    return (TValue)(object)new Guid(value.ToString());
                }

                return (TValue)Convert.ChangeType(value, targetType);
            }
            catch
            {
                return default(TValue);
            }
        }

        private static bool HasColumn(IDataRecord record, string name)
        {
            for (int i = 0; i < record.FieldCount; i++)
            {
                if (string.Equals(record.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
