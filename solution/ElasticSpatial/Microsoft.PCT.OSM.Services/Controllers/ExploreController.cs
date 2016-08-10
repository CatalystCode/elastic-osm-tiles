
namespace Microsoft.PCT.OSM.Services.Controllers
{
    using ApplicationInsights.DataContracts;
    using Configuration;
    using DataModel;
    using DataModels;
    using Http;
    using Http.Fanout;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Telemetry;

    /// <summary>
    /// The main <see cref="ApiController"/> for explore / what's around me.
    /// </summary>
    [Authorize]
    public class ExploreController : ApiController
    {
        private const int DEFAULT_TIMEOUT_MS = 15000;

        private IHttpActionResult CheckArguments(SpatialDataRequest request)
        {
            // Check overall request
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            // Check that location is provided
            if (request.CurrentLocation == null)
            {
                return BadRequest("Current location must be provided");
            }

            // Check that latitude and longitude has values
            if (request.CurrentLocation.Latitude == 0 || request.CurrentLocation.Longitude == 0)
            {
                return BadRequest("Latitude and Longitude must be provided");
            }

            // Check CurrentLocation
            if (request.CurrentLocation != null)
            {
                if (request.CurrentLocation.Latitude < -90 || request.CurrentLocation.Latitude > 90)
                {
                    return BadRequest("CurrentLocation latitude must be between or equal to the allowed values of -90 and 90");
                }

                if (request.CurrentLocation.Longitude < -180 || request.CurrentLocation.Longitude > 180)
                {
                    return BadRequest("CurrentLocation longitude must be between or equal to the allowed values of -180 and 180");
                }
            }
            return null;
        }
        
        List<IClientResponse> PostProcessResponse(IEnumerable<Place> elasticPlaces, ClientResponseStatus status)
        {
            var postProcessedResponse = new SpatialDataResponse()
            {
                Results = elasticPlaces
            };

            postProcessedResponse.ReturnedResultCount = postProcessedResponse.Results.Count();
            postProcessedResponse.ResponseStatus = status;
            postProcessedResponse.SourceResultCount = elasticPlaces.Count();

            return new List<IClientResponse>()
            {
                postProcessedResponse
            };
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> PostExploreAsync(SpatialDataRequest request)
        {
            if (Request?.Content == null)
                throw new ArgumentNullException("Expected HTTP request content.");

            IHttpActionResult badArgument = CheckArguments(request);

            var cts = new CancellationTokenSource(DEFAULT_TIMEOUT_MS);

            if (badArgument != null)
            {
                TelemetryInstance.Current.TrackException(new ArgumentException("PostExploreAsync bad request exception"));
                return badArgument;
            }
            
            var requestFactory = new HttpRequestFactory();
            var tileServerBaseUri = ConfigurationHelper.GetSetting("TileServer");
            var tileKey = ConfigurationHelper.GetSetting("TileServerAPIKey");

            var service = new LocationExploreElasticService(tileServerBaseUri, tileKey, TelemetryInstance.Current, requestFactory)
            {
                ResultLimit = request.Limit
            };
            
            var response = default(IEnumerable<Place>);
            var responseStatus = default(ClientResponseStatus);

            try
            {
                var elasticStopWatch = Stopwatch.StartNew();
                response = await service.SearchAsync(request.CurrentLocation, request.SearchRadius);
                elasticStopWatch.Stop();

                responseStatus = new ResponseStatus(HttpStatusCode.OK, elasticStopWatch.ElapsedMilliseconds).ClientResponseStatus;
            }
            catch (Exception ex)
            {
                responseStatus = new ResponseStatus(ex).ClientResponseStatus;
            }

            if (response.Count() == 0)
            {
                var message = "No responses were received.";
                TelemetryInstance.Current.TrackTrace(message, SeverityLevel.Error);

                // TODO: not necessarily a bad request.
                return BadRequest(message);
            }

            try
            {
                return Json(PostProcessResponse(response, responseStatus));
            }
            catch (JsonSerializationException)
            {
                var message = "JsonSerializationException occured when trying to serialize JSON message.";
                TelemetryInstance.Current.TrackTrace(message, SeverityLevel.Error);

                return BadRequest("Unable to Deserialize response from OSM spatial data service.");
            }
        }

    }
}