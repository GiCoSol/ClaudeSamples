using Microsoft.EntityFrameworkCore;
using MyApp.Common;
using MyApp.Infrastructure.DbContext;
using MyApp.Models.Entities;
using MyApp.Models.Enums;
using MyApp.Services.DataContracts.Order;
using MyApp.Services.Interfaces;
using MyApp.Services.Mapper;

namespace MyApp.Services;

public class OrderService(AppDbContext db, OrderMapper mapper, ILogger<OrderService> logger) : IOrderService
{
    public async Task<PagedResult<OrderResponseListItem>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = db.Orders.AsNoTracking().OrderByDescending(o => o.CreatedAt);
        var total = await query.CountAsync(ct);
        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<OrderResponseListItem>(
            entities.Select(mapper.ToListItemResponse).ToList(),
            total, page, pageSize);
    }

    public async Task<OrderResponseDetail?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return order is null ? null : mapper.ToDetailResponse(order);
    }

    public async Task<OrderResponse> CreateAsync(OrderRequestCreate request, CancellationToken ct)
    {
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        var items = new List<OrderItem>();
        foreach (var itemRequest in request.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == itemRequest.ProductId)
                ?? throw new NotFoundException($"Product {itemRequest.ProductId} not found");

            if (product.Stock < itemRequest.Quantity)
                throw new ConflictException($"Insufficient stock for product '{product.Name}'");

            product.Stock -= itemRequest.Quantity;
            items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = itemRequest.Quantity
            });
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            Status = OrderStatus.Pending,
            Total = items.Sum(i => i.UnitPrice * i.Quantity),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = items
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Order created {OrderId}", order.Id);
        return mapper.ToResponse(order);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, OrderRequestUpdateStatus request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            logger.LogWarning("Order not found {OrderId}", id);
            return false;
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Order status updated {OrderId} -> {Status}", order.Id, order.Status);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            logger.LogWarning("Order not found {OrderId}", id);
            return false;
        }

        db.Orders.Remove(order);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Order deleted {OrderId}", id);
        return true;
    }
}
