using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Ecommerce.Identity.Api.Data;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}

public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options);
