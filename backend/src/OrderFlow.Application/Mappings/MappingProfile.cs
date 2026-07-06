using AutoMapper;
using OrderFlow.Application.DTOs;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderResponseDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Username, o => o.MapFrom(s => s.User != null ? s.User.Username : string.Empty))
            .ForMember(d => d.OrderItems, o => o.MapFrom(s => s.OrderItems));

        CreateMap<Order, OrderSummaryDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.OrderItems.Count));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.LineTotal));

        CreateMap<Product, ProductResponseDto>();
        CreateMap<Product, ProductSummaryDto>();
        CreateMap<CreateProductDto, Product>();
    }
}
