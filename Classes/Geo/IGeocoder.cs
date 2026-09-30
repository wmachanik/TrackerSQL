namespace TrackerSQL.Classes.Geo
{
    /// <summary>Address → GPS point. HeiGIT (openrouteservice Pelias) today; others can be added behind this.</summary>
    public interface IGeocoder
    {
        /// <param name="radiusKm">With a focus point: only accept results within this distance of it.</param>
        GeocodeResult Search(string addressText, double? focusLatitude, double? focusLongitude, double? radiusKm = null);
    }
}
