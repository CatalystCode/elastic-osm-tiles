
namespace Microsoft.PCT.OSM.Services
{
    using ApplicationInsights;
    using DataModel;
    using Http;
    using Internal;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;    
    
    /// <summary>
    /// Base class bridge that takes ensures any in-scope tiles are indexed and the categories are cached.
    /// </summary>
    public abstract class BaseElasticService<TResponse> : ElasticRequestManager<TResponse> where TResponse : IEnumerable
    {
        const string DEFAULT_AMENITY_TYPE = "Place";
        protected const int DEFAULT_RESULT_LIMIT = 100;
        protected const double DEFAULT_SEARCH_RADIUS = 500;

        /// <summary>
        /// Instantiates the base abstract class.
        /// </summary>
        /// <param name="TileServerBaseUri">The mapzen tile based URI.</param>
        /// <param name="TileKey">The API key for the tile server.</param>
        /// <param name="telemetry">The telemetry client instance.</param>
        /// <param name="requestFactory">Factory class responsible for the request fanout</param>
        protected BaseElasticService(string TileServerBaseUri, string TileKey, TelemetryClient telemetry,
                                     IHttpRequestFactory requestFactory)
        : base(TileServerBaseUri, TileKey, telemetry, requestFactory)
        {
        }
        
        /// <summary>
        /// Responsible for indexing a <see cref="List<GeoTile>" to elastic./>
        /// </summary>
        /// <param name="tiles">The list of <see cref="GeoTile"/> to index Elastic.</param>
        /// <returns></returns>
        public async Task<IEnumerable<SpatialDataResponse>> IndexTilesToElasticAsync(List<GeoTile> tiles)
        {
            if (tiles == null)
            {
                throw new ArgumentNullException(nameof(tiles));
            }
            
            return await base.IndexRequestedTilesToElastic(tiles);
        }

        /// <summary>
        /// Invokes a proximity search in elastic.
        /// </summary>
        /// <param name="userLocation">The users current location.</param>
        /// <param name="SearchRadiusMeters">Search radius in meters.</param>
        /// <returns></returns>
        public override async Task<TResponse> SearchAsync(IGeoCoordinate userLocation, double? SearchRadiusMeters)
        {
            if (userLocation == null)
            {
                throw new ArgumentNullException(nameof(userLocation));
            }

            var radius = SearchRadiusMeters.HasValue ? SearchRadiusMeters.Value : DEFAULT_SEARCH_RADIUS;
            var indexedTiles = await base.IndexMissingTilesToElasticAsync(userLocation, radius);
            var searchStopwatch = Stopwatch.StartNew();

            var results = await QueryPlacesAsync(userLocation, radius);

            LogTelemetryEvent(results, userLocation, searchStopwatch.ElapsedMilliseconds, radius);

            return results;
        }
        
        /// <summary>
        /// Performs a near distance proximity search against the elastic search places index based on a distance, a layer filter and <see cref="IGeoCoordinate"/>.
        /// </summary>
        /// <param name="userLocation">The users current location.</param>
        /// <param name="searchRadiusMeters">Search Radius Dinstance in meters.</param>
        /// <param name="layer">The elastic places index layer filter.</param>
        /// <returns></returns>
        protected async Task<IEnumerable<Place>> InvokeElasticSearchQueryAsync(IGeoCoordinate userLocation, double searchRadiusMeters, string layer = null)
        {
            if (searchRadiusMeters == 0)
            {
                throw new ArgumentNullException(nameof(searchRadiusMeters));
            }

            if (userLocation == null)
            {
                throw new ArgumentNullException(nameof(userLocation));
            }

            return await ElasticManager.Instance.ProximitySearchAsync(searchRadiusMeters,
                                                                                      new GeoCoordinate()
                                                                                      {
                                                                                          Latitude = userLocation.Latitude,
                                                                                          Longitude = userLocation.Longitude
                                                                                      }, layer);
        }
        
        /// <summary>
        /// Log telemetry event to application insights.
        /// </summary>
        /// <param name="places">The list of <see cref="SpatialDataLocationItem"/> results.</param>
        /// <param name="userLocation">The user location lat/long.</param>
        /// <param name="elapsedTime">The elapsed time of the search query.</param>
        /// /// <param name="searchRadius">The search radius in meters.</param>
        protected abstract void LogTelemetryEvent(TResponse places, IGeoCoordinate userLocation, long elapsedTime, double searchRadius);

        /// <summary>
        /// Performs a proximity distance search for all points of interest from the elastic cluster.
        /// </summary>
        /// <param name="userLocation">The users current location.</param>
        /// <param name="SearchRadiusMeters">Search Radius Dinstance in meters.</param>
        /// <returns></returns>
        protected abstract Task<TResponse> QueryPlacesAsync(IGeoCoordinate userLocation, double SearchRadiusMeters);
    }
}
