namespace Microsoft.PCT.OSM
{
    using GeoJSON.Net;
    using GeoJSON.Net.Feature;
    using GeoJSON.Net.Geometry;
    using Microsoft.PCT.OSM.DataModel;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Class thats responsible for parsing a GeoJSON formatted <see cref="FeatureCollection"/> into 
    /// a <see cref="Dictionary<string, Place>"/>.
    /// </summary>
    public class GeoJsonParser
    {
        private const string TIMESTAMP_FORMAT = "yyy-MM-dd HH:mm:ss";

        private const string LAYER_LANDUSE = "landuse";
        private const string LAYER_BUILDING = "buildings";
        private const string LAYER_POI = "pois";
        private const string LAYER_BOUNDARIES = "boundaries";
        private const string LAYER_ROADS = "roads";
        private const string LAYER_PLACES = "places";
        private const string LAYER_WATER = "water";
        private const string LAYER_TRANSIT = "transit";

        /// <summary>
        /// Mainly exposed parsing method responsible for parsing a <see cref="GeoTileVector"/> from Mapzen
        /// into a <see cref="Dictionary<string, Place>"/> so it can be consumed and indexed by elastic.
        /// </summary>
        /// <param name="vectorTile">The Mapzen geo tile.</param>
        /// <param name="tileQuadKey">The Tile Quad Key.</param>
        /// <returns></returns>
        public static Dictionary<string, Place> TryParseGeoJsonToPlaceDictionary(GeoTileVector vectorTile, string tileQuadKey)
        {
            var osmObjectDictionary = new Dictionary<string, Place>();

            ProcessFeatureCollecion(vectorTile.landuse, tileQuadKey, LAYER_LANDUSE, ref osmObjectDictionary);
            ProcessFeatureCollecion(vectorTile.buildings, tileQuadKey, LAYER_BUILDING, ref osmObjectDictionary);
            ProcessFeatureCollecion(vectorTile.pois, tileQuadKey, LAYER_POI, ref osmObjectDictionary);
            ProcessFeatureCollecion(vectorTile.places, tileQuadKey, LAYER_PLACES, ref osmObjectDictionary);
            ProcessFeatureCollecion(vectorTile.boundaries, tileQuadKey, LAYER_BOUNDARIES, ref osmObjectDictionary);
            ProcessFeatureCollecion(vectorTile.roads, tileQuadKey, LAYER_ROADS, ref osmObjectDictionary);
            ProcessFeatureCollecion(vectorTile.transit, tileQuadKey, LAYER_TRANSIT, ref osmObjectDictionary);
            ProcessFeatureCollecion(vectorTile.water, tileQuadKey, LAYER_WATER, ref osmObjectDictionary);

            return osmObjectDictionary;
        }

        /// <summary>
        /// Returns a string to a title case format.
        /// </summary>
        private static string ToTitleCase(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }
        
        /// <summary>
        /// Mergem two places with the same id into one. It compares and keeps the important information from each.
        /// </summary>
        private static Place MergeGeometryProperties(Place existingEntry, ref Place place)
        {
            if (existingEntry.name != existingEntry.type && place.name == place.type)
            {
                place.name = existingEntry.name;
            }

            if (existingEntry.type != "none")
            {
                place.type = existingEntry.type;
                place.amenity = existingEntry.amenity;
            }

            if (!string.IsNullOrWhiteSpace(existingEntry.street))
            {
                place.street = existingEntry.street;
            }

            if (!string.IsNullOrWhiteSpace(existingEntry.cuisine))
            {
                place.cuisine = existingEntry.cuisine;
            }

            if (!string.IsNullOrWhiteSpace(existingEntry.highway))
            {
                place.highway = existingEntry.highway;
            }

            if (!string.IsNullOrWhiteSpace(existingEntry.footway))
            {
                place.footway = existingEntry.footway;
            }

            if (!string.IsNullOrWhiteSpace(existingEntry.housenumber))
            {
                place.housenumber = existingEntry.housenumber;
            }

            return place;
        }

        /// <summary>
        /// Parses the geometry properties and returns a place.
        /// </summary>
        private static bool TryParseGeometryProperties(Dictionary<string, object> geometryProperties, ref Place place)
        {
            if (place == null)
            {
                throw new ArgumentException(nameof(place));
            }

            var kind = default(object);
            var amenity = default(string);
            var name = default(object);
            var street = default(object);
            var cuisine = default(object);
            var houseNumber = default(object);
            var entityIdLong = default(long);
            var footway = default(object);
            var highway = default(object);

            //Get the place type and id from OSM.
            if (!geometryProperties.TryGetValue("kind", out kind))
            {
                //if there is no kind/type then set to none.
                kind = "none";
            }

            amenity = kind.ToString();
            long.TryParse(place.Id, out entityIdLong);
            //Skp any nodes that are less than 0.
            if (entityIdLong < 0)
            {
                return false;
            }

            if (!geometryProperties.TryGetValue("name", out name))
            {
                name = kind;
            }

            if (geometryProperties.TryGetValue("addr_street", out street))
            {
                place.street = street.ToString();
            }

            if (geometryProperties.TryGetValue("cuisine", out cuisine))
            {
                place.cuisine = cuisine.ToString();
            }

            if (geometryProperties.TryGetValue("highway", out highway))
            {
                place.highway = highway.ToString();
            }

            if (geometryProperties.TryGetValue("footway", out footway))
            {
                place.footway = footway.ToString();
            }

            if (geometryProperties.TryGetValue("addr_housenumber", out houseNumber))
            {
                place.housenumber = houseNumber.ToString();
            }

            place.amenity = amenity;
            place.name = name.ToString();

            return true;
        }

