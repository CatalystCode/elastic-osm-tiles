
namespace Microsoft.PCT.OSM.DataModel
{
    using Http.Fanout;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class represents the response contract for Spatial Data.
    /// </summary>
    [Serializable]
    public class SpatialDataResponse : IClientResponse
    {
        /// <summary>
        /// The spatial data response status. 
        /// </summary>
        public ClientResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// The result count
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int ReturnedResultCount { get; set; }

        /// <summary>
        /// The OSM data source provider.
        /// </summary>
        public string SourceProvider { get; set; }

        /// <summary>
        /// This is the result count from the original service provider.
        /// </summary>
        public int SourceResultCount { get; set; }

        /// <summary>
        /// The spatial data locations.
        /// </summary>
        [DataMember(Name = "results")]
        public IEnumerable<Place> Results { get; set; }
    }
}
