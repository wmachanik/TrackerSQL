// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Administration.ManageUsers
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Administration
{
    public partial class ManageUsers : Page
    {
        protected GridView gvUserAccounts;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.Page.IsPostBack)
                return;
            this.BindUserAccounts();
        }

        private void BindUserAccounts()
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "User management page entered.");

            this.gvUserAccounts.DataSource = (object)Membership.GetAllUsers();
            this.gvUserAccounts.DataBind();
        }
    }
}