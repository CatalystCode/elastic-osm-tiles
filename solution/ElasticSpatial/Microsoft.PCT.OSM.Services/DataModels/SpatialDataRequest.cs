
using Microsoft.PCT.OSM.DataModel;

namespace Microsoft.PCT.OSM.Services.DataModels
{
    /// <summary>
    /// Input class for Spatial data service aka. api/pointsofinterest.
    /// </summary>
    public class SpatialDataRequest
    {
        /// <summary>
        /// Gets or sets the current location.
        /// </summary>
        public GeoCoordinate CurrentLocation { get; set; }

        /// <summary>
        /// Gets or sets the search radius
        /// </summary>
        public double? SearchRadius { get; set; }

        /// <summary>
        /// Gets or sets the max results limit.
        /// </summary>
        public int? Limit { get; set; }
    }
}