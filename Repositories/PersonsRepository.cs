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

        protected override string CoreColumns =>
            "PersonID, PersonName, Abbreviation, Enabled, NormalDeliveryDoW, SecurityUsername";

        protected override string LookupColumns =>
            "PersonID, Abbreviation";

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
                "SELECT PersonName FROM PeopleTbl WHERE PersonID = @PersonID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@PersonID", DataValue = personId, DataDbType = DbType.Int32 }
                });
        }
    }
}