using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.PCT.Http
{
    sealed class HttpRequest : IHttpRequest
    {
        private readonly HttpWebRequest _request;

        public HttpRequest(HttpWebRequest request)
        {
            _request = request;
        }

        public Task<Stream> GetRequestStreamAsync(CancellationToken cancellationToken)
        {
            return _request.GetRequestStreamAsync(cancellationToken);
        }

        public async Task<IHttpResponse> GetResponseAsync(CancellationToken cancellationToken)
        {
            var response = (HttpWebResponse)await _request.GetResponseAsync(cancellationToken).ConfigureAwait(false);
            return new HttpResponse(response);
        }
    }
}
