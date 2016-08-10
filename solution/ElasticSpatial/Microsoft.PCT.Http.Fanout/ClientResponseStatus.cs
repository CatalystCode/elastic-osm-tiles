
namespace Microsoft.PCT.Http.Fanout
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// The response status container.
    /// </summary>
    [DataContract]
    public class ClientResponseStatus
    {
        private static readonly HttpStatusCode invalidStatusCode = (HttpStatusCode)(-1);
        private int _StatusCode;

        public ClientResponseStatus() { }

        public ClientResponseStatus(Exception exception)
        {
            this.ResponseExceptionMessage = exception?.Message;
            this.ResponseException = exception?.GetType().ToString();
        }

        public ClientResponseStatus(Exception exception, long executionTime, HttpStatusCode status) : this(exception)
        {
            this.ExecutionTimeMS = executionTime;
            this.StatusCode = (int)status;
        }

        /// <summary>
        /// tracks the request execution time
        /// </summary>
        [DataMember(Name = "ExecutionTimeMS")]
        public long ExecutionTimeMS { get; set; }

        /// <summary>
        /// The exception type thrown by the service provider.
        /// </summary>
        [DataMember(Name = "ResponseExceptionMessage")]
        public string ResponseExceptionMessage { get; set; }

        /// <summary>
        /// The exception type thrown by the service provider.
        /// </summary>
        [DataMember(Name = "ResponseException")]
        public string ResponseException { get; set; }

        /// <summary>
        /// The <see cref="HttpStatusCode"/> for the returned response.
        /// </summary>
        [DataMember(Name = "StatusCode")]
        public int StatusCode
        {
            get { return _StatusCode != 0 ? _StatusCode : (int)invalidStatusCode; }
            set { _StatusCode = value; }
        }
    }
}
