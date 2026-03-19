using FluentValidation;
using MyApp.Models.Enums;

namespace MyApp.Services.DataContracts.Order;

public record OrderRequestUpdateStatus(OrderStatus Status);

public class OrderRequestUpdateStatusValidator : AbstractValidator<OrderRequestUpdateStatus>
{
    public OrderRequestUpdateStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
