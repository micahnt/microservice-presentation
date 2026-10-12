---
marp: true
theme: default
paginate: true
size: 16:9
header: 'Microservices with .NET 8 — EShop Microservices'
footer: 'Microservices & DDD · REST & gRPC · Messaging · API Gateway'
style: |
  section { font-size: 24px; }
  section.lead { text-align: center; justify-content: center; }
  section.lead h1 { font-size: 56px; }
  section.divider { background: #1f3b57; color: #fff; justify-content: center; }
  section.divider h1, section.divider h2 { color: #fff; }
  pre { font-size: 15px; line-height: 1.35; }
  table { font-size: 19px; }
  .cols { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 28px; }
  .small { font-size: 18px; }
  section.dense { font-size: 20px; }
  section.dense table { font-size: 17px; }
  section.dense pre { font-size: 13px; line-height: 1.25; }
  section.dense h2 { margin-bottom: 8px; }
---

<!-- _class: lead -->
<!-- _header: '' -->
<!-- _footer: '' -->

# Microservices with .NET 8

### Microservices & DDD · REST and gRPC · Messaging · API Gateway

Reference project: **EShopMicroservices** (Catalog, Basket, Discount, Ordering, YARP gateway, Shopping.Web)

---

## Agenda (60 minutes)

| # | Topic | Time |
|---|---|---|
| | Opening | ~2&nbsp;min |
| 1 | **Introduction to Microservices & DDD**: monolith vs. microservices, design principles, DDD, bounded contexts, entities, value objects | ~16&nbsp;min |
| 2 | **Building a microservice** (short): from analysis to a running container | ~5&nbsp;min |
| 3 | **Synchronous communication**: service-to-service calls, REST vs. gRPC, Protobuf and gRPC in .NET | ~10&nbsp;min |
| 4 | **Asynchronous communication**: event-driven basics, message brokers (RabbitMQ), pub/sub | ~10&nbsp;min |
| 5 | **API Gateway & BFF**: role of a gateway, Backends for Frontends, YARP and Ocelot | ~7&nbsp;min |
| 6 | **Demo**: one checkout, end to end | ~6&nbsp;min |
| 7 | Summary and Q&A | ~4&nbsp;min |

---

<!-- _class: divider -->
<!-- _header: '' -->

# Part 1
## Introduction to Microservices & DDD

---

## Architecture design journey

```
   Monolithic   ──►  Modular Monolithic  ──►   Microservices    ──►    Serverless

   Monolithic         Macroservices            Microservices           Nanoservices
 ┌─────────────┐   ┌──────────────────┐   ┌─────┐┌─────┐┌─────┐     (f) (f) (f) (f)
 │ Application │   │     Service      │   │ Svc ││ Svc ││ Svc │     (f) (f) (f) (f)
 └─────────────┘   ├────────┬─────────┤   └─────┘└─────┘└─────┘     (f) (f) (f) (f)
    ( 1 DB )       │Service │ Service │    (DB)   (DB)   (DB)       functions
                   └────────┴─────────┘
                    ( DB )   ( DB )        each service owns its DB
```

**Which approach should we choose?**
- **It depends on your project.** There is no single right answer.
- Use a **mixed architecture**, and plan **transitions** between architectures as the system grows.
- Martin Fowler and Sam Newman both recommend **monolith first**, then extracting services one by one.

---

## What are microservices?

- **Small, independent, and loosely coupled** services that work together.
- Each service has its **own codebase**, owned by a **small team**.
- Services communicate through **well-defined APIs** (HTTP/REST, gRPC, messaging).
- Each service is **deployed independently and autonomously**.
- **Technology agnostic**: each service can use a different stack.
- Each service has **its own database**, which is not shared with other services.

> "…an approach to developing a single application as a suite of small services, each running in its own process and communicating with lightweight mechanisms, often an HTTP resource API."
> — Martin Fowler

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

```
 Monolith:       Team 1 ┐
                 Team 2 ├─► Single code base ──► 1 deployment pipeline ──► Production
                 Team 3 ┘
 Microservices:  Product team ──► Product code ──► own pipeline ──► Product service
                 Basket team  ──► Basket code  ──► own pipeline ──► Basket service
                 Order team   ──► Order code   ──► own pipeline ──► Order service
```

---

## Microservices design principles

<div class="cols">
<div>

1. **Single responsibility**: one service per **business capability** (Catalog, Basket, Ordering), not per technical layer
2. **Loose coupling, high cohesion**: change one service without changing others; related logic stays together
3. **Autonomy**: deploy independently and **own your data** (database per service)

</div>
<div>

4. **Well-defined contracts**: talk only through APIs and events. *Smart endpoints, dumb pipes*
5. **Design for failure**: timeouts, retries, circuit breakers; one failing service must not take down the rest
6. **Automate and observe**: CI/CD, containers, health checks, centralized logging

</div>
</div>

<span class="small">Based on Martin Fowler's microservice characteristics: componentization via services, business capabilities, products not projects, smart endpoints and dumb pipes, decentralized governance and data, infrastructure automation, design for failure.</span>

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

## Introduction to Domain-Driven Design (DDD)

- Introduced by **Eric Evans** (*Domain-Driven Design*, 2003). Not a technology or framework, but a way to **model software around the business**.
- Core idea: **break a complex problem into smaller parts** and solve each one in its own model.

<div class="cols">
<div>

### Strategic DDD — *the big picture*
- Understand and model the **business domain**
- Find **subdomains** and **bounded contexts**
- Define how they relate (**context map**)
- 👉 Tells us **where to split** into microservices

</div>
<div>

### Tactical DDD — *inside one service*
- Design patterns for the code
- **Entities**, **value objects**, **aggregates**
- Domain events, repositories, domain services
- 👉 Tells us **how to build** one service's model

</div>
</div>

---

## DDD core concepts

| Concept | Meaning | In EShop |
|---|---|---|
| **Domain** | The business problem the software solves | E-commerce |
| **Subdomain** | One area of expertise inside the domain | Product catalog, shopping cart, discounts, orders |
| **Ubiquitous language** | Shared words used by domain experts *and* in the code | `Order`, `OrderItem`, `ShippingAddress`, `Payment` appear in code exactly as the business says them |
| **Bounded context** | A logical boundary where one model and one language apply | Catalog, Basket, Discount, Ordering |
| **Context map** | How the bounded contexts relate and communicate | Basket → Discount (gRPC), Basket → Ordering (event) |

---

## Bounded contexts → microservices

**Requirements** → user stories → **nouns** (entities) and **verbs** (use cases) → **bounded contexts**

| User story | Noun / verb | Bounded context → service |
|---|---|---|
| List and filter products by category | Product, Category | **Catalog** |
| Put products into the shopping cart | ShoppingCart, Item | **Basket** |
| Apply coupons and see the total cost | Coupon, Discount | **Discount** |
| Check out the cart and create an order | Order, Address, Payment | **Ordering** |

- **Same word, different model**: `Product` in Catalog has name, description, image, categories and price. `Product` in Ordering has only an ID, name and price.
- A bounded context is *usually* one microservice, but it can be split further for scaling needs.

---

## Tactical DDD: entities, value objects, aggregates

| | **Entity** | **Value object** |
|---|---|---|
| Identified by | Its **ID** | Its **values** (no ID) |
| Two with the same data are… | Still **different** objects | **Equal** |
| Changes over time? | Yes, has a **lifecycle** | No, **immutable**: replace it instead |
| In EShop Ordering | `Order`, `OrderItem`, `Customer`, `Product` | `Address`, `Payment`, `OrderName`, `OrderId` |

**Aggregate**: a cluster of entities and value objects treated as **one unit of consistency**. Its main entity is the **aggregate root**, the only entry point.

```
 Order  (aggregate root)  ── OrderId, CustomerId, OrderName, Address ×2, Payment   ← value objects
   └── OrderItem × N      (entities, changed only through Order.Add / Order.Remove)
```

---

<!-- _class: dense -->

## Entities and value objects in the Ordering code

<div class="cols">
<div>

```csharp
// Entity: identity + audit fields
public abstract class Entity<T> : IEntity<T>
{
    public T Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
    // ...
}

// Aggregate root: owns its items, raises events
public class Order : Aggregate<OrderId>
{
    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyList<OrderItem> OrderItems
        => _orderItems.AsReadOnly();
    public Address ShippingAddress { get; private set; }

    public void Add(ProductId productId, int quantity, decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        _orderItems.Add(new OrderItem(Id, productId, quantity, price));
    }
}
```

</div>
<div>

```csharp
// Value object: a C# record = equality by value
public record Address
{
    public string FirstName { get; } = default!;
    public string AddressLine { get; } = default!;
    public string Country { get; } = default!;
    // ...
    public static Address Of(string firstName, ...,
        string emailAddress, string addressLine, ...)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(emailAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(addressLine);
        return new Address(firstName, ...);
    }
}

// Strongly typed ID: also a value object
public record OrderId
{
    public Guid Value { get; }
    public static OrderId Of(Guid value) =>
        value == Guid.Empty
            ? throw new DomainException("OrderId cannot be empty.")
            : new OrderId(value);
}
```

</div>
</div>

<span class="small">Simplified from `Ordering.Domain`. Entities are created with `Create(...)`, value objects with `Of(...)`, and both validate their input.</span>

---

## The big picture: EShop microservices

```
 CLIENT APPS        API GATEWAY              MICROSERVICES                          EVENT BUS

                    ┌────────────┐       ┌───────────────────┐
                    │            │──────►│ Catalog           │──► PostgreSQL (Marten)
                    │            │       │ Vertical Slice    │
                    │            │       └───────────────────┘
┌──────────────┐    │            │       ┌───────────────────┐                     ┌───────────┐
│ Shopping.Web │───►│    YARP    │──────►│ Basket            │──► Postgres + Redis │           │
│ Razor + Refit│    │ API Gateway│       │ Vertical Slice    │──── publish ───────►│ RabbitMQ  │
└──────────────┘    │            │       └─────────┬─────────┘                     │     +     │
                    │            │            gRPC │                               │MassTransit│
                    │            │       ┌─────────▼─────────┐                     │           │
                    │            │       │ Discount.Grpc     │──► SQLite (EF Core) │           │
                    │            │       └───────────────────┘                     │           │
                    │            │       ┌───────────────────┐                     │           │
                    │            │──────►│ Ordering          │──► SQL Server       │           │
                    │            │       │ Clean Arch + DDD  │◄──── consume ───────│           │
                    └────────────┘       └───────────────────┘                     └───────────┘
```

<span class="small">Based on the course's *Big Picture* slide. The gateway routes to Catalog, Basket, and Ordering (`appsettings.json`). Discount is reached only through Basket's gRPC client.</span>

---

<!-- _class: divider -->
<!-- _header: '' -->

# Part 2
## Building a Microservice (short)

---

<!-- _class: dense -->

## From analysis to a running service

<div class="cols">
<div>

**The process for each service**
1. **Domain analysis**: models, use cases, API endpoints, data store
2. **Technical analysis**: architecture style, patterns, libraries, folders
3. **Develop** with ASP.NET Core 8 Minimal APIs
4. **Containerize** with a Dockerfile
5. **Orchestrate** with docker-compose

**`Program.cs` always has 5 steps:** create the builder → register services (DI) → build → configure the pipeline → run

</div>
<div>

**The smallest microservice** (`section4/TodoApi`)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt =>
    opt.UseInMemoryDatabase("TodoList"));
var app = builder.Build();

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapPost("/todoitems", async (TodoItem todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.Run();
```

One process, one responsibility, its own data store, an HTTP API.

</div>
</div>

---

<!-- _class: dense -->

## A real service: Catalog.API (Vertical Slice + CQRS)

<div class="cols">
<div>

```
Catalog.API/
├── Models/Product.cs
├── Products/          ← one folder per feature
│   ├── CreateProduct/
│   │   ├── CreateProductEndpoint.cs
│   │   └── CreateProductHandler.cs
│   ├── GetProductById/
│   ├── GetProducts/
│   ├── UpdateProduct/
│   └── DeleteProduct/
├── Dockerfile
└── Program.cs
```

**Libraries:** Carter (endpoints), MediatR (CQRS), Marten (PostgreSQL as a document DB), FluentValidation, Mapster

</div>
<div>

**Request flow:** HTTP → endpoint → MediatR → handler → DB

```csharp
// Endpoint (Carter module, simplified)
app.MapGet("/products/{id}", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GetProductByIdQuery(id));
    return Results.Ok(result.Adapt<GetProductByIdResponse>());
});

// Query handler (simplified)
internal class GetProductByIdQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(
        GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await session.LoadAsync<Product>(query.Id, ct)
            ?? throw new ProductNotFoundException(query.Id);
        return new GetProductByIdResult(product);
    }
}
```

</div>
</div>

---

<!-- _class: dense -->

## Containerize and orchestrate

<div class="cols">
<div>

**Multi-stage Dockerfile** (generated by Visual Studio)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base  # small runtime
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build     # full SDK
COPY . .
RUN dotnet build "./Catalog.API.csproj" -c Release

FROM build AS publish
RUN dotnet publish "./Catalog.API.csproj" -c Release -o /app/publish

FROM base AS final                                 # only the output
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
```

