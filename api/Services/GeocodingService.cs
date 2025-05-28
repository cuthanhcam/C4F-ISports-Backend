using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using api.Interfaces;
using Microsoft.Extensions.Logging;
using api.Dtos.Field;

namespace api.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeocodingService> _logger;

        public GeocodingService(IConfiguration configuration, HttpClient httpClient, ILogger<GeocodingService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ValidateAddressResponseDto> ValidateAddressAsync(ValidateAddressDto addressDto)
        {
            var query = NormalizeQuery(addressDto);
            try
            {
                return await GetCoordinatesFromOpenCageAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed with full query: {Query}. Retrying with address only: {Address}", query, addressDto.Address);
                try
                {
                    return await GetCoordinatesFromOpenCageAsync(addressDto.Address);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to validate address for query: {Query}", addressDto.Address);
                    return new ValidateAddressResponseDto
                    {
                        IsValid = false,
                        FormattedAddress = addressDto.Address,
                        Latitude = 0,
                        Longitude = 0
                    };
                }
            }
        }

        private string NormalizeQuery(ValidateAddressDto addressDto)
        {
            if (string.IsNullOrWhiteSpace(addressDto.Address))
            {
                _logger.LogError("Address is null or empty");
                throw new ArgumentException("Địa chỉ không được để trống");
            }

            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(addressDto.FieldName))
                queryParts.Add(addressDto.FieldName.Trim());
            queryParts.Add(addressDto.Address.Trim());
            if (!string.IsNullOrWhiteSpace(addressDto.District))
                queryParts.Add(addressDto.District.Trim());
            if (!string.IsNullOrWhiteSpace(addressDto.City))
                queryParts.Add(addressDto.City.Trim());

            var query = string.Join(", ", queryParts);

            if (!query.EndsWith(", Việt Nam", StringComparison.OrdinalIgnoreCase) &&
                !query.EndsWith(", Vietnam", StringComparison.OrdinalIgnoreCase))
            {
                query = $"{query}, Việt Nam";
            }

            _logger.LogInformation("Normalized query: {Query}", query);
            return query;
        }

        private async Task<ValidateAddressResponseDto> GetCoordinatesFromOpenCageAsync(string query)
        {
            var apiKey = _configuration["Geocoding:OpenCageApiKey"];
            var baseUrl = _configuration["Geocoding:OpenCageGeocodingUrl"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogError("OpenCage API configuration is missing: ApiKey={ApiKey}, BaseUrl={BaseUrl}", apiKey, baseUrl);
                throw new InvalidOperationException("Cấu hình API OpenCage không hợp lệ.");
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
                if (result == null || result.Results == null || result.Results.Length == 0)
                {
                    _logger.LogWarning("No geocoding results found for query: {Query}. Full response: {Response}", query, content);
                    return new ValidateAddressResponseDto
                    {
                        IsValid = false,
                        FormattedAddress = query,
                        Latitude = 0,
                        Longitude = 0
                    };
                }

                var firstResult = result.Results[0];
                if (firstResult.Geometry == null)
                {
                    _logger.LogError("Geometry is null in first result for query: {Query}", query);
                    return new ValidateAddressResponseDto
                    {
                        IsValid = false,
                        FormattedAddress = query,
                        Latitude = 0,
                        Longitude = 0
                    };
                }

                var lat = firstResult.Geometry.Lat;
                var lng = firstResult.Geometry.Lng;
                var formattedAddress = firstResult.Formatted ?? query;

                _logger.LogInformation("Address validated: Lat={Lat}, Lng={Lng}, FormattedAddress={FormattedAddress}", lat, lng, formattedAddress);

                return new ValidateAddressResponseDto
                {
                    IsValid = true,
                    Latitude = lat,
                    Longitude = lng,
                    FormattedAddress = formattedAddress
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate address for query: {Query}", query);
                throw;
            }
        }

        public class OpenCageGeocodingResponse
        {
            [JsonPropertyName("results")]
            public OpenCageResult[] Results { get; set; } = Array.Empty<OpenCageResult>();

            [JsonPropertyName("status")]
            public OpenCageStatus Status { get; set; } = new OpenCageStatus();
        }

        public class OpenCageResult
        {
            [JsonPropertyName("geometry")]
            public OpenCageGeometry Geometry { get; set; } = new OpenCageGeometry();

            [JsonPropertyName("components")]
            public Dictionary<string, object> Components { get; set; } = new Dictionary<string, object>();

            [JsonPropertyName("formatted")]
            public string Formatted { get; set; } = string.Empty;
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
            public string Message { get; set; } = string.Empty;
        }
    }
}