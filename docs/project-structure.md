# Cấu Trúc Project Backend C4F-ISports v2.0.0

## 1. Tổng Quan
Backend sử dụng .NET 8.0 với Layered Architecture, chia thành các tầng: Data, Models, Interfaces, Repositories, Services, Dtos, Controllers, Middlewares. Cấu trúc được tối ưu để hỗ trợ sân lớn/sân nhỏ và OAuth2.

## 2. Cấu Trúc Thư Mục
```
api/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── SeedData.cs
│   ├── UnitOfWork.cs
│   ├── Migrations/
│   ├── Configurations/
│   │   ├── AccountConfiguration.cs
│   │   ├── SubFieldConfiguration.cs
│   │   ├── ...
│   ├── Seeders/
│   │   ├── AccountSeeder.cs
│   │   ├── SubFieldSeeder.cs
│   │   ├── ...
├── Models/
│   ├── Account.cs
│   ├── SubField.cs
│   ├── ...
├── Interfaces/
│   ├── IAuthService.cs
│   ├── ISubFieldService.cs
│   ├── ...
├── Repositories/
│   ├── GenericRepository.cs
│   ├── SubFieldRepository.cs
│   ├── ...
├── Services/
│   ├── AuthService.cs
│   ├── SubFieldService.cs
│   ├── ...
├── Dtos/
│   ├── Auth/
│   │   ├── LoginDto.cs
│   │   ├── ...
│   ├── SubField/
│   │   ├── SubFieldDto.cs
│   │   ├── ...
│   ├── ...
├── Controllers/
│   ├── AuthController.cs
│   ├── SubFieldController.cs
│   ├── ...
├── Middlewares/
│   ├── RoleMiddleware.cs
├── Configurations/
│   ├── OAuthConfig.cs
├── Utils/
│   ├── PasswordHasher.cs
├── BackgroundServices/
│   ├── NotificationCleanupService.cs
├── Exceptions/
│   ├── AppException.cs
│   ├── ResourceNotFoundException.cs
├── Properties/
│   ├── launchSettings.json
├── .gitignore
├── Program.cs
├── Startup.cs
├── api.csproj
├── appsettings.json
├── appsettings.Development.json
```

## 3. Mô Tả Tầng
- **Data**: Quản lý database, migrations, seeding.
- **Models**: Entity ánh xạ với database, bao gồm `SubField`.
- **Interfaces**: Định nghĩa hợp đồng cho service/repository.
- **Repositories**: Tương tác với database qua EF Core.
- **Services**: Xử lý logic nghiệp vụ, bao gồm `SubFieldService`.
- **Dtos**: Chuyển đổi dữ liệu giữa client và server.
- **Controllers**: Xử lý request từ client.
- **Middlewares**: Xử lý xác thực, phân quyền.
- **BackgroundServices**: Xử lý tác vụ nền.