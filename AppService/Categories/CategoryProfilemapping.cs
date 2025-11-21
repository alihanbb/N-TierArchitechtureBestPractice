using AppRepository.Categories;
using AppRepository.Products;
using AppService.Categories.Create;
using AppService.Categories.Dto;
using AppService.Categories.Update;
using AutoMapper;

namespace AppService.Categories
{
    public class CategoryProfilemapping : Profile
    {
       public CategoryProfilemapping() 
        {
            CreateMap<Category, CategoryWithProductDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
                .ReverseMap()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id));

            CreateMap<CreateCategoryRequest, Category>().ForMember(
                  dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName.ToLowerInvariant()));

            CreateMap<UpdateCategoryRequest, Category>().ForMember(
               dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName.ToLowerInvariant()));
        }
    }
}
