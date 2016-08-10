
namespace Microsoft.PCT.OSM.DataModel
{
    /// <summary>
    /// The data model including the tile x, y and quadkey.
    /// </summary>
    public class GeoTile : ITile
    {
        /// <summary>
        /// The X position of a tile.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The unique elastic search document identifer.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Y position of a tile.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Unique quadkey identifier for a given <see cref="GeoTile"/>.
        /// </summary>
        public string QuadKey { get; set; }

        /// <summary>
        /// Tile zoom level.
        /// </summary>
        public int ZoomLevel { get; set; }
    }
}
