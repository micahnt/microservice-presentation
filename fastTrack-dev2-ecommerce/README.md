# fastTrack-dev2-ecommerce

A small e-commerce system built as .NET 8 microservices with a React front end. It runs locally
with Docker Compose.

```
Browser (React SPA)
   │  http://localhost:6004  — UI and API on one origin, so no CORS
   ▼
YARP gateway ──/identity-service/*──▶ Identity API  ──▶ PostgreSQL (IdentityDb)
             ──/catalog-service/*───▶ Catalog API   ──▶ PostgreSQL (CatalogDb)
             ──/basket-service/*────▶ Basket API    ──▶ Redis
             ──/ordering-service/*──▶ Ordering API  ──▶ SQL Server (OrderDb)

Basket ──HTTP (Refit)──▶ Catalog     current price and stock
Basket ──gRPC (h2c)───▶ Discount    coupon per product (SQLite, internal only)
Basket ──BasketCheckoutEvent──▶ RabbitMQ ──▶ Ordering   creates the order
Ordering ──OrderCreatedEvent (outbox)──▶ RabbitMQ ──▶ Catalog   decrements stock
```

| Service | What it owns |
|---|---|
| **Identity** | Users and roles (ASP.NET Core Identity). `register` and `login` return a JWT. |
| **Catalog** | Products and stock (EF Core + Npgsql). Anyone can read; only admins can write. |
| **Basket** | One basket per user in Redis (30-day expiry). Prices are recomputed on every save. |
| **Discount** | Coupons over gRPC. Internal, not routed by the gateway. |
| **Ordering** | Orders (Clean Architecture + DDD, EF Core SQL Server). Includes a simulated payment. |
| **Gateway** | YARP routes plus the built React app. |
| **Web** | React 18 + Vite + TypeScript app: shop, basket, checkout, my orders, and admin pages. |

There is no MediatR, Mapster, Marten or Carter. Endpoints are controllers that call use-case classes
(`ProductCreator`, `OrderReader`, …). Validation is FluentValidation (auto-validation), mapping is
hand-written `ToDto()` methods, and errors are returned as RFC 7807 `ProblemDetails`.

## Run it

Requirements: Docker Desktop. For running outside Docker you also need .NET 8 SDK, Node 20+ and
Yarn 1 (`npm install -g yarn`).

```bash
cd src
cp .env.example .env          # JWT_SIGNING_KEY: every service validates tokens with it
docker compose up --build -d
```

Open **http://localhost:6004**. The first start takes a minute: SQL Server needs time to boot,
and services restart until their database is ready.

| Account | Password | Role |
|---|---|---|
| `admin@shop.local` | `Admin123` | Admin: manage products and orders. Can also shop. |
| `customer@shop.local` | `Customer123` | Customer |

You can also register a new account (it gets the Customer role).

Reset all data: `docker compose down -v && docker compose up --build -d`.

### UI with hot reload

Keep the stack running, then:

```bash
cd src/Web
yarn
yarn dev                      # http://localhost:5173, proxies /*-service to the gateway on 6004
```

### Services from the IDE

Each project has `Properties/launchSettings.json` that uses the same host port as Docker
(table below), and its `appsettings.json` points to `localhost`. To run one service in the IDE:
stop its container, then start the project. The gateway's IDE profile uses `appsettings.Local.json`.

## Ports

| Component | URL |
|---|---|
| Gateway (UI + API) | http://localhost:6004 |
| Catalog API | http://localhost:6000/swagger |
| Basket API | http://localhost:6001/swagger |
| Discount gRPC | `grpcurl -plaintext localhost:6002 list` |
| Ordering API | http://localhost:6003/swagger |
| Identity API | http://localhost:6005/swagger |
| PostgreSQL (`catalogdb`) | localhost:5432, `postgres` / `postgres` |
| SQL Server (`orderdb`) | localhost:1433, `sa` / `SwN12345678` |
| Redis (`distributedcache`) | localhost:6379 |
| RabbitMQ (`messagebroker`) | http://localhost:15672, `guest` / `guest` |

**Getting a token for Swagger:** call `POST http://localhost:6004/identity-service/auth/login`
with `{ "email": "admin@shop.local", "password": "Admin123" }`. Then click **Authorize** in Swagger
and paste the token.

**Postman:** import `src/Ecommerce.postman_collection.json`. Run a *Login* request first; it saves
`{{token}}` for the other requests.

