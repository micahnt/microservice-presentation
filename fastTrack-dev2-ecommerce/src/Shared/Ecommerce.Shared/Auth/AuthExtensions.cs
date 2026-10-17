using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Shared.Auth;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
}

public static class Policies
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
}

public static class AuthExtensions
{
    // Binds the "Jwt" section; the app refuses to start if it is missing or the key is too short.
    public static JwtSettings AddJwtSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Jwt");
        services.AddOptions<JwtSettings>().Bind(section).ValidateDataAnnotations().ValidateOnStart();

        var settings = section.Get<JwtSettings>() ?? new JwtSettings();
        services.AddSingleton(settings);
        return settings;
    }

    // Every HTTP service validates the JWT issued by the Identity service (shared HS256 key).
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, JwtSettings settings)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false; // keep "sub", "role" as-is
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
                    NameClaimType = "name",
                    RoleClaimType = "role",
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Admin, p => p.RequireRole(Roles.Admin))
            .AddPolicy(Policies.Customer, p => p.RequireRole(Roles.Customer, Roles.Admin))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}
