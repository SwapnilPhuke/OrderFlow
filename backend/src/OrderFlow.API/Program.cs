using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using OrderFlow.Application;
using OrderFlow.Infrastructure;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.API.Middleware;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext());

    // ── Layers ─────────────────────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── JWT Auth ───────────────────────────────────────────────────────────
    var jwt = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwt["SecretKey"]
        ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

    builder.Services
        .AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer           = true,
                ValidIssuer              = jwt["Issuer"]   ?? "OrderFlow.API",
                ValidateAudience         = true,
                ValidAudience            = jwt["Audience"] ?? "OrderFlow.Client",
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization(opt =>
        opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));

    // ── API Versioning ─────────────────────────────────────────────────────
    builder.Services.AddApiVersioning(opt =>
    {
        opt.DefaultApiVersion                   = new ApiVersion(1, 0);
        opt.AssumeDefaultVersionWhenUnspecified = true;
        opt.ReportApiVersions                   = true;
        opt.ApiVersionReader                    = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    }).AddApiExplorer(opt =>
    {
        opt.GroupNameFormat           = "'v'VVV";
        opt.SubstituteApiVersionInUrl = true;
    });

    // ── Rate Limiting ──────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(opt =>
    {
        opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit          = 100,
                    Window               = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit           = 5
                }));

        opt.OnRejected = async (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await ctx.HttpContext.Response.WriteAsync("Too many requests. Try again later.", token);
        };
    });

    // ── Health Checks ──────────────────────────────────────────────────────
    var healthChecks = builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database");

    var redisConn = builder.Configuration["Redis:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(redisConn))
        healthChecks.AddRedis(redisConn, "redis");

    // ── CORS ───────────────────────────────────────────────────────────────
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];

    builder.Services.AddCors(opt =>
        opt.AddPolicy("AllowFrontend", p =>
            p.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

    // ── Swagger ────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "OrderFlow API",
            Version     = "v1",
            Description = "CQRS-based Order Management System — .NET 9 / MediatR / Redis / RabbitMQ"
        });

        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Type         = SecuritySchemeType.ApiKey,
            Scheme       = "Bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Description  = "Enter: Bearer {your JWT token}"
        });

        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── Build ──────────────────────────────────────────────────────────────
    var app = builder.Build();

    // Auto-migrate on startup (skip for in-memory/test)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.IsRelational())
            db.Database.Migrate();
        else
            db.Database.EnsureCreated();
    }

    // ── Middleware pipeline ────────────────────────────────────────────────
    app.UseGlobalExceptionHandler();
    app.UseSecurityHeaders();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderFlow API v1"));
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrderFlow API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Required for WebApplicationFactory in integration tests
public partial class Program { }
