using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Xml;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Runs create/alter commands from App_Data SQLCommands-*.xml packs (XMLtoSQL format).
    /// </summary>
    public static class XmlSchemaCommandRunner
    {
        public static string ResolveXmlPath(string fileName)
        {
            string name = Path.GetFileName(fileName ?? string.Empty);
            if (string.IsNullOrEmpty(name) || !string.Equals(name, fileName, StringComparison.Ordinal))
                name = Path.GetFileName(name);

            string appData;
            if (HttpContext.Current != null)
                appData = HttpContext.Current.Server.MapPath("~/App_Data/");
            else
                appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

            string direct = Path.Combine(appData, name);
            if (File.Exists(direct))
                return direct;

            string archived = Path.Combine(appData, "Archive", name);
            if (File.Exists(archived))
                return archived;

            return direct;
        }

        public static List<string> LoadSchemaCommands(string path)
        {
            var list = new List<string>();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return list;

            var doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList nodes = doc.SelectNodes("//command[@type='create' or @type='alter']");
            if (nodes == null)
                return list;

            foreach (XmlNode node in nodes)
            {
                string sql = (node.InnerText ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(sql))
                    list.Add(sql);
            }
            return list;
        }

        /// <summary>
        /// Runs create/alter commands for packs whose tables or columns are not in the database yet.
        /// Looks in App_Data, then App_Data\Archive. A pack already present in the database is skipped
        /// even when the XML file has been archived or removed.
        /// </summary>
        public static int RunFiles(IEnumerable<string> fileNames, out string error)
        {
            error = null;
            if (fileNames == null)
                return 0;

            int ran = 0;
            using (var db = new TrackerSQLDb())
            {
                foreach (string file in fileNames)
                {
                    if (string.IsNullOrWhiteSpace(file))
                        continue;

                    string missing = MissingSchemaPiece(db, file);
                    if (missing == null)
                        continue;

                    string path = ResolveXmlPath(file);
                    if (!File.Exists(path))
                    {
                        error = "Database is missing " + missing + ". Schema XML not found in App_Data or Archive: " + Path.GetFileName(file);
                        return ran;
                    }

                    foreach (string sql in LoadSchemaCommands(path))
                    {
                        db.ExecuteNonQuery(sql);
                        ran++;
                    }

                    AppLogger.WriteLog(SystemConstants.LogTypes.System,
                        "Applied schema pack " + Path.GetFileName(file) + " (database was missing " + missing + ").");
                }
            }
            return ran;
        }

        /// <summary>Null when this pack's tables and columns are already in the database.</summary>
        private static string MissingSchemaPiece(TrackerSQLDb db, string fileName)
        {
            string name = Path.GetFileName(fileName);
            SchemaProbe[] probes;
            if (!Probes.TryGetValue(name, out probes) || probes == null || probes.Length == 0)
                return name;

            foreach (SchemaProbe probe in probes)
            {
                if (!probe.Exists(db))
                    return probe.Describe();
            }
            return null;
        }

        private static bool TableExists(TrackerSQLDb db, string table)
        {
            object result = db.ExecuteScalar(
                "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@TableName", DataValue = table, DataDbType = DbType.String }
                });
            return result != null && result != DBNull.Value;
        }

        private static bool ColumnExists(TrackerSQLDb db, string table, string column)
        {
            object result = db.ExecuteScalar(
                "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName",
                new List<DBParameter>
                {
                    new DBParameter { ParamName = "@TableName", DataValue = table, DataDbType = DbType.String },
                    new DBParameter { ParamName = "@ColumnName", DataValue = column, DataDbType = DbType.String }
                });
            return result != null && result != DBNull.Value;
        }

        private sealed class SchemaProbe
        {
            public string Table;
            public string Column;

            public bool Exists(TrackerSQLDb db)
            {
                return string.IsNullOrEmpty(Column) ? TableExists(db, Table) : ColumnExists(db, Table, Column);
            }

            public string Describe()
            {
                return string.IsNullOrEmpty(Column) ? "table " + Table : Table + "." + Column;
            }
        }

        private static SchemaProbe Table(string table)
        {
            return new SchemaProbe { Table = table };
        }

        private static SchemaProbe Column(string table, string column)
        {
            return new SchemaProbe { Table = table, Column = column };
        }

        /// <summary>Last object each startup pack creates. Present means that pack has already been applied.</summary>
        private static readonly Dictionary<string, SchemaProbe[]> Probes =
            new Dictionary<string, SchemaProbe[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "SQLCommands-MobileApi-01.xml", new[] { Table("MobileApiSettingsTbl"), Table("MobileDeviceTokensTbl") } },
                { "SQLCommands-OrderFlags-01.xml", new[] { Column("OrdersTbl", "InvoiceTypeID") } },
                { "SQLCommands-AreaDeliveryOrder-01.xml", new[] { Column("AreasTbl", "ShortCode") } },
                { "SQLCommands-DriverGps-01.xml", new[] { Table("GpsSettingsTbl") } },
                { "SQLCommands-DriverGps-02.xml", new[] { Table("DeliveryRouteTbl") } },
                { "SQLCommands-DriverGps-03.xml", new[] { Table("DriverLocationPingTbl") } },
                { "SQLCommands-DriverGps-04.xml", new[] { Column("MobileDeliveryProofTbl", "NotAtClient") } },
                { "SQLCommands-DriverGps-05.xml", new[] { Column("MobileApiSettingsTbl", "DeliveryTerms") } },
                { "SQLCommands-DriverGps-06.xml", new[] { Table("RoadLegTbl") } },
                { "SQLCommands-DriverGps-07.xml", new[] { Column("PeopleTbl", "HandsOverCollections") } },
                { "SQLCommands-DriverGps-08.xml", new[] { Column("OrdersTbl", "DeliverToAddressID") } },
                { "SQLCommands-DriverGps-09.xml", new[] { Column("ContactDeliverToAddressTbl", "AddressKind") } },
                { "SQLCommands-DriverGps-10.xml", new[] { Column("ContactDeliverToAddressTbl", "CellNumber") } },
                { "SQLCommands-DriverGps-11.xml", new[] { Column("GpsSettingsTbl", "DefaultStartTime") } },
                { "SQLCommands-DriverGps-12.xml", new[] { Column("OrdersTbl", "WalkInAreaID") } },
                { "SQLCommands-DriverGps-13.xml", new[] { Column("ContactDeliverToAddressTbl", "LastUsedDate") } },
                { "SQLCommands-DriverGps-14.xml", new[] { Table("AltAddressKindsTbl") } },
                { "SQLCommands-DriverGps-15.xml", new[] { Column("AltAddressKindsTbl", "IsSundryDefault") } },
                { "SQLCommands-DriverGps-16.xml", new[] { Column("DeliveryRouteTbl", "StopKey") } },
                { "SQLCommands-DriverGps-17.xml", new[] { Column("MobileApiSettingsTbl", "SupplierCollectionContactTypeID") } },
                { "SQLCommands-DriverGps-18.xml", new[] { Column("ContactTypesTbl", "DriverCollectionOnly") } },
                { "SQLCommands-OrderDone-01.xml", new[] { Column("WooCommerceSettingsTbl", "CompleteWooOrderOnDone") } },
                { "SQLCommands-WooCommerce-01.xml", new[] { Table("WooCatalogCacheTbl") } },
                { "SQLCommands-WooCommerce-02.xml", new[] { Table("WooPaymentMethodMapTbl") } },
                { "SQLCommands-WooCommerce-03.xml", new[] { Table("OrderWaybillTbl") } },
                { "SQLCommands-WooCommerce-04.xml", new[] { Column("WooCommerceSettingsTbl", "CheckNewOrdersOnNewOrder") } },
                { "SQLCommands-WooCommerce-05.xml", new[] { Column("WooCommerceSettingsTbl", "ImportCollectShippingKeywords") } },
                { "SQLCommands-WooCommerce-06.xml", new[] { Table("QuaffeeLoversGrantTbl") } },
                { "SQLCommands-WooCommerce-07.xml", new[] { Column("WooCommerceSettingsTbl", "WordPressRoleGrantUsername") } },
                { "SQLCommands-DeliveryPromise-01.xml", new[] { Table("DeliveryPromiseRuleTbl") } },
                { "SQLCommands-Postal-01.xml", new[] { Table("SaPostalCodeTbl") } },
                { "SQLCommands-ContactChangeLog-01.xml", new[] { Table("ContactChangeLogTbl") } },
                { "SQLCommands-ContactPortal-01.xml", new[] { Table("ContactPortalSettingsTbl") } },
                { "SQLCommands-ClientMail-01.xml", new[] { Table("ClientMailTemplatesTbl") } },
                { "SQLCommands-ClientMail-02.xml", new[] { Table("ClientMailSendLogTbl") } },
            };
    }
}
