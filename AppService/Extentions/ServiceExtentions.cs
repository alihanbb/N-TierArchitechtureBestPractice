using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AppService.Products;
using FluentValidation.AspNetCore;
using FluentValidation;
using System.Reflection;
using AppService.ExceptionHandlers;
using AppService.Categories;

namespace AppService.Extentions
{
    public static class ServiceExtentions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
           services.AddScoped<IProductService, ProductService>();
           services.AddScoped<ICategoryService, CategoryService>();

           services.AddFluentValidationAutoValidation();

           services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
           
           services.AddAutoMapper(Assembly.GetExecutingAssembly());

           services.AddExceptionHandler<CriticalExsepsionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();



            return services;
        }
    }
}
