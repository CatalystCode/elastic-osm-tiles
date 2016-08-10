using System;
using System.Collections;
using System.Threading.Tasks;

namespace Microsoft.PCT.OSM.DataModel
{
    /// <summary>
    /// The core interface for an Elastic service handler.
    /// </summary>
    public interface IElasticService
    {
        /// <summary>
        /// Queries the Elastic Places Index based on the radius set in the constructor and the users <see cref="IGeoCoordinate"/> location.
        /// </summary>
        /// <param name="userLocation">The users location coordinates.</param>
        /// <returns></returns>
        Task<IEnumerable> SearchAsync(IGeoCoordinate userLocation, double? SearchRadiusMeters);
    }
}
