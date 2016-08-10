
namespace Microsoft.PCT.OSM.DataModel
{
    /// <summary>
    /// Main interface representitive of a geo location.
    /// </summary>
    public interface IGeoCoordinate
    {
        /// <summary>
        /// Gets the latitude.
        /// </summary>
        /// <value>
        /// The latitude.
        /// </value>
        double Latitude { get; }

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        /// <value>
        /// The longitude.
        /// </value>
        double Longitude { get; }
        
    }
}
