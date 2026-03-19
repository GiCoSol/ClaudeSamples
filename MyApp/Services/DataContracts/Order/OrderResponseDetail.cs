using MyApp.Models.Enums;

namespace MyApp.Services.DataContracts.Order;

public record OrderItemResponse(Guid Id, Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

public record OrderResponseDetail(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    OrderStatus Status,
    decimal Total,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OrderItemResponse> Items);
