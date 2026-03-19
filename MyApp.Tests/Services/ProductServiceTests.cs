using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyApp.Infrastructure.DbContext;
using MyApp.Models.Entities;
using MyApp.Services;
using MyApp.Services.DataContracts.Product;
using MyApp.Services.Mapper;

namespace MyApp.Tests.Services;

public class ProductServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _service = new ProductService(_context, new ProductMapper(), NullLogger<ProductService>.Instance);
    }

    public async Task InitializeAsync()
        => await _context.Database.EnsureCreatedAsync();

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyPagedResult()
    {
        var result = await _service.GetAllAsync(1, 20, CancellationToken.None);

        result.Total.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetAllAsync_WithProducts_ReturnsPaginatedItems()
    {
        await SeedProductsAsync(3);

        var result = await _service.GetAllAsync(1, 2, CancellationToken.None);

        result.Total.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsDetailResponse()
    {
        var product = await SeedProductAsync("Laptop", 999.99m);

        var result = await _service.GetByIdAsync(product.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Laptop");
        result.Price.Should().Be(999.99m);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProduct_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesAndReturnsProduct()
    {
        var request = new ProductRequestCreate("Monitor", "4K display", 499.00m, 10);

        var result = await _service.CreateAsync(request, CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Monitor");
        result.Price.Should().Be(499.00m);

        var inDb = await _context.Products.FindAsync(result.Id);
        inDb.Should().NotBeNull();
        inDb!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesAndReturnsTrue()
    {
        var product = await SeedProductAsync("Keyboard", 79.99m);
        var request = new ProductRequestUpdate("Mechanical Keyboard", null, 129.99m, 5);

        var result = await _service.UpdateAsync(product.Id, request, CancellationToken.None);

        result.Should().BeTrue();
        var updated = await _context.Products.FindAsync(product.Id);
        updated!.Name.Should().Be("Mechanical Keyboard");
        updated.Price.Should().Be(129.99m);
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ReturnsFalse()
    {
        var request = new ProductRequestUpdate("Ghost", null, 1m, 0);

        var result = await _service.UpdateAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_DeletesAndReturnsTrue()
    {
        var product = await SeedProductAsync("Mouse", 49.99m);

        var result = await _service.DeleteAsync(product.Id, CancellationToken.None);

        result.Should().BeTrue();
        var deleted = await _context.Products.FindAsync(product.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProduct_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeFalse();
    }

    private async Task<Product> SeedProductAsync(string name, decimal price, int stock = 10)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Stock = stock,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    private async Task SeedProductsAsync(int count)
    {
        for (var i = 1; i <= count; i++)
            await SeedProductAsync($"Product {i}", i * 10m);
    }
}
