## Mô tả chức năng các tầng - Layered Architecture:

- **Data**: Quản lý kết nối database qua EF Core, bao gồm `ApplicationDbContext` (DbContext), migrations, cấu hình ánh xạ entity, và seeding dữ liệu mẫu.
- **Models**: Định nghĩa các entity ánh xạ với bảng database (e.g., `Account`, `SubField`, `Booking`). Sử dụng Data Annotations để validation.
- **Interfaces**: Định nghĩa hợp đồng cho services (`IAuthService`, `ISubFieldService`) và repositories (`IRepository<T>`).
- **Repositories**: Tương tác với database qua EF Core, cung cấp các phương thức CRUD và truy vấn tùy chỉnh (e.g., `SubFieldRepository`, `BookingRepository`).
- **Services**: Chứa logic nghiệp vụ (e.g., `AuthService` xử lý đăng nhập, `BookingService` kiểm tra khung giờ trống). Tích hợp với các dịch vụ bên thứ ba (Cloudinary, VNPay, SendGrid).
- **Dtos**: Chuyển đổi dữ liệu giữa client và server, đảm bảo dữ liệu gửi/nhận gọn nhẹ và bảo mật (e.g., `LoginDto`, `SubFieldDto`).
- **Controllers**: Xử lý HTTP requests, ánh xạ tới services (e.g., `AuthController`, `BookingController`). Sử dụng `[Authorize]` và `[Role]` để bảo vệ endpoint.
- **Middlewares**: Xử lý xác thực (`RoleMiddleware`), lỗi toàn cục (`ExceptionMiddleware`), và logging.
- **BackgroundServices**: Chạy các tác vụ nền như gửi thông báo nhắc nhở (`BookingReminderService`) hoặc xóa thông báo cũ (`NotificationCleanupService`).
- **Tests**: Chứa unit tests (kiểm tra logic services) và integration tests (kiểm tra API endpoints) sử dụng xUnit hoặc NUnit.
- **Configurations**: Cấu hình các dịch vụ bên thứ ba (OAuth2, VNPay, Cloudinary, SendGrid).
- **Utils**: Các tiện ích hỗ trợ như mã hóa mật khẩu (`PasswordHasher`), tính toán khoảng cách địa lý (`GeoCalculator`), hoặc kiểm tra khung giờ (`TimeSlotValidator`).
