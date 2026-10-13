# Speaker Script: Microservices with .NET 8 (60 minutes)

Companion to `microservices-slides.md`. Each section below matches one slide. The number in brackets is the target **clock time when you leave that slide**, so you can check your pace as you go.

Speaking pace assumed: about 120 words per minute, with short pauses on code and diagram slides. The spoken text is about 5,250 words (about 44 minutes); the demo and pauses fill the rest. Lines in *italics* are stage directions, not things to say.

---

## Timing plan

| Part | Slides | Duration | Clock at end |
|---|---|---|---|
| Opening | 1–2 | 2:00 | 2:00 |
| Part 1: Introduction to Microservices & DDD | 3–15 | 16:00 | 18:00 |
| Part 2: Building a microservice (short) | 16–19 | 5:00 | 23:00 |
| Part 3: Synchronous communication (REST & gRPC) | 20–26 | 10:00 | 33:00 |
| Part 4: Asynchronous communication & message brokers | 27–33 | 10:00 | 43:00 |
| Part 5: API Gateway & BFF | 34–39 | 7:00 | 50:00 |
| Demo | 40–41 | 6:00 | 56:00 |
| Summary and Q&A | 42–44 | 4:00 | 60:00 |

**If you are running late**, these slides can be shortened to one sentence without losing the story:
- Slide 7 (benefits and challenges): "The benefits are speed and independent scaling; the cost is complexity, network calls and harder consistency."
- Slide 8 (when to use): "Use them for a real reason, never without DevOps, and never with a shared database."
- Slide 19 (containerize): "Each service is packaged in a Docker image, and docker-compose runs them all together."
- Slide 32 (messaging setup): "One shared extension method configures MassTransit for every service."
- Slide 39 (choosing a method): it repeats Parts 3–5.

**If you are more than 2 minutes late at the demo**, send only request 1 and point at the two logs; skip requests 2–4.

---

## Before the talk (15 minutes before)

*No Docker needed: the demo runs with the .NET 8 SDK only.*

1. Build once, so nothing compiles on stage: in `grpc-demo`, run `dotnet build`.
2. Reset the data: delete `grpc-demo/Discount.Grpc/discountdb*`, so a Huawei coupon from a rehearsal is gone. The file is recreated and seeded when the server starts.
3. Start the two services in two terminals, side by side so both logs are visible:
   `dotnet run --project Discount.Grpc`, then `dotnet run --project Basket.API`
4. Warm up: send request 1 from `demo.http` once, so the first gRPC call on stage isn't slow. Sending it again simply overwrites the basket. Do **not** send request 3; it creates the coupon.
5. Open `grpc-demo/demo.http` in VS Code (REST Client extension) or Visual Studio. Increase the terminal font size so the log lines are readable.
6. **Backup plan:** take screenshots of each demo step while it works. If something fails on stage, show the screenshots and keep going.

---

## Slide 1: Title  `[1:00]`

Good morning, everyone, and thanks for being here.

Today's topic is microservices with .NET 8. In the next sixty minutes we'll cover four things. First: what microservices are, and how Domain-Driven Design helps us decide where to split a system. Second: how services talk to each other synchronously, with REST and gRPC. Third: how they talk asynchronously, with events and a message broker. And fourth: the API gateway that sits in front of them all.

Everything I show comes from one real project, called EShop Microservices. It's an e-commerce system with a Catalog, a Basket, a Discount service, an Ordering service, an API gateway, and a web front end. Every code sample on these slides comes from that repository, some trimmed to fit on a slide, and near the end I'll run it live.

## Slide 2: Agenda  `[2:15]`

Here is the plan.

We start with about sixteen minutes on microservices and Domain-Driven Design: monolith versus microservices, design principles, and the DDD ideas of bounded contexts, entities and value objects.

Then a short five minutes on what it takes to build one microservice.

Then ten minutes each on synchronous and asynchronous communication, and seven minutes on API gateways and the Backends for Frontends pattern.

Then a short live demo of the gRPC call between Basket and Discount, running on my laptop without Docker. And we'll finish with a summary and your questions. If something is unclear along the way, feel free to raise your hand.

---

## Slide 3: Part 1 divider  `[2:30]`

Let's start with the big picture: what are microservices, and how do we design them?

## Slide 4: Architecture design journey  `[3:30]`

Microservices are one step in a journey of four architecture styles.

