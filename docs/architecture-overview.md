# Tổng Quan Kiến Trúc Backend C4F-ISports v2.0.0

## 1. Mục Tiêu
Cung cấp cái nhìn tổng quan về kiến trúc backend của C4F-ISports v2.0.0, mô tả cách các thành phần (database, services, APIs, third-party integrations) tương tác để hỗ trợ các tính năng như xác thực, quản lý sân, đặt sân, thanh toán, đánh giá, thông báo, và khuyến mãi.

## 2. Kiến Trúc Tổng Quan
Backend sử dụng **Layered Architecture** trên **.NET 8.0**, với các tầng:
- **Presentation**: Controllers xử lý HTTP requests, trả về responses.
- **Application**: Services chứa logic nghiệp vụ (e.g., `AuthService`, `BookingService`).
- **Domain**: Models định nghĩa entities (e.g., `Account`, `SubField`).
- **Infrastructure**: Repositories tương tác với database, tích hợp third-party services.

```
[Client] --> [API Gateway/Load Balancer] --> [Controllers]
    |                                   |
[Middlewares]                       [Services]
    |                                   |
[Repositories]                    [Third-Party Services]
    |                                   |
[Database]                     [Cloudinary, VNPay, SendGrid, Google Maps]
```

## 3. Thành Phần Chính

### 3.1. Database
- **Công nghệ**: SQL Server, EF Core 8.0.
- **Schema**: 21 bảng (`Account`, `User`, `Field`, `SubField`, `Booking`, v.v.) được định nghĩa trong `database-schema.markdown`.
- **Tối ưu**:
  - Indexes trên `FieldId`, `SubFieldId`, `UserId`, `BookingDate`.
  - Decimal precision (`18,2`) cho giá tiền (`FieldPricing.Price`, `Booking.TotalPrice`).
  - Eager/lazy loading tùy chỉnh trong `ApplicationDbContext`.

### 3.2. APIs
- **Base URL**: `/api/v2`.
- **Endpoints**: Định nghĩa trong `api-endpoints.markdown`, bao gồm:
  - **Auth**: `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`.
  - **Fields**: `/api/fields`, `/api/fields/{id}`.
  - **Bookings**: `/api/bookings`, `/api/bookings/simple`, `/api/bookings/availability`.
  - **Payments**: `/api/payments`, `/api/payments/webhook`.
  - **Reviews**: `/api/reviews`, `/api/fields/{id}/reviews`.
  - **Notifications**: `/api/notifications`.
  - **Promotions**: `/api/promotions`.
  - **Users**: `/api/users/profile`, `/api/users/favorites`.
- **Tài liệu**: Swagger (`/swagger`) cung cấp mô tả endpoint, request/response, error codes.

### 3.3. Third-Party Services
- **Cloudinary**: Upload/quản lý ảnh sân (`FieldImage`).
- **VNPay**: Thanh toán đơn đặt sân (`Payment`).
- **SendGrid**: Gửi email xác thực, thông báo (`Notification`).
- **Google Maps API**: Tìm kiếm sân theo vị trí (`Field.Latitude`, `Field.Longitude`).
- **Redis**: Caching dữ liệu `Field`, `SubField`, `FieldPricing` để giảm tải database.

### 3.4. Background Services
- **BookingReminderService**: Gửi email nhắc nhở trước giờ đặt sân.
- **NotificationCleanupService**: Xóa thông báo cũ (`Notification.IsRead = true` sau 30 ngày).

### 3.5. Security
- **Authentication**: JWT cho local login, OAuth2 cho Google login.
- **Authorization**: `RoleMiddleware` kiểm tra quyền (`User`, `Owner`, `Admin`).
- **Password Hashing**: bcrypt cho `Account.Password`.
- **Refresh Tokens**: Quản lý qua bảng `RefreshTokens`.

## 4. Luồng Dữ Liệu (Ví Dụ: Đặt Sân)
1. Người dùng gửi request tới `/api/bookings` với `SubFieldId`, `StartTime`, `EndTime`.
2. `BookingController` gọi `BookingService` để:
   - Kiểm tra khung giờ trống (`FieldPricing`, `BookingTimeSlot`).
   - Tính `TotalPrice` (dựa trên `FieldPricing.Price`, `BookingService.Price`, `Promotion`).
   - Lưu `Booking` và `BookingTimeSlot` vào database.
3. `BookingService` gọi `PaymentService` để tạo URL thanh toán VNPay.
4. Người dùng thanh toán, VNPay gửi webhook tới `/api/payments/webhook`.
5. `PaymentService` cập nhật `Payment.Status` và `Booking.PaymentStatus`.
6. `NotificationService` gửi email xác nhận qua SendGrid.

## 5. Tối Ưu Hiệu Suất
- **Caching**: Redis lưu trữ `Field`, `SubField`, `FieldPricing` với TTL 1 giờ.
- **Indexing**: Tối ưu truy vấn với index trên `FieldId`, `SubFieldId`, `BookingDate`.
- **Pagination**: API trả về dữ liệu phân trang (`skip`, `take`) để giảm tải.
- **Async/Await**: Tất cả service/repository sử dụng async để cải thiện throughput.
- **Background Tasks**: Tác vụ nặng (gửi email, xóa thông báo) chạy qua `BackgroundServices`.

## 6. Công Cụ và Thư Viện
- **.NET 8.0**: Framework chính.
- **EF Core 8.0**: ORM.
- **Redis**: Caching.
- **Cloudinary**: Ảnh.
- **VNPay**: Thanh toán.
- **SendGrid**: Email.
- **Google Maps API**: Tìm kiếm vị trí.
- **Serilog**: Logging.
- **Swagger**: API docs.
- **xUnit/Moq**: Testing.

## 7. Hướng Dẫn Triển Khai
1. Cấu hình environment variables trong `appsettings.json` (database, Cloudinary, VNPay, SendGrid, Redis).
2. Chạy migrations: `dotnet ef database update`.
3. Khởi động Redis server.
4. Chạy ứng dụng: `dotnet run`.
5. Truy cập Swagger: `https://<host>/swagger`.

## 8. Liên Kết Tài Liệu
- **Database Schema**: `database-schema.markdown`.
- **API Endpoints**: `api-endpoints.markdown`.
- **Models**: `Models-v2.0.0.cs`.
- **Project Structure**: `project-structure.md`.
- **API Integration Guide**: `api-integration-guide.md`.