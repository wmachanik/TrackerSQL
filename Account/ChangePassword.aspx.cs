// Decompiled with JetBrains decompiler
// Type: TrackerSQL.Account.ChangePassword
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Web.UI;
using TrackerSQL.Classes;

////- only form later versions #nullable disable
namespace TrackerSQL.Account
{

    public partial class ChangePassword : Page
    {
        protected System.Web.UI.WebControls.ChangePassword ChangeUserPassword;

        protected void Page_Load(object sender, EventArgs e)
        {
            AppLogger.WriteLog(SystemConstants.LogTypes.Login, "User change password page entered.");

        }
    }
}