using AppService.Products;

namespace AppService.Categories.Dto
{
    public record CategoryWithProductDto(int CategoryId, string CategoryName, List<ProductDto> Products);

}
