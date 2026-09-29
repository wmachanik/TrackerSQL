using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class InvoiceTypesRepository : RepositoryBase<InvoiceType>
    {
        protected override string TableName => "InvoiceTypesTbl";
        protected override string KeyColumn => "InvoiceTypeID";

        // Minimal/core columns for targeted fetches
        protected override string CoreColumns => "InvoiceTypeID, InvoiceTypeDesc, Enabled, Notes, PillLabel, PillColour";
        // Lookup (id + display text)
        protected override string LookupColumns => "InvoiceTypeID, InvoiceTypeDesc";

        public override int Insert(InvoiceType invoiceType)
        {
            const string sql = "INSERT INTO InvoiceTypesTbl (InvoiceTypeDesc, Enabled, Notes, PillLabel, PillColour) VALUES (@InvoiceTypeDesc, @Enabled, @Notes, @PillLabel, @PillColour)";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@InvoiceTypeDesc", DataValue = invoiceType.InvoiceTypeDesc, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Enabled", DataValue = invoiceType.Enabled ?? false, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = invoiceType.Notes ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PillLabel", DataValue = TextOrDbNull(invoiceType.PillLabel), DataDbType = DbType.String },
                new DBParameter { ParamName = "@PillColour", DataValue = TextOrDbNull(invoiceType.PillColour), DataDbType = DbType.String }
            };

            return ExecNonQuery(sql, parameters);
        }

        public override int Update(InvoiceType invoiceType)
        {
            const string sql = "UPDATE InvoiceTypesTbl SET InvoiceTypeDesc = @InvoiceTypeDesc, Enabled = @Enabled, Notes = @Notes, PillLabel = @PillLabel, PillColour = @PillColour WHERE InvoiceTypeID = @InvoiceTypeID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@InvoiceTypeDesc", DataValue = invoiceType.InvoiceTypeDesc, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Enabled", DataValue = invoiceType.Enabled ?? false, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = invoiceType.Notes ?? string.Empty, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PillLabel", DataValue = TextOrDbNull(invoiceType.PillLabel), DataDbType = DbType.String },
                new DBParameter { ParamName = "@PillColour", DataValue = TextOrDbNull(invoiceType.PillColour), DataDbType = DbType.String },
                new DBParameter { ParamName = "@InvoiceTypeID", DataValue = invoiceType.InvoiceTypeID, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters);
        }

        public override bool Delete(int invoiceTypeID)
        {
            const string sql = "DELETE FROM InvoiceTypesTbl WHERE InvoiceTypeID = @InvoiceTypeID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@InvoiceTypeID", DataValue = invoiceTypeID, DataDbType = DbType.Int32 }
            };

            return ExecNonQuery(sql, parameters) > 0;
        }

        /// <summary>All invoice types by id — used to render order flag pills.</summary>
        public Dictionary<int, InvoiceType> GetAllById()
        {
            var map = new Dictionary<int, InvoiceType>();
            foreach (var invoiceType in GetAll())
            {
                if (invoiceType != null && invoiceType.InvoiceTypeID > 0)
                    map[invoiceType.InvoiceTypeID] = invoiceType;
            }
            return map;
        }

        private static object TextOrDbNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }
    }
}
