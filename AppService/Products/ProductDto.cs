using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppService.Products
{
    public record ProductDto(int ProductId, string ProductName, decimal Price, int Stock);
}