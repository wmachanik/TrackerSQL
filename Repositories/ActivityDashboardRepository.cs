using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
  public class ActivityDashboardRepository
  {
    private static bool ColumnExists(TrackerSQLDb db, string table, string column)
    {
      string sql = "SELECT CASE WHEN COL_LENGTH(N'dbo." + table + "', N'" + column + "') IS NULL THEN 0 ELSE 1 END";
      try
      {
        return db.ExecuteScalar<int>(sql) == 1;
      }
      catch
      {
        return false;
      }
    }

    private static bool TableExists(TrackerSQLDb db, string table)
    {
      try
      {
        return db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sys.tables WHERE name = N'" + table + "'") > 0;
      }
      catch
      {
        return false;
      }
    }

    public ActivityPeriodSnapshot LoadSnapshot(int days, DateTime fromDate, DateTime toDate)
    {
      var snap = new ActivityPeriodSnapshot
      {
        Days = days,
        FromDate = fromDate.Date,
        ToDate = toDate.Date
      };

      var parameters = DateParams(fromDate, toDate);

      using (var db = new TrackerSQLDb())
      {
        snap.OrdersAdded = ScalarInt(db,
            @"SELECT COUNT(*) FROM OrdersTbl
              WHERE OrderDate >= @FromDate AND OrderDate < @ToExclusive", parameters);

        snap.OrdersCompleted = ScalarInt(db,
            @"SELECT COUNT(*) FROM OrdersTbl
              WHERE Done = 1 AND RequiredByDate >= @FromDate AND RequiredByDate < @ToExclusive", parameters);

        if (TableExists(db, "WooOrderInfoTbl"))
        {
          snap.WooOrdersImported = ScalarInt(db,
              @"SELECT COUNT(*)
                FROM WooOrderInfoTbl w
                INNER JOIN OrdersTbl o ON o.OrderID = w.OrderID
                WHERE o.OrderDate >= @FromDate AND o.OrderDate < @ToExclusive", parameters);
        }

        if (TableExists(db, "MobileDeliveryProofTbl"))
        {
          snap.DriverDeliveryAttempts = ScalarInt(db,
              @"SELECT COUNT(*) FROM MobileDeliveryProofTbl
                WHERE DeliveredAt >= @FromDate AND DeliveredAt < @ToExclusive", parameters);

          snap.DriverDeliveriesSuccessful = ScalarInt(db,
              @"SELECT COUNT(*) FROM MobileDeliveryProofTbl
                WHERE DeliveredAt >= @FromDate AND DeliveredAt < @ToExclusive
                  AND Outcome = N'Delivered'", parameters);

          if (ColumnExists(db, "MobileDeliveryProofTbl", "PodSentAt"))
          {
            snap.DeliveryConfirmationEmails = ScalarInt(db,
                @"SELECT COUNT(*) FROM MobileDeliveryProofTbl
                  WHERE PodSentAt >= @FromDate AND PodSentAt < @ToExclusive", parameters);
          }
        }

        snap.RepairsLogged = ScalarInt(db,
            @"SELECT COUNT(*) FROM RepairsTbl
              WHERE DateLogged >= @FromDate AND DateLogged < @ToExclusive", parameters);

        snap.RepairsCompleted = ScalarInt(db,
            @"SELECT COUNT(*) FROM RepairsTbl
              WHERE RepairStatusID = @DoneStatus
                AND LastStatusChange >= @FromDate AND LastStatusChange < @ToExclusive",
            WithDoneStatus(parameters));

        snap.CoffeeUnitsDelivered = ScalarDouble(db,
            @"SELECT ISNULL(SUM(ol.QtyOrdered), 0)
              FROM OrderLinesTbl ol
              INNER JOIN ItemsTbl i ON i.ItemID = ol.ItemID
              INNER JOIN OrdersTbl o ON o.OrderID = ol.OrderID
              WHERE o.Done = 1
                AND o.RequiredByDate >= @FromDate AND o.RequiredByDate < @ToExclusive
                AND i.ItemServiceTypeID = @CoffeeServiceType", WithCoffeeType(parameters));

        if (TableExists(db, "PeopleTbl") && ColumnExists(db, "PeopleTbl", "IsDispatched"))
        {
          snap.OrdersCouriered = ScalarInt(db,
              @"SELECT COUNT(*) FROM OrdersTbl o
                INNER JOIN PeopleTbl p ON p.PersonID = o.ToBeDeliveredByID
                WHERE o.Done = 1
                  AND o.RequiredByDate >= @FromDate AND o.RequiredByDate < @ToExclusive
                  AND ISNULL(p.IsDispatched, 0) = 1
                  AND o.ToBeDeliveredByID <> @CollectionPersonId",
              WithCollectionId(parameters));

          snap.OrdersCollected = ScalarInt(db,
              @"SELECT COUNT(*) FROM OrdersTbl
                WHERE Done = 1
                  AND RequiredByDate >= @FromDate AND RequiredByDate < @ToExclusive
                  AND ToBeDeliveredByID = @CollectionPersonId",
              WithCollectionId(parameters));

          snap.OrdersOwnDriver = ScalarInt(db,
              @"SELECT COUNT(*) FROM OrdersTbl o
                LEFT JOIN PeopleTbl p ON p.PersonID = o.ToBeDeliveredByID
                WHERE o.Done = 1
                  AND o.RequiredByDate >= @FromDate AND o.RequiredByDate < @ToExclusive
                  AND o.ToBeDeliveredByID <> @CollectionPersonId
                  AND ISNULL(p.IsDispatched, 0) = 0",
              WithCollectionId(parameters));
        }
        else
        {
          snap.OrdersCollected = ScalarInt(db,
              @"SELECT COUNT(*) FROM OrdersTbl
                WHERE Done = 1
                  AND RequiredByDate >= @FromDate AND RequiredByDate < @ToExclusive
                  AND ToBeDeliveredByID = @CollectionPersonId",
              WithCollectionId(parameters));

          snap.OrdersCouriered = ScalarInt(db,
              @"SELECT COUNT(*) FROM OrdersTbl
                WHERE Done = 1
                  AND RequiredByDate >= @FromDate AND RequiredByDate < @ToExclusive
                  AND ToBeDeliveredByID IN (@CourierPersonId, @ParcelDispatchPersonId)",
              WithLegacyCourierIds(parameters));

          snap.OrdersOwnDriver = Math.Max(0,
              snap.OrdersCompleted - snap.OrdersCollected - snap.OrdersCouriered);
        }

        if (TableExists(db, "ClientMailSendLogTbl"))
        {
          snap.ClientMailEmailsSent = ScalarInt(db,
              @"SELECT ISNULL(SUM(EmailsSent), 0) FROM ClientMailSendLogTbl
                WHERE SentAt >= @FromDate AND SentAt < @ToExclusive", parameters);
        }

        if (TableExists(db, "SentRemindersLogTbl"))
        {
          snap.RemindersSent = ScalarInt(db,
              @"SELECT COUNT(*) FROM SentRemindersLogTbl
                WHERE ReminderSent = 1
                  AND DateSentReminder >= @FromDate AND DateSentReminder < @ToExclusive", parameters);
        }
      }

      return snap;
    }

    public List<ActivityCoffeeByItemRow> CoffeeByItemDelivered(DateTime fromDate, DateTime toDate, int maxRows)
    {
      var list = new List<ActivityCoffeeByItemRow>();
      var parameters = WithCoffeeType(DateParams(fromDate, toDate));

      string sql = @"
SELECT TOP (" + maxRows.ToString(CultureInfo.InvariantCulture) + @")
    i.ItemDesc,
    ROUND(SUM(ol.QtyOrdered), 2) AS Qty
FROM OrderLinesTbl ol
INNER JOIN ItemsTbl i ON i.ItemID = ol.ItemID
INNER JOIN OrdersTbl o ON o.OrderID = ol.OrderID
WHERE o.Done = 1
  AND o.RequiredByDate >= @FromDate AND o.RequiredByDate < @ToExclusive
  AND i.ItemServiceTypeID = @CoffeeServiceType
GROUP BY i.ItemDesc
ORDER BY SUM(ol.QtyOrdered) DESC, i.ItemDesc ASC";

      using (var db = new TrackerSQLDb())
      using (var rdr = db.ExecuteReader(sql, parameters))
      {
        while (rdr != null && rdr.Read())
        {
          list.Add(new ActivityCoffeeByItemRow
          {
            ItemDesc = rdr["ItemDesc"] as string ?? "(unknown)",
            Qty = rdr["Qty"] == DBNull.Value ? 0 : Convert.ToDouble(rdr["Qty"])
          });
        }
      }

      return list;
    }

    public int CountRecurringDueBetween(DateTime windowStart, DateTime windowEnd)
    {
      const string sql = @"
SELECT COUNT(*)
FROM RecurringOrderItemsTbl ri
INNER JOIN RecurringOrdersTbl r ON r.RecurringOrderID = ri.RecurringOrderID
WHERE r.Enabled = 1
  AND ri.NextDateRequired IS NOT NULL
  AND ri.NextDateRequired >= @WindowStart
  AND ri.NextDateRequired <= @WindowEnd";

      var parameters = new List<DBParameter>
      {
        new DBParameter { ParamName = "@WindowStart", DataValue = windowStart.Date, DataDbType = DbType.Date },
        new DBParameter { ParamName = "@WindowEnd", DataValue = windowEnd.Date, DataDbType = DbType.Date }
      };

      try
      {
        using (var db = new TrackerSQLDb())
          return db.ExecuteScalar<int>(sql, parameters);
      }
      catch
      {
        return 0;
      }
    }

    /// <summary>Contacts whose predicted next coffee falls in the window (usage model).</summary>
    public int CountPredictedOrdersBetween(DateTime windowStart, DateTime windowEnd)
    {
      using (var db = new TrackerSQLDb())
      {
        if (!TableExists(db, "ContactsItemsPredictedTbl"))
          return 0;
      }

      const string sql = @"
SELECT COUNT(DISTINCT p.ContactID)
FROM ContactsItemsPredictedTbl p
INNER JOIN ContactsTbl c ON c.ContactID = p.ContactID
WHERE p.NextCoffeeBy IS NOT NULL
  AND p.NextCoffeeBy >= @WindowStart
  AND p.NextCoffeeBy <= @WindowEnd
  AND ISNULL(c.Enabled, 1) = 1";

      var parameters = new List<DBParameter>
      {
        new DBParameter { ParamName = "@WindowStart", DataValue = windowStart.Date, DataDbType = DbType.Date },
        new DBParameter { ParamName = "@WindowEnd", DataValue = windowEnd.Date, DataDbType = DbType.Date }
      };

      try
      {
        using (var db = new TrackerSQLDb())
          return db.ExecuteScalar<int>(sql, parameters);
      }
      catch
      {
        return 0;
      }
    }

    private static List<DBParameter> DateParams(DateTime fromDate, DateTime toDate)
    {
      return new List<DBParameter>
      {
        new DBParameter { ParamName = "@FromDate", DataValue = fromDate.Date, DataDbType = DbType.Date },
        new DBParameter
        {
          ParamName = "@ToExclusive",
          DataValue = toDate.Date.AddDays(1),
          DataDbType = DbType.Date
        }
      };
    }

    private static List<DBParameter> WithDoneStatus(List<DBParameter> parameters)
    {
      var list = new List<DBParameter>(parameters)
      {
        new DBParameter
        {
          ParamName = "@DoneStatus",
          DataValue = RepairsRepository.DoneStatusId,
          DataDbType = DbType.Int32
        }
      };
      return list;
    }

    private static List<DBParameter> WithCoffeeType(List<DBParameter> parameters)
    {
      var list = new List<DBParameter>(parameters)
      {
        new DBParameter
        {
          ParamName = "@CoffeeServiceType",
          DataValue = SystemConstants.ServiceTypeConstants.Coffee,
          DataDbType = DbType.Int32
        }
      };
      return list;
    }

    private static List<DBParameter> WithCollectionId(List<DBParameter> parameters)
    {
      var list = new List<DBParameter>(parameters)
      {
        new DBParameter
        {
          ParamName = "@CollectionPersonId",
          DataValue = SystemConstants.DeliveryConstants.CollectionID,
          DataDbType = DbType.Int32
        }
      };
      return list;
    }

    private static List<DBParameter> WithLegacyCourierIds(List<DBParameter> parameters)
    {
      var list = WithCollectionId(parameters);
      list.Add(new DBParameter
      {
        ParamName = "@CourierPersonId",
        DataValue = SystemConstants.DeliveryConstants.CourierDeliveryID,
        DataDbType = DbType.Int32
      });
      list.Add(new DBParameter
      {
        ParamName = "@ParcelDispatchPersonId",
        DataValue = SystemConstants.DeliveryConstants.ParcelDispatchID,
        DataDbType = DbType.Int32
      });
      return list;
    }

    private static int ScalarInt(TrackerSQLDb db, string sql, List<DBParameter> parameters)
    {
      try
      {
        return db.ExecuteScalar<int>(sql, parameters);
      }
      catch
      {
        return 0;
      }
    }

    private static double ScalarDouble(TrackerSQLDb db, string sql, List<DBParameter> parameters)
    {
      try
      {
        return db.ExecuteScalar<double>(sql, parameters);
      }
      catch
      {
        return 0;
      }
    }
  }
}