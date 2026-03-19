using FluentValidation;

namespace MyApp.Services.DataContracts.Order;

public record OrderItemRequestCreate(Guid ProductId, int Quantity);

public record OrderRequestCreate(string CustomerName, string CustomerEmail, List<OrderItemRequestCreate> Items);

public class OrderRequestCreateValidator : AbstractValidator<OrderRequestCreate>
{
    public OrderRequestCreateValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
