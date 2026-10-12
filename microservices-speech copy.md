# Speaker Script: Microservices with .NET 8 (50 minutes)

Companion to `microservices-slides.md`. Each section below matches one slide. The number in brackets is the target **clock time when you leave that slide**, so you can check your pace as you go.

Speaking pace assumed: about 130 words per minute. Lines in *italics* are stage directions, not things to say.

---

## Timing plan

| Part | Slides | Duration | Clock at end |
|---|---|---|---|
| Opening | 1–2 | 2:00 | 2:00 |
| Part 1: Overview | 3–12 | 13:00 | 15:00 |
| Part 2: How to create a microservice | 13–21 | 11:00 | 26:00 |
| Part 3: Service invocation methods | 22–32 | 13:00 | 39:00 |
| Part 4: Demo | 33–38 | 8:00 | 47:00 |
| Summary and Q&A | 39–41 | 3:00 | 50:00 |

**If you are running late**, these slides can be shortened to a single sentence without losing the story:
- Slide 6 (characteristics): "These are Fowler's eight characteristics; the two that matter most today are decentralized data and design for failure."
- Slide 9 (monolith vs. microservices table): it repeats slides 7 and 8.
- Slide 20 (Dockerfile): "It's a standard multi-stage Dockerfile: build with the SDK image, run on the small runtime image."
- Slide 29 (messaging setup): "One shared extension method configures MassTransit for every service."

**If you are more than 3 minutes late at the demo**, skip demo ① (REST through the gateway) and go straight to ② and ③. Those show the interesting parts: gRPC and messaging.

---

## Before the talk (15 minutes before)

*Do this before people arrive. Never start Docker live on stage: pulling the SQL Server and RabbitMQ images alone can take several minutes.*

1. Start the containers from `section20`:
   `docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --build`
2. Run `docker ps` and confirm all 11 containers are up.
3. Warm up the services so the first demo call is not slow: call `http://localhost:6004/catalog-service/products` once.
4. Clean up any earlier test run: `curl -X DELETE http://localhost:6004/basket-service/basket/swn`
5. Open these, each in its own tab or window:
   - A terminal with the demo `curl` commands ready (or Postman with `EShopMicroservices.postman_collection.json` imported)
   - The RabbitMQ UI at http://localhost:15672 (login guest / guest), on the **Exchanges** tab
   - Shopping.Web at http://localhost:6005
   - A terminal ready for `docker-compose logs -f discount.grpc` (run in `section20`)
6. **Backup plan:** take screenshots of each demo step while it works. If Docker fails on stage, show the screenshots and keep going.

---

## Slide 1: Title  `[0:45]`

Good morning, everyone, and thanks for being here.

Today's topic is microservices with .NET 8. By the end of these fifty minutes, I want you to be able to answer three questions. First: what is a microservice, and when should you use one? Second: how do you actually build one in .NET? And third: once you have several services, how do they talk to each other?

Everything I show today comes from one real project, called EShop Microservices. It's an e-commerce system with a Catalog, a Basket, a Discount service, an Ordering service, an API gateway, and a web front end. So this isn't just theory. Every piece of code on these slides is in the repository, and at the end I'll run it live.

## Slide 2: Agenda  `[2:00]`

Here is the plan.

We start with about thirteen minutes of overview: the definition, the benefits, the costs, and when microservices are the wrong choice.

Then, about eleven minutes on how to create a microservice. We'll go from a tiny twenty-line service up to a real service running in a Docker container.

Then the core of the talk: service invocation methods. That means REST, gRPC, asynchronous messaging with RabbitMQ, and the API gateway.

Then about eight minutes of live demo, where a single checkout request uses all four methods.

And we'll finish with a summary and your questions. If something is unclear along the way, feel free to raise your hand. I'd rather fix confusion early than at the end.

---

## Slide 3: Part 1 divider  `[2:15]`

Let's start with the big picture: what are microservices, and why do people use them?

## Slide 4: Architecture design journey  `[3:45]`

Microservices didn't appear from nowhere. They are one step in a long journey of architecture styles.

