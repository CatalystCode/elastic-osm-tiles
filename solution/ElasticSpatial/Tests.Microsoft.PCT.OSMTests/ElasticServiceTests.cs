using Microsoft.PCT.Configuration;
using Microsoft.PCT.Http;
using Microsoft.PCT.OSM.DataModel;
using Microsoft.PCT.OSM.Services;
using Microsoft.PCT.TestingUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Microsoft.PCT.OSMTests
{
    [TestClass]
    public class ElasticServiceTests
    {
        private const double NYC_LAT = 40.756022;
        private const double NYC_LONG = -73.9883047;

        static async Task<IEnumerable<Place>> QueryElasticAsync(double radius, IGeoCoordinate location, int limit)
        {
            var service = CreateExploreHandler(limit);

            return await service.SearchAsync(location, radius) as IEnumerable<Place>;
        }

        [TestMethod]
        public async Task SpatialDataServiceHandlerTests_BadArgumentCheck()
        {
            var testPosition = new GeoCoordinate(NYC_LAT, NYC_LONG);
            var service = CreateExploreHandler(2);
            await AssertEx.Throws<ArgumentNullException>(() => service.SearchAsync(null, null), ex => Assert.AreEqual("userLocation", ex.ParamName));
        }

        [TestMethod]
        public async Task SpatialDataServiceHandlerTests_RadiusCheck()
        {
            var testPosition = new GeoCoordinate(NYC_LAT, NYC_LONG);
            double requestedRadius = 200;
            var results = await QueryElasticAsync(requestedRadius, testPosition, 50);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count() == 50);
        }

        [TestMethod]
        public async Task SpatialDataServiceHandlerTests_SuccessWhatsAroundMeCheck()
        {
            var testPosition = new GeoCoordinate(NYC_LAT, NYC_LONG);
            var results = await QueryElasticAsync(500, testPosition, 50);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count() == 50);
            bool TheatreExists = false;

            foreach (var item in results)
            {
                if (item.amenity == "theatre")
                {
                    TheatreExists = true;
                }
            }

            Assert.IsTrue(TheatreExists);
        }

        private static IElasticService CreateExploreHandler(int limit)
        {
            var tileServerBaseUri = ConfigurationHelper.GetSetting("TileServer");
            var tileKey = ConfigurationHelper.GetSetting("TileServerAPIKey");

            var service = new LocationExploreElasticService(tileServerBaseUri, tileKey, null, new HttpRequestFactory())
            {
                ResultLimit = limit
            };

            return service;
        }
    }
}
