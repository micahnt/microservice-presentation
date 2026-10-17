namespace Ecommerce.Catalog.Api.Startup;

public static class DatabaseExtensions
{
    // Dev only: apply migrations (creates CatalogDb if needed) and seed products.
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(CatalogInitialData.Products);
            await db.SaveChangesAsync();
        }
    }
}
