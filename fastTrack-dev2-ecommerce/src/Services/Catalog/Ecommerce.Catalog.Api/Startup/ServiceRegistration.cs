namespace Ecommerce.Catalog.Api.Startup;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
        .AddScoped<IProductRepository, ProductRepository>()
        .AddScoped<IProductReader, ProductReader>()
        .AddScoped<IProductCreator, ProductCreator>()
        .AddScoped<IProductUpdater, ProductUpdater>()
        .AddScoped<IProductDeleter, ProductDeleter>();
}
