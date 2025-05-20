# Install Packages C4F-ISports v2.0.0

Dưới đây là các lệnh CLI để cài đặt tất cả các gói NuGet cần thiết cho dự án, dựa trên file `api.csproj`. Hãy chạy các lệnh này trong thư mục chứa file `api.csproj` để đảm bảo các gói được thêm vào đúng dự án.

```bash
dotnet add package AutoMapper --version 12.0.1
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package CloudinaryDotNet --version 1.27.0
dotnet add package Hangfire.AspNetCore --version 1.8.14
dotnet add package Mailjet.Api --version 3.0.0
dotnet add package Microsoft.AspNetCore.Authentication.Google --version 8.0.3
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.3
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.3
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson --version 8.0.3
dotnet add package Microsoft.AspNetCore.OpenApi --version 8.0.3
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.3
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.3
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.3
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis --version 8.0.3
dotnet add package Microsoft.Extensions.Configuration --version 9.0.0
dotnet add package Microsoft.Extensions.Configuration.EnvironmentVariables --version 9.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 9.0.0
dotnet add package Microsoft.Extensions.DependencyInjection --version 9.0.0
dotnet add package Microsoft.Extensions.Http --version 9.0.0
dotnet add package Microsoft.Extensions.Http.Polly --version 8.0.3
dotnet add package Microsoft.Extensions.Logging --version 9.0.0
dotnet add package AspNetCore.HealthChecks.Redis --version 9.0.0
dotnet add package AspNetCore.HealthChecks.SqlServer --version 8.0.2
dotnet add package Newtonsoft.Json --version 13.0.3
dotnet add package Polly --version 8.5.2
dotnet add package Polly.Extensions.Http --version 3.0.0
dotnet add package SendGrid --version 9.29.3
dotnet add package Serilog.AspNetCore --version 8.0.3
dotnet add package Serilog.Sinks.Console --version 6.0.0
dotnet add package Serilog.Sinks.File --version 7.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.7.3
```

## Lưu ý

- Đảm bảo bạn đang ở đúng thư mục chứa file `api.csproj` khi chạy các lệnh trên.
- Sau khi chạy các lệnh, hãy chạy `dotnet restore` để đảm bảo tất cả các gói được tải xuống và tích hợp đúng cách.
- Nếu bạn gặp bất kỳ lỗi nào liên quan đến phiên bản gói, hãy kiểm tra file `api.csproj` để đảm bảo rằng các phiên bản được chỉ định khớp với các lệnh trên.
