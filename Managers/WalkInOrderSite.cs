using System;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Walk-in (ZZName / contact 9) delivery site lives on the order, never on the shared contact.
    /// </summary>
    public static class WalkInOrderSite
    {
        public static bool IsWalkIn(long contactId) =>
            contactId == SystemConstants.CustomerConstants.SundryCustomerID;

        public static bool ColumnsReady => OrdersRepository.HasWalkInSiteColumns();

        public static string AreaJoinSql(string alias)
        {
            if (!ColumnsReady)
                return string.Empty;
            return " LEFT JOIN AreasTbl " + alias + " ON " + alias + ".AreaID = o.WalkInAreaID ";
        }

        private static string ZzNameStandardSql()
        {
            string zz = "o.ContactID = " + SystemConstants.CustomerConstants.SundryCustomerID;
            if (!OrdersRepository.HasDeliverToAddressIdColumn())
                return zz;
            return zz + " AND (o.DeliverToAddressID IS NULL OR o.DeliverToAddressID = 0)";
        }

        public static string AreaNameSql(string contactAreaAlias, string walkAlias)
        {
            if (!ColumnsReady)
                return contactAreaAlias + ".AreaName";
            return "CASE WHEN " + ZzNameStandardSql()
                + " AND " + walkAlias + ".AreaName IS NOT NULL THEN " + walkAlias + ".AreaName ELSE "
                + contactAreaAlias + ".AreaName END";
        }

        public static string AreaCodeSql(string contactAreaAlias, string walkAlias)
        {
            if (!ColumnsReady)
                return contactAreaAlias + ".ShortCode";
            return "CASE WHEN " + ZzNameStandardSql()
                + " AND " + walkAlias + ".AreaID IS NOT NULL THEN " + walkAlias + ".ShortCode ELSE "
                + contactAreaAlias + ".ShortCode END";
        }

        public static string GpsLatSql(string contactLatExpr)
        {
            string zzStd = ZzNameStandardSql();
            if (!ColumnsReady)
                return "CASE WHEN " + zzStd + " THEN CAST(NULL AS FLOAT) ELSE " + contactLatExpr + " END";
            return "CASE WHEN " + zzStd + " THEN CAST(o.WalkInLatitude AS FLOAT) ELSE " + contactLatExpr + " END";
        }

        public static string GpsLngSql(string contactLngExpr)
        {
            string zzStd = ZzNameStandardSql();
            if (!ColumnsReady)
                return "CASE WHEN " + zzStd + " THEN CAST(NULL AS FLOAT) ELSE " + contactLngExpr + " END";
            return "CASE WHEN " + zzStd + " THEN CAST(o.WalkInLongitude AS FLOAT) ELSE " + contactLngExpr + " END";
        }

        /// <summary>Look up the walk-in address and store the point on the order (not CustomerLocationTbl).</summary>
        public static void TryGeocode(OrderHeaderData header, string userName)
        {
            if (header == null || !IsWalkIn(header.CustomerID) || !ColumnsReady)
                return;

            string address = SundryNotes.Address(header.Notes);
            if (string.IsNullOrWhiteSpace(address))
            {
                header.WalkInLatitude = null;
                header.WalkInLongitude = null;
                return;
            }

            if (header.WalkInLatitude.HasValue && header.WalkInLongitude.HasValue)
                return;

            try
            {
                GeocodeResult result = new CustomerLocationManager().SearchAddress(address);
                if (!result.Success)
                    return;
                header.WalkInLatitude = Math.Round((decimal)result.Latitude, 6);
                header.WalkInLongitude = Math.Round((decimal)result.Longitude, 6);
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery,
                    "Walk-in GPS for order " + header.OrderID + ": " + (result.Label ?? address), userName);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "Walk-in GPS lookup: " + ex.Message, userName);
            }
        }
    }
}
