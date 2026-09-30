namespace TrackerSQL.Classes.Geo
{
    public enum GeocodeFailure
    {
        None,
        NotConfigured,
        NotFound,
        DailyLimitReached,
        PerMinuteLimitReached,
        Error
    }

    /// <summary>One geocoder answer. <see cref="Message"/> never contains the API key.</summary>
    public class GeocodeResult
    {
        public bool Success => Failure == GeocodeFailure.None;
        public GeocodeFailure Failure { get; set; }
        public string Message { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        /// <summary>The place the geocoder matched, e.g. "12 Main Road, Constantia, Cape Town".</summary>
        public string Label { get; set; }
        /// <summary>Pelias layer: address, venue, street, neighbourhood, locality, postalcode, ...</summary>
        public string Layer { get; set; }
        /// <summary>0 to 1.</summary>
        public double? Confidence { get; set; }
        /// <summary>GpsSettings.ProviderHeigit or ProviderAzureMaps (Azure Maps points expire).</summary>
        public string Provider { get; set; }

        public static GeocodeResult Fail(GeocodeFailure failure, string message)
        {
            return new GeocodeResult { Failure = failure, Message = message };
        }

        /// <summary>Province / country level matches: too rough to deliver to or to group stops by.</summary>
        public static bool IsCoarseLayer(string layer)
        {
            switch ((layer ?? string.Empty).ToLowerInvariant())
            {
                case "country":
                case "dependency":
                case "macroregion":
                case "region":
                case "macrocounty":
                case "county":
                case "continent":
                case "empire":
                case "ocean":
                case "marinearea":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>True when the match is a real address or place, not just a suburb / town / postcode.</summary>
        public bool IsPrecise
        {
            get
            {
                string layer = (Layer ?? string.Empty).ToLowerInvariant();
                return (layer == "address" || layer == "venue") && (!Confidence.HasValue || Confidence.Value >= 0.8);
            }
        }
    }
}
