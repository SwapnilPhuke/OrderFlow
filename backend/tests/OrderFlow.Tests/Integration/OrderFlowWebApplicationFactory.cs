using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Interfaces;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Tests.Integration;

public class OrderFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server with in-memory DB
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase($"OrderFlowTest_{Guid.NewGuid()}"));

            // Replace Redis with no-op cache so tests don't need Redis
            var cacheDesc = services.SingleOrDefault(d => d.ServiceType == typeof(ICacheService));
            if (cacheDesc != null) services.Remove(cacheDesc);
            services.AddScoped<ICacheService, OrderFlow.Infrastructure.Caching.NullCacheService>();
        });

        builder.UseSetting("JwtSettings:SecretKey",
            "integration-test-secret-key-min-32-characters-long");
        builder.UseSetting("JwtSettings:Issuer",   "OrderFlow.API");
        builder.UseSetting("JwtSettings:Audience", "OrderFlow.Client");
        builder.UseSetting("JwtSettings:AccessTokenExpirationMinutes", "15");
        builder.UseSetting("JwtSettings:RefreshTokenExpirationDays",   "7");
        builder.UseSetting("Redis:ConnectionString", "");
    }
}
