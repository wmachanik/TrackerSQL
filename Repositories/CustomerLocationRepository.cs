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
            LocationStatus, Source, Confidence, CreatedDate, UpdatedDate, UpdatedBy, ExpiresDate, DriverFixes, LastDriverFixAt";

        /// <summary>Azure Maps points are looked up again this many days before they expire.</summary>
        private static readonly string RefreshWindowSql = "DATEADD(day, " + GpsSettings.AzureMapsRefreshDaysBefore + ", GETDATE())";

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

        private static bool _tableSeen;

        /// <summary>Whether sorting queries can join CustomerLocationTbl; only a positive answer is cached.</summary>
        public static bool TableSeen()
        {
            if (!_tableSeen)
                _tableSeen = new CustomerLocationRepository().TableExists();
            return _tableSeen;
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
        Source = @Source, Confidence = @Confidence, UpdatedDate = GETDATE(), UpdatedBy = @UpdatedBy,
        ExpiresDate = @ExpiresDate, DriverFixes = @DriverFixes, LastDriverFixAt = @LastDriverFixAt
    WHERE ContactID = @ContactID;
ELSE
    INSERT INTO dbo.CustomerLocationTbl
        (ContactID, Latitude, Longitude, Geohash, MapAreaID, GeocodedAddress, SourceAddress, LocationStatus, Source, Confidence, UpdatedBy,
         ExpiresDate, DriverFixes, LastDriverFixAt)
    VALUES
        (@ContactID, @Latitude, @Longitude, @Geohash, @MapAreaID, @GeocodedAddress, @SourceAddress, @LocationStatus, @Source, @Confidence, @UpdatedBy,
         @ExpiresDate, @DriverFixes, @LastDriverFixAt);";

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
                    P("@UpdatedBy", Truncate(loc.UpdatedBy, 128), DbType.String),
                    P("@ExpiresDate", loc.ExpiresDate, DbType.DateTime),
                    P("@DriverFixes", loc.DriverFixes, DbType.Int32),
                    P("@LastDriverFixAt", loc.LastDriverFixAt, DbType.DateTime)
                });
        }

        public void Delete(int contactId)
        {
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery("DELETE FROM dbo.CustomerLocationTbl WHERE ContactID = @ContactID",
                    new List<DBParameter> { P("@ContactID", contactId, DbType.Int32) });
        }

        /// <summary>Removes provider points past their keep date (Azure Maps terms). Returns how many went.</summary>
        public int DeleteExpired()
        {
            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<int>(@"
DELETE FROM dbo.CustomerLocationTbl WHERE ExpiresDate IS NOT NULL AND ExpiresDate < GETDATE();
SELECT @@ROWCOUNT;");
        }

        /// <summary>
        /// Phone position of the most recent earlier Delivered proof for this contact (usable accuracy only),
        /// or null. Used to tell a one-off bad fix from a pin that really is in the wrong place.
        /// </summary>
        public Tuple<decimal, decimal> GetPreviousDeliveryFix(int contactId, long excludeProofId, DateTime before, decimal maxAccuracyM)
        {
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(@"
SELECT TOP (1) Latitude, Longitude FROM dbo.MobileDeliveryProofTbl
WHERE ContactID = @ContactID AND ProofID <> @ProofID AND Outcome = 'Delivered'
  AND DeliveredAt < @Before
  AND Latitude IS NOT NULL AND Longitude IS NOT NULL
  AND (AccuracyM IS NULL OR AccuracyM <= @MaxAcc)
ORDER BY DeliveredAt DESC, ProofID DESC",
                new List<DBParameter>
                {
                    P("@ContactID", contactId, DbType.Int32),
                    P("@ProofID", excludeProofId, DbType.Int64),
                    P("@Before", before, DbType.DateTime),
                    P("@MaxAcc", maxAccuracyM, DbType.Decimal)
                }))
            {
                if (r == null || !r.Read())
                    return null;
                return Tuple.Create(Convert.ToDecimal(r["Latitude"]), Convert.ToDecimal(r["Longitude"]));
            }
        }

        /// <summary>A delivery agreed with a hand-placed pin (or a mismatch was noted): only the fix counters change.</summary>
        public void TouchDriverFix(int contactId, int driverFixes, DateTime fixAt, string status)
        {
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(@"
UPDATE dbo.CustomerLocationTbl SET DriverFixes = @DriverFixes, LastDriverFixAt = @FixAt,
    LocationStatus = ISNULL(@Status, LocationStatus)
WHERE ContactID = @ContactID",
                    new List<DBParameter>
                    {
                        P("@ContactID", contactId, DbType.Int32),
                        P("@DriverFixes", driverFixes, DbType.Int32),
                        P("@FixAt", fixAt, DbType.DateTime),
                        P("@Status", status, DbType.AnsiString)
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
        public List<int> ListContactIdsNeedingLookup(int max, bool openOrdersOnly = false)
        {
            string sql = "SELECT TOP (" + Math.Max(1, max) + ") c.ContactID" + NeedingLookupFromWhere(openOrdersOnly)
                + " ORDER BY c.ContactID";
            var ids = new List<int>();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql))
            {
                while (r != null && r.Read())
                    ids.Add(Convert.ToInt32(r["ContactID"]));
            }
            return ids;
        }

        public int CountContactsNeedingLookup(bool openOrdersOnly = false)
        {
            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<int>("SELECT COUNT(*)" + NeedingLookupFromWhere(openOrdersOnly));
        }

        /// <summary>
        /// No location yet (or an Azure Maps point close to expiry) and an address. Default: enabled contacts whose
        /// "Delivery By" is a DELIVER person. Open orders only: contacts with an order not done yet that one of our drivers is to deliver.
        /// </summary>
        private static string NeedingLookupFromWhere(bool openOrdersOnly)
        {
            string common = @"
  AND LEN(LTRIM(RTRIM(ISNULL(c.BillingAddress, N'')))) > 0
  AND NOT EXISTS (SELECT 1 FROM dbo.CustomerLocationTbl l WHERE l.ContactID = c.ContactID
                  AND (l.ExpiresDate IS NULL OR l.ExpiresDate > " + RefreshWindowSql + "))";

            if (openOrdersOnly)
                return @"
FROM dbo.ContactsTbl c
WHERE EXISTS (SELECT 1 FROM dbo.OrdersTbl o
              INNER JOIN dbo.PeopleTbl p ON p.PersonID = o.ToBeDeliveredByID
              WHERE o.ContactID = c.ContactID AND ISNULL(o.Done, 0) = 0 AND p.DeliveryType = 'DELIVER')" + common;

            return @"
FROM dbo.ContactsTbl c
INNER JOIN dbo.PeopleTbl p ON p.PersonID = c.PreferredAgentID
WHERE ISNULL(c.Enabled, 0) = 1
  AND p.DeliveryType = 'DELIVER'" + common;
        }

        /// <summary>Rough points from the geocoder or a postcode (not placed by hand), oldest first.</summary>
        public List<int> ListRoughContactIds(int max)
        {
            string sql = "SELECT TOP (" + Math.Max(1, max) + @") ContactID FROM dbo.CustomerLocationTbl
WHERE LocationStatus = 'REVIEW_REQUIRED' AND ISNULL(Source, '') NOT IN ('MANUAL', 'DRIVER')
ORDER BY UpdatedDate, ContactID";
            var ids = new List<int>();
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql))
            {
                while (r != null && r.Read())
                    ids.Add(Convert.ToInt32(r["ContactID"]));
            }
            return ids;
        }

        /// <summary>Locations that still need a person to check them, with the contact name for the list.</summary>
        public List<KeyValuePair<CustomerLocation, string>> ListNeedingReview(int max)
        {
            string sql = "SELECT TOP (" + Math.Max(1, max) + @") l.ContactID, l.Latitude, l.Longitude, l.Geohash,
    l.MapAreaID, l.GeocodedAddress, l.SourceAddress, l.LocationStatus, l.Source, l.Confidence,
    l.CreatedDate, l.UpdatedDate, l.UpdatedBy, l.ExpiresDate, l.DriverFixes, l.LastDriverFixAt, c.CompanyName
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
                UpdatedBy = r["UpdatedBy"] as string,
                ExpiresDate = r["ExpiresDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["ExpiresDate"]),
                DriverFixes = r["DriverFixes"] == DBNull.Value ? 0 : Convert.ToInt32(r["DriverFixes"]),
                LastDriverFixAt = r["LastDriverFixAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["LastDriverFixAt"])
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
