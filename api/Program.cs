using api.Data;
// using api.Services;
// using api.Repositories;
// using api.Helpers;
// using api.Middlewares;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var services = builder.Services;

// Cấu hình Database với Entity Framework Core
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// Đăng ký Identity & Authentication (JWT Bearer)
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// Cấu hình Cloudinary để upload ảnh
services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
services.AddScoped<CloudinaryService>();

// Cấu hình CORS để frontend có thể gọi API
services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000") // Thay bằng URL frontend của bạn
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Đăng ký Services & Repositories
// services.AddScoped<IAuthService, AuthService>();
// services.AddScoped<IFieldService, FieldService>();
// services.AddScoped<IBookingService, BookingService>();
// services.AddScoped<IPaymentService, PaymentService>();
// services.AddScoped<IReviewService, ReviewService>();
// services.AddScoped<INotificationService, NotificationService>();
// services.AddScoped<IPromotionService, PromotionService>();
// services.AddScoped<IAdminService, AdminService>();
// services.AddScoped<IUnitOfWork, UnitOfWork>();

// services.AddScoped<IAuthRepository, AuthRepository>();
// services.AddScoped<IFieldRepository, FieldRepository>();
// services.AddScoped<IBookingRepository, BookingRepository>();
// services.AddScoped<IPaymentRepository, PaymentRepository>();
// services.AddScoped<IReviewRepository, ReviewRepository>();
// services.AddScoped<INotificationRepository, NotificationRepository>();
// services.AddScoped<IPromotionRepository, PromotionRepository>();
// services.AddScoped<IAdminRepository, AdminRepository>();

// Controllers và Swagger
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "C4F-ISport API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer <token>'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Cấu hình Middleware
app.UseCors("AllowFrontend");
// app.UseMiddleware<ExceptionHandlingMiddleware>(); // Xử lý lỗi toàn cục
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "C4F-ISport API v1"));

app.MapControllers();
app.Run();