        /// <summary>
        /// Process an entire feature set from a layer.
        /// </summary>
        private static void ProcessFeatureCollecion(FeatureCollection features, string tileQuadKey, string layer, ref Dictionary<string, Place> osmPlaces)
        {
            if (features == null)
            {
                return;
            }

            foreach (var feature in features.Features)
            {
                var osmId = default(object);
                var place = default(Place);
                var existingEntry = default(Place);
                bool newEntry = false;

                //Skip the OSM feature if its missing an identifier.
                if (!feature.Properties.TryGetValue("id", out osmId))
                {
                    continue;
                }

                if (!osmPlaces.TryGetValue(osmId.ToString(), out place))
                {
                    var osmIdentifierLong = default(long);

                    if (long.TryParse(osmId.ToString(), out osmIdentifierLong) && osmIdentifierLong <= 0)
                    {
                        continue;
                    }

                    //indexing documents on a tile / OSM id basis. This is due to the fact that ways can bleed into other tiles. 
                    //For performance purposes, index operations need to happen on a tile by tile basis. we don't have access to
                    //neighboring tile coordinates for spanning ways. 
                    var elasticDocumentId = string.Format("{0}-{1}", tileQuadKey, osmId.ToString());
                    place = new Place { Id = elasticDocumentId, type = "node", osmId = osmId.ToString(), layer = layer, timestamp = DateTime.Now.ToString(TIMESTAMP_FORMAT), quadKey = tileQuadKey };
                    newEntry = !osmPlaces.ContainsKey(place.osmId);
                }

                if (!TryParseGeometryProperties(feature.Properties, ref place))
                {
                    continue;
                }

                if (newEntry)
                {
                    osmPlaces.Add(place.osmId, place);
                }
                else
                {
                    existingEntry = osmPlaces[place.osmId];

                    // Merge properties from both places to keep important information.
                    MergeGeometryProperties(existingEntry, ref place);
                }

                if (feature.Geometry.Type == GeoJSONObjectType.Polygon)
                {
                    var polygonGeometry = feature.Geometry as Polygon;
                    place.type = "way";
                    var newCoordinates = new Coordinates() { type = "multilinestring", coordinates = ConvertMultiLineStringto3dArray(polygonGeometry.Coordinates) };

                    if (newEntry || place.coordinates == null)
                    {
                        place.coordinates = newCoordinates;
                    }
                    else
                    {
                        MergeTwoPlacesCoordinates(existingEntry, newCoordinates, ref place);
                    }
                }
                else if (feature.Geometry.Type == GeoJSONObjectType.LineString)
                {
                    var polygonGeometry = feature.Geometry as LineString;
                    place.type = "way";
                    var newCoordinates = new Coordinates() { type = "multilinestring", coordinates = ConvertMultiLineStringto3dArray(new List<LineString>() { polygonGeometry }) };

                    if (newEntry || place.coordinates == null)
                    {
                        place.coordinates = newCoordinates;
                    }
                    else
                    {
                        MergeTwoPlacesCoordinates(existingEntry, newCoordinates, ref place);
                    }
                }
                else if (feature.Geometry.Type == GeoJSONObjectType.Point)
                {
                    var coordinates = ((Point)feature.Geometry).Coordinates as GeographicPosition;

                    if (newEntry || existingEntry.type == "node")
                    {
                        place.location = new Location() { lat = coordinates.Latitude, lon = coordinates.Longitude };
                    }
                }

                osmPlaces[place.osmId] = place;
            }
        }

        /// <summary>
        /// Merges the coordinates from two places with the same id.
        /// </summary>
        private static void MergeTwoPlacesCoordinates(Place existingPlace, Coordinates newCoordinates, ref Place newPlace)
        {
            if (existingPlace.type == "way")
            {
                existingPlace.coordinates = new Coordinates() { type = "multilinestring", coordinates = MergeTwo3dArrayCoordinates(newCoordinates.coordinates, newPlace.coordinates.coordinates) };
            }
            else
            {
                existingPlace.type = "way";
                existingPlace.coordinates = newCoordinates;
            }

            newPlace = existingPlace;
        }

        /// <summary>
        /// Merges the 3d coordinates array of two places with the same id. 
        /// </summary>
        private static double[][][] MergeTwo3dArrayCoordinates(double[][][] array1, double[][][] array2)
        {
            var coordinates = new double[array1.Length + array2.Length][][];
            var i = 0;

            for (; i < array1.Length; i++)
            {
                coordinates[i] = new double[array1[i].Length][];
                for (int s = 0; s < array1[i].Length; s++)
                {
                    var lineCoords = array1[i][s];
                    coordinates[i][s] = new double[] { lineCoords[0], lineCoords[1] };
                }
            }

            for (int j = 0; j < array2.Length; j++)
            {
                coordinates[i] = new double[array2[j].Length][];
                for (int s = 0; s < array2[j].Length; s++)
                {
                    var lineCoords = array2[j][s];
                    coordinates[i][s] = new double[] { lineCoords[0], lineCoords[1] };
                }

                i++;
            }

            return coordinates;
        }

        /// <summary>
        /// Converts the multi line coordinates into a 3d array.
        /// </summary>
        private static double[][][] ConvertMultiLineStringto3dArray(List<LineString> multilinestring)
        {
            var coordinates = new double[multilinestring.Count][][];

            for (int i = 0; i < multilinestring.Count; i++)
            {
                coordinates[i] = new double[multilinestring[i].Coordinates.Count][];
                for (int s = 0; s < multilinestring[i].Coordinates.Count; s++)
                {
                    var lineCoords = multilinestring[i].Coordinates[s] as GeographicPosition;
                    coordinates[i][s] = new double[] { lineCoords.Longitude, lineCoords.Latitude };
                }
            }

            return coordinates;
        }
    }
}
