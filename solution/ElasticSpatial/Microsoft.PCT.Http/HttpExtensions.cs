
namespace Microsoft.PCT.Http
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    static class HttpExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "HTTP headers are complex!")]
        public static void AddHeaders(
            this HttpWebRequest httpWebRequest,
            KeyValuePair<string, string>[] headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            foreach (var header in headers)
            {
                switch (header.Key.ToUpperInvariant())
                {
                    case "ACCEPT":
                        httpWebRequest.Accept = header.Value;
                        break;
                    case "CONNECTION":
                        httpWebRequest.Connection = header.Value;
                        break;
                    case "CONTENT-LENGTH":
                        httpWebRequest.ContentLength = long.Parse(header.Value, CultureInfo.InvariantCulture);
                        break;
                    case "CONTENT-TYPE":
                        httpWebRequest.ContentType = header.Value;
                        break;
                    case "DATE":
                        httpWebRequest.Date = DateTime.Parse(header.Value, CultureInfo.InvariantCulture);
                        break;
                    case "EXPECT":
                        httpWebRequest.Expect = header.Value;
                        break;
                    case "HOST":
                        httpWebRequest.Host = header.Value;
                        break;
                    case "IF-MODIFIED-SINCE":
                        httpWebRequest.IfModifiedSince = DateTime.Parse(header.Value, CultureInfo.InvariantCulture);
                        break;
                    case "RANGE":
                        httpWebRequest.AddRange(long.Parse(header.Value, CultureInfo.InvariantCulture));
                        break;
                    case "REFERER":
                        httpWebRequest.Referer = header.Value;
                        break;
                    case "TRANSFER-ENCODING":
                        httpWebRequest.SendChunked = true;
                        httpWebRequest.TransferEncoding = header.Value;
                        break;
                    case "USER-AGENT":
                        httpWebRequest.UserAgent = header.Value;
                        break;
                    default:
                        httpWebRequest.Headers.Add(header.Key, header.Value);
                        break;
                }
            }
        }

        public static async Task<Stream> GetRequestStreamAsync(this WebRequest request, CancellationToken token)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            using (request.AttachAborter(token))
            {
                return await request.GetRequestStreamAsync().ConfigureAwait(false);
            }
        }

        public static async Task<WebResponse> GetResponseAsync(this WebRequest request, CancellationToken token)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            using (request.AttachAborter(token))
            {
                return await request.GetResponseAsync().ConfigureAwait(false);
            }
        }

        private static CancellationTokenRegistration AttachAborter(this WebRequest request, CancellationToken token)
        {
            return token.Register(
                state =>
                {
                    var webRequest = (WebRequest)state;
                    webRequest.Abort();
                },
                request,
                false);
        }
    }
}
