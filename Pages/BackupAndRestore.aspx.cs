// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.BackupAndRestore
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class BackupAndRestore : Page
    {
        protected Button btnSecurty2Local;
        protected Literal ltrlMsg;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSecurty2Local_Click(object sender, EventArgs e)
        {
            try
            {
                string str = "C:\\backups\\TrackerDOTNetSecurityBackup.bak";
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ApplicationServices"].ToString());
                SqlCommand sqlCommand = new SqlCommand();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
                DataTable dataTable = new DataTable();
                connection.Open();
                new SqlCommand($"restore database TrackerDoNetSecurity FROM DISK = '{str}'", connection).ExecuteNonQuery();
                connection.Close();
                this.ltrlMsg.Text = "Restore complete";
            }
            catch (Exception ex)
            {
                this.ltrlMsg.Text = "<b>ERROR:</b> " + ex.Message;
            }
        }
    }
}