On the left is the classic **monolith**: one application with one database. Next is the **modular monolith**. It's still one deployment, but split into a few big parts, which you can think of as **macroservices**. Then come **microservices**: many small services, and each one owns its own database. And on the far right is **serverless**, where the units become individual functions, sometimes called **nanoservices**.

Moving to the right, the pieces get smaller and the number of databases grows. It's a spectrum, not a ladder where the right side is always better.

So which one should you choose? **It depends on your project.** Many systems use a **mixed architecture** and plan **transitions** as they grow. Martin Fowler and Sam Newman both recommend starting with a **monolith first**, and extracting services one at a time when you have a real need.

## Slide 5: What are microservices?  `[4:45]`

So, what exactly is a microservice? Six ideas.

They are **small, independent, and loosely coupled**. Each service has **its own codebase**, owned by a **small team**. They communicate only through **well-defined APIs**: HTTP, gRPC, or messages. Each one is **deployed independently**: I can release a new Basket service without touching Ordering. They are **technology agnostic**. And each service has **its own database**, not shared with anyone else.

The quote is from Martin Fowler's original article: "a suite of small services, each running in its own process and communicating with lightweight mechanisms." If you remember one sentence from today, make it that one.

## Slide 6: Monolith vs. microservices  `[7:00]`

Let's compare the two directly, row by row.

**Structure**: a monolith is one undivided unit, one application and one database. Microservices are many different services, each with its own database, and possibly its own technology.

**Scalability**: a monolith scales as one unit, so to handle more traffic you copy the whole application. Microservices scale each service separately. On Black Friday, I can scale just Basket and Ordering.

**Deployment**: a monolith is simple to deploy, but it's all at once: a one-line change means redeploying everything. Microservices deploy independently, with zero downtime, through their own CI/CD pipelines. The drawing at the bottom shows this. In a monolith, three teams commit to a **single code base** with **one pipeline**, so every release must be coordinated. With microservices, each team has its **own code, its own pipeline, and its own service**, and ships when it's ready.

**Data**: one shared database, versus one database per service.

**Failure**: one bug, like a memory leak, can take down the whole monolith. In microservices, faults are isolated to one service.

And **team fit**: a monolith suits small or new teams. Microservices need an experienced team that's comfortable with containers, DevOps and monitoring. Without those skills, the extra complexity hurts more than it helps.

Neither one is "better". A monolith is a perfectly good choice for many systems.

And many successful microservice systems started life as a monolith. The monolith taught the team where the real boundaries were, and *then* they split it.

## Slide 7: Benefits and challenges  `[8:15]`

Here are both sides, honestly.

The **benefits**: agility, because small teams ship without coordinating a huge release; **independent scaling**; small codebases; independent deployment; the right tool for each job; and **fault isolation**: if Discount crashes, customers can still browse products.

The **challenges** are real. The whole system is more **complex**. Every call is now a **network call** that can be slow or fail. End-to-end testing is harder. **Data consistency** is harder, because there's no single database transaction across services. And debugging across many services is hard.

So microservices don't remove complexity. They **move** it, from inside the code to between the services.

## Slide 8: When to use, and when not to  `[9:15]`

Use them when you need **zero-downtime independent deployment**, **independent scaling**, different data stores, or several autonomous teams. And most importantly, when you have a *really good reason*.

Avoid them when you'd build a **distributed monolith**: services so tightly coupled that you still deploy them together. That gives you all the complexity and none of the benefits. Also avoid them without a **DevOps foundation**, with a **small team**, for a **brand-new startup** whose domain keeps changing, and never let services **share a database**.

## Slide 9: Design principles  `[10:45]`

If we do choose microservices, these six principles keep us out of trouble.

**One: single responsibility.** One service per **business capability**, like Catalog or Ordering, not per technical layer like "the database service".

**Two: loose coupling, high cohesion.** I can change one service without changing the others, and everything about one capability lives together.

**Three: autonomy.** Each service deploys on its own and **owns its data**.

**Four: well-defined contracts.** Services talk only through APIs and events. Fowler calls this "smart endpoints and dumb pipes": the logic lives in the services, and the network just moves data.

**Five: design for failure.** In a distributed system, something is always broken somewhere. We use timeouts, retries and circuit breakers so one failing service doesn't take down the rest.

**Six: automate and observe.** CI/CD, containers, health checks and centralized logging aren't optional extras; without them, many services become unmanageable.

## Slide 10: Introduction to DDD  `[12:15]`

