# Database Schema C4F-ISports v2.0.0

## 1. Tổng Quan
Database sử dụng SQL Server, hỗ trợ quản lý sân lớn chứa nhiều sân nhỏ, giá thuê linh hoạt theo khung giờ, tích hợp OAuth2, gợi ý sân, và điểm thưởng. Schema gồm 17 bảng, bao gồm `SubField` để quản lý sân nhỏ.

## 2. Bảng và Mô Tả

1. **Account**
   - **Mô tả**: Quản lý thông tin đăng nhập và phân quyền, hỗ trợ OAuth2.
   - **Cột**:
     - `AccountId` (int, PK)
     - `Email` (nvarchar(255), UNIQUE)
     - `Password` (nvarchar(255))
     - `Role` (nvarchar(20))
     - `IsActive` (bit)
     - `CreatedAt` (datetime)
     - `LastLogin` (datetime, nullable)
     - `OAuthProvider` (nvarchar(50), nullable)
     - `OAuthId` (nvarchar(100), nullable)
     - `AccessToken` (nvarchar(255), nullable)
     - `RefreshToken` (nvarchar(255), nullable)
     - `TokenExpiry` (datetime, nullable)

2. **User**
   - **Mô tả**: Thông tin người dùng.
   - **Cột**:
     - `UserId` (int, PK)
     - `AccountId` (int, FK)
     - `FullName` (nvarchar(100))
     - `Email` (nvarchar(255), UNIQUE)
     - `Phone` (nvarchar(20))
     - `Gender` (nvarchar(10), nullable)
     - `DateOfBirth` (date, nullable)
     - `AvatarUrl` (nvarchar(255), nullable)
     - `LoyaltyPoints` (decimal(10,2))

3. **Owner**
   - **Mô tả**: Thông tin chủ sân.
   - **Cột**:
     - `OwnerId` (int, PK)
     - `AccountId` (int, FK)
     - `FullName` (nvarchar(100))
     - `Phone` (nvarchar(20))
     - `Email` (nvarchar(255))
     - `CreatedAt` (datetime)
     - `UpdatedAt` (datetime)

4. **Sport**
   - **Mô tả**: Loại thể thao.
   - **Cột**:
     - `SportId` (int, PK)
     - `SportName` (nvarchar(50))

5. **Field**
   - **Mô tả**: Thông tin sân lớn.
   - **Cột**:
     - `FieldId` (int, PK)
     - `SportId` (int, FK)
     - `FieldName` (nvarchar(100))
     - `Phone` (nvarchar(20))
     - `Address` (nvarchar(255))
     - `OpenHours` (nvarchar(100))
     - `OwnerId` (int, FK)
     - `Status` (nvarchar(20))
     - `Latitude` (decimal(9,6))
     - `Longitude` (decimal(9,6))
     - `CreatedAt` (datetime)
     - `UpdatedAt` (datetime)

6. **SubField**
   - **Mô tả**: Thông tin sân nhỏ thuộc sân lớn.
   - **Cột**:
     - `SubFieldId` (int, PK)
     - `FieldId` (int, FK)
     - `SubFieldName` (nvarchar(100))
     - `FieldType` (nvarchar(50))
     - `Status` (nvarchar(20))

7. **FieldDescription**
   - **Mô tả**: Mô tả chi tiết sân lớn.
   - **Cột**:
     - `FieldDescriptionId` (int, PK)
     - `FieldId` (int, FK)
     - `Description` (nvarchar(MAX))

8. **FieldImage**
   - **Mô tả**: Hình ảnh sân lớn.
   - **Cột**:
     - `FieldImageId` (int, PK)
     - `FieldId` (int, FK)
     - `ImageUrl` (nvarchar(MAX))

9. **FieldAmenity**
   - **Mô tả**: Tiện ích sân lớn.
   - **Cột**:
     - `FieldAmenityId` (int, PK)
     - `FieldId` (int, FK)
     - `AmenityName` (nvarchar(100))

10. **FieldPricing**
    - **Mô tả**: Giá thuê sân nhỏ theo khung giờ và ngày.
    - **Cột**:
      - `FieldPricingId` (int, PK)
      - `SubFieldId` (int, FK)
      - `StartTime` (time)
      - `EndTime` (time)
      - `DayOfWeek` (int)
      - `Price` (decimal(10,2))

11. **FieldService**
    - **Mô tả**: Dịch vụ đi kèm sân lớn.
    - **Cột**:
      - `FieldServiceId` (int, PK)
      - `FieldId` (int, FK)
      - `ServiceName` (nvarchar(100))
      - `Price` (decimal(10,2))

12. **Promotion**
    - **Mô tả**: Khuyến mãi.
    - **Cột**:
      - `PromotionId` (int, PK)
      - `Code` (nvarchar(50))
      - `Description` (nvarchar(255))
      - `DiscountType` (nvarchar(20))
      - `DiscountValue` (decimal(10,2))
      - `StartDate` (datetime)
      - `EndDate` (datetime)
      - `MinBookingValue` (decimal(10,2))
      - `MaxDiscountAmount` (decimal(10,2))
      - `IsActive` (bit)

