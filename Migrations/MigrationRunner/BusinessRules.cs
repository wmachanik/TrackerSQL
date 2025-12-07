using System.Collections.Generic;

namespace MigrationRunner
{
    // Root of BusinessRules.json
    internal sealed class BusinessRulesIndex
    {
        public Dictionary<string, TableBusinessRules> Tables { get; set; } = new Dictionary<string, TableBusinessRules>(System.StringComparer.OrdinalIgnoreCase);
    }

    internal sealed class TableBusinessRules
    {
        // Optional: Conditional composite key logic (e.g., OrdersTbl)
        public List<CompositeKeyConditionalRule> CompositeKeyRules { get; set; } = new List<CompositeKeyConditionalRule>();

        // Optional: Computed columns to generate during ETL
        public List<ComputedColumnRule> ComputedColumns { get; set; } = new List<ComputedColumnRule>();

        // Optional: Recurring orders grouping instructions
        public RecurringOrdersGroupingRule RecurringGrouping { get; set; }
    }

    internal sealed class CompositeKeyConditionalRule
    {
        // A simple boolean expression (evaluated in ETL), e.g. "CustomerID = 9"
        public string Condition { get; set; } = "";

        // The key columns when condition evaluates true/false
        public List<string> TrueKey { get; set; } = new List<string>();
        public List<string> FalseKey { get; set; } = new List<string>();

        // Where this key applies: "Header" or "Line" (default: Header)
        public string Part { get; set; } = "Header";
    }

    internal sealed class ComputedColumnRule
    {
        // Name of the computed target column
        public string Target { get; set; } = "";

        // "H" (Header), "L" (Line) or "Single" for non-normalize
        public string Part { get; set; } = "Single";

        // Expression (string), e.g. Access/SQL-like: "IIF(CustomerID = 9, CustomerID & '|' & RoastDate & '|' & Notes, CustomerID & '|' & RoastDate)"
        public string Expression { get; set; } = "";
    }

    internal sealed class RecurringOrdersGroupingRule
    {
        // Header grouping key (e.g., CustomerID)
        public List<string> HeaderGroupBy { get; set; } = new List<string>();

        // Lines grouping (e.g., ItemRequiredID, PackagingID, PrepTypeID)
        public List<string> LineGroupBy { get; set; } = new List<string>();

        // Aggregations to apply for lines, e.g.: { "QtyRequired": "SUM" }
        public Dictionary<string, string> LineAggregations { get; set; } = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

        // Optional: filter or extra conditions (string evaluated in ETL)
        public string Filter { get; set; } = "";
    }
}