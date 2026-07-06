using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using OrderFlow.Application.Commands.PlaceOrder;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Application.Mappings;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Events;

namespace OrderFlow.Tests.Unit;

public class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository>   _orders   = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IPublisher>         _publisher = new();
    private readonly Mock<ICacheService>      _cache    = new();
    private readonly IMapper                  _mapper;

    public PlaceOrderCommandHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    private PlaceOrderCommandHandler CreateHandler() => new(
        _orders.Object, _products.Object, _publisher.Object,
        _cache.Object, _mapper, Mock.Of<ILogger<PlaceOrderCommandHandler>>());

    // ── Happy path ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidOrder_CreatesOrderAndPublishesEvent()
    {
        var product = new Product { Id = 1, Name = "Laptop", Price = 999m, Stock = 10, Category = "Electronics", Description = "Test" };
        _products.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(product);
        _products.Setup(r => r.DecrementStockAsync(1, 2, default)).ReturnsAsync(true);

        var createdOrder = new Order
        {
            Id          = 42,
            UserId      = 7,
            TotalAmount = 1998m,
            OrderItems  = new List<OrderItem>
            {
                new() { Id = 1, ProductId = 1, Product = product, Quantity = 2, UnitPrice = 999m }
            },
            User = new User { Id = 7, Username = "testuser", Email = "t@t.com", PasswordHash = "x" }
        };
        _orders.Setup(r => r.CreateAsync(It.IsAny<Order>(), default)).ReturnsAsync(createdOrder);

        var command = new PlaceOrderCommand(7, [new PlaceOrderItemDto(1, 2)], "Rush please");
        var result  = await CreateHandler().Handle(command, default);

        Assert.True(result.Success);
        Assert.Equal(42, result.Data!.Id);
        Assert.Equal(1998m, result.Data.TotalAmount);
        _publisher.Verify(p => p.Publish(It.IsAny<OrderPlacedDomainEvent>(), default), Times.Once);
    }

    // ── Product not found ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ProductNotFound_ReturnsFailure()
    {
        _products.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Product?)null);

        var command = new PlaceOrderCommand(1, [new PlaceOrderItemDto(99, 1)], null);
        var result  = await CreateHandler().Handle(command, default);

        Assert.False(result.Success);
        Assert.Contains("99", result.Message);
        _orders.Verify(r => r.CreateAsync(It.IsAny<Order>(), default), Times.Never);
    }

    // ── Insufficient stock ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_InsufficientStock_ReturnsFailure()
    {
        var product = new Product { Id = 2, Name = "Keyboard", Price = 50m, Stock = 1, Category = "Electronics", Description = "Test" };
        _products.Setup(r => r.GetByIdAsync(2, default)).ReturnsAsync(product);

        var command = new PlaceOrderCommand(1, [new PlaceOrderItemDto(2, 5)], null);
        var result  = await CreateHandler().Handle(command, default);

        Assert.False(result.Success);
        Assert.Contains("stock", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── Empty items ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmptyItems_ReturnsFailure()
    {
        var command = new PlaceOrderCommand(1, [], null);
        var result  = await CreateHandler().Handle(command, default);

        Assert.False(result.Success);
    }
}
