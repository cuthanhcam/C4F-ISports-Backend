# Tiến Độ Dự Án Backend C4F-ISports v2.0.0

## 1. Tổng Quan
Dự án được chia thành 8 giai đoạn, mỗi giai đoạn tập trung vào một nhóm tính năng. Tổng thời gian dự kiến là **3-4 tuần**, bắt đầu từ tháng 5/2025. Hiện tại, một số công việc chuẩn bị (models, schema, API docs) đã hoàn thành, hỗ trợ tốt cho các giai đoạn tiếp theo.

## 2. Tiến Độ Chi Tiết

### Giai Đoạn 1: Thiết Lập Nền Tảng
- **Nhánh**: `feature/setup`
- **Thời gian**: 3 ngày
- **Trạng thái**: Đang tiến hành (50% hoàn thành)
- **Công việc đã hoàn thành**:
  - Định nghĩa models (`Models-v2.0.0.cs`).
  - Xây dựng database schema (`database-schema.markdown`).
  - Thiết kế API endpoints (`api-endpoints.markdown`).
  - Tạo cấu trúc project (.NET Solution).
  - Cấu hình `ApplicationDbContext`, migrations, seeding dữ liệu.
- **Công việc còn lại**:
  - Thiết lập CloudinaryService, SendGridService, GeocodingService.
  - Cấu hình Swagger, Serilog, Redis.
  - Triển khai endpoint `/api/health`.
- **Kết quả mong đợi**:
  - Project structure hoàn chỉnh.
  - Database schema được áp dụng qua migrations.
  - Endpoint `/api/health` hoạt động.

### Giai Đoạn 2: Xác Thực và Phân Quyền
- **Nhánh**: `feature/authentication`
- **Thời gian**: 4 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Triển khai `AuthService` (JWT login, refresh token).
  - Tạo `AuthController` với các endpoint (`/api/auth/register`, `/api/auth/login`, `/api/auth/refresh-token`, `/api/auth/forgot-password`, `/api/auth/reset-password`, `/api/auth/verify-email`).
  - Thiết lập `RoleMiddleware` kiểm tra quyền (`User`, `Owner`, `Admin`).
  - Viết unit test cho `AuthService`.
- **Kết quả mong đợi**:
  - Đăng ký, đăng nhập qua email và mật khẩu với JWT.
  - Middleware bảo vệ các endpoint yêu cầu xác thực.

### Giai Đoạn 3: Quản Lý Người Dùng
- **Nhánh**: `feature/user-management`
- **Thời gian**: 4 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo `UserService`, `UserController` cho quản lý profile, lịch sử đặt sân, sân yêu thích, điểm thưởng.
  - Triển khai thêm/xóa sân yêu thích (`/api/users/favorites`).
  - Quản lý điểm thưởng (`LoyaltyPoints`).
  - Viết unit test.
- **Kết quả mong đợi**:
  - Người dùng quản lý thông tin cá nhân, xem lịch sử đặt sân, thêm/xóa sân yêu thích, kiểm tra điểm thưởng.

### Giai Đoạn 4: Quản Lý Sân và Sân Nhỏ
- **Nhánh**: `feature/field-management`
- **Thời gian**: 5 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo `FieldService`, `SubFieldService`, `FieldController` cho CRUD sân lớn, sân nhỏ, giá thuê.
  - Tích hợp Cloudinary cho upload ảnh (`FieldImage`).
  - Tìm kiếm sân theo vị trí với Google Maps API (`/api/fields?latitude&longitude`).
  - Viết unit test.
- **Kết quả mong đợi**:
  - Owner quản lý sân lớn/sân nhỏ, upload ảnh, thiết lập giá.
  - Người dùng tìm kiếm sân theo vị trí.

### Giai Đoạn 5: Quản Lý Đặt Sân
- **Nhánh**: `feature/booking-management`
- **Thời gian**: 5 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo `BookingService`, `BookingController` cho CRUD đặt sân, kiểm tra khung giờ trống.
  - Triển khai xem trước đơn đặt sân (`/api/bookings/preview`).
  - Hỗ trợ đặt sân phức tạp (`mainBooking` và `relatedBookings`).
  - Viết unit test.
