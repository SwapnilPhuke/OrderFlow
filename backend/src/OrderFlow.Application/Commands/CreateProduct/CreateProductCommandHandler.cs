using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<ProductResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<ProductResponseDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            Price = request.Dto.Price,
            Stock = request.Dto.Stock,
            Category = request.Dto.Category
        };

        var created = await _productRepository.CreateAsync(product, cancellationToken);

        await _cacheService.RemoveByPrefixAsync("products:");

        _logger.LogInformation("Product '{ProductName}' (ID: {ProductId}) created", created.Name, created.Id);

        var dto = _mapper.Map<ProductResponseDto>(created);
        return ApiResponse<ProductResponseDto>.Ok(dto, "Product created successfully.");
    }
}
