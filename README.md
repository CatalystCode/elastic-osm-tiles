# Elastic Openstreetmap Tiles
A C# based framework that crowdsources openstreetmap geojson based tile data into elasticsearch. Tile data is sourced directly from Mapzens super amazing tile vector [service](https://mapzen.com/projects/vector-tiles/). You will need to sign up for a [free API key](https://mapzen.com/documentation/vector-tiles/api-keys-and-rate-limits/) before using this library.

This project was inspired from the Cities Unlocked collaberation with Microsoft and Guidedogs. These elastic components are built off [NEST](https://github.com/elastic/elasticsearch-net) which is the offical Elasticsearch.NET client. NEST provides a developer friendly abstraction into elastics REST API which translates C# lambda functions into elastic http API statements. NEST also has connection pooling baked in to remove failed nodes from the pool of connecting cluster nodes.

## What problem does this library solve
Querying Openstreetmap data in your own environment involves setting up osmosis, putting a data import job in place, pre-loading your data server with planet files across the world which can take weeks to import. 

We expose REST API's that efficiently serve-up openstreetmap data that's queried off elastic search. This way, you gain access to spatial data from anywhere in the world without needing to host and load gigabytes of planet files. 

We cache data from mapzens vector service into elastic, this way you can query the data however you want.

## Why not query OSM data from Overpass
Overpass is great for adhod queries into openstreetmap providing with granular filter capabilities. Overpass has strict data volume restrictions that will block service requests if requested map bounds exceeds a certain limit. Overpass is also incredibly slow, and using in a production environment is asking for a world of pain and anxiety.

If you plan on using OSM in production, the data should be sourced from an environment within your control. 

## Prerequisites
- Mapzen Tile Server [API key](https://mapzen.com/documentation/vector-tiles/api-keys-and-rate-limits/)
- Visual Studio
- An [Elastic Cluster](https://gist.github.com/erikschlegel/0f4330009c7c5ae83831889609a8bb7c#file-azureelasticclustersetup-md) with basic auth setup.
- Elastic Places and Tiles [indexes](https://github.com/erikschlegel/planet2elastic#setup-elastic-indexes)

## Installation
- git clone <repository-url> this repository
- change into the new directory

## Running / Development
- Open up the ElasticSpatial solution file in Visual Studio
- Modify the Web.config file(\solution\ElasticSpatial\Microsoft.PCT.OSM.Services\Web.config) and provide your elastic credentials and mapzen tile server key.
```C#
  <appSettings>
    <add key="ElasticDefaultIndex" value="places" />
    <add key="ElasticCacheServerHost" value="" />
    <add key="ElasticCachePort" value="9200" />
    <add key="ElasticUsername" value="" />
    <add key="ElasticPwd" value="" />
    <add key="TileServer" value="http://vector.mapzen.com/osm/all/{0}/{1}/{2}.json?api_key={3}" />
    <add key="TileServerAPIKey" value="" />
  </appSettings>
```
- Run the solution in Visual Studio as the services project is set as the startup for the solution.
- You can start querying an area by using the explore service as mentioned below. 

### Services
#### Explore Service (Spatial Data API)
URL:  
`http://your-service-host/api/explore`

Method:  
`POST`

##### Payload Example for 14 7th Ave, New York, NY 10014
```
Content-Type: application/json; charset=UTF-8
{
 "Limit" : 30,
 "CurrentLocation" : {
       "Latitude": 40.7367711,
       "Longitude":-74.0010207
 },
   "SearchRadius": 500
} 

```

Return example:
```
[
  {
    "ResponseStatus": {
      "ExecutionTimeMS": 170,
      "ResponseExceptionMessage": null,
      "ResponseException": null,
      "StatusCode": 200
    },
    "ReturnedResultCount": 30,
    "SourceResultCount": 30,
    "Results": [
      {
        "StreetName": "West 12th Street",
        "AddressLine": "125 West 12th Street",
        "EntityId": "250473022",
        "Latitude": 40.73653307,
        "Type": "way",
        "Longitude": -73.99871982,
        "Name": "building",
        "Tags": "building",
        "SuperCategory": "Place"
      },
      {
        "StreetName": "13th Street",
        "AddressLine": "118 13th Street",
        "EntityId": "250473919",
        "Latitude": 40.73679568,
        "Type": "way",
        "Longitude": -73.99862307,
        "Name": "13th St. Resdience Hall",
        "Tags": "building",
        "SuperCategory": "Place"
      },
```

### C# Elastic Services
#### LocationExploreElasticService
- Usage

```C#
var requestFactory = new HttpRequestFactory();
var tileServerBaseUri = ConfigurationHelper.GetSetting("TileServer");
var tileKey = ConfigurationHelper.GetSetting("TileServerAPIKey");

 var service = new LocationExploreElasticService(tileServerBaseUri, tileKey, TelemetryInstance.Current, requestFactory)
{
                ResultLimit = request.Limit
};

var response = default(IEnumerable<Place>);
response = await service.SearchAsync(request.CurrentLocation, request.SearchRadius);
```

- Unit Test
   - Project: Tests.Microsoft.PCT.OSM
   - File: ElasticServiceTests.cs
   - Sample Test: SpatialDataServiceHandlerTests_SuccessWhatsAroundMeCheck