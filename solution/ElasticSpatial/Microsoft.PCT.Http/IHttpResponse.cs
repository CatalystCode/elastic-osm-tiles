
namespace Microsoft.PCT.Http
{
    using System;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Represents an HTTP response.
    /// </summary>
    public interface IHttpResponse : IDisposable
    {
        /// <summary>
        /// Gets the headers that are associated with this response from the server.
        /// </summary>
        WebHeaderCollection Headers { get; }

        /// <summary>
        /// HTTP status code.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets response stream.
        /// </summary>
        /// <returns>Response stream.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling the method two times in succession may create different results.")]
        Stream GetResponseStream();
    }
}
