
namespace Microsoft.PCT.Telemetry
{
    using ApplicationInsights;
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Web;

    /// <summary>
    /// Helpers to get a unique telemetry client per request.
    /// </summary>
    public static class TelemetryInstance
    {
        private const string SessionIdHeaderKey = "X-SessionId";
        private const string ClientIdHeaderKey = "X-ClientId";

        private static readonly string s_contextId = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the telemetry client for the current request.
        /// </summary>
        public static TelemetryClient Current
        {
            get
            {
                var contextObject = CallContext.GetData(s_contextId);

                //
                // This will not guarantee a singleton instance of the
                // telemetry client per request, but will guarantee that each
                // request has a unique telemetry client. If we wish to
                // guarantee a singleton instance, it will require a static 
                // lock, which will not be very performant. 
                // 
                if (contextObject == null)
                {
                    contextObject = Create(HttpContext.Current);
                    CallContext.SetData(s_contextId, contextObject);
                }

                var context = contextObject as TelemetryClient;
                if (context == null)
                {
                    throw new InvalidOperationException("Invalid telemetry client in call context.");
                }

                return context;
            }
        }

        internal static TelemetryClient Create(HttpContext httpContext)
        {
            var client = new TelemetryClient();
            var context = client.Context;

            var currentRequest = httpContext?.Request;
            if (currentRequest != null)
            {
                context.Session.Id = currentRequest.Headers?.Get(SessionIdHeaderKey);
                context.User.Id = currentRequest.Headers?.Get(ClientIdHeaderKey);
            }

            return client;
        }
    }
}
