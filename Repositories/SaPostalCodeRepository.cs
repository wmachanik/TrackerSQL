using System;

using System.Collections.Generic;

using System.Data;

using System.Globalization;

using TrackerSQL.Classes;

using TrackerSQL.Models;



namespace TrackerSQL.Repositories

{

    public class SaPostalCodeRepository

    {

        public int GetRowCount()

        {

            using (var db = new TrackerSQLDb())

                return db.ExecuteScalar<int>("SELECT COUNT(*) FROM SaPostalCodeTbl");

        }



        public bool TableExists()

        {

            try

            {

                using (var db = new TrackerSQLDb())

                {

                    int n = db.ExecuteScalar<int>(

                        "SELECT COUNT(*) FROM sys.tables WHERE name = N'SaPostalCodeTbl'");

                    return n > 0;

                }

            }

            catch

            {

                return false;

            }

        }



        public void TruncateAll()

        {

            using (var db = new TrackerSQLDb())

                db.ExecuteNonQuery("TRUNCATE TABLE SaPostalCodeTbl");

        }



        public int BulkInsert(IList<SaPostalCode> rows)

        {

            if (rows == null || rows.Count == 0)

                return 0;



            const string sql = @"

INSERT INTO SaPostalCodeTbl (PostalCode, PlaceName, Province, Municipality, Latitude, Longitude)

VALUES (@PostalCode, @PlaceName, @Province, @Municipality, @Latitude, @Longitude)";



            int n = 0;

            using (var db = new TrackerSQLDb())

            {

                foreach (var row in rows)

                {

                    if (row == null || row.PostalCode <= 0 || string.IsNullOrWhiteSpace(row.PlaceName))

                        continue;

                    var p = new List<DBParameter>

                    {

                        new DBParameter { ParamName = "@PostalCode", DataValue = row.PostalCode, DataDbType = DbType.Int32 },

                        new DBParameter { ParamName = "@PlaceName", DataValue = row.PlaceName.Trim(), DataDbType = DbType.String },

                        new DBParameter { ParamName = "@Province", DataValue = (object)NullIfBlank(row.Province) ?? DBNull.Value, DataDbType = DbType.String },

                        new DBParameter { ParamName = "@Municipality", DataValue = (object)NullIfBlank(row.Municipality) ?? DBNull.Value, DataDbType = DbType.String },

                        new DBParameter { ParamName = "@Latitude", DataValue = (object)row.Latitude ?? DBNull.Value, DataDbType = DbType.Decimal },

                        new DBParameter { ParamName = "@Longitude", DataValue = (object)row.Longitude ?? DBNull.Value, DataDbType = DbType.Decimal }

                    };

                    db.ExecuteNonQuery(sql, p);

                    n++;

                }

            }

            return n;

        }



        public List<SaPostalCode> Search(string query, int maxRows = 200)

        {

            var list = new List<SaPostalCode>();

            string q = (query ?? string.Empty).Trim();

            if (q.Length == 0)

                return list;



            string sql;

            var p = new List<DBParameter>();

            if (int.TryParse(q, NumberStyles.Integer, CultureInfo.InvariantCulture, out int code) && q.Length <= 4)

            {

                sql = @"

SELECT TOP (@Max) PostalCodeID, PostalCode, PlaceName, Province, Municipality, Latitude, Longitude

FROM SaPostalCodeTbl

WHERE PostalCode = @Code OR CAST(PostalCode AS NVARCHAR(4)) LIKE @Like

ORDER BY PostalCode, PlaceName";

                p.Add(new DBParameter { ParamName = "@Code", DataValue = code, DataDbType = DbType.Int32 });

                p.Add(new DBParameter { ParamName = "@Like", DataValue = q + "%", DataDbType = DbType.String });

            }

            else

            {

                sql = @"

SELECT TOP (@Max) PostalCodeID, PostalCode, PlaceName, Province, Municipality, Latitude, Longitude

FROM SaPostalCodeTbl

WHERE PlaceName LIKE @Place OR Province LIKE @Place OR Municipality LIKE @Place

ORDER BY PlaceName, PostalCode";

                p.Add(new DBParameter { ParamName = "@Place", DataValue = "%" + q + "%", DataDbType = DbType.String });

            }

            p.Add(new DBParameter { ParamName = "@Max", DataValue = maxRows, DataDbType = DbType.Int32 });



            using (var db = new TrackerSQLDb())

            using (var rdr = db.ExecuteReader(sql, p))

            {

                while (rdr != null && rdr.Read())

                    list.Add(MapRow(rdr));

            }

            return list;

        }



        public List<int> GetDistinctCodesForPlaceMatch(string placeMatch)

