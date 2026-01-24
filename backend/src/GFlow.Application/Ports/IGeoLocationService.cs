using System.Threading.Tasks;

namespace GFlow.Application.Ports
{
    public interface IGeoLocationService
    {
        Task<GeoLocationResult?> GetLocationAsync(string ip);
        Task<GeoLocationResult?> GeocodeAsync(string city);
    }


    public class GeoLocationResult
    {
        public string? CountryCode { get; set; }
        public string? City { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
    }
}
