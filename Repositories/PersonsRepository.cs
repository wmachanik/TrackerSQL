using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class PersonsRepository : RepositoryBase<Person>
    {
        protected override string TableName => "PeopleTbl";
        protected override string KeyColumn => "PersonID";

        // Live column is Person; app model uses PersonName
        private const string SelectColumnsBase =
            "PersonID, Person AS PersonName, Abbreviation, Enabled, NormalDeliveryDoW, SecurityUsername";

        private static bool? _hasIsDispatched;

        private static bool HasIsDispatchedColumn()
        {
            if (_hasIsDispatched.HasValue)
                return _hasIsDispatched.Value;
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    _hasIsDispatched = db.ExecuteScalar<int>(@"
SELECT COUNT(*) FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.PeopleTbl') AND name = N'IsDispatched'") > 0;
                }
            }
            catch
            {
                _hasIsDispatched = false;
            }
            return _hasIsDispatched.Value;
        }

        private static void InvalidateColumnCache()
        {
            _hasIsDispatched = null;
        }

        private string SelectColumns =>
            HasIsDispatchedColumn()
                ? SelectColumnsBase + ", IsDispatched"
                : SelectColumnsBase;

        protected override string CoreColumns => SelectColumns;

        protected override string LookupColumns =>
            HasIsDispatchedColumn()
                ? "PersonID, Person AS PersonName, Abbreviation, Enabled, IsDispatched"
                : "PersonID, Person AS PersonName, Abbreviation, Enabled";

        /// <summary>
        /// Ensures IsDispatched exists, seeds Prgo/Cour, and one-shot migrates Woo CSV IDs.
        /// </summary>
        public void EnsureIsDispatchedColumn()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    db.ExecuteNonQuery(@"
IF OBJECT_ID(N'dbo.PeopleTbl', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.PeopleTbl', N'IsDispatched') IS NULL
BEGIN
    ALTER TABLE dbo.PeopleTbl ADD IsDispatched BIT NOT NULL
        CONSTRAINT DF_People_IsDispatched DEFAULT (0);
END");
                    InvalidateColumnCache();

                    if (!HasIsDispatchedColumn())
                        return;

                    db.ExecuteNonQuery(@"
UPDATE dbo.PeopleTbl
SET IsDispatched = 1
WHERE IsDispatched = 0
  AND (
        UPPER(LTRIM(RTRIM(Abbreviation))) IN (N'PRGO', N'COUR')
     OR PersonID IN (5, 7)
  )");

                    // One-shot migrate from Woo settings CSV (if present).
                    if (db.ExecuteScalar<int>(@"
SELECT COUNT(*) FROM sys.tables WHERE name = N'WooCommerceSettingsTbl'") > 0
                        && db.ExecuteScalar<int>(@"
SELECT COUNT(*) FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.WooCommerceSettingsTbl')
  AND name = N'DispatchDeliveryPersonIds'") > 0)
                    {
                        string csv = db.ExecuteScalar<string>(@"
SELECT TOP 1 DispatchDeliveryPersonIds FROM dbo.WooCommerceSettingsTbl ORDER BY SettingsID");
                        ApplyDispatchIdCsv(db, csv);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "PersonsRepository.EnsureIsDispatchedColumn: " + ex.Message);
            }
        }

        private static void ApplyDispatchIdCsv(TrackerSQLDb db, string csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
                return;

            foreach (string part in csv.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(part.Trim(), out int id) || id <= 0)
                    continue;
                db.ExecuteNonQuery(@"
UPDATE dbo.PeopleTbl SET IsDispatched = 1
WHERE PersonID = @PersonID AND IsDispatched = 0",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@PersonID", DataValue = id, DataDbType = DbType.Int32 }
                    });
            }
        }

        public override List<Person> GetAll(string SortBy)
        {
            EnsureIsDispatchedColumn();
            var list = new List<Person>();
            string sql = "SELECT " + SelectColumns + " FROM PeopleTbl";
            if (!string.IsNullOrWhiteSpace(SortBy))
            {
                string orderBy = SortBy.Equals("PersonName", StringComparison.OrdinalIgnoreCase)
                    ? "Person"
                    : SortBy;
                sql += " ORDER BY " + orderBy;
            }
            else
            {
                sql += " ORDER BY Abbreviation";
            }

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr.Read())
                    list.Add(MapPerson(rdr));
            }
            return list;
        }

        public override Person GetById(int id)
        {
            EnsureIsDispatchedColumn();
            string sql = "SELECT " + SelectColumns + " FROM PeopleTbl WHERE PersonID = @Id";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Id", DataValue = id, DataDbType = DbType.Int32 }
            };
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, p))
            {
                if (rdr != null && rdr.Read())
                    return MapPerson(rdr);
            }
            return null;
        }

        public bool IsDispatchPerson(int personId)
        {
            if (personId <= 0)
                return false;

            EnsureIsDispatchedColumn();
            if (HasIsDispatchedColumn())
            {
                bool? flag = ExecuteScalar<bool?>(
                    "SELECT IsDispatched FROM PeopleTbl WHERE PersonID = @PersonID",
                    new List<DBParameter>
                    {
                        new DBParameter { ParamName = "@PersonID", DataValue = personId, DataDbType = DbType.Int32 }
                    });
                if (flag.HasValue)
                    return flag.Value;
            }

            // Fallback if column missing / person missing
            return personId == SystemConstants.DeliveryConstants.CourierDeliveryID
                || personId == SystemConstants.DeliveryConstants.ParcelDispatchID;
        }

        public List<Person> GetDispatchPeople()
        {
            EnsureIsDispatchedColumn();
            var list = new List<Person>();
            if (!HasIsDispatchedColumn())
            {
                foreach (var p in GetAll("Abbreviation") ?? new List<Person>())
                {
                    if (p != null && IsDispatchPerson(p.PersonID))
                        list.Add(p);
                }
                return list;
            }

            string sql = "SELECT " + SelectColumns + @"
FROM PeopleTbl
WHERE IsDispatched = 1
ORDER BY Abbreviation";
            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                while (rdr != null && rdr.Read())
                    list.Add(MapPerson(rdr));
            }
            return list;
        }

        public override int Insert(Person entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            EnsureIsDispatchedColumn();

            string sql;
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@PersonName", DataValue = entity.PersonName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Abbreviation", DataValue = entity.Abbreviation ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Enabled", DataValue = entity.Enabled ?? true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@NormalDeliveryDoW", DataValue = entity.NormalDeliveryDoW ?? 0, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SecurityUsername", DataValue = (object)entity.SecurityUsername ?? DBNull.Value, DataDbType = DbType.String }
            };

            if (HasIsDispatchedColumn())
            {
                sql = @"
INSERT INTO PeopleTbl
(Person, Abbreviation, Enabled, NormalDeliveryDoW, SecurityUsername, IsDispatched)
VALUES
(@PersonName, @Abbreviation, @Enabled, @NormalDeliveryDoW, @SecurityUsername, @IsDispatched);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
                parameters.Add(new DBParameter { ParamName = "@IsDispatched", DataValue = entity.IsDispatched, DataDbType = DbType.Boolean });
            }
            else
            {
                sql = @"
INSERT INTO PeopleTbl
(Person, Abbreviation, Enabled, NormalDeliveryDoW, SecurityUsername)
VALUES
(@PersonName, @Abbreviation, @Enabled, @NormalDeliveryDoW, @SecurityUsername);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            }

            return ExecuteScalar<int>(sql, parameters);
        }

        public override int Update(Person entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            EnsureIsDispatchedColumn();

            string sql;
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@PersonName", DataValue = entity.PersonName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Abbreviation", DataValue = entity.Abbreviation ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Enabled", DataValue = entity.Enabled ?? true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@NormalDeliveryDoW", DataValue = entity.NormalDeliveryDoW ?? 0, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SecurityUsername", DataValue = (object)entity.SecurityUsername ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PersonID", DataValue = entity.PersonID, DataDbType = DbType.Int32 }
            };

            if (HasIsDispatchedColumn())
            {
                sql = @"
UPDATE PeopleTbl
SET Person = @PersonName,
    Abbreviation = @Abbreviation,
    Enabled = @Enabled,
    NormalDeliveryDoW = @NormalDeliveryDoW,
    SecurityUsername = @SecurityUsername,
    IsDispatched = @IsDispatched
WHERE PersonID = @PersonID";
                parameters.Add(new DBParameter { ParamName = "@IsDispatched", DataValue = entity.IsDispatched, DataDbType = DbType.Boolean });
            }
            else
            {
                sql = @"
UPDATE PeopleTbl
SET Person = @PersonName,
    Abbreviation = @Abbreviation,
    Enabled = @Enabled,
    NormalDeliveryDoW = @NormalDeliveryDoW,
    SecurityUsername = @SecurityUsername
WHERE PersonID = @PersonID";
            }

            return ExecNonQuery(sql, parameters);
        }

        private static Person MapPerson(IDataRecord rdr)
        {
            var p = DbMapper.Map<Person>(rdr);
            if (p != null && DbMapper.HasColumn(rdr, "IsDispatched") && rdr["IsDispatched"] != DBNull.Value)
                p.IsDispatched = Convert.ToBoolean(rdr["IsDispatched"]);
            return p;
        }

        public int? GetNormalDeliveryDoW(int personId)
        {
            return ExecuteScalar<int?>(
                "SELECT NormalDeliveryDoW FROM PeopleTbl WHERE PersonID = @PersonID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@PersonID", DataValue = personId, DataDbType = DbType.Int32 }
                });
        }

        public int? GetPersonIdByAbbreviation(string abbreviation)
        {
            if (string.IsNullOrWhiteSpace(abbreviation))
                return null;

            return ExecuteScalar<int?>(
                "SELECT PersonID FROM PeopleTbl WHERE UPPER(LTRIM(RTRIM(Abbreviation))) = UPPER(LTRIM(RTRIM(@Abbreviation)))",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Abbreviation", DataValue = abbreviation.Trim(), DataDbType = DbType.String }
                });
        }

        /// <summary>Default sales agent (abbreviation Q). Creates the PeopleTbl row if missing.</summary>
        public int GetOrEnsureDefaultSalesAgentId()
        {
            string abbr = SystemConstants.PersonConstants.DefaultSalesAgentAbbr;
            int? existing = GetPersonIdByAbbreviation(abbr);
            if (existing.HasValue && existing.Value > 0)
                return existing.Value;

            var person = new Person
            {
                PersonName = SystemConstants.PersonConstants.DefaultSalesAgentName,
                Abbreviation = abbr,
                Enabled = true,
                NormalDeliveryDoW = 0,
                IsDispatched = false
            };

            int newId = Insert(person);
            if (newId > 0)
            {
                AppLogger.WriteLog("system",
                    "Created default sales agent '" + abbr + "' (PersonID=" + newId + ").");
            }

            return newId;
        }

        public int? GetPersonIdBySecurityUsername(string securityUsername)
        {
            return ExecuteScalar<int?>(
                "SELECT PersonID FROM PeopleTbl WHERE SecurityUsername = @SecurityUsername",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@SecurityUsername", DataValue = securityUsername, DataDbType = DbType.String }
                });
        }

        public string GetPersonNameById(int personId)
        {
            return ExecuteScalar<string>(
                "SELECT Person FROM PeopleTbl WHERE PersonID = @PersonID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@PersonID", DataValue = personId, DataDbType = DbType.Int32 }
                });
        }
    }
}
