using Ecommerce.Discount.Grpc;
using Refit;
using StackExchange.Redis;

namespace Ecommerce.Basket.Api.Startup;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
        .AddScoped<IBasketRepository, BasketRepository>()
        .AddScoped<ICatalogService, CatalogService>()
        .AddScoped<IBasketReader, BasketReader>()
        .AddScoped<IBasketStorer, BasketStorer>()
        .AddScoped<IBasketDeleter, BasketDeleter>()
        .AddScoped<IBasketCheckoutProcessor, BasketCheckoutProcessor>();

    public static IServiceCollection AddClients(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis: AbortOnConnectFail=false so the service starts even if Redis is still booting.
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis")!);
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });

        // Catalog over HTTP (Refit) for the current price
        services.AddRefitClient<ICatalogClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["CatalogSettings:Url"]!))
            .AddStandardResilienceHandler();

        // Discount over gRPC, plain HTTP/2 (h2c): no certificate handling needed
        services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o =>
            o.Address = new Uri(configuration["GrpcSettings:DiscountUrl"]!));

        return services;
    }
}
