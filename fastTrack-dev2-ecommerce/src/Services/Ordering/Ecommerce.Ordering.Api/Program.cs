using System.Text.Json.Serialization;
using Ecommerce.Ordering.Application;
using Ecommerce.Ordering.Infrastructure;
using Ecommerce.Ordering.Infrastructure.Data.Extensions;
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

// 3 + 4. Application (use cases) and Infrastructure (EF Core SQL Server, outbox, RabbitMQ, payments)
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);
builder.Services.AddHealthChecks().AddSqlServer(builder.Configuration.GetConnectionString("Database")!);

// 5. MVC + validation + docs
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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
    await app.InitialiseDatabaseAsync();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
    { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }).AllowAnonymous();

app.Run();