**Dockerfile → image → container**

</div>
<div>

**docker-compose** runs everything together

```yaml
services:
  catalogdb:
    image: postgres
  catalog.api:
    build:
      dockerfile: Services/Catalog/Catalog.API/Dockerfile
    environment:
      - ConnectionStrings__Database=Server=catalogdb;...
    depends_on: [ catalogdb ]
    ports:
      - "6000:8080"
```

- Inside Docker, services find each other by **service name**: `http://catalog.api:8080`
- EShop runs **11 containers**: 5 backing services + 6 apps

</div>
</div>

---

<!-- _class: divider -->
<!-- _header: '' -->

# Part 3
## Synchronous Communication: REST & gRPC

---

## Service-to-service communication: sync vs. async

Services must cooperate: **ask for data** ("what's the discount?") or **trigger work** ("create the order").

| | **Synchronous** | **Asynchronous** |
|---|---|---|
| How it works | Client sends a request and **waits** for the response | Client sends a message and **does not wait** |
| Protocols | HTTP/REST, **gRPC**, GraphQL, WebSocket | **AMQP** via a broker (RabbitMQ, Kafka) |
| Patterns | Request / response | One-to-one (queue), one-to-many (**pub/sub** topic) |
| Coupling | Temporal coupling: both services must be up | Loose: the broker stores and retries |
| EShop example | Basket → Discount (**gRPC**)<br>Web → Gateway → APIs (**REST**) | Basket → Ordering (**BasketCheckout event**) |

