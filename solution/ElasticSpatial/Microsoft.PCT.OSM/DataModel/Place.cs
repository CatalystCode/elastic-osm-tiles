
namespace Microsoft.PCT.OSM.DataModel
{
    public class Place : IPlace
    {
        public string name { get; set; }
        public string Id { get; set; }
        public string type { get; set; }
        public Location location { get; set; }
        public Coordinates coordinates { get; set; }
        public string amenity { get; set; }
        public string cuisine { get; set; }
        public string phone { get; set; }
        public string street { get; set; }
        public string postcode { get; set; }
        public string housenumber { get; set; }
        public string city { get; set; }
        public double height { get; set; }
        public string quadKey { get; set; }
        public string footway { get; set; }
        public string highway { get; set; }
        public double volume { get; set; }
        public string timestamp { get; set; }
        public string layer { get; set; }
        public string osmId { get; set; }
        public double distance { get; set; }
        public double _score { get; set; }
    }
}
