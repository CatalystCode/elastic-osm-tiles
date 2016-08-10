using Microsoft.PCT.OSM.DataModel;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.PCT.OSM.Internal
{
    public static class ElasticManagerExtensions
    {
        /// <summary>
        /// Brokers a bulk operation on a series of index(s) on a Elastic Host. 
        /// </summary>
        /// <param name="operations">The <see cref="BulkDescriptor"/> detailing the operations to the elastic host.</param>
        /// <returns>The <see cref="IBulkResponse"/> response from the operation.</returns>
        public static async Task<IBulkResponse> BulkOperationAsync(this ElasticManager self, BulkDescriptor operations)
        {
            return await self.Connection.BulkAsync(operations);
        }

        /// <summary>
        /// Brokers a bulk operation on a series of index(s) on a Elastic Host. 
        /// </summary>
        /// <param name="operations">The <see cref="BulkDescriptor"/> detailing the operations to the elastic host.</param>
        /// <returns>The <see cref="IBulkResponse"/> response from the operation.</returns>
        public static IBulkResponse BulkOperation(this ElasticManager self, BulkDescriptor operations)
        {
            return self.Connection.Bulk(operations);
        }

        /// <summary>
        /// Performs a near distance proximity search against the elastic search places index based on a distance and <see cref="IGeoCoordinate"/>.
        /// </summary>
        /// <param name="meterDistance">The radius distance used to perform the geo distance search.</param>
        /// <param name="location">The <see cref="IGeoCoordinate"/> location center.</param>
        /// <param name="layer">The mapzen layer to filter the results.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<Place>> ProximitySearchAsync(this ElasticManager self, double meterDistance, IGeoCoordinate location, string layer = null)
        {
            if (meterDistance <= 0)
                throw new ArgumentNullException(nameof(meterDistance));

            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (location.Latitude == 0 || location.Longitude == 0)
                throw new ArgumentNullException(nameof(location));

            int currentPageFrom = 0;
            long placesCount = long.MaxValue;

            var searchResponse = new List<Place>();
            //TODO: Integrate elastic scrolling as opposed to hard coded paging. 
            int pageSize = 4000;

            var searchDescriptor = new SearchDescriptor<Place>()
                                           .Index(ElasticManager.PLACES_INDEX)
                                           .Type(ElasticManager.PLACES_TYPE)
                                           .Size(pageSize)
                                           .Query(f =>
                                                  f.Term(p => p.layer, layer) &&
                                                  f.Bool(b =>
                                                  {
                                                      var filters = new List<Func<QueryContainerDescriptor<Place>, QueryContainer>>();

                                                      filters.Add(filter => filter
                                                             .GeoDistance(bb => bb.Field(bf => bf.location)
                                                                                   .Location(new GeoLocation(location.Latitude, location.Longitude))
                                                                                   .DistanceType(GeoDistanceType.Plane)
                                                                                   .Distance(new Distance(meterDistance, DistanceUnit.Meters))));

                                                      filters.Add(filter => filter.GeoShapeCircle(gc => gc
                                                                 .Field(p => p.coordinates)
                                                                 .Coordinates(new Nest.GeoCoordinate(location.Latitude, location.Longitude))
                                                                 .Radius(string.Format("{0}m", meterDistance))));

                                                      return b.Should(filters);
                                                  }));

            do
            {
                var places = default(ISearchResponse<Place>);
                try
                {
                    places = await self.Connection.SearchAsync<Place>(searchDescriptor.From(currentPageFrom));
                }
                catch (Exception)
                {
                    throw;
                }

                placesCount = places.Total;

                foreach (var place in places.Hits)
                {
                    var poi = place.Source;
                    searchResponse.Add(poi);
                }

                currentPageFrom += places.Documents.Count();
            }
            while (currentPageFrom < placesCount);

            return searchResponse;
        }

        /// <summary>
        /// Performs a near distance proximity search against the elastic search places index based on a distance and <see cref="IGeoCoordinate"/>.
        /// </summary>
        /// <param name="meterDistance">The radius distance used to perform the geo distance search.</param>
        /// <param name="location">The <see cref="IGeoCoordinate"/> location center.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<Place>> ProximitySearchAsync(this ElasticManager self, double meterDistance, IGeoCoordinate location)
        {
            return await self.ProximitySearchAsync(meterDistance, location, null);
        }

        /// <summary>
        /// Filters the <see cref="List{T}"/> that are available in the elastic cache.
        /// </summary>
        /// <param name="tiles">The nearby tiles based on the users current position.</param>
        /// <returns></returns>
        public static async Task<List<Tile>> CachedTiles(this ElasticManager self, List<GeoTile> tiles)
        {
            if (tiles == null)
                throw new ArgumentNullException(nameof(tiles));

            int pageSize = 100;
            int currentPageFrom = 0;
            long tileCount = long.MaxValue;
            var tileList = new List<Tile>();
            var searchDescriptor = new SearchDescriptor<Tile>()
                                          .Index(ElasticManager.TILES_INDEX)
                                          .Type(ElasticManager.TILES_TYPE)
                                          .Size(pageSize)
                                          .Query(f => f.Bool(b =>
                                          {
                                              var filters = new List<Func<QueryContainerDescriptor<Tile>, QueryContainer>>();

                                              foreach (var tile in tiles)
                                              {
                                                  filters.Add(filter => filter
                                                    .Term(term => term.Field(field => field.QuadKey)
                                                    .Value(tile.QuadKey)));
                                              }

                                              return b.Should(filters);
                                          }));

            do
            {
                var results = default(ISearchResponse<Tile>);

                try
                {

                    results = await self.Connection.SearchAsync<Tile>(searchDescriptor.From(currentPageFrom));
                }
                catch (Exception)
                {
                    throw;
                }

                tileCount = results.Total;

                foreach (var tile in results.Hits)
                {
                    tileList.Add(tile.Source);
                }

                currentPageFrom += results.Documents.Count();
            }
            while (currentPageFrom < tileCount);

            return tileList;
        }

        /// <summary>
        /// Performs a local search based on a place / POI query. 
        /// </summary>
        /// <param name="query">The targeted point of interest / place.</param>
        /// <param name="searchRadius">The radius distance used to perform the geo distance search.</param>
        /// <param name="location">The <see cref="IGeoCoordinate"/> location center.</param>
        /// <param name="layer">The layer filter to apply for local search.</param>
        /// <returns></returns>
        public static async Task<ISearchResponse<Place>> LocalizedPoiSearch(this ElasticManager self, string query, double searchRadius, IGeoCoordinate location, string layer = null)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (location == null)
                throw new ArgumentNullException(nameof(location));

            var results = await self.Connection.SearchAsync<Place>(search => search
                                          .Size(2000)
                                          .Query(m => m.MultiMatch(fm => fm.Fields(f => f.Field(amenity => amenity.amenity)
                                                                                         .Field(name => name.name)
                                                                                         .Field(cuisine => cuisine.cuisine))
                                                            .Fuzziness(Fuzziness.Auto)
                                                            .Type(TextQueryType.MostFields)
                                                            .Query(query)
                                                       ) &&
                                                       m.Term(p => p.layer, layer) &&
                                                       m.Bool(b => {
                                                           var filters = new List<Func<QueryContainerDescriptor<Place>, QueryContainer>>();

                                                           filters.Add(filter => filter
                                                                  .GeoDistance(bb => bb.Field(bf => bf.location)
                                                                                        .Location(new GeoLocation(location.Latitude, location.Longitude))
                                                                                        .DistanceType(GeoDistanceType.Plane)
                                                                                        .Distance(new Distance(searchRadius, DistanceUnit.Meters))));

                                                           filters.Add(filter => filter.GeoShapeCircle(gc => gc
                                                                      .Field(p => p.coordinates)
                                                                      .Coordinates(new Nest.GeoCoordinate(location.Latitude, location.Longitude))
                                                                      .Radius(string.Format("{0}m", searchRadius))));

                                                           return b.Should(filters);
                                                       })));

            return results;
        }
        
        /// <summary>
        /// Returns a <see cref="Dictionary{string, GeoTile}"/> of all <see cref="GeoTile"/> in the elastic tiles index.
        /// </summary>
        /// <returns></returns>
        public static async Task<Dictionary<string, GeoTile>> GetAllCachedTiles(this ElasticManager self)
        {
            int pageSize = 100;
            int currentPageFrom = 0;
            long tileCount = long.MaxValue;
            var tileDictionary = new Dictionary<string, GeoTile>();
            var searchDescriptor = new SearchDescriptor<Tile>()
                                           .Index(ElasticManager.TILES_INDEX)
                                           .Type(ElasticManager.TILES_TYPE)
                                           .Size(pageSize)
                                           .Query(q => q.MatchAll());

            do
            {
                var tiles = default(ISearchResponse<Tile>);
                try
                {
                    tiles = await self.Connection.SearchAsync<Tile>(searchDescriptor.From(currentPageFrom));
                }
                catch (Exception e)
                {
                    throw e;
                }


                tileCount = tiles.Total;

                foreach (var tile in tiles.Hits)
                {
                    var quadKey = tile.Source.QuadKey;
                    int tileX = 0, tileY = 0, zoomLevel = 0;
                    if (quadKey == null)
                    {
                        continue;
                    }

                    TileSystem.QuadKeyToTileXY(quadKey, out tileX, out tileY, out zoomLevel);
                    var key = string.Format("{0}_{1}", tileX, tileY);
                    if (!tileDictionary.ContainsKey(key))
                    {
                        tileDictionary.Add(key, new GeoTile() { QuadKey = quadKey, X = tileX, Y = tileY, ZoomLevel = zoomLevel });
                    }
                }

                currentPageFrom += tiles.Documents.Count();
            }
            while (currentPageFrom < tileCount);

            return tileDictionary;
        }
    }
}
