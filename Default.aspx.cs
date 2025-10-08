// Decompiled with JetBrains decompiler
// Type: TrackerDotNet._Default
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet
{

    public partial class Default : Page
    {
        //protected Label lblTotalCupCount;
        //protected SqlDataSource sdsCupCountTotal;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            bool flag = new CheckBrowser().fBrowserIsMobile();
            this.Session["RunningOnMoble"] = (object)flag;
            //if (flag)
            //    this.MasterPageFile = "~/MobileSite.master";
            //else
            //    this.MasterPageFile = "~/Site.master";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var now = TimeZoneUtils.Now();
                // Example format: Monday, 03 Feb 2025 14:32 SAST
                litCurrentDate.Text = $"Date: {now:dddd, dd MMM yyyy HH:mm} {TimeZoneUtils.GetZoneAbbreviation()}";
                this.sdsCupCountTotal.DataBind();
                foreach (DataRowView dataRowView in (DataView)this.sdsCupCountTotal.Select(DataSourceSelectArguments.Empty))
                    this.lblTotalCupCount.Text = $"{dataRowView["TotalCupCount"]:n0}";
            }
        }
    }
}