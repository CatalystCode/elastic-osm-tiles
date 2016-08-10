using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Net;
using Microsoft.PCT.Http.Fanout;
using Microsoft.PCT.TestingUtilities;

namespace Tests.Microsoft.PCT.Shared
{
    [TestClass]
    public class ResponseStatusTests
    {
        [TestMethod]
        public void ResponseStatus_ArgumentChecks()
        {
            AssertEx.Throws<ArgumentNullException>(() => new ResponseStatus(null), ex => Assert.AreEqual("exception", ex.ParamName));
            AssertEx.Throws<ArgumentOutOfRangeException>(() => new ResponseStatus(HttpStatusCode.OK, -1), ex => Assert.AreEqual("executionTimeMilliseconds", ex.ParamName));
        }

        [TestMethod]
        public void ResponseStatus_OkResponse()
        {
            var responseStatus = new ResponseStatus(HttpStatusCode.OK, 0);
            Assert.AreEqual(HttpStatusCode.OK, responseStatus.StatusCode);
            Assert.AreEqual(0, responseStatus.ExecutionTimeMS);
            Assert.IsNull(responseStatus.ResponseException);
        }

        [TestMethod]
        public void ResponseStatus_NonWebException()
        {
            var ex = new Exception();
            var responseStatus = new ResponseStatus(ex);
            Assert.AreEqual(HttpStatusCode.InternalServerError, responseStatus.StatusCode);
            Assert.AreEqual(ex.ToString(), responseStatus.ResponseException);
            Assert.AreEqual(-1L, responseStatus.ExecutionTimeMS);
        }

        [TestMethod]
        public void ResponseStatus_WebException_NullResponse()
        {
            var ex = new WebException("Error.", WebExceptionStatus.ConnectFailure);
            var responseStatus = new ResponseStatus(ex);
            Assert.AreEqual(HttpStatusCode.InternalServerError, responseStatus.StatusCode);
            Assert.AreEqual(ex.ToString(), responseStatus.ResponseException);
        }

        private static bool HaveAdminPrivileges()
        {
            bool isElevated;
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            return isElevated;
        }

        [TestMethod]
        public async Task ResponseStatus_WebException_HttpErrorResponse()
        {
            if (!HaveAdminPrivileges())
                Assert.Inconclusive("Marking as inconclusive as the system user doesnt have admin privileges");

            using (var handler = CreateStatusCodeListener(HttpStatusCode.NotFound, "http://*:61854/"))
            {
                var request = WebRequest.Create("http://localhost:61854/");
                var webException = default(WebException);

                try
                {
                    await request.GetResponseAsync();
                }
                catch (WebException ex)
                {
                    webException = ex;
                }

                Assert.IsNotNull(webException);
                var responseStatus = new ResponseStatus(webException);
                Assert.AreEqual(HttpStatusCode.NotFound, responseStatus.StatusCode);
                Assert.AreEqual(webException.ToString(), responseStatus.ResponseException);
            }
        }

        static TestHttpListener CreateStatusCodeListener(HttpStatusCode statusCode, params string[] prefixes)
        {
            return new TestHttpListener(r => r.StatusCode = (int)statusCode, prefixes);
        }
    }
}