**Rule of thumb:** REST for public APIs, gRPC for internal service-to-service calls, messaging for workflows that span services.

---

## REST over HTTP

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

<!-- _class: dense -->

## Introduction to Protobuf and gRPC

<div class="cols">
<div>

**gRPC**: open-source RPC framework from Google
- Runs on **HTTP/2**
- The client calls a remote method **as if it were a local object**

**Protocol Buffers (Protobuf)**
- Language-neutral **contract** in a `.proto` file
- `service` = the methods, `message` = the data
- `= 1, = 2…` are **field numbers** for the binary encoding, not default values
- Compact **binary** format, smaller and faster than JSON
- In .NET, the build **generates** the server base class and the client from the `.proto` file

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

## gRPC in .NET: the server (Discount.Grpc)

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
// Services/DiscountService.cs — inherits the generated base class (simplified)
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

<!-- _class: dense -->

## gRPC in .NET: the client (Basket.API calls Discount)

```xml
<!-- Basket.API.csproj — generate only the client stub from the shared .proto file -->
<Protobuf Include="..\..\Discount\Discount.Grpc\Protos\discount.proto" GrpcServices="Client" />
```

```csharp
// Program.cs — register a typed gRPC client
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!))
  .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
  {   // development only: accept the self-signed dev certificate
      ServerCertificateCustomValidationCallback =
          HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
  });
```

