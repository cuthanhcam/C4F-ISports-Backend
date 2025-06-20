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
using Serilog;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using api.Repositories;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;
using System.Linq;
using api.Models;
using Microsoft.AspNetCore.Builder;
using VNPAY.NET;

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

// Cấu hình middleware
ConfigureMiddleware(app);

// Áp dụng migrations và seed dữ liệu
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting application...");

await SeedDatabaseAsync(app);

logger.LogInformation("Application startup completed.");

app.Run();

void ConfigureServices(WebApplicationBuilder builder)
{
    // DbContext với SQL Server
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)));

    // Authentication (JWT)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"])),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "sub"
        };
    });

    // Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("OwnerOnly", policy => policy.RequireRole("Owner"));
        options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    });

    // CloudinaryService
    builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
    builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

    // GeocodingService
    builder.Services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);
    });

    // Email Sender
    var retryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    builder.Services.AddHttpClient<IEmailSender, GmailSmtpEmailSender>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    }).AddPolicyHandler(retryPolicy);

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", corsBuilder =>
        {
            if (builder.Environment.IsDevelopment())
            {
                corsBuilder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
            }
            else
            {
                corsBuilder.WithOrigins(builder.Configuration["FEUrl"] ?? "http://localhost:5173")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
            }
        });
    });

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("auth", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.User.Identity?.Name ?? "anonymous",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 15,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueLimit = 5,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));

        options.AddPolicy("booking", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.User.Identity?.Name ?? "anonymous",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueLimit = 2
                }));

        options.AddPolicy("fields", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.User.Identity?.Name ?? "anonymous",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueLimit = 10
                }));

        options.AddPolicy("api", httpContext =>
            RateLimitPartition.GetSlidingWindowLimiter(
                httpContext.User.Identity?.Name ?? "anonymous",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 15,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 2,
                    QueueLimit = 5,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
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

    // Redis Cache
    if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("Redis")))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
            options.InstanceName = "C4F-ISports-";
        });
    }

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddCheck("API", () => HealthCheckResult.Healthy("API is running"));

    // Controllers
    builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        });

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v2", new OpenApiInfo
        {
            Title = "C4F ISports API",
            Version = "v2.0.0",
            Description = "API for C4F ISports application, supporting field booking, user management, and more."
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
        else
        {
            Console.WriteLine($"XML documentation file not found: {xmlPath}");
        }

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

        options.OperationFilter<FormFileOperationFilter>();
        options.DocumentFilter<SwaggerDocumentFilter>();
    });

    builder.Services.AddHttpContextAccessor();

    // Services và Repositories
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IEmailSender, GmailSmtpEmailSender>();
    builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
    builder.Services.AddScoped<IFieldService, api.Services.FieldService>();
    builder.Services.AddScoped<ISportService, SportService>();
    builder.Services.AddScoped<IPromotionService, PromotionService>();
    builder.Services.AddScoped<IBookingService, api.Services.BookingService>();
    builder.Services.Configure<VNPaySettings>(builder.Configuration.GetSection("VNPay"));
    builder.Services.AddSingleton<IVnpay, Vnpay>(); // Đăng ký IVnpayService
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();

    // Logging
    builder.Services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
        logging.AddEventSourceLogger();
        logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        logging.AddFilter("api.Services", LogLevel.Debug);
        logging.AddFilter("api.Controllers", LogLevel.Debug);
        logging.AddFilter("Swashbuckle", LogLevel.Debug);
    });
}

// Hàm cấu hình middleware
void ConfigureMiddleware(WebApplication app)
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception occurred. Path: {Path}", context.Request.Path);
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal Server Error",
                    message = exceptionHandlerPathFeature.Error.Message,
                    innerException = exceptionHandlerPathFeature.Error.InnerException?.Message,
                    stackTrace = app.Environment.IsDevelopment() ? exceptionHandlerPathFeature.Error.StackTrace : null
                });
            }
            else
            {
                logger.LogError("Unknown error occurred. Path: {Path}", context.Request.Path);
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal Server Error",
                    message = "An unknown error occurred."
                });
            }
        });
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "C4F ISports API v2");
            options.DocumentTitle = "C4F ISports API Documentation";
        });
    }

    app.Use(async (context, next) =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogDebug("Handling request: {Method} {Url}", context.Request.Method, context.Request.Path);
        await next.Invoke();
        logger.LogDebug("Finished handling request: {StatusCode}", context.Response.StatusCode);
    });

    app.UseCors("AllowSpecificOrigins");
    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();

    app.MapHealthChecks("/api/health");

    app.MapControllers();
}

// Hàm khởi tạo và seed dữ liệu vào database
async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
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

public class FormFileOperationFilter : IOperationFilter
{
    private readonly ILogger<FormFileOperationFilter> _logger;

    public FormFileOperationFilter(ILogger<FormFileOperationFilter> logger)
    {
        _logger = logger;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFileCollection))
            .ToList();

        if (fileParameters.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = fileParameters.ToDictionary(
                                p => p.Name,
                                p => new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            ),
                            Required = fileParameters.Select(p => p.Name).ToHashSet()
                        }
                    }
                },
                Description = "File upload (supports single or multiple files)",
                Required = true
            };

            _logger.LogDebug("Applied FormFileOperationFilter for action: {ActionName}", context.ApiDescription.ActionDescriptor.DisplayName);
        }
    }
}

public class SwaggerDocumentFilter : IDocumentFilter
{
    private readonly ILogger<SwaggerDocumentFilter> _logger;

    public SwaggerDocumentFilter(ILogger<SwaggerDocumentFilter> logger)
    {
        _logger = logger;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        try
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                _logger.LogDebug("Processing API: {HttpMethod} {RelativePath}", apiDescription.HttpMethod, apiDescription.RelativePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Swagger document: {Message}", ex.Message);
            throw;
        }
    }
}