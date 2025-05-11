# Kế Hoạch Phát Triển Backend C4F-ISports v2.0.0

## 1. Mục Tiêu
Xây dựng backend cho ứng dụng đặt sân thể thao C4F-ISports v2.0.0, hỗ trợ các tính năng: xác thực (local + OAuth2), quản lý sân lớn/sân nhỏ, đặt sân, thanh toán, đánh giá, thông báo, khuyến mãi, và thống kê. Backend được triển khai trên **.NET 8.0**, sử dụng **SQL Server**, **Redis**, và tích hợp các dịch vụ bên thứ ba (Cloudinary, VNPay, SendGrid, Google Maps API).

## 2. Kế Hoạch Chi Tiết

### Giai Đoạn 1: Thiết Lập Nền Tảng
- **Nhánh**: `feature/setup`
- **Thời gian**: 3 ngày
- **Công việc**:
  - Tạo .NET Solution với Layered Architecture (`Data`, `Models`, `Services`, v.v.).
  - Cấu hình `ApplicationDbContext`, migrations, seeding dữ liệu mẫu.
  - Thiết lập tích hợp OAuth2 (Google), Cloudinary (ảnh), SendGrid (email).
  - Cấu hình Redis (caching), Swagger (API docs), Serilog (logging).
  - Triển khai endpoint `/api/health` để kiểm tra server.
- **Deliverables**:
  - Cấu trúc project hoàn chỉnh (`api.sln`).
  - Database schema được áp dụng qua migrations.
  - Tài liệu API cơ bản trên Swagger.
  - Endpoint `/api/health` trả về `{ "status": "healthy" }`.
- **Công cụ**: .NET 8.0, EF Core, SQL Server, Redis, Swagger, Serilog.

### Giai Đoạn 2: Xác Thực và Phân Quyền
- **Nhánh**: `feature/authentication`
- **Thời gian**: 4 ngày
- **Công việc**:
  - Triển khai `AuthService` (đăng ký, đăng nhập local, OAuth2, refresh token).
  - Tạo `AuthController` với các endpoint (`/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`, v.v.).
  - Thiết lập `RoleMiddleware` kiểm tra quyền (`User`, `Owner`, `Admin`).
  - Viết unit test cho `AuthService` (dùng xUnit, Moq).
- **Deliverables**:
  - Người dùng đăng ký/đăng nhập qua email hoặc Google.
  - JWT và refresh token hoạt động.
  - Middleware bảo vệ endpoint dựa trên role.
  - Unit test coverage >80% cho `AuthService`.
- **Công cụ**: JWT, bcrypt, OAuth2, xUnit, Moq.

### Giai Đoạn 3: Quản Lý Người Dùng
- **Nhánh**: `feature/user-management`
- **Thời gian**: 4 ngày
- **Công việc**:
  - Tạo `UserService`, `UserController` cho quản lý profile (`/api/users/profile`), lịch sử đặt sân (`/api/users/bookings`), sân yêu thích (`/api/users/favorites`), điểm thưởng (`LoyaltyPoints`).
  - Triển khai thêm/xóa sân yêu thích, cập nhật điểm thưởng sau mỗi lần đặt sân.
  - Viết unit test cho `UserService`.
- **Deliverables**:
  - Người dùng xem/cập nhật profile, lịch sử đặt sân, danh sách sân yêu thích.
  - Điểm thưởng được cộng tự động sau khi đặt sân.
  - Unit test coverage >80% cho `UserService`.
- **Công cụ**: EF Core, xUnit, Moq.

### Giai Đoạn 4: Quản Lý Sân và Sân Nhỏ
- **Nhánh**: `feature/field-management`
- **Thời gian**: 5 ngày
- **Công việc**:
  - Tạo `FieldService`, `SubFieldService`, `FieldController` cho CRUD sân lớn (`Field`), sân nhỏ (`SubField`), giá thuê (`FieldPricing`).
  - Tích hợp Cloudinary để upload ảnh sân (`FieldImage`).
  - Tìm kiếm sân theo vị trí với Google Maps API (`/api/fields?latitude&longitude`).
  - Viết unit test cho `FieldService`, `SubFieldService`.
- **Deliverables**:
  - Owner tạo/cập nhật/xóa sân lớn, sân nhỏ, thiết lập giá thuê.
  - Người dùng tìm kiếm sân theo vị trí, xem chi tiết sân.
  - Unit test coverage >80% cho `FieldService`, `SubFieldService`.
- **Công cụ**: Cloudinary, Google Maps API, EF Core, xUnit, Moq.

### Giai Đoạn 5: Quản Lý Đặt Sân
- **Nhánh**: `feature/booking-management`
- **Thời gian**: 5 ngày
- **Công việc**:
  - Tạo `BookingService`, `BookingController` cho CRUD đặt sân (`/api/bookings`), kiểm tra khung giờ trống (`/api/bookings/availability`).
  - Triển khai xem trước đơn đặt sân (`/api/bookings/preview`) với tính năng `mainBooking` và `relatedBookings`.
  - Hỗ trợ hủy đơn đặt sân (`/api/bookings/{id}/cancel`).
  - Viết unit test cho `BookingService`.
