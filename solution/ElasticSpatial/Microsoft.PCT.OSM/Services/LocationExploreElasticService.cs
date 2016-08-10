
namespace Microsoft.PCT.OSM.Services
{
    using ApplicationInsights;
    using DataModel;
    using Microsoft.PCT.Http;
    using Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class LocationExploreElasticService : BaseElasticService<IEnumerable<Place>>
    {
        private const int SPATIAL_DATA_DEFAULT_RESPONSE_LIMIT = 100;

        /// <summary>
        /// Instantiates the explore service handler.
        /// </summary>
        /// <param name="TileServerBaseUri">Mapzen tile vector service base uri.</param>
        /// <param name="TileKey">Mapzen vector tile API key.</param>
        /// <param name="telemetry">The telemetry client instance.</param>
        /// <param name="requestFactory">Factory class responsible for the request fanout</param>
        public LocationExploreElasticService(string TileServerBaseUri, string TileKey, TelemetryClient telemetry,
                                         IHttpRequestFactory requestFactory)
        : base(TileServerBaseUri, TileKey, telemetry, requestFactory)
        {
        }

        /// <summary>
        /// Specify / Get the maximum number of returned <see cref="SpatialDataLocationItem"/>.
        /// </summary>
        public int? ResultLimit { get; set; }

        /// <summary>
        /// Performs a proximity distance search for all points of interest off the elastic cluster.
        /// </summary>
        /// <param name="userLocation">The users current location.</param>
        /// <param name="searchRadiusMeters">Search Radius Dinstance in meters.</param>
        /// <returns></returns>
        protected override async Task<IEnumerable<Place>> QueryPlacesAsync(IGeoCoordinate userLocation, double searchRadiusMeters)
        {
            if (searchRadiusMeters == 0)
            {
                throw new ArgumentNullException(nameof(searchRadiusMeters));
            }
            if (!ResultLimit.HasValue)
            {
                ResultLimit = SPATIAL_DATA_DEFAULT_RESPONSE_LIMIT;
            }

            var places = await InvokeElasticSearchQueryAsync(userLocation, searchRadiusMeters, null);

            return places.Take(ResultLimit.Value);
        }

        /// <summary>
        /// Log telemetry event to Application Insights.
        /// </summary>
        /// <param name="places">The list of <see cref="SpatialDataLocationItem"/> results.</param>
        /// <param name="userLocation">The user location lat/long.</param>
        /// <param name="elapsedTime">The elapsed time of the search query.</param>
        /// <param name="searchRadius">The search radius in meters.</param>
        protected override void LogTelemetryEvent(IEnumerable<Place> places, IGeoCoordinate userLocation,
                                                  long elapsedTime, double searchRadius)
        {
            var properties = new Dictionary<string, string>
                {
                    { "SourceProvider", "Elastic-Cluster" },
                    { "Request_Location", JsonHelpers.SerializeObjectToJson(userLocation) },
                };

            var metrics = new Dictionary<string, double>
                {
                    { "ExecutionTimeMS", elapsedTime },
                    { "Request_Limit", ResultLimit.Value },
                    { "Request_SearchRadius", searchRadius },
                };
        }
    }
}