On the left, we have the classic **monolith**: one application, one deployment, one database. Then we organized it into **layers**: UI, business logic, data access. Then into **modules** inside the same application. Then came **SOA**, service-oriented architecture, where services talked through an enterprise service bus. Then **microservices**. And at the far right, **serverless**, where each function is its own unit.

The point of this slide is that this is a **spectrum of granularity**, not a ladder where the right side is always better. You can have big "macroservices", normal microservices, or tiny "nanoservices" that are probably too small.

So which one should you choose? The honest answer is: **it depends on your project**. Martin Fowler and Sam Newman, two of the best-known authors on this topic, both recommend starting with a **monolith first**. Keep it simple, learn the domain, and then extract services one at a time when you have a real need. Keep that in mind, because it comes back when we talk about when *not* to use microservices.

## Slide 5: What are microservices?  `[5:15]`

So, what exactly is a microservice? Six ideas.

One: microservices are **small, independent, and loosely coupled**. Each one does one job.

Two: each service has **its own codebase**, and it's owned by a **small team**. The team can build, test, and deploy it without waiting for anyone else.

Three: they communicate through **well-defined APIs**, like HTTP, gRPC, or messages. Never by reaching into each other's code or database.

Four: each service is **deployed independently**. I can release a new Basket service on Tuesday without touching Ordering.

Five: they are **technology agnostic**. One service can use PostgreSQL, another SQL Server, another Redis. In theory they can even use different programming languages.

And six, the one people forget most often: each service has **its own database**, not shared with any other service.

The quote at the bottom is from Martin Fowler's original article: "a suite of small services, each running in its own process and communicating with lightweight mechanisms." If you remember one sentence from today, make it that one.

## Slide 6: Characteristics  `[6:30]`

Fowler also listed eight characteristics you usually find in a microservice system. I won't read all of them, but I want to highlight four.

**Organized around business capabilities.** We don't split by technical layer, like "the database team" and "the UI team". We split by business function: a Catalog team, an Ordering team.

**Smart endpoints and dumb pipes.** The business logic lives inside the services. The network in between, whether HTTP or a message broker, just moves data. It doesn't contain business rules.

**Decentralized data management.** We just saw this: each service owns its data.

And **design for failure.** In a distributed system, something is *always* broken somewhere. A network call will time out. A service will restart. So we design with retries, timeouts, and fallbacks from day one.

Put simply: each service follows the Single Responsibility Principle, applied at the level of the whole system.

## Slide 7: Benefits and challenges  `[8:30]`

Now, why would we accept all this extra complexity? Let's be honest about both sides.

On the left, the **benefits**. The first is agility: small teams ship features faster because they don't coordinate a huge release. The second is **independent scaling**. On Black Friday, maybe only the Basket and Ordering services get heavy traffic. With microservices, I scale just those two instead of copying the entire application ten times. We also get small codebases that are easy to understand, independent deployments through CI/CD, the freedom to pick the right tool for each job, and **fault isolation**: if the Discount service crashes, customers can still browse products.

On the right, the **challenges**, and they are real. The system as a whole is much more **complex**. Every call between services is now a **network call**, which can be slow or fail. Testing an end-to-end flow is harder. **Data consistency** is harder too, because there's no single database transaction across services, so we accept **eventual consistency**. Deployment needs serious DevOps automation. You need centralized logging and monitoring. And you can't just set a breakpoint and step through the whole flow in your IDE anymore.

So microservices don't remove complexity. They **move** it: from inside the code to between the services.

## Slide 8: When to use, and when not to  `[10:00]`

Given those costs, when do they make sense?

Use microservices when you really need **independent deployment with zero downtime**, when you need to **scale parts of the system independently**, when different parts need **different data stores**, and when you have **several autonomous teams**. Most importantly: when you have a *really good reason*, not because it's fashionable.

And when should you avoid them? The worst case is the **distributed monolith**. That's when you split into services, but they're so tightly coupled that you still have to deploy them all together. You get all the complexity of microservices and none of the benefits.

Also avoid them if you have **no DevOps foundation**: no CI/CD, no containers, no monitoring. Avoid them if your **team is small**, because the team will spend its time running infrastructure instead of building features. Avoid them for a **brand-new startup** whose domain keeps changing, because moving boundaries between services is very expensive. And watch out for the **shared database** anti-pattern: if two services share tables, they aren't really independent.

