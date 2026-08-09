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
        private const string SelectColumns =
            "PersonID, Person AS PersonName, Abbreviation, Enabled, NormalDeliveryDoW, SecurityUsername";

        protected override string CoreColumns => SelectColumns;

        protected override string LookupColumns =>
            "PersonID, Person AS PersonName, Abbreviation, Enabled";

        public override List<Person> GetAll(string SortBy)
        {
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
                    list.Add(DbMapper.Map<Person>(rdr));
            }
            return list;
        }

        public override int Insert(Person entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO PeopleTbl
                (
                    Person,
                    Abbreviation,
                    Enabled,
                    NormalDeliveryDoW,
                    SecurityUsername
                )
                VALUES
                (
                    @PersonName,
                    @Abbreviation,
                    @Enabled,
                    @NormalDeliveryDoW,
                    @SecurityUsername
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@PersonName", DataValue = entity.PersonName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Abbreviation", DataValue = entity.Abbreviation ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Enabled", DataValue = entity.Enabled ?? true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@NormalDeliveryDoW", DataValue = entity.NormalDeliveryDoW ?? 0, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SecurityUsername", DataValue = (object)entity.SecurityUsername ?? DBNull.Value, DataDbType = DbType.String }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public override int Update(Person entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                UPDATE PeopleTbl
                SET
                    Person = @PersonName,
                    Abbreviation = @Abbreviation,
                    Enabled = @Enabled,
                    NormalDeliveryDoW = @NormalDeliveryDoW,
                    SecurityUsername = @SecurityUsername
                WHERE PersonID = @PersonID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@PersonName", DataValue = entity.PersonName ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Abbreviation", DataValue = entity.Abbreviation ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Enabled", DataValue = entity.Enabled ?? true, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@NormalDeliveryDoW", DataValue = entity.NormalDeliveryDoW ?? 0, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@SecurityUsername", DataValue = (object)entity.SecurityUsername ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PersonID", DataValue = entity.PersonID, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters);
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
            return ExecuteScalar<int?>(
                "SELECT PersonID FROM PeopleTbl WHERE Abbreviation LIKE @Abbreviation",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@Abbreviation", DataValue = abbreviation, DataDbType = DbType.String }
                });
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
