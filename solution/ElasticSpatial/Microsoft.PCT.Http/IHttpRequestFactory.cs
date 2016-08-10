
namespace Microsoft.PCT.Http
{
    using System;
    using System.Collections.Generic;
    using System.Net.Cache;

    /// <summary>
    /// Creates instances of <see cref="I:IHttpRequest"/>.
    /// </summary>
    public interface IHttpRequestFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="I:IHttpRequest"/>.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="method">The method.</param>
        /// <param name="cachePolicy">The cache policy. Can be null.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>An instance of <see cref="I:IHttpRequest"/>.</returns>
        IHttpRequest Create(Uri uri, string method, HttpRequestCachePolicy cachePolicy, params KeyValuePair<string, string>[] headers);
    }
}
