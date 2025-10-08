// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Account.Register
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Account
{
    public partial class Register : Page
    {
        protected CreateUserWizard RegisterUser;
        protected CreateUserWizardStep RegisterUserWizardStep;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.RegisterUser.ContinueDestinationPageUrl = this.Request.QueryString["ReturnUrl"];
        }

        protected void RegisterUser_CreatedUser(object sender, EventArgs e)
        {
            FormsAuthentication.SetAuthCookie(this.RegisterUser.UserName, false);
            string url = this.RegisterUser.ContinueDestinationPageUrl;
            if (string.IsNullOrEmpty(url))
                url = "~/";
            this.Response.Redirect(url);
        }
    }
}