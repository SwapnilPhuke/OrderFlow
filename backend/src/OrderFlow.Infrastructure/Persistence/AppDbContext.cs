using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User>         Users         { get; set; }
    public DbSet<Product>      Products      { get; set; }
    public DbSet<Order>        Orders        { get; set; }
    public DbSet<OrderItem>    OrderItems    { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50);
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).IsRequired().HasMaxLength(1000);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.Category).IsRequired().HasMaxLength(100);
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.User).WithMany(u => u.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.Status);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Ignore(x => x.LineTotal);
            e.HasOne(x => x.Order).WithMany(o => o.OrderItems).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(p => p.OrderItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed admin user (password: Admin@123456)
        modelBuilder.Entity<User>().HasData(new User
        {
            Id           = 1,
            Username     = "admin",
            Email        = "admin@orderflow.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
            Role         = "Admin",
            CreatedAt    = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Seed 10 products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop Pro 15",        Description = "High-performance laptop with 16GB RAM and 512GB SSD",  Price = 1299.99m, Stock = 25, Category = "Electronics", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 2, Name = "Wireless Mouse",       Description = "Ergonomic wireless mouse with 6-month battery life",    Price = 49.99m,  Stock = 150, Category = "Electronics", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 3, Name = "Mechanical Keyboard",  Description = "RGB backlit mechanical keyboard with Cherry MX switches", Price = 129.99m, Stock = 80, Category = "Electronics", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 4, Name = "4K Monitor 27\"",      Description = "Ultra HD IPS monitor with HDR400 and 144Hz refresh rate",  Price = 549.99m, Stock = 40, Category = "Electronics", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 5, Name = "USB-C Hub 7-in-1",     Description = "Multi-port USB-C hub with HDMI, USB 3.0, SD card reader", Price = 69.99m,  Stock = 200, Category = "Accessories", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 6, Name = "Noise-Cancel Headphones", Description = "Over-ear headphones with ANC and 30-hour battery",   Price = 299.99m, Stock = 60, Category = "Audio",       CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 7, Name = "Webcam 4K",            Description = "4K webcam with auto-focus and built-in ring light",     Price = 149.99m, Stock = 90, Category = "Electronics", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 8, Name = "Standing Desk Mat",    Description = "Anti-fatigue cushioned mat for standing desks",         Price = 89.99m,  Stock = 120, Category = "Office",      CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 9, Name = "SSD 1TB External",     Description = "Portable NVMe SSD with USB 3.2 Gen 2 — up to 1050 MB/s", Price = 109.99m, Stock = 8,  Category = "Storage",    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 10, Name = "Smart Desk Lamp",     Description = "LED desk lamp with wireless charging pad and USB port",  Price = 59.99m,  Stock = 5,  Category = "Office",     CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
