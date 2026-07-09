using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Interfaces;
using OrderFlow.Infrastructure.Messaging.Consumers;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Tests.Integration;

public class OrderFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // ── DbContext: replace SQL Server with InMemory ─────────────────
            // Must remove BOTH DbContextOptions<AppDbContext> AND all
            // IDbContextOptionsConfiguration<AppDbContext> entries — EF Core 7+
            // stores each options-configuration lambda as a separate
            // IDbContextOptionsConfiguration registration. Leaving the SQL Server
            // one in place causes both providers to apply together and throws.
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

            var optionsConfigs = services
                .Where(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>))
                .ToList();
            foreach (var d in optionsConfigs) services.Remove(d);

            // Capture the name OUTSIDE the lambda so every DbContext within
            // this factory instance shares the same in-memory database.
            // (Guid inside the lambda would create a new DB per request,
            // breaking register-then-login and duplicate-username checks.)
            var dbName = $"OrderFlowTest_{Guid.NewGuid()}";
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase(dbName));

            // ── MassTransit: replace RabbitMQ with InMemory ─────────────────
            // Remove all MassTransit registrations (bus, health checks, hosted
            // services) so the factory does not attempt a RabbitMQ connection.
            var massTransitDescriptors = services
                .Where(d =>
                    d.ServiceType?.Namespace?.StartsWith("MassTransit") == true ||
                    d.ImplementationType?.Namespace?.StartsWith("MassTransit") == true ||
                    d.ImplementationInstance?.GetType().Namespace?.StartsWith("MassTransit") == true)
                .ToList();
            foreach (var d in massTransitDescriptors)
                services.Remove(d);

            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderPlacedConsumer>();
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            });

            // ── Redis: replace with no-op cache ─────────────────────────────
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
