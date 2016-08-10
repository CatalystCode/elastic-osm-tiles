
namespace Microsoft.PCT.OSM.DataModel
{

    /// <summary>
    /// The interface for the tiles index in elasticsearch.
    /// </summary>
    public interface ITile
    {
        string QuadKey { get; set; }

        string Id { get; set; }
    }
}
