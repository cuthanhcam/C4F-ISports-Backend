using System.Security.Claims;
using api.Dtos.Sport;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace api.Interfaces
{
    public interface ISportService
    {
        /// <summary>
        /// Lấy danh sách tất cả các môn thể thao đang hoạt động.
        /// </summary>
        /// <param name="keyword">Tìm kiếm theo tên môn thể thao.</param>
        /// <param name="sort">Sắp xếp theo trường (SportName:asc/desc, CreatedAt:asc/desc).</param>
        /// <param name="page">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Số mục mỗi trang (mặc định: 10).</param>
        /// <returns>Danh sách môn thể thao với phân trang.</returns>
        Task<(IList<SportDto> Data, int Total, int Page, int PageSize)> GetAllSportsAsync(
            string? keyword, string? sort, int page, int pageSize);

        /// <summary>
        /// Lấy thông tin chi tiết của một môn thể thao theo ID.
        /// </summary>
        /// <param name="sportId">ID của môn thể thao.</param>
        /// <returns>Thông tin chi tiết của môn thể thao.</returns>
        Task<SportDto> GetSportByIdAsync(int sportId);

        /// <summary>
        /// Tạo một môn thể thao mới.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="createSportDto">Thông tin môn thể thao cần tạo.</param>
        /// <returns>Thông tin môn thể thao vừa tạo.</returns>
        Task<SportDto> CreateSportAsync(ClaimsPrincipal user, CreateSportDto createSportDto);

        /// <summary>
        /// Cập nhật thông tin môn thể thao.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="sportId">ID của môn thể thao cần cập nhật.</param>
        /// <param name="updateSportDto">Thông tin cần cập nhật.</param>
        /// <returns>Thông tin môn thể thao đã cập nhật.</returns>
        Task<SportDto> UpdateSportAsync(ClaimsPrincipal user, int sportId, UpdateSportDto updateSportDto);

        /// <summary>
        /// Xóa mềm một môn thể thao.
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="sportId">ID của môn thể thao cần xóa.</param>
        Task DeleteSportAsync(ClaimsPrincipal user, int sportId);

        Task<string> UploadSportIconAsync(ClaimsPrincipal user, int sportId, IFormFile file);
    }
}