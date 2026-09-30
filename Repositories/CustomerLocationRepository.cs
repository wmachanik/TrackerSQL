using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class CustomerLocationRepository
    {
        private const string SelectColumns = @"ContactID, Latitude, Longitude, Geohash, MapAreaID, GeocodedAddress, SourceAddress,
            LocationStatus, Source, Confidence, CreatedDate, UpdatedDate, UpdatedBy";

        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID(N'dbo.CustomerLocationTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                return false;
            }
        }

        public CustomerLocation GetByContactId(int contactId)
        {
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(
                "SELECT " + SelectColumns + " FROM dbo.CustomerLocationTbl WHERE ContactID = @ContactID",
                new List<DBParameter> { P("@ContactID", contactId, DbType.Int32) }))
            {
                return r != null && r.Read() ? Map(r) : null;
            }
        }

        public void Upsert(CustomerLocation loc)
        {
            if (loc == null) throw new ArgumentNullException(nameof(loc));
            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.CustomerLocationTbl WHERE ContactID = @ContactID)
    UPDATE dbo.CustomerLocationTbl SET
        Latitude = @Latitude, Longitude = @Longitude, Geohash = @Geohash, MapAreaID = @MapAreaID,
        GeocodedAddress = @GeocodedAddress, SourceAddress = @SourceAddress, LocationStatus = @LocationStatus,
        Source = @Source, Confidence = @Confidence, UpdatedDate = GETDATE(), UpdatedBy = @UpdatedBy
    WHERE ContactID = @ContactID;
ELSE
    INSERT INTO dbo.CustomerLocationTbl
        (ContactID, Latitude, Longitude, Geohash, MapAreaID, GeocodedAddress, SourceAddress, LocationStatus, Source, Confidence, UpdatedBy)
    VALUES
        (@ContactID, @Latitude, @Longitude, @Geohash, @MapAreaID, @GeocodedAddress, @SourceAddress, @LocationStatus, @Source, @Confidence, @UpdatedBy);";

            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(sql, new List<DBParameter>
                {
                    P("@ContactID", loc.ContactID, DbType.Int32),
                    P("@Latitude", Math.Round(loc.Latitude, 6), DbType.Decimal),
                    P("@Longitude", Math.Round(loc.Longitude, 6), DbType.Decimal),
                    P("@Geohash", loc.Geohash ?? string.Empty, DbType.AnsiString),
                    P("@MapAreaID", loc.MapAreaID, DbType.Int32),
                    P("@GeocodedAddress", Truncate(loc.GeocodedAddress, 500), DbType.String),
                    P("@SourceAddress", Truncate(loc.SourceAddress, 500), DbType.String),
                    P("@LocationStatus", loc.LocationStatus ?? CustomerLocation.StatusApproximate, DbType.AnsiString),
                    P("@Source", loc.Source, DbType.AnsiString),
                    P("@Confidence", loc.Confidence.HasValue ? Math.Round(loc.Confidence.Value, 3) : (decimal?)null, DbType.Decimal),
                    P("@UpdatedBy", Truncate(loc.UpdatedBy, 128), DbType.String)
                });
        }

        public void SetStatus(int contactId, string status, string userName)
        {
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(
                    "UPDATE dbo.CustomerLocationTbl SET LocationStatus = @Status, UpdatedDate = GETDATE(), UpdatedBy = @UpdatedBy WHERE ContactID = @ContactID",
                    new List<DBParameter>
                    {
                        P("@ContactID", contactId, DbType.Int32),
                        P("@Status", status, DbType.AnsiString),
                        P("@UpdatedBy", Truncate(userName, 128), DbType.String)
                    });
        }

        /// <summary>
        /// Enabled contacts whose "Delivery By" person is one of our drivers (DELIVER) and who have no location yet
        /// or whose address changed since it was looked up (verified points are kept).
        /// </summary>
        public List<int> ListContactIdsNeedingLookup(int max)
        {
            string sql = "SELECT TOP (" + Math.Max(1, max) + @") c.ContactID
FROM dbo.ContactsTbl c
INNER JOIN dbo.PeopleTbl p ON p.PersonID = c.PreferredAgentID
LEFT JOIN dbo.CustomerLocationTbl l ON l.ContactID = c.ContactID
WHERE ISNULL(c.Enabled, 0) = 1
  AND p.DeliveryType = 'DELIVER'
  AND LEN(LTRIM(RTRIM(ISNULL(c.BillingAddress, N'')))) > 0
  AND l.ContactID IS NULL
ORDER BY c.ContactID";
            var ids = new List<int>();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql))
            {
                while (r != null && r.Read())
                    ids.Add(Convert.ToInt32(r["ContactID"]));
            }
            return ids;
        }

        public int CountContactsNeedingLookup()
        {
            const string sql = @"
SELECT COUNT(*)
FROM dbo.ContactsTbl c
INNER JOIN dbo.PeopleTbl p ON p.PersonID = c.PreferredAgentID
LEFT JOIN dbo.CustomerLocationTbl l ON l.ContactID = c.ContactID
WHERE ISNULL(c.Enabled, 0) = 1
  AND p.DeliveryType = 'DELIVER'
  AND LEN(LTRIM(RTRIM(ISNULL(c.BillingAddress, N'')))) > 0
  AND l.ContactID IS NULL";
            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<int>(sql);
        }

        /// <summary>Locations that still need a person to check them, with the contact name for the list.</summary>
        public List<KeyValuePair<CustomerLocation, string>> ListNeedingReview(int max)
        {
            string sql = "SELECT TOP (" + Math.Max(1, max) + @") l.ContactID, l.Latitude, l.Longitude, l.Geohash,
    l.MapAreaID, l.GeocodedAddress, l.SourceAddress, l.LocationStatus, l.Source, l.Confidence,
    l.CreatedDate, l.UpdatedDate, l.UpdatedBy, c.CompanyName
FROM dbo.CustomerLocationTbl l
INNER JOIN dbo.ContactsTbl c ON c.ContactID = l.ContactID
WHERE l.LocationStatus = 'REVIEW_REQUIRED'
ORDER BY c.CompanyName";
            var list = new List<KeyValuePair<CustomerLocation, string>>();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql))
            {
                while (r != null && r.Read())
                    list.Add(new KeyValuePair<CustomerLocation, string>(Map(r), r["CompanyName"] as string));
            }
            return list;
        }

        private static CustomerLocation Map(IDataRecord r)
        {
            return new CustomerLocation
            {
                ContactID = Convert.ToInt32(r["ContactID"]),
                Latitude = Convert.ToDecimal(r["Latitude"]),
                Longitude = Convert.ToDecimal(r["Longitude"]),
                Geohash = r["Geohash"] as string,
                MapAreaID = r["MapAreaID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["MapAreaID"]),
                GeocodedAddress = r["GeocodedAddress"] as string,
                SourceAddress = r["SourceAddress"] as string,
                LocationStatus = r["LocationStatus"] as string,
                Source = r["Source"] as string,
                Confidence = r["Confidence"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["Confidence"]),
                CreatedDate = Convert.ToDateTime(r["CreatedDate"]),
                UpdatedDate = Convert.ToDateTime(r["UpdatedDate"]),
                UpdatedBy = r["UpdatedBy"] as string
            };
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= max ? value : value.Substring(0, max);
        }

        private static DBParameter P(string name, object value, DbType type)
        {
            return new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
        }
    }
}
