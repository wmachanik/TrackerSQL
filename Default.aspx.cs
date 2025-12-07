// Decompiled with JetBrains decompiler
// Type: TrackerSQL._Default
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Web.UI;
using TrackerSQL.Classes;
using TrackerDotNet.Classes.Sql;

//- only form later versions #nullable disable
namespace TrackerSQL
{

    public partial class Default : Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            bool flag = new CheckBrowser().fBrowserIsMobile();
            this.Session["RunningOnMoble"] = (object)flag;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var now = TimeZoneUtils.Now();
                // Example format: Monday, 03 Feb 2025 14:32 SAST
                litCurrentDate.Text = $"Date: {now:dddd, dd MMM yyyy HH:mm} {TimeZoneUtils.GetZoneAbbreviation()}";
                try
                {
                    var repo = new TotalCountTrackerRepository();
                    var latest = repo.GetLatest();
                    if (latest != null && latest.TotalCount.HasValue)
                        lblTotalCupCount.Text = latest.TotalCount.Value.ToString("n0");
                    else
                        lblTotalCupCount.Text = "0";
                }
                catch (Exception ex)
                {
                    // Fallback display; log error
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, "Default.aspx: TotalCount query failed: " + ex.Message);
                    lblTotalCupCount.Text = "0";
                }
            }
        }
    }
}