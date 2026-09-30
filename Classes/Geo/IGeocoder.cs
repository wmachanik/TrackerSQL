namespace TrackerSQL.Classes.Geo
{
    /// <summary>Address → GPS point. HeiGIT (openrouteservice Pelias) today; others can be added behind this.</summary>
    public interface IGeocoder
    {
        GeocodeResult Search(string addressText, double? focusLatitude, double? focusLongitude);
    }
}
