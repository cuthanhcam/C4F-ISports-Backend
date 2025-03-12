// Code mẫu cho file Program.cs trong ứng dụng ASP.NET Core Web API.
// Các bước cần thực hiện:
// 1. DbContext: Kết nối với SQL Server bằng chuỗi kết nối trong appsettings.json.
// 2. JWT Authentication: Sử dụng JwtBearer để xác thực token dựa trên JwtSettings. Bạn cần triển khai AuthService để tạo token.
// 3. CloudinaryService: Đăng ký như một scoped service để sử dụng trong các controller.
// 4. CORS: Cho phép tất cả origin trong môi trường phát triển (có thể hạn chế trong production).
// 5. Controllers: Thêm hỗ trợ cho các API controller.
// 6. Swagger: Thêm Swagger để kiểm thử API trong môi trường phát triển.
// 7. Middleware: Sắp xếp đúng thứ tự: CORS -> Authentication -> Authorization -> Controllers.
// 8. Seeding: Gọi SeedData.Initialize trong scope để thêm dữ liệu mẫu khi ứng dụng khởi động.

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using api.Data;
using api.Services;
using api.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Cấu hình DbContext với SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]))
        };
    });

// 3. Cấu hình CloudinaryService
builder.Services.AddScoped<CloudinaryService>();

// 4. Đăng ký các service khác (sẽ thêm sau khi bạn triển khai)
// builder.Services.AddScoped<IAuthService, AuthService>(); // Ví dụ, bạn sẽ cần triển khai AuthService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IEmailSender, SendGridEmailSender>();

// 5. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// 6. Thêm Controllers
builder.Services.AddControllers();

// 7. Thêm Swagger để kiểm thử API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Redirect HTTP sang HTTPS (không cần trong môi trường phát triển)

// Áp dụng CORS
app.UseCors("AllowAll");

// Middleware Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 8. Áp dụng migrations và seed dữ liệu
// Seed dữ liệu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting database migration and seeding...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        logger.LogInformation("Migrations applied successfully.");
        SeedData.Initialize(services);
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.Run();