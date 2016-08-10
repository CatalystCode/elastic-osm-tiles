using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.PCT.OSM;
using System.Threading.Tasks;
using Microsoft.PCT.OSM.Internal;
using System.Collections.Generic;
using Microsoft.PCT.OSM.DataModel;
using System.Linq;
using Microsoft.PCT.TestingUtilities;

namespace Tests.Microsoft.PCT.OSM
{
    [TestClass()]
    public class ElasticManagerTests
    {
        [TestMethod]
        public void ElasticManager_Connection_Test()
        {
            var connection = ElasticManager.Instance.Connection;
            Assert.IsTrue(connection.NodesInfo().Nodes.Count >= 1);
        }

        [TestMethod]
        public async Task ElasticManager_CachedTile_Test()
        {
            var tiles = new List<GeoTile>() { new GeoTile() { QuadKey = "0320101101322021", Id = "0320101101322021" } };

            var results = await ElasticManager.Instance.CachedTiles(tiles);
            Assert.AreEqual(results.Count(), 1);
        }

        [TestMethod]
        public async Task ElasticManager_CachedTile__Invalid_Argument_Test()
        {
            await AssertEx.Throws<ArgumentNullException>(() => ElasticManager.Instance.CachedTiles(null), ex => Assert.AreEqual("tiles", ex.ParamName));
        }

        public async Task ElasticManager_NearbyPlacesIndex_Test()
        {
            var descriptor = new Nest.BulkDescriptor();
            var coord1 = new double[][][] { new double[][] { new double[] { -74.0008906, 40.73659338 }, new double[] { -74.0008904, 40.73659339 }, new double[] { -74.0008902, 40.73659332 }, new double[] { -75.0008902, 41.73659332 } } };
            descriptor.Index<Place>(op => op.Document(new Place()
            {
                amenity = "restaurant",
                cuisine = "american",
                name = "Bluestone",
                Id = "232323223",
                type = "node",
                coordinates = new Coordinates() { type = "multilinestring", coordinates = coord1 }
            })
            .Index("places"));

            descriptor.Index<Place>(op => op.Document(new Place()
            {
                amenity = "restaurant",
                cuisine = "italian",
                name = "Right Next Door",
                Id = "232323221",
                type = "node",
                location = new Location() { lat = 40.73659338, lon = -74.0008906 }
            })
            .Index("places"));

            var test = await ElasticManager.Instance.BulkOperationAsync(descriptor);
            var nearbyPlaces = await ElasticManager.Instance.ProximitySearchAsync(2, new GeoCoordinate() { Latitude = 40.73659338, Longitude = -74.0008906 });
            Assert.AreEqual(nearbyPlaces.Count(), 2);
        }

        [TestMethod]
        public async Task ElasticManager_ProxmitySearch_Test_NoResults()
        {
            var nearbyPlaces = await ElasticManager.Instance.ProximitySearchAsync(50, new GeoCoordinate() { Latitude = 0.01, Longitude = 0.01 });

            Assert.IsTrue(nearbyPlaces.Count() == 0);
        }

        [TestMethod]
        public async Task ElasticManager_ProxmitySearch_Invalid_Argument()
        {
            await AssertEx.Throws<ArgumentNullException>(() => ElasticManager.Instance.ProximitySearchAsync(50, new GeoCoordinate()), ex => Assert.AreEqual("location", ex.ParamName));
            await AssertEx.Throws<ArgumentNullException>(() => ElasticManager.Instance.ProximitySearchAsync(0, new GeoCoordinate() { Latitude = 42.31210699, Longitude = -71.19647991 }), ex => Assert.AreEqual("meterDistance", ex.ParamName));
            await AssertEx.Throws<ArgumentNullException>(() => ElasticManager.Instance.ProximitySearchAsync(0, new GeoCoordinate() { Latitude = 42.31210699, Longitude = -71.19647991 }), ex => Assert.AreEqual("meterDistance", ex.ParamName));
        }

        [TestMethod]
        public async Task ElasticManager_ProxmitySearch_TileFilter_Base()
        {
            double currentLat = 40.736771, currentLong = -74.0010207;
            double radiusDistance = 300;

            var quadKeys = new List<GeoTile>() { new GeoTile() { QuadKey = "0320101101322020" },
                                                         new GeoTile() { QuadKey = "0320101101322021" },
                                                         new GeoTile() { QuadKey = "0320101101233131" },
                                                         new GeoTile() { QuadKey = "0320101101322022" },
                                                         new GeoTile() { QuadKey = "0320101101322002" },
                                                         new GeoTile() { QuadKey = "0320101101322003" },
                                                         new GeoTile() { QuadKey = "0320101101322023" },
                                                         new GeoTile() { QuadKey = "0320101101233133" },
                                                         new GeoTile() { QuadKey = "0320101101233113" }};

            var nearbyPlaces = await ElasticManager.Instance.ProximitySearchAsync(radiusDistance, new GeoCoordinate() { Latitude = currentLat, Longitude = currentLong });
            int documentCount = nearbyPlaces.Count();

            Assert.IsTrue(documentCount > 0);
        }

        [TestMethod]
        public void ElasticManager_TileCacheMiss_Test()
        {
            var nearbyTiles = new List<GeoTile>() { new GeoTile() { QuadKey = "tile1" }, new GeoTile() { QuadKey = "tile2" }, new GeoTile() { QuadKey = "tile3" }, new GeoTile() { QuadKey = "tile4" } };
            var cachedTiles = new List<Tile>() { new Tile() { QuadKey = "tile2" }, new Tile() { QuadKey = "tile3" } };
            var tileMisses = ElasticManager.TileCacheMisses(cachedTiles, nearbyTiles);

            Assert.AreEqual(tileMisses.Count(), 2);
            Assert.AreEqual(tileMisses.FirstOrDefault().QuadKey, "tile1");
        }

        [TestMethod]
        public async Task ElasticManager_GetAllCachedTiles_Test()
        {
            var currentTiles = await ElasticManager.Instance.GetAllCachedTiles();

            Assert.IsTrue(currentTiles.Count > 1);
        }
    }
}