```csharp
// StoreBasketHandler.cs — call it like a local method (Handle omitted)
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

## When to use REST, when to use gRPC

| | **REST** | **gRPC** |
|---|---|---|
| Transport | HTTP/1.1 or HTTP/2 | HTTP/2 |
| Payload | JSON (text, human-readable) | Protobuf (binary, smaller and faster) |
| Contract | Optional (OpenAPI) | **Required** (`.proto`), code is generated |
| Browser support | Native | Needs gRPC-Web or a proxy |
| Streaming | Request/response only | Client, server and **bidirectional** streaming |
| Easy to test by hand | Yes: curl, browser, Postman | Needs gRPC tooling (Postman, grpcurl) |

<div class="cols">
<div>

**Choose REST** for public APIs, browsers, third parties, and simple CRUD.

</div>
<div>

**Choose gRPC** for internal, high-volume, low-latency calls between your own services.

</div>
</div>

**In EShop:** REST behind the gateway for the web app; gRPC for Basket → Discount, called once per cart item.

---

<!-- _class: divider -->
<!-- _header: '' -->

# Part 4
## Asynchronous Communication & Message Brokers

---

## Basic event-driven architecture

<div class="cols">
<div>

**The problem: chains of sync calls**

```
 Gateway → ShoppingCart → Product
                        → Pricing
                        → Order → Billing
                                → Shipping