## Slide 9: Monolith vs. microservices  `[11:00]`

This table summarizes the comparison, so I'll just point out the rows that matter most.

For **scalability**, a monolith scales as one unit; microservices scale each service separately. For **deployment**, a monolith is simple but all-or-nothing; microservices deploy independently. For **data**, one shared database versus one database per service. And for **failure**, a single bug can take down a monolith, while microservices isolate faults.

Neither one is "better". A monolith is a perfectly good choice for many systems. Choose based on your team and your problem.

## Slide 10: Database-per-service  `[12:15]`

Let's zoom in on the most important rule: **database per service**.

A service's data can only be reached **through that service's API**. The Ordering service is never allowed to run a SQL query against the Catalog database. This is what keeps services truly independent: Catalog can change its schema tomorrow, and nobody else breaks.

A nice side effect is **polyglot persistence**, meaning each service picks the storage that fits its data. Look at EShop. **Catalog** uses PostgreSQL through a library called Marten, which lets it store products as flexible JSON documents. **Basket** uses PostgreSQL too, plus **Redis** as a fast cache, because carts are read constantly. **Discount** uses SQLite, which is enough for a small coupon table. And **Ordering** uses **SQL Server**, because orders have a rich relational structure with customers, items, addresses, and payments.

Four services, three different database technologies, and that's completely fine.

## Slide 11: Decomposing the domain  `[13:30]`

A very common question is: how do I decide *where* to split my system into services?

The approach in this project comes from Domain-Driven Design. You start from **requirements** and **user stories**. For each story, you find the **nouns**, which become your entities, and the **verbs**, which become your use cases. Then you group them into **bounded contexts**.

Look at the table. "List and filter products by category" gives us Product and Category, which is the **Catalog** context. "Put products into the cart" gives us ShoppingCart, which is **Basket**. "Apply coupons" gives us **Discount**. "Check out and create an order" gives us **Ordering**.

Two practical tips. First, do this *with domain experts*, not alone at your desk; they know where the real boundaries are. Second, a bounded context is *usually* one microservice, but not always. If one part needs to scale very differently, you can split it further.

## Slide 12: The big picture  `[15:00]`

And here is the result: the full EShop architecture. Take a moment with this one, because the whole rest of the talk walks through it.

At the top is **Shopping.Web**, a Razor Pages web application. It never talks to the services directly. It only talks to the **YARP API Gateway**.

The gateway routes requests over HTTP to three services: **Catalog**, **Basket**, and **Ordering**. Notice they use different internal architectures: Catalog and Basket use Vertical Slice architecture, while Ordering uses Clean Architecture with DDD, because its domain is richer.

Now look at the two special connections. **Basket calls Discount using gRPC**, to get the discount for each product. That's synchronous: Basket waits for the answer. And **Basket publishes an event to RabbitMQ** when the user checks out, and **Ordering consumes** that event to create the order. That's asynchronous: Basket does not wait.

So this one diagram already contains every communication method we'll discuss: REST, gRPC, messaging, and a gateway. Keep it in mind.

---

## Slide 13: Part 2 divider  `[15:15]`

Now that we know *what* we're building, let's see *how* to build a single microservice.

## Slide 14: The process for each microservice  `[16:30]`

In this project, every service is built using the same repeatable process, and I recommend it for your own services too.

First, **domain analysis**. What are the domain models? For Catalog, that's `Product`. What are the use cases? Create, read, update, delete, and filter by category. What API endpoints expose them? For example, `GET /products/{id}`. And what data store fits best?

Second, **technical analysis**. Which architecture style? Vertical Slice or Clean Architecture. Which patterns? CQRS, mediator, dependency injection. Which libraries? And what does the folder structure look like?

Only after these two steps do we write code. Then we **containerize** the service with a Dockerfile, and **orchestrate** it with the other services using docker-compose.

Doing the analysis first sounds slow, but it's what stops you from building a distributed monolith.

## Slide 15: Why ASP.NET Core 8  `[17:45]`

