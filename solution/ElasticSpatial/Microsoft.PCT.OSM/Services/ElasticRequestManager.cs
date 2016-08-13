
namespace Microsoft.PCT.OSM.Services
{
    using ApplicationInsights;
    using Http;
    using DataModel;
    using Internal;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using RequestHandlers;
    using System.IO;

    /// <summary>
    /// Manages the indexing of new tile data to Elastic.
    /// </summary>
    public abstract class ElasticRequestManager<TResponse> : IElasticService
        where TResponse : IEnumerable
    {
        ElasticTilesServiceHandler _TileIndexingService;

        string _TileServerBaseUri;
        string _TileServerKey;

        protected readonly TelemetryClient _Telemetry;
        protected readonly IHttpRequestFactory _RequestFactory;

        protected const int TILE_ZOOM_LEVEL = 16;
        const int DEFAULT_TILE_REFRESH_BATCH_SIZE = 40;

        /// <summary>
        /// Instantiates an instance of the Elastic Indexer Manager.
        /// </summary>
        /// <param name="TileServerBaseUri">Mapzen vector tile service base.</param>
        /// <param name="TileKey">Mapzen vector tile service key.</param>
        /// <param name="telemetry">App Insight telemtry instance.</param>
        /// <param name="requestFactory">Factory class responsible for the request fanout.</param>
        protected ElasticRequestManager(string TileServerBaseUri, string TileKey,
                                        TelemetryClient telemetry, IHttpRequestFactory requestFactory)
        {
            _Telemetry = telemetry;
            _TileServerBaseUri = TileServerBaseUri;
            _TileServerKey = TileKey;
            _RequestFactory = requestFactory;
        }

        /// <summary>
        /// The vector tile batch size. This controls the number of async requests to Mapzens 
        /// vector for each request batch.
        /// </summary>
        public int? VectorTileBatchSize { get; set; }

        /// <summary>
        /// Indexes any nearby missing tiles to the elastic cluster. 
        /// </summary>
        /// <param name="userLocation">The users current location.</param>
        /// <param name="SearchRadiusMeters">Search radius meters to query for nearby tiles.</param>
        /// <param name="categories">The classification categories.</param>
        /// <param name="namelessAmenities">The OSM nameless amentity types.</param>
        /// <returns></returns>
        protected async Task<IEnumerable<SpatialDataResponse>> IndexMissingTilesToElasticAsync(IGeoCoordinate userLocation, double SearchRadiusMeters)
        {
            if (userLocation == null)
            {
                throw new ArgumentNullException(nameof(userLocation));
            }

            if (SearchRadiusMeters == 0)
            {
                throw new ArgumentNullException(nameof(SearchRadiusMeters));
            }

            var closestGeoTiles = TileSystem.LatLongToNearbyGeoTiles(TILE_ZOOM_LEVEL, userLocation.Latitude, userLocation.Longitude, SearchRadiusMeters);

            if (closestGeoTiles.Count == 0)
            {
                throw new ArgumentException("No Geo Tiles were able to be retrieved. Either an invalid lat / long or unsupported radius distance was sent.");
            }

            var cachedTiles = await ElasticManager.Instance.CachedTiles(closestGeoTiles);
            var cacheMisses = ElasticManager.TileCacheMisses(cachedTiles, closestGeoTiles);

            return await IndexRequestedTilesToElastic(cacheMisses);
        }

        /// <summary>
        /// Converts a <see cref="List{GeoTile}"/> to a service request list to Mapzens tile server.
        /// </summary>
        /// <param name="tiles">The <see cref="List{GeoTile}"/> to request from Mapzen.</param>
        /// <returns>A <see cref="List<KeyValuePair<string, Uri>>"/> of service call requests</returns>
        List<KeyValuePair<string, Uri>> TileServiceCalls(List<GeoTile> tiles, int zoomLevel)
        {
            var providers = new List<KeyValuePair<string, Uri>>();

            foreach (var tile in tiles)
            {
                var url = string.Format(CultureInfo.InvariantCulture, _TileServerBaseUri, zoomLevel,
                                        tile.X, tile.Y, _TileServerKey);
                providers.Add(new KeyValuePair<string, Uri>(tile.QuadKey, new Uri(url)));
            }

            return providers;
        }

        protected async Task<IEnumerable<SpatialDataResponse>> IndexRequestedTilesToElastic(List<GeoTile> tiles)
        {
            if (tiles == null)
            {
                throw new ArgumentException(nameof(tiles));
            }

            if (_RequestFactory == null)
            {
                throw new ArgumentNullException(nameof(_RequestFactory));
            }

            var serviceCalls = TileServiceCalls(tiles, TILE_ZOOM_LEVEL);
            int timeoutMS = 500000;
            int processedItems = 0;
            var requestBatchSize = VectorTileBatchSize.HasValue ? VectorTileBatchSize.Value : DEFAULT_TILE_REFRESH_BATCH_SIZE;
            var request = new HttpRequestMessage(HttpMethod.Get, default(string)) { Content = new StreamContent(new MemoryStream()) };
            var geoTileList = new List<SpatialDataResponse>();

            if (serviceCalls.Count > 0)
            {

                var cts = new CancellationTokenSource(timeoutMS);
                //batching service call fannouts to avoid flooding mazpen tile server.
                while (processedItems < serviceCalls.Count)
                {
                    var service = new ElasticTilesServiceHandler(serviceCalls.Skip(processedItems).Take(requestBatchSize),
                                          _RequestFactory, _Telemetry);

                    var responses = await service.CallGetServiceRequestAsync(cts.Token, null, new object());

                    geoTileList.AddRange(responses.Select(response => (SpatialDataResponse)response));

                    processedItems += requestBatchSize;

                    //Thread.Sleep(2000);
                }
            }

            return geoTileList;
        }

        async Task<IEnumerable> IElasticService.SearchAsync(IGeoCoordinate userLocation, double? SearchRadiusMeters)
        {
            return await SearchAsync(userLocation, SearchRadiusMeters);
        }

        /// <summary>
        /// Invokes a proximity search in elastic.
        /// </summary>
        /// <param name="userLocation">The users current location.</param>
        /// <param name="SearchRadiusMeters">Search radius in meters.</param>
        /// <returns></returns>
        public abstract Task<TResponse> SearchAsync(IGeoCoordinate userLocation, double? SearchRadiusMeters);
    }
}
