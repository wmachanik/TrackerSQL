using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ItemPackagingsRepository : RepositoryBase<ItemPackaging>
    {
        protected override string TableName => "ItemPackagingsTbl";
        protected override string KeyColumn => "ItemPackagingID";

        protected override string CoreColumns =>
            "ItemPackagingID, ItemPrepDescription AS ItemPackagingDesc, AdditionalNotes, Symbol, Colour, BGColour";

        protected override string LookupColumns =>
            "ItemPackagingID, ItemPrepDescription AS ItemPackagingDesc";

        public override List<ItemPackaging> GetAll(string sortBy)
        {
            string sql = $@"
                SELECT {CoreColumns}
                FROM {TableName}
                ORDER BY {MapSortColumnOrDefault(sortBy)}";

            var list = new List<ItemPackaging>();

            using (var rdr = ExecReader(sql))
            {
                while (rdr != null && rdr.Read())
                {
                    list.Add(DbMapper.Map<ItemPackaging>(rdr));
                }
            }

            return list;
        }

        public override int Insert(ItemPackaging entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                INSERT INTO ItemPackagingsTbl
                (
                    ItemPrepDescription,
                    AdditionalNotes,
                    Symbol,
                    Colour,
                    BGColour
                )
                VALUES
                (
                    @ItemPackagingDesc,
                    @AdditionalNotes,
                    @Symbol,
                    @Colour,
                    @BGColour
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@ItemPackagingDesc",
                    DataValue = entity.ItemPackagingDesc,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@AdditionalNotes",
                    DataValue = entity.AdditionalNotes ?? string.Empty,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@Symbol",
                    DataValue = entity.Symbol ?? string.Empty,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@Colour",
                    DataValue = entity.Colour ?? (object)DBNull.Value,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@BGColour",
                    DataValue = entity.BGColour ?? string.Empty,
                    DataDbType = DbType.String
                }
            };

            return ExecuteScalar<int>(sql, parameters);
        }

        public override int Update(ItemPackaging entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            const string sql = @"
                UPDATE ItemPackagingsTbl
                SET
                    ItemPrepDescription = @ItemPackagingDesc,
                    AdditionalNotes = @AdditionalNotes,
                    Symbol = @Symbol,
                    Colour = @Colour,
                    BGColour = @BGColour
                WHERE ItemPackagingID = @ItemPackagingID";

            var parameters = new List<DBParameter>
            {
                new DBParameter
                {
                    ParamName = "@ItemPackagingDesc",
                    DataValue = entity.ItemPackagingDesc,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@AdditionalNotes",
                    DataValue = entity.AdditionalNotes ?? string.Empty,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@Symbol",
                    DataValue = entity.Symbol ?? string.Empty,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@Colour",
                    DataValue = entity.Colour ?? (object)DBNull.Value,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@BGColour",
                    DataValue = entity.BGColour ?? string.Empty,
                    DataDbType = DbType.String
                },
                new DBParameter
                {
                    ParamName = "@ItemPackagingID",
                    DataValue = entity.ItemPackagingID,
                    DataDbType = DbType.Int32
                }
            };

            return ExecNonQuery(sql, parameters);
        }

        private static string MapSortColumnOrDefault(string sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return "ItemPrepDescription";
            }

            string trimmed = sortBy.Trim();

            if (trimmed.Equals("ItemPackagingDesc", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("ItemPackagingDesc ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "ItemPrepDescription ASC";
            }

            if (trimmed.Equals("ItemPackagingDesc DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "ItemPrepDescription DESC";
            }

            if (trimmed.Equals("ItemPackagingID", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("ItemPackagingID ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "ItemPackagingID ASC";
            }

            if (trimmed.Equals("ItemPackagingID DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "ItemPackagingID DESC";
            }

            if (trimmed.Equals("Symbol", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("Symbol ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "Symbol ASC";
            }

            if (trimmed.Equals("Symbol DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "Symbol DESC";
            }

            if (trimmed.Equals("BGColour", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("BGColour ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "BGColour ASC";
            }

            if (trimmed.Equals("BGColour DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "BGColour DESC";
            }

            if (trimmed.Equals("Colour", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("Colour ASC", StringComparison.OrdinalIgnoreCase))
            {
                return "Colour ASC";
            }

            if (trimmed.Equals("Colour DESC", StringComparison.OrdinalIgnoreCase))
            {
                return "Colour DESC";
            }

            return "ItemPrepDescription";
        }

        public override ItemPackaging GetById(int id)
        {
            string sql = $@"
                SELECT {CoreColumns}
                FROM {TableName}
                WHERE {KeyColumn} = @Id";

            var parameters = new List<DBParameter>
            {
                new DBParameter { DataValue = id, DataDbType = DbType.Int32, ParamName = "@Id" }
            };

            return ExecuteQuerySingle<ItemPackaging>(sql, parameters);
        }

        public string GetPackagingDescById(int packagingId)
        {
            if (packagingId <= 0)
            {
                return string.Empty;
            }

            var packaging = GetById(packagingId);
            return packaging?.ItemPackagingDesc ?? string.Empty;
        }
    }
}