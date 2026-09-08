using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ContactsAccInfoRepository : RepositoryBase<ContactsAccInfo>
    {
        protected override string TableName => "ContactsAccInfoTbl";
        protected override string KeyColumn => "ContactsAccInfoID";

        protected override string CoreColumns =>
            "ContactsAccInfoID, ContactID, RequiresPurchOrder, ContactVATNo, BillAddr1, BillAddr2, BillAddr3, BillAddr4, BillAddr5, " +
            "ShipAddr1, ShipAddr2, ShipAddr3, ShipAddr4, ShipAddr5, AccEmail, AltAccEmail, PaymentTermID, Limit, FullCoName, " +
            "AccFirstName, AccLastName, AltAccFirstName, AltAccLastName, PriceLevelID, InvoiceTypeID, RegNo, BankAccNo, BankBranch, Enabled, Notes";

        public ContactsAccInfo GetByContactId(int contactId)
        {
            string sql = "SELECT " + CoreColumns + @"
                FROM ContactsAccInfoTbl
                WHERE ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            using (var rdr = ExecReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    return DbMapper.Map<ContactsAccInfo>(rdr);
                }
            }

            return null;
        }

        public int? GetPaymentTermIdByContactId(int contactId)
        {
            return ExecuteScalar<int?>(
                "SELECT PaymentTermID FROM ContactsAccInfoTbl WHERE ContactID = @ContactID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
                });
        }

        public int? GetInvoiceTypeIdByContactId(int contactId)
        {
            return ExecuteScalar<int?>(
                "SELECT InvoiceTypeID FROM ContactsAccInfoTbl WHERE ContactID = @ContactID",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
                });
        }

        /// <summary>Prefetch invoice types for many contacts (Delivery Sheet) — one query instead of N.</summary>
        public Dictionary<int, int> GetInvoiceTypeIdsByContactIds(IEnumerable<int> contactIds)
        {
            var map = new Dictionary<int, int>();
            if (contactIds == null)
                return map;

            var ids = new List<int>();
            var seen = new HashSet<int>();
            foreach (int id in contactIds)
            {
                if (id <= 0 || !seen.Add(id))
                    continue;
                ids.Add(id);
            }
            if (ids.Count == 0)
                return map;

            // Batched IN clauses (SQL Server parameter limit safety).
            const int batchSize = 200;
            for (int offset = 0; offset < ids.Count; offset += batchSize)
            {
                int count = Math.Min(batchSize, ids.Count - offset);
                var parameters = new List<DBParameter>();
                var names = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    string name = "@C" + i;
                    names.Add(name);
                    parameters.Add(new DBParameter
                    {
                        ParamName = name,
                        DataValue = ids[offset + i],
                        DataDbType = DbType.Int32
                    });
                }

                string sql = "SELECT ContactID, InvoiceTypeID FROM ContactsAccInfoTbl WHERE ContactID IN ("
                    + string.Join(",", names) + ")";
                using (var rdr = ExecReader(sql, parameters))
                {
                    while (rdr != null && rdr.Read())
                    {
                        int contactId = Convert.ToInt32(rdr["ContactID"]);
                        int typeId = rdr["InvoiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["InvoiceTypeID"]);
                        map[contactId] = typeId;
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// Sets only the InvoiceTypeID for a contact. Creates a minimal acc-info row when the
        /// contact does not have one yet, so the change is never silently lost.
        /// When the type actually changes, a dated note is prepended to ContactsTbl.Notes.
        /// </summary>
        /// <param name="reason">Optional context for the note, e.g. "recurring order added".</param>
        public bool SetInvoiceTypeByContactId(int contactId, int invoiceTypeId, string reason = null)
        {
            if (contactId <= 0 || invoiceTypeId <= 0)
                return false;

            int? previousTypeId = GetInvoiceTypeIdByContactId(contactId);

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@InvoiceTypeID", DataValue = invoiceTypeId, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@ContactID", DataValue = contactId, DataDbType = DbType.Int32 }
            };

            int rows = ExecNonQuery(
                "UPDATE ContactsAccInfoTbl SET InvoiceTypeID = @InvoiceTypeID WHERE ContactID = @ContactID",
                parameters);

            bool ok = rows > 0;
            if (!ok)
            {
                ok = Insert(new ContactsAccInfo
                {
                    ContactID = contactId,
                    InvoiceTypeID = invoiceTypeId,
                    Enabled = true
                }) > 0;
            }

            if (ok && (!previousTypeId.HasValue || previousTypeId.Value != invoiceTypeId))
            {
                string typeDesc = ResolveInvoiceTypeDesc(invoiceTypeId);
                string note = string.IsNullOrWhiteSpace(reason)
                    ? "Account type set to " + typeDesc
                    : "Account type set to " + typeDesc + " — " + reason.Trim();
                new ContactsRepository().AppendSystemNote(contactId, note);
            }

            return ok;
        }

        private static string ResolveInvoiceTypeDesc(int invoiceTypeId)
        {
            try
            {
                var invoiceType = new InvoiceTypesRepository().GetById(invoiceTypeId);
                if (!string.IsNullOrWhiteSpace(invoiceType?.InvoiceTypeDesc))
                    return invoiceType.InvoiceTypeDesc;
            }
            catch
            {
                // Fall through to id-based label
            }

            return "type " + invoiceTypeId;
        }

        public override int Insert(ContactsAccInfo entity)
        {
            const string sql = @"
                INSERT INTO ContactsAccInfoTbl
                (ContactID, RequiresPurchOrder, ContactVATNo, BillAddr1, BillAddr2, BillAddr3, BillAddr4, BillAddr5,
                 ShipAddr1, ShipAddr2, ShipAddr3, ShipAddr4, ShipAddr5, AccEmail, AltAccEmail, PaymentTermID, Limit,
                 FullCoName, AccFirstName, AccLastName, AltAccFirstName, AltAccLastName, PriceLevelID, InvoiceTypeID,
                 RegNo, BankAccNo, BankBranch, Enabled, Notes)
                VALUES
                (@ContactID, @RequiresPurchOrder, @ContactVATNo, @BillAddr1, @BillAddr2, @BillAddr3, @BillAddr4, @BillAddr5,
                 @ShipAddr1, @ShipAddr2, @ShipAddr3, @ShipAddr4, @ShipAddr5, @AccEmail, @AltAccEmail, @PaymentTermID, @Limit,
                 @FullCoName, @AccFirstName, @AccLastName, @AltAccFirstName, @AltAccLastName, @PriceLevelID, @InvoiceTypeID,
                 @RegNo, @BankAccNo, @BankBranch, @Enabled, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return ExecuteScalar<int>(sql, BuildParameters(entity, includeId: false));
        }

        public override int Update(ContactsAccInfo entity)
        {
            const string sql = @"
                UPDATE ContactsAccInfoTbl SET
                    ContactID = @ContactID, RequiresPurchOrder = @RequiresPurchOrder, ContactVATNo = @ContactVATNo,
                    BillAddr1 = @BillAddr1, BillAddr2 = @BillAddr2, BillAddr3 = @BillAddr3, BillAddr4 = @BillAddr4, BillAddr5 = @BillAddr5,
                    ShipAddr1 = @ShipAddr1, ShipAddr2 = @ShipAddr2, ShipAddr3 = @ShipAddr3, ShipAddr4 = @ShipAddr4, ShipAddr5 = @ShipAddr5,
                    AccEmail = @AccEmail, AltAccEmail = @AltAccEmail, PaymentTermID = @PaymentTermID, Limit = @Limit,
                    FullCoName = @FullCoName, AccFirstName = @AccFirstName, AccLastName = @AccLastName,
                    AltAccFirstName = @AltAccFirstName, AltAccLastName = @AltAccLastName, PriceLevelID = @PriceLevelID,
                    InvoiceTypeID = @InvoiceTypeID, RegNo = @RegNo, BankAccNo = @BankAccNo, BankBranch = @BankBranch,
                    Enabled = @Enabled, Notes = @Notes
                WHERE ContactsAccInfoID = @ContactsAccInfoID";

            return ExecNonQuery(sql, BuildParameters(entity, includeId: true));
        }

        private static List<DBParameter> BuildParameters(ContactsAccInfo entity, bool includeId)
        {
            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = entity.ContactID, DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@RequiresPurchOrder", DataValue = entity.RequiresPurchOrder ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@ContactVATNo", DataValue = entity.ContactVATNo ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BillAddr1", DataValue = entity.BillAddr1 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BillAddr2", DataValue = entity.BillAddr2 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BillAddr3", DataValue = entity.BillAddr3 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BillAddr4", DataValue = entity.BillAddr4 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BillAddr5", DataValue = entity.BillAddr5 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ShipAddr1", DataValue = entity.ShipAddr1 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ShipAddr2", DataValue = entity.ShipAddr2 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ShipAddr3", DataValue = entity.ShipAddr3 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ShipAddr4", DataValue = entity.ShipAddr4 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@ShipAddr5", DataValue = entity.ShipAddr5 ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AccEmail", DataValue = entity.AccEmail ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AltAccEmail", DataValue = entity.AltAccEmail ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PaymentTermID", DataValue = FkOrDbNull(entity.PaymentTermID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@Limit", DataValue = entity.Limit ?? (object)DBNull.Value, DataDbType = DbType.Double },
                new DBParameter { ParamName = "@FullCoName", DataValue = entity.FullCoName ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AccFirstName", DataValue = entity.AccFirstName ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AccLastName", DataValue = entity.AccLastName ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AltAccFirstName", DataValue = entity.AltAccFirstName ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@AltAccLastName", DataValue = entity.AltAccLastName ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@PriceLevelID", DataValue = FkOrDbNull(entity.PriceLevelID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@InvoiceTypeID", DataValue = FkOrDbNull(entity.InvoiceTypeID), DataDbType = DbType.Int32 },
                new DBParameter { ParamName = "@RegNo", DataValue = entity.RegNo ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BankAccNo", DataValue = entity.BankAccNo ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@BankBranch", DataValue = entity.BankBranch ?? (object)DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@Enabled", DataValue = entity.Enabled ?? (object)DBNull.Value, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@Notes", DataValue = entity.Notes ?? (object)DBNull.Value, DataDbType = DbType.String }
            };

            if (includeId)
            {
                parameters.Add(new DBParameter { ParamName = "@ContactsAccInfoID", DataValue = entity.ContactsAccInfoID, DataDbType = DbType.Int32 });
            }

            return parameters;
        }
    }
}
