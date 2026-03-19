using FluentValidation;

namespace MyApp.Services.DataContracts.Product;

public record ProductRequestUpdate(string Name, string? Description, decimal Price, int Stock);

public class ProductRequestUpdateValidator : AbstractValidator<ProductRequestUpdate>
{
    public ProductRequestUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
