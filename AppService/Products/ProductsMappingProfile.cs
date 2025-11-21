using AppRepository.Products;
using AppService.Products.Create;
using AppService.Products.Update;
using AutoMapper;

namespace AppService.Products
{
    public class ProductsMappingProfile : Profile
    {
        public ProductsMappingProfile()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId));

            CreateMap<CreateProductRequest, Product>().ForMember(
                dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName.ToLowerInvariant()));

            CreateMap<UpdateProductRequest, Product>().ForMember(
               dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName.ToLowerInvariant()));

        }
    }
}
