
namespace Microsoft.PCT.OSM.DataModel
{
    /// <summary>
    /// A container for geocoordinate data.
    /// </summary>
    public class GeoCoordinate : IGeoCoordinate
    {
        /// <summary>
        /// Main contructor for GeoCoordinate with no arguments.
        /// </summary>
        public GeoCoordinate()
        {
        }

        /// <summary>
        /// Contructor for GeoCoordinate with latitude and longitude arguments.
        /// </summary>
        public GeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        public double Latitude
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        public double Longitude
        {
            get;
            set;
        }
        
    }
}