Our tool is ASP.NET Core 8. It's cross-platform, open source, and one of the fastest web frameworks in public benchmarks. For microservices, the key features are: **Minimal APIs**, which let you write an endpoint in one line; **built-in dependency injection**, configuration, logging, and health checks; **first-class gRPC support**; and small container images.

Every ASP.NET Core service has the same `Program.cs` shape, in five steps. **One**, create the builder. **Two**, register your services in the DI container: databases, MediatR, and so on. **Three**, build the app. **Four**, configure the HTTP pipeline, meaning your endpoints and middleware. **Five**, run it.

Once you recognize these five steps, you can read the startup code of every service in this project.

## Slide 16: TodoApi, the simplest microservice  `[19:15]`

Here's the smallest possible microservice. This is the TodoApi from section 4 of the repository.

Two commands to create it: `dotnet new web`, and add the EF Core in-memory database package.

Then the whole service fits on one slide. We register a `DbContext` using an in-memory database. We build the app. Then we map endpoints: `GET /todoitems` returns all items, `GET /todoitems/{id}` returns one, and `POST /todoitems` saves a new item and returns `201 Created` with its location.

Notice what makes this a microservice, even though it's tiny: it runs in **its own process**, it has **one responsibility**, it owns **its own data store**, and it exposes an **HTTP API**. That's the definition we saw on slide 5, in about twenty lines of code.

Of course, real services need more structure, so let's look at one.

## Slide 17: Catalog.API structure  `[20:30]`

This is **Catalog.API**, the first real service in EShop. It uses **Vertical Slice Architecture**.

Look at the folder tree on the left. Instead of folders like "Controllers", "Services", and "Repositories", there is **one folder per feature**: CreateProduct, GetProductById, GetProducts, and so on. Each folder contains everything that feature needs: its endpoint and its handler. If I need to change how "get product by ID" works, I open one folder, and I don't touch anything else.

On the right are the libraries. **Carter** organizes Minimal API endpoints into modules. **MediatR** implements CQRS, separating commands, which change data, from queries, which read data. **Marten** turns PostgreSQL into a document database. **FluentValidation** validates requests. **Mapster** maps objects.

There's also a shared **BuildingBlocks** library with the common CQRS interfaces, the validation and logging behaviors, and a global exception handler, so every service doesn't reinvent them.

## Slide 18: Endpoint → MediatR → handler  `[22:15]`

Let's follow one request through the code: `GET /products/{id}`.

At the top is the **endpoint**. It's a Carter module. It maps the route, and its only job is to translate HTTP into a query: it creates a `GetProductByIdQuery` with the ID and sends it through MediatR's `ISender`. When the result comes back, it maps it to a response object and returns `200 OK`.

The endpoint knows nothing about the database. That's the point.

At the bottom is the **handler**, the query side of CQRS. The `GetProductByIdQuery` is a simple record. The handler receives a Marten `IDocumentSession` through constructor injection, loads the product by ID, and if the product doesn't exist, throws a `ProductNotFoundException`. The global exception handler turns that into a proper 404 response.

So the flow is: **HTTP → endpoint → MediatR → handler → database**. Every feature in Catalog and Basket follows exactly this pattern, which makes the code very predictable.

## Slide 19: Catalog Program.cs  `[23:30]`

And here's how it's all wired together, following the same five steps we saw earlier.

We register **MediatR** from the current assembly and add two **pipeline behaviors**: validation and logging. A pipeline behavior wraps every request, like middleware for MediatR. So every command is automatically validated and logged, without writing that code in each handler. These are the cross-cutting concerns.

Then we register the FluentValidation validators, Carter, and **Marten** with the connection string from configuration. Then the exception handler, and **health checks** that also check the PostgreSQL connection.

After `Build`, we map the Carter endpoints, enable the exception handler, and expose a `/health` endpoint. That health endpoint matters in production: Docker or Kubernetes can call it to know whether the service is alive and its database is reachable.

## Slide 20: Dockerfile  `[24:45]`

Now we package the service. This is a **multi-stage Dockerfile**, generated by Visual Studio.

