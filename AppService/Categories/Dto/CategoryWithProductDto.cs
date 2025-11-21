using AppService.Products;

namespace AppService.Categories.Dto
{
    public class CategoryWithProductDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = default!;
        public List<ProductDto> Products { get; set; } = new();
    }
}
