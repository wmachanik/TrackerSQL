using System.Text;

namespace TrackerSQL.Classes.Geo
{
    /// <summary>Standard base-32 geohash. Nearby points share a prefix, so sorting by it keeps stops close together.</summary>
    public static class Geohash
    {
        private const string Base32 = "0123456789bcdefghjkmnpqrstuvwxyz";

        public static string Encode(double latitude, double longitude, int precision = 9)
        {
            if (precision < 1) precision = 1;
            if (precision > 12) precision = 12;

            double latMin = -90, latMax = 90, lngMin = -180, lngMax = 180;
            var hash = new StringBuilder(precision);
            bool evenBit = true;
            int bit = 0, index = 0;

            while (hash.Length < precision)
            {
                if (evenBit)
                {
                    double mid = (lngMin + lngMax) / 2;
                    if (longitude >= mid) { index = index * 2 + 1; lngMin = mid; }
                    else { index *= 2; lngMax = mid; }
                }
                else
                {
                    double mid = (latMin + latMax) / 2;
                    if (latitude >= mid) { index = index * 2 + 1; latMin = mid; }
                    else { index *= 2; latMax = mid; }
                }
                evenBit = !evenBit;

                if (++bit == 5)
                {
                    hash.Append(Base32[index]);
                    bit = 0;
                    index = 0;
                }
            }
            return hash.ToString();
        }
    }
}