The first stage, `base`, uses the small **ASP.NET runtime** image and exposes ports 8080 and 8081. The `build` stage uses the full **.NET SDK** image. It copies the project files, restores packages, copies the source, and builds. Notice it also copies the BuildingBlocks project, because Catalog depends on it. The `publish` stage produces the release output. And the `final` stage copies only the published output onto the small runtime image.

Why multiple stages? So the final image doesn't contain the SDK or the source code. It's smaller, faster to deploy, and more secure.

The workflow is always the same: **Dockerfile → image → container**. And images are stored in a registry, like Docker Hub or Azure Container Registry.

## Slide 21: docker-compose  `[26:00]`

One container is easy. Eleven containers need **docker-compose**.

On the left, `docker-compose.yml` declares *what* runs: a PostgreSQL container for the Catalog database, and the Catalog API built from its Dockerfile. The override file says *how* it runs: environment variables, including the connection string that points to `catalogdb`, a `depends_on` so the database starts first, and the port mapping.

On the right is the port convention used throughout the project. Each service has a local port in the 5000 range and a Docker port in the 6000 range. Inside the container, every service listens on 8080 and 8081.

And here's the important detail: inside the Docker network, services find each other **by service name**. The gateway doesn't call "localhost:6000"; it calls `http://catalog.api:8080`. That's basic service discovery, provided by Docker.

We now have a running service. So how do services talk to each other?

---

## Slide 22: Part 3 divider  `[26:15]`

This is the core of the talk: service invocation methods.

## Slide 23: Sync vs. async  `[27:45]`

There are two fundamental styles of communication.

**Synchronous**: the client sends a request and **waits** for the response. The protocols are HTTP with REST, gRPC, GraphQL, or WebSockets. The pattern is request and response. The catch is **temporal coupling**: both services must be up at the same moment. If the server is down, the client fails.

**Asynchronous**: the client sends a message and **does not wait**. The message goes to a **broker**, like RabbitMQ or Kafka, using a protocol such as AMQP. It can be one-to-one, using a queue, or one-to-many, using publish/subscribe. The coupling is loose: if the receiver is down, the broker keeps the message and delivers it later.

In EShop, we use both. Basket calls Discount **synchronously with gRPC**, because it needs the price right now. The web app calls the APIs **synchronously with REST**. And Basket tells Ordering about a checkout **asynchronously**, with an event.

The rule of thumb at the bottom is worth remembering: **REST for public APIs, gRPC for internal calls, messaging for workflows that span services.**

## Slide 24: REST over HTTP  `[28:45]`

Method one is the most familiar: **REST over HTTP**. We have resource-oriented URLs like `GET /products` and `POST /basket`, with JSON payloads that any client understands: browsers, mobile apps, Postman.

On the client side, calling REST APIs with raw `HttpClient` means a lot of repetitive code: build the URL, serialize, send, check the status, deserialize. Shopping.Web avoids that with a library called **Refit**. You write an **interface** and put the route on each method as an attribute, like `[Get("/catalog-service/products/{id}")]`. Refit generates the implementation for you.

In `Program.cs`, you register it with `AddRefitClient` and set the base address to the gateway. Then, in a Razor page, you just inject `ICatalogService` and call `GetProduct(id)`, like a normal C# method. It's type-safe, and it uses HttpClientFactory underneath.

## Slide 25: gRPC  `[30:15]`

Method two is **gRPC**, a remote procedure call framework created by Google.

Why use it instead of REST? Three reasons. It runs on **HTTP/2**. It uses **Protocol Buffers**, a compact binary format that is much smaller and faster to parse than JSON. And it's **contract-first**: you write a `.proto` file, and the tooling generates the server base class and the client code for you, in C# or almost any other language.

On the right is the real contract for the Discount service. It defines a service called `DiscountProtoService` with four methods: Get, Create, Update, and Delete discount. It also defines the messages. For example, `GetDiscountRequest` has a product name, and `CouponModel` has an ID, product name, description, and amount. The numbers, like `= 1` and `= 2`, are field tags used in the binary encoding, not default values.

The big idea of gRPC is that the client calls a remote method **as if it were a local object**. It's ideal for fast, internal, service-to-service calls. It's less ideal for browsers, which is why the public side of EShop still uses REST.

## Slide 26: gRPC server  `[31:15]`

