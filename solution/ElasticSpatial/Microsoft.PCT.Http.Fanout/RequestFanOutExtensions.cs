
namespace Microsoft.PCT.Http.Fanout
{
    using Serialization;
    using System;
    using System.IO;

    /// <summary>
    /// This class is an extension for common functionality for fanout requests
    /// </summary>
    public static class RequestFanOutExtensions
    {
        /// <summary>
        /// Inspects the request stream, ensure that it can be deserialized to the typeparam and returns a byte array to the calling controller
        /// </summary>
        /// <typeparam name="T">The request object that will be getting deserialized.</typeparam>
        /// <param name="requestStream">The stream of request</param>
        /// <param name="requestContentType">The content type of the request</param>
        /// <param name="validationFunc">The action function for validating the deserialized object</param>
        /// <returns>The bytes contained in the stream.</returns>
        public static byte[] GetBytesAndValidate<T>(Stream requestStream, string requestContentType, Action<T> validationAction)
        {
            return GetBytesAndValidate(requestStream, requestContentType, validationAction, _ => { });
        }

        /// <summary>
        /// Inspects the request stream, ensure that it can be deserialized to the typeparam and returns a byte array to the calling controller
        /// </summary>
        /// <typeparam name="T">The request object that will be getting deserialized.</typeparam>
        /// <param name="requestStream">The stream of request</param>
        /// <param name="requestContentType">The content type of the request</param>
        /// <param name="validationFunc">The action function for validating the deserialized object</param>
        /// <param name="preProcessAction">The action function for pre-processing the deserialized object</param>
        /// <returns>The bytes contained in the stream.</returns>
        public static byte[] GetBytesAndValidate<T>(Stream requestStream, string requestContentType, Action<T> validationAction, Action<T> preProcessAction)
        {
            if (validationAction == null)
            {
                throw new ArgumentNullException(nameof(validationAction));
            }

            if (requestStream == null)
            {
                throw new ArgumentNullException(nameof(requestStream));
            }

            if (preProcessAction == null)
            {
                throw new ArgumentNullException(nameof(preProcessAction));
            }

            if (requestContentType == null)
            {
                throw new ArgumentNullException(nameof(requestContentType));
            }

            if (requestContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) == -1)
                throw new ArgumentOutOfRangeException(nameof(requestContentType), "Expecting a POST request in JSON format");

            using (MemoryStream ms = new MemoryStream())
            {
                requestStream.CopyTo(ms);
                ms.Position = 0;

                var deserializedRequest = JsonHelpers.Deserialize<T>(ms);
                if (deserializedRequest == null)
                {
                    throw new InvalidOperationException("Request schema does not follow the expected format");
                }
                validationAction(deserializedRequest);
                preProcessAction(deserializedRequest);
                return JsonHelpers.SerializeBytes(deserializedRequest);
            }
        }
    }
}
