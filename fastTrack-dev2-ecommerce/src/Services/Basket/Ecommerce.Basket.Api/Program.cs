using System.Text.Json.Serialization;
using Ecommerce.Basket.Api.Startup;
using Ecommerce.Shared.Exceptions.Handler;
using Ecommerce.Shared.Messaging.MassTransit;
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

// 3. Infrastructure: Redis, Catalog (Refit), Discount (gRPC), RabbitMQ (no outbox: no relational DB)
builder.Services.AddClients(builder.Configuration);
builder.Services.AddMessageBroker(builder.Configuration);
builder.Services.AddHealthChecks().AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// 4. Application
builder.Services.AddApplicationServices();

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
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
    { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }).AllowAnonymous();

app.Run();
