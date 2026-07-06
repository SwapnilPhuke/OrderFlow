using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;

namespace OrderFlow.Application.Queries.GetProducts;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 12,
    string? Category = null,
    string? Search = null) : IRequest<ApiResponse<PaginatedResult<ProductResponseDto>>>;
