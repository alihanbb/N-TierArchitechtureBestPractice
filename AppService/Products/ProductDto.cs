using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppService.Products
{
    public class ProductDto
    {
        public ProductDto()
        {
        }

        public ProductDto(int productId, string productName, decimal price, int stock)
        {
            ProductId = productId;
            ProductName = productName;
            Price = price;
            Stock = stock;
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}