using Elasticsearch.Net;
using Microsoft.PCT.Configuration;
using Microsoft.PCT.OSM.DataModel;
using Nest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.PCT.OSM
{
    public sealed class ElasticManager
    {
        static volatile ElasticManager _instance;
        static object syncRoot = new object();

        public const string TILES_INDEX = "tiles";
        public const string TILES_TYPE = "geotile";
        public const string PLACES_INDEX = "places";
        public const string PLACES_TYPE = "place";

        const string ELASTIC_SERVER_HOST = "ElasticCacheServerHost";
        const string ELASTIC_SERVER_PORT = "ElasticCachePort";
        const string ELASTIC_DEFAULT_INDEX = "ElasticDefaultIndex";
        const string ELASTIC_USERNAME = "ElasticUsername";
        const string ELASTIC_PWD = "ElasticPwd";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ElasticManager()
        {
        }

        ElasticManager(bool useConnectionPool)
        {
            if (!useConnectionPool)
            {
                Connection = SingleNodeConnection();
            }
            else
            {
                Connection = UseConnectionPool();
            }
        }

        ElasticClient UseConnectionPool()
        {
            var pool = new SingleNodeConnectionPool(ElasticClusterUri);
            return new ElasticClient(new ConnectionSettings(pool)
                                    .DefaultIndex(ConfigurationHelper.GetSetting(ELASTIC_DEFAULT_INDEX))
                                    .BasicAuthentication(ConfigurationHelper.GetSetting(ELASTIC_USERNAME), ConfigurationHelper.GetSetting(ELASTIC_PWD)));
        }

        ElasticClient SingleNodeConnection()
        {
            var pool = new SingleNodeConnectionPool(ElasticClusterUri);
            var connectionSettings = new ConnectionSettings(pool)
                .DefaultIndex(ConfigurationHelper.GetSetting(ELASTIC_DEFAULT_INDEX));

            return new ElasticClient(connectionSettings);
        }

        Uri ElasticClusterUri
        {
            get
            {
                var hostname = ConfigurationHelper.GetSetting(ELASTIC_SERVER_HOST);
                var port = ConfigurationHelper.GetSetting(ELASTIC_SERVER_PORT);

                if (hostname == null)
                    throw new ArgumentNullException(nameof(hostname));

                if (port == null)
                    throw new ArgumentNullException(nameof(port));

                return new Uri(string.Format(CultureInfo.InvariantCulture, "http://{0}:{1}", hostname, port,
                                                 ConfigurationHelper.GetSetting(ELASTIC_SERVER_PORT)));
            }
        }

        public static ElasticManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new ElasticManager(true);
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the active <see cref="ElasticClient"/> connection to the elastic host.
        /// </summary>
        public ElasticClient Connection
        {
            get; private set;
        }

        /// <summary>
        /// Determines which nearby <see cref="List{GeoTile}"/> s have not been cached in elastic.
        /// </summary>
        /// <param name="elasticTileDocuments">The <see cref="ITile"/>s that have been cached in elastic.</param>
        /// <param name="nearbyTiles">The closest <see cref="GeoTile"/>s based on the users current location.</param>
        /// <returns>The <see cref="List{GeoTile}"/> that need to be cached to elastic.</returns>
        public static List<GeoTile> TileCacheMisses(List<Tile> elasticTileDocuments, List<GeoTile> nearbyTiles)
        {
            var tileHashSet = new HashSet<string>(elasticTileDocuments.Select(tile => tile.QuadKey).ToArray());
            var cachedMisses = new List<GeoTile>();

            foreach (var tile in nearbyTiles)
            {
                if (!tileHashSet.Contains(tile.QuadKey))
                {
                    cachedMisses.Add(tile);
                }
            }

            return cachedMisses;
        }
    }
}