Now, the hardest question in microservices is: **where do I draw the lines between services?** That's where Domain-Driven Design comes in.

DDD was introduced by **Eric Evans** in his 2003 book. It isn't a technology or a framework. It's a way of **modeling software around the business**. The core idea is simple: break a complex problem into smaller parts, and give each part its own model.

DDD has two halves. **Strategic DDD** is the big picture: understand the business domain, find its subdomains and bounded contexts, and map how they relate. That tells us **where to split** into microservices. **Tactical DDD** works inside one service: patterns like entities, value objects and aggregates. That tells us **how to build** each service's model.

We'll do both: strategic first, then tactical.

## Slide 11: DDD core concepts  `[13:30]`

Five words you'll hear in every DDD conversation.

The **domain** is the business problem the software solves: for us, e-commerce. A **subdomain** is one area of expertise inside it: the product catalog, the shopping cart, discounts, orders.

The **ubiquitous language** is a shared vocabulary used by domain experts *and* in the code. If the business says "order item" and "shipping address", the code says `OrderItem` and `ShippingAddress`, not "row" or "data object". That removes a whole class of misunderstandings.

A **bounded context** is a boundary inside which one model and one language apply. In EShop: Catalog, Basket, Discount, and Ordering.

And the **context map** shows how the contexts relate. For us: Basket asks Discount over gRPC, and Basket informs Ordering with an event.

## Slide 12: Bounded contexts → microservices  `[14:45]`

How do we find the bounded contexts in practice? The course uses a simple method. Start from **user stories**. Find the **nouns**, which become entities, and the **verbs**, which become use cases. Then group them.

"List and filter products" gives us **Catalog**. "Put products in the cart" gives us **Basket**. "Apply coupons" gives us **Discount**. "Check out and create an order" gives us **Ordering**.

Now look at the first bullet, because it's the heart of the bounded context idea: **the same word can mean different things in different contexts.** In Catalog, a `Product` has a name, description, image, categories and price, because Catalog's job is to show products. In Ordering, a `Product` has only an ID, a name and a price, because that's all an order needs. Two models, each correct inside its own boundary. Trying to force one shared "Product" model on everyone is exactly how monoliths get tangled.

A bounded context is usually one microservice, but you can split one further if a part needs to scale differently.

## Slide 13: Entities, value objects, aggregates  `[16:00]`

Now tactical DDD: how to model the inside of a service. Three building blocks.

An **entity** is identified by its **ID**. Two orders with exactly the same data are still two different orders, because they have different IDs. Entities have a **lifecycle**: an order is created, paid, shipped.

A **value object** has no ID. It's defined by its **values**. Two addresses with the same street, city and zip code are the same address. And value objects are **immutable**: you don't edit an address, you replace it with a new one.

In the Ordering service, `Order`, `OrderItem`, `Customer` and `Product` are entities. `Address`, `Payment`, `OrderName`, and even `OrderId` are value objects.

An **aggregate** is a cluster of entities and value objects treated as one unit of consistency. Its main entity is the **aggregate root**, and it's the only way in. Here, `Order` is the root. Nobody adds an order item directly; you call `Order.Add`, and the order makes sure its rules hold, like quantity and price being positive.

## Slide 14: Entities and value objects in code  `[17:00]`

Here's what that looks like in the Ordering code.

On the left, the `Entity` base class: an ID plus audit fields. `Order` inherits from `Aggregate`, which is an entity that can also raise domain events. Notice the order items list is **private**; outsiders get a read-only view. The only way to add an item is the `Add` method, which validates the quantity and price first. That's the aggregate root protecting its rules.

On the right, the value objects. `Address` is a C# **record**, so two addresses with the same values are automatically equal. It's created through a factory method called `Of`, which validates the input. And `OrderId` is a **strongly typed ID**: instead of passing raw GUIDs around, where it's easy to mix up an order ID and a customer ID, each ID has its own type, and an empty GUID is rejected.

## Slide 15: The big picture  `[18:00]`

And here's how the whole system fits together. Read it left to right.

This is the reference system for the rest of the talk, so it's worth a few seconds.

**Shopping.Web**, the client, only talks to the **YARP API gateway**. The gateway routes to **Catalog**, **Basket** and **Ordering**, each with its own database. Catalog and Basket use Vertical Slice architecture; Ordering uses Clean Architecture with the DDD model we just saw.

