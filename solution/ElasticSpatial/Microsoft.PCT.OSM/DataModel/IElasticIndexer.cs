using Microsoft.PCT.OSM.Exceptions;
using Nest;
using System.Collections.Generic;

namespace Microsoft.PCT.OSM.DataModel
{
    /// <summary>
    /// Defines the parsing indexer to push documents to elastic.
    /// </summary>
    public interface IElasticIndexer
    {
        /// <summary>
        /// Indexes the <see cref="BulkDescriptor"/> doucments to elastic.
        /// </summary>
        /// <param name="documentBatch">The Document batch to index.</param>
        /// <param name="ErrorGroup">Any encountered errors.</param>
        /// <returns></returns>
        bool TryElasticBulkIndex(BulkDescriptor documentBatch, out List<IndexErrorGroup> ErrorGroup);
    }
}
