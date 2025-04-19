using AppRepository.Products;

namespace AppRepository.Categories
 {
    public class Category : IAuditEntitiy
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public List<Product>? Products { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
