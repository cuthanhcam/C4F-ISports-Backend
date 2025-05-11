# Cấu Trúc Project Backend C4F-ISports v2.0.0

## 1. Tổng Quan
Backend sử dụng **.NET 8.0** với **Layered Architecture**, chia thành các tầng: **Data**, **Models**, **Interfaces**, **Repositories**, **Services**, **Dtos**, **Controllers**, **Middlewares**, **BackgroundServices**, và **Tests**. Cấu trúc được thiết kế để hỗ trợ các tính năng chính như quản lý sân lớn/sân nhỏ, đặt sân, thanh toán, đánh giá, thông báo, và xác thực OAuth2. Tất cả mã nguồn được tổ chức để dễ bảo trì và mở rộng.

## 2. Cấu Trúc Thư Mục
```
api/
├── Data/
│   ├── ApplicationDbContext.cs                  # EF Core DbContext cho database
│   ├── UnitOfWork.cs                           # Quản lý transaction
│   ├── Migrations/                             # EF Core migrations
│   ├── Configurations/                         # Cấu hình ánh xạ entity
│   │   ├── AccountConfiguration.cs
│   │   ├── SubFieldConfiguration.cs
│   │   ├── BookingConfiguration.cs
│   │   ├── ...                                 # Các file cấu hình khác
│   ├── Seeders/                                # Dữ liệu mẫu ban đầu
│   │   ├── AccountSeeder.cs
│   │   ├── SubFieldSeeder.cs
│   │   ├── SportSeeder.cs
│   │   ├── ...
├── Models/
│   ├── Account.cs                              # Entity cho bảng Accounts
│   ├── SubField.cs                             # Entity cho bảng SubFields
│   ├── Booking.cs                              # Entity cho bảng Bookings
│   ├── ...                                     # Các entity khác
├── Interfaces/
│   ├── IAuthService.cs                         # Interface cho AuthService
│   ├── ISubFieldService.cs                     # Interface cho SubFieldService
│   ├── IBookingService.cs                      # Interface cho BookingService
│   ├── IRepository.cs                          # Generic repository interface
│   ├── ...
├── Repositories/
│   ├── GenericRepository.cs                    # Repository cơ bản
│   ├── SubFieldRepository.cs                   # Repository cho SubFields
│   ├── BookingRepository.cs                    # Repository cho Bookings
│   ├── ...
├── Services/
│   ├── AuthService.cs                          # Logic xác thực (login, OAuth2)
│   ├── SubFieldService.cs                      # Logic quản lý sân nhỏ
│   ├── BookingService.cs                       # Logic quản lý đặt sân
│   ├── CloudinaryService.cs                    # Tích hợp Cloudinary
│   ├── NotificationService.cs                  # Gửi thông báo
│   ├── PaymentService.cs                       # Xử lý thanh toán
│   ├── ...
├── Dtos/
│   ├── Auth/
│   │   ├── LoginDto.cs                         # DTO cho đăng nhập
│   │   ├── RegisterDto.cs                      # DTO cho đăng ký
│   │   ├── ...
│   ├── SubField/
│   │   ├── SubFieldDto.cs                      # DTO cho sân nhỏ
│   │   ├── SubFieldCreateDto.cs                # DTO tạo sân nhỏ
│   │   ├── ...
│   ├── Booking/
│   │   ├── BookingDto.cs                       # DTO cho đơn đặt sân
│   │   ├── BookingCreateDto.cs                 # DTO tạo đơn đặt sân
│   │   ├── ...
│   ├── ...
├── Controllers/
│   ├── AuthController.cs                       # Xử lý API xác thực
│   ├── SubFieldController.cs                   # Xử lý API sân nhỏ
│   ├── BookingController.cs                    # Xử lý API đặt sân
│   ├── ...
├── Middlewares/
│   ├── RoleMiddleware.cs                       # Kiểm tra quyền truy cập
│   ├── ExceptionMiddleware.cs                  # Xử lý lỗi toàn cục
├── Configurations/
│   ├── OAuthConfig.cs                          # Cấu hình OAuth2
│   ├── VNPayConfig.cs                          # Cấu hình VNPay
│   ├── CloudinaryConfig.cs                     # Cấu hình Cloudinary
│   ├── SendGridConfig.cs                       # Cấu hình SendGrid
├── Utils/
│   ├── PasswordHasher.cs                       # Mã hóa mật khẩu (bcrypt)
│   ├── TimeSlotValidator.cs                    # Kiểm tra khung giờ đặt sân
│   ├── GeoCalculator.cs                        # Tính khoảng cách địa lý
├── BackgroundServices/
│   ├── NotificationCleanupService.cs           # Xóa thông báo cũ
│   ├── BookingReminderService.cs               # Gửi nhắc nhở đặt sân
├── Exceptions/
│   ├── AppException.cs                         # Lỗi tùy chỉnh
│   ├── ResourceNotFoundException.cs            # Lỗi không tìm thấy
│   ├── ValidationException.cs                  # Lỗi validation
├── Tests/
│   ├── UnitTests/
│   │   ├── AuthServiceTests.cs                 # Unit test cho AuthService
│   │   ├── BookingServiceTests.cs              # Unit test cho BookingService
│   │   ├── ...
│   ├── IntegrationTests/
│   │   ├── AuthControllerTests.cs              # Integration test cho AuthController
│   │   ├── BookingControllerTests.cs           # Integration test cho BookingController
│   │   ├── ...
├── Properties/
│   ├── launchSettings.json                     # Cấu hình chạy local
├── .gitignore
├── Program.cs                                  # Điểm vào ứng dụng
├── Startup.cs                                  # Cấu hình DI, middleware
├── api.csproj                                  # File project
├── appsettings.json                            # Cấu hình chung
├── appsettings.Development.json                # Cấu hình dev
├── README.md                                   # Hướng dẫn project
```

## 3. Mô Tả Tầng
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

## 4. Công Cụ và Thư Viện
- **.NET 8.0**: Framework chính.
- **EF Core 8.0**: ORM cho database.
- **Redis**: Caching dữ liệu `Field`, `SubField`, `FieldPricing`.
- **Cloudinary**: Quản lý ảnh sân (`FieldImage`).
- **SendGrid**: Gửi email xác thực, thông báo.
- **VNPay**: Xử lý thanh toán.
- **Google Maps API**: Tìm kiếm sân theo vị trí.
- **Serilog**: Logging.
- **Swagger**: Tài liệu API.
- **xUnit/NUnit**: Unit testing.
- **Moq**: Mocking cho unit tests.

## 5. Hướng Dẫn Setup
1. Clone repository: `git clone https://github.com/cuthanhcam/C4F-ISports-Backend.git`.
2. Cài đặt dependencies: `dotnet restore`.
3. Cấu hình environment variables trong `appsettings.json` (Cloudinary, SendGrid, VNPay, Redis, SQL Server).
4. Chạy migrations: `dotnet ef database update`.
5. Chạy ứng dụng: `dotnet run`.
6. Truy cập Swagger: `https://localhost:5231/swagger`.

## 6. Quy Tắc Đặt Tên
- **File/Class**: PascalCase (e.g., `SubFieldService.cs`).
- **Method**: PascalCase (e.g., `GetSubFieldsAsync`).
- **Variable**: camelCase (e.g., `subFieldId`).
- **DTO**: Kết thúc bằng `Dto` (e.g., `SubFieldDto`).
- **Configuration**: Kết thúc bằng `Configuration` (e.g., `SubFieldConfiguration`).
- **Seeder**: Kết thúc bằng `Seeder` (e.g., `SubFieldSeeder`).