And notice the two special connections. **Basket calls Discount over gRPC**: synchronous. And **Basket publishes an event to RabbitMQ**, which **Ordering consumes**: asynchronous. Those two arrows are Parts 3 and 4 of this talk. And the gateway in front of it all is Part 5. So everything we discuss from now on is somewhere on this one diagram.

*If someone compares this with the course's "Big Picture" slide: that picture also draws a gateway arrow to Discount. In the actual code, the YARP config only routes to Catalog, Basket, and Ordering.*

---

## Slide 16: Part 2 divider  `[18:15]`

Before we connect services, let's look quickly at what one service looks like.

## Slide 17: From analysis to a running service  `[19:45]`

Every service in this project follows the same five steps. **Domain analysis**: models, use cases, endpoints and data store. **Technical analysis**: architecture style, patterns and libraries. Then **develop** with ASP.NET Core Minimal APIs, **containerize** with a Dockerfile, and **orchestrate** with docker-compose.

And every `Program.cs` has the same five-step shape: create the builder, register services in dependency injection, build, configure the pipeline, and run.

On the right is the smallest possible microservice, the TodoApi from section 4. It registers an in-memory database, maps a GET and a POST endpoint, and runs. That's about fifteen lines, and it already meets the definition: its own process, one responsibility, its own data, and an HTTP API.

## Slide 18: Catalog.API  `[21:30]`

A real service needs more structure. Catalog uses **Vertical Slice Architecture**: one folder per feature, like CreateProduct or GetProductById, each holding its endpoint and its handler. To change one feature, you open one folder.

On the right is the request flow: **HTTP, endpoint, MediatR, handler, database.** The endpoint only translates HTTP into a query and sends it through MediatR. The handler loads the product with Marten, and if it's missing, throws an exception that the global handler turns into a 404. The endpoint knows nothing about the database, and the handler knows nothing about HTTP.

Two patterns are at work here. **CQRS**, Command Query Responsibility Segregation, means reads and writes are handled separately: `GetProductByIdQuery` is a query, it only reads; `CreateProductCommand` is a command, it changes data. And **MediatR** is an in-process mediator: the endpoint sends a message, and MediatR finds the one handler for it. A nice side effect is that MediatR can wrap every request in **pipeline behaviors**. EShop uses two: one validates every command with FluentValidation, and one logs every request. So no handler has to repeat that code.

## Slide 19: Containerize and orchestrate  `[23:00]`

Finally, we package it. The Dockerfile has several stages: build with the full .NET SDK image, then copy only the published output onto the small runtime image. The final image is smaller and contains no source code.

On the right, docker-compose runs the service next to its database, with environment variables, a startup dependency, and a port mapping. Inside Docker, services find each other **by name**: the gateway calls `http://catalog.api:8080`. EShop runs eleven containers this way.

The port mapping `6000:8080` means: port 8080 inside the container is reachable as 6000 on my laptop. Every service listens on 8080 inside its container, and gets its own 6000-range port outside. The eleven containers are five backing services, two PostgreSQL databases, Redis, SQL Server and RabbitMQ, plus six applications. One command starts them all.

Now we have services. Let's connect them.

---

## Slide 20: Part 3 divider  `[23:15]`

Part three: synchronous communication.

## Slide 21: Service-to-service communication  `[24:45]`

Services must cooperate in two ways: **ask for data**, like "what's the discount on this product?", or **trigger work**, like "create this order".

There are two styles. **Synchronous**: the client sends a request and **waits**. HTTP with REST, gRPC, GraphQL. The catch is **temporal coupling**: both services must be up at the same moment.

**Asynchronous**: the client sends a message and **does not wait**. The message goes to a broker, using a protocol like AMQP. If the receiver is down, the broker keeps the message.

EShop uses both. Basket asks Discount **synchronously with gRPC**, because it needs the price right now. The web app calls the APIs **with REST**. And Basket tells Ordering about a checkout **asynchronously**.

The rule of thumb: **REST for public APIs, gRPC for internal calls, messaging for workflows that span services.**

## Slide 22: REST over HTTP  `[25:45]`

REST is the most familiar: resource URLs like `GET /products` and `POST /basket`, with JSON that every client understands.

Calling REST with raw `HttpClient` means repetitive code: build the URL, serialize, send, deserialize. Shopping.Web avoids that with **Refit**. You write an **interface**, put the route on each method as an attribute, and Refit generates the implementation. Register it with `AddRefitClient`, point it at the gateway, and in a Razor page you just call `GetProduct(id)` like a normal C# method.

