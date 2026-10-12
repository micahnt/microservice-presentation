---
marp: true
theme: default
paginate: true
size: 16:9
header: 'Microservices with .NET 8 — EShop Microservices'
footer: 'Overview · Building a service · Service invocation · Demo'
style: |
  section { font-size: 24px; }
  section.lead { text-align: center; justify-content: center; }
  section.lead h1 { font-size: 56px; }
  section.divider { background: #1f3b57; color: #fff; justify-content: center; }
  section.divider h1, section.divider h2 { color: #fff; }
  pre { font-size: 15px; line-height: 1.35; }
  table { font-size: 19px; }
  .cols { display: grid; grid-template-columns: 1fr 1fr; gap: 28px; }
  .small { font-size: 18px; }
---

<!-- _class: lead -->
<!-- _header: '' -->
<!-- _footer: '' -->

# Microservices with .NET 8

### Overview · How to build a service · Service invocation · Demo

Reference project: **EShopMicroservices** (Catalog, Basket, Discount, Ordering, YARP gateway, Shopping.Web)

---

## Agenda

1. **Overview**: what microservices are, benefits, challenges, and when to use them
2. **How to create a microservice**: from domain analysis to a running container
3. **Service invocation methods**
   - Synchronous: REST/HTTP, gRPC
   - Asynchronous: messaging with RabbitMQ and MassTransit
   - API Gateway: YARP reverse proxy
4. **Demo**: the EShop checkout flow, end to end
5. Summary and references

---

<!-- _class: divider -->
<!-- _header: '' -->

# Part 1
## Microservices Overview

---

## Architecture design journey

```
 Monolith ──► Layered Monolith ──► Modular Monolith ──► SOA ──► Microservices ──► Serverless
  (one unit)   (UI / BL / DAL)      (modules in 1 app)   (ESB)   (small, independent)  (functions)
```

- There is no single "right" architecture. **It depends on your project.**
- Granularity is a spectrum: *macroservices → microservices → nanoservices*.
- Mixed architectures are common, and you can move between styles as the system grows.
- Martin Fowler and Sam Newman both recommend **monolith first**, then extracting services one by one.

---

## What are microservices?

- **Small, independent, and loosely coupled** services that work together.
- Each service has its **own codebase**, owned by a **small team**.
- Services communicate through **well-defined APIs** (HTTP/REST, gRPC, messaging).
- Each service is **deployed independently and autonomously**.
- **Technology agnostic**: each service can use a different stack.
- Each service has **its own database**, which is not shared with other services.

> "An approach to developing a single application as a suite of small services, each running in its own process and communicating with lightweight mechanisms."
> — Martin Fowler

---

## Microservices characteristics (Martin Fowler)

<div class="cols">
<div>

- Componentization via services
- Organized around **business capabilities**
- Products, not projects
- Smart endpoints and dumb pipes

</div>
<div>

- Decentralized governance
- **Decentralized data management**
- Infrastructure automation (CI/CD)
- **Design for failure**

</div>
</div>

Each service follows the **Single Responsibility Principle** and is scoped to a **bounded context**.

---

## Benefits and challenges

<div class="cols">
<div>

### ✅ Benefits
- Agility and faster time-to-market
- **Independent scaling** of each service
- Small, focused teams
- Small, separate codebases
- Easy, independent deployment (CI/CD)
- Right tool for each job (polyglot)
- Resilience and fault isolation
- Data isolation

</div>
<div>

### ⚠️ Challenges
- Overall system **complexity**
- Network problems and **latency**; chatty calls
- Harder end-to-end development and testing
- **Data consistency** (eventual consistency)
- Deployment needs DevOps investment
- Centralized logging and monitoring required
- Debugging across many services is hard

</div>
</div>

---

## When to use microservices, and when not to

<div class="cols">
<div>

### Use them when you need
- Independent deployment with **zero downtime**
- **Independent scaling** of parts of the app
- Different data stores per use case
- Autonomous teams that own services
- A *really good reason* (not hype)

</div>
<div>

### Avoid them when
- You would build a **distributed monolith** (the worst case)
- You have no DevOps, CI/CD, containers, or monitoring
- The team is **small**
- The product is a **brand-new startup** with an unclear domain
- Services would use a **shared database** (anti-pattern)

</div>
</div>

---

## Monolith vs. microservices

| Aspect | Monolith | Microservices |
|---|---|---|
| Structure | One undivided unit | Many heterogeneous services and DBs |
| Scalability | Scale the whole app | Scale each service independently |
| Deployment | Simple, all at once | Independent, zero-downtime, CI/CD |
| Data | One shared database | Database per service (polyglot) |
| Failure | One bug can take down everything | Faults are isolated |
| Team fit | Small or new teams | Experienced teams with container/DevOps skills |

---

## Database-per-service and polyglot persistence

- A service's data is **only accessible through its API**. No direct DB access from other services.
- Each service can **choose the best storage** for its needs, change its schema freely, and scale on its own.

| Service | Data store in EShop | Why |
|---|---|---|
| **Catalog.API** | PostgreSQL + Marten (document DB) | Flexible product documents |
| **Basket.API** | PostgreSQL (Marten) + **Redis** cache | Fast key-value shopping carts |
| **Discount.Grpc** | SQLite (EF Core) | Small, simple coupon table |
| **Ordering.API** | **SQL Server** (EF Core) | Rich relational order model |

---

## Decomposing the domain into services

**E-commerce requirements** → user stories → **nouns** (entities) and **verbs** (use cases) → **bounded contexts**

| User story | Noun / verb | Bounded context → service |
|---|---|---|
| List and filter products by category | Product, Category | **Catalog** |
| Put products into the shopping cart | ShoppingCart, Item | **Basket** |
| Apply coupons and see the total cost | Coupon, Discount | **Discount** |
| Check out the cart and create an order | Order, Address, Payment | **Ordering** |

- Use DDD **Context Mapping** together with domain experts to find the boundaries.
- A bounded context is *usually* one microservice, but it can be split further for scaling needs.

---

## The big picture: EShop microservices

```
                      ┌──────────────────────┐
                      │  Shopping.Web (Razor)│  Refit HTTP clients
                      └──────────┬───────────┘
                                 │ HTTP
                      ┌──────────▼───────────┐
                      │ YarpApiGateway (YARP)│  routing + rate limiting
                      └──┬────────┬────────┬─┘
                  HTTP   │        │        │   HTTP
          ┌──────────────▼┐  ┌────▼──────┐ ┌▼──────────────┐
          │  Catalog.API  │  │ Basket.API│ │ Ordering.API  │
          │ Vertical Slice│  │ VS + CQRS │ │ Clean Arch+DDD│
          └──────┬────────┘  └─┬───┬───┬─┘ └──┬─────────▲──┘
                 │     gRPC ◄──┘   │   │      │         │ consume
          PostgreSQL (Marten)      │   │      SQL Server│
                 ┌─────────────────▼┐  │ publish ┌──────┴──────┐
                 │  Discount.Grpc   │  └────────►│  RabbitMQ   │
                 │  SQLite / EF Core│  Redis +   │ (MassTransit)│
                 └──────────────────┘  PostgreSQL└─────────────┘
```

---

<!-- _class: divider -->
<!-- _header: '' -->

# Part 2
## How to Create a Microservice

---

## The process for each microservice

<div class="cols">
<div>

### 1. Domain analysis
1. Domain models (e.g. `Product`)
2. Application use cases (CRUD, filtering)
3. API endpoints (`GET /products/{id}`)
4. Underlying data structures (DB choice)

</div>
<div>

### 2. Technical analysis
1. Architecture style (Vertical Slice / Clean)
2. Patterns and principles (CQRS, Mediator, DI)
3. Libraries (Carter, MediatR, Marten…)
4. Project folder structure

</div>
</div>

### 3. Develop → 4. Containerize (Dockerfile) → 5. Orchestrate (docker-compose)

---

## Why ASP.NET Core 8 for microservices

- Cross-platform, open-source, and **high-performance**
- **Minimal APIs**: REST endpoints with almost no boilerplate
- **Built-in dependency injection**, configuration, logging, and health checks
- First-class **gRPC** support
- Cloud-ready configuration, small container images, and Native AOT

### `Program.cs`: the five steps
```csharp
var builder = WebApplication.CreateBuilder(args);   // 1. Create the builder
builder.Services.AddXxx(...);                         // 2. Register services (DI)
var app = builder.Build();                            // 3. Build the app
app.MapGet(...); app.UseXxx();                        // 4. Configure the HTTP pipeline
app.Run();                                            // 5. Run
```

---

## Step 1: the simplest microservice (TodoApi, section 4)

```bash
dotnet new web -n TodoApi
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
var app = builder.Build();

app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) => await db.Todos.FindAsync(id));

app.MapPost("/todoitems", async (TodoItem todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.Run();
```

One process, one responsibility, its own data store, and an HTTP API.

---

## Step 2: a real service — Catalog.API (Vertical Slice + CQRS)

<div class="cols">
<div>

```
Catalog.API/
├── Models/
│   └── Product.cs
├── Products/            ← one folder per feature
│   ├── CreateProduct/
│   │   ├── CreateProductEndpoint.cs
│   │   └── CreateProductHandler.cs
│   ├── GetProductById/
│   ├── GetProducts/
│   ├── GetProductByCategory/
│   ├── UpdateProduct/
│   └── DeleteProduct/
├── Data/CatalogInitialData.cs
├── Dockerfile
└── Program.cs
```

</div>
<div class="small">

**Libraries**
- **Carter**: organizes Minimal API endpoints
- **MediatR**: CQRS commands and queries
- **Marten**: PostgreSQL as a document DB
- **FluentValidation**: validation pipeline behavior
- **Mapster**: object mapping

**Shared `BuildingBlocks` library**
- `ICommand` / `IQuery` abstractions
- Validation and logging behaviors
- Global exception handler

</div>
</div>

---

## Endpoint → MediatR → handler

```csharp
// GetProductByIdEndpoint.cs — the HTTP layer (Carter)
public class GetProductByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(id));
            return Results.Ok(result.Adapt<GetProductByIdResponse>());
        })
        .WithName("GetProductById");
    }
}

// GetProductByIdHandler.cs — the query side of CQRS
public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;

internal class GetProductByIdQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await session.LoadAsync<Product>(query.Id, ct)
                      ?? throw new ProductNotFoundException(query.Id);
        return new GetProductByIdResult(product);
    }
}
```

---

## Wiring it together: Catalog `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));   // cross-cutting concerns
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddCarter();

builder.Services.AddMarten(opts =>
    opts.Connection(builder.Configuration.GetConnectionString("Database")!))
    .UseLightweightSessions();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);

var app = builder.Build();
app.MapCarter();
app.UseExceptionHandler(options => { });
app.UseHealthChecks("/health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.Run();
```

---

## Step 3: containerize with Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base        # small runtime image
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build           # SDK image for building
WORKDIR /src
COPY ["Services/Catalog/Catalog.API/Catalog.API.csproj", "Services/Catalog/Catalog.API/"]
COPY ["BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/BuildingBlocks/"]
RUN dotnet restore "./Services/Catalog/Catalog.API/Catalog.API.csproj"
COPY . .
WORKDIR "/src/Services/Catalog/Catalog.API"
RUN dotnet build "./Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
```

**Dockerfile → image → container.** Images are stored in a registry such as Docker Hub or ACR.

---

## Step 4: orchestrate with docker-compose

<div class="cols">
<div>

```yaml
# docker-compose.yml
services:
  catalogdb:
    image: postgres
  catalog.api:
    build:
      context: .
      dockerfile: Services/Catalog/Catalog.API/Dockerfile

# docker-compose.override.yml
  catalog.api:
    environment:
      - ASPNETCORE_HTTP_PORTS=8080
      - ConnectionStrings__Database=Server=catalogdb;...
    depends_on: [ catalogdb ]
    ports:
      - "6000:8080"
      - "6060:8081"
```

</div>
<div class="small">

**Port convention (http / https)**

| Service | Local | Docker |
|---|---|---|
| Catalog | 5000 / 5050 | 6000 / 6060 |
| Basket | 5001 / 5051 | 6001 / 6061 |
| Discount | 5002 / 5052 | 6002 / 6062 |
| Ordering | 5003 / 5053 | 6003 / 6063 |
| YARP gateway | 5004 / 5054 | 6004 / 6064 |
| Shopping.Web | 5005 / 5055 | 6005 / 6065 |

Inside each container, the service listens on 8080 / 8081.
Services find each other by **service name**, e.g. `http://catalog.api:8080`.

</div>
</div>

---

<!-- _class: divider -->
<!-- _header: '' -->

# Part 3
## Service Invocation Methods

---

## Communication types: sync vs. async

| | **Synchronous** | **Asynchronous** |
|---|---|---|
| How it works | Client sends a request and **waits** for the response | Client sends a message and **does not wait** |
| Protocols | HTTP/REST, **gRPC**, GraphQL, WebSocket | **AMQP** via a broker (RabbitMQ, Kafka) |
| Patterns | Request / response | One-to-one (queue), one-to-many (**pub/sub** topic) |
| Coupling | Temporal coupling: both services must be up | Loose: the broker stores and retries |
| EShop example | Basket → Discount (**gRPC**)<br>Web → Gateway → APIs (**REST**) | Basket → Ordering (**BasketCheckout event**) |

**Rule of thumb:** use REST for public APIs, gRPC for internal service-to-service calls, and messaging for workflows that span services.

---

## Method 1: REST over HTTP (synchronous)

- Resource-oriented endpoints: `GET /products`, `POST /basket`, `GET /orders`
- JSON payloads, understood by every client (browser, mobile, Postman)
- **Shopping.Web** consumes the APIs with **Refit**, a type-safe REST client generated from an interface:

```csharp
public interface ICatalogService
{
    [Get("/catalog-service/products?pageNumber={pageNumber}&pageSize={pageSize}")]
    Task<GetProductsResponse> GetProducts(int? pageNumber = 1, int? pageSize = 10);

    [Get("/catalog-service/products/{id}")]
    Task<GetProductByIdResponse> GetProduct(Guid id);
}

// Program.cs — Refit plugs into HttpClientFactory
builder.Services.AddRefitClient<ICatalogService>()
    .ConfigureHttpClient(c =>
        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!));
```

---

## Method 2: gRPC (synchronous, high performance)

<div class="cols">
<div>

- Open-source RPC framework created by Google
- Uses **HTTP/2** and binary **Protocol Buffers**
- **Contract-first**: a `.proto` file generates the server base class and the client stub
- The client calls a remote method **as if it were a local object**
- Ideal for **internal** service-to-service calls

</div>
<div>

```protobuf
// Discount.Grpc/Protos/discount.proto
syntax = "proto3";
option csharp_namespace = "Discount.Grpc";

service DiscountProtoService {
  rpc GetDiscount (GetDiscountRequest)
      returns (CouponModel);
  rpc CreateDiscount (CreateDiscountRequest)
      returns (CouponModel);
  rpc UpdateDiscount (UpdateDiscountRequest)
      returns (CouponModel);
  rpc DeleteDiscount (DeleteDiscountRequest)
      returns (DeleteDiscountResponse);
}

message GetDiscountRequest { string productName = 1; }

message CouponModel {
  int32 id = 1;
  string productName = 2;
  string description = 3;
  int32 amount = 4;
}
```

</div>
</div>

---

## gRPC server: Discount.Grpc

```csharp
// Program.cs
builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));
var app = builder.Build();
app.UseMigration();
app.MapGrpcService<DiscountService>();
```

```csharp
// Services/DiscountService.cs — inherits the base class generated from the .proto file
public class DiscountService(DiscountContext dbContext, ILogger<DiscountService> logger)
    : DiscountProtoService.DiscountProtoServiceBase
{
    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await dbContext.Coupons
            .FirstOrDefaultAsync(x => x.ProductName == request.ProductName)
            ?? new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc" };

        return coupon.Adapt<CouponModel>();
    }
    // CreateDiscount, UpdateDiscount, DeleteDiscount ...
}
```

---

## gRPC client: Basket.API calls Discount

```xml
<!-- Basket.API.csproj — generate only the client stub from the shared .proto file -->
<Protobuf Include="..\..\Discount\Discount.Grpc\Protos\discount.proto" GrpcServices="Client" />
```

```csharp
// Program.cs — register a typed gRPC client
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!));
```

```csharp
// StoreBasketHandler.cs — call it like a local method
public class StoreBasketCommandHandler(IBasketRepository repository,
        DiscountProtoService.DiscountProtoServiceClient discountProto)
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    private async Task DeductDiscount(ShoppingCart cart, CancellationToken ct)
    {
        foreach (var item in cart.Items)
        {
            var coupon = await discountProto.GetDiscountAsync(
                new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: ct);
            item.Price -= coupon.Amount;
        }
    }
}
```

---

## Method 3: asynchronous messaging (pub/sub)

<div class="cols">
<div>

**Why async?** A chain of synchronous calls (A → B → C → D) couples services tightly and adds latency.

- The producer **publishes** an event to a **message broker**
- The broker **fans out** the event to every subscriber
- The consumer does not need to be up at publish time
- Adding new subscribers requires **no change** to the producer

**Watch out for:** the broker as a single point of failure, harder debugging, at-least-once delivery (make consumers **idempotent**), and no guaranteed ordering.

</div>
<div>

**RabbitMQ** is an AMQP message broker.

Components: Producer → **Exchange** → (Binding) → **Queue** → Consumer

| Exchange | Routing |
|---|---|
| Direct | By exact routing key → one queue |
| **Fanout** | Broadcast to all bound queues |
| Topic | By routing-key pattern |
| Headers | By message header attributes |

**MassTransit** is a .NET abstraction over RabbitMQ that handles exchanges, queues, retries, and serialization for you.

</div>
</div>

---

## Messaging setup: the shared BuildingBlocks.Messaging library

```csharp
// The integration event contract, shared by the publisher and the consumer
public record IntegrationEvent
{
    public Guid Id => Guid.NewGuid();
    public DateTime OccurredOn => DateTime.Now;
    public string EventType => GetType().AssemblyQualifiedName;
}

public record BasketCheckoutEvent : IntegrationEvent
{
    public string UserName { get; set; } = default!;
    public decimal TotalPrice { get; set; } = default!;
    // address + payment fields ...
}
```

```csharp
// One extension method that registers MassTransit with RabbitMQ in any service
public static IServiceCollection AddMessageBroker(this IServiceCollection services,
    IConfiguration configuration, Assembly? assembly = null)
{
    services.AddMassTransit(config =>
    {
        config.SetKebabCaseEndpointNameFormatter();
        if (assembly != null) config.AddConsumers(assembly);   // consumers only

        config.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
            {
                host.Username(configuration["MessageBroker:UserName"]);
                host.Password(configuration["MessageBroker:Password"]);
            });
            cfg.ConfigureEndpoints(context);
        });
    });
    return services;
}
```

---

## Publish (Basket) → consume (Ordering)

<div class="cols">
<div>

**Publisher: Basket.API**
```csharp
// Program.cs
builder.Services.AddMessageBroker(builder.Configuration);

// CheckoutBasketHandler.cs
public class CheckoutBasketCommandHandler(
    IBasketRepository repository,
    IPublishEndpoint publishEndpoint) : ...
{
  public async Task<CheckoutBasketResult> Handle(...)
  {
    var basket = await repository.GetBasket(userName, ct);
    if (basket == null) return new(false);

    var evt = dto.Adapt<BasketCheckoutEvent>();
    evt.TotalPrice = basket.TotalPrice;

    await publishEndpoint.Publish(evt, ct);
    await repository.DeleteBasket(userName, ct);
    return new(true);
  }
}
```

</div>
<div>

**Consumer: Ordering.Application**
```csharp
// DependencyInjection.cs — also scans for consumers
services.AddMessageBroker(configuration,
    Assembly.GetExecutingAssembly());

// BasketCheckoutEventHandler.cs
public class BasketCheckoutEventHandler(
    ISender sender,
    ILogger<BasketCheckoutEventHandler> logger)
    : IConsumer<BasketCheckoutEvent>
{
  public async Task Consume(
      ConsumeContext<BasketCheckoutEvent> context)
  {
    logger.LogInformation("Integration Event handled: {e}",
        context.Message.GetType().Name);

    var command = MapToCreateOrderCommand(context.Message);
    await sender.Send(command);   // creates the order
  }
}
```

</div>
</div>

---

## Method 4: API Gateway with YARP

<div class="cols">
<div>

- **Gateway Routing**: one public entry point that routes to internal services
- **API Gateway pattern**: routing, aggregation, and cross-cutting concerns (auth, rate limiting, caching, SSL)
- **BFF (Backend for Frontend)**: one gateway per client type to avoid a single bottleneck
- **YARP**: Microsoft's lightweight, high-performance .NET reverse proxy

```csharp
builder.Services.AddReverseProxy()
  .LoadFromConfig(builder.Configuration
                  .GetSection("ReverseProxy"));
builder.Services.AddRateLimiter(o =>
  o.AddFixedWindowLimiter("fixed", opt => {
      opt.Window = TimeSpan.FromSeconds(10);
      opt.PermitLimit = 5; }));

app.UseRateLimiter();
app.MapReverseProxy();
```

</div>
<div>

```json
"ReverseProxy": {
  "Routes": {
    "catalog-route": {
      "ClusterId": "catalog-cluster",
      "Match": { "Path": "/catalog-service/{**catch-all}" },
      "Transforms": [ { "PathPattern": "{**catch-all}" } ]
    },
    "ordering-route": {
      "ClusterId": "ordering-cluster",
      "RateLimiterPolicy": "fixed",
      "Match": { "Path": "/ordering-service/{**catch-all}" },
      "Transforms": [ { "PathPattern": "{**catch-all}" } ]
    }
  },
  "Clusters": {
    "catalog-cluster": {
      "Destinations": { "destination1":
        { "Address": "http://catalog.api:8080" } }
    },
    "ordering-cluster": { "...": "http://ordering.api:8080" }
  }
}
```

`GET /catalog-service/products` → `http://catalog.api:8080/products`

</div>
</div>

---

## Choosing an invocation method

| Method | Style | Best for | EShop usage |
|---|---|---|---|
| **REST / HTTP** | Sync, JSON | Public and client-facing APIs | Web → Gateway → Catalog, Basket, Ordering |
| **gRPC** | Sync, binary, HTTP/2 | Fast internal calls with strict contracts | Basket → Discount (price calculation) |
| **Messaging (RabbitMQ)** | Async, events | Cross-service workflows, decoupling, fan-out | Basket → Ordering (checkout) |
| **API Gateway (YARP)** | Sync, reverse proxy | Single entry point, rate limiting, security | Shopping.Web → YarpApiGateway |

Real systems **mix** these methods. Keep sync call chains short and use async messaging for everything that does not need an immediate answer.

---

<!-- _class: divider -->
<!-- _header: '' -->

# Part 4
## Demo: EShop Checkout Flow

---

## Demo scenario

```
 ① GET  /catalog-service/products        Client ──REST──► YARP ──► Catalog.API ──► PostgreSQL
 ② POST /basket-service/basket           Client ──REST──► YARP ──► Basket.API
                                                                     │ gRPC GetDiscount("IPhone X")
                                                                     ▼
                                                               Discount.Grpc  (−150)
                                                                     │
                                                         Basket stored in PostgreSQL + Redis
 ③ POST /basket-service/basket/checkout  Client ──REST──► YARP ──► Basket.API
                                                                     │ Publish BasketCheckoutEvent
                                                                     ▼
                                                                 RabbitMQ
                                                                     │ Consume
                                                                     ▼
                                                               Ordering.API ──► SQL Server
 ④ GET  /ordering-service/orders         Client ──REST──► YARP ──► Ordering.API   (new order)
```

This one flow shows **REST, gRPC, pub/sub messaging, and the API Gateway** working together.

---

## Demo setup: run everything with Docker

**Prerequisites:** Docker Desktop, the .NET 8 SDK, and Visual Studio 2022 (optional)

```bash
cd EShopMicroservices-Udemy-Sections-main/section20

# Option A: Visual Studio → set "docker-compose" as the startup project → F5
# Option B: CLI
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --build

docker ps          # 11 containers: 5 backing services + 6 apps
```

| What | URL |
|---|---|
| Shopping.Web | http://localhost:6005 |
| YARP API Gateway | http://localhost:6004 |
| RabbitMQ management UI | http://localhost:15672 (guest / guest) |
| Health checks | http://localhost:6000/health, http://localhost:6001/health |

> Note: Basket calls Discount over HTTPS inside Docker, which needs an ASP.NET dev certificate (`dotnet dev-certs https --trust`). Visual Studio sets this up automatically.

---

## Demo ①: REST through the gateway

```bash
# List products (routed by YARP to catalog.api:8080/products)
curl "http://localhost:6004/catalog-service/products?pageNumber=1&pageSize=5"

# Get a single product
curl http://localhost:6004/catalog-service/products/5334c996-8457-4cf0-815c-ed2b77c4ff61
```

```json
{
  "product": {
    "id": "5334c996-8457-4cf0-815c-ed2b77c4ff61",
    "name": "IPhone X",
    "category": [ "Smart Phone" ],
    "price": 950.00
  }
}
```

**Takeaway:** the client only knows the gateway. The internal service address is hidden.

---

## Demo ②: gRPC call inside the basket

```bash
curl -X POST http://localhost:6004/basket-service/basket \
  -H "Content-Type: application/json" \
  -d '{ "Cart": { "UserName": "swn", "Items": [ {
          "ProductId": "5334c996-8457-4cf0-815c-ed2b77c4ff61",
          "ProductName": "IPhone X", "Price": 950, "Quantity": 1, "Color": "Black" } ] } }'

curl http://localhost:6004/basket-service/basket/swn
```

- Basket.API calls **Discount.Grpc** → `GetDiscount("IPhone X")` → the coupon amount is **150**
- The stored price becomes **950 − 150 = 800**
- Check the Discount container logs:
  `Discount is retrieved for ProductName : IPhone X, Amount : 150`

**Takeaway:** a synchronous gRPC call using the contract generated from `discount.proto`.

---

## Demo ③ and ④: async checkout → order created

```bash
curl -X POST http://localhost:6004/basket-service/basket/checkout \
  -H "Content-Type: application/json" \
  -d '{ "BasketCheckoutDto": {
        "userName": "swn", "customerId": "189dc8dc-990f-48e0-a37b-e6f2b60b9d7d",
        "firstName": "swn", "lastName": "swn", "emailAddress": "test@test.com",
        "addressLine": "34 Charles Street", "country": "USA", "state": "Michigan",
        "zipCode": "48198", "cardName": "swn", "cardNumber": "485-3184",
        "expiration": "11/30", "cvv": "333", "paymentMethod": 1 } }'
# → { "isSuccess": true }   (Basket responds right away; it doesn't wait for Ordering)

curl "http://localhost:6004/ordering-service/orders?pageIndex=0&pageSize=10"
# → the new order with OrderName "swn" appears
```

- **RabbitMQ UI** (localhost:15672) → Exchanges: `BuildingBlocks.Messaging.Events:BasketCheckoutEvent`, plus the kebab-case queue that MassTransit created for the Ordering consumer
- Ordering logs: `Integration Event handled: BasketCheckoutEvent`
- Then open **Shopping.Web** at http://localhost:6005 and go through the same flow in the UI

---

## Summary

- **Microservices** are small, autonomous services built around business capabilities, each with its own data and deployment. They are powerful but complex, so adopt them only for a real reason.
- **To create one:** domain analysis → technical analysis → ASP.NET Core Minimal API → Dockerfile → docker-compose.
- **Invocation methods:**
  - **REST** for client-facing APIs (consumed with Refit)
  - **gRPC** for fast, contract-first internal calls
  - **Async messaging** (RabbitMQ + MassTransit) for decoupled, event-driven workflows
  - **API Gateway** (YARP) as the single entry point, with routing and rate limiting
- **EShop** combines all four in a single checkout flow.

---

## References

- Source code: `EShopMicroservices-Udemy-Sections-main/section4` (TodoApi) … `section20` (final)
- Course slides: `dotnet+8+slides.pdf` (Mehmet Ozkaya, *.NET 8 Microservices*)
- Upstream repo: https://github.com/aspnetrun/run-aspnetcore-microservices
- Martin Fowler, *Microservices*: https://martinfowler.com/articles/microservices.html
- Microsoft, *.NET Microservices Architecture*: https://learn.microsoft.com/dotnet/architecture/microservices/
- gRPC on .NET: https://learn.microsoft.com/aspnet/core/grpc/
- MassTransit: https://masstransit.io · RabbitMQ: https://www.rabbitmq.com
- YARP: https://microsoft.github.io/reverse-proxy/
