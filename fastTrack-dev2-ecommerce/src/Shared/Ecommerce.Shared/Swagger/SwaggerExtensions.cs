using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Ecommerce.Shared.Swagger;

public static class SwaggerExtensions
{
    // Swagger UI with an "Authorize" button: paste the token from /identity-service/auth/login.
    public static IServiceCollection AddSwaggerWithBearer(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            };
            options.AddSecurityDefinition("Bearer", scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
        });
        return services;
    }
}
