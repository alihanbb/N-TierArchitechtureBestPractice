using AppRepository.Products;
using FluentValidation;

namespace AppService.Products.Create
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Ürün ismi gereklidir")
                .Length(3, 10).WithMessage("Ürün ismi 3-10 arasında karakterden oluşmalıdır");
            //.MustAsync(MustUniqueProductNameAsync).WithMessage("Ürün ismi veritabanında bulunmaktadır");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Fiyat gereklidir")
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");

            RuleFor(x => x.Stock)
                .GreaterThan(0).WithMessage("Stok 0'dan büyük olmalıdır")
                .InclusiveBetween(1, 100).WithMessage("Stok adedi 1-100 arasında olmalıdır");

        }
        //private async Task<bool> MustUniqueProductNameAsync(string productName, CancellationToken arg2)
        //{

        //    return !await _productRepository.Where(x => x.ProductName == productName).AnyAsync();

        //}
    }
}