Here's the server side, in the Discount.Grpc project.

In `Program.cs`, we add gRPC with `AddGrpc`, register the EF Core context on SQLite, run migrations at startup, and map our service with `MapGrpcService`.

The service class, `DiscountService`, **inherits from `DiscountProtoServiceBase`**. We didn't write that base class; it was generated from the `.proto` file. We simply override `GetDiscount`: look up the coupon by product name, and if there isn't one, return a "No Discount" coupon with amount zero. Then map it to the generated `CouponModel` and return it.

Notice there's no routing, no JSON, no HTTP status codes. The contract defines all of that.

## Slide 27: gRPC client  `[32:30]`

And here's the client side, in Basket.API. Three small steps.

**One**, in the project file, include the *same* `discount.proto` file, but with `GrpcServices="Client"`. The build then generates only the client stub, not the server.

**Two**, in `Program.cs`, register a typed gRPC client with `AddGrpcClient`, and read the Discount service address from configuration. Locally, that's localhost:5052; in Docker, it's `discount.grpc:8081`.

**Three**, inject the client into the handler and call it. In `StoreBasketCommandHandler`, for each item in the cart, we call `GetDiscountAsync` with the product name and subtract the coupon amount from the price.

Look at how natural that call is: it reads like a local method call, but it's actually a binary HTTP/2 request to another container. That's the value of gRPC.

## Slide 28: Asynchronous messaging  `[34:15]`

Method three is **asynchronous messaging**, and it solves a specific problem.

Imagine a chain of synchronous calls: A calls B, B calls C, C calls D. Every service in the chain must be up. The latencies add up. And if D is slow, *everyone* is slow. The services are tightly coupled.

With messaging, the producer **publishes an event** to a **message broker**, and it's done. The broker **fans out** the event to every subscriber. The consumer doesn't even need to be running when the event is published; the broker keeps it. And if tomorrow we add a new service that also cares about checkouts, say, a notification service, it just subscribes. The producer doesn't change at all.

It isn't free, though. The broker becomes a critical component, so it must be highly available. Debugging a flow across a broker is harder. And brokers usually deliver **at least once**, so a consumer may receive the same message twice. That means consumers must be **idempotent**: handling a message twice must be safe. Message order also isn't guaranteed.

On the right, **RabbitMQ**, the broker we use. The key concept: producers don't send directly to queues. They send to an **exchange**, and the exchange routes to queues according to its type: **direct** by exact key, **fanout** to everyone, **topic** by pattern, or **headers**.

On top of RabbitMQ, we use **MassTransit**, a .NET library that creates the exchanges and queues, handles serialization, and adds retries. So in our code we think in terms of *events*, not exchanges and bindings.

## Slide 29: Messaging setup  `[35:15]`

Here's the shared messaging library. Two parts.

At the top is the **contract**: an `IntegrationEvent` base record with an ID, a timestamp, and the event type. Then `BasketCheckoutEvent` extends it with the user name, total price, address, and payment fields. Both the publisher and the consumer reference this same class. It's the shared agreement between Basket and Ordering.

At the bottom is **one extension method**, `AddMessageBroker`. It configures MassTransit with kebab-case endpoint names, optionally scans an assembly for consumers, and connects to RabbitMQ using the host, user name, and password from configuration. Every service calls this one method, so the messaging setup is identical everywhere.

## Slide 30: Publish and consume  `[36:30]`

Now let's see both sides of the checkout.

On the left is the **publisher**, in Basket. The checkout handler receives an `IPublishEndpoint` from MassTransit. It loads the basket; if there's none, it returns false. Otherwise it maps the checkout data into a `BasketCheckoutEvent`, sets the total price from the basket, and calls **`Publish`**. Then it deletes the basket and returns success, **immediately**. It doesn't wait for an order to be created.

On the right is the **consumer**, in Ordering. The registration passes the assembly, so MassTransit finds the consumers in it. The `BasketCheckoutEventHandler` implements **`IConsumer<BasketCheckoutEvent>`**. When a message arrives, `Consume` is called. It logs the event, maps it into a `CreateOrderCommand`, and sends it through MediatR, which creates the order.

