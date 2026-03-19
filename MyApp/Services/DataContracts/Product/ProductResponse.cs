namespace MyApp.Services.DataContracts.Product;

public record ProductResponse(Guid Id, string Name, decimal Price, int Stock);
