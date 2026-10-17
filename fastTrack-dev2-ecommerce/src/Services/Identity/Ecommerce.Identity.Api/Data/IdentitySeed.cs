namespace Ecommerce.Identity.Api.Data;

// Local demo accounts. Dev only (called from MigrateAndSeedAsync).
public static class IdentitySeed
{
    private static readonly (string Email, string Password, string FirstName, string LastName, string Role)[] Users =
    [
        ("admin@shop.local", "Admin123", "Shop", "Admin", Roles.Admin),
        ("customer@shop.local", "Customer123", "Demo", "Customer", Roles.Customer),
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        foreach (var role in new[] { Roles.Admin, Roles.Customer })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole<Guid>(role));
        }

        var users = services.GetRequiredService<UserManager<AppUser>>();
        foreach (var u in Users)
        {
            if (await users.FindByEmailAsync(u.Email) is not null)
                continue;

            var user = new AppUser
            {
                Id = Guid.NewGuid(), UserName = u.Email, Email = u.Email,
                FirstName = u.FirstName, LastName = u.LastName,
            };
            await users.CreateAsync(user, u.Password);
            await users.AddToRoleAsync(user, u.Role);
        }
    }
}
