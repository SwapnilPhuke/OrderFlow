using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;

namespace OrderFlow.Application.Queries.GetProductById;

public record GetProductByIdQuery(int ProductId) : IRequest<ApiResponse<ProductResponseDto>>;
