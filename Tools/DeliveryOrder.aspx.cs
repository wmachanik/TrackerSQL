using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    /// <summary>The order drivers really delivered in (DeliveryVisitTbl), next to where today's route suggestion puts each stop.</summary>
    public partial class DeliveryOrder : Page
    {
        public class VisitRow
        {
            public string Driver { get; set; }
            public string StopText { get; set; }
            public string TimeText { get; set; }
            public int ContactID { get; set; }
            public string Contact { get; set; }
            public string Area { get; set; }
            public string PlanText { get; set; }
            public string SuggestedText { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            litVisits.Text = DeliveryVisitRepository.LearnFromVisits.ToString(CultureInfo.InvariantCulture);
            litMinStops.Text = DeliveryVisitRepository.MinStopsToLearn.ToString(CultureInfo.InvariantCulture);
            if (IsPostBack)
                return;

            if (!DeliveryVisitRepository.TableSeen())
            {
                ShowMessage("The delivery order table is not installed yet; restart the site (SQLCommands-DriverGps-02.xml).", true);
                return;
            }
            List<DateTime> recent = new DeliveryVisitRepository().ListDates(10);
            litRecentDays.Text = recent.Count == 0
                ? "No days from the driver app yet."
                : string.Join(" · ", recent.Select(d => string.Format(CultureInfo.InvariantCulture,
                    "<a href=\"?date={0:yyyy-MM-dd}\">{0:ddd dd MMM}</a>", d)));

            DateTime day = recent.Count > 0 ? recent[0] : TimeZoneUtils.Now().Date;
            if (DateTime.TryParseExact(Request.QueryString["date"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime wanted))
                day = wanted.Date;
            txtDate.Text = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            BindDrivers();
            BindVisits();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            BindVisits();
        }

        protected void Date_Changed(object sender, EventArgs e)
        {
            BindDrivers();
            BindVisits();
        }

        protected void btnPrevDay_Click(object sender, EventArgs e)
        {
            MoveDay(-1);
        }

        protected void btnNextDay_Click(object sender, EventArgs e)
        {
            MoveDay(1);
        }

        private void MoveDay(int days)
        {
            DateTime date = TryGetDate(out DateTime current) ? current : TimeZoneUtils.Now().Date;
            txtDate.Text = date.AddDays(days).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            BindDrivers();
            BindVisits();
        }

        private bool TryGetDate(out DateTime date)
        {
            return DateTime.TryParseExact(txtDate.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private void BindDrivers()
        {
            string selected = ddlDriver.SelectedValue;
            ddlDriver.Items.Clear();
            ddlDriver.Items.Add(new ListItem("All drivers", string.Empty));
            if (!TryGetDate(out DateTime date))
                return;
            foreach (var driver in new DeliveryVisitRepository().ListDay(date, null)
                .GroupBy(v => v.PersonID).Select(g => g.First()))
            {
                ddlDriver.Items.Add(new ListItem(driver.DriverAbbreviation ?? ("Person " + driver.PersonID),
                    driver.PersonID.ToString(CultureInfo.InvariantCulture)));
            }
            if (ddlDriver.Items.FindByValue(selected) != null)
                ddlDriver.SelectedValue = selected;
        }

        private void BindVisits()
        {
            if (!TryGetDate(out DateTime date))
            {
                gvVisits.DataSource = new List<VisitRow>();
                gvVisits.DataBind();
                return;
            }
            int? personId = int.TryParse(ddlDriver.SelectedValue, out int p) ? p : (int?)null;

            try
            {
                List<DeliveryVisitRepository.Visit> visits = new DeliveryVisitRepository().ListDay(date, personId);
                Dictionary<string, int> suggested = SuggestedStops(date);
                Dictionary<string, int> plan = new DeliveryRouteRepository().GetDay(date);
                gvVisits.DataSource = visits.Select(v => new VisitRow
                {
                    Driver = v.DriverAbbreviation ?? ("Person " + v.PersonID),
                    StopText = v.Position.HasValue ? v.Position.Value + " of " + v.Stops : "-",
                    TimeText = v.FirstAt.ToString("HH:mm", CultureInfo.InvariantCulture)
                        + (v.LastAt > v.FirstAt.AddMinutes(1) ? " (again " + v.LastAt.ToString("HH:mm", CultureInfo.InvariantCulture) + ")" : string.Empty),
                    ContactID = v.ContactID,
                    Contact = v.CompanyName ?? ("Contact " + v.ContactID),
                    Area = v.AreaName,
                    PlanText = plan.TryGetValue(v.PersonID + ":" + v.ContactID, out int planned) ? planned.ToString(CultureInfo.InvariantCulture) : "-",
                    SuggestedText = suggested.TryGetValue(v.PersonID + ":" + v.ContactID, out int seq)
                        || suggested.TryGetValue("*:" + v.ContactID, out seq) ? seq.ToString(CultureInfo.InvariantCulture) : "-"
                }).ToList();
                gvVisits.DataBind();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Delivery order page: " + ex.Message);
                ShowMessage("Could not load the delivery order: " + ex.Message, true);
            }
        }

        /// <summary>Stop numbers on that day's sheet as suggested today, keyed "driver:contact" (and "*:contact").</summary>
        private static Dictionary<string, int> SuggestedStops(DateTime date)
        {
            var map = new Dictionary<string, int>();
            foreach (MobileDeliveryStop stop in new MobileDeliveryManager().GetSheet(date, null).Orders)
            {
                string key = (stop.DeliveryById ?? 0) + ":" + stop.ContactId;
                if (!map.ContainsKey(key))
                    map[key] = stop.Seq;
                if (!map.ContainsKey("*:" + stop.ContactId))
                    map["*:" + stop.ContactId] = stop.Seq;
            }
            return map;
        }

        private void ShowMessage(string text, bool isError)
        {
            lblMessage.Text = System.Web.HttpUtility.HtmlEncode(text);
            lblMessage.CssClass = "status-message " + (isError ? "status-error" : "status-success");
            lblMessage.Visible = true;
        }
    }
}
