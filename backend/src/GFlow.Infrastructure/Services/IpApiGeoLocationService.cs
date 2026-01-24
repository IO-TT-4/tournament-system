using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GFlow.Application.Ports;

namespace GFlow.Infrastructure.Services
{
    public class IpApiGeoLocationService : IGeoLocationService
    {
        private readonly HttpClient _httpClient;

        public IpApiGeoLocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GeoLocationResult?> GetLocationAsync(string ip)
        {
            if (string.IsNullOrEmpty(ip) || ip == "::1" || ip == "127.0.0.1")
            {
                // Local testing fallback - Warsaw
                return new GeoLocationResult
                {
                    CountryCode = "PL",
                    City = "Warsaw",
                    Lat = 52.2297,
                    Lng = 21.0122
                };
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<IpApiResponse>($"http://ip-api.com/json/{ip}");
                if (response?.Status == "success")
                {
                    return new GeoLocationResult
                    {
                        CountryCode = response.CountryCode,
                        City = response.City,
                        Lat = response.Lat,
                        Lng = response.Lon
                    };
                }
            }
            catch
            {
                // Silent fail/fallback
            }

            return null;
        }

        public async Task<GeoLocationResult?> GeocodeAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return null;

            try
            {
                // Nominatim REQUIRES a User-Agent.
                using var request = new HttpRequestMessage(HttpMethod.Get, $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(city)}&format=json&limit=1");
                request.Headers.Add("User-Agent", "GFlowTournamentSystem/1.0");

                var response = await _httpClient.SendAsync(request);
                var results = await response.Content.ReadFromJsonAsync<List<NominatimResult>>();

                if (results != null && results.Any())
                {
                    var first = results[0];
                    return new GeoLocationResult
                    {
                        City = city,
                        Lat = double.Parse(first.lat, System.Globalization.CultureInfo.InvariantCulture),
                        Lng = double.Parse(first.lon, System.Globalization.CultureInfo.InvariantCulture)
                    };
                }
            }
            catch
            {
                // Fallback/log
            }
            return null;
        }

        private class NominatimResult
        {
            public string lat { get; set; } = "";
            public string lon { get; set; } = "";
        }

        private class IpApiResponse
        {
            public string? Status { get; set; }
            public string? CountryCode { get; set; }
            public string? City { get; set; }
            public double? Lat { get; set; }
            public double? Lon { get; set; }
        }
    }
}