- **Kết quả mong đợi**:
  - Người dùng đặt sân nhỏ, hủy đơn, xem đơn đặt sân.

### Giai Đoạn 6: Thanh Toán và Đánh Giá
- **Nhánh**: `feature/payment-review`
- **Thời gian**: 5 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo `PaymentService`, `PaymentController` cho thanh toán, xử lý webhook.
  - Tích hợp VNPay cho thanh toán (`/api/payments`).
  - Tạo `ReviewService` cho đánh giá sân, trả lời đánh giá (`/api/reviews`).
  - Viết unit test.
- **Kết quả mong đợi**:
  - Người dùng thanh toán đơn đặt sân, đánh giá sân.
  - Owner trả lời đánh giá.

### Giai Đoạn 7: Thông Báo, Khuyến Mãi, Thống Kê
- **Nhánh**: `feature/notification-promotion-stats`
- **Thời gian**: 4 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo `NotificationService` cho thông báo (`/api/notifications`).
  - Tạo `PromotionService` cho quản lý khuyến mãi (`/api/promotions`).
  - Tạo `StatisticsService` cho báo cáo doanh thu, đặt sân.
  - Viết unit test.
- **Kết quả mong đợi**:
  - Gửi thông báo, áp dụng khuyến mãi, xem báo cáo.

### Giai Đoạn 8: Kiểm Thử và Hoàn Thiện
- **Nhánh**: `develop`
- **Thời gian**: 3 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Kiểm thử API với Postman, chạy unit test và integration test.
  - Tối ưu truy vấn database, tích hợp Redis caching.
  - Hoàn thiện tài liệu API trên Swagger.
  - Merge vào nhánh `main` để deploy.
- **Kết quả mong đợi**:
  - Backend sẵn sàng deploy lên môi trường production.

## 3. Tối Ưu và Mở Rộng
- **Caching**: Sử dụng Redis để cache dữ liệu thường truy cập (`Field`, `SubField`, `FieldPricing`) nhằm giảm tải database.
- **Indexing**: Đã cấu hình index cho `FieldId`, `SubFieldId`, `BookingDate`, `UserId` trong `ApplicationDbContext` để tối ưu truy vấn.
- **Gợi ý sân**: Dựa trên `SearchHistory` và `FavoriteField` để đề xuất sân phù hợp cho người dùng.
- **Điểm thưởng**: Tích lũy `LoyaltyPoints` sau mỗi lần đặt sân, cho phép đổi ưu đãi hoặc giảm giá.
- **Scalability**: Sử dụng background services (`BookingReminderService`, `NotificationCleanupService`) để xử lý tác vụ nặng.

## 4. Quản Lý Tiến Độ
- **Họp team**: Hàng tuần (e.g., thứ Hai 10:00 AM) để cập nhật tiến độ, giải quyết blocker.
- **Git workflow**:
  - Mỗi giai đoạn sử dụng nhánh riêng (e.g., `feature/setup`, `feature/authentication`).
  - Code review qua pull request trước khi merge vào `develop`.
  - Nhánh `main` chỉ chứa code sẵn sàng deploy.
- **Phân công**:
  - **Backend**: Bạn (code, viết tài liệu, quản lý tiến độ).
  - **Frontend**: 2 thành viên (tích hợp API, phát triển giao diện React).
- **Công cụ**:
  - **Trello/Jira**: Theo dõi task.
  - **Slack**: Giao tiếp team.
  - **GitHub**: Quản lý mã nguồn.

## 5. Rủi Ro và Giải Pháp
- **Rủi ro**: Tích hợp dịch vụ bên thứ ba (VNPay, Cloudinary) có thể gặp lỗi cấu hình.
  - **Giải pháp**: Test tích hợp sớm, chuẩn bị mock data cho dev.
- **Rủi ro**: Truy vấn database chậm khi dữ liệu lớn.
  - **Giải pháp**: Tối ưu index, sử dụng Redis caching, phân trang (`skip`, `take`) trong API.
- **Rủi ro**: Frontend chậm tích hợp API.
  - **Giải pháp**: Cung cấp tài liệu API chi tiết (`api-integration-guide.md`), mock API với Swagger.