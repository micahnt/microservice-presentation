using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Shared.Messaging.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services,
        IConfiguration configuration, Assembly? assembly = null,
        Action<IBusRegistrationConfigurator>? outbox = null)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            if (assembly != null)
                config.AddConsumers(assembly);

            outbox?.Invoke(config);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
                {
                    host.Username(configuration["MessageBroker:UserName"]);
                    host.Password(configuration["MessageBroker:Password"]);
                });
                configurator.UseMessageRetry(r => r.Intervals(500, 2000, 5000));
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    // EF Core transactional outbox: Publish() writes to the OutboxMessage table and the message is
    // sent only after SaveChanges commits. Consumers also get the inbox (de-duplicates redeliveries).
    public static void UsePostgres<TDbContext>(this IBusRegistrationConfigurator config)
        where TDbContext : DbContext =>
        config.UseOutbox<TDbContext>(o => o.UsePostgres());

    public static void UseSqlServer<TDbContext>(this IBusRegistrationConfigurator config)
        where TDbContext : DbContext =>
        config.UseOutbox<TDbContext>(o => o.UseSqlServer());

    private static void UseOutbox<TDbContext>(this IBusRegistrationConfigurator config,
        Action<IEntityFrameworkOutboxConfigurator> database)
        where TDbContext : DbContext
    {
        config.AddEntityFrameworkOutbox<TDbContext>(o =>
        {
            database(o);
            o.UseBusOutbox();
        });

        config.AddConfigureEndpointsCallback((context, _, endpoint) =>
            endpoint.UseEntityFrameworkOutbox<TDbContext>(context));
    }
}
