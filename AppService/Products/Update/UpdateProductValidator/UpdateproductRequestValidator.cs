using FluentValidation;

namespace AppService.Products.Update.UpdateProductValidator
{
    public class UpdateproductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateproductRequestValidator()
        {
            RuleFor(x => x.ProductName)
               .NotEmpty().WithMessage("Ürün ismi gereklidir")
               .Length(3, 10).WithMessage("Ürün ismi 3-10 arasında karakterden oluşmalıdır");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Fiyat gereklidir")
                .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır");

            RuleFor(x => x.Stock)
                .GreaterThan(0).WithMessage("Stok 0'dan büyük olmalıdır")
                .InclusiveBetween(1, 100).WithMessage("Stok adedi 1-100 arasında olmalıdır");

        }
    }
}
