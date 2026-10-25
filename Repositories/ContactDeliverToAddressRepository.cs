using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactDeliverToAddressRepository
    {
        private static bool? _tableSeen;

        public static bool TableSeen()
        {
            if (_tableSeen.HasValue)
                return _tableSeen.Value;
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    _tableSeen = db.ExecuteScalar<int>(@"
SELECT CASE WHEN OBJECT_ID(N'dbo.ContactDeliverToAddressTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
                }
            }
            catch
            {
                _tableSeen = false;
            }
            return _tableSeen.Value;
        }

        public List<ContactDeliverToAddress> ListForContact(int contactId, bool enabledOnly = false)
        {
            if (contactId <= 0 || !TableSeen())
                return new List<ContactDeliverToAddress>();

            string sql = @"
                SELECT d.DeliverToAddressID, d.ContactID, d.Label, d.DeliverToPerson,
                       d.Address1, d.Address2, d.Address3, d.AreaID, d.StateOrProvince, d.PostalCode,
                       d.Latitude, d.Longitude, d.Geohash, d.GeocodedAddress, d.SourceAddress,
                       d.LocationStatus, d.Source, d.LocationUpdatedDate, d.LocationUpdatedBy,
                       d.SortOrder, d.Enabled, d.ToBeDeliveredByID,
                       a.AreaName,
                       COALESCE(p.Abbreviation, p.Person) AS DeliveryByDisplay
                FROM dbo.ContactDeliverToAddressTbl d
                LEFT JOIN dbo.AreasTbl a ON a.AreaID = d.AreaID
                LEFT JOIN dbo.PeopleTbl p ON p.PersonID = d.ToBeDeliveredByID
                WHERE d.ContactID = @ContactID"
                + (enabledOnly ? " AND d.Enabled = 1" : string.Empty)
                + " ORDER BY d.SortOrder, d.Label";

            var list = new List<ContactDeliverToAddress>();
            var p = new List<DBParameter> { new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                while (rdr.Read())
                    list.Add(Map(rdr));
            }
            return list;
        }

        public ContactDeliverToAddress GetById(int id)
        {
            if (id <= 0 || !TableSeen())
                return null;

            const string sql = @"
                SELECT d.DeliverToAddressID, d.ContactID, d.Label, d.DeliverToPerson,
                       d.Address1, d.Address2, d.Address3, d.AreaID, d.StateOrProvince, d.PostalCode,
                       d.Latitude, d.Longitude, d.Geohash, d.GeocodedAddress, d.SourceAddress,
                       d.LocationStatus, d.Source, d.LocationUpdatedDate, d.LocationUpdatedBy,
                       d.SortOrder, d.Enabled, d.ToBeDeliveredByID,
                       a.AreaName,
                       COALESCE(p.Abbreviation, p.Person) AS DeliveryByDisplay
                FROM dbo.ContactDeliverToAddressTbl d
                LEFT JOIN dbo.AreasTbl a ON a.AreaID = d.AreaID
                LEFT JOIN dbo.PeopleTbl p ON p.PersonID = d.ToBeDeliveredByID
                WHERE d.DeliverToAddressID = @Id";

            var p = new List<DBParameter> { new DBParameter { ParamName = "@Id", DataValue = id, DataDbType = DbType.Int32 } };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                return rdr.Read() ? Map(rdr) : null;
            }
        }

        public int Insert(ContactDeliverToAddress row)
        {
            if (row == null || row.ContactID <= 0 || !TableSeen())
                return 0;

            if (row.SortOrder <= 0)
                row.SortOrder = NextSortOrder(row.ContactID);

            const string sql = @"
                INSERT INTO dbo.ContactDeliverToAddressTbl (
                    ContactID, Label, DeliverToPerson, Address1, Address2, Address3,
                    AreaID, StateOrProvince, PostalCode, Latitude, Longitude, ToBeDeliveredByID, SortOrder, Enabled)
                VALUES (
                    @ContactID, @Label, @DeliverToPerson, @Address1, @Address2, @Address3,
                    @AreaID, @StateOrProvince, @PostalCode, @Latitude, @Longitude, @ToBeDeliveredByID, @SortOrder, @Enabled);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<int>(sql, Params(row, includeId: false));
        }

        public bool Update(ContactDeliverToAddress row)
        {
            if (row == null || row.DeliverToAddressID <= 0 || !TableSeen())
                return false;

            const string sql = @"
                UPDATE dbo.ContactDeliverToAddressTbl SET
                    Label = @Label,
                    DeliverToPerson = @DeliverToPerson,
                    Address1 = @Address1,
                    Address2 = @Address2,
                    Address3 = @Address3,
                    AreaID = @AreaID,
                    StateOrProvince = @StateOrProvince,
                    PostalCode = @PostalCode,
                    Latitude = @Latitude,
                    Longitude = @Longitude,
                    ToBeDeliveredByID = @ToBeDeliveredByID,
                    SortOrder = @SortOrder,
                    Enabled = @Enabled
                WHERE DeliverToAddressID = @DeliverToAddressID AND ContactID = @ContactID";

            return ExecNonQuery(sql, Params(row, includeId: true)) > 0;
        }

        public void UpdateGps(ContactDeliverToAddress row)
        {
            if (row == null || row.DeliverToAddressID <= 0 || !TableSeen())
                return;

            const string sql = @"
                UPDATE dbo.ContactDeliverToAddressTbl SET
                    Latitude = @Latitude,
                    Longitude = @Longitude,
                    Geohash = @Geohash,
                    GeocodedAddress = @GeocodedAddress,
                    SourceAddress = @SourceAddress,
                    LocationStatus = @LocationStatus,
                    Source = @Source,
                    LocationUpdatedDate = GETDATE(),
                    LocationUpdatedBy = @LocationUpdatedBy
                WHERE DeliverToAddressID = @DeliverToAddressID AND ContactID = @ContactID";

            ExecNonQuery(sql, new List<DBParameter>
            {
                new DBParameter { ParamName = "@DeliverToAddressID", DataValue = row.DeliverToAddressID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = row.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Latitude", DataValue = row.Latitude ?? (object)DBNull.Value, DataDbType = DbType.Decimal },
                new DBParameter { ParamName = "@Longitude", DataValue = row.Longitude ?? (object)DBNull.Value, DataDbType = DbType.Decimal },
                new DBParameter { ParamName = "@Geohash", DataValue = (object)row.Geohash ?? DBNull.Value, DataDbType = DbType.AnsiString },
                new DBParameter { ParamName = "@GeocodedAddress", DataValue = (object)row.GeocodedAddress ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@SourceAddress", DataValue = (object)row.SourceAddress ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@LocationStatus", DataValue = (object)row.LocationStatus ?? DBNull.Value, DataDbType = DbType.AnsiString },
                new DBParameter { ParamName = "@Source", DataValue = (object)row.Source ?? DBNull.Value, DataDbType = DbType.AnsiString },
                new DBParameter { ParamName = "@LocationUpdatedBy", DataValue = (object)row.LocationUpdatedBy ?? DBNull.Value, DataDbType = DbType.String }
            });
        }

        public void ClearGps(int deliverToAddressId, int contactId)
        {
            if (deliverToAddressId <= 0 || contactId <= 0 || !TableSeen())
                return;

            ExecNonQuery(@"
                UPDATE dbo.ContactDeliverToAddressTbl SET
                    Latitude = NULL, Longitude = NULL, Geohash = NULL, GeocodedAddress = NULL,
                    SourceAddress = NULL, LocationStatus = NULL, Source = NULL,
                    LocationUpdatedDate = NULL, LocationUpdatedBy = NULL
                WHERE DeliverToAddressID = @Id AND ContactID = @ContactID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Id", DataValue = deliverToAddressId, DataDbType = DbType.Int32 },
                    new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
                });
        }

        public bool Delete(int deliverToAddressId, int contactId)
        {
            if (deliverToAddressId <= 0 || contactId <= 0 || !TableSeen())
                return false;

            return ExecNonQuery(
                "DELETE FROM dbo.ContactDeliverToAddressTbl WHERE DeliverToAddressID = @Id AND ContactID = @ContactID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Id", DataValue = deliverToAddressId, DataDbType = DbType.Int32 },
                    new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
                }) > 0;
        }

        private int NextSortOrder(int contactId)
        {
            using (var db = new TrackerSQLDb())
            {
                object o = db.ExecuteScalar(
                    "SELECT MAX(SortOrder) FROM dbo.ContactDeliverToAddressTbl WHERE ContactID = @ContactID",
                    new List<DBParameter> { new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 } });
                int max = o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
                return max + 10;
            }
        }

        private static ContactDeliverToAddress Map(IDataRecord r)
        {
            return new ContactDeliverToAddress
            {
                DeliverToAddressID = Convert.ToInt32(r["DeliverToAddressID"]),
                ContactID = Convert.ToInt32(r["ContactID"]),
                Label = Str(r, "Label"),
                DeliverToPerson = Str(r, "DeliverToPerson"),
                Address1 = Str(r, "Address1"),
                Address2 = Str(r, "Address2"),
                Address3 = Str(r, "Address3"),
                AreaID = NInt(r, "AreaID"),
                StateOrProvince = Str(r, "StateOrProvince"),
                PostalCode = Str(r, "PostalCode"),
                Latitude = NDec(r, "Latitude"),
                Longitude = NDec(r, "Longitude"),
                Geohash = DbMapper.HasColumn(r, "Geohash") ? Str(r, "Geohash") : null,
                GeocodedAddress = DbMapper.HasColumn(r, "GeocodedAddress") ? Str(r, "GeocodedAddress") : null,
                SourceAddress = DbMapper.HasColumn(r, "SourceAddress") ? Str(r, "SourceAddress") : null,
                LocationStatus = DbMapper.HasColumn(r, "LocationStatus") ? Str(r, "LocationStatus") : null,
                Source = DbMapper.HasColumn(r, "Source") ? Str(r, "Source") : null,
                LocationUpdatedDate = DbMapper.HasColumn(r, "LocationUpdatedDate") && r["LocationUpdatedDate"] != DBNull.Value
                    ? (DateTime?)Convert.ToDateTime(r["LocationUpdatedDate"]) : null,
                LocationUpdatedBy = DbMapper.HasColumn(r, "LocationUpdatedBy") ? Str(r, "LocationUpdatedBy") : null,
                SortOrder = Convert.ToInt32(r["SortOrder"]),
                Enabled = Convert.ToBoolean(r["Enabled"]),
                ToBeDeliveredByID = DbMapper.HasColumn(r, "ToBeDeliveredByID") ? NInt(r, "ToBeDeliveredByID") : null,
                AreaName = Str(r, "AreaName"),
                DeliveryByDisplay = DbMapper.HasColumn(r, "DeliveryByDisplay") ? Str(r, "DeliveryByDisplay") : null
            };
        }

        private static string Str(IDataRecord r, string name) =>
            r[name] == DBNull.Value ? null : Convert.ToString(r[name]);

        private static int? NInt(IDataRecord r, string name) =>
            r[name] == DBNull.Value ? (int?)null : Convert.ToInt32(r[name]);

        private static decimal? NDec(IDataRecord r, string name) =>
            r[name] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r[name]);

        private static List<DBParameter> Params(ContactDeliverToAddress row, bool includeId)
        {
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = row.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Label", DataValue = (object)row.Label ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@DeliverToPerson", DataValue = (object)row.DeliverToPerson ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Address1", DataValue = (object)row.Address1 ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Address2", DataValue = (object)row.Address2 ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Address3", DataValue = (object)row.Address3 ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AreaID", DataValue = row.AreaID.HasValue && row.AreaID.Value > 0 ? (object)row.AreaID.Value : DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@StateOrProvince", DataValue = (object)row.StateOrProvince ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PostalCode", DataValue = (object)row.PostalCode ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Latitude", DataValue = row.Latitude.HasValue ? (object)row.Latitude.Value : DBNull.Value, DataDbType = DbType.Decimal },
                new DBParameter { ParamName = "@Longitude", DataValue = row.Longitude.HasValue ? (object)row.Longitude.Value : DBNull.Value, DataDbType = DbType.Decimal },
                new DBParameter { ParamName = "@ToBeDeliveredByID", DataValue = row.ToBeDeliveredByID.HasValue && row.ToBeDeliveredByID.Value > 0 ? (object)row.ToBeDeliveredByID.Value : DBNull.Value, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SortOrder", DataValue = row.SortOrder, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Enabled", DataValue = row.Enabled, DataDbType = DbType.Boolean }
            };
            if (includeId)
                p.Add(new DBParameter { ParamName = "@DeliverToAddressID", DataValue = row.DeliverToAddressID, DataDbType = DbType.Int32 });
            return p;
        }

        private static int ExecNonQuery(string sql, List<DBParameter> parameters)
        {
            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(sql, parameters);
        }
    }
}
