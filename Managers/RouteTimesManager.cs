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
    /// When each stop on a driver's route was delivered, which one is next, and when the rest are expected.
    /// Expected times start only once the driver has left the depot (a position further than <see cref="LeftDepotMetres"/>
    /// from it) or made a delivery, and chain from the latest of the last delivery and the last position:
    /// straight line × 1.3 at the average speed, plus the minutes per stop. The driver app works them out the same way.
    /// </summary>
    public class RouteTimesManager
    {
        public const int LeftDepotMetres = 300;
        private const double RoadFactor = 1.3;
        /// <summary>Driving minutes for a stop with no GPS point when no other leg is known.</summary>
        private const double DefaultLegMinutes = 10;

        public class StopTime
        {
            public int OrderId;
            public int PersonId;
            public int Seq;
            public DateTime? DeliveredAt;
            public bool NotDelivered;
            public DateTime? Expected;
            /// <summary>The stop or the place before it has no GPS point, so the time is a guess.</summary>
            public bool Rough;
            public bool Next;
        }

        public class DriverState
        {
            public int PersonId;
            /// <summary>Left the depot or delivered something today.</summary>
            public bool Started;
            public DateTime? LeftAt;
            public int Left;
        }

        public class DayTimes
        {
            public Dictionary<int, StopTime> Stops = new Dictionary<int, StopTime>();
            public Dictionary<int, DriverState> Drivers = new Dictionary<int, DriverState>();
            public bool Today;
        }

        public static MobileRouteTimesSettings ForApp()
        {
            try
            {
                GpsSettings s = new GpsSettingsRepository().Get();
                return new MobileRouteTimesSettings
                {
                    SpeedKmh = Math.Max(5, s.AverageSpeedKmh),
                    ServiceMinutes = Math.Max(0, s.ServiceMinutes),
                    DepotLat = s.HasDepotPoint ? (double?)s.DepotLatitude.Value : null,
                    DepotLng = s.HasDepotPoint ? (double?)s.DepotLongitude.Value : null,
                    LeftDepotMetres = LeftDepotMetres
                };
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "Route time settings could not be read: " + ex.Message);
                return null;
            }
        }

        /// <summary>Times for every driver's stops on the day (delivered times on any day; next and expected only today).</summary>
        public DayTimes ForDay(DateTime day)
        {
            DateTime now = TimeZoneUtils.Now();
            var result = new DayTimes { Today = day.Date == now.Date };
            GpsSettings settings = new GpsSettingsRepository().Get();
            double? depotLat = settings.HasDepotPoint ? (double?)settings.DepotLatitude.Value : null;
            double? depotLng = settings.HasDepotPoint ? (double?)settings.DepotLongitude.Value : null;

            List<MobileDeliveryStop> stops = new MobileDeliveryManager().GetSheet(day, null).Orders
                .Where(o => o.DeliveryById.HasValue).ToList();
            Dictionary<int, List<DriverLocationRepository.Ping>> pings = result.Today && DriverLocationRepository.TableSeen()
                ? new DriverLocationRepository().ForDay(day).GroupBy(p => p.PersonID).ToDictionary(g => g.Key, g => g.ToList())
                : new Dictionary<int, List<DriverLocationRepository.Ping>>();

            var people = new PersonsRepository();
            foreach (var driverStops in stops.GroupBy(o => o.DeliveryById.Value))
            {
                int personId = driverStops.Key;
                if (!people.IsDeliverPerson(personId))
                    continue;
                List<List<MobileDeliveryStop>> visits = Visits(driverStops.OrderBy(o => o.Seq));
                var state = new DriverState { PersonId = personId };
                result.Drivers[personId] = state;

                DateTime? lastDelivered = null;
                List<MobileDeliveryStop> lastVisit = null;
                foreach (List<MobileDeliveryStop> visit in visits)
                {
                    foreach (MobileDeliveryStop stop in visit)
                    {
                        var time = new StopTime { OrderId = stop.OrderId, PersonId = personId, Seq = stop.Seq };
                        if (stop.Proof != null && DateTime.TryParse(stop.Proof.DeliveredAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime at))
                        {
                            time.DeliveredAt = at;
                            time.NotDelivered = stop.Proof.Outcome == MobileDeliveryManager.OutcomeNotDelivered;
                            if (at.Date == day.Date && stop.Proof.NotAtClient != true && (!lastDelivered.HasValue || at > lastDelivered.Value))
                            {
                                lastDelivered = at;
                                lastVisit = visit;
                            }
                        }
                        result.Stops[stop.OrderId] = time;
                    }
                }

                List<List<MobileDeliveryStop>> open = visits.Where(v => !v.All(Finished)).ToList();
                state.Left = open.Count;
                if (!result.Today || open.Count == 0)
                    continue;
                foreach (MobileDeliveryStop stop in open[0])
                    result.Stops[stop.OrderId].Next = true;

                pings.TryGetValue(personId, out List<DriverLocationRepository.Ping> driverPings);
                DriverLocationRepository.Ping latest = driverPings?.LastOrDefault();
                if (depotLat.HasValue && driverPings != null)
                    state.LeftAt = driverPings.FirstOrDefault(p =>
                        GeoDistance.Metres(depotLat.Value, depotLng.Value, (double)p.Latitude, (double)p.Longitude) > LeftDepotMetres)?.PingTime;
                state.Started = lastDelivered.HasValue || state.LeftAt.HasValue;
                if (!state.Started)
                    continue;

                DateTime t;
                double? lat, lng;
                if (lastDelivered.HasValue && (latest == null || latest.PingTime <= lastDelivered.Value))
                {
                    t = lastDelivered.Value;
                    MobileStopGps gps = lastVisit == null ? null : PointOf(lastVisit);
                    lat = gps?.Lat ?? (latest != null ? (double?)latest.Latitude : depotLat);
                    lng = gps?.Lng ?? (latest != null ? (double?)latest.Longitude : depotLng);
                }
                else
                {
                    t = latest.PingTime;
                    lat = (double)latest.Latitude;
                    lng = (double)latest.Longitude;
                }

                Chain(open, result, t, lat, lng, now, Math.Max(5, settings.AverageSpeedKmh), Math.Max(0, settings.ServiceMinutes),
                    AverageLegMinutes(visits, Math.Max(5, settings.AverageSpeedKmh)));
            }
            return result;
        }

        private static void Chain(List<List<MobileDeliveryStop>> open, DayTimes result, DateTime t, double? lat, double? lng, DateTime now,
            double speedKmh, double serviceMinutes, double averageLeg)
        {
            bool first = true;
            foreach (List<MobileDeliveryStop> visit in open)
            {
                MobileStopGps gps = PointOf(visit);
                bool rough = gps == null || !lat.HasValue;
                double minutes = rough ? averageLeg : LegMinutes(lat.Value, lng.Value, gps.Lat.Value, gps.Lng.Value, speedKmh);
                t = t.AddMinutes(minutes);
                if (first && t < now)
                    t = now;
                first = false;
                foreach (MobileDeliveryStop stop in visit)
                {
                    StopTime time = result.Stops[stop.OrderId];
                    time.Expected = t;
                    time.Rough = rough;
                }
                t = t.AddMinutes(serviceMinutes);
                if (gps != null)
                {
                    lat = gps.Lat;
                    lng = gps.Lng;
                }
            }
        }

        private static double LegMinutes(double lat1, double lng1, double lat2, double lng2, double speedKmh)
        {
            return GeoDistance.Metres(lat1, lng1, lat2, lng2) / 1000 * RoadFactor / speedKmh * 60;
        }

        /// <summary>The average driving time between neighbouring stops that both have a point; used for stops without one.</summary>
        private static double AverageLegMinutes(List<List<MobileDeliveryStop>> visits, double speedKmh)
        {
            var legs = new List<double>();
            MobileStopGps previous = null;
            foreach (List<MobileDeliveryStop> visit in visits)
            {
                MobileStopGps gps = PointOf(visit);
                if (gps != null && previous != null)
                    legs.Add(LegMinutes(previous.Lat.Value, previous.Lng.Value, gps.Lat.Value, gps.Lng.Value, speedKmh));
                previous = gps;
            }
            return legs.Count > 0 ? legs.Average() : DefaultLegMinutes;
        }

        private static MobileStopGps PointOf(IEnumerable<MobileDeliveryStop> visit) =>
            visit.Select(s => s.Gps).FirstOrDefault(g => g?.Lat != null && g.Lng.HasValue);

        private static bool Finished(MobileDeliveryStop stop) => stop.Done || stop.Proof != null;

        /// <summary>Orders next to each other for the same client are one stop.</summary>
        private static List<List<MobileDeliveryStop>> Visits(IEnumerable<MobileDeliveryStop> route)
        {
            var visits = new List<List<MobileDeliveryStop>>();
            foreach (MobileDeliveryStop stop in route)
            {
                if (visits.Count > 0 && visits[visits.Count - 1][0].ContactId == stop.ContactId
                    && stop.ContactId != SystemConstants.CustomerConstants.SundryCustomerID)
                    visits[visits.Count - 1].Add(stop);
                else
                    visits.Add(new List<MobileDeliveryStop> { stop });
            }
            return visits;
        }
    }
}
