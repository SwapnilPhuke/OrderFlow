using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Common;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;

namespace OrderFlow.Application.Queries.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ApiResponse<PaginatedResult<ProductResponseDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<GetProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<PaginatedResult<ProductResponseDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products:{request.Page}:{request.PageSize}:{request.Category ?? "all"}:{request.Search ?? "none"}";

        var cached = await _cacheService.GetAsync<PaginatedResult<ProductResponseDto>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Products cache hit for key {CacheKey}", cacheKey);
            return ApiResponse<PaginatedResult<ProductResponseDto>>.Ok(cached);
        }

        var (products, totalCount) = await _productRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Category, request.Search, cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        var result = PaginatedResult<ProductResponseDto>.Create(dtos, totalCount, request.Page, request.PageSize);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return ApiResponse<PaginatedResult<ProductResponseDto>>.Ok(result);
    }
}
