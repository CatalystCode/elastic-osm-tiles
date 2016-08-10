using Microsoft.PCT.OSM;
using Microsoft.PCT.OSM.DataModel;
using Microsoft.PCT.TestingUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Microsoft.PCT.OSMTests
{
    [TestClass]
    public class TileSystemTests
    {
        [TestMethod]
        public void TileSystemTests_LatLongToQuadKey_BaseScenario_Tests()
        {
            double currentLat = 40.735803, currentLong = -74.0035627;
            int zoomLevel = 16;
            double radiusDistance = 300;
            var expectedQuadKeys = new List<GeoTile>() { new GeoTile() { QuadKey = "0320101101322020" },
                                                         new GeoTile() { QuadKey = "0320101101233131" },
                                                         new GeoTile() { QuadKey = "0320101101322002" },
                                                         new GeoTile() { QuadKey = "0320101101233113" }};

            var quadKeys = TileSystem.LatLongToNearbyGeoTiles(zoomLevel, currentLat, currentLong, radiusDistance);
            Assert.AreEqual(quadKeys.Count(), expectedQuadKeys.Count());
            Assert.IsTrue(GeoTileListsAreEqual(quadKeys, expectedQuadKeys));
        }

        [TestMethod]
        public void TileSystemTests_LatLongToQuadKey_AllNearbyScenario_Test()
        {
            double currentLat = 40.736771, currentLong = -74.0010207;
            int zoomLevel = 16;
            double radiusDistance = 300;

            var expectedQuadKeys = new List<GeoTile>() { new GeoTile() { QuadKey = "0320101101322020" },
                                                         new GeoTile() { QuadKey = "0320101101322021" },
                                                         new GeoTile() { QuadKey = "0320101101233131" },
                                                         new GeoTile() { QuadKey = "0320101101322022" },
                                                         new GeoTile() { QuadKey = "0320101101322002" },
                                                         new GeoTile() { QuadKey = "0320101101322003" },
                                                         new GeoTile() { QuadKey = "0320101101322023" },
                                                         new GeoTile() { QuadKey = "0320101101233133" },
                                                         new GeoTile() { QuadKey = "0320101101233113" }};

            var quadKeys = TileSystem.LatLongToNearbyGeoTiles(zoomLevel, currentLat, currentLong, radiusDistance);
            Assert.AreEqual(quadKeys.Count(), expectedQuadKeys.Count());
            Assert.IsTrue(GeoTileListsAreEqual(quadKeys, expectedQuadKeys));
        }

        [TestMethod]
        public void TileSystemTests_LatLongToQuadKey_LargeAreaScenario_Test()
        {
            double currentLat = 40.736771, currentLong = -74.0010207;
            int zoomLevel = 16;
            double radiusDistance = 3000;
            int expectedTileCount = 169;

            var quadKeys = TileSystem.LatLongToNearbyGeoTiles(zoomLevel, currentLat, currentLong, radiusDistance);
            Assert.AreEqual(quadKeys.Count(), expectedTileCount);
        }

        [TestMethod]
        public void TileSystemTests_LatLongToQuadKey_IsolatedScenario_Test()
        {
            double currentLat = 40.736771, currentLong = -74.0010207;
            int zoomLevel = 16;
            double radiusDistance = 100;

            var expectedQuadKeys = new List<GeoTile>() { new GeoTile() { QuadKey = "0320101101322020" } };

            var quadKeys = TileSystem.LatLongToNearbyGeoTiles(zoomLevel, currentLat, currentLong, radiusDistance);
            Assert.AreEqual(quadKeys.Count(), expectedQuadKeys.Count());
            Assert.IsTrue(GeoTileListsAreEqual(quadKeys, expectedQuadKeys));
        }

        [TestMethod]
        public void TileSystemTests_LatLongToQuadKey_InvalidArguments_Test()
        {
            AssertEx.Throws<ArgumentNullException>(() => TileSystem.LatLongToNearbyGeoTiles(16, 12, 23, 0));
            AssertEx.Throws<ArgumentNullException>(() => TileSystem.LatLongToNearbyGeoTiles(0, 12, 23, 0));
            AssertEx.Throws<InvalidOperationException>(() => TileSystem.LatLongToNearbyGeoTiles(16, 1623, 23, 100));
        }

        private static bool GeoTileListsAreEqual(List<GeoTile> listA, List<GeoTile> listB)
        {
            foreach (var tile in listA)
            {
                if (!listB.Exists(item => item.QuadKey == tile.QuadKey))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
