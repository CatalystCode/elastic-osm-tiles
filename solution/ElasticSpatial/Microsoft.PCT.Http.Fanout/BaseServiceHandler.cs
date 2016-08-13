
namespace Microsoft.PCT.Http.Fanout
{
    using Http;
    using Serialization;
    using Reactive;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;

    /// <summary>
    /// A base class for fanning out requests.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public abstract class BaseServiceHandler<TRequest, TResponse>
        where TResponse : class, IServiceResponse
    {
        private static string DefaultContentType = "application/json";

        protected readonly IEnumerable<KeyValuePair<string, Uri>> serviceProviders;
        protected readonly IHttpRequestFactory requestFactory;
        private const string SessionIdHeaderKey = "X-SessionId";
        private const string ClientIdHeaderKey = "X-ClientId";
        private const string SessionIAppInsightPropName = "SessionIdentifier";
        private const string CientIAppInsightPropName = "ClientIdentifier";
        private const string ContentTypeHeaderKey = "Content-Type";
        private readonly Dictionary<string, string> RelevantHeaderMapping = new Dictionary<string, string> { { SessionIdHeaderKey, SessionIAppInsightPropName }, { ClientIdHeaderKey, CientIAppInsightPropName } };

        /// <summary>
        /// Base contructor for BaseServiceHandler, which also registers all HTTP endpoints to the provider list
        /// </summary>
        /// <param name="serviceProviders">List of endpoint URI</param>
        /// <param name="requestFactory">Factory class responsible for the request fanout</param>
        protected BaseServiceHandler(IEnumerable<KeyValuePair<string, Uri>> serviceProviders, IHttpRequestFactory requestFactory)
        {
            if (serviceProviders == null)
                throw new ArgumentNullException(nameof(serviceProviders));
            if (requestFactory == null)
                throw new ArgumentNullException(nameof(requestFactory));

            this.serviceProviders = serviceProviders;
            this.requestFactory = requestFactory;
        }

        /// <summary>
        /// Gets the headers to use for this request.
        /// </summary>
        /// <param name="requestHeaders">The enumeration of client request headers.</param>
        /// <returns></returns>
        protected virtual KeyValuePair<string, string>[] ParseRelevantHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders)
        {
            if (requestHeaders == null)
                return null;

            var headers = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(String.Format("Request_Header_{0}", ContentTypeHeaderKey), DefaultContentType)
            };

            var headerDictionary = requestHeaders.ToDictionary(v => v.Key, v => v.Value.First());

            //Map the client header name to it's corresponding mapping, which is very important for app insight telemetry.
            foreach (var headerKey in RelevantHeaderMapping)
            {
                var heaverValue = default(string);
                if (headerDictionary.TryGetValue(headerKey.Key, out heaverValue))
                {
                    headers.Add(new KeyValuePair<string, string>(headerKey.Value, heaverValue));
                }
            }

