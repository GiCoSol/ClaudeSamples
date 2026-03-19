namespace MyApp.Services.DataContracts.Product;

public record ProductResponseDetail(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    DateTime UpdatedAt);
