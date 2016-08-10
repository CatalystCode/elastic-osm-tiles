
namespace Microsoft.PCT.OSM.Exceptions
{
    /// <summary>
    /// Tracks errors occuring during elastic indexing opertaoins(s)
    /// </summary>
    public class IndexErrorGroup
    {
        /// <summary>
        /// Instantiate new error group based on the error message.
        /// </summary>
        /// <param name="errorMessage">error message from the elastic index operation.</param>
        public IndexErrorGroup(string errorMessage)
        {
            ErrorMessage = errorMessage;
            ErrorCount = 1;
        }

        /// <summary>
        /// Increment the error count due to an exception occurence. 
        /// </summary>
        public void ErrorOccured()
        {
            ErrorCount++;
        }

        /// <summary>
        /// Number of error occurences for a certain error type occuring in an indexing operation.
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Error message for an error type occuring in an indexing operation.
        /// </summary>
        public string ErrorMessage { get; set; }

    }
}
