# Tiến Độ Dự Án Backend C4F-ISports v2.0.0

## 1. Tổng Quan
Dự án chia thành 8 giai đoạn, mỗi giai đoạn tương ứng một nhóm tính năng. Tổng thời gian dự kiến là 3-4 tuần, bắt đầu từ tháng 5/2025.

## 2. Tiến Độ Chi Tiết

### Giai Đoạn 1: Thiết Lập Nền Tảng
- **Nhánh**: `feature/setup`
- **Thời gian**: 3 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo cấu trúc project.
  - Cấu hình DbContext, migrations, seeding.
  - Thiết lập OAuth2, Cloudinary, SendGrid, Redis.
  - Cấu hình Swagger, Serilog.
- **Kết quả mong đợi**:
  - Project structure hoàn chỉnh.
  - Database schema cơ bản.
  - Endpoint `/api/health`.

### Giai Đoạn 2: Xác Thực và Phân Quyền
- **Nhánh**: `feature/authentication`
- **Thời gian**: 4 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Triển khai AuthService, AuthController (OAuth2, local login).
  - Thiết lập middleware role.
  - Viết unit test.
- **Kết quả mong đợi**:
  - Đăng ký, đăng nhập qua email/OAuth2.
  - Middleware bảo vệ endpoint.

### Giai Đoạn 3: Quản Lý Người Dùng
- **Nhánh**: `feature/user-management`
- **Thời gian**: 4 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo UserService, UserController.
  - Triển khai quản lý profile, sân yêu thích, điểm thưởng.
  - Viết unit test.
- **Kết quả mong đợi**:
  - Quản lý thông tin cá nhân, lịch sử, điểm thưởng.

### Giai Đoạn 4: Quản Lý Sân và Sân Nhỏ
- **Nhánh**: `feature/field-management`
- **Thời gian**: 5 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo FieldService, SubFieldService, FieldController.
  - Tích hợp Cloudinary, Google Maps API.
  - Viết unit test.
- **Kết quả mong đợi**:
  - Quản lý sân lớn, sân nhỏ, tìm kiếm sân.

### Giai Đoạn 5: Quản Lý Đặt Sân
- **Nhánh**: `feature/booking-management`
- **Thời gian**: 5 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo BookingService, BookingController.
  - Triển khai xem trước đơn đặt.
  - Viết unit test.
- **Kết quả mong đợi**:
  - Đặt sân nhỏ, hủy đơn, quản lý đơn.

### Giai Đoạn 6: Thanh Toán và Đánh Giá
- **Nhánh**: `feature/payment-review`
- **Thời gian**: 5 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo PaymentService, ReviewService.
  - Tích hợp VNPay.
  - Viết unit test.
- **Kết quả mong đợi**:
  - Thanh toán, đánh giá, trả lời đánh giá.

### Giai Đoạn 7: Thông Báo, Khuyến Mãi, Thống Kê
- **Nhánh**: `feature/notification-promotion-stats`
- **Thời gian**: 4 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Tạo NotificationService, PromotionService, StatisticsService.
  - Triển khai gợi ý khuyến mãi, thống kê.
  - Viết unit test.
- **Kết quả mong đợi**:
  - Thông báo, khuyến mãi, báo cáo.

### Giai Đoạn 8: Kiểm Thử và Hoàn Thiện
- **Nhánh**: `develop`
- **Thời gian**: 3 ngày
- **Trạng thái**: Chưa bắt đầu
- **Công việc**:
  - Kiểm thử API, unit test.
  - Tối ưu database, tích hợp Redis.
  - Hoàn thiện tài liệu Swagger.
- **Kết quả mong đợi**:
  - Backend sẵn sàng deploy.

## 3. Tối Ưu và Mở Rộng
- **Caching**: Redis cho `Field`, `SubField`, `FieldPricing`.
- **Indexing**: `FieldId`, `SubFieldId`, `BookingDate`.
- **Gợi ý sân**: Dựa trên `SearchHistory`, `FavoriteField`.
- **Điểm thưởng**: Tích lũy và đổi ưu đãi.

## 4. Quản Lý Tiến Độ
- **Họp team**: Hàng tuần để cập nhật.
- **Git**: Nhánh riêng cho mỗi giai đoạn, review code.
- **Phân công**:
  - Backend: Bạn (code, tài liệu, quản lý).
  - Frontend: 2 thành viên (tích hợp API, giao diện).