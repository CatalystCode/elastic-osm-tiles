
namespace Microsoft.PCT.Http
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Cache;

    /// <summary>
    /// Implementation of the factory that creates instances of <see cref="HttpRequestFactory"/>.
    /// </summary>
    public sealed class HttpRequestFactory : IHttpRequestFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="I:IHttpRequest"/>.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="method">The method.</param>
        /// <param name="cachePolicy">The cache policy. Can be null.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>An instance of <see cref="I:IHttpRequest"/>.</returns>
        public IHttpRequest Create(Uri uri, string method, HttpRequestCachePolicy cachePolicy, params KeyValuePair<string, string>[] headers)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var request = WebRequest.CreateHttp(uri);
            request.AllowAutoRedirect = false;
            request.Method = method;

            if (cachePolicy != null)
            {
                request.CachePolicy = cachePolicy;
            }

            if (headers != null)
            {
                request.AddHeaders(headers);
            }

            return new HttpRequest(request);
        }
    }
}