## Slide 23: Introduction to Protobuf and gRPC  `[27:30]`

Now gRPC, a remote procedure call framework from Google. It runs on **HTTP/2**, and the client calls a remote method **as if it were a local object**.

The contract is written in **Protocol Buffers**, in a `.proto` file. On the right is the real contract for Discount. A `service` block lists the methods: GetDiscount, Create, Update and Delete. `message` blocks define the data: a `GetDiscountRequest` with a product name, and a `CouponModel` with ID, name, description and amount.

Two things people often miss. First, the numbers, like `= 1` and `= 2`, are **field numbers** used in the binary encoding, not default values. Never reuse or change them once in use. Second, Protobuf is **binary**: much smaller and faster to parse than JSON.

And in .NET, you don't write the networking code. The build **generates** a server base class and a client class from the `.proto` file.

## Slide 24: gRPC server  `[28:45]`

Here's the server. In `Program.cs`: `AddGrpc`, the database, and `MapGrpcService`.

`DiscountService` **inherits from `DiscountProtoServiceBase`**, the class generated from the `.proto` file. We just override `GetDiscount`: find the coupon by product name, or return a "No Discount" coupon with amount zero. Notice there's no routing, no JSON and no status codes; the contract defines all of that.

Errors work differently too. Instead of returning an HTTP 404, the service throws an `RpcException` with a gRPC status code. For example, `DeleteDiscount` throws `StatusCode.NotFound` if the coupon doesn't exist, and `Create` and `Update` throw `InvalidArgument` for a bad request. The client receives those as typed exceptions. The other three methods, Create, Update and Delete, follow the same pattern: map the Protobuf message to the database model, save, and map back.

## Slide 25: gRPC client  `[30:30]`

And the client, in Basket. Three steps.

**One**: the Basket project includes the *same* `.proto` file, with `GrpcServices="Client"`, so only the client is generated. The contract is shared; the code is not.

In fact, the Basket project doesn't even copy the file. It links to Discount's `.proto` file directly, so if the contract changes, both sides see the change on the next build.

**Two**: register a typed client with `AddGrpcClient` and read Discount's address from configuration. The extra handler accepts the self-signed development certificate. That's fine on a laptop, but never ship it to production.

**Three**: inject the client and call it. For each cart item, `GetDiscountAsync` with the product name, and subtract the coupon amount. It reads like a local method call, but it's a binary HTTP/2 request to another container.

## Slide 26: When to use REST, when to use gRPC  `[33:00]`

So when do you pick which? Let's go row by row.

Both can run on HTTP/2, but gRPC requires it. REST sends **text JSON**; gRPC sends **binary Protobuf**, smaller and faster. For REST a contract is optional, for example OpenAPI; for gRPC it's **required**, and code is generated from it. **Browsers** speak REST natively, but gRPC needs gRPC-Web or a proxy. gRPC supports **streaming** in both directions; REST is request and response. And REST is easy to test by hand with curl or a browser; gRPC needs tooling.

So: **choose REST** for public APIs, browsers, third parties and simple CRUD. **Choose gRPC** for internal, high-volume, low-latency calls between your own services.

That's exactly EShop's split: REST behind the gateway for the web app, and gRPC for Basket to Discount, which is called once for every item in every cart.

Think about what that means in numbers. A cart with ten items means ten Discount calls every time the basket is saved, for every user. With gRPC, those calls share one HTTP/2 connection and send a few bytes of binary each, instead of opening requests and parsing JSON. That's where gRPC pays off.

And it's not an either-or choice for the whole system. Most real systems do exactly what EShop does: REST at the edge, where browsers and partners connect, and gRPC inside, between services you own. ASP.NET Core can even expose one gRPC service as REST as well, with a feature called JSON transcoding, if you need both.

---

## Slide 27: Part 4 divider  `[33:15]`

Part four: asynchronous communication, and why it matters.

## Slide 28: Basic event-driven architecture  `[35:00]`

Start with the problem on the left. Imagine creating an order using only synchronous calls. The gateway calls ShoppingCart. ShoppingCart calls Product and Pricing, then Order. Order calls Billing and Shipping. Six calls for one click. Every service must be up, latencies add up, and if Shipping is slow, everyone is slow.

Event-driven architecture flips this around. An **event** is a **fact that already happened**, named in the past tense: *BasketCheckedOut*, *OrderCreated*. The **producer** publishes it and moves on. It **doesn't know** who's listening. **Consumers** react in their own time.

