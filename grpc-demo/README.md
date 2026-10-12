# gRPC demo: Basket calls Discount (no Docker)

A cut-down version of course sections 10–12. Two services run locally with `dotnet run`:

```
 HTTP/JSON (REST)                 gRPC (HTTP/2 + Protobuf)
 curl / .http ──► Basket.API ─────────────────────────────► Discount.Grpc ──► SQLite (discountdb file)
                  :5001          GetDiscount / CreateDiscount     :5002
                  (in-memory carts)
```

| Project | Role | Based on |
|---|---|---|
| `Discount.Grpc` | gRPC **server**: `discount.proto` + `DiscountService` + EF Core SQLite | Section 11, copied almost unchanged |
| `Basket.API` | gRPC **client**: stores carts and deducts the discount for each item | Section 12, simplified: in-memory store instead of Marten/PostgreSQL/Redis, Minimal API instead of Carter/MediatR |

## Requirements

.NET 8 SDK only. No Docker, no database server.

## Run

Open two terminals in this folder:

```bash
# terminal 1: gRPC server (creates and seeds the SQLite file on first run)
dotnet run --project Discount.Grpc

# terminal 2: client service
dotnet run --project Basket.API
```

Then send the requests in `demo.http` (VS Code "REST Client" extension, or Visual Studio), or with curl:

```bash
curl -X POST http://localhost:5001/basket -H "Content-Type: application/json" \
  -d '{"userName":"swn","items":[{"productId":"5334c996-8457-4cf0-815c-ed2b77c4ff61","productName":"IPhone X","price":950,"quantity":1,"color":"Black"}]}'
# → price 800 (950 − 150 coupon)
```

What to show in the logs:
- Discount.Grpc: `Discount is retrieved for ProductName : IPhone X, Amount : 150`
- Basket.API: `gRPC GetDiscount(IPhone X) returned amount 150`

Seeded coupons: **IPhone X −150**, **Samsung 10 −100**. Any other product gets "No Discount" (amount 0).

To reset the data, stop Discount.Grpc and delete `Discount.Grpc/discountdb*`.

## Where each concept lives

| Concept | File | Course lecture |
|---|---|---|
| Protobuf contract (`service`, `message`, field numbers) | `Discount.Grpc/Protos/discount.proto` | 152 |
| Generate server code from the `.proto` | `Discount.Grpc/Discount.Grpc.csproj` (`GrpcServices="Server"`) | 153 |
| Implement the service (override generated base class) | `Discount.Grpc/Services/DiscountService.cs` | 154, 160–163 |
| Register gRPC in the pipeline | `Discount.Grpc/Program.cs` (`AddGrpc`, `MapGrpcService`) | 155 |
| HTTP/2 without TLS | `Discount.Grpc/appsettings.json` (`Kestrel:EndpointDefaults:Protocols = Http2`) | 155 |
| Share the contract, generate the client only | `Basket.API/Basket.API.csproj` (`GrpcServices="Client"`, linked file) | 168–169 |
| Register the typed gRPC client | `Basket.API/Program.cs` (`AddGrpcClient`) | 170 |
| Call it like a local method | `Basket.API/Program.cs` (`GetDiscountAsync`) | 168 |

## Differences from the course code

- No Docker: the client talks to `http://localhost:5002` over plain HTTP/2, so no dev certificate and no `DangerousAcceptAnyServerCertificateValidator` handler are needed.
- `Basket.API` keeps carts in memory (lost on restart) and has no MediatR, Carter, Marten, Redis or validation, so the gRPC call is the only thing to look at.
- An extra `POST /discount` endpoint calls the gRPC `CreateDiscount` method, so you can add a coupon live during the demo.
- `UseMigration` calls `Migrate()` instead of an un-awaited `MigrateAsync()`, so the database is ready before the first request.