```

Six calls for one click. Every service must be up, latencies add up, and one slow service slows everyone.

</div>
<div>

**The idea: publish events**
- An **event** is a fact that **already happened**, named in the past tense: *BasketCheckedOut*, *OrderCreated*
- The **producer** publishes it and moves on. It does **not know** who listens.
- **Consumers** react in their own time
- **Command vs. event**: "CreateOrder" tells one service what to do; "BasketCheckedOut" announces what happened to anyone interested

</div>
</div>

---

## Domain events vs. integration events

| | **Domain event** | **Integration event** |
|---|---|---|
| Scope | **Inside** one service / bounded context | **Between** services |
| Transport | In-process, in-memory (MediatR) | **Message broker** (RabbitMQ) |
| EShop example | `OrderCreatedEvent`, raised by the `Order` aggregate | `BasketCheckoutEvent`, from Basket to Ordering |

```
   Basket service                             Ordering service
 ┌──────────────┐  BasketCheckoutEvent   ┌───────────────────────────────────────────────┐
 │ ShoppingCart │ ─────(RabbitMQ)──────► │ Handler ──► Order.Create(...)                 │
 └──────────────┘   integration event    │               │ raises OrderCreatedEvent      │
                                         │               ▼ (domain event, via MediatR)   │
                                         │ OrderCreatedEventHandler ──► could publish    │
                                         │   OrderCreatedIntegrationEvent (feature flag) │
                                         └───────────────────────────────────────────────┘
