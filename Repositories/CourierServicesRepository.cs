using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class CourierServicesRepository : RepositoryBase<CourierService>
    {
        private List<CourierService> _allCache;

        protected override string TableName => "CourierServicesTbl";
        protected override string KeyColumn => "CourierServiceID";

        protected override string CoreColumns =>
            "CourierServiceID, ServiceCode, ServiceName, TrackingUrl, IsDefault, IsEnabled, SortOrder";

        protected override string LookupColumns => CoreColumns;

        public override List<CourierService> GetAll(string SortBy)
        {
            EnsureExists();
            var list = new List<CourierService>(LoadAll());
            if (string.Equals(SortBy, "ServiceName", StringComparison.OrdinalIgnoreCase))
                list.Sort((a, b) => string.Compare(a.ServiceName, b.ServiceName, StringComparison.OrdinalIgnoreCase));
            else if (string.Equals(SortBy, "ServiceCode", StringComparison.OrdinalIgnoreCase))
                list.Sort((a, b) => string.Compare(a.ServiceCode, b.ServiceCode, StringComparison.OrdinalIgnoreCase));
            else
                list.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
            return list;
        }

        public override int Insert(CourierService entity)
        {
            EnsureExists();
            if (entity == null)
                return 0;

            const string sql = @"
INSERT INTO CourierServicesTbl
(ServiceCode, ServiceName, TrackingUrl, IsDefault, IsEnabled, SortOrder)
VALUES
(@ServiceCode, @ServiceName, @TrackingUrl, @IsDefault, @IsEnabled, @SortOrder)";
            int n = ExecNonQuery(sql, BuildParams(entity, includeId: false));
            if (n > 0 && entity.IsDefault)
                ClearOtherDefaults(0);
            InvalidateCache();
            return n;
        }

        public override int Update(CourierService entity)
        {
            EnsureExists();
            if (entity == null || entity.CourierServiceID <= 0)
                return 0;

            const string sql = @"
UPDATE CourierServicesTbl SET
    ServiceCode = @ServiceCode,
    ServiceName = @ServiceName,
    TrackingUrl = @TrackingUrl,
    IsDefault = @IsDefault,
    IsEnabled = @IsEnabled,
    SortOrder = @SortOrder
WHERE CourierServiceID = @CourierServiceID";
            int n = ExecNonQuery(sql, BuildParams(entity, includeId: true));
            if (n > 0 && entity.IsDefault)
                ClearOtherDefaults(entity.CourierServiceID);
            InvalidateCache();
            return n;
        }

        public override bool Delete(int id)
        {
            EnsureExists();
            const string sql = "DELETE FROM CourierServicesTbl WHERE CourierServiceID = @Id AND ServiceCode <> N'None'";
            var p = new List<DBParameter>
            {
                new DBParameter { ParamName = "@Id", DataValue = id, DataDbType = DbType.Int32 }
            };
            bool ok = ExecNonQuery(sql, p) > 0;
            InvalidateCache();
            return ok;
        }

        public CourierService GetByIdSafe(int id)
        {
            if (id <= 0)
                return null;
            EnsureExists();
            return LoadAll().FirstOrDefault(r => r.CourierServiceID == id);
        }

        public CourierService GetDefault()
        {
            EnsureExists();
            return LoadAll().FirstOrDefault(r => r.IsDefault && r.IsEnabled)
                ?? LoadAll().FirstOrDefault(r => string.Equals(r.ServiceCode, "Fastway", StringComparison.OrdinalIgnoreCase))
                ?? LoadAll().FirstOrDefault(r => r.IsEnabled && !r.IsNone);
        }

        public CourierService GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            EnsureExists();
            return LoadAll().FirstOrDefault(r =>
                string.Equals(r.ServiceCode, code.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Prefer contact preference (when a real courier), else map Delivered-by Prgo→Pargo,
        /// else system default (Fastway). Contact "None" means no preference → use default path.
        /// </summary>
        public int? ResolvePreferredId(int? contactPreferredId, int? deliveredByPersonId)
        {
            EnsureExists();
            if (contactPreferredId.HasValue && contactPreferredId.Value > 0)
            {
                var pref = GetByIdSafe(contactPreferredId.Value);
                if (pref != null && pref.IsEnabled && !pref.IsNone)
                    return pref.CourierServiceID;
            }

            if (deliveredByPersonId == SystemConstants.DeliveryConstants.ParcelDispatchID)
            {
                var pargo = GetByCode("Pargo");
                if (pargo != null) return pargo.CourierServiceID;
            }

            var def = GetDefault();
            return def?.CourierServiceID;
        }

        public void FillDropDown(ListControl ddl, int? selectedValue, bool includeNone = true)
        {
            if (ddl == null)
                return;

            EnsureExists();
            ddl.Items.Clear();

            IEnumerable<CourierService> rows = LoadAll()
                .Where(r => r.IsEnabled || (selectedValue.HasValue && r.CourierServiceID == selectedValue.Value))
                .OrderBy(r => r.SortOrder);

            if (!includeNone)
                rows = rows.Where(r => !r.IsNone);

            foreach (var row in rows)
            {
                string text = row.ServiceName ?? row.ServiceCode ?? ("#" + row.CourierServiceID);
                if (!row.IsEnabled)
                    text = "_" + text;
                ddl.Items.Add(new ListItem(text, row.CourierServiceID.ToString()));
            }

            if (selectedValue.HasValue && selectedValue.Value > 0)
            {
                string sel = selectedValue.Value.ToString();
                if (ddl.Items.FindByValue(sel) != null)
                    ddl.SelectedValue = sel;
                else if (ddl.Items.Count > 0)
                    ddl.SelectedIndex = 0;
            }
            else
            {
                var none = LoadAll().FirstOrDefault(r => r.IsNone);
                if (none != null && ddl.Items.FindByValue(none.CourierServiceID.ToString()) != null)
                    ddl.SelectedValue = none.CourierServiceID.ToString();
                else if (ddl.Items.Count > 0)
                    ddl.SelectedIndex = 0;
            }
        }

        public void EnsureExists()
        {
            ExecNonQuery(@"
IF OBJECT_ID(N'dbo.CourierServicesTbl', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CourierServicesTbl
    (
        CourierServiceID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CourierServicesTbl PRIMARY KEY,
        ServiceCode NVARCHAR(32) NOT NULL,
        ServiceName NVARCHAR(100) NOT NULL,
        TrackingUrl NVARCHAR(500) NULL,
        IsDefault BIT NOT NULL CONSTRAINT DF_CourierSvc_Default DEFAULT (0),
        IsEnabled BIT NOT NULL CONSTRAINT DF_CourierSvc_Enabled DEFAULT (1),
        SortOrder INT NOT NULL CONSTRAINT DF_CourierSvc_Sort DEFAULT (0),
        CONSTRAINT UQ_CourierServices_Code UNIQUE (ServiceCode)
    );
END");

            ExecNonQuery(@"
MERGE dbo.CourierServicesTbl AS t
USING (VALUES
    (N'None',        N'None',         NULL, 0, 1, 0),
    (N'Fastway',     N'Fastway',      N'https://www.fastway.co.za/our-services/track-your-parcel', 1, 1, 10),
    (N'Pargo',       N'Pargo',        N'https://pargo.co.za/track-trace/', 0, 1, 20),
    (N'CourierGuy',  N'Courier Guy',  N'https://thecourierguy.co.za/tracking/', 0, 1, 30)
) AS s(ServiceCode, ServiceName, TrackingUrl, IsDefault, IsEnabled, SortOrder)
ON t.ServiceCode = s.ServiceCode
WHEN NOT MATCHED THEN
    INSERT (ServiceCode, ServiceName, TrackingUrl, IsDefault, IsEnabled, SortOrder)
    VALUES (s.ServiceCode, s.ServiceName, s.TrackingUrl, s.IsDefault, s.IsEnabled, s.SortOrder);");

            // Contact preference column
            ExecNonQuery(@"
IF COL_LENGTH(N'dbo.ContactsTbl', N'PreferredCourierServiceID') IS NULL
    ALTER TABLE dbo.ContactsTbl ADD PreferredCourierServiceID INT NULL;");

            // Waybill FK (Carrier name still stored for display)
            ExecNonQuery(@"
IF OBJECT_ID(N'dbo.OrderWaybillTbl', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.OrderWaybillTbl', N'CourierServiceID') IS NULL
    ALTER TABLE dbo.OrderWaybillTbl ADD CourierServiceID INT NULL;");

            InvalidateCache();
        }

        private void ClearOtherDefaults(int keepId)
        {
            const string sql = @"
UPDATE CourierServicesTbl SET IsDefault = 0
WHERE CourierServiceID <> @KeepId AND IsDefault = 1";
            ExecNonQuery(sql, new List<DBParameter>
            {
                new DBParameter { ParamName = "@KeepId", DataValue = keepId, DataDbType = DbType.Int32 }
            });
        }

        private List<DBParameter> BuildParams(CourierService entity, bool includeId)
        {
            var list = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ServiceCode", DataValue = (entity.ServiceCode ?? string.Empty).Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@ServiceName", DataValue = (entity.ServiceName ?? string.Empty).Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@TrackingUrl", DataValue = string.IsNullOrWhiteSpace(entity.TrackingUrl) ? (object)DBNull.Value : entity.TrackingUrl.Trim(), DataDbType = DbType.String },
                new DBParameter { ParamName = "@IsDefault", DataValue = entity.IsDefault, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@IsEnabled", DataValue = entity.IsEnabled, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@SortOrder", DataValue = entity.SortOrder, DataDbType = DbType.Int32 }
            };
            if (includeId)
                list.Add(new DBParameter { ParamName = "@CourierServiceID", DataValue = entity.CourierServiceID, DataDbType = DbType.Int32 });
            return list;
        }

        private List<CourierService> LoadAll()
        {
            if (_allCache != null)
                return _allCache;

            var list = new List<CourierService>();
            try
            {
                string sql = "SELECT " + CoreColumns + " FROM CourierServicesTbl ORDER BY SortOrder, ServiceName";
                using (var db = CreateDb())
                using (var rdr = db.ExecuteReader(sql))
                {
                    while (rdr != null && rdr.Read())
                        list.Add(Map(rdr));
                }
            }
            catch
            {
                list = BuiltInRows();
            }

            _allCache = list;
            return list;
        }

        private void InvalidateCache()
        {
            _allCache = null;
        }

        private static CourierService Map(IDataRecord rdr)
        {
            return new CourierService
            {
                CourierServiceID = Convert.ToInt32(rdr["CourierServiceID"]),
                ServiceCode = rdr["ServiceCode"] as string,
                ServiceName = rdr["ServiceName"] as string,
                TrackingUrl = rdr["TrackingUrl"] as string,
                IsDefault = rdr["IsDefault"] != DBNull.Value && Convert.ToBoolean(rdr["IsDefault"]),
                IsEnabled = rdr["IsEnabled"] == DBNull.Value || Convert.ToBoolean(rdr["IsEnabled"]),
                SortOrder = rdr["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["SortOrder"])
            };
        }

        private static List<CourierService> BuiltInRows()
        {
            return new List<CourierService>
            {
                new CourierService { CourierServiceID = 1, ServiceCode = "None", ServiceName = "None", TrackingUrl = null, IsDefault = false, IsEnabled = true, SortOrder = 0 },
                new CourierService { CourierServiceID = 2, ServiceCode = "Fastway", ServiceName = "Fastway", TrackingUrl = "https://www.fastway.co.za/our-services/track-your-parcel", IsDefault = true, IsEnabled = true, SortOrder = 10 },
                new CourierService { CourierServiceID = 3, ServiceCode = "Pargo", ServiceName = "Pargo", TrackingUrl = "https://pargo.co.za/track-trace/", IsDefault = false, IsEnabled = true, SortOrder = 20 },
                new CourierService { CourierServiceID = 4, ServiceCode = "CourierGuy", ServiceName = "Courier Guy", TrackingUrl = "https://thecourierguy.co.za/tracking/", IsDefault = false, IsEnabled = true, SortOrder = 30 }
            };
        }
    }
}
