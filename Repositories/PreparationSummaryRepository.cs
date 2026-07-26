using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class PreparationSummaryRepository
    {
        /// <summary>
        /// Gets preparation summary for orders within a date range.
        /// </summary>
        public List<PreparationSummaryItem> GetPreparationSummary(DateTime dateFrom, DateTime dateTo, bool usePrepDate)
        {
            var list = new List<PreparationSummaryItem>();

            string dateField = usePrepDate ? "OrdersTbl.PrepDate" : "OrdersTbl.RequiredByDate";

            string sql = @"
SELECT 
    ItemsTbl.ItemDesc, 
    ROUND(SUM(OrderLinesTbl.QtyOrdered), 2) AS Quantity 
FROM 
    (OrderLinesTbl 
        INNER JOIN ItemsTbl ON OrderLinesTbl.ItemID = ItemsTbl.ItemID
        INNER JOIN OrdersTbl ON OrderLinesTbl.OrderID = OrdersTbl.OrderID)
WHERE 
    " + dateField + @" >= @DateFrom 
    AND " + dateField + @" <= @DateTo 
    AND ItemsTbl.ItemServiceTypeID = " + SystemConstants.ServiceTypeConstants.CoffeeStr + @"
GROUP BY 
    ItemsTbl.ItemDesc
ORDER BY 
    ItemsTbl.ItemDesc";

            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = dateFrom, DataDbType = DbType.Date, ParamName = "@DateFrom" },
                new DBParameter { DataValue = dateTo, DataDbType = DbType.Date, ParamName = "@DateTo" }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new PreparationSummaryItem
                    {
                        ItemDesc = GetValue<string>(rdr, "ItemDesc"),
                        Quantity = GetValue<double>(rdr, "Quantity")
                    });
                }
            }

            return list;
        }

        private T GetValue<T>(IDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return default(T);

                object value = reader.GetValue(ordinal);
                if (value == null || value == DBNull.Value)
                    return default(T);

                if (typeof(T) == typeof(string))
                    return (T)(object)Convert.ToString(value);

                if (typeof(T) == typeof(int))
                    return (T)(object)Convert.ToInt32(value);

                if (typeof(T) == typeof(double))
                    return (T)(object)Convert.ToDouble(value);

                if (typeof(T) == typeof(decimal))
                    return (T)(object)Convert.ToDecimal(value);

                if (typeof(T) == typeof(bool))
                    return (T)(object)Convert.ToBoolean(value);

                if (typeof(T) == typeof(DateTime))
                    return (T)(object)Convert.ToDateTime(value);

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
    }
}
