using MyApp.Models.Enums;

namespace MyApp.Services.DataContracts.Order;

public record OrderResponseListItem(Guid Id, string CustomerName, decimal Total, OrderStatus Status, DateTime CreatedAt);