And a useful distinction: a **command** tells one specific service what to do, like "CreateOrder". An **event** just announces what happened, to anyone interested. Commands couple you to a receiver; events don't.

An everyday analogy: a command is a phone call to one person, asking them to do something. An event is an announcement on a notice board: "the basket was checked out". Whoever cares reads it and acts. The person who posted it doesn't need to know who read it, or even be around when they do.

In EShop, checkout is the event. Basket doesn't call Ordering and say "create an order". It announces "this basket was checked out", and Ordering decides what that means for it.

## Slide 29: Domain vs. integration events  `[36:30]`

In DDD systems, there are two kinds of events.

A **domain event** stays **inside** one service. It's raised by an aggregate and delivered in-process, in memory. In EShop, when `Order.Create` runs, the `Order` aggregate raises an `OrderCreatedEvent`, and MediatR delivers it to a handler in the same service.

An **integration event** crosses **between** services, through a message broker. In EShop, that's `BasketCheckoutEvent`, from Basket to Ordering, through RabbitMQ.

The diagram shows how they chain together. The integration event arrives in Ordering, the handler creates an order, the order raises a domain event, and that domain event's handler could publish a new integration event, `OrderCreatedIntegrationEvent`, for other services like shipping. In this project, that last step is behind a feature flag and switched off.

## Slide 30: The role of a message broker  `[38:15]`

So what does the broker in the middle actually do? It **receives** messages, **stores** them until consumers are ready, even if a consumer is down, **routes** them to the right queues, and **delivers and retries**. It decouples services in **time**, because they don't need to be up together, and in **location**, because the producer doesn't need the consumer's address.

**RabbitMQ** is an open-source broker that implements the AMQP protocol. The key concept: producers don't send directly to queues. They send to an **exchange**, and the exchange routes to queues based on its type: **direct** by exact key, **fanout** to everyone, **topic** by pattern, or **headers**.

On top of RabbitMQ, EShop uses **MassTransit**, a .NET library that creates the exchanges and queues and handles serialization and retries. So our code thinks in terms of events, not exchanges.

RabbitMQ also comes with a management web UI, on port 15672 in our setup. It shows every exchange, queue and message rate, and it's the first place to look when a message seems to be lost.

## Slide 31: Basic pub/sub pattern  `[39:45]`

The most common way to use a broker is **publish/subscribe**.

Basket publishes **one** `BasketCheckoutEvent` to an exchange. The broker **fans out** a copy to every subscriber's queue. Today in EShop there's one subscriber, Ordering. But tomorrow we could add a Notification service to send an email, and an Analytics service to record the sale, **without changing a single line in Basket**. That's the real power of pub/sub.

Compare it with a plain queue: a queue gives each message to **one** consumer; pub/sub gives **every** subscriber its own copy.

On the right, the costs. Brokers usually deliver **at least once**, so a message can arrive twice, and consumers must be **idempotent**: processing it twice must be safe. Order isn't guaranteed. The broker becomes critical, so it must be highly available. And there's **eventual consistency**: the order appears a moment after checkout, not instantly.

## Slide 32: Messaging setup  `[41:00]`

Here's how EShop sets this up. On the left is the **contract**: an `IntegrationEvent` base record, and `BasketCheckoutEvent` with user name, total price, address and payment. Both Basket and Ordering reference this same class.

On the right is one shared extension method, `AddMessageBroker`. It configures MassTransit, optionally registers consumers from an assembly, and connects to RabbitMQ with settings from configuration. Every service calls this one method.

Two details. `ConfigureEndpoints` tells MassTransit to create a queue for every consumer it found, and bind it to the right exchange, so we never declare queues by hand. And the broker address comes from configuration: locally `amqp://localhost:5672`, and inside Docker `amqp://ecommerce-mq:5672`, set by an environment variable in docker-compose. Same code, different environment.

## Slide 33: Publish and consume  `[43:00]`

And both ends of the checkout.

On the left, the **publisher** in Basket. The checkout handler gets an `IPublishEndpoint` from MassTransit. It loads the basket, maps the checkout data into a `BasketCheckoutEvent`, sets the total price, and calls **`Publish`**. Then it deletes the basket and returns success, **immediately**. It doesn't wait for an order.

On the right, the **consumer** in Ordering. It implements **`IConsumer<BasketCheckoutEvent>`**. When a message arrives, `Consume` logs it, maps it into a `CreateOrderCommand`, and sends it through MediatR, which creates the order.

