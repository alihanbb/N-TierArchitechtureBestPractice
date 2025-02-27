using System.Reflection;
using AppRepository.Categories;
using AppRepository.Products;
using Microsoft.EntityFrameworkCore;

namespace AppRepository.Context
{
    public class AppDbContextcs : DbContext
    {
        public AppDbContextcs(DbContextOptions<AppDbContextcs> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Category> Categorys { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
    //public class AppDbContextcs(DbContextOptions<AppDbContextcs> options) : DbContext(options)
    //    {
    //        public DbSet<Product> Products { get; set; } = default!;

    //       protected override void OnModelCreating(ModelBuilder modelBuilder)
    //       {
    //          modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    //          base.OnModelCreating(modelBuilder);
    //       }


    //    }



}
