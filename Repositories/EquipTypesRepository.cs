using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class EquipTypesRepository : RepositoryBase<EquipType>
    {
        protected override string TableName => "EquipTypesTbl";
        protected override string KeyColumn => "EquipTypeID";

        protected override string CoreColumns =>
            "EquipTypeID, EquipTypeName, EquipTypeDesc AS EquipTypeDescription";

        protected override string LookupColumns =>
            "EquipTypeID, EquipTypeName, EquipTypeDesc AS EquipTypeDescription";

        public override int Insert(EquipType entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO EquipTypesTbl
                (
                    EquipTypeName,
                    EquipTypeDesc
                )
                VALUES
                (
                    @EquipTypeName,
                    @EquipTypeDesc
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@EquipTypeName",
                    DataValue = entity.EquipTypeName,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@EquipTypeDesc",
                    DataValue = entity.EquipTypeDescription ?? string.Empty,
                    DataDbType = DbType.String
                }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public override int Update(EquipType entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                UPDATE EquipTypesTbl
                SET
                    EquipTypeName = @EquipTypeName,
                    EquipTypeDesc = @EquipTypeDesc
                WHERE EquipTypeID = @EquipTypeID";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@EquipTypeName",
                    DataValue = entity.EquipTypeName,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@EquipTypeDesc",
                    DataValue = entity.EquipTypeDescription ?? string.Empty,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@EquipTypeID",
                    DataValue = entity.EquipTypeID,
                    DataDbType = DbType.Int32
                }
            };

            return ExecNonQuery(sql, parameters);
        }

        public string GetEquipTypeName(int equipTypeId)
        {
            var equip = GetKeyColsById(equipTypeId);
            return equip?.EquipTypeName ?? string.Empty;
        }
    }
}