Notice: **Basket has no idea Ordering exists.** It only publishes an event. That's loose coupling in practice.

One question worth asking about this handler: what if `Publish` succeeds, but deleting the basket fails? Or the other way round? Writing to the database and publishing a message are two separate operations, and either can fail. That's called the **dual write problem**. The standard fix is the **Outbox pattern**: save the event in the same database transaction as the data, and publish it afterwards. The course covers it later; for today, just know the problem exists.

---

## Slide 34: Part 5 divider  `[43:15]`

Last topic: the API gateway, and the Backends for Frontends pattern.

## Slide 35: The role of an API Gateway  `[44:45]`

Without a gateway, every client must know every service's address, and every service must handle authentication, SSL and rate limiting on its own.

A gateway is a **single entry point** between clients and services, and it does three jobs. **Gateway routing**: send each request to the right internal service. If we move or split a service, only the gateway config changes, not the clients. **Gateway aggregation**: combine several service calls into one response, so a mobile app makes one call instead of five. And **gateway offloading**: handle cross-cutting concerns in one place.

On the right is the pipeline a request passes through: validation, IP allowlisting, authentication, rate limiting, service discovery, and protocol conversion, before it reaches a service.

The risk: one gateway for everything can become a **bottleneck** and a **single point of failure**. Which leads to the next pattern.

## Slide 36: Backends for Frontends  `[46:00]`

**Backends for Frontends**, or BFF, means **one gateway per type of client**, instead of one gateway for everyone.

Why? Different clients need different things. A phone screen needs less data than a desktop page, in a different shape. If one gateway serves everyone, it fills up with client-specific logic and becomes a bottleneck that every team has to change. With BFF, the web team owns the web gateway, the mobile team owns the mobile gateway, and each returns exactly what its frontend needs.

EShop has only one client, Shopping.Web, so one gateway is enough. You'd add BFFs when, say, a mobile app arrives.

## Slide 37: Introduction to YARP  `[47:30]`

EShop implements its gateway with **YARP**, which stands for "Yet Another Reverse Proxy". It's a reverse-proxy library from Microsoft, built on ASP.NET Core. It supports HTTP/2, gRPC and WebSockets, load balancing and health checks, and it plugs into normal ASP.NET Core middleware, like the rate limiter.

The code on the left is the *entire* EShop gateway: add the reverse proxy, load its config, add a rate limiter allowing 5 requests per 10 seconds, and map the proxy.

On the right is the config. A **route** matches a path, like `/catalog-service/` plus anything, and points to a **cluster**. The **transform** removes the prefix. The cluster says where the service lives: `catalog.api:8080`. So `/catalog-service/products` becomes `catalog.api:8080/products`. And the ordering route also applies the rate limiting policy.

## Slide 38: Ocelot, and YARP vs. Ocelot  `[49:00]`

The other well-known .NET option is **Ocelot**, an open-source API gateway library configured with an `ocelot.json` file. EShop doesn't use it, so at the bottom of the slide I've written the same Catalog route for Ocelot, just for comparison. An **upstream** path is what the client calls; a **downstream** path and host are where Ocelot forwards it. The startup code is just `AddOcelot` and `UseOcelot`.

How do they compare? **YARP** comes from Microsoft and is a reverse-proxy **toolkit**: very fast, very customizable in C#, and it gets extra features from standard ASP.NET Core middleware. **Ocelot** is a community project and a ready-made **API gateway**: features like rate limiting, authentication, caching and request aggregation are switched on in its own config file.

Both implement the gateway routing pattern. YARP is the one this course uses.

## Slide 39: Choosing an invocation method  `[50:00]`

Let's put it all together. **REST** for client-facing APIs. **gRPC** for fast internal calls with strict contracts. **Messaging** for cross-service workflows and decoupling. And the **API gateway** as the single front door.

Real systems mix them. The advice to take home: keep synchronous call chains **short**, and use messaging for anything that doesn't need an immediate answer.

A simple test you can use: does the caller need the answer *right now* to continue? Basket needs the discount right now to show a price, so that's a synchronous call. Basket doesn't need the order number to finish checkout, so that's an event.

Enough slides. Let's see it running.

---

## Slide 40: Demo setup  `[51:00]`

*(Both services are already running in two terminals, side by side, so both logs are visible. VS Code shows `demo.http`.)*

