using api.Interfaces;
using api.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace api.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> config, ILogger<CloudinaryService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (config?.Value == null)
            {
                _logger.LogError("CloudinarySettings is not configured in appsettings.json.");
                throw new ArgumentNullException(nameof(config), "CloudinarySettings is not configured.");
            }

            try
            {
                if (string.IsNullOrEmpty(config.Value.CloudName) || 
                    string.IsNullOrEmpty(config.Value.ApiKey) || 
                    string.IsNullOrEmpty(config.Value.ApiSecret))
                {
                    _logger.LogError("CloudinarySettings is missing required fields (CloudName, ApiKey, or ApiSecret).");
                    throw new ArgumentException("CloudinarySettings is missing required fields.");
                }

                var account = new CloudinaryDotNet.Account(
                    config.Value.CloudName,
                    config.Value.ApiKey,
                    config.Value.ApiSecret);

                _cloudinary = new Cloudinary(account);
                _cloudinary.Api.Secure = true;
                _logger.LogInformation("CloudinaryService initialized with CloudName: {CloudName}", config.Value.CloudName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Cloudinary. StackTrace: {StackTrace}", ex.StackTrace);
                throw new Exception("Failed to initialize Cloudinary.", ex);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogError("UploadImageAsync: File is null or empty.");
                throw new ArgumentException("File cannot be null or empty.", nameof(file));
            }

            if (_cloudinary == null)
            {
                _logger.LogError("Cloudinary instance is not initialized.");
                throw new InvalidOperationException("Cloudinary instance is not initialized.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fit")
                };

                _logger.LogInformation("Uploading image {FileName} to Cloudinary...", file.FileName);
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult?.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error.Message);
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
                }

                if (uploadResult.SecureUrl == null)
                {
                    _logger.LogError("Cloudinary upload returned null URL for {FileName}.", file.FileName);
                    throw new Exception("Cloudinary upload returned null URL.");
                }

                _logger.LogInformation("Image {FileName} uploaded successfully. URL: {Url}", file.FileName, uploadResult.SecureUrl.ToString());
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image {FileName}. StackTrace: {StackTrace}", file.FileName, ex.StackTrace);
                throw new Exception($"Failed to upload image {file.FileName}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogError("DeleteImageAsync: PublicId is null or empty.");
                throw new ArgumentException("PublicId cannot be null or empty.", nameof(publicId));
            }

            try
            {
                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);
                if (result.Result == "ok")
                {
                    _logger.LogInformation("Image {PublicId} deleted successfully.", publicId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to delete image {PublicId}: {Result}", publicId, result.Result);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image {PublicId}. StackTrace: {StackTrace}", publicId, ex.StackTrace);
                return false;
            }
        }
    }
}