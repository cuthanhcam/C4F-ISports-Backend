using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Field;
using api.Utils;

namespace api.Interfaces
{
    /// <summary>
    /// Interface cung cấp các dịch vụ quản lý sân thể thao.
    /// </summary>
    public interface IFieldService
    {
        /// <summary>
        /// Lấy danh sách sân với các bộ lọc và sắp xếp.
        /// </summary>
        /// <param name="filter">Bộ lọc tìm kiếm sân.</param>
        /// <returns>Danh sách sân phân trang.</returns>
        Task<PagedResult<FieldResponseDto>> GetFieldsAsync(FieldFilterDto filter);

        /// <summary>
        /// Lấy thông tin chi tiết của một sân theo ID.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="include">Danh sách dữ liệu liên quan cần bao gồm.</param>
        /// <returns>Thông tin sân.</returns>
        Task<FieldResponseDto> GetFieldByIdAsync(int fieldId, string include);

        
        Task<PagedResult<OwnerFieldResponseDto>> GetOwnerFieldsAsync(OwnerFieldFilterDto filter, ClaimsPrincipal user);

        /// <summary>
        /// Xác thực địa chỉ của sân.
        /// </summary>
        /// <param name="dto">Thông tin địa chỉ cần xác thực.</param>
        /// <returns>Kết quả xác thực địa chỉ.</returns>
        Task<ValidateAddressResponseDto> ValidateAddressAsync(ValidateAddressDto dto);

        /// <summary>
        /// Tạo mới một sân.
        /// </summary>
        /// <param name="dto">Thông tin sân cần tạo.</param>
        /// <param name="user">Thông tin người dùng đang đăng nhập.</param>
        /// <returns>Thông tin sân vừa tạo.</returns>
        Task<FieldResponseDto> CreateFieldAsync(CreateFieldDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Tải lên hình ảnh cho sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="dto">Thông tin hình ảnh.</param>
        /// <param name="user">Thông tin người dùng đang đăng nhập.</param>
        /// <returns>Thông tin hình ảnh vừa tải lên.</returns>
        Task<FieldImageResponseDto> UploadFieldImageAsync(int fieldId, UploadFieldImageDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Cập nhật thông tin sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="dto">Thông tin cập nhật.</param>
        /// <param name="user">Thông tin người dùng đang đăng nhập.</param>
        /// <returns>Thông tin sân đã cập nhật.</returns>
        Task<FieldResponseDto> UpdateFieldAsync(int fieldId, UpdateFieldDto dto, ClaimsPrincipal user);

        /// <summary>
        /// Xóa mềm một sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="user">Thông tin người dùng đang đăng nhập.</param>
        /// <returns>Thông tin sân đã xóa.</returns>
        Task<DeleteFieldResponseDto> DeleteFieldAsync(int fieldId, ClaimsPrincipal user);

        /// <summary>
        /// Lấy danh sách khung giờ trống của sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="filter">Bộ lọc khung giờ.</param>
        /// <returns>Danh sách khung giờ trống.</returns>
        Task<List<AvailabilityResponseDto>> GetFieldAvailabilityAsync(int fieldId, AvailabilityFilterDto filter);

        /// <summary>
        /// Lấy danh sách đánh giá của sân.
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="filter">Bộ lọc đánh giá.</param>
        /// <returns>Danh sách đánh giá phân trang.</returns>
        Task<PagedResult<ReviewResponseDto>> GetFieldReviewsAsync(int fieldId, ReviewFilterDto filter);

        /// <summary>
        /// Lấy danh sách đặt sân của một sân (chỉ dành cho Owner).
        /// </summary>
        /// <param name="fieldId">ID của sân.</param>
        /// <param name="filter">Bộ lọc đặt sân.</param>
        /// <param name="user">Thông tin người dùng đang đăng nhập.</param>
        /// <returns>Danh sách đặt sân phân trang.</returns>
        Task<PagedResult<BookingResponseDto>> GetFieldBookingsAsync(int fieldId, BookingFilterDto filter, ClaimsPrincipal user);
    }
}