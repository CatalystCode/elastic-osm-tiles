using Microsoft.PCT.OSM.DataModel;
using Microsoft.PCT.OSM.Exceptions;
using Microsoft.PCT.OSM.Internal;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.PCT.OSM
{
    /// <summary>
    /// Responsible for indexing a batch of documents to the elastic cluster.
    /// </summary>
    public class ElasticPlacesIndexer : IElasticIndexer
    {
        public ElasticPlacesIndexer() { }

        /// <summary>
        /// Indexes the <see cref="BulkDescriptor"/> doucments to elastic.
        /// </summary>
        /// <param name="documentBatch">The Document batch to index.</param>
        /// <param name="ErrorGroup">Any encountered errors.</param>
        /// <returns></returns>
        public bool TryElasticBulkIndex(BulkDescriptor documentBatch, out List<IndexErrorGroup> ErrorGroup)
        {
            ErrorGroup = new List<IndexErrorGroup>();

            var results = ElasticManager.Instance.BulkOperation(documentBatch);

            Console.WriteLine("Document Indexing took: " + results.Took);

            if (results.ItemsWithErrors.Count() > 0)
            {
                ErrorGroup = CollectIndexingErrors(results.ItemsWithErrors);
                return false;
            }

            return true;
        }

        private List<IndexErrorGroup> CollectIndexingErrors(IEnumerable<BulkResponseItemBase> errors)
        {
            var errorDictionary = new Dictionary<string, IndexErrorGroup>();
            foreach (var error in errors)
            {
                var errorGroup = default(IndexErrorGroup);

                if (errorDictionary.TryGetValue(error.Error.Reason, out errorGroup))
                {
                    errorGroup.ErrorOccured();
                }
                else
                {
                    errorDictionary.Add(error.Error.Reason, new IndexErrorGroup(error.Error.Reason));
                }
            }

            return new List<IndexErrorGroup>(errorDictionary.Values);
        }
    }
}
