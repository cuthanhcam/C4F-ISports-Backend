using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Sport;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace api.Services
{
    public class SportService : ISportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SportService> _logger;
        private readonly IAuthService _authService;
        private readonly ICloudinaryService _cloudinaryService;

        public SportService(
            IUnitOfWork unitOfWork,
            ILogger<SportService> logger,
            IAuthService authService,
            ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _authService = authService;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<(IList<SportDto> Data, int Total, int Page, int PageSize)> GetAllSportsAsync(
            string? keyword, string? sort, int page, int pageSize)
        {
            _logger.LogInformation("Lấy danh sách môn thể thao");
            IQueryable<Sport> query = _unitOfWork.Repository<Sport>()
                .FindQueryable(s => s.DeletedAt == null);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => s.SportName.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(sort))
            {
                var sortParts = sort.Split(':');
                var sortField = sortParts[0];
                var sortDirection = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

                if (sortField == "SportName")
                {
                    query = sortDirection == "desc"
                        ? query.OrderByDescending(s => s.SportName)
                        : query.OrderBy(s => s.SportName);
                }
                else if (sortField == "CreatedAt")
                {
                    query = sortDirection == "desc"
                        ? query.OrderByDescending(s => s.CreatedAt)
                        : query.OrderBy(s => s.CreatedAt);
                }
            }
            else
            {
                query = query.OrderBy(s => s.SportName);
            }

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SportDto
                {
                    SportId = s.SportId,
                    SportName = s.SportName,
                    Description = s.Description,
                    ImageUrl = s.ImageUrl,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation("Lấy danh sách môn thể thao thành công, Total: {Total}", total);
            return (data, total, page, pageSize);
        }

        public async Task<SportDto> GetSportByIdAsync(int sportId)
        {
            _logger.LogInformation("Lấy thông tin môn thể thao, SportId: {SportId}", sportId);
            var sport = await _unitOfWork.Repository<Sport>()
                .FindQueryable(s => s.SportId == sportId && s.DeletedAt == null)
                .Select(s => new SportDto
                {
                    SportId = s.SportId,
                    SportName = s.SportName,
                    Description = s.Description,
                    ImageUrl = s.ImageUrl,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (sport == null)
            {
                _logger.LogWarning("Không tìm thấy môn thể thao với SportId: {SportId}", sportId);
                throw new ArgumentException("Sport not found");
            }

            _logger.LogInformation("Lấy thông tin môn thể thao thành công, SportId: {SportId}", sportId);
            return sport;
        }

        public async Task<SportDto> CreateSportAsync(ClaimsPrincipal user, CreateSportDto createSportDto)
        {
            _logger.LogInformation("Tạo môn thể thao mới");
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "Admin")
            {
                _logger.LogWarning("Vai trò không hợp lệ để tạo môn thể thao: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only admins can create sports.");
            }

            var sport = new Sport
            {
                SportName = createSportDto.SportName,
                Description = createSportDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Sport>().AddAsync(sport);
            await _unitOfWork.SaveChangesAsync();

            var result = new SportDto
            {
                SportId = sport.SportId,
                SportName = sport.SportName,
                Description = sport.Description,
                ImageUrl = sport.ImageUrl,
                CreatedAt = sport.CreatedAt,
                Message = "Sport created successfully"
            };

            _logger.LogInformation("Tạo môn thể thao thành công, SportId: {SportId}", sport.SportId);
            return result;
        }

        public async Task<SportDto> UpdateSportAsync(ClaimsPrincipal user, int sportId, UpdateSportDto updateSportDto)
        {
            _logger.LogInformation("Cập nhật môn thể thao, SportId: {SportId}", sportId);
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "Admin")
            {
                _logger.LogWarning("Vai trò không hợp lệ để cập nhật môn thể thao: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only admins can update sports.");
            }

            var sport = await _unitOfWork.Repository<Sport>()
                .FindSingleAsync(s => s.SportId == sportId && s.DeletedAt == null);
            if (sport == null)
            {
                _logger.LogWarning("Không tìm thấy môn thể thao với SportId: {SportId}", sportId);
                throw new ArgumentException("Sport not found");
            }

            sport.SportName = updateSportDto.SportName;
            sport.Description = updateSportDto.Description;
            sport.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Sport>().Update(sport);
            await _unitOfWork.SaveChangesAsync();

            var result = new SportDto
            {
                SportId = sport.SportId,
                SportName = sport.SportName,
                Description = sport.Description,
                ImageUrl = sport.ImageUrl,
                CreatedAt = sport.CreatedAt,
                UpdatedAt = sport.UpdatedAt,
                Message = "Sport updated successfully"
            };

            _logger.LogInformation("Cập nhật môn thể thao thành công, SportId: {SportId}", sportId);
            return result;
        }

        public async Task DeleteSportAsync(ClaimsPrincipal user, int sportId)
        {
            _logger.LogInformation("Xóa môn thể thao, SportId: {SportId}", sportId);
            var account = await _authService.GetCurrentUserAsync(user);

            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "Admin")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xóa môn thể thao: {Role}", account.Role);
                throw new UnauthorizedAccessException("Only admins can delete sports.");
            }

            var sport = await _unitOfWork.Repository<Sport>()
                .FindSingleAsync(s => s.SportId == sportId && s.DeletedAt == null);
            if (sport == null)
            {
                _logger.LogWarning("Không tìm thấy môn thể thao với SportId: {SportId}", sportId);
                throw new ArgumentException("Sport not found");
            }

            sport.DeletedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Sport>().Update(sport);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Xóa môn thể thao thành công, SportId: {SportId}", sportId);
        }

        public async Task<string> UploadSportIconAsync(ClaimsPrincipal user, int sportId, IFormFile file)
        {
            _logger.LogInformation("Tải lên icon cho môn thể thao, SportId: {SportId}", sportId);

            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Invalid or missing token");
            }

            if (account.Role != "Admin")
            {
                _logger.LogWarning("Vai trò không hợp lệ để tải lên icon: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ admin mới được tải lên icon của sport.");
            }

            var sport = await _unitOfWork.Repository<Sport>()
                .FindSingleAsync(s => s.SportId == sportId && s.DeletedAt == null);
            if (sport == null)
            {
                _logger.LogWarning("Không tìm thấy môn thể thao với SportId: {SportId}", sportId);
                throw new ArgumentException("Sport not found");
            }

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File ảnh trống hoặc không hợp lệ");
                throw new ArgumentException("Vui lòng chọn file ảnh hợp lệ.");
            }

            // Kiểm tra định dạng file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Định dạng file không hợp lệ: {FileExtension}", fileExtension);
                throw new ArgumentException("Định dạng file không hỗ trợ. Chỉ chấp nhận JPG, JPEG, PNG và GIF.");
            }

            // Giới hạn kích thước file (5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                _logger.LogWarning("Kích thước file quá lớn: {FileSize} bytes", file.Length);
                throw new ArgumentException("Kích thước file không được vượt quá 5MB.");
            }

            try
            {
                // Tải ảnh lên Cloudinary
                var uploadResult = await _cloudinaryService.UploadImageAsync(file);
                string imageUrl = uploadResult.Url;

                // Cập nhật URL ảnh
                sport.ImageUrl = imageUrl;
                sport.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Sport>().Update(sport);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Tải lên icon thành công cho môn thể thao, SportId: {SportId}", sportId);
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lên icon cho môn thể thao");
                throw new Exception("Không thể tải lên icon: " + ex.Message);
            }
        }
    }
}