- **Deliverables**:
  - Người dùng đặt sân nhỏ, xem khung giờ trống, hủy đơn.
  - Hỗ trợ đặt sân phức tạp với nhiều sân nhỏ.
  - Unit test coverage >80% cho `BookingService`.
- **Công cụ**: EF Core, xUnit, Moq.

### Giai Đoạn 6: Thanh Toán và Đánh Giá
- **Nhánh**: `feature/payment-review`
- **Thời gian**: 5 ngày
- **Công việc**:
  - Tạo `PaymentService`, `PaymentController` cho thanh toán (`/api/payments`), xử lý webhook từ VNPay.
  - Tích hợp VNPay để thanh toán đơn đặt sân.
  - Tạo `ReviewService` cho tạo/ xem/ trả lời đánh giá (`/api/reviews`).
  - Viết unit test cho `PaymentService`, `ReviewService`.
- **Deliverables**:
  - Người dùng thanh toán đơn đặt sân qua VNPay.
  - Người dùng tạo đánh giá, owner trả lời đánh giá.
  - Unit test coverage >80% cho `PaymentService`, `ReviewService`.
- **Công cụ**: VNPay, EF Core, xUnit, Moq.

### Giai Đoạn 7: Thông Báo, Khuyến Mãi, Thống Kê
- **Nhánh**: `feature/notification-promotion-stats`
- **Thời gian**: 4 ngày
- **Công việc**:
  - Tạo `NotificationService` cho gửi thông báo (`/api/notifications`) qua SendGrid.
  - Tạo `PromotionService` cho quản lý khuyến mãi (`/api/promotions`), áp dụng mã giảm giá.
  - Tạo `StatisticsService` cho báo cáo doanh thu, số lượng đặt sân.
  - Viết unit test cho các service.
- **Deliverables**:
  - Gửi thông báo qua email (đặt sân, khuyến mãi).
  - Áp dụng mã khuyến mãi khi đặt sân.
  - Owner xem báo cáo thống kê.
  - Unit test coverage >80% cho các service.
- **Công cụ**: SendGrid, EF Core, xUnit, Moq.

### Giai Đoạn 8: Kiểm Thử và Hoàn Thiện
- **Nhánh**: `develop`
- **Thời gian**: 3 ngày
- **Công việc**:
  - Kiểm thử toàn bộ API với Postman, chạy unit test và integration test.
  - Tối ưu truy vấn database (indexing, eager/lazy loading).
  - Tích hợp Redis caching cho `Field`, `SubField`, `FieldPricing`.
  - Hoàn thiện tài liệu API trên Swagger.
  - Merge vào nhánh `main` để deploy.
- **Deliverables**:
  - Backend đạt test coverage >80%.
  - API docs hoàn chỉnh trên Swagger.
  - Backend sẵn sàng deploy lên môi trường production.
- **Công cụ**: Postman, Redis, Swagger, xUnit.

## 3. Tổng Thời Gian
- **Dự kiến**: 3-4 tuần (21-28 ngày).
- **Bắt đầu**: 11/05/2025.
- **Hoàn thành**: Cuối tháng 5/2025 hoặc đầu tháng 6/2025.

## 4. Quản Lý Team
- **Backend**: Bạn (code backend, viết tài liệu, quản lý tiến độ).
- **Frontend**: 2 người (tích hợp API, phát triển giao diện React).
- **Họp team**: Hàng tuần (thứ Hai 10:00 AM) qua Zoom/Slack.
- **Git workflow**:
  - Nhánh riêng cho mỗi giai đoạn (`feature/*`).
  - Pull request với ít nhất 1 reviewer trước khi merge.
  - Nhánh `develop` để tích hợp, `main` để deploy.
- **Công cụ quản lý**:
  - **Trello/Jira**: Theo dõi task.
  - **Slack**: Giao tiếp team.
  - **GitHub**: Quản lý mã nguồn, code review.

## 5. Quản Lý Rủi Ro
- **Rủi ro 1**: Tích hợp dịch vụ bên thứ ba (VNPay, Cloudinary, SendGrid) gặp lỗi.
  - **Giải pháp**: Test tích hợp sớm trong Giai đoạn 1, sử dụng mock data nếu cần.
- **Rủi ro 2**: Truy vấn database chậm khi dữ liệu lớn.
  - **Giải pháp**: Tối ưu index trong `ApplicationDbContext`, dùng Redis caching, phân trang API.
- **Rủi ro 3**: Frontend chậm tích hợp do thiếu tài liệu API.
  - **Giải pháp**: Cung cấp `api-integration-guide.md`, mock API trên Swagger từ Giai đoạn 2.
- **Rủi ro 4**: Bug trong logic đặt sân phức tạp (`mainBooking`, `relatedBookings`).
  - **Giải pháp**: Viết unit test kỹ lưỡng cho `BookingService`, kiểm tra edge cases.

## 6. Tài Liệu Hỗ Trợ
- **Database Schema**: `database-schema.markdown`.
- **API Endpoints**: `api-endpoints.markdown`.
- **Models**: `Models-v2.0.0.cs`.
- **API Integration Guide**: `api-integration-guide.md` (sẽ được cung cấp).
- **Architecture Overview**: `architecture-overview.md` (sẽ được cung cấp).