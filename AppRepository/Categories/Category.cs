using AppRepository.Products;

namespace AppRepository.Categories
 {
    public class Category : BaseEntity<int>,IAuditEntitiy
    {
        public string CategoryName { get; set; }

        public List<Product>? Products { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
