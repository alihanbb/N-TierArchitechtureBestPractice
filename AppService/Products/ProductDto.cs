using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppService.Products
{
    public record ProductDto(int ProductId, string ProductName, decimal Price, int Stock);
    //                 YADA
    //public record ProductDto
    //{
    //    public int ProductId { get; init; }
    //    public string ProductName { get; init; }
    //    public decimal Price { get; init; }
    //    public int Stock { get; init; }
    //}
}
