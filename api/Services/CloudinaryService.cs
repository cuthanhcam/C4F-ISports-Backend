using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace api.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudinarySettings = configuration.GetSection("CloudinarySettings");
            var account = new Account(
                cloudinarySettings["CloudName"],
                cloudinarySettings["ApiKey"],
                cloudinarySettings["ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        // Upload hình ảnh từ IFormFile
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Transformation = new Transformation().Width(800).Height(800).Crop("limit") // Tùy chỉnh kích thước
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString(); // Trả về URL an toàn
        }

        // Xóa hình ảnh theo publicId
        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }
    }
}