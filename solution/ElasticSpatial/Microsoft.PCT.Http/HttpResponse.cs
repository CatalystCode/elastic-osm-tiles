
namespace Microsoft.PCT.Http
{
    using System.IO;
    using System.Net;

    sealed class HttpResponse : IHttpResponse
    {
        private readonly HttpWebResponse _response;

        public HttpResponse(HttpWebResponse response)
        {
            _response = response;
        }

        public WebHeaderCollection Headers
        {
            get { return _response.Headers; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _response.StatusCode; }
        }

        public Stream GetResponseStream()
        {
            return _response.GetResponseStream();
        }

        public void Dispose()
        {
            _response.Dispose();
        }
    }
}
