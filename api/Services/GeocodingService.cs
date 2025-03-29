using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using api.Interfaces;

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

        public async Task<(decimal latitude, decimal longitude)> GetCoordinatesFromAddressAsync(string address)
        {
            return await GetCoordinatesFromOpenCageAsync(address);
        }

        private async Task<(decimal latitude, decimal longitude)> GetCoordinatesFromOpenCageAsync(string address)
        {
            var apiKey = _configuration["Geocoding:OpenCageApiKey"];
            var baseUrl = _configuration["Geocoding:OpenCageGeocodingUrl"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogError("OpenCage API configuration is missing. Please check your configuration.");
                throw new Exception("Cấu hình API OpenCage không hợp lệ.");
            }

            _logger.LogInformation("Original address: {Address}", address);

            // Chuẩn hóa địa chỉ trước khi gọi API
            var addressParts = address.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            _logger.LogInformation("Address parts before normalization: {Parts}", string.Join(", ", addressParts));

            // Tạo địa chỉ tìm kiếm với các phần quan trọng
            var searchParts = new List<string>();
            
            // Thêm số nhà và tên đường
            var streetPart = addressParts.FirstOrDefault();
            if (!string.IsNullOrEmpty(streetPart))
            {
                searchParts.Add(streetPart);
            }

            // Thêm phường/xã
            var wardPart = addressParts.FirstOrDefault(p => p.Contains("Đông Hòa", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(wardPart))
            {
                searchParts.Add(wardPart);
            }

            // Thêm quận/huyện
            searchParts.Add("Dĩ An");

            // Thêm tỉnh/thành
            searchParts.Add("Bình Dương");

            // Thêm quốc gia
            searchParts.Add("Việt Nam");

            // Nối các phần địa chỉ bằng khoảng trắng
            var normalizedAddress = string.Join(" ", searchParts);
            _logger.LogInformation("Final normalized address: {Address}", normalizedAddress);

            // URL encode toàn bộ địa chỉ
            var encodedAddress = WebUtility.UrlEncode(normalizedAddress);
            
            var url = $"{baseUrl}?q={encodedAddress}&key={apiKey}&language=vi&countrycode=vn&no_annotations=1&limit=1";

            _logger.LogInformation("Final encoded URL: {Url}", url.Replace(apiKey, "[REDACTED]"));

            try
            {
                _logger.LogDebug("Sending HTTP GET request to OpenCage API...");
                var response = await _httpClient.GetAsync(url);
                _logger.LogDebug("Received response with status code: {StatusCode}", response.StatusCode);

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Raw response content: {Content}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenCage API returned non-success status code: {StatusCode}, Response: {Response}", 
                        response.StatusCode, content);
                    throw new HttpRequestException($"API trả về mã lỗi: {response.StatusCode}");
                }

                _logger.LogDebug("Deserializing response...");
                var result = JsonSerializer.Deserialize<OpenCageGeocodingResponse>(content);

                if (result == null)
                {
                    _logger.LogError("Failed to deserialize response. Response content: {Content}", content);
                    throw new Exception("Không thể xử lý phản hồi từ API.");
                }

                _logger.LogDebug("Response deserialized successfully. Status: {Code} - {Message}", 
                    result.Status?.Code, result.Status?.Message);

                if (result.Status != null && result.Status.Code != 200)
                {
                    _logger.LogError("OpenCage API returned error status: {Code} - {Message}", 
                        result.Status.Code, result.Status.Message);
                    throw new Exception($"API trả về lỗi: {result.Status.Message}");
                }

                if (result.Results == null || result.Results.Length == 0)
                {
                    _logger.LogWarning("No results found for address: {Address}", normalizedAddress);
                    throw new Exception($"Không tìm thấy tọa độ từ địa chỉ: {normalizedAddress}");
                }

                var firstResult = result.Results[0];
                _logger.LogDebug("First result: {Result}", JsonSerializer.Serialize(firstResult));

                if (firstResult.Geometry == null)
                {
                    _logger.LogError("Geometry is null in the first result. Full result: {Result}", 
                        JsonSerializer.Serialize(firstResult));
                    throw new Exception("Dữ liệu tọa độ không hợp lệ.");
                }

                var lat = firstResult.Geometry.Lat;
                var lng = firstResult.Geometry.Lng;

                _logger.LogInformation("Raw coordinates from API: Lat={Lat}, Lng={Lng}", lat, lng);

                // Chuyển đổi từ double sang decimal với độ chính xác phù hợp
                var coordinates = ((decimal)Math.Round(lat, 6), (decimal)Math.Round(lng, 6));
                _logger.LogDebug("Converted coordinates: Lat={Lat}, Lng={Lng}", coordinates.Item1, coordinates.Item2);

                return coordinates;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request to OpenCage failed for address: {Address}", normalizedAddress);
                throw;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to deserialize OpenCage response for address: {Address}", normalizedAddress);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OpenCage response for address: {Address}", normalizedAddress);
                throw;
            }
        }

        private class OpenCageGeocodingResponse
        {
            public OpenCageResult[] Results { get; set; }
            public OpenCageStatus Status { get; set; }
        }

        private class OpenCageResult
        {
            public OpenCageGeometry Geometry { get; set; }
            public OpenCageComponents Components { get; set; }
            public string Formatted { get; set; }
        }

        private class OpenCageGeometry
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        private class OpenCageComponents
        {
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
        }

        private class OpenCageStatus
        {
            public int Code { get; set; }
            public string Message { get; set; }
        }
    }
}