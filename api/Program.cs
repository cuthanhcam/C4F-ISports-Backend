using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using api.Data;
using api.Interfaces;
using api.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Google;
using Serilog;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using api.Models;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
});

// Cấu hình các dịch vụ
ConfigureServices(builder);

// Xây dựng ứng dụng
var app = builder.Build();

// Cấu hình pipeline
ConfigureMiddleware(app);

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Preparing to run SeedDatabaseAsync...");

// Áp dụng migrations và seed dữ liệu
await SeedDatabaseAsync(app);

logger.LogInformation("Application startup completed.");

app.Run();

void ConfigureServices(WebApplicationBuilder builder)
{
    // 1. Cấu hình DbContext với SQL Server
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)));

    // 2. Cấu hình Authentication (JWT + Google OAuth2)
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudiences = new[] { jwtSettings["Audience"] },
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]))
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["OAuth:Google:ClientId"];
        options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"];
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

    // 3. Cấu hình Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("OwnerOnly", policy => policy.RequireRole("Owner"));
        options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    });

    // 4. Cấu hình CloudinaryService
    builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
    builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

    // 5. Đăng ký các service
    builder.Services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);
    });
    builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();

    // 6. Cấu hình CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
    });

    // 7. Cấu hình Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("auth", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.User.Identity?.Name ?? "anonymous",
                partition => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueLimit = 5,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));
        options.AddPolicy("booking", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.User.Identity?.Name ?? "anonymous",
                partition => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueLimit = 2
                }));
        options.AddPolicy("fields", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.User.Identity?.Name ?? "anonymous",
                partition => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueLimit = 10
                }));
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = "Rate limit exceeded. Please try again later."
            });
        };
    });

    // 8. Cấu hình Redis Cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "C4F-ISports-";
    });

    // 9. Cấu hình Health Checks
    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddRedis(builder.Configuration.GetConnectionString("Redis"));

    // 10. Thêm Controllers với JSON options
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });

    // 11. Cấu hình Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v2", new OpenApiInfo { Title = "C4F ISports API", Version = "v2.0.0" });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter JWT with Bearer (e.g., 'Bearer {token}')",
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
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Handling request: {Method} {Url}", context.Request.Method, context.Request.Path);
        await next.Invoke();
        logger.LogInformation("Finished handling request: {StatusCode}", context.Response.StatusCode);
    });

    app.UseCors("AllowSpecificOrigins");

    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();

    app.MapHealthChecks("/api/health");

    app.MapControllers();
}

async Task SeedDatabaseAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Starting database migration and seeding...");
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            var canConnect = await context.Database.CanConnectAsync();
            logger.LogInformation("Database connection status: {CanConnect}", canConnect);

            var accountCount = await context.Accounts.CountAsync();
            logger.LogInformation("Accounts table has {Count} records before seeding.", accountCount);

            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");

            await SeedData.InitializeAsync(services);
            logger.LogInformation("Database seeding completed successfully.");

            accountCount = await context.Accounts.CountAsync();
            logger.LogInformation("Accounts table has {Count} records after seeding.", accountCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database. StackTrace: {StackTrace}", ex.StackTrace);
            throw;
        }
    }
}