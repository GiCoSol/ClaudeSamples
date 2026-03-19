using FluentValidation;

namespace MyApp.Services.DataContracts.Product;

public record ProductRequestCreate(string Name, string? Description, decimal Price, int Stock);

public class ProductRequestCreateValidator : AbstractValidator<ProductRequestCreate>
{
    public ProductRequestCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
