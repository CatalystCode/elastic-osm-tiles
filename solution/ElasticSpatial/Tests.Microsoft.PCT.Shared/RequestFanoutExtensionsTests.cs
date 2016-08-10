using Microsoft.PCT.Http.Fanout;
using Microsoft.PCT.Serialization;
using Microsoft.PCT.TestingUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Microsoft.PCT.Shared
{
    [TestClass]
    public class RequestFanOutExtensionsTests
    {
        [TestMethod]
        public void RequestFanOutExtensions_GetBytesAndValidateTests_Validation_For_BadRequest()
        {
            var requestByteArr = default(byte[]);
            var requestMessage = "{ 'message': 'Invalid Request'}";

            using (var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(requestMessage)))
            {
                AssertEx.Throws<ArgumentException>(() => requestByteArr = RequestFanOutExtensions.GetBytesAndValidate<MockRequest>(requestStream, "application/json", reqObj => ValidateRequest(reqObj)));
            }
        }

        [TestMethod]
        public void RequestFanOutExtensions_GetBytesAndValidateTests_Validation_For_GoodRequest()
        {
            var requestByteArr = default(byte[]);
            var requestMessage = "{ 'message': 'Good Request'}";

            using (var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(requestMessage)))
            {
                requestByteArr = RequestFanOutExtensions.GetBytesAndValidate<MockRequest>(requestStream, "application/json", reqObj => ValidateRequest(reqObj));
                Assert.IsTrue(requestByteArr.Length > 0);
            }
        }

        [TestMethod]
        public void RequestFanOutExtensions_GetBytesAndValidateTests_PreProcess_Request()
        {
            var requestByteArr = default(byte[]);
            var requestMessage = "{ 'message': 'Original Request'}";

            using (var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(requestMessage)))
            {
                requestByteArr = RequestFanOutExtensions.GetBytesAndValidate<MockRequest>(requestStream, "application/json", reqObj => ValidateRequest(reqObj), reqObj => PreProcessRequest(reqObj));

                using (var stream = new MemoryStream(requestByteArr))
                {
                    var deserializedRequest = JsonHelpers.Deserialize<MockRequest>(stream);
                    Assert.IsTrue(deserializedRequest.message == "Changed Request");
                }

            }
        }

        private static void ValidateRequest(MockRequest request)
        {
            if (request.message == "Invalid Request")
            {
                throw new ArgumentException(nameof(request));
            }
        }

        private static void PreProcessRequest(MockRequest request)
        {
            request.message = "Changed Request";
        }
    }

    public class MockRequest
    {
        [DataMember(Name = "message")]
        public string message { set; get; }
    }
}
