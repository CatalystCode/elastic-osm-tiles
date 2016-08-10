
namespace Microsoft.PCT.OSM.DataModel
{
    /// <summary>
    /// The datamodel class representing a document item for the tiles elastic index.
    /// </summary>
    public class Tile : ITile
    {
        /// <summary>
        /// The unique quadkey unique identifier for a <see cref="GeoTile"/>.
        /// </summary>
        public string QuadKey { get; set; }

        /// <summary>
        /// The unique quadkey unique identifier for a <see cref="GeoTile"/>.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The time when the tiles elastic index document was last updated. 
        /// </summary>
        public string Timestamp { get; set; }
    }
}
