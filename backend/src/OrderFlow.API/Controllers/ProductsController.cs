using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Commands.CreateProduct;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Queries.GetProductById;
using OrderFlow.Application.Queries.GetProducts;

namespace OrderFlow.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get paginated + searchable product catalogue (public).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 12,
        [FromQuery] string? category = null,
        [FromQuery] string? search   = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetProductsQuery(page, pageSize, category, search), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single product by ID (public).</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new product (Admin only).</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateProductCommand(dto), cancellationToken);
        if (!result.Success) return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }
}
