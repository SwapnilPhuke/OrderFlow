using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;

namespace OrderFlow.Application.Commands.CreateProduct;

public record CreateProductCommand(CreateProductDto Dto) : IRequest<ApiResponse<ProductResponseDto>>;
