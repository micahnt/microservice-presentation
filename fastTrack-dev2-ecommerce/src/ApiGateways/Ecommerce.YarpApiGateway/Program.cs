using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 50;
    });
});

var app = builder.Build();

app.UseRateLimiter();

// wwwroot = the React build (copied in by the Dockerfile): one origin for UI and API, so no CORS.
app.UseDefaultFiles();
app.UseStaticFiles();

// /{service}-service/* routes are matched before the fallback, so they never return index.html.
app.MapReverseProxy();
app.MapFallbackToFile("index.html"); // client-side routes like /products/123

app.Run();
