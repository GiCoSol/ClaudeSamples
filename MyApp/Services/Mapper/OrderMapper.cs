using MyApp.Models.Entities;
using MyApp.Services.DataContracts.Order;
using Riok.Mapperly.Abstractions;

namespace MyApp.Services.Mapper;

[Mapper]
public partial class OrderMapper
{
    [MapperIgnoreSource(nameof(Order.Items))]
    [MapperIgnoreSource(nameof(Order.CustomerEmail))]
    [MapperIgnoreSource(nameof(Order.CreatedAt))]
    [MapperIgnoreSource(nameof(Order.UpdatedAt))]
    public partial OrderResponse ToResponse(Order entity);

    public partial OrderResponseDetail ToDetailResponse(Order entity);

    [MapperIgnoreSource(nameof(Order.Items))]
    [MapperIgnoreSource(nameof(Order.CustomerEmail))]
    [MapperIgnoreSource(nameof(Order.UpdatedAt))]
    public partial OrderResponseListItem ToListItemResponse(Order entity);

    [MapperIgnoreSource(nameof(OrderItem.OrderId))]
    [MapperIgnoreSource(nameof(OrderItem.Order))]
    public partial OrderItemResponse ToItemResponse(OrderItem item);
}
