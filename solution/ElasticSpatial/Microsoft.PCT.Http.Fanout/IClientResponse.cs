
namespace Microsoft.PCT.Http.Fanout
{
    /// <summary>
    /// This interface represents the deserialized response returned to the client
    /// </summary>
    public interface IClientResponse
    {
        /// <summary>
        /// The source provider.
        /// </summary>
        string SourceProvider { get; set; }

        /// <summary>
        /// The status of the response
        /// </summary>
        ClientResponseStatus ResponseStatus { get; set; }
    }
}
