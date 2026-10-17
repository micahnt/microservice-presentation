using Ecommerce.Ordering.Application.Orders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Ecommerce.Ordering.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddFeatureManagement();

        services.AddScoped<IOrderCreator, OrderCreator>();
        services.AddScoped<IOrderReader, OrderReader>();
        services.AddScoped<IOrderUpdater, OrderUpdater>();
        services.AddScoped<IOrderDeleter, OrderDeleter>();

        return services;
    }
}
