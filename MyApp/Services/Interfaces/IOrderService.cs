using MyApp.Common;
using MyApp.Services.DataContracts.Order;

namespace MyApp.Services.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderResponseListItem>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<OrderResponseDetail?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<OrderResponse> CreateAsync(OrderRequestCreate request, CancellationToken ct);
    Task<bool> UpdateStatusAsync(Guid id, OrderRequestUpdateStatus request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
