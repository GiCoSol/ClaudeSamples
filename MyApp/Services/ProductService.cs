using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Infrastructure.DbContext;
using MyApp.Models.Entities;
using MyApp.Services.DataContracts.Product;
using MyApp.Services.Interfaces;
using MyApp.Services.Mapper;

namespace MyApp.Services;

public class ProductService(AppDbContext db, ProductMapper mapper, ILogger<ProductService> logger) : IProductService
{
    public async Task<PagedResult<ProductResponseListItem>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking().OrderBy(p => p.Name);
        var total = await query.CountAsync(ct);
        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ProductResponseListItem>(
            entities.Select(mapper.ToListItemResponse).ToList(),
            total, page, pageSize);
    }

    public async Task<ProductResponseDetail?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return product is null ? null : mapper.ToDetailResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequestCreate request, CancellationToken ct)
    {
        var product = mapper.ToEntity(request);
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Product created {ProductId}", product.Id);
        return mapper.ToResponse(product);
    }

    public async Task<bool> UpdateAsync(Guid id, ProductRequestUpdate request, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            logger.LogWarning("Product not found {ProductId}", id);
            return false;
        }

        mapper.UpdateEntity(request, product);
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Product updated {ProductId}", product.Id);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            logger.LogWarning("Product not found {ProductId}", id);
            return false;
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Product deleted {ProductId}", id);
        return true;
    }
}
