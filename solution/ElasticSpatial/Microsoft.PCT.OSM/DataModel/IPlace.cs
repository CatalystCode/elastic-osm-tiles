
namespace Microsoft.PCT.OSM.DataModel
{
    public interface IPlace
    {
        Location location { get; set; }
        Coordinates coordinates { get; set; }
    }

    public class Location
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }

    public class Coordinates
    {
        public string type { get; set; }
        public double[][][] coordinates { get; set; }
    }
}
