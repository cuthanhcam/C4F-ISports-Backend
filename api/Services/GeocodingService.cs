using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using api.Interfaces;
using Microsoft.Extensions.Logging;

namespace api.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeocodingService> _logger;

        public GeocodingService(IConfiguration configuration, HttpClient httpClient, ILogger<GeocodingService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(decimal latitude, decimal longitude)> GetCoordinatesFromAddressAsync(string fieldName, string address)
        {
            var query = NormalizeQuery(fieldName, address);
            try
            {
                return await GetCoordinatesFromOpenCageAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed with full query: {Query}. Retrying with address only: {Address}", query, address);
                return await GetCoordinatesFromOpenCageAsync(address);
            }
        }

        private string NormalizeQuery(string fieldName, string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                _logger.LogError("Address is null or empty");
                throw new ArgumentException("Địa chỉ không được để trống");
            }

            address = address.Trim();
            var query = string.IsNullOrWhiteSpace(fieldName) ? address : $"{fieldName}, {address}";

            if (!query.EndsWith(", Việt Nam", StringComparison.OrdinalIgnoreCase) &&
                !query.EndsWith(", Vietnam", StringComparison.OrdinalIgnoreCase))
            {
                query = $"{query}, Việt Nam";
            }

            _logger.LogInformation("Normalized query: {Query}", query);
            return query;
        }

        private async Task<(decimal latitude, decimal longitude)> GetCoordinatesFromOpenCageAsync(string query)
        {
            var apiKey = _configuration["Geocoding:OpenCageApiKey"];
            var baseUrl = _configuration["Geocoding:OpenCageGeocodingUrl"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogError("OpenCage API configuration is missing: ApiKey={ApiKey}, BaseUrl={BaseUrl}", apiKey, baseUrl);
                throw new Exception("Cấu hình API OpenCage không hợp lệ.");
            }

            var encodedQuery = WebUtility.UrlEncode(query);
            var url = $"{baseUrl}?q={encodedQuery}&key={apiKey}&language=vi&countrycode=vn&no_annotations=1&limit=1";
            _logger.LogInformation("Sending request to OpenCage: {Url}", url.Replace(apiKey, "[REDACTED]"));

            try
            {
                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Full response from OpenCage: StatusCode={StatusCode}, Content={Content}", response.StatusCode, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenCage API failed: StatusCode={StatusCode}, Response={Response}", response.StatusCode, content);
                    throw new HttpRequestException($"API trả về mã lỗi: {response.StatusCode}");
                }

                var result = JsonSerializer.Deserialize<OpenCageGeocodingResponse>(content);
                if (result == null)
                {
                    _logger.LogError("Deserialization failed: Result is null for query: {Query}", query);
                    throw new Exception($"Không thể phân tích phản hồi từ OpenCage cho: {query}");
                }

                if (result.Results == null || result.Results.Length == 0)
                {
                    _logger.LogWarning("No geocoding results found for query: {Query}. Full response: {Response}", query, content);
                    throw new Exception($"Không tìm thấy tọa độ cho: {query}");
                }

                var firstResult = result.Results[0];
                if (firstResult.Geometry == null)
                {
                    _logger.LogError("Geometry is null in first result for query: {Query}", query);
                    throw new Exception($"Không tìm thấy tọa độ trong kết quả cho: {query}");
                }

                var lat = (decimal)Math.Round(firstResult.Geometry.Lat, 6);
                var lng = (decimal)Math.Round(firstResult.Geometry.Lng, 6);
                _logger.LogInformation("Coordinates retrieved: Lat={Lat}, Lng={Lng}", lat, lng);

                return (lat, lng);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get coordinates for query: {Query}", query);
                throw;
            }
        }

        public class OpenCageGeocodingResponse
        {
            [JsonPropertyName("results")]
            public OpenCageResult[] Results { get; set; }

            [JsonPropertyName("status")]
            public OpenCageStatus Status { get; set; }
        }

        public class OpenCageResult
        {
            [JsonPropertyName("geometry")]
            public OpenCageGeometry Geometry { get; set; }

            [JsonPropertyName("components")]
            public Dictionary<string, object> Components { get; set; }

            [JsonPropertyName("formatted")]
            public string Formatted { get; set; }
        }

        public class OpenCageGeometry
        {
            [JsonPropertyName("lat")]
            public double Lat { get; set; }

            [JsonPropertyName("lng")]
            public double Lng { get; set; }
        }

        public class OpenCageStatus
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }
        }
    }
}