// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.MobileSiteMaster
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerDotNet
{
    public partial class MobileSiteMaster : MasterPage
    {
        protected ContentPlaceHolder HeadContent;
        protected HtmlForm frmMiniMain;
        protected HyperLink hlHome;
        protected HyperLink hlDeliveries;
        protected HyperLink hlRepairs;
        protected ContentPlaceHolder MainContent;

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}