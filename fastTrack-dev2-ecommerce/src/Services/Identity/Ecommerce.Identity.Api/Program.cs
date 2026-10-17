using Ecommerce.Identity.Api.Startup;
using Ecommerce.Shared.Exceptions.Handler;
using Ecommerce.Shared.Swagger;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Settings
var jwt = builder.Services.AddJwtSettings(builder.Configuration);

// 2. Cross-cutting
builder.Logging.ClearProviders().AddJsonConsole();
builder.Services.AddHttpLogging(_ => { });
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddJwtAuth(jwt);

// 3. Infrastructure
var connectionString = builder.Configuration.GetConnectionString("Database")!;
builder.Services.AddDbContext<AppIdentityDbContext>(o => o.UseNpgsql(connectionString));
builder.Services.AddIdentityCore<AppUser>(o =>
    {
        o.User.RequireUniqueEmail = true;
        o.Password.RequireNonAlphanumeric = false; // dev-friendly
        o.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppIdentityDbContext>();
builder.Services.AddHealthChecks().AddNpgSql(connectionString);

// 4. Application
builder.Services.AddScoped<ITokenIssuer, TokenIssuer>();

// 5. MVC + validation + docs
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSwaggerWithBearer();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.MigrateAndSeedAsync();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
    { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }).AllowAnonymous();

app.Run();
