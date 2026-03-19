using MyApp.Services.Interfaces;
using MyApp.Services.Mapper;

namespace MyApp.Services.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductServices(this IServiceCollection services)
    {
        services.AddSingleton<ProductMapper>();
        services.AddScoped<IProductService, ProductService>();
        return services;
    }

    public static IServiceCollection AddOrderServices(this IServiceCollection services)
    {
        services.AddSingleton<OrderMapper>();
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}
