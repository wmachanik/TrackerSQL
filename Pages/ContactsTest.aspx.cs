using System;
using System.Text;
using System.Web.UI;
using TrackerDotNet.Classes.Sql;
using TrackerSQL.Classes;

namespace TrackerSQL.Pages
{
    public partial class ContactsTest : Page
    {
        protected System.Web.UI.WebControls.Button btnTest;
        protected System.Web.UI.WebControls.Label lblResults;
        protected System.Web.UI.WebControls.GridView gvTestData;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnTest_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<h2>Diagnostic Test Results</h2>");

            try
            {
                // Test 1: Check connection string
                sb.AppendLine("<h3>1. Connection String Test</h3>");
                var connStr = System.Configuration.ConfigurationManager.ConnectionStrings["TrackerDataSQL"]?.ConnectionString;
                if (!string.IsNullOrEmpty(connStr))
                {
                    sb.AppendLine($"<p style='color:green;'>? TrackerDataSQL connection string found</p>");
                    sb.AppendLine($"<p>Connection: {connStr.Substring(0, Math.Min(50, connStr.Length))}...</p>");
                }
                else
                {
                    sb.AppendLine("<p style='color:red;'>? TrackerDataSQL connection string NOT found!</p>");
                }

                // Test 2: Direct SQL query
                sb.AppendLine("<h3>2. Direct SQL Query Test</h3>");
                try
                {
                    using (var db = new TrackerSQLDb())
                    {
                        var sql = @"SELECT TOP 5 c.ContactID, c.CompanyName, c.ContactFirstName, c.ContactLastName,
                                          a.AreaName AS City, c.PhoneNumber, c.EmailAddress, p.Abbreviation AS DeliveryBy,
                                          e.EquipTypeName, c.MachineSN, c.AutoFulfill, c.Enabled
                                   FROM ContactsTbl c
                                   LEFT OUTER JOIN PeopleTbl p ON c.PreferedAgentID = p.PersonID
                                   LEFT OUTER JOIN AreasTbl a ON c.Area = a.AreaID
                                   LEFT OUTER JOIN EquipTypesTbl e ON c.EquipTypeID = e.EquipTypeID
                                   WHERE c.Enabled = 1
                                   ORDER BY c.CompanyName";

                        var dt = db.ReturnDataTable(sql);
                        sb.AppendLine($"<p style='color:green;'>? SQL query executed successfully</p>");
                        sb.AppendLine($"<p>Rows returned: {dt.Rows.Count}</p>");
                        sb.AppendLine($"<p>Columns: {dt.Columns.Count}</p>");

                        if (dt.Rows.Count > 0)
                        {
                            sb.AppendLine("<p>First row sample:</p><ul>");
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                sb.AppendLine($"<li><strong>{dt.Columns[i].ColumnName}</strong>: {dt.Rows[0][i]}</li>");
                            }
                            sb.AppendLine("</ul>");
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"<p style='color:red;'>? SQL query failed: {ex.Message}</p>");
                    sb.AppendLine($"<pre>{ex.StackTrace}</pre>");
                }

                // Test 3: Repository method call
                sb.AppendLine("<h3>3. Repository Method Test</h3>");
                try
                {
                    var repo = new ContactSummariesRepository();
                    var contacts = repo.GetAllContactSummaries("CompanyName", 1, string.Empty);
                    sb.AppendLine($"<p style='color:green;'>? Repository method executed successfully</p>");
                    sb.AppendLine($"<p>Contacts returned: {contacts.Count}</p>");

                    if (contacts.Count > 0)
                    {
                        sb.AppendLine("<p>First contact sample:</p><ul>");
                        var first = contacts[0];
                        sb.AppendLine($"<li><strong>CustomerID</strong>: {first.CustomerID}</li>");
                        sb.AppendLine($"<li><strong>CompanyName</strong>: {first.CompanyName}</li>");
                        sb.AppendLine($"<li><strong>ContactFirstName</strong>: {first.ContactFirstName}</li>");
                        sb.AppendLine($"<li><strong>ContactLastName</strong>: {first.ContactLastName}</li>");
                        sb.AppendLine($"<li><strong>City</strong>: {first.City}</li>");
                        sb.AppendLine($"<li><strong>PhoneNumber</strong>: {first.PhoneNumber}</li>");
                        sb.AppendLine($"<li><strong>EmailAddress</strong>: {first.EmailAddress}</li>");
                        sb.AppendLine($"<li><strong>DeliveryBy</strong>: {first.DeliveryBy}</li>");
                        sb.AppendLine($"<li><strong>EquipTypeName</strong>: {first.EquipTypeName}</li>");
                        sb.AppendLine($"<li><strong>MachineSN</strong>: {first.MachineSN}</li>");
                        sb.AppendLine($"<li><strong>autofulfill</strong>: {first.autofulfill}</li>");
                        sb.AppendLine($"<li><strong>enabled</strong>: {first.enabled}</li>");
                        sb.AppendLine("</ul>");

                        // Bind to GridView
                        gvTestData.DataSource = contacts;
                        gvTestData.DataBind();
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"<p style='color:red;'>? Repository method failed: {ex.Message}</p>");
                    sb.AppendLine($"<pre>{ex.StackTrace}</pre>");
                }

                sb.AppendLine("<h3>Test Complete</h3>");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"<p style='color:red;'>? Unexpected error: {ex.Message}</p>");
                sb.AppendLine($"<pre>{ex.StackTrace}</pre>");
            }

            lblResults.Text = sb.ToString();
        }
    }
}
