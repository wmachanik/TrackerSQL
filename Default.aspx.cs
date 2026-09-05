//------------------------------------------------------------------------------
// TrackerSQL v3.x — _Default
// WebForms page code-behind for _Default.
//------------------------------------------------------------------------------

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Repositories;

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
            // Show user-admin cards for Administrators (and Administrator alias / configured admin user)
            if (pnlUserAdministration != null)
                pnlUserAdministration.Visible = SecurityManager.IsAdmin();

            if (!IsPostBack)
            {
                var now = TimeZoneUtils.Now();
                litCurrentDate.Text = $"Date: {now:dddd, dd MMM yyyy HH:mm} {TimeZoneUtils.GetZoneAbbreviation()}";
                try
                {
                    // Live total across contacts (legacy: SUM(LastCupCount) FROM ClientUsageTbl).
                    long totalCups = new ContactsUsageRepository().GetSumOfLastCupCounts();
                    lblTotalCupCount.Text = totalCups.ToString("n0");
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, "Default.aspx: TotalCount query failed: " + ex.Message);
                    lblTotalCupCount.Text = "0";
                }
            }
        }
    }
}