Notice: **Basket has no idea Ordering exists.** It only publishes an event. That's loose coupling in practice.

## Slide 31: API Gateway with YARP  `[38:00]`

Method four is a pattern rather than a protocol: the **API Gateway**.

Without a gateway, the web client would need to know the address of every service. With **Gateway Routing**, the client sees **one entry point**, and the gateway routes each request to the right internal service. If we move or split a service later, only the gateway configuration changes, not the client.

A full **API Gateway** can also aggregate calls and handle cross-cutting concerns: authentication, rate limiting, caching, SSL. And for larger systems, the **Backend-for-Frontend** pattern means one gateway per client type, such as web and mobile, so a single gateway doesn't become a bottleneck.

We implement ours with **YARP**, Microsoft's reverse proxy library for .NET. The code on the left is the *entire* gateway: add the reverse proxy, load its configuration, add a rate limiter that allows 5 requests per 10 seconds, and map the proxy.

On the right is the configuration. A **route** matches a path, like `/catalog-service/{**catch-all}`, and points to a **cluster**. The **transform** removes the prefix. The cluster says where the service actually lives: `http://catalog.api:8080`. So a request to `/catalog-service/products` becomes `catalog.api:8080/products`. Notice that the ordering route also applies the rate limiting policy.

## Slide 32: Choosing a method  `[39:00]`

To wrap up this part, here's how to choose.

**REST**: synchronous JSON, best for public and client-facing APIs. In EShop, that's everything behind the gateway.

**gRPC**: synchronous, binary, HTTP/2, best for fast internal calls with a strict contract. In EShop, Basket to Discount.

**Messaging**: asynchronous events, best for workflows that span services, and when you want decoupling or fan-out. In EShop, Basket to Ordering.

**API Gateway**: the single entry point, with routing, rate limiting, and security.

Real systems **mix** them. The advice to take home: keep your synchronous call chains **short**, and use messaging for anything that doesn't need an immediate answer.

Enough slides. Let's see it running.

---

## Slide 33: Part 4 divider  `[39:15]`

*(Transition only. Go straight to the next slide.)*

## Slide 34: Demo scenario  `[40:15]`

Here's what we'll do. It's one user journey, the checkout, and it touches every method we discussed.

**Step one**: list products. That's REST, through the gateway, to Catalog.

**Step two**: add an iPhone to the basket. Basket will call **Discount over gRPC** to get the coupon, and save the discounted price.

**Step three**: check out. Basket will **publish an event to RabbitMQ**, and **Ordering will consume it** and create an order in SQL Server.

**Step four**: we query Ordering to see that the new order really exists.

## Slide 35: Demo setup  `[41:00]`

*(Keep this short. Everything is already running.)*

I started everything before the talk with one docker-compose command. It builds and runs eleven containers: five backing services, which are two PostgreSQL databases, Redis, SQL Server, and RabbitMQ, plus six applications.

*[Switch to the terminal and run `docker ps`.]*

Here they are, all up. These are the URLs we'll use: the gateway on port 6004, the web app on 6005, and the RabbitMQ dashboard on 15672.

## Slide 36: Demo ①, REST through the gateway  `[42:30]`

*[Run the first curl: `GET http://localhost:6004/catalog-service/products?pageNumber=1&pageSize=5`.]*

I'm calling the **gateway**, on port 6004, with the prefix `catalog-service`. YARP matches the catalog route, removes the prefix, and forwards the call to `catalog.api:8080/products`. And here are the products, loaded from PostgreSQL by Marten.

*[Run the second curl, for the product by ID.]*

And one product by ID: the iPhone X, category "Smart Phone", price **950**. Remember that number.

The takeaway here: my client only knows the gateway. It has no idea where Catalog actually runs.

## Slide 37: Demo ②, gRPC inside the basket  `[44:30]`

*[In a second terminal inside `section20`, run `docker-compose logs -f discount.grpc` and keep it visible next to the first.]*

Now I'll store a basket for user "swn" with one iPhone X at 950.

*[Run the POST to `/basket-service/basket`.]*

Look at the Discount logs: "Discount is retrieved for ProductName: IPhone X, Amount: 150." That line was written by the Discount service, because **Basket just called it over gRPC** while handling my request.

