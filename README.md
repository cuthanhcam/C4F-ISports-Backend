# C4F-ISports-Backend

Web API Refactor - Version 2.0.0

Sports field management and scheduling web application - Backend

## 1. Mô Tả Dự Án
C4F-ISports là ứng dụng quản lý sân thể thao và đặt lịch trực tuyến, kết nối người dùng với chủ sân. Backend cung cấp API hỗ trợ tìm kiếm sân, đặt sân nhỏ, thanh toán, quản lý đánh giá, thông báo, và thống kê. Phiên bản 2.0.0 cải tiến từ phiên bản cũ, bổ sung quản lý sân nhỏ (`SubField`), tích hợp OAuth2, gợi ý sân thông minh, và điểm thưởng.

## 2. Mục Tiêu
- Xây dựng backend mạnh mẽ, dễ mở rộng, hỗ trợ sân lớn chứa nhiều sân nhỏ với giá thuê linh hoạt.
- Tích hợp OAuth2 cho xác thực an toàn.
- Tối ưu hiệu suất với caching và indexing.
- Bổ sung gợi ý sân, điểm thưởng, và lịch sử tìm kiếm.
- Cung cấp tài liệu API chi tiết để tích hợp frontend và trình bày cho giảng viên.

## 3. Vai Trò và Chức Năng
### 3.1. Người Dùng (User)
- Đăng ký/đăng nhập qua email hoặc OAuth2 (Google, Facebook).
- Tìm kiếm sân theo vị trí, loại thể thao, khung giờ.
- Đặt sân nhỏ, chọn dịch vụ, thanh toán, áp dụng khuyến mãi.
- Đánh giá sân lớn, xem lịch sử đặt sân, nhận thông báo.
- Quản lý điểm thưởng và sân yêu thích.

### 3.2. Chủ Sân (Owner)
- Quản lý sân lớn, sân nhỏ, giá thuê, dịch vụ, tiện ích.
- Xử lý đơn đặt sân, trả lời đánh giá.
- Xem thống kê doanh thu và lượt đặt sân.

### 3.3. Admin
- Quản lý người dùng, chủ sân, loại thể thao.
- Kiểm duyệt đánh giá, xem báo cáo hệ thống.

## 4. Luồng Hoạt Động Chính
### 4.1. Đăng Ký và Đăng Nhập
- Đăng ký qua email hoặc OAuth2, xác thực tài khoản.
- Đăng nhập với email/mật khẩu hoặc OAuth provider.

### 4.2. Tìm Kiếm và Đặt Sân
- Tìm kiếm sân lớn theo vị trí, loại thể thao.
- Chọn sân nhỏ, kiểm tra khung giờ trống, xem giá thuê.
- Đặt sân, chọn dịch vụ, áp dụng khuyến mãi, thanh toán.

### 4.3. Quản Lý Đặt Sân
- Chủ sân xác nhận/hủy đơn đặt.
- Người dùng hủy/đổi lịch đặt sân (nếu được phép).

### 4.4. Thanh Toán
- Thanh toán trực tuyến qua VNPay.
- Hỗ trợ hoàn tiền khi hủy đơn.

### 4.5. Đánh Giá và Thông Báo
- Người dùng đánh giá sân lớn, chủ sân trả lời.
- Gửi thông báo thời gian thực qua email.

## 5. Tính Năng Nổi Bật
- **Quản lý sân nhỏ**: Hỗ trợ sân lớn chứa nhiều sân nhỏ với giá thuê linh hoạt.
- **Tìm kiếm thông minh**: Bộ lọc vị trí, loại sân, khung giờ.
- **Gợi ý sân**: Dựa trên lịch sử tìm kiếm, sân yêu thích.
- **Điểm thưởng**: Tích lũy điểm khi đặt sân, đổi ưu đãi.
- **OAuth2**: Đăng nhập an toàn qua Google, Facebook.

## 6. Công Nghệ Sử Dụng
- **Ngôn ngữ**: C# (.NET Core 8.0)
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Tích hợp**:
  - Cloudinary (quản lý hình ảnh)
  - SendGrid (gửi email)
  - VNPay (thanh toán)
  - Google Maps API (tìm kiếm vị trí)
  - OAuth2 (Google, Facebook)
- **Authentication**: OAuth2, JWT (cho tài khoản local)
- **Công cụ**: Swagger (tài liệu API), xUnit (unit test), Serilog (logging), Redis (caching)

## 7. Đội Ngũ Phát Triển
- **Backend Developer/Team Leader**: 1 người (code backend, quản lý tiến độ).
- **Frontend Developers**: 2 người (phát triển giao diện, tích hợp API).