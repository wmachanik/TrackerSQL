// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.LoadSendCoffeeCheckup
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class LoadSendCoffeeCheckup : Page
    {
        private const string CONST_LOADTIMERFIRE = "LoadTimerFier";
        protected HtmlGenericControl DivLoading;

        protected void Page_Load(object sender, EventArgs e)
        {
            int num = this.IsPostBack ? 1 : 0;
        }
    }
}