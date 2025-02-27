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
            CreateMap<Product, ProductDto>().ReverseMap();

            CreateMap<CreateProductRequest, Product>().ForMember(
                dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName.ToLowerInvariant()));

            CreateMap<UpdateProductRequest, Product>().ForMember(
               dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName.ToLowerInvariant()));

        }
    }
}
