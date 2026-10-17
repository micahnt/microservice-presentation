namespace Ecommerce.Identity.Api.Presentation.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(UserManager<AppUser> users, ITokenIssuer tokens, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost("register"), AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(), UserName = request.Email, Email = request.Email,
            FirstName = request.FirstName, LastName = request.LastName,
        };

        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
                throw new ConflictException("This email is already registered.");
            throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await users.AddToRoleAsync(user, Roles.Customer);
        return Ok(new AuthResponse(tokens.Issue(user, [Roles.Customer])));
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email);
        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return Ok(new AuthResponse(tokens.Issue(user, await users.GetRolesAsync(user))));
    }

    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var user = await users.FindByIdAsync(currentUser.UserId.ToString())
            ?? throw new NotFoundException("User", currentUser.UserId);

        return Ok(new MeResponse(user.Id, user.Email!, user.FirstName, user.LastName, await users.GetRolesAsync(user)));
    }
}
