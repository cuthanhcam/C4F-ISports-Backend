// Code mẫu cho file Program.cs trong ứng dụng ASP.NET Core Web API.
// Các bước cần thực hiện:
// 1. DbContext: Kết nối với SQL Server bằng chuỗi kết nối trong appsettings.json.
// 2. JWT Authentication: Sử dụng JwtBearer để xác thực token dựa trên JwtSettings. Bạn cần triển khai AuthService để tạo token.
// 3. CloudinaryService: Đăng ký như một scoped service để sử dụng trong các controller.
// 4. CORS: Cho phép tất cả origin trong môi trường phát triển (có thể hạn chế trong production).
// 5. Controllers: Thêm hỗ trợ cho các API controller.
// 6. Cấu hình Rate Limiting
// 7. Swagger: Thêm Swagger để kiểm thử API trong môi trường phát triển.
// 8. Middleware: Sắp xếp đúng thứ tự: CORS -> Authentication -> Authorization -> Controllers.
// 9s. Seeding: Gọi SeedData.Initialize trong scope để thêm dữ liệu mẫu khi ứng dụng khởi động.

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using api.Data;
using api.Services;
using api.Interfaces;
using Microsoft.OpenApi.Models;
using CloudinaryDotNet;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

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
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings");
builder.Services.AddSingleton(new Cloudinary(new Account(
    cloudinarySettings["CloudName"],
    cloudinarySettings["ApiKey"],
    cloudinarySettings["ApiSecret"]
)));
builder.Services.AddScoped<CloudinaryService>();

// Thêm HttpClient và GeocodingService với timeout
builder.Services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// 4. Đăng ký các service khác
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFieldService, FieldService>();
builder.Services.AddSingleton<IEmailSender, SendGridEmailSender>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

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

// 6. Cấu hình Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? "anonymous",
            partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 5,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

// 7. Thêm Controllers
builder.Services.AddControllers();

// 8. Thêm Swagger để kiểm thử API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "C4F ISports API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field (e.g., 'Bearer {token}')",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

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
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

// 9. Áp dụng migrations và seed dữ liệu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Starting database migration and seeding...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync(); // Đổi thành async
        logger.LogInformation("Migrations applied successfully.");
        await SeedData.InitializeAsync(services); // Thêm await
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.Run();