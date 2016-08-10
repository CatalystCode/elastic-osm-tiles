using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.PCT.Http
{
    /// <summary>
    /// Represents an HTTP request.
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the request stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The stream.</returns>
        Task<Stream> GetRequestStreamAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the response stream.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response.</returns>
        Task<IHttpResponse> GetResponseAsync(CancellationToken cancellationToken);
    }
}
