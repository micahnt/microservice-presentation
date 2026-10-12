using System.Collections.Concurrent;
using Basket.API.Models;
using Discount.Grpc;

var builder = WebApplication.CreateBuilder(args);

// gRPC client for Discount.Grpc (generated from discount.proto)
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
});

// In-memory basket store: replaces Marten/PostgreSQL + Redis so the demo needs no Docker
builder.Services.AddSingleton<ConcurrentDictionary<string, ShoppingCart>>();

var app = builder.Build();

// Store a basket: call Discount over gRPC for each item, then save the discounted cart
app.MapPost("/basket", async (
    ShoppingCart cart,
    DiscountProtoService.DiscountProtoServiceClient discountProto,
    ConcurrentDictionary<string, ShoppingCart> store,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    foreach (var item in cart.Items)
    {
        var coupon = await discountProto.GetDiscountAsync(
            new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: ct);

        logger.LogInformation("gRPC GetDiscount({ProductName}) returned amount {Amount}", item.ProductName, coupon.Amount);
        item.Price -= coupon.Amount;
    }

    store[cart.UserName] = cart;
    return Results.Created($"/basket/{cart.UserName}", cart);
});

app.MapGet("/basket/{userName}", (string userName, ConcurrentDictionary<string, ShoppingCart> store) =>
    store.TryGetValue(userName, out var cart) ? Results.Ok(cart) : Results.NotFound());

app.MapDelete("/basket/{userName}", (string userName, ConcurrentDictionary<string, ShoppingCart> store) =>
    store.TryRemove(userName, out _) ? Results.NoContent() : Results.NotFound());

// Admin: create a coupon through the gRPC CreateDiscount method
app.MapPost("/discount", async (CouponModel coupon, DiscountProtoService.DiscountProtoServiceClient discountProto) =>
    Results.Ok(await discountProto.CreateDiscountAsync(new CreateDiscountRequest { Coupon = coupon })));

app.Run();
