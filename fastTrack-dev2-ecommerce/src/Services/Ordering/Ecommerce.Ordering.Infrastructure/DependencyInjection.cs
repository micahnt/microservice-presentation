using Ecommerce.Ordering.Application.Consumers;
using Ecommerce.Ordering.Application.Data;
using Ecommerce.Ordering.Application.Payments;
using Ecommerce.Ordering.Infrastructure.Payments;
using Ecommerce.Shared.Messaging.MassTransit;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices
        (this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IPaymentGateway, FakePaymentGateway>();

        // Consumers live in the Application assembly; the outbox uses this DbContext (SQL Server).
        services.AddMessageBroker(configuration, typeof(BasketCheckoutEventConsumer).Assembly,
            outbox => outbox.UseSqlServer<ApplicationDbContext>());

        return services;
    }
}
