using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    /// <summary>
    /// Each driver's stops for a day in route order with what is done, what is next and when the rest are expected
    /// (the same times as the driver app), and below it the order drivers really delivered in (DeliveryVisitTbl).
    /// </summary>
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

            DateTime day = TimeZoneUtils.Now().Date;
            if (DateTime.TryParseExact(Request.QueryString["date"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime wanted))
                day = wanted.Date;
            txtDate.Text = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            BindRecentDays();
            BindDrivers();
            BindAll();
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            BindAll();
        }

        protected void Date_Changed(object sender, EventArgs e)
        {
            BindDrivers();
            BindAll();
        }

        protected void btnPrevDay_Click(object sender, EventArgs e)
        {
            MoveDay(-1);
        }

        protected void btnNextDay_Click(object sender, EventArgs e)
        {
            MoveDay(1);
        }

        protected void btnToday_Click(object sender, EventArgs e)
        {
            txtDate.Text = TimeZoneUtils.Now().Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            BindDrivers();
            BindAll();
        }

        protected void tmrRefresh_Tick(object sender, EventArgs e)
        {
            BindAll();
        }

        private void MoveDay(int days)
        {
            DateTime date = TryGetDate(out DateTime current) ? current : TimeZoneUtils.Now().Date;
            txtDate.Text = date.AddDays(days).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            BindDrivers();
            BindAll();
        }

        private bool TryGetDate(out DateTime date)
        {
            return DateTime.TryParseExact(txtDate.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private int? SelectedPerson => int.TryParse(ddlDriver.SelectedValue, out int p) ? p : (int?)null;

        private void BindRecentDays()
        {
            if (!DeliveryVisitRepository.TableSeen())
            {
                litRecentDays.Text = "none yet.";
                return;
            }
            List<DateTime> recent = new DeliveryVisitRepository().ListDates(10);
            litRecentDays.Text = recent.Count == 0
                ? "none yet."
                : string.Join(" · ", recent.Select(d => string.Format(CultureInfo.InvariantCulture,
                    "<a href=\"?date={0:yyyy-MM-dd}\">{0:ddd dd MMM}</a>", d)));
        }

        /// <summary>Everyone with stops on the day's sheet or deliveries recorded that day.</summary>
        private void BindDrivers()
        {
            string selected = ddlDriver.SelectedValue;
            ddlDriver.Items.Clear();
            ddlDriver.Items.Add(new ListItem("All drivers", string.Empty));
            if (!TryGetDate(out DateTime date))
                return;
            var names = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (MobileDeliveryStop stop in new MobileDeliveryManager().GetSheet(date, null).Orders.Where(o => o.DeliveryById.HasValue))
                    names[stop.DeliveryBy ?? ("Person " + stop.DeliveryById.Value)] = stop.DeliveryById.Value;
                if (DeliveryVisitRepository.TableSeen())
                    foreach (var visit in new DeliveryVisitRepository().ListDay(date, null))
                        names[visit.DriverAbbreviation ?? ("Person " + visit.PersonID)] = visit.PersonID;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Delivery order drivers: " + ex.Message);
            }
            foreach (var pair in names.GroupBy(p => p.Value).Select(g => g.First()))
                ddlDriver.Items.Add(new ListItem(pair.Key, pair.Value.ToString(CultureInfo.InvariantCulture)));
            if (ddlDriver.Items.FindByValue(selected) != null)
                ddlDriver.SelectedValue = selected;
        }

        private void BindAll()
        {
            bool haveDate = TryGetDate(out DateTime date);
            tmrRefresh.Enabled = haveDate && date == TimeZoneUtils.Now().Date;
            BindBoard(haveDate ? date : (DateTime?)null);
            BindVisits(haveDate ? date : (DateTime?)null);
        }

        // ---------------- Board: done / next / expected per driver ----------------

        private void BindBoard(DateTime? date)
        {
            litBoard.Text = string.Empty;
            litUpdated.Text = string.Empty;
            if (!date.HasValue)
                return;
            DateTime day = date.Value;
            DateTime now = TimeZoneUtils.Now();
            try
            {
                List<MobileDeliveryStop> orders = new MobileDeliveryManager().GetSheet(day, null).Orders;
                RouteTimesManager.DayTimes times = new RouteTimesManager().ForDay(day, orders);
                Dictionary<int, DeliveryRouteRepository.PlanInfo> plans = DeliveryRouteRepository.TableSeen()
                    ? new DeliveryRouteRepository().GetPlanInfo(day) : new Dictionary<int, DeliveryRouteRepository.PlanInfo>();
                Dictionary<int, Person> people = (new PersonsRepository().GetAll("PersonName") ?? new List<Person>())
                    .GroupBy(p => p.PersonID).ToDictionary(g => g.Key, g => g.First());

                int? only = SelectedPerson;
                var drivers = orders.Where(o => !only.HasValue || o.DeliveryById == only)
                    .GroupBy(o => o.DeliveryById ?? 0)
                    .Select(g => new
                    {
                        PersonId = g.Key,
                        Stops = g.ToList(),
                        Own = times.Drivers.ContainsKey(g.Key),
                        Name = people.TryGetValue(g.Key, out Person p) ? p.PersonName : g.First().DeliveryBy ?? "No driver set"
                    })
                    .OrderBy(d => d.Own ? 0 : 1).ThenBy(d => d.Name)
                    .ToList();

                if (drivers.Count == 0)
                {
                    litBoard.Text = "<p class='muted'>No deliveries booked for " + day.ToString("ddd d MMM", CultureInfo.InvariantCulture) + ".</p>";
                    return;
                }

                var html = new StringBuilder();
                foreach (var driver in drivers)
                {
                    people.TryGetValue(driver.PersonId, out Person person);
                    times.Drivers.TryGetValue(driver.PersonId, out RouteTimesManager.DriverState state);
                    plans.TryGetValue(driver.PersonId, out DeliveryRouteRepository.PlanInfo plan);
                    AppendDriver(html, driver.Name, person, driver.Stops, state, plan, times, day, now);
                }
                litBoard.Text = html.ToString();
                litUpdated.Text = (day == now.Date ? "Updated " : "As at ") + now.ToString("HH:mm", CultureInfo.InvariantCulture)
                    + (day == now.Date ? " · refreshes every minute" : string.Empty);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Delivery order board: " + ex.Message);
                ShowMessage("Could not load the day's deliveries: " + ex.Message, true);
            }
        }

        private static void AppendDriver(StringBuilder html, string name, Person person, List<MobileDeliveryStop> stops,
            RouteTimesManager.DriverState state, DeliveryRouteRepository.PlanInfo plan, RouteTimesManager.DayTimes times, DateTime day, DateTime now)
        {
            List<List<MobileDeliveryStop>> visits = stops
                .GroupBy(s => s.ContactId > 0 && s.ContactId != SystemConstants.CustomerConstants.SundryCustomerID ? s.ContactId : -s.OrderId)
                .Select(g => g.OrderBy(s => s.Seq).ToList())
                .OrderBy(v => v[0].Seq)
                .ToList();
            int done = visits.Count(v => v.All(Finished));
            int notDelivered = visits.Count(v => v.All(Finished) && v.Any(s => s.Proof?.Outcome == MobileDeliveryManager.OutcomeNotDelivered));
            bool today = day == now.Date;

            var summary = new List<string>();
            if (state == null)
                summary.Add(DeliveryTypeText(person?.DeliveryType));
            else if (done == visits.Count)
                summary.Add("All done");
            else if (today)
                summary.Add(state.Started ? "On route" : "Not left the depot yet (times start once they leave or deliver)");
            summary.Add(done + " of " + visits.Count + " done" + (notDelivered > 0 ? " (" + notDelivered + " not delivered)" : string.Empty));
            DateTime? last = visits.Where(v => !v.All(Finished))
                .Select(v => times.Stops.TryGetValue(v[0].OrderId, out RouteTimesManager.StopTime t) ? t.Expected : null)
                .Where(t => t.HasValue).DefaultIfEmpty(null).Max();
            if (last.HasValue)
                summary.Add("last stop about " + last.Value.ToString("HH:mm", CultureInfo.InvariantCulture));
            if (state?.LastSeen != null && today)
                summary.Add("phone last seen " + state.LastSeen.Value.ToString("HH:mm", CultureInfo.InvariantCulture));
            if (plan != null)
            {
                string at = plan.SetAt.ToString(plan.SetAt.Date == now.Date ? "HH:mm" : "HH:mm 'on' ddd d MMM", CultureInfo.InvariantCulture);
                summary.Add(plan.ByOffice ? "order set by the office (" + plan.UserName + ") at " + at : "their own order (set in the app at " + at + ")");
            }

            string title = name + (string.IsNullOrWhiteSpace(person?.Abbreviation) || person.Abbreviation == name ? string.Empty : " (" + person.Abbreviation + ")");
            html.Append("<div class='dorder-driver'><h2>").Append(Enc(title)).Append("</h2>")
                .Append("<p class='small dorder-summary'>").Append(Enc(string.Join(" · ", summary))).Append("</p>")
                .Append("<div class='dorder-progress' title='").Append(done).Append(" of ").Append(visits.Count).Append(" done'><span style='width:")
                .Append(visits.Count == 0 ? 0 : done * 100 / visits.Count).Append("%'></span></div>")
                .Append("<table class='TblWhite small dorder-table'><thead><tr><th>#</th><th>Client</th><th>Area</th><th>Status</th></tr></thead><tbody>");

            foreach (List<MobileDeliveryStop> visit in visits)
            {
                string status = StatusOf(visit, times, day, now, out string rowCss, out string statusCss);
                MobileDeliveryStop first = visit[0];
                string client = Enc(first.Company ?? ("Order " + first.OrderId));
                if (first.ContactId > 0 && first.ContactId != SystemConstants.CustomerConstants.SundryCustomerID)
                    client = "<a href='" + VirtualPathUtility.ToAbsolute("~/Pages/ContactDetails.aspx") + "?ID=" + first.ContactId + "'>" + client + "</a>";
                if (visit.Count > 1)
                    client += " <span class='muted'>(" + visit.Count + " orders)</span>";
                html.Append("<tr class='").Append(rowCss).Append("'><td class='dorder-seq'>").Append(first.Seq).Append("</td><td>").Append(client)
                    .Append("</td><td>").Append(Enc(first.Area ?? string.Empty)).Append("</td><td class='dorder-status ").Append(statusCss).Append("'>")
                    .Append(Enc(status)).Append("</td></tr>");
            }
            html.Append("</tbody></table></div>");
        }

        /// <summary>Delivered / not delivered / collected at a time, next, expected, or pending.</summary>
        private static string StatusOf(List<MobileDeliveryStop> visit, RouteTimesManager.DayTimes times, DateTime day, DateTime now,
            out string rowCss, out string statusCss)
        {
            rowCss = string.Empty;
            statusCss = string.Empty;
            List<MobileDeliveryStop> open = visit.Where(s => !Finished(s)).ToList();
            if (open.Count == 0)
            {
                rowCss = "is-done";
                MobileDeliveryProofSummary proof = visit.Select(s => s.Proof).Where(p => p != null)
                    .OrderByDescending(p => p.DeliveredAt, StringComparer.Ordinal).FirstOrDefault();
                if (proof == null)
                {
                    statusCss = "is-done";
                    return "Done in Tracker";
                }
                string at = DateTime.TryParse(proof.DeliveredAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime when)
                    ? " " + when.ToString(when.Date == day.Date ? "HH:mm" : "HH:mm 'on' d MMM", CultureInfo.InvariantCulture) : string.Empty;
                if (proof.Outcome == MobileDeliveryManager.OutcomeNotDelivered)
                {
                    statusCss = "is-bad";
                    return "Not delivered" + at + (string.IsNullOrWhiteSpace(proof.Reason) ? string.Empty : " · " + proof.Reason);
                }
                if (proof.NotAtClient == true)
                {
                    statusCss = "is-away";
                    return (proof.Reason ?? "Not at the client") + at;
                }
                statusCss = "is-done";
                return "Delivered" + at + (string.IsNullOrWhiteSpace(proof.Reason) ? string.Empty : " · " + proof.Reason)
                    + (string.IsNullOrWhiteSpace(proof.ReceivedBy) ? string.Empty : " · " + proof.ReceivedBy);
            }

            string prefix = open.Count < visit.Count ? (visit.Count - open.Count) + " of " + visit.Count + " done · " : string.Empty;
            times.Stops.TryGetValue(open[0].OrderId, out RouteTimesManager.StopTime time);
            string expected = time?.Expected == null ? string.Empty
                : (time.Rough ? "roughly " : "about ") + time.Expected.Value.ToString("HH:mm", CultureInfo.InvariantCulture);
            if (time?.Next == true)
            {
                rowCss = "is-next";
                statusCss = "is-next";
                return prefix + "Next" + (expected.Length > 0 ? " · " + expected : string.Empty);
            }
            if (expected.Length > 0)
                return prefix + expected;
            if (day.Date < now.Date)
            {
                statusCss = "is-missing";
                return prefix + "Not captured in the app";
            }
            return prefix + "To do";
        }

        private static bool Finished(MobileDeliveryStop stop) => stop.Done || stop.Proof != null;

        private static string DeliveryTypeText(string type)
        {
            switch (DeliveryTypes.Normalise(type))
            {
                case DeliveryTypes.Courier: return "Courier (no route times)";
                case DeliveryTypes.Collect: return "Collections (no route times)";
                default: return "Not one of our drivers (no route times)";
            }
        }

        private static string Enc(string text) => HttpUtility.HtmlEncode(text ?? string.Empty);

        // ---------------- The order they really delivered in ----------------

        private void BindVisits(DateTime? date)
        {
            if (!date.HasValue || !DeliveryVisitRepository.TableSeen())
            {
                gvVisits.DataSource = new List<VisitRow>();
                gvVisits.DataBind();
                return;
            }

            try
            {
                List<DeliveryVisitRepository.Visit> visits = new DeliveryVisitRepository().ListDay(date.Value, SelectedPerson);
                Dictionary<string, int> suggested = SuggestedStops(date.Value);
                Dictionary<string, int> plan = new DeliveryRouteRepository().GetDay(date.Value);
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
            lblMessage.Text = HttpUtility.HtmlEncode(text);
            lblMessage.CssClass = "status-message " + (isError ? "status-error" : "status-success");
            lblMessage.Visible = true;
        }
    }
}
