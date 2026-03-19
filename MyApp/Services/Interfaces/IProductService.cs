using MyApp.Common;
using MyApp.Services.DataContracts.Product;

namespace MyApp.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductResponseListItem>> GetAllAsync(int page, int pageSize, CancellationToken ct);
    Task<ProductResponseDetail?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ProductResponse> CreateAsync(ProductRequestCreate request, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, ProductRequestUpdate request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
