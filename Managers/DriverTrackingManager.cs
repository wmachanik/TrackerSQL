using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Classes.Geo;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Driver tracking: positions from the driver app (only when tracking is on and inside the work days and hours),
    /// the daily clean-up of old positions, and the Driver tracking page's data (latest positions, stops left, ETAs).
    /// </summary>
    public class DriverTrackingManager
    {
        public const int MaxPointsPerCall = 200;
        /// <summary>Queued positions older than this are not stored (the phone was offline for days).</summary>
        private const int MaxAgeHours = 24;
        /// <summary>Straight-line distance × this ≈ road distance.</summary>
        private const double RoadFactor = 1.3;

        private static readonly object CleanupSync = new object();
        private static DateTime _lastCleanup = DateTime.MinValue;

        public class ReceiveResult
        {
            public int Saved;
            public int Skipped;
            public bool TrackingEnabled;
        }

        /// <summary>Tracking is on for this local time: the right weekday and inside the work hours.</summary>
        public static bool InWindow(GpsSettings s, DateTime local)
        {
            if (s == null)
                return false;
            if (s.TrackingWeekdaysOnly && (local.DayOfWeek == DayOfWeek.Saturday || local.DayOfWeek == DayOfWeek.Sunday))
                return false;
            TimeSpan from = ParseTime(s.WorkHoursStart, new TimeSpan(7, 0, 0));
            TimeSpan to = ParseTime(s.WorkHoursEnd, new TimeSpan(18, 0, 0));
            TimeSpan t = local.TimeOfDay;
            return from <= to ? t >= from && t <= to : t >= from || t <= to;
        }

        public static MobileTrackingSettings ForApp()
        {
            GpsSettings s;
            try
            {
                s = new GpsSettingsRepository().Get();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver tracking settings could not be read: " + ex.Message);
                return new MobileTrackingSettings { Enabled = false, Minutes = 10, WeekdaysOnly = true, From = "07:00", To = "18:00" };
            }
            return new MobileTrackingSettings
            {
                Enabled = s.TrackingEnabled && DriverLocationRepository.TableSeen(),
                Minutes = Math.Max(1, s.PingIntervalMinutes),
                WeekdaysOnly = s.TrackingWeekdaysOnly,
                From = FormatTime(ParseTime(s.WorkHoursStart, new TimeSpan(7, 0, 0))),
                To = FormatTime(ParseTime(s.WorkHoursEnd, new TimeSpan(18, 0, 0)))
            };
        }

        /// <summary>Stores the phone's queued positions. <paramref name="error"/> is set for a bad request.</summary>
        public ReceiveResult Receive(MobileLocationRequest request, MobileTokenUser user, out string error)
        {
            error = null;
            var result = new ReceiveResult();
            if (!DriverLocationRepository.TableSeen())
            {
                error = "Driver tracking is not installed yet (SQLCommands-DriverGps-03.xml).";
                return result;
            }
            if (user?.PersonId == null)
            {
                error = "This sign-in is not linked to a person in People, so its position cannot be stored.";
                return result;
            }

            GpsSettings settings = new GpsSettingsRepository().Get();
            result.TrackingEnabled = settings.TrackingEnabled;
            List<MobileLocationPoint> points = request?.Points ?? new List<MobileLocationPoint>();
            if (!settings.TrackingEnabled)
            {
                result.Skipped = points.Count;
                return result;
            }
            if (points.Count > MaxPointsPerCall)
            {
                result.Skipped += points.Count - MaxPointsPerCall;
                points = points.Skip(points.Count - MaxPointsPerCall).ToList();
            }

            DateTime now = TimeZoneUtils.Now();
            var repo = new DriverLocationRepository();
            foreach (MobileLocationPoint point in points)
            {
                decimal? lat = ValidCoordinate(point?.Lat, 90), lng = ValidCoordinate(point?.Lng, 180);
                DateTime? at = ParseTime(point?.At, now);
                if (!lat.HasValue || !lng.HasValue || (lat.Value == 0 && lng.Value == 0) || !at.HasValue || !InWindow(settings, at.Value))
                {
                    result.Skipped++;
                    continue;
                }
                repo.Insert(new DriverLocationRepository.Ping
                {
                    PersonID = user.PersonId.Value,
                    Latitude = lat.Value,
                    Longitude = lng.Value,
                    AccuracyM = ValidAccuracy(point.Accuracy),
                    PingTime = at.Value,
                    Source = string.Equals(point.Source, DriverLocationRepository.SourceAppOpen, StringComparison.OrdinalIgnoreCase)
                        ? DriverLocationRepository.SourceAppOpen : DriverLocationRepository.SourcePeriodic
                }, user.UserName);
                result.Saved++;
            }

            CleanupIfDue(settings);
            return result;
        }

        /// <summary>The position of a delivery counts as a position too (when tracking is on). Never fails the sync.</summary>
        public static void RecordDelivery(int? personId, decimal? lat, decimal? lng, decimal? accuracy, DateTime at, string userName)
        {
            if (!personId.HasValue || !lat.HasValue || !lng.HasValue)
                return;
            try
            {
                if (!DriverLocationRepository.TableSeen())
                    return;
                GpsSettings settings = new GpsSettingsRepository().Get();
                if (!settings.TrackingEnabled || !InWindow(settings, at))
                    return;
                new DriverLocationRepository().Insert(new DriverLocationRepository.Ping
                {
                    PersonID = personId.Value,
                    Latitude = lat.Value,
                    Longitude = lng.Value,
                    AccuracyM = accuracy,
                    PingTime = at,
                    Source = DriverLocationRepository.SourceDeliveryComplete
                }, userName);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Delivery, "Driver position from delivery not stored: " + ex.Message, userName);
            }
        }

        /// <summary>Deletes positions past the retention period, at most once a day.</summary>
        public static void CleanupIfDue(GpsSettings settings)
        {
            DateTime today = TimeZoneUtils.Now().Date;
            lock (CleanupSync)
            {
                if (_lastCleanup == today)
                    return;
                _lastCleanup = today;
            }
            try
            {
                int days = Math.Max(1, settings?.PingRetentionDays ?? 30);
                int deleted = new DriverLocationRepository().DeleteOlderThan(days);
                if (deleted > 0)
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver tracking: deleted " + deleted + " position(s) older than " + days + " days.");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver tracking clean-up failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Today's drivers: everyone with a position today or with stops for a "delivers" person, their latest position,
        /// status and the stops left in route order with ETAs (straight line × 1.3 at the average speed, plus the service time).
        /// </summary>
        public DriverTrackingBoard Board()
        {
            GpsSettings settings = new GpsSettingsRepository().Get();
            DateTime now = TimeZoneUtils.Now();
            DateTime day = now.Date;
            var board = new DriverTrackingBoard
            {
                Day = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Now = now.ToString("HH:mm", CultureInfo.InvariantCulture),
                TrackingEnabled = settings.TrackingEnabled,
                FreshMinutes = settings.FreshMinutes,
                StaleMinutes = settings.StaleMinutes,
                DepotLat = settings.HasDepotPoint ? (double?)settings.DepotLatitude.Value : null,
                DepotLng = settings.HasDepotPoint ? (double?)settings.DepotLongitude.Value : null
            };

            Dictionary<int, DriverLocationRepository.Ping> pings = DriverLocationRepository.TableSeen()
                ? new DriverLocationRepository().LatestPerPerson(day, now).ToDictionary(p => p.PersonID)
                : new Dictionary<int, DriverLocationRepository.Ping>();
            if (DriverLocationRepository.TableSeen())
                CleanupIfDue(settings);

            List<MobileDeliveryStop> orders = new MobileDeliveryManager().GetSheet(day, null).Orders
                .Where(o => o.DeliveryById.HasValue).ToList();
            Dictionary<int, PersonInfo> people = LoadPeople();
            Dictionary<int, Tuple<double, double>> points = LoadPoints(orders.Select(o => o.ContactId).Distinct());

            var personIds = new HashSet<int>(pings.Keys);
            foreach (int id in orders.Select(o => o.DeliveryById.Value).Distinct())
            {
                if (people.TryGetValue(id, out PersonInfo p) && p.Delivers)
                    personIds.Add(id);
            }

            foreach (int personId in personIds)
            {
                people.TryGetValue(personId, out PersonInfo person);
                var driver = new DriverTrackingDriver
                {
                    PersonId = personId,
                    Name = person?.Name ?? ("Person " + personId),
                    Abbreviation = person?.Abbreviation
                };

                if (pings.TryGetValue(personId, out DriverLocationRepository.Ping ping))
                {
                    driver.Lat = (double)ping.Latitude;
                    driver.Lng = (double)ping.Longitude;
                    driver.Accuracy = ping.AccuracyM.HasValue ? (double?)ping.AccuracyM.Value : null;
                    driver.LastSeen = ping.PingTime.ToString("HH:mm", CultureInfo.InvariantCulture);
                    driver.MinutesAgo = Math.Max(0, (int)Math.Floor((now - ping.PingTime).TotalMinutes));
                    driver.Source = ping.Source;
                    driver.Colour = driver.MinutesAgo <= settings.FreshMinutes ? "green"
                        : driver.MinutesAgo <= settings.StaleMinutes ? "orange" : "red";
                }
                else
                {
                    driver.Colour = "grey";
                }

                driver.Stops = orders.Where(o => o.DeliveryById == personId)
                    .GroupBy(o => o.ContactId)
                    .Select(g => new DriverTrackingStop
                    {
                        Seq = g.Min(o => o.Seq),
                        ContactId = g.Key,
                        Company = g.First().Company,
                        Area = g.First().Area,
                        Orders = g.Count(),
                        Done = g.All(o => o.Done || o.Proof != null),
                        Lat = points.TryGetValue(g.Key, out Tuple<double, double> pt) ? (double?)pt.Item1 : null,
                        Lng = pt?.Item2
                    })
                    .OrderBy(s => s.Seq)
                    .ToList();
                driver.StopsDone = driver.Stops.Count(s => s.Done);
                driver.StopsLeft = driver.Stops.Count - driver.StopsDone;
                driver.StopsNoPoint = driver.Stops.Count(s => !s.Done && !s.Lat.HasValue);

                if (driver.Stops.Count == 0)
                    driver.Status = "No stops";
                else if (driver.StopsLeft == 0)
                    driver.Status = "Finished";
                else if (!driver.MinutesAgo.HasValue || driver.MinutesAgo > settings.StaleMinutes)
                    driver.Status = "Offline";
                else
                    driver.Status = "On route";

                AddEtas(driver, board, settings, now);
                board.Drivers.Add(driver);
            }

            board.Drivers = board.Drivers
                .OrderBy(d => d.Status == "On route" ? 0 : d.Status == "Offline" ? 1 : d.Status == "Finished" ? 2 : 3)
                .ThenBy(d => d.Name)
                .ToList();
            return board;
        }

        private static void AddEtas(DriverTrackingDriver driver, DriverTrackingBoard board, GpsSettings settings, DateTime now)
        {
            double? lat = driver.Lat, lng = driver.Lng;
            driver.FromDepot = !lat.HasValue;
            if (!lat.HasValue)
            {
                lat = board.DepotLat;
                lng = board.DepotLng;
            }
            if (!lat.HasValue || driver.StopsLeft == 0)
                return;

            double speedKmh = Math.Max(5, settings.AverageSpeedKmh);
            DateTime t = now;
            bool any = false;
            foreach (DriverTrackingStop stop in driver.Stops.Where(s => !s.Done && s.Lat.HasValue))
            {
                double km = GeoDistance.Metres(lat.Value, lng.Value, stop.Lat.Value, stop.Lng.Value) / 1000 * RoadFactor;
                t = t.AddMinutes(km / speedKmh * 60);
                stop.Eta = t.ToString("HH:mm", CultureInfo.InvariantCulture);
                t = t.AddMinutes(Math.Max(0, settings.ServiceMinutes));
                lat = stop.Lat;
                lng = stop.Lng;
                any = true;
            }
            if (any)
                driver.FinishEta = t.ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        private class PersonInfo
        {
            public string Name;
            public string Abbreviation;
            public bool Delivers;
        }

        private static Dictionary<int, PersonInfo> LoadPeople()
        {
            var map = new Dictionary<int, PersonInfo>();
            using (var db = new TrackerSQLDb())
            {
                bool hasType = db.ExecuteScalar<int>("SELECT CASE WHEN COL_LENGTH(N'dbo.PeopleTbl', N'DeliveryType') IS NULL THEN 0 ELSE 1 END") == 1;
                using (var r = db.ExecuteReader("SELECT PersonID, Person, Abbreviation"
                    + (hasType ? ", DeliveryType" : ", NULL AS DeliveryType") + " FROM dbo.PeopleTbl"))
                {
                    while (r != null && r.Read())
                    {
                        string type = r["DeliveryType"] == DBNull.Value ? null : r["DeliveryType"].ToString();
                        map[Convert.ToInt32(r["PersonID"])] = new PersonInfo
                        {
                            Name = r["Person"] == DBNull.Value ? null : r["Person"].ToString(),
                            Abbreviation = r["Abbreviation"] == DBNull.Value ? null : r["Abbreviation"].ToString(),
                            Delivers = !hasType || string.Equals(type, "DELIVER", StringComparison.OrdinalIgnoreCase)
                        };
                    }
                }
            }
            return map;
        }

        private static Dictionary<int, Tuple<double, double>> LoadPoints(IEnumerable<int> contactIds)
        {
            var map = new Dictionary<int, Tuple<double, double>>();
            List<int> ids = contactIds.ToList();
            if (ids.Count == 0)
                return map;
            string sql = "SELECT ContactID, Latitude, Longitude FROM dbo.CustomerLocationTbl WHERE ContactID IN ("
                + string.Join(",", ids.Select(i => i.ToString(CultureInfo.InvariantCulture))) + ")";
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql))
            {
                while (r != null && r.Read())
                    map[Convert.ToInt32(r["ContactID"])] = Tuple.Create(Convert.ToDouble(r["Latitude"]), Convert.ToDouble(r["Longitude"]));
            }
            return map;
        }

        private static TimeSpan ParseTime(string text, TimeSpan fallback)
        {
            return TimeSpan.TryParseExact((text ?? string.Empty).Trim(), new[] { @"hh\:mm", @"h\:mm" }, CultureInfo.InvariantCulture, out TimeSpan t)
                ? t : fallback;
        }

        private static string FormatTime(TimeSpan t) => t.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

        /// <summary>The phone's time in South African time; null when missing, in the future or too old.</summary>
        private static DateTime? ParseTime(string value, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(value))
                return now;
            string text = value.Trim();
            bool hasOffset = text.EndsWith("Z", StringComparison.OrdinalIgnoreCase)
                || System.Text.RegularExpressions.Regex.IsMatch(text, @"[+-]\d{2}:?\d{2}$");
            DateTime result;
            if (hasOffset && DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dto))
                result = TimeZoneUtils.ConvertUtcToUserZone(dto.UtcDateTime);
            else if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return null;
            if (result > now.AddMinutes(5) || result < now.AddHours(-MaxAgeHours))
                return null;
            return result;
        }

        private static decimal? ValidCoordinate(decimal? value, int limit)
        {
            if (!value.HasValue || value.Value < -limit || value.Value > limit)
                return null;
            return Math.Round(value.Value, 6);
        }

        private static decimal? ValidAccuracy(decimal? value)
        {
            return value.HasValue && value.Value > 0 && value.Value < 100000 ? Math.Round(value.Value, 1) : (decimal?)null;
        }
    }
}
