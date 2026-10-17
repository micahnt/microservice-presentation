namespace Ecommerce.Identity.Api.Startup;

public static class DatabaseExtensions
{
    // Dev only: apply migrations (creates IdentityDb if needed) and seed roles + demo users.
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
        await db.Database.MigrateAsync();
        await IdentitySeed.SeedAsync(scope.ServiceProvider);
    }
}