```

---

## The role of a message broker (RabbitMQ)

<div class="cols">
<div>

**What the broker does**
- **Receives** messages from producers
- **Stores** them until consumers are ready (even if a consumer is down)
- **Routes** them to the right queues
- **Delivers** and **retries**
- Decouples services in **time** (not both up at once) and **location** (no addresses needed)

**MassTransit** is a .NET library on top of RabbitMQ: it creates exchanges and queues and handles serialization and retries.

</div>
<div>

**RabbitMQ**: an open-source broker that implements **AMQP**

Producer → **Exchange** → (binding) → **Queue** → Consumer

| Exchange | Routes messages |
|---|---|
| Direct | By exact routing key, to one queue |
| **Fanout** | To **all** bound queues (broadcast) |
| Topic | By routing-key pattern |
| Headers | By message header attributes |

Similar brokers: Apache Kafka, Azure Service Bus, ActiveMQ

</div>
</div>

---

<!-- _class: dense -->

## Basic pub/sub pattern

```
                                            ┌──► queue ──► Ordering       (creates the order)      ← in EShop
  Basket ──publish BasketCheckoutEvent──► Exchange
  (publisher)                             (fanout)
                                            ├──► queue ──► Notification   (sends an email)         ← could be added
                                            └──► queue ──► Analytics      (records the sale)       ← could be added
