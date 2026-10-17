using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Shared.Auth;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string Email { get; }
    string Name { get; }
    bool IsAdmin { get; }
}

public class CurrentUser(IHttpContextAccessor http) : ICurrentUser
{
    private ClaimsPrincipal? User => http.HttpContext?.User;

    // False inside message consumers and background work (no HttpContext).
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid UserId => Guid.Parse(Required("sub"));
    public string Email => Required("email");
    public string Name => User?.FindFirstValue("name") is { Length: > 0 } name ? name : Email;
    public bool IsAdmin => User?.IsInRole(Roles.Admin) == true;

    private string Required(string claim) =>
        User?.FindFirstValue(claim) ?? throw new UnauthorizedAccessException($"Token has no '{claim}' claim.");
}
