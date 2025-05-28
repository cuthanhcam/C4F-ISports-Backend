using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Interfaces
{
    public interface ICloudinaryService
    {
        // Task<string> UploadImageAsync(IFormFile file);
        Task<ImageUploadResponse> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);
    }
    public class ImageUploadResponse
    {
        public string Url { get; set; }
        public string PublicId { get; set; }
    }
}