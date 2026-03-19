namespace MyApp.Services.DataContracts.Product;

public record ProductResponseListItem(Guid Id, string Name, decimal Price, int Stock);
