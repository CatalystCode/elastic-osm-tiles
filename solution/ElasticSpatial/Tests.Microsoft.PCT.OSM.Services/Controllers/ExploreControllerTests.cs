using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.PCT.OSM;
using Microsoft.PCT.OSM.Internal;
using Microsoft.PCT.OSM.DataModel;
using System.Linq;
using Microsoft.PCT.TestingUtilities;
using System.Web.Http;
using Microsoft.PCT.OSM.Services;
using System.Net.Http;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using System.Text;
using Microsoft.PCT.Serialization;
using System.Collections.Generic;

namespace Tests.Microsoft.PCT.OSM.Services.Controllers
{
    [TestClass]
    public class ExploreControllerTests
    {
        [TestMethod]
        public async Task ExploreController_ProxmitySearch_AmenityNotNullCheck()
        {
            double currentLat = 51.454496, currentLong = -0.974407;
            double radiusDistance = 500;

            var nearbyPlaces = await ElasticManager.Instance.ProximitySearchAsync(radiusDistance, new GeoCoordinate() { Latitude = currentLat, Longitude = currentLong });

            var poiList = nearbyPlaces.ToList();
            Assert.IsNotNull(poiList.FirstOrDefault()?.amenity);
        }

        [TestMethod]
        public async Task ExploreController_ProxmitySearch_Success_Http_Response_Test()
        {
            await IntegrationTest.Run(async () =>
            {
                var config = new HttpConfiguration();
                double currentLat = 51.454496, currentLong = -0.974407;

                WebApiConfig.Register(config);

                using (var server = new HttpServer(config))
                using (var client = new HttpMessageInvoker(server))
                using (var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/explore"))
                {
                    var goodRequest = "{'CurrentLocation': { 'latitude': " + currentLat + ", 'longitude': " + currentLong + "}, 'SearchRadius': 500, 'Limit': '500'}";
                    request.Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(goodRequest)));
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    using (var response = await client.SendAsync(request, CancellationToken.None))
                    {
                        Assert.IsTrue(response.IsSuccessStatusCode);
                        Assert.AreEqual(response.Content.Headers.ContentType.MediaType, "application/json");
                        var stream = await response.Content.ReadAsStreamAsync();
                        var recognized = JsonHelpers.Deserialize<Queue<SpatialDataResponse>>(stream);
                        var osmResults = recognized.FirstOrDefault();

                        Assert.IsNotNull(osmResults);

                        // Make sure crossings and steps are marked as mobility. Check for John Lewis to be marked as a landmark.
                        Assert.IsNotNull(osmResults.Results.FirstOrDefault()?.amenity);
                    }
                }
            });
        }
    }
}
