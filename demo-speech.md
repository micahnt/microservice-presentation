# Speaker Script: Tactical DDD and gRPC Demo (about 11 minutes)

Companion to `demo-slides.html`. Each section matches one slide. The number in brackets is the target **clock time when you leave that slide**. Lines in *italics* are stage directions, not things to say.

---

## Before the session (15 minutes before)

*No Docker needed: the demo runs with the .NET 8 SDK only.*

1. Build once, so nothing compiles on stage: in `grpc-demo`, run `dotnet build`.
2. Reset the data: delete `grpc-demo/Discount.Grpc/discountdb*`, so a Huawei coupon from a rehearsal is gone. The file is recreated and seeded when the server starts.
3. Start the two services in two terminals, side by side so both logs are visible:
   `dotnet run --project Discount.Grpc`, then `dotnet run --project Basket.API`
4. Warm up: send request 1 from `demo.http` once, so the first gRPC call on stage isn't slow. Sending it again simply overwrites the basket. Do **not** send request 3; it creates the coupon.
5. Open `grpc-demo/demo.http` in VS Code (REST Client extension) or Visual Studio. Increase the terminal font size so the log lines are readable.
6. **Backup plan:** take screenshots of each demo step while it works. If something fails on stage, show the screenshots and keep going.

---

## Slide 1: Title  `[0:30]`

This session goes one level deeper than the main talk. First, how to model the inside of one service with tactical DDD, using the Ordering service. Then a live demo of the gRPC call between Basket and Discount.

## Slide 2: Entities, value objects, aggregates  `[1:45]`

Now tactical DDD: how to model the inside of a service. Three building blocks.

An **entity** is identified by its **ID**. Two orders with exactly the same data are still two different orders, because they have different IDs. Entities have a **lifecycle**: an order is created, paid, shipped.

A **value object** has no ID. It's defined by its **values**. Two addresses with the same street, city and zip code are the same address. And value objects are **immutable**: you don't edit an address, you replace it with a new one.

In the Ordering service, `Order`, `OrderItem`, `Customer` and `Product` are entities. `Address`, `Payment`, `OrderName`, and even `OrderId` are value objects.

An **aggregate** is a cluster of entities and value objects treated as one unit of consistency. Its main entity is the **aggregate root**, and it's the only way in. Here, `Order` is the root. Nobody adds an order item directly; you call `Order.Add`, and the order makes sure its rules hold, like quantity and price being positive.

## Slide 3: Entities and value objects in code  `[2:45]`

Here's what that looks like in the Ordering code.

On the left, the `Entity` base class: an ID plus audit fields. `Order` inherits from `Aggregate`, which is an entity that can also raise domain events. Notice the order items list is **private**; outsiders get a read-only view. The only way to add an item is the `Add` method, which validates the quantity and price first. That's the aggregate root protecting its rules.

On the right, the value objects. `Address` is a C# **record**, so two addresses with the same values are automatically equal. It's created through a factory method called `Of`, which validates the input. And `OrderId` is a **strongly typed ID**: instead of passing raw GUIDs around, where it's easy to mix up an order ID and a customer ID, each ID has its own type, and an empty GUID is rejected.

---

## Slide 4: Choosing an invocation method  `[4:45]`

Let's put it all together. The table reads left to right: the method, its style, what it's best for, and where EShop uses it.

**REST over HTTP**: synchronous, with JSON. Best for public and client-facing APIs, because every client understands it. In EShop, that's the web app going through the gateway to Catalog, Basket and Ordering.

**gRPC**: also synchronous, but binary, over HTTP/2. Best for fast internal calls where both sides share a strict contract. In EShop, that's Basket asking Discount for the price of every item, which is exactly what the demo will show.

**Messaging with RabbitMQ**: asynchronous, with events. Best for workflows that cross services, when you want decoupling, or fan-out to several consumers. In EShop, that's checkout: Basket publishes, Ordering consumes.

And the **API gateway with YARP**: synchronous, a reverse proxy. It's not a way for services to talk to each other, but the single entry point for clients, where routing, rate limiting and security live. In EShop, Shopping.Web only ever calls YarpApiGateway.

Real systems mix them. The advice to take home: keep synchronous call chains **short**, and use messaging for anything that doesn't need an immediate answer.

A simple test you can use: does the caller need the answer *right now* to continue? Basket needs the discount right now to show a price, so that's a synchronous call. Basket doesn't need the order number to finish checkout, so that's an event.

Enough slides. Let's see it running.

---

## Slide 5: Demo setup  `[5:45]`

*(Both services are already running in two terminals, side by side, so both logs are visible. VS Code shows `demo.http`.)*

For the demo, I've taken just the gRPC part of the course, sections 10 to 12, and made it run on my laptop with nothing but the .NET SDK. No Docker, no database server.

There are two projects. **Discount.Grpc** is the gRPC server: the `.proto` contract, `DiscountService`, and a small SQLite file with two seeded coupons: 150 off the iPhone X and 100 off the Samsung 10. **Basket.API** is the client. I've cut it down to the essentials: it keeps carts in memory, and when you store a basket, it calls Discount over gRPC for every item.

Both projects use the same `discount.proto` file. The server project generates the server base class, the client project generates the client. One contract, two generated sides.

And one difference from the course: here the client calls `localhost:5002` over plain HTTP/2, without TLS, so we don't need the development certificate workaround that the course code uses.

## Slide 6: Demo steps  `[10:45]`

*[Send request 1 in `demo.http`: store a basket with three items.]*

I store a basket for user "swn" with three phones: an iPhone X at 950, two Samsung 10 at 840, and a Huawei Plus at 650. This is a normal REST call, JSON in and JSON out.

Look at the response: iPhone **800**, Samsung **740**, Huawei still **650**.

Now the logs. On the Discount side: "Discount is retrieved for ProductName: IPhone X, Amount: 150", then Samsung with 100, and for the Huawei, "No Discount", amount zero. On the Basket side, the matching lines: "gRPC GetDiscount returned amount 150", and so on. Three items, three gRPC calls, and in the code each one looks like a normal C# method call.

*[Send request 2: get the basket.]*

Reading it back, the discounted prices were saved. The total is 2,930.

*[Send request 3: create a coupon.]*

Now let's change the data on the other side. This request goes to Basket, and Basket calls a *different* gRPC method, `CreateDiscount`, to add a 50 coupon for the Huawei. The response is the new `CouponModel`, with id 3: that's the Protobuf message, turned back into JSON for us to read.

*[Send request 4: store the Huawei basket again.]*

And if I store the Huawei basket again: **600**. Basket didn't change; the Discount service did, and Basket picked up the new price on the next call.

*Optional, if you have a minute: stop Discount.Grpc (Ctrl+C) and send request 1 again. The request fails with a server error, because the gRPC call is synchronous: Basket can't finish without Discount. Say: "That's temporal coupling, and it's exactly why checkout uses an event instead of a direct call." Then restart Discount.Grpc.*

That's gRPC in practice: one contract, generated code on both sides, and a remote call that reads like a local one. The code is in the `grpc-demo` folder, with a README that explains how to run it.

---

## Likely questions and short answers

**"Why make OrderId a type instead of just using a Guid?"**
It's called avoiding "primitive obsession". If every ID is a `Guid`, it's easy to pass a customer ID where an order ID was expected, and the compiler won't notice. With strongly typed IDs, that mistake doesn't compile, and validation, like rejecting empty IDs, lives in one place.