13. **Booking**
    - **Mô tả**: Đơn đặt sân nhỏ.
    - **Cột**:
      - `BookingId` (int, PK)
      - `UserId` (int, FK)
      - `SubFieldId` (int, FK)
      - `BookingDate` (date)
      - `StartTime` (time)
      - `EndTime` (time)
      - `TotalPrice` (decimal(10,2))
      - `Status` (nvarchar(20))
      - `PaymentStatus` (nvarchar(20))
      - `CreatedAt` (datetime)
      - `UpdatedAt` (datetime)
      - `PromotionId` (int, FK, nullable)

14. **BookingService**
    - **Mô tả**: Dịch vụ đi kèm đơn đặt sân.
    - **Cột**:
      - `BookingServiceId` (int, PK)
      - `BookingId` (int, FK)
      - `FieldServiceId` (int, FK)
      - `Quantity` (int)
      - `Price` (decimal(10,2))

15. **Payment**
    - **Mô tả**: Thanh toán.
    - **Cột**:
      - `PaymentId` (int, PK)
      - `BookingId` (int, FK)
      - `Amount` (decimal(10,2))
      - `PaymentMethod` (nvarchar(50))
      - `TransactionId` (nvarchar(100))
      - `Status` (nvarchar(20))
      - `CreatedAt` (datetime)

16. **Review**
    - **Mô tả**: Đánh giá sân lớn.
    - **Cột**:
      - `ReviewId` (int, PK)
      - `UserId` (int, FK)
      - `FieldId` (int, FK)
      - `Rating` (int)
      - `Comment` (nvarchar(MAX))
      - `CreatedAt` (datetime)
      - `OwnerReply` (nvarchar(MAX), nullable)
      - `ReplyDate` (datetime, nullable)

17. **Notification**
    - **Mô tả**: Thông báo.
    - **Cột**:
      - `NotificationId` (int, PK)
      - `UserId` (int, FK)
      - `Title` (nvarchar(100))
      - `Content` (nvarchar(MAX))
      - `IsRead` (bit)
      - `CreatedAt` (datetime)

18. **FavoriteField**
    - **Mô tả**: Sân lớn yêu thích.
    - **Cột**:
      - `FavoriteId` (int, PK)
      - `UserId` (int, FK)
      - `FieldId` (int, FK)
      - `AddedDate` (datetime)

19. **SearchHistory**
    - **Mô tả**: Lịch sử tìm kiếm sân lớn.
    - **Cột**:
      - `SearchHistoryId` (int, PK)
      - `UserId` (int, FK)
      - `SearchQuery` (nvarchar(255))
      - `SearchDate` (datetime)
      - `FieldId` (int, FK, nullable)

## 3. Mô Hình Quan Hệ
- `Account` (1) ↔ (0..1) `User`
- `Account` (1) ↔ (0..1) `Owner`
- `Owner` (1) ↔ (0..n) `Field`
- `Sport` (1) ↔ (0..n) `Field`
- `Field` (1) ↔ (0..n) `SubField`
- `Field` (1) ↔ (0..n) `FieldDescription`
- `Field` (1) ↔ (0..n) `FieldImage`
- `Field` (1) ↔ (0..n) `FieldAmenity`
- `Field` (1) ↔ (0..n) `FieldService`
- `Field` (1) ↔ (0..n) `Review`
- `Field` (1) ↔ (0..n) `FavoriteField`
- `SubField` (1) ↔ (0..n) `FieldPricing`
- `SubField` (1) ↔ (0..n) `Booking`
- `User` (1) ↔ (0..n) `Booking`
- `User` (1) ↔ (0..n) `Review`
- `User` (1) ↔ (0..n) `Notification`
- `User` (1) ↔ (0..n) `FavoriteField`
- `User` (1) ↔ (0..n) `SearchHistory`
- `Booking` (1) ↔ (0..n) `BookingService`
- `Booking` (1) ↔ (0..n) `Payment`
- `Booking` (0..1) ↔ (1) `Promotion`
- `FieldService` (1) ↔ (0..n) `BookingService`
- `SearchHistory` (0..1) ↔ (1) `Field`

## 4. Tối Ưu Hiệu Suất
- **Index**:
  - `FieldId`, `SubFieldId`, `UserId`, `BookingDate`, `SearchDate`.
  - UNIQUE: `Email` trong `Account`, `User`.
  - `Code` trong `Promotion`.
- **Lazy Loading**: Cho `Bookings`, `Reviews`, `SubFields`.
- **Partitioning**: Phân vùng `Booking`, `SearchHistory` theo `BookingDate`, `SearchDate`.
- **Caching**: Redis cho `Field`, `SubField`, `FieldPricing`, `SearchHistory`.