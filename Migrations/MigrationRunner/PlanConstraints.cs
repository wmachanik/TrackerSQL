using System.Collections.Generic;

namespace MigrationRunner
{
    public sealed class ConstraintsIndex
    {
        public List<ConstraintTable> Tables { get; set; } = new List<ConstraintTable>();
    }

    public sealed class ConstraintTable
    {
        public string Table { get; set; } = "";
        public List<string> PrimaryKey { get; set; } = new List<string>();
        public List<string> IdentityColumns { get; set; } = new List<string>();
        public List<ForeignKeyDef> ForeignKeys { get; set; } = new List<ForeignKeyDef>();
        // explicit NOT NULL overrides from CSV
        public List<string> NotNullColumns { get; set; } = new List<string>();
        // NEW: explicit type overrides from CSV (After Type)
        public Dictionary<string, string> ColumnTypes { get; set; } = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
    }

    public sealed class ForeignKeyDef
    {
        public string Column { get; set; } = "";
        public string RefTable { get; set; } = "";
        public string RefColumn { get; set; } = "";
    }
}