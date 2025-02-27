using AppRepository.Context;
using AppRepository.Repository;
using Microsoft.EntityFrameworkCore;

namespace AppRepository.Products
{
    public class ProductRepository(AppDbContextcs contextcs) : GenericRepository<Product>(contextcs), IProductRepository
    {
        public Task<IEnumerable<object>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>> GetTopPriceProductAsync(int count)
        {
           return  Contextcs.Products.OrderByDescending(x => x.Price).Take(count).ToListAsync();
        }
    }
}
