using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Commands.CancelOrder;
using OrderFlow.Application.Commands.PlaceOrder;
using OrderFlow.Application.Commands.UpdateOrderStatus;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Queries.GetOrderById;
using OrderFlow.Application.Queries.GetOrders;
using OrderFlow.Domain.Enums;

namespace OrderFlow.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator                  _mediator;
    private readonly ILogger<OrdersController>  _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    private int  UserId  => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    /// <summary>Get paginated orders (admin sees all; customers see their own).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetOrdersQuery(UserId, IsAdmin, page, pageSize, status), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single order by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id, UserId, IsAdmin), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Place a new order.</summary>
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new PlaceOrderCommand(UserId, request.Items, request.Notes), cancellationToken);

        if (!result.Success) return BadRequest(result);

        _logger.LogInformation("Order {OrderId} placed by user {UserId}", result.Data!.Id, UserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
    }

    /// <summary>Update order status (Admin only for Processing→Shipped→Delivered; customer can cancel).</summary>
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateOrderStatusCommand(id, UserId, request.Status, IsAdmin), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cancel an order (Pending or Processing only).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelOrderCommand(id, UserId, IsAdmin), cancellationToken);
        return result.Success ? NoContent() : BadRequest(result);
    }
}

public record PlaceOrderRequest(List<PlaceOrderItemDto> Items, string? Notes);
public record UpdateStatusRequest(OrderStatus Status);
