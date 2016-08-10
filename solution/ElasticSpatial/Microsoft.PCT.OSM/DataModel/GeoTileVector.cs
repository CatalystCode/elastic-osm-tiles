
namespace Microsoft.PCT.OSM.DataModel
{
    using GeoJSON.Net.Feature;
    using Microsoft.PCT.Http.Fanout;
    using System.Runtime.Serialization;

    /// <summary>
    /// The response data model for vector geo tiles from Mapzen.
    /// </summary>
    [DataContract]
    public class GeoTileVector : IServiceResponse
    {
        /// <summary>
        /// This should be Overpass, for now
        /// </summary>
        public string SourceProvider { get; set; }

        /// <summary>
        /// The spatial data response status. 
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all boundaries related nodes in OSM.
        /// </summary>
        [DataMember(Name = "boundaries")]
        public FeatureCollection boundaries { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all earth related nodes in OSM.
        /// </summary>
        [DataMember(Name = "earth")]
        public FeatureCollection earth { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all roads related nodes in OSM.
        /// </summary>
        [DataMember(Name = "roads")]
        public FeatureCollection roads { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all transit related nodes in OSM.
        /// </summary>
        [DataMember(Name = "transit")]
        public FeatureCollection transit { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all water related nodes in OSM.
        /// </summary>
        [DataMember(Name = "water")]
        public FeatureCollection water { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all places related nodes in OSM.
        /// </summary>
        [DataMember(Name = "places")]
        public FeatureCollection places { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all landuse related nodes in OSM.
        /// </summary>
        [DataMember(Name = "landuse")]
        public FeatureCollection landuse { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all way objects in OSM.
        /// </summary>
        [DataMember(Name = "buildings")]
        public FeatureCollection buildings { get; set; }

        /// <summary>
        /// The <see cref="FeatureCollection"/> for all point of interest related nodes in OSM.
        /// </summary>
        [DataMember(Name = "pois")]
        public FeatureCollection pois { get; set; }
    }
}