For the demo, I've taken just the gRPC part of the course, sections 10 to 12, and made it run on my laptop with nothing but the .NET SDK. No Docker, no database server.

There are two projects. **Discount.Grpc** is the gRPC server, exactly the service we saw on the slides: the `.proto` contract, `DiscountService`, and a small SQLite file with two seeded coupons: 150 off the iPhone X and 100 off the Samsung 10. **Basket.API** is the client. I've cut it down to the essentials: it keeps carts in memory, and when you store a basket, it calls Discount over gRPC for every item.

Both projects use the same `discount.proto` file. The server project generates the server base class, the client project generates the client. One contract, two generated sides.

And one difference from the course: here the client calls `localhost:5002` over plain HTTP/2, without TLS, so we don't need the development certificate workaround from the earlier slide.

## Slide 41: Demo steps  `[56:00]`

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

*Optional, if you have a minute: stop Discount.Grpc (Ctrl+C) and send request 1 again. The request fails with a server error, because the gRPC call is synchronous: Basket can't finish without Discount. Say: "That's temporal coupling, from Part 3, and it's exactly why checkout uses an event instead of a direct call." Then restart Discount.Grpc.*

That's gRPC in practice: one contract, generated code on both sides, and a remote call that reads like a local one.

---

## Slide 42: Summary  `[57:15]`

Let's wrap up.

**Microservices** are small, autonomous services built around business capabilities, each with its own data and deployment. Adopt them only for a real reason.

**DDD** tells us **where to split**, using bounded contexts and a ubiquitous language, and **how to model** each service, with entities, value objects and aggregates.

For **synchronous** communication, **REST** for client-facing APIs and **gRPC with Protobuf** for fast internal calls. For **asynchronous** communication, **events** through a **message broker** with pub/sub, for decoupled workflows. And an **API gateway**, YARP or Ocelot, as the single front door, with **BFF** when you have several kinds of clients.

And in the demo we saw the synchronous part live: Basket calling Discount over gRPC, with one shared contract.

## Slide 43: References  `[57:30]`

The course, the slides and all the code are listed here, from section 4 up to section 20, the complete system. For DDD, Eric Evans' book is the original, and Microsoft's free e-book on .NET microservices architecture is the best next step. And the gRPC demo I just ran is in the `grpc-demo` folder, with a README that explains how to run it.

## Slide 44: Questions  `[60:00]`

That's all from me. Thank you! I'm happy to take your questions.

---

## Likely questions and short answers

**"Is a bounded context always one microservice?"**
Usually, but not always. A bounded context is a logical boundary in the model; a microservice is a deployment boundary. One bounded context can be split into several services if parts need to scale differently. The reverse, one service spanning several bounded contexts, is a warning sign.

**"Why make OrderId a type instead of just using a Guid?"**
It's called avoiding "primitive obsession". If every ID is a `Guid`, it's easy to pass a customer ID where an order ID was expected, and the compiler won't notice. With strongly typed IDs, that mistake doesn't compile, and validation, like rejecting empty IDs, lives in one place.

**"How do you keep data consistent without a shared database transaction?"**
With eventual consistency and events. For stronger guarantees, there are two patterns. The **Outbox pattern** saves the event in the same database transaction as the data, and a background process publishes it later, which avoids the "dual write" problem. The **Saga pattern** coordinates multi-step business transactions, with compensating actions when a step fails. Both are covered later in the course.

**"Why gRPC for Discount but REST for everything else?"**
Discount is only called by other services, once per cart item, so speed and a strict contract matter. The other services are called by a browser-based app through the gateway, and browsers work best with REST and JSON.

**"What happens if RabbitMQ is down during checkout?"**
In this code, the publish would fail or time out, and the checkout request would return an error. That's why production systems run the broker as a cluster, and why the Outbox pattern exists.

**"What if the Discount service is down?"**
Then the store-basket call fails, because the gRPC call is synchronous. In production you'd add a timeout, retries and a circuit breaker, for example with Polly, and possibly fall back to "no discount".

**"Should I pick YARP or Ocelot?"**
Both work. YARP is from Microsoft, very fast, and you build features with standard ASP.NET Core middleware. Ocelot gives you more gateway features out of the box through configuration. For a new .NET project, I'd start with YARP, as this course does.

**"How does this run in production?"**
The same container images run on Kubernetes, such as Azure AKS or Amazon EKS, which adds service discovery, scaling, health-check-based restarts and rolling deployments. docker-compose is for local development.
