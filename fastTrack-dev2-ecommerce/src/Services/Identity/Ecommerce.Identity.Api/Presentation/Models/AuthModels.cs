namespace Ecommerce.Identity.Api.Presentation.Models;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token);

public record MeResponse(Guid Id, string Email, string FirstName, string LastName, IList<string> Roles);
