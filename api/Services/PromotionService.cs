using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Dtos.Promotion;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PromotionService> _logger;
        private readonly IAuthService _authService;

        public PromotionService(
            IUnitOfWork unitOfWork,
            ILogger<PromotionService> logger,
            IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _authService = authService;
        }

        public async Task<(IList<PromotionListDto> Data, int Total, int Page, int PageSize)> GetPromotionsAsync(
            int? fieldId, string? code, int page, int pageSize)
        {
            _logger.LogInformation("Lấy danh sách khuyến mãi");

            IQueryable<Promotion> query = _unitOfWork.Repository<Promotion>()
                .FindQueryable(p => p.IsActive && p.DeletedAt == null && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);

            if (fieldId.HasValue)
            {
                query = query.Where(p => p.FieldId == fieldId.Value);
            }

            if (!string.IsNullOrEmpty(code))
            {
                query = query.Where(p => p.Code == code);
            }

            query = query.OrderByDescending(p => p.StartDate);

            var total = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PromotionListDto
                {
                    PromotionId = p.PromotionId,
                    Code = p.Code ?? string.Empty,
                    Description = p.Description,
                    DiscountType = p.DiscountType ?? string.Empty,
                    DiscountValue = p.DiscountValue ?? 0,
                    StartDate = p.StartDate ?? DateTime.UtcNow,
                    EndDate = p.EndDate ?? DateTime.UtcNow,
                    MinBookingValue = p.MinBookingValue,
                    MaxDiscountAmount = p.MaxDiscountAmount,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            _logger.LogInformation("Lấy danh sách khuyến mãi thành công, tổng số: {Total}", total);
            return (data, total, page, pageSize);
        }

        public async Task<PromotionDto> GetPromotionByIdAsync(int promotionId)
        {
            _logger.LogInformation("Lấy chi tiết khuyến mãi với ID: {PromotionId}", promotionId);

            var promotion = await _unitOfWork.Repository<Promotion>()
                .FindSingleAsync(p => p.PromotionId == promotionId && p.DeletedAt == null);
            if (promotion == null)
            {
                _logger.LogWarning("Không tìm thấy khuyến mãi với ID: {PromotionId}", promotionId);
                throw new ArgumentException("Khuyến mãi không tồn tại.");
            }

            var promotionDto = new PromotionDto
            {
                PromotionId = promotion.PromotionId,
                FieldId = promotion.FieldId ?? 0,
                Code = promotion.Code ?? string.Empty,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType ?? string.Empty,
                DiscountValue = promotion.DiscountValue ?? 0,
                StartDate = promotion.StartDate ?? DateTime.UtcNow,
                EndDate = promotion.EndDate ?? DateTime.UtcNow,
                UsageLimit = promotion.UsageLimit,
                MinBookingValue = promotion.MinBookingValue,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                IsActive = promotion.IsActive,
                UsageCount = promotion.UsageCount
            };

            _logger.LogInformation("Lấy chi tiết khuyến mãi thành công với ID: {PromotionId}", promotionId);
            return promotionDto;
        }

        public async Task<PromotionDto> CreatePromotionAsync(ClaimsPrincipal user, CreatePromotionDto createPromotionDto)
        {
            _logger.LogInformation("Tạo khuyến mãi mới");

            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Token không hợp lệ hoặc đã hết hạn.");
            }

            if (account.Role != "Owner")
            {
                _logger.LogWarning("Vai trò không hợp lệ để tạo khuyến mãi: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ chủ sân mới có thể tạo khuyến mãi.");
            }

            var owner = await _unitOfWork.Repository<Owner>()
                .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
            if (owner == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin chủ sân không tồn tại.");
            }

            var field = await _unitOfWork.Repository<Field>()
                .FindSingleAsync(f => f.FieldId == createPromotionDto.FieldId && f.OwnerId == owner.OwnerId && f.DeletedAt == null);
            if (field == null)
            {
                _logger.LogWarning("Sân không tồn tại hoặc không thuộc chủ sân: FieldId={FieldId}", createPromotionDto.FieldId);
                throw new ArgumentException("Sân không tồn tại hoặc bạn không có quyền quản lý sân này.");
            }

            var existingPromotion = await _unitOfWork.Repository<Promotion>()
                .FindSingleAsync(p => p.Code == createPromotionDto.Code && p.DeletedAt == null);
            if (existingPromotion != null)
            {
                _logger.LogWarning("Mã khuyến mãi đã tồn tại: {Code}", createPromotionDto.Code);
                throw new ArgumentException("Mã khuyến mãi đã tồn tại.");
            }

            var promotion = new Promotion
            {
                FieldId = createPromotionDto.FieldId,
                Code = createPromotionDto.Code,
                Description = createPromotionDto.Description,
                DiscountType = createPromotionDto.DiscountType,
                DiscountValue = createPromotionDto.DiscountValue,
                StartDate = createPromotionDto.StartDate,
                EndDate = createPromotionDto.EndDate,
                UsageLimit = createPromotionDto.UsageLimit,
                MinBookingValue = createPromotionDto.MinBookingValue,
                MaxDiscountAmount = createPromotionDto.MaxDiscountAmount,
                IsActive = true,
                UsageCount = 0
            };

            await _unitOfWork.Repository<Promotion>().AddAsync(promotion);
            await _unitOfWork.SaveChangesAsync();

            var promotionDto = new PromotionDto
            {
                PromotionId = promotion.PromotionId,
                FieldId = promotion.FieldId ?? 0,
                Code = promotion.Code ?? string.Empty,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType ?? string.Empty,
                DiscountValue = promotion.DiscountValue ?? 0,
                StartDate = promotion.StartDate ?? DateTime.UtcNow,
                EndDate = promotion.EndDate ?? DateTime.UtcNow,
                UsageLimit = promotion.UsageLimit,
                MinBookingValue = promotion.MinBookingValue,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                IsActive = promotion.IsActive,
                UsageCount = promotion.UsageCount,
                Message = "Khuyến mãi được tạo thành công."
            };

            _logger.LogInformation("Tạo khuyến mãi thành công với ID: {PromotionId}", promotion.PromotionId);
            return promotionDto;
        }

        public async Task<PromotionDto> UpdatePromotionAsync(ClaimsPrincipal user, int promotionId, UpdatePromotionDto updatePromotionDto)
        {
            _logger.LogInformation("Cập nhật khuyến mãi với ID: {PromotionId}", promotionId);

            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Token không hợp lệ hoặc đã hết hạn.");
            }

            if (account.Role != "Owner")
            {
                _logger.LogWarning("Vai trò không hợp lệ để cập nhật khuyến mãi: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ chủ sân mới có thể cập nhật khuyến mãi.");
            }

            var owner = await _unitOfWork.Repository<Owner>()
                .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
            if (owner == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin chủ sân không tồn tại.");
            }

            var promotion = await _unitOfWork.Repository<Promotion>()
                .FindSingleAsync(p => p.PromotionId == promotionId && p.DeletedAt == null);
            if (promotion == null)
            {
                _logger.LogWarning("Không tìm thấy khuyến mãi với ID: {PromotionId}", promotionId);
                throw new ArgumentException("Khuyến mãi không tồn tại.");
            }

            var field = await _unitOfWork.Repository<Field>()
                .FindSingleAsync(f => f.FieldId == promotion.FieldId && f.OwnerId == owner.OwnerId && f.DeletedAt == null);
            if (field == null)
            {
                _logger.LogWarning("Sân không thuộc chủ sân: FieldId={FieldId}", promotion.FieldId);
                throw new ArgumentException("Bạn không có quyền cập nhật khuyến mãi này.");
            }

            var existingPromotion = await _unitOfWork.Repository<Promotion>()
                .FindSingleAsync(p => p.Code == updatePromotionDto.Code && p.PromotionId != promotionId && p.DeletedAt == null);
            if (existingPromotion != null)
            {
                _logger.LogWarning("Mã khuyến mãi đã tồn tại: {Code}", updatePromotionDto.Code);
                throw new ArgumentException("Mã khuyến mãi đã tồn tại.");
            }

            promotion.Code = updatePromotionDto.Code;
            promotion.Description = updatePromotionDto.Description;
            promotion.DiscountType = updatePromotionDto.DiscountType;
            promotion.DiscountValue = updatePromotionDto.DiscountValue;
            promotion.StartDate = updatePromotionDto.StartDate;
            promotion.EndDate = updatePromotionDto.EndDate;
            promotion.UsageLimit = updatePromotionDto.UsageLimit;
            promotion.MinBookingValue = updatePromotionDto.MinBookingValue;
            promotion.MaxDiscountAmount = updatePromotionDto.MaxDiscountAmount;

            _unitOfWork.Repository<Promotion>().Update(promotion);
            await _unitOfWork.SaveChangesAsync();

            var promotionDto = new PromotionDto
            {
                PromotionId = promotion.PromotionId,
                FieldId = promotion.FieldId ?? 0,
                Code = promotion.Code ?? string.Empty,
                Description = promotion.Description,
                DiscountType = promotion.DiscountType ?? string.Empty,
                DiscountValue = promotion.DiscountValue ?? 0,
                StartDate = promotion.StartDate ?? DateTime.UtcNow,
                EndDate = promotion.EndDate ?? DateTime.UtcNow,
                UsageLimit = promotion.UsageLimit,
                MinBookingValue = promotion.MinBookingValue,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                IsActive = promotion.IsActive,
                UsageCount = promotion.UsageCount,
                Message = "Khuyến mãi được cập nhật thành công."
            };

            _logger.LogInformation("Cập nhật khuyến mãi thành công với ID: {PromotionId}", promotionId);
            return promotionDto;
        }

        public async Task DeletePromotionAsync(ClaimsPrincipal user, int promotionId)
        {
            _logger.LogInformation("Xóa khuyến mãi với ID: {PromotionId}", promotionId);

            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Token không hợp lệ hoặc đã hết hạn.");
            }

            if (account.Role != "Owner")
            {
                _logger.LogWarning("Vai trò không hợp lệ để xóa khuyến mãi: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ chủ sân mới có thể xóa khuyến mãi.");
            }

            var owner = await _unitOfWork.Repository<Owner>()
                .FindSingleAsync(o => o.AccountId == account.AccountId && o.DeletedAt == null);
            if (owner == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin chủ sân cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin chủ sân không tồn tại.");
            }

            var promotion = await _unitOfWork.Repository<Promotion>()
                .FindSingleAsync(p => p.PromotionId == promotionId && p.DeletedAt == null);
            if (promotion == null)
            {
                _logger.LogWarning("Không tìm thấy khuyến mãi với ID: {PromotionId}", promotionId);
                throw new ArgumentException("Khuyến mãi không tồn tại.");
            }

            var field = await _unitOfWork.Repository<Field>()
                .FindSingleAsync(f => f.FieldId == promotion.FieldId && f.OwnerId == owner.OwnerId && f.DeletedAt == null);
            if (field == null)
            {
                _logger.LogWarning("Sân không thuộc chủ sân: FieldId={FieldId}", promotion.FieldId);
                throw new ArgumentException("Bạn không có quyền xóa khuyến mãi này.");
            }

            promotion.DeletedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Promotion>().Update(promotion);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Xóa khuyến mãi thành công với ID: {PromotionId}", promotionId);
        }

        public async Task<ApplyPromotionResponseDto> ApplyPromotionAsync(ClaimsPrincipal user, ApplyPromotionDto applyPromotionDto)
        {
            _logger.LogInformation("Áp dụng mã khuyến mãi: {Code} cho BookingId: {BookingId}", applyPromotionDto.Code, applyPromotionDto.BookingId);

            var account = await _authService.GetCurrentUserAsync(user);
            if (account == null)
            {
                _logger.LogWarning("Không tìm thấy tài khoản người dùng");
                throw new UnauthorizedAccessException("Token không hợp lệ hoặc đã hết hạn.");
            }

            if (account.Role != "User")
            {
                _logger.LogWarning("Vai trò không hợp lệ để áp dụng khuyến mãi: {Role}", account.Role);
                throw new UnauthorizedAccessException("Chỉ người dùng mới có thể áp dụng khuyến mãi.");
            }

            var userEntity = await _unitOfWork.Repository<User>()
                .FindSingleAsync(u => u.AccountId == account.AccountId && u.DeletedAt == null);
            if (userEntity == null)
            {
                _logger.LogWarning("Không tìm thấy thông tin người dùng cho AccountId: {AccountId}", account.AccountId);
                throw new ArgumentException("Thông tin người dùng không tồn tại.");
            }

            var booking = await _unitOfWork.Repository<Booking>()
                .FindSingleAsync(b => b.BookingId == applyPromotionDto.BookingId && b.UserId == userEntity.UserId && b.DeletedAt == null);
            if (booking == null)
            {
                _logger.LogWarning("Không tìm thấy đơn đặt sân với BookingId: {BookingId}", applyPromotionDto.BookingId);
                throw new ArgumentException("Đơn đặt sân không tồn tại hoặc không thuộc người dùng này.");
            }

            var promotion = await _unitOfWork.Repository<Promotion>()
                .FindSingleAsync(p => p.Code == applyPromotionDto.Code && p.IsActive && p.DeletedAt == null && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);
            if (promotion == null)
            {
                _logger.LogWarning("Mã khuyến mãi không hợp lệ hoặc đã hết hạn: {Code}", applyPromotionDto.Code);
                throw new ArgumentException("Mã khuyến mãi không hợp lệ hoặc đã hết hạn.");
            }

            if (promotion.FieldId != booking.SubField.FieldId)
            {
                _logger.LogWarning("Mã khuyến mãi không áp dụng cho sân này: FieldId={FieldId}", booking.SubField.FieldId);
                throw new ArgumentException("Mã khuyến mãi không áp dụng cho sân này.");
            }

            if (promotion.UsageLimit.HasValue && promotion.UsageCount >= promotion.UsageLimit.Value)
            {
                _logger.LogWarning("Mã khuyến mãi đã đạt giới hạn sử dụng: {Code}", applyPromotionDto.Code);
                throw new ArgumentException("Mã khuyến mãi đã đạt giới hạn sử dụng.");
            }

            if (promotion.MinBookingValue.HasValue && booking.TotalPrice < promotion.MinBookingValue.Value)
            {
                _logger.LogWarning("Đơn đặt sân không đạt giá trị tối thiểu: TotalPrice={TotalPrice}, MinBookingValue={MinBookingValue}", booking.TotalPrice, promotion.MinBookingValue.Value);
                throw new ArgumentException("Đơn đặt sân không đạt giá trị tối thiểu để áp dụng khuyến mãi.");
            }

            decimal discount = 0;
            if (promotion.DiscountType == "Percentage")
            {
                discount = booking.TotalPrice * (promotion.DiscountValue ?? 0) / 100;
                if (promotion.MaxDiscountAmount.HasValue && discount > promotion.MaxDiscountAmount.Value)
                {
                    discount = promotion.MaxDiscountAmount.Value;
                }
            }
            else if (promotion.DiscountType == "Fixed")
            {
                discount = promotion.DiscountValue ?? 0;
            }

            var newTotalPrice = booking.TotalPrice - discount;
            if (newTotalPrice < 0)
            {
                newTotalPrice = 0;
            }

            booking.PromotionId = promotion.PromotionId;
            promotion.UsageCount++;
            _unitOfWork.Repository<Booking>().Update(booking);
            _unitOfWork.Repository<Promotion>().Update(promotion);
            await _unitOfWork.SaveChangesAsync();

            var response = new ApplyPromotionResponseDto
            {
                BookingId = booking.BookingId,
                PromotionId = promotion.PromotionId,
                Discount = discount,
                NewTotalPrice = newTotalPrice,
                Message = "Khuyến mãi được áp dụng thành công."
            };

            _logger.LogInformation("Áp dụng mã khuyến mãi thành công: {Code} cho BookingId: {BookingId}", applyPromotionDto.Code, applyPromotionDto.BookingId);
            return response;
        }
    }
}