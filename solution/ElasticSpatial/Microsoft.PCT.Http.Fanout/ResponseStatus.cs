
namespace Microsoft.PCT.Http.Fanout
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    /// <summary>
    /// The response status container.
    /// </summary>
    [DataContract]
    public class ResponseStatus
    {
        private const long invalidExecutionTimeMs = -1L;
        private static readonly HttpStatusCode invalidStatusCode = (HttpStatusCode)(-1);

        private readonly HttpStatusCode? status;
        private readonly Exception exception;
        private long? executionTimeMs;

        /// <summary>
        /// Instantiates a <see cref="ResponseStatus"/> from an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public ResponseStatus(Exception exception)
            : this(exception, null)
        {
        }

        /// <summary>
        /// Instantiates a <see cref="ResponseStatus"/> from an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="executionTimeMilliseconds">The execution time in milliseconds.</param>
        public ResponseStatus(Exception exception, long executionTimeMilliseconds)
            : this(exception, (long?)executionTimeMilliseconds)
        {
        }

        /// <summary>
        /// Instantiates a <see cref="ResponseStatus"/> with the given status.
        /// </summary>
        /// <param name="status">The HTTP status.</param>
        /// <param name="executionTimeMilliseconds">The execution time in milliseconds.</param>
        public ResponseStatus(HttpStatusCode status, long executionTimeMilliseconds)
            : this(status, (long?)executionTimeMilliseconds)
        {
        }

        private ResponseStatus(Exception exception, long? executionTimeMilliseconds)
            : this(ExtractStatusCode(exception), executionTimeMilliseconds)
        {
            // null check in ExtractStatusCode
            this.exception = exception;
        }

        private ResponseStatus(HttpStatusCode? status, long? executionTimeMilliseconds)
        {
            if (executionTimeMilliseconds.HasValue && executionTimeMilliseconds.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(executionTimeMilliseconds));

            this.status = status;
            this.executionTimeMs = executionTimeMilliseconds;
        }

        private ResponseStatus() { /* used only for deserialization */ }

        /// <summary>
        /// Manages the response status code.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return status ?? invalidStatusCode; }
        }

        /// <summary>
        /// The exception type thrown by the service provider.
        /// </summary>
        [DataMember(Name = "ResponseException")]
        public string ResponseException
        {
            get { return exception?.ToString(); }
        }

        /// <summary>
        /// The exception type thrown by the service provider.
        /// </summary>
        [DataMember(Name = "ResponseExceptionType")]
        public Type ResponseExceptionType
        {
            get { return exception?.GetType(); }
        }

        /// <summary>
        /// tracks the request execution time
        /// </summary>
        public long ExecutionTimeMS
        {
            get { return executionTimeMs ?? invalidExecutionTimeMs; }
        }

        /// <summary>
        /// Increment Execution time for response. Used in the event when there are multiple requests for a single fannout.  
        /// </summary>
        public void IncreaseExecutionTimeMS(long value)
        {
            if (executionTimeMs.HasValue)
            {
                executionTimeMs = executionTimeMs.Value + value;
            }
            else
            {
                executionTimeMs = value;
            }
        }

        public ClientResponseStatus ClientResponseStatus
        {
            get
            {
                return new ClientResponseStatus(exception, executionTimeMs.HasValue ? executionTimeMs.Value : 0, StatusCode);
            }
        }

        /// <summary>
        /// Determines on whether the ExecutionTimeMS property should be serialized. 
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeExecutionTimeMS()
        {
            return (ExecutionTimeMS > 0);
        }

        private static HttpStatusCode ExtractStatusCode(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var webException = exception as WebException;
            if (webException != null && webException.Response != null)
            {
                var httpResponse = webException.Response as HttpWebResponse;
                if (httpResponse != null)
                {
                    return httpResponse.StatusCode;
                }
            }

            return HttpStatusCode.InternalServerError;
        }
    }
}