```

<div class="cols">
<div>

**How it works**
- The publisher sends **one** message to an exchange (a *topic*)
- The broker **fans out** a copy to every subscriber's queue
- Subscribers work **in parallel and independently**
- **Add a subscriber without changing the publisher**

**Queue vs. pub/sub:** a queue gives each message to **one** consumer; pub/sub gives **every** subscriber its own copy.

</div>
<div>

**Watch out for**
- **At-least-once delivery**: a message can arrive twice, so consumers must be **idempotent**
- **No guaranteed order** of messages
- The broker is critical: run it **highly available**
- Harder to **debug and trace** across services
- **Eventual consistency**: the order appears a moment after checkout, not instantly

</div>
</div>

---

<!-- _class: dense -->

## Messaging setup: the shared BuildingBlocks.Messaging library

<div class="cols">
<div>

**The event contract** (shared by publisher and consumer)

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

</div>
<div>

**One extension method** registers MassTransit + RabbitMQ in any service

```csharp
public static IServiceCollection AddMessageBroker(
    this IServiceCollection services,
    IConfiguration configuration, Assembly? assembly = null)
{
  services.AddMassTransit(config =>
  {
    config.SetKebabCaseEndpointNameFormatter();
    if (assembly != null)
        config.AddConsumers(assembly);   // consumers only

    config.UsingRabbitMq((context, cfg) =>
    {
      cfg.Host(new Uri(configuration["MessageBroker:Host"]!),
        host =>
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

</div>
</div>

---

## Publish (Basket) → consume (Ordering)

<div class="cols">
<div>

**Publisher: Basket.API**
```csharp
// Program.cs
builder.Services.AddMessageBroker(builder.Configuration);

// CheckoutBasketHandler.cs (simplified)
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

<!-- _class: divider -->
<!-- _header: '' -->

# Part 5
## API Gateway & BFF

---

<!-- _class: dense -->

## The role of an API Gateway

<div class="cols">
<div>

**Without a gateway**, every client must know every service's address, and every service must handle auth, SSL and rate limiting itself.

**With a gateway**: one **single entry point** between clients and services

1. **Gateway routing**: route each request to the right internal service (layer 7)
2. **Gateway aggregation**: combine several service calls into one response
3. **Gateway offloading**: handle cross-cutting concerns in one place

**Risk:** a single gateway can become a **bottleneck** or **single point of failure**.

</div>
<div>

**Request flow through a gateway**

```
 Client
   │
   ▼
 ┌───────────────────────────┐
 │ Request validation        │
 │ IP allowlisting           │
 │ Authentication / authz    │
 │ Rate limiting             │
 │ Service discovery         │
 │ Protocol conversion       │
 └────────────┬──────────────┘
              ▼
 Customer · Basket · Discount · Order
```

</div>
</div>

---

## Pattern: Backends for Frontends (BFF)

```
   Web app      ──►  ┌────────────────────┐ ──┐
                     │ Web BFF gateway    │   │
   Mobile app   ──►  ├────────────────────┤ ──┼──►  Customer · ShoppingCart · Discount · Order
                     │ Mobile BFF gateway │   │
   Desktop app  ──►  ├────────────────────┤ ──┘
                     │ Desktop BFF gateway│
                     └────────────────────┘
```

- **One gateway per type of client**, instead of one gateway for everyone
- Each BFF returns **exactly what its frontend needs**: a phone screen needs less data than a desktop page
- Avoids one big gateway full of client-specific logic, which becomes a bottleneck
- Each frontend team can **own and change its own BFF**
- EShop has **one** client (Shopping.Web), so one gateway is enough. Add BFFs when a mobile app arrives.

---

<!-- _class: dense -->

## Introduction to YARP (Yet Another Reverse Proxy)

<div class="cols">
<div>

- **Reverse proxy** library from Microsoft, built on ASP.NET Core
- Configured with **routes** (what to match) and **clusters** (where to send)
- Supports HTTP/2, **gRPC** and WebSockets, load balancing, health checks, and transforms
- Plugs into normal ASP.NET Core middleware: **rate limiting**, auth

**The whole EShop gateway** (`YarpApiGateway/Program.cs`)

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

<!-- _class: dense -->

## Introduction to Ocelot, and YARP vs. Ocelot

**Ocelot** is an open-source **API gateway** library for .NET, configured with an `ocelot.json` file.

| | **YARP** | **Ocelot** |
|---|---|---|
| From | Microsoft | Open-source community |
| What it is | Reverse-proxy **toolkit**, highly customizable in C# | Ready-made **API gateway** |
| Config | Routes + clusters (`appsettings.json` or code) | Upstream/downstream routes (`ocelot.json`) |
| Built in | Routing, load balancing, health checks, transforms | Routing, rate limiting, auth, caching, request aggregation |
| Extra features | From ASP.NET Core middleware | Mostly through Ocelot's own config |
| In EShop | ✅ Used | ❌ Not used |

<div class="cols">
<div>

```json
{ "Routes": [ {
    "UpstreamPathTemplate": "/catalog-service/{everything}",
    "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete" ],
    "DownstreamPathTemplate": "/{everything}",
    "DownstreamScheme": "http",
    "DownstreamHostAndPorts": [ { "Host": "catalog.api", "Port": 8080 } ]
} ] }
```

</div>
<div>

```csharp
builder.Configuration.AddJsonFile("ocelot.json");
builder.Services.AddOcelot(builder.Configuration);
var app = builder.Build();
await app.UseOcelot();
app.Run();
```

<span class="small">Illustration only: the same Catalog route, written for Ocelot.</span>

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

<!-- _class: dense -->

## Demo: one checkout, end to end

```
 ① POST /basket-service/basket            Client ──REST──► YARP ──► Basket ──gRPC──► Discount (−150)
 ② POST /basket-service/basket/checkout   Client ──REST──► YARP ──► Basket ──publish──► RabbitMQ
                                                                                         │ consume
                                                                                         ▼
                                                                         Ordering ──► SQL Server
 ③ GET  /ordering-service/orders          Client ──REST──► YARP ──► Ordering   (new order)
```

**Setup** (started before the talk, from `section20`):

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --build
```

| What | URL |
|---|---|
| YARP API Gateway | http://localhost:6004 |
| RabbitMQ management UI | http://localhost:15672 (guest / guest) |
| Shopping.Web | http://localhost:6005 |

> Basket calls Discount over HTTPS inside Docker, which needs an ASP.NET dev certificate (`dotnet dev-certs https --trust`). Visual Studio sets this up automatically.

---

<!-- _class: dense -->

## Demo commands

```bash
# ① Store a basket: REST through the gateway, then gRPC to Discount
curl -X POST http://localhost:6004/basket-service/basket -H "Content-Type: application/json" \
  -d '{ "Cart": { "UserName": "swn", "Items": [ { "ProductId": "5334c996-8457-4cf0-815c-ed2b77c4ff61",
        "ProductName": "IPhone X", "Price": 950, "Quantity": 1, "Color": "Black" } ] } }'
curl http://localhost:6004/basket-service/basket/swn          # price is now 800 (950 − 150)

# ② Check out: Basket publishes BasketCheckoutEvent to RabbitMQ
curl -X POST http://localhost:6004/basket-service/basket/checkout -H "Content-Type: application/json" \
  -d '{ "BasketCheckoutDto": { "userName": "swn", "customerId": "189dc8dc-990f-48e0-a37b-e6f2b60b9d7d",
        "firstName": "swn", "lastName": "swn", "emailAddress": "test@test.com",
        "addressLine": "34 Charles Street", "country": "USA", "state": "Michigan", "zipCode": "48198",
        "cardName": "swn", "cardNumber": "485-3184", "expiration": "11/30", "cvv": "333",
        "paymentMethod": 1 } }'                                  # → { "isSuccess": true }, immediately

# ③ The order was created asynchronously by Ordering
curl "http://localhost:6004/ordering-service/orders?pageIndex=0&pageSize=10"   # seeded ORD_1, ORD_2 + "swn"
```

**What to watch:** Discount log `Discount is retrieved for ProductName : IPhone X, Amount : 150` · RabbitMQ exchange `BuildingBlocks.Messaging.Events:BasketCheckoutEvent` · Ordering logs `Integration Event handled: BasketCheckoutEvent`, then `Domain Event handled: OrderCreatedEvent`

<span class="small">The course code creates the order with fixed demo items (`BasketCheckoutEventHandler`, marked TODO), not the basket's items.</span>

---

## Summary

- **Microservices** are small, autonomous services built around business capabilities, each with its own data and deployment. Adopt them only for a real reason.
- **DDD** tells us **where to split** (bounded contexts, ubiquitous language) and **how to model** each service (entities, value objects, aggregates).
- **Synchronous**: **REST** for client-facing APIs, **gRPC + Protobuf** for fast, contract-first internal calls.
- **Asynchronous**: **events** through a **message broker** (RabbitMQ + MassTransit) with **pub/sub**, for decoupled workflows.
- **API Gateway** (YARP or Ocelot) as the single entry point; **BFF** when you have several kinds of clients.
- **EShop** combines all of these in a single checkout flow.

---

## References

- Course: **.NET 8 Microservices: DDD, CQRS, Vertical/Clean Architecture** (Mehmet Ozkaya, Udemy): https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/
- Course slides: `dotnet+8+slides.pdf` · Source code: `section4` (TodoApi) … `section20` (final)
- Upstream repo: https://github.com/aspnetrun/run-aspnetcore-microservices
- Martin Fowler, *Microservices*: https://martinfowler.com/articles/microservices.html
- Eric Evans, *Domain-Driven Design: Tackling Complexity in the Heart of Software* (2003)
- Microsoft, *.NET Microservices Architecture*: https://learn.microsoft.com/dotnet/architecture/microservices/
- gRPC on .NET: https://learn.microsoft.com/aspnet/core/grpc/ · MassTransit: https://masstransit.io · RabbitMQ: https://www.rabbitmq.com
- YARP: https://microsoft.github.io/reverse-proxy/ · Ocelot: https://github.com/ThreeMammals/Ocelot

---

<!-- _class: lead -->
<!-- _header: '' -->
<!-- _footer: '' -->

# Questions?

### Thank you!

Code: `section20` · Postman: `EShopMicroservices.postman_collection.json`
