using AutoMapper;
using MediatR;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;

namespace OrderFlow.Application.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ApiResponse<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProductResponseDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            return ApiResponse<ProductResponseDto>.Fail($"Product {request.ProductId} not found.");

        var dto = _mapper.Map<ProductResponseDto>(product);
        return ApiResponse<ProductResponseDto>.Ok(dto);
    }
}
