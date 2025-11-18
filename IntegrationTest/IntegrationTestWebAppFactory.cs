using AppRepository.Context;
using AppRepository.Categories;
using AppRepository.Products;
using AppRepository.Repository;
using AppRepository.UnitOfWorks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

namespace IntegrationTest;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"IntegrationTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureTestServices(services =>
        {
            // InMemory veritabaný ekle - Her factory instance için ayný unique isim
            services.AddDbContext<AppDbContextcs>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
            
            // Repository servisleri ekle
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
        });
    }

    public void InitializeDbForTests()
    {
        using var scope = Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<AppDbContextcs>();
        
        // Mevcut verileri temizle
        if (db.Products.Any())
        {
            db.Products.RemoveRange(db.Products.ToList());
            db.SaveChanges();
        }
        if (db.Categorys.Any())
        {
            db.Categorys.RemoveRange(db.Categorys.ToList());
            db.SaveChanges();
        }
        
        // Seed data
        SeedData(db);
    }

    private static void SeedData(AppDbContextcs context)
    {
        // Kategoriler ekle
        context.Categorys.AddRange(
            new Category { CategoryId = 1, CategoryName = "Elektronik", Created = DateTime.UtcNow },
            new Category { CategoryId = 2, CategoryName = "Giyim", Created = DateTime.UtcNow },
            new Category { CategoryId = 3, CategoryName = "Kitap", Created = DateTime.UtcNow }
        );
        context.SaveChanges();

        // Ürünler ekle
        context.Products.AddRange(
            new Product
            {
                ProductId = 1,
                ProductName = "Laptop",
                Price = 15000,
                Stock = 10,
                CategoryId = 1,
                Created = DateTime.UtcNow
            },
            new Product
            {
                ProductId = 2,
                ProductName = "Mouse",
                Price = 150,
                Stock = 50,
                CategoryId = 1,
                Created = DateTime.UtcNow
            },
            new Product
            {
                ProductId = 3,
                ProductName = "T-Shirt",
                Price = 200,
                Stock = 100,
                CategoryId = 2,
                Created = DateTime.UtcNow
            }
        );
        context.SaveChanges();
    }
}
