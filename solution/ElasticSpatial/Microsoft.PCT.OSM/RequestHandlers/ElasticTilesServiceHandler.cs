
namespace Microsoft.PCT.OSM.RequestHandlers
{
    using ApplicationInsights;
    using DataModel;
    using Exceptions;
    using Http;
    using Http.Fanout;
    using Nest;
    using Reactive;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    
    /// <summary>
    /// The request / response service data processor for OSM Overpass data for Guide Dogs explore. 
    /// </summary>
    public class ElasticTilesServiceHandler : BaseServiceHandler<object, GeoTileVector>
    {
        private readonly IElasticIndexer _Indexer;
        private readonly TelemetryClient _Telemetry;

        private const string TIMESTAMP_FORMAT = "yyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Base contructor for Microsoft.GuideDogs.Shared.Handlers.BaseServiceHandler, which also registers all HTTP endpoints to the provider list
        /// </summary>
        /// <param name="serviceProviders">List of endpoint URI</param>
        /// <param name="requestFactory">Factory class responsible for the request fanout</param>
        public ElasticTilesServiceHandler(IEnumerable<KeyValuePair<string, Uri>> serviceProviders,
                                             IHttpRequestFactory requestFactory, TelemetryClient telemetry)
            : base(serviceProviders, requestFactory)
        {
            _Indexer = new ElasticPlacesIndexer();
            _Telemetry = telemetry;
        }

        /// <summary>
        /// Abstract method for defining how a <see cref="Exceptional"/> with an <see cref="Exception"/> is converted into a <see cref="IServiceResponse"/>.
        /// </summary>
        /// <param name="exception">The raised <see cref="Exception"/>.</param>
        /// <returns></returns>
        protected override IClientResponse ConvertExceptionToResponse(Exception exception)
        {
            var invalidSpatialDataException = exception as InvalidSpatialDataResponseException;
            var metrics = default(ResponseStatus);
            if (invalidSpatialDataException != null && invalidSpatialDataException.Response != null)
            {
                var executionTime = invalidSpatialDataException.ExecutionTimeMilliseconds;
                metrics = executionTime.HasValue
                    ? new ResponseStatus(invalidSpatialDataException.InnerException, executionTime.Value)
                    : new ResponseStatus(invalidSpatialDataException.InnerException);
            }
            else
            {
                metrics = new ResponseStatus(exception);
            }

            return new SpatialDataResponse
            {
                SourceProvider = base.serviceProviders.First().Key,
                ResponseStatus = metrics.ClientResponseStatus
            };
        }

        /// <summary>
        /// App Insight logging.
        /// </summary>
        /// <param name="response">The Spatial Data Contract Response</param>
        /// <param name="deserializedRequest">The deserialized Request.</param>
        /// <param name="requestHeaders">Request headers.</param>
        protected override void Log(Exceptional<GeoTileVector> response, object deserializedRequest, KeyValuePair<string, string>[] requestHeaders)
        {
            if (response.HasValue)
            {
                var location = response.Value;
                var properties = new Dictionary<string, string>
                {
                    { "Mapzen_Tile_Fetch", response.Value.SourceProvider },
                };

                // TODO: most of these are not really metrics
                var metrics = new Dictionary<string, double>
                {
                    { "ExecutionTimeMS", location.ResponseStatus.ExecutionTimeMS },
                    { "Response_POI_FeatureCount", location.pois != null ? location.pois.Features.Count : 0},
                    { "Response_Buildings_FeatureCount", location.buildings != null ? location.buildings.Features.Count : 0},
                    { "Response_Roads_FeatureCount", location.roads != null ? location.roads.Features.Count : 0},
                    { "Response_Places_FeatureCount", location.places != null ? location.places.Features.Count : 0},
                };

                _Telemetry?.TrackEvent("Event.Services.TileCache.Add", properties, metrics);
            }
            else
            {
                _Telemetry?.TrackException(response.Exception);
            }
        }

        /// <summary>
        /// Post Processes the response from OSM into a format acceptable for Guide Dogs consumers.
        /// </summary>
        /// <param name="response">The <see cref="OsmSpatialDataResponse"/> response from OSM.</param>
        /// <param name="deserializedRequest">The client <see cref="SpatialDataRequest"/> request.</param>
        /// <returns></returns>
        protected override IClientResponse PostProcessResponse(GeoTileVector response, object deserializedRequest)
        {
            var postProcessedResponse = new SpatialDataResponse();

            postProcessedResponse.SourceProvider = response.SourceProvider;
            postProcessedResponse.ResponseStatus = response.ResponseStatus.ClientResponseStatus;

            return postProcessedResponse;
        }

        private bool TryElasticBulkIndex(Dictionary<string, Place> osmPlaces, string tileQuadkey, out List<IndexErrorGroup> ErrorGroup)
        {
            var indexDescriptor = new BulkDescriptor();

            foreach (var osmPlace in osmPlaces.Values)
            {
                indexDescriptor.Index<Place>(op => op.Document(osmPlace)
                          .Index("places"));
            }

            //now also cache the geo tiles quadkey into elastic.
            indexDescriptor.Index<Tile>(op => op.Document(new Tile() { QuadKey = tileQuadkey, Id = tileQuadkey, Timestamp = DateTime.Now.ToString(TIMESTAMP_FORMAT) })
                                                    .Index("tiles").Type("geotile"));

            return _Indexer.TryElasticBulkIndex(indexDescriptor, out ErrorGroup);
        }

        /// <summary>
        /// Validates the response coming back from the core vecotr tile service.
        /// </summary>
        /// <param name="response">Spatial Data Response</param>
        /// <returns>The Exceptional</returns>
        protected override Exceptional<GeoTileVector> Validate(Exceptional<GeoTileVector> response)
        {
            var exception = default(Exception);

            if (response.HasValue && response.Exception == null)
            {
                var indexParseTime = Stopwatch.StartNew();
                var errorGroupList = default(List<IndexErrorGroup>);

                var parsedTile = GeoJsonParser.TryParseGeoJsonToPlaceDictionary(response.Value, response.Value.SourceProvider);

                if (!TryElasticBulkIndex(parsedTile, response.Value.SourceProvider, out errorGroupList))
                {
                    exception = new InvalidSpatialDataResponseException("Encountered error(s) attempting to invoke an OSM bulk index operation.", errorGroupList);
                }

                indexParseTime.Stop();

                if (exception != null)
                {
                    var originalMetrics = response.Value.ResponseStatus;
                    response.Value.ResponseStatus = new ResponseStatus(exception, originalMetrics.ExecutionTimeMS + indexParseTime.ElapsedMilliseconds);
                    return exception.ToExceptional<GeoTileVector>();
                }
                else
                {
                    response.Value.ResponseStatus.IncreaseExecutionTimeMS(indexParseTime.ElapsedMilliseconds);
                }
            }
            else if (response.Exception != null)
            {
                exception = new InvalidSpatialDataResponseException("Error occured extracting osm tile details", response.Exception);
                return exception.ToExceptional<GeoTileVector>();
            }

            return response;
        }
    }
}
