using AppRepository.Categories;
using AppRepository.Connect;
using AppRepository.Context;
using AppRepository.Products;
using AppRepository.Repository;
using AppRepository.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppRepository.Extentions
{
    public static class RepositoryExtentions
    {
        public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContextcs>(options =>
            {
                var connnectionStrings = configuration.GetSection(ConnectionStringOption.Key).Get<ConnectionStringOption>();
                options.UseNpgsql(connnectionStrings!.ConnectionStrings, defaultConnectionOptionAction =>
                {
                    defaultConnectionOptionAction.MigrationsAssembly(typeof(RepositoryAssembly).Assembly.FullName);
                });
           
            });
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //Addscoped veri tabanına gitme durumunda kullan, bağzı durumlarda singleton kullanılabilir. İyice bu durumu araştır.
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            return services;
        }
    }
}
