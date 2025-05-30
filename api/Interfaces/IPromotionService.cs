using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Promotion;

namespace api.Interfaces
{
    public interface IPromotionService
    {
        /// <summary>
        /// Lấy danh sách khuyến mãi đang hoạt động.
        /// </summary>
        /// <param name="fieldId">ID sân (tùy chọn).</param>
        /// <param name="code">Mã khuyến mãi (tùy chọn).</param>
        /// <param name="page">Số trang.</param>
        /// <param name="pageSize">Số mục mỗi trang.</param>
        /// <returns>Danh sách khuyến mãi với phân trang.</returns>
        Task<(IList<PromotionListDto> Data, int Total, int Page, int PageSize)> GetPromotionsAsync(
            int? fieldId, string? code, int page, int pageSize);

        /// <summary>
        /// Lấy chi tiết một khuyến mãi theo ID.
        /// </summary>
        /// <param name="promotionId">ID khuyến mãi.</param>
        /// <returns>Thông tin chi tiết khuyến mãi.</returns>
        Task<PromotionDto> GetPromotionByIdAsync(int promotionId);

        /// <summary>
        /// Tạo khuyến mãi mới (chỉ dành cho Owner).
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="createPromotionDto">Thông tin khuyến mãi cần tạo.</param>
        /// <returns>Thông tin khuyến mãi đã tạo.</returns>
        Task<PromotionDto> CreatePromotionAsync(ClaimsPrincipal user, CreatePromotionDto createPromotionDto);

        /// <summary>
        /// Cập nhật khuyến mãi (chỉ dành cho Owner).
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="promotionId">ID khuyến mãi.</param>
        /// <param name="updatePromotionDto">Thông tin khuyến mãi cần cập nhật.</param>
        /// <returns>Thông tin khuyến mãi đã cập nhật.</returns>
        Task<PromotionDto> UpdatePromotionAsync(ClaimsPrincipal user, int promotionId, UpdatePromotionDto updatePromotionDto);

        /// <summary>
        /// Xóa khuyến mãi (chỉ dành cho Owner, soft delete).
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="promotionId">ID khuyến mãi.</param>
        Task DeletePromotionAsync(ClaimsPrincipal user, int promotionId);

        /// <summary>
        /// Áp dụng mã khuyến mãi cho đơn đặt sân (dành cho User).
        /// </summary>
        /// <param name="user">Thông tin người dùng từ ClaimsPrincipal.</param>
        /// <param name="applyPromotionDto">Thông tin mã khuyến mãi và đơn đặt sân.</param>
        /// <returns>Kết quả sau khi áp dụng khuyến mãi.</returns>
        Task<ApplyPromotionResponseDto> ApplyPromotionAsync(ClaimsPrincipal user, ApplyPromotionDto applyPromotionDto);
    }
}