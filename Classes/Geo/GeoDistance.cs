using System;

namespace TrackerSQL.Classes.Geo
{
    /// <summary>Straight-line (great-circle) distance between two GPS points.</summary>
    public static class GeoDistance
    {
        private const double EarthRadiusMetres = 6371000;

        public static double Metres(double lat1, double lng1, double lat2, double lng2)
        {
            double dLat = ToRad(lat2 - lat1);
            double dLng = ToRad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return EarthRadiusMetres * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        public static double Metres(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            return Metres((double)lat1, (double)lng1, (double)lat2, (double)lng2);
        }

        private static double ToRad(double degrees) => degrees * Math.PI / 180;
    }
}
