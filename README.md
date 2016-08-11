# Elastic Openstreetmap Tiles
A C# based framework that crowdsources openstreetmap geojson based tile data into elasticsearch. Tile data is sourced directly from Mapzens super amazing tile vector [service](https://mapzen.com/projects/vector-tiles/). You will need to sign up for a [free API key](https://mapzen.com/documentation/vector-tiles/api-keys-and-rate-limits/) before using this library.

This project was inspired from the Cities Unlocked collaberation with Microsoft and Guidedogs. These elastic components are built off [NEST](https://github.com/elastic/elasticsearch-net) which is the offical Elasticsearch.NET client. NEST provides a developer friendly abstraction into elastics REST API which translates C# lambda functions into elastic http API statements. NEST also has connection pooling baked in to remove failed nodes from the pool of connecting cluster nodes.

## What problem does this library solve
Querying Openstreetmap data in your own environment involves setting up osmosis, putting a data import job in place, pre-loading your data server with planet files across the world which can take weeks to import. 

We expose REST API's that efficiently serve-up openstreetmap data that's queried off elastic search. This way, you gain access to spatial data from anywhere in the world without needing to host and load gigabytes of planet files. 

We cache data from mapzens vector service into elastic, this way you can query the data however you want.

## Why not query OSM data from Overpass
Overpass is great for adhod queries into openstreetmap providing with granular filter capabilities. Overpass has strict data volume restrictions that will block service requests if requested map bounds exceeds a certain limit. Overpass is also incredibly slow, and using in a production environment is asking for a world of pain and anxiety. 