        {

            var codes = new List<int>();

            if (string.IsNullOrWhiteSpace(placeMatch))

                return codes;



            const string sql = @"

SELECT DISTINCT PostalCode

FROM SaPostalCodeTbl

WHERE PlaceName LIKE @Place

ORDER BY PostalCode";

            var p = new List<DBParameter>

            {

                new DBParameter { ParamName = "@Place", DataValue = "%" + placeMatch.Trim() + "%", DataDbType = DbType.String }

            };

            using (var db = new TrackerSQLDb())

            using (var rdr = db.ExecuteReader(sql, p))

            {

                while (rdr != null && rdr.Read())

                    codes.Add(Convert.ToInt32(rdr["PostalCode"], CultureInfo.InvariantCulture));

            }

            return codes;

        }



        public List<int> GetAllDistinctCodes()
        {
            var codes = new List<int>();
            if (!TableExists())
                return codes;
            const string sql = "SELECT DISTINCT PostalCode FROM SaPostalCodeTbl ORDER BY PostalCode";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    codes.Add(Convert.ToInt32(rdr["PostalCode"], CultureInfo.InvariantCulture));
            }
            return codes;
        }

        public List<SaPostalCode> GetAllCodesWithPrimaryPlace()
        {
            var list = new List<SaPostalCode>();
            if (!TableExists())
                return list;
            const string sql = @"
SELECT PostalCode, MIN(PlaceName) AS PlaceName
FROM SaPostalCodeTbl
GROUP BY PostalCode
ORDER BY PostalCode";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new SaPostalCode
                    {
                        PostalCode = Convert.ToInt32(rdr["PostalCode"], CultureInfo.InvariantCulture),
                        PlaceName = rdr["PlaceName"] == DBNull.Value ? string.Empty : rdr["PlaceName"].ToString()
                    });
                }
            }
            return list;
        }

        /// <summary>All place/code rows for address matching (may include multiple places per code).</summary>
        public List<SaPostalCode> GetAllPlaceCodePairs()
        {
            var list = new List<SaPostalCode>();
            if (!TableExists())
                return list;
            const string sql = @"
SELECT PostalCode, PlaceName
FROM SaPostalCodeTbl
WHERE PlaceName IS NOT NULL AND LTRIM(RTRIM(PlaceName)) <> N''
ORDER BY LEN(PlaceName) DESC, PlaceName";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(new SaPostalCode
                    {
                        PostalCode = Convert.ToInt32(rdr["PostalCode"], CultureInfo.InvariantCulture),
                        PlaceName = rdr["PlaceName"] == DBNull.Value ? string.Empty : rdr["PlaceName"].ToString()
                    });
                }
            }
            return list;
        }

        public List<SaPostalCode> GetPlacesForPostalCode(int postalCode, int maxRows = 100)
        {
            var list = new List<SaPostalCode>();
            if (postalCode <= 0)
                return list;

            const string sql = @"
SELECT TOP (@Max) PostalCodeID, PostalCode, PlaceName, Province, Municipality, Latitude, Longitude
FROM SaPostalCodeTbl
WHERE PostalCode = @Code
ORDER BY PlaceName";

            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Code", DataValue = postalCode, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Max", DataValue = maxRows, DataDbType = DbType.Int32 }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                while (rdr != null && rdr.Read())
                    list.Add(MapRow(rdr));
            }
            return list;
        }

        public List<int> GetDistinctCodesForPlaceNames(IEnumerable<string> placeNames)

        {

            var set = new HashSet<int>();

            if (placeNames == null)

                return new List<int>();

            foreach (string name in placeNames)

            {

                foreach (int code in GetDistinctCodesForPlaceMatch(name))

                    set.Add(code);

            }

            var list = new List<int>(set);

            list.Sort();

            return list;

        }



        private static SaPostalCode MapRow(IDataRecord rdr)

        {

            return new SaPostalCode

            {

                PostalCodeID = Convert.ToInt32(rdr["PostalCodeID"], CultureInfo.InvariantCulture),

                PostalCode = Convert.ToInt32(rdr["PostalCode"], CultureInfo.InvariantCulture),

                PlaceName = rdr["PlaceName"] == DBNull.Value ? string.Empty : rdr["PlaceName"].ToString(),

                Province = rdr["Province"] == DBNull.Value ? null : rdr["Province"].ToString(),

                Municipality = rdr["Municipality"] == DBNull.Value ? null : rdr["Municipality"].ToString(),

                Latitude = rdr["Latitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["Latitude"], CultureInfo.InvariantCulture),

                Longitude = rdr["Longitude"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["Longitude"], CultureInfo.InvariantCulture)

            };

        }



        private static string NullIfBlank(string s)

        {

            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        }

    }

}

