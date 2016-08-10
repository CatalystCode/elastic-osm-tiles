
namespace Microsoft.PCT.OSM.Exceptions
{
    using DataModel;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Exception type for invalid spatial data responses.
    /// </summary>
    [Serializable]
    public class InvalidSpatialDataResponseException : Exception
    {
        private readonly SpatialDataResponse response;
        private readonly long? executionTimeMilliseconds;
        private readonly List<IndexErrorGroup> errorGroup;

        /// <summary>
        /// Instantiates the <see cref="Microsoft.GuideDogs.SpatialData.InvalidSpatialDataResponseException"/>.
        /// </summary>
        public InvalidSpatialDataResponseException() { }

        /// <summary>
        /// Instantiates the <see cref="Microsoft.GuideDogs.SpatialData.InvalidSpatialDataResponseException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorGroup">The error details occuring during a elastic index operation.</param>
        public InvalidSpatialDataResponseException(string message, List<IndexErrorGroup> errorGroup)
            : base(message)
        {
            this.errorGroup = errorGroup;
        }

        /// <summary>
        /// Instantiates the <see cref="Microsoft.GuideDogs.SpatialData.InvalidSpatialDataResponseException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="response">The invalid response.</param>
        public InvalidSpatialDataResponseException(string message, SpatialDataResponse response)
            : base(message)
        {
            this.response = response;
        }

        /// <summary>
        /// Instantiates the <see cref="Microsoft.GuideDogs.SpatialData.InvalidSpatialDataResponseException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidSpatialDataResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Instantiates the <see cref="Microsoft.GuideDogs.SpatialData.InvalidSpatialDataResponseException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="executionTimeMilliseconds">The execution time.</param>
        public InvalidSpatialDataResponseException(string message, Exception innerException, int executionTimeMilliseconds)
            : base(message, innerException)
        {
            this.executionTimeMilliseconds = executionTimeMilliseconds;
        }

        /// <summary>
        /// Instantiates the exception with serialization information.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context.</param>
        protected InvalidSpatialDataResponseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// The location response.
        /// </summary>
        public SpatialDataResponse Response
        {
            get { return response; }
        }

        /// <summary>
        /// The execution time leading up to the response exception.
        /// </summary>
        public long? ExecutionTimeMilliseconds
        {
            get
            {
                return executionTimeMilliseconds ?? response?.ResponseStatus?.ExecutionTimeMS;
            }
        }

        /// <summary>
        /// Gets the object data for exception serialization.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("LocationResponse", this.Response);
            info.AddValue("ExecutionTimeMs", this.ExecutionTimeMilliseconds);

            base.GetObjectData(info, context);
        }
    }
}
