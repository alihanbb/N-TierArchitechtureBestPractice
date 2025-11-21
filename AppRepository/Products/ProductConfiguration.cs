using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppRepository.Products
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .HasColumnName("ProductId");
            builder.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(x => x.Price)
                .HasColumnType("decimal(18,2)");
            builder.Property(x => x.Stock)
                .HasDefaultValue(0);

            // Foreign key relationship
            builder.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId);
        }
    }
}
