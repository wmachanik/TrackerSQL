using System;
using System.Data;
using System.Data.OleDb;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

namespace TrackerDotNet.test
{
    public partial class ShowTableStruct : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTableNames();
            }
        }

        private void LoadTableNames()
        {
            try
            {
                TrackerDb db = new TrackerDb();
                
                ddlTables.Items.Clear();
                ddlTables.Items.Add(new ListItem("-- Select Table --", ""));
                
                // Method 1: Try using ADOX (preferred method for Access)
                bool tablesLoaded = TryLoadTablesWithADOX(db);
                
                // Method 2: If ADOX fails, try direct SQL queries
                if (!tablesLoaded)
                {
                    tablesLoaded = TryLoadTablesWithSQL(db);
                }
                
                // Method 3: Fallback to hardcoded list
                if (!tablesLoaded)
                {
                    LoadFallbackTableNames();
                }
                
                db.Close();
            }
            catch (Exception ex)
            {
                LoadFallbackTableNames();
                Response.Write($"<script>alert('Error loading table names: {ex.Message}');</script>");
            }
        }

        private bool TryLoadTablesWithADOX(TrackerDb db)
        {
            try
            {
                // Get the connection string from TrackerDb
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[SystemConstants.DatabaseConstants.ConnectionStringName].ConnectionString;
                
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    DataTable tables = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                    
                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        if (!tableName.StartsWith("MSys") && !tableName.StartsWith("~"))
                        {
                            ddlTables.Items.Add(new ListItem(tableName, tableName));
                        }
                    }
                    
                    return ddlTables.Items.Count > 1; // More than just the "-- Select Table --" item
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TryLoadTablesWithSQL(TrackerDb db)
        {
            try
            {
                // Try different SQL approaches
                string[] sqlQueries = {
                    "SELECT Name FROM MSysObjects WHERE Type=1 AND Flags=0 ORDER BY Name",
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'",
                    "SHOW TABLES"
                };

                foreach (string sql in sqlQueries)
                {
                    try
                    {
                        var reader = db.ExecuteSQLGetDataReader(sql);
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                string tableName = reader[0].ToString();
                                if (!tableName.StartsWith("MSys") && !tableName.StartsWith("~"))
                                {
                                    ddlTables.Items.Add(new ListItem(tableName, tableName));
                                }
                            }
                            reader.Close();
                            
                            if (ddlTables.Items.Count > 1)
                            {
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // Try next query
                        continue;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void LoadFallbackTableNames()
        {
            ddlTables.Items.Clear();
            ddlTables.Items.Add(new ListItem("-- Select Table --", ""));
            
            string[] tableNames = { 
                "SysDataTbl", 
                "CustomerTbl", 
                "OrderTbl", 
                "ItemTbl", 
                "DeliveryMethodTbl", 
                "ItemTypeTbl",
                "OrderItemsTbl",
                "UsersTbl"
            };
            
            foreach (string tableName in tableNames)
            {
                ddlTables.Items.Add(new ListItem(tableName, tableName));
            }
        }

        protected void chkFullView_CheckedChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlTables.SelectedValue))
            {
                ShowTableStructure(ddlTables.SelectedValue);
            }
        }

        protected void ddlTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlTables.SelectedValue))
            {
                ShowTableStructure(ddlTables.SelectedValue);
            }
        }

        protected void btnShowData_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlTables.SelectedValue))
            {
                ShowTableData(ddlTables.SelectedValue);
            }
        }

        private void ShowTableStructure(string tableName)
        {
            try
            {
                TrackerDb db = new TrackerDb();
                
                string sql = chkFullView.Checked 
                    ? $"SELECT * FROM [{tableName}] WHERE 1=0" // Get structure only
                    : $"SELECT TOP 1 * FROM [{tableName}]"; // Get structure with sample data
                
                DataTable structure = new DataTable();
                
                var reader = db.ExecuteSQLGetDataReader(sql);
                if (reader != null)
                {
                    structure.Load(reader);
                    reader.Close();
                }
                
                // Create a schema table to show column information
                DataTable schemaTable = new DataTable();
                schemaTable.Columns.Add("Column Name", typeof(string));
                schemaTable.Columns.Add("Data Type", typeof(string));
                schemaTable.Columns.Add("Allow Nulls", typeof(string));
                schemaTable.Columns.Add("Max Length", typeof(string));
                
                foreach (DataColumn column in structure.Columns)
                {
                    DataRow row = schemaTable.NewRow();
                    row["Column Name"] = column.ColumnName;
                    row["Data Type"] = column.DataType.Name;
                    row["Allow Nulls"] = column.AllowDBNull ? "Yes" : "No";
                    row["Max Length"] = column.MaxLength == -1 ? "MAX" : column.MaxLength.ToString();
                    schemaTable.Rows.Add(row);
                }
                
                gvStructure.DataSource = schemaTable;
                gvStructure.DataBind();
                
                // Show the data button
                btnShowData.Visible = true;
                btnShowData.Text = $"Show Top 10 Records from {tableName}";
                
                db.Close();
            }
            catch (Exception ex)
            {
                gvStructure.DataSource = null;
                gvStructure.DataBind();
                btnShowData.Visible = false;
                Response.Write($"<script>alert('Error showing table structure: {ex.Message}');</script>");
            }
        }

        private void ShowTableData(string tableName)
        {
            try
            {
                TrackerDb db = new TrackerDb();
                
                string sql = $"SELECT TOP 10 * FROM [{tableName}]";
                
                DataTable dataTable = new DataTable();
                
                var reader = db.ExecuteSQLGetDataReader(sql);
                if (reader != null)
                {
                    dataTable.Load(reader);
                    reader.Close();
                }
                
                gvStructure.DataSource = dataTable;
                gvStructure.DataBind();
                
                // Update button text to show structure again
                btnShowData.Text = $"Show Structure of {tableName}";
                btnShowData.CommandArgument = "structure";
                
                db.Close();
            }
            catch (Exception ex)
            {
                Response.Write($"<script>alert('Error showing table data: {ex.Message}');</script>");
            }
        }
    }
}