## How a purchase flows

1. The UI posts `{ productId, quantity }` lines to `POST /basket-service/basket`. Basket gets the
   price and stock from Catalog and the coupon from Discount, then saves the basket in Redis.
   The client never sends prices, so a discount can't be applied twice.
2. `POST /basket-service/basket/checkout` takes a shipping address and a payment method
   (`CashOnDelivery` or `FakeCard`). Basket publishes `BasketCheckoutEvent`, deletes the basket,
   and returns `202` with a `checkoutId`. No card data is sent or stored anywhere.
3. Ordering consumes the event, adds the customer if needed, and creates the order with a
   snapshot of each item's name and price.
   - **FakeCard:** a fake gateway charges the card. Orders up to $5,000 become `Paid`; larger
     ones become `PaymentFailed`.
   - **CashOnDelivery:** the order stays `Pending`.
4. Unless the payment failed, Ordering publishes `OrderCreatedEvent` through the EF Core
   **outbox**, so it is sent only after the order is committed. Catalog consumes it and
   decrements stock. The inbox drops redelivered messages, and the unique `Orders.CheckoutId`
   prevents a duplicate order.
5. An admin moves the order along: `Pending`/`Paid` → `Shipped` → `Completed`, or `Cancelled`.

## Permissions

All services validate the JWT (HS256 key shared through `JWT_SIGNING_KEY`). Every endpoint
requires a signed-in user unless it is marked anonymous.

| Action | Who |
|---|---|
| Browse products, `/health` | Anyone |
| Basket, checkout, my orders | Customer or Admin. The user id comes from the token, never from the URL. |
| Create, update or delete products; all orders; change order status; delete orders | Admin |

## Repository layout

```
src/
├── Ecommerce.sln, docker-compose*.yml, .env.example, Ecommerce.postman_collection.json
├── Shared/
│   ├── Ecommerce.Shared            JWT auth + ICurrentUser, GlobalExceptionHandler, Swagger, pagination
│   └── Ecommerce.Shared.Messaging  event records, AddMessageBroker (MassTransit 8 + EF outbox)
├── Services/
│   ├── Identity/Ecommerce.Identity.Api
│   ├── Catalog/Ecommerce.Catalog.Api
│   ├── Basket/Ecommerce.Basket.Api
│   ├── Discount/Ecommerce.Discount.Grpc
│   └── Ordering/Ecommerce.Ordering.{Api,Application,Domain,Infrastructure}
├── ApiGateways/Ecommerce.YarpApiGateway   its Dockerfile also builds the Web app
└── Web                                    React + Vite + TypeScript
```

The HTTP services share this folder layout:

| Folder | Contents |
|---|---|
| `Presentation/` | Controllers, request/response models, validators, mappers |
| `ApplicationLogic/` | Use cases |
| `Data/` | `DbContext` or repository, plus migrations |
| `Startup/` | Service registration, migrate and seed |

Ordering keeps its four Clean Architecture projects.

## Database migrations

Services apply their migrations at startup in Development. To add a migration, use the repo-local
`dotnet-ef` (8.x). Run these from `src/`, and always pass `--startup-project`, because otherwise
`docker-compose.dcproj` gets picked as the startup project.

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project Services/Catalog/Ecommerce.Catalog.Api --startup-project Services/Catalog/Ecommerce.Catalog.Api --output-dir Data/Migrations
dotnet ef migrations add <Name> --project Services/Identity/Ecommerce.Identity.Api --startup-project Services/Identity/Ecommerce.Identity.Api --output-dir Data/Migrations
dotnet ef migrations add <Name> --project Services/Ordering/Ecommerce.Ordering.Infrastructure --startup-project Services/Ordering/Ecommerce.Ordering.Api --output-dir Data/Migrations
```

## Notes

- **Package versions:** pin `Microsoft.*` and `Npgsql.*` packages to 8.x (`--version 8.*`); without
  it, `dotnet add package` installs 10.x, which doesn't support `net8.0`. Stay on
  **MassTransit 8.x**, because 9.x needs a commercial license.
- **Feature flag:** `FeatureManagement:OrderFullfilment` controls whether Ordering publishes
  `OrderCreatedEvent`, which drives the stock updates. It is on in compose.
- **Discount coupons** are matched by product name (`discount.proto`). Renaming a product in
  Catalog drops its coupon.