*[Run `GET /basket-service/basket/swn`.]*

And if I read the basket back, the price is now **800**: 950 minus the 150 coupon. That's a synchronous gRPC call, using client code generated from the same `.proto` file we saw on the slide.

*If the price is still 950: the gRPC call probably failed because of the HTTPS certificate. Say "this is exactly the kind of network failure we design for", show the screenshot, and move on.*

## Slide 38: Demo ③ and ④, async checkout  `[47:00]`

Now the most interesting part: checkout.

*[Run the checkout POST.]*

The response is `isSuccess: true`, and it came back **immediately**. Basket did not wait for Ordering. It just published the event and returned.

*[Switch to the RabbitMQ UI, Exchanges tab.]*

In RabbitMQ, you can see the exchange MassTransit created for our event, named after the event type, `BuildingBlocks.Messaging.Events:BasketCheckoutEvent`. And if you look at the message rates, you can see our message just went through.

*[Run `docker-compose logs ordering.api` and find the line "Integration Event handled: BasketCheckoutEvent".]*

The Ordering service logged "Integration Event handled: BasketCheckoutEvent". It picked the event up from the queue and created the order.

*[Run `GET /ordering-service/orders?pageIndex=0&pageSize=10`.]*

And here is the new order, with order name "swn". Two services, two databases, no direct call between them, connected only by an event.

*If you have time left (clock before 46:00): open http://localhost:6005, add a product to the cart, and check out through the UI. Say: "Same flow, but now through a real front end using Refit."*

---

## Slide 39: Summary  `[48:15]`

Let's wrap up.

**Microservices** are small, autonomous services built around business capabilities, each with its own data and its own deployment. They're powerful, but they add real complexity, so adopt them when you have a real reason, and consider starting with a monolith.

**To create one**, follow a repeatable process: analyze the domain, analyze the technical design, build it with ASP.NET Core Minimal APIs, package it with a Dockerfile, and run it alongside everything else with docker-compose.

**To connect services**, you have four tools. **REST** for client-facing APIs. **gRPC** for fast, contract-first internal calls. **Asynchronous messaging** with RabbitMQ and MassTransit for decoupled, event-driven workflows. And an **API gateway** like YARP as the single front door.

And as we saw in the demo, one simple checkout used all four.

## Slide 40: References  `[48:30]`

All the code is in the repository, from section 4, the tiny TodoApi, up to section 20, the complete system. There's a Postman collection if you want to replay the demo yourself. The links here are the best places to go deeper: Martin Fowler's article, and Microsoft's free e-book on .NET microservices architecture.

## Slide 41: Questions  `[50:00]`

That's all from me. Thank you! I'm happy to take any questions.

---

## Likely questions and short answers

**"How do you keep data consistent without a shared database transaction?"**
With eventual consistency and events. For stronger guarantees, there are two patterns. The **Outbox pattern** saves the event in the same database transaction as the data, and a background process publishes it later, which avoids the "dual write" problem. The **Saga pattern** coordinates multi-step business transactions, with compensating actions when a step fails. Both are covered later in the course slides.

**"Why gRPC for Discount but REST for everything else?"**
Discount is only called by other services, and it's called once per cart item, so speed and a strict contract matter. The other services are called by a browser-based app through the gateway, and browsers work best with REST and JSON.

**"What happens if RabbitMQ is down during checkout?"**
In this code, the publish would fail and the checkout request would return an error. That's why production systems run the broker as a cluster, and why the Outbox pattern exists: the event is saved first and published once the broker is back.

**"What if the Discount service is down?"**
Then the `StoreBasket` call fails, because the gRPC call is synchronous. In production, you would add a timeout, retries, and a circuit breaker, for example with Polly, and possibly fall back to "no discount".

**"How many services should I start with?"**
Fewer than you think. Start with a modular monolith, then extract a service when a specific part needs independent scaling or deployment.

**"How does this run in production?"**
The same container images run on Kubernetes, such as Azure AKS or Amazon EKS. Kubernetes adds service discovery, scaling, health-check-based restarts, and rolling deployments. docker-compose is for local development.
