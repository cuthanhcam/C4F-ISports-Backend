# Kế Hoạch Phát Triển Backend C4F-ISports v2.0.0

## Kế Hoạch Chi Tiết

### Giai Đoạn 1: Thiết Lập Nền Tảng
- **Nhánh**: `feature/setup`
- **Thời gian**: 3 ngày
- **Công việc**:
  - Tạo cấu trúc project (.NET Solution).
  - Cấu hình DbContext, migrations, seeding dữ liệu.
  - Thiết lập OAuth2, CloudinaryService, SendGridService.
  - Cấu hình Swagger, Serilog, Redis.
  - Tạo Models, Configurations, Seeders.
- **Kết quả**:
  - Project structure hoàn chỉnh.
  - Database schema cơ bản.
  - Endpoint `/api/health`.

### Giai Đoạn 2: Xác Thực và Phân Quyền
- **Nhánh**: `feature/authentication`
- **Thời gian**: 4 ngày
- **Công việc**:
  - Triển khai AuthService (OAuth2, local login, refresh token).
  - Tạo AuthController với các endpoint.
  - Thiết lập middleware kiểm tra role.
  - Viết unit test cho AuthService.
- **Kết quả**:
  - Đăng ký, đăng nhập qua email/OAuth2.
  - Middleware bảo vệ endpoint.

### Giai Đoạn 3: Quản Lý Người Dùng
- **Nhánh**: `feature/user-management`
- **Thời gian**: 4 ngày
- **Công việc**:
  - Tạo UserService, UserController cho profile, lịch sử đặt sân, sân yêu thích, điểm thưởng.
  - Triển khai thêm/xóa sân yêu thích, quản lý điểm thưởng.
  - Viết unit test.
- **Kết quả**:
  - Quản lý thông tin cá nhân, lịch sử, điểm thưởng.

### Giai Đoạn 4: Quản Lý Sân và Sân Nhỏ
- **Nhánh**: `feature/field-management`
- **Thời gian**: 5 ngày
- **Công việc**:
  - Tạo FieldService, SubFieldService, FieldController cho CRUD sân lớn, sân nhỏ, giá thuê.
  - Tích hợp Cloudinary cho ảnh sân.
  - Tìm kiếm sân theo vị trí với Google Maps API.
  - Viết unit test.
- **Kết quả**:
  - Quản lý sân lớn, sân nhỏ, tìm kiếm sân.

### Giai Đoạn 5: Quản Lý Đặt Sân
- **Nhánh**: `feature/booking-management`
- **Thời gian**: 5 ngày
- **Công việc**:
  - Tạo BookingService, BookingController cho CRUD đặt sân, kiểm tra khung giờ trống.
  - Triển khai xem trước đơn đặt sân.
  - Viết unit test.
- **Kết quả**:
  - Đặt sân nhỏ, hủy đơn, quản lý đơn.

### Giai Đoạn 6: Thanh Toán và Đánh Giá
- **Nhánh**: `feature/payment-review`
- **Thời gian**: 5 ngày
- **Công việc**:
  - Tạo PaymentService, PaymentController cho thanh toán, webhook.
  - Tích hợp VNPay.
  - Tạo ReviewService cho đánh giá sân, trả lời đánh giá.
  - Viết unit test.
- **Kết quả**:
  - Thanh toán, đánh giá, trả lời đánh giá.

### Giai Đoạn 7: Thông Báo, Khuyến Mãi, Thống Kê
- **Nhánh**: `feature/notification-promotion-stats`
- **Thời gian**: 4 ngày
- **Công việc**:
  - Tạo NotificationService, PromotionService, StatisticsService.
  - Triển khai thông báo, gợi ý khuyến mãi, thống kê.
  - Viết unit test.
- **Kết quả**:
  - Thông báo, khuyến mãi, báo cáo.

### Giai Đoạn 8: Kiểm Thử và Hoàn Thiện
- **Nhánh**: `develop`
- **Thời gian**: 3 ngày
- **Công việc**:
  - Kiểm thử API với Postman, unit test.
  - Tối ưu truy vấn database, tích hợp Redis.
  - Hoàn thiện tài liệu API trên Swagger.
  - Merge vào `main` để deploy.
- **Kết quả**:
  - Backend sẵn sàng deploy.

## 3. Tổng Thời Gian
- **Dự kiến**: 3-4 tuần (21-28 ngày).
- **Hoàn thành**: Cuối tháng 5/2025 hoặc đầu tháng 6/2025.

## 4. Quản Lý Team
- **Backend (bạn)**: Code backend, viết tài liệu, quản lý tiến độ.
- **Frontend (2 người)**: Tích hợp API, phát triển giao diện.
- **Họp team**: Hàng tuần để cập nhật tiến độ.
- **Git**: Nhánh riêng cho mỗi giai đoạn, review code trước merge.