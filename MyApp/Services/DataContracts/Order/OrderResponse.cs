using MyApp.Models.Enums;

namespace MyApp.Services.DataContracts.Order;

public record OrderResponse(Guid Id, string CustomerName, decimal Total, OrderStatus Status);
