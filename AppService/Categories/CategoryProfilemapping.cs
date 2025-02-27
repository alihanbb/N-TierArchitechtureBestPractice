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
            CreateMap<Category, CategoryWithProductDto>().ReverseMap();

            CreateMap<CategoryDto,Category>().ReverseMap();

            CreateMap<CreateCategoryRequest, Category>().ForMember(
                  dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName.ToLowerInvariant()));

            CreateMap<UpdateCategoryRequest, Category>().ForMember(
               dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName.ToLowerInvariant()));
        }
    }
}