            return headers.ToArray();
        }

        /// <summary>
        /// Fans out the request to the location providers.
        /// </summary>
        /// <param name="request">The request body.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The list of responses.</returns>
        /// <param name="requestHeaders">The enumeration of client request headers.</param>
        /// <param name="deserializedRequest">The deserialized Request.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics are hard to avoid with asynchronous behavior")]
        public async Task<IList<Exceptional<TResponse>>> FanOutProviderRequestTasksAsync(byte[] request, CancellationToken token,
                                                                       IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders,
                                                                       TRequest deserializedRequest)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var headers = ParseRelevantHeaders(requestHeaders);
            var tileRequests = serviceProviders.Select(provider => SendPostRequestAsync(provider, request, token, headers));

            //Continous check if the response queue has been fully processed until the time to live has been triggered
            return await tileRequests
                .ToObservable(token)
                .Where(tileResponse => !tileResponse.HasValue || tileResponse.Value != null)
                .Select(Validate)
                .Do(tileResponse => Log(tileResponse, deserializedRequest, headers))
                .ToList();
        }

        /// <summary>
        /// Sends a GET request to a given service provider.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The response.</returns>
        /// <param name="requestHeaders">The enumeration of client request headers.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Nested generics are hard to avoid with asynchronous behavior")]
        public virtual async Task<IList<IClientResponse>> CallGetServiceRequestAsync(CancellationToken token,
                                                                       IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders,
                                                                       TRequest deserializedRequest)
        {
            var headers = ParseRelevantHeaders(requestHeaders);
            var taskList = serviceProviders.Select(provider => SendGetRequestAsync<TResponse>(provider, token, headers));

            //Continous check if the response queue has been fully processed until the time to live has been triggered
            return await taskList
                .ToObservable(token)
                .Where(response => !response.HasValue || response.Value != null)
                .Select(Validate)
                .Do(response => Log(response, deserializedRequest, headers))
                .Select(response => PrepareClientResponse(response, deserializedRequest))
                .ToList();
        }

        protected IClientResponse PrepareClientResponse(Exceptional<TResponse> response, TRequest deserializedRequest)
        {
            return response.HasValue
                        ? PostProcessResponse(response.Value, deserializedRequest)
                        : ConvertExceptionToResponse(response.Exception);
        }

        /// <summary>
        /// Abstract method for defining how a <see cref="Exceptional"/> with an <see cref="Exception"/> is converted into a <see cref="IServiceResponse"/>.
        /// </summary>
        /// <param name="exception">The raised <see cref="Exception"/>.</param>
        /// <returns></returns>
        protected abstract IClientResponse ConvertExceptionToResponse(Exception exception);

        /// <summary>
        /// Validates the response and throws an exception for failed conditional checks
        /// </summary>
        /// <param name="response">The response from the HTTP service</param>
        /// <returns>The validated response</returns>
        protected abstract Exceptional<TResponse> Validate(Exceptional<TResponse> response);

        /// <summary>
        /// Perform logging on the response.
        /// </summary>
        /// <param name="response">The response from the HTTP service</param>
        /// <param name="deserializedRequest">The deserialized Request.</param>
        /// <param name="requestHeaders">Request headers.</param>
        protected abstract void Log(Exceptional<TResponse> response, TRequest deserializedRequest, KeyValuePair<string, string>[] requestHeaders);


        /// <summary>
        /// Perform logging on the response.
        /// </summary>
        /// <param name="response">The response from the HTTP service</param>
        /// <param name="deserializedRequest">The deserialized Request.</param>
        /// <param name="requestHeaders">Request headers.</param>
        protected abstract IClientResponse PostProcessResponse(TResponse response, TRequest deserializedRequest);

        /// <summary>
        /// Makes a async GET request to a given service provider
        /// </summary>
        /// <param name="serviceProvider">The list of service provider uris</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="requestHeaders">The enumeration of client request headers</param>
        /// <returns></returns>
        protected async Task<T> SendGetRequestAsync<T>(KeyValuePair<string, Uri> serviceProvider, CancellationToken token,
                                            KeyValuePair<string, string>[] requestHeaders) where T : class, IServiceResponse
        {
            var stopwatch = Stopwatch.StartNew();
            var httpRequest = requestFactory.Create(serviceProvider.Value, "GET", null, requestHeaders);

            var result = default(T);
            using (var response = await httpRequest.GetResponseAsync(token))
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return null;
                }

                try
                {
                    result = JsonHelpers.Deserialize<T>(response.GetResponseStream());
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            if (result == null)
            {
                throw new InvalidOperationException("Invalid Web response. The location response is null.");
            }

            result.SourceProvider = serviceProvider.Key;
            result.ResponseStatus = new ResponseStatus(HttpStatusCode.OK, stopwatch.ElapsedMilliseconds);
            return result;
        }

        /// <summary>
        /// Makes a async POST request to a given service provider
        /// </summary>
        /// <param name="serviceProvider">The list of service providers to make a fanout request</param>
        /// <param name="postRequest">The request body</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="requestHeaders">The enumeration of client request headers</param>
        /// <returns></returns>
        private async Task<TResponse> SendPostRequestAsync(KeyValuePair<string, Uri> serviceProvider, byte[] postRequest, CancellationToken token,
                                            KeyValuePair<string, string>[] requestHeaders)
        {
            var stopwatch = Stopwatch.StartNew();
            var httpRequest = requestFactory.Create(serviceProvider.Value, "POST", null, requestHeaders);
            using (var requestStream = await httpRequest.GetRequestStreamAsync(token))
            {
                await requestStream.WriteAsync(postRequest, 0, postRequest.Length);
            }

            var result = default(TResponse);
            using (var response = await httpRequest.GetResponseAsync(token))
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return null;
                }

                result = JsonHelpers.Deserialize<TResponse>(response.GetResponseStream());
            }

            if (result == null)
            {
                throw new InvalidOperationException("Invalid Web response. The location response is null.");
            }

            result.SourceProvider = serviceProvider.Key;
            result.ResponseStatus = new ResponseStatus(HttpStatusCode.OK, stopwatch.ElapsedMilliseconds);
            return result;
        }
    }
}
