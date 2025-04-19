using AppRepository.Categories;

namespace AppRepository.Products
{
    public class Product : IAuditEntitiy
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
