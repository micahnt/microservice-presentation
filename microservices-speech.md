# Speaker Script: Microservices with .NET 8 (60 minutes)

Companion to `microservices-slides.md`. Each section below matches one slide. The number in brackets is the target **clock time when you leave that slide**, so you can check your pace as you go.

Speaking pace assumed: about 120 words per minute, with short pauses on code and diagram slides. The spoken text is about 4,950 words (about 41 minutes); pauses and a longer Q&A fill the rest. Lines in *italics* are stage directions, not things to say.

---

## Timing plan

| Part | Slides | Duration | Clock at end |
|---|---|---|---|
| Opening | 1–2 | 2:00 | 2:00 |
| Part 1: Introduction to Microservices & DDD | 3–13 | 13:45 | 15:45 |
| Part 2: Synchronous communication (REST & gRPC) | 14–19 | 10:00 | 25:45 |
| Part 3: Asynchronous communication & message brokers | 20–27 | 11:45 | 37:30 |
| Part 4: API Gateway & BFF | 28–32 | 6:30 | 44:00 |
| Summary and Q&A | 33–35 | 16:00 | 60:00 |

**If you are running late**, these slides can be shortened to one sentence without losing the story:
- Slide 7 (benefits and challenges): "The benefits are speed and independent scaling; the cost is complexity, network calls and harder consistency."
- Slide 8 (when to use): "Use them for a real reason, never without DevOps, and never with a shared database."
- Slide 25 (messaging setup): "One shared extension method configures MassTransit for every service."

---

## Slide 1: Title  `[1:00]`

Good morning, everyone, and thanks for being here.

Today's topic is microservices with .NET 8. In the next sixty minutes we'll cover four things. First: what microservices are, and how Domain-Driven Design helps us decide where to split a system. Second: how services talk to each other synchronously, with REST and gRPC. Third: how they talk asynchronously, with events and a message broker. And fourth: the API gateway that sits in front of them all.

Everything I show comes from one real project, called EShop Microservices. It's an e-commerce system with a Catalog, a Basket, a Discount service, an Ordering service, an API gateway, and a web front end. Every code sample on these slides comes from that repository, some trimmed to fit on a slide, and near the end I'll run it live.

## Slide 2: Agenda  `[2:15]`

Here is the plan.

We start with about fourteen minutes on microservices and Domain-Driven Design: monolith versus microservices, design principles, and the DDD idea of bounded contexts.

Then ten minutes each on synchronous and asynchronous communication, and seven minutes on API gateways and the Backends for Frontends pattern.

And we'll finish with a summary and your questions. If something is unclear along the way, feel free to raise your hand.

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

**Deployment**: the unit of deployment is different. A monolith is **one deployable unit**: a one-line change means building and redeploying the whole application. Microservices are **many deployable units**: each service is its own container, with its own CI/CD pipeline, released independently. Small systems can host those containers on **Azure Container Apps**; large ones usually run them on **Kubernetes**, for example AKS on Azure. The drawing at the bottom shows this.

One note: zero downtime is not a microservices-only feature. It's an operations question, not an architecture one. A monolith behind a **load balancer** can run version 1 and version 2 side by side, and switch traffic to version 2 without interrupting users.

**Data**: one shared database, versus one database per service.

**Failure**: one bug, like a memory leak, can take down the whole monolith. In microservices, faults are isolated to one service.

And **typical fit**: a monolith is not "for small teams"; it depends on the business domain. It's the usual choice for an **MVP** or a new project, while the domain and its bounded contexts are still being discovered. Microservices fit mature, well-understood domains, and they need a team that's comfortable with containers, DevOps and monitoring.

Neither one is "better". A monolith is a perfectly good choice for many systems.

And many successful microservice systems started life as a monolith. The monolith taught the team where the real boundaries were, and *then* they split it.

## Slide 7: Benefits and challenges  `[8:15]`

Here are both sides, honestly.

The **benefits**: agility, because small teams ship without coordinating a huge release; **independent scaling**; small codebases; independent deployment; the right tool for each job; and **fault isolation**: if Discount crashes, customers can still browse products.

The **challenges** are real. The whole system is more **complex**. Every call is now a **network call** that can be slow or fail. End-to-end testing is harder. **Data consistency** is harder, because there's no single database transaction across services. And debugging across many services is hard.

So microservices don't remove complexity. They **move** it, from inside the code to between the services.

## Slide 8: When to use, and when not to  `[9:15]`

Use them when you need to **release parts of the system independently**, **independent scaling**, different data stores, or several autonomous teams. And most importantly, when you have a *really good reason*. Zero downtime on its own is not a reason: as we just saw, a monolith behind a load balancer can deploy without downtime too.

Avoid them when you'd build a **distributed monolith**: services so tightly coupled that you still deploy them together. That gives you all the complexity and none of the benefits. Also avoid them without a **DevOps foundation**, for an **MVP or early project** whose bounded contexts aren't clear yet, and never let services **share a database**.

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

Today we focus on the strategic side. The tactical patterns get their own session, together with a live demo.

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

## Slide 13: The big picture  `[15:45]`

And here's how the whole system fits together. Read it left to right.

This is the reference system for the rest of the talk, so it's worth a few seconds.

**Shopping.Web**, the client, only talks to the **YARP API gateway**. The gateway routes to **Catalog**, **Basket** and **Ordering**, each with its own database. Catalog and Basket use Vertical Slice architecture; Ordering uses Clean Architecture with a DDD domain model.

And notice the two special connections. **Basket calls Discount over gRPC**: synchronous. And **Basket publishes an event to RabbitMQ**, which **Ordering consumes**: asynchronous. Those two arrows are Parts 2 and 3 of this talk. And the gateway in front of it all is Part 4. So everything we discuss from now on is somewhere on this one diagram.

*If someone compares this with the course's "Big Picture" slide: that picture also draws a gateway arrow to Discount. In the actual code, the YARP config only routes to Catalog, Basket, and Ordering.*

---

## Slide 14: Part 2 divider  `[16:00]`

Part two: synchronous communication.

## Slide 15: Service-to-service communication  `[18:30]`

Services must cooperate in two ways: **ask for data**, like "what's the discount on this product?", or **trigger work**, like "create this order".

There are two styles. **Synchronous**: the client sends a request and **waits**. HTTP with REST, gRPC, GraphQL. The catch is **temporal coupling**: both services must be up at the same moment.

**Asynchronous**: the client sends a message and **does not wait**. The message goes to a broker, like RabbitMQ or Kafka, using a protocol like AMQP.

There are two patterns. **One-to-one**, with a **queue**: each message is processed by exactly one consumer, like a task handed to one worker. **One-to-many**, with **publish/subscribe**: one message is delivered to every subscriber, like an announcement that several services can react to.

And the coupling is **loose**. The sender only knows the broker, not the receiver. If the receiver is down, the broker **stores** the message and delivers it when the receiver is back, and it **retries** if processing fails. So the two services don't need to be up at the same time.

EShop uses both. Basket asks Discount **synchronously with gRPC**, because it needs the price right now. The web app calls the APIs **with REST**. And Basket tells Ordering about a checkout **asynchronously**.

The rule of thumb: **REST for public APIs, gRPC for internal calls, messaging for workflows that span services.**

## Slide 16: Introduction to Protobuf and gRPC  `[20:15]`

Now gRPC, a remote procedure call framework from Google. It runs on **HTTP/2**, and the client calls a remote method **as if it were a local object**.

The contract is written in **Protocol Buffers**, in a `.proto` file. On the right is the real contract for Discount. A `service` block lists the methods: GetDiscount, Create, Update and Delete. `message` blocks define the data: a `GetDiscountRequest` with a product name, and a `CouponModel` with ID, name, description and amount.

Two things people often miss. First, the numbers, like `= 1` and `= 2`, are **field numbers** used in the binary encoding, not default values. Never reuse or change them once in use. Second, Protobuf is **binary**: much smaller and faster to parse than JSON.

And in .NET, you don't write the networking code. The build **generates** a server base class and a client class from the `.proto` file.

## Slide 17: gRPC server  `[21:30]`

Here's the server. In `Program.cs`: `AddGrpc`, the database, and `MapGrpcService`.

`DiscountService` **inherits from `DiscountProtoServiceBase`**, the class generated from the `.proto` file. We just override `GetDiscount`: find the coupon by product name, or return a "No Discount" coupon with amount zero. Notice there's no routing, no JSON and no status codes; the contract defines all of that.

Errors work differently too. Instead of returning an HTTP 404, the service throws an `RpcException` with a gRPC status code. For example, `DeleteDiscount` throws `StatusCode.NotFound` if the coupon doesn't exist, and `Create` and `Update` throw `InvalidArgument` for a bad request. The client receives those as typed exceptions. The other three methods, Create, Update and Delete, follow the same pattern: map the Protobuf message to the database model, save, and map back.

## Slide 18: gRPC client  `[23:15]`

And the client, in Basket. Three steps.

**One**: the Basket project includes the *same* `.proto` file, with `GrpcServices="Client"`, so only the client is generated. The contract is shared; the code is not.

In fact, the Basket project doesn't even copy the file. It links to Discount's `.proto` file directly, so if the contract changes, both sides see the change on the next build.

**Two**: register a typed client with `AddGrpcClient` and read Discount's address from configuration. The extra handler accepts the self-signed development certificate. That's fine on a laptop, but never ship it to production.

**Three**: inject the client and call it. For each cart item, `GetDiscountAsync` with the product name, and subtract the coupon amount. It reads like a local method call, but it's a binary HTTP/2 request to another container.

## Slide 19: When to use REST, when to use gRPC  `[25:45]`

So when do you pick which? Let's go row by row.

Both can run on HTTP/2, but gRPC requires it. REST sends **text JSON**; gRPC sends **binary Protobuf**, smaller and faster. For REST a contract is optional, for example OpenAPI; for gRPC it's **required**, and code is generated from it. **Browsers** speak REST natively, but gRPC needs gRPC-Web or a proxy. gRPC supports **streaming** in both directions; REST is request and response. And REST is easy to test by hand with curl or a browser; gRPC needs tooling.

So: **choose REST** for public APIs, browsers, third parties and simple CRUD. **Choose gRPC** for internal, high-volume, low-latency calls between your own services.

That's exactly EShop's split: REST behind the gateway for the web app, and gRPC for Basket to Discount, which is called once for every item in every cart.

Think about what that means in numbers. A cart with ten items means ten Discount calls every time the basket is saved, for every user. With gRPC, those calls share one HTTP/2 connection and send a few bytes of binary each, instead of opening requests and parsing JSON. That's where gRPC pays off.

And it's not an either-or choice for the whole system. Most real systems do exactly what EShop does: REST at the edge, where browsers and partners connect, and gRPC inside, between services you own. ASP.NET Core can even expose one gRPC service as REST as well, with a feature called JSON transcoding, if you need both.

---

## Slide 20: Part 3 divider  `[26:00]`

Part three: asynchronous communication, and why it matters.

## Slide 21: Basic event-driven architecture  `[27:45]`

Start with the problem on the left. Imagine creating an order using only synchronous calls. The gateway calls ShoppingCart. ShoppingCart calls Product and Pricing, then Order. Order calls Billing and Shipping. Six calls for one click. Every service must be up, latencies add up, and if Shipping is slow, everyone is slow.

Event-driven architecture flips this around. An **event** is a **fact that already happened**, named in the past tense: *BasketCheckedOut*, *OrderCreated*. The **producer** publishes it and moves on. It **doesn't know** who's listening. **Consumers** react in their own time.

And a useful distinction: a **command** tells one specific service what to do, like "CreateOrder". An **event** just announces what happened, to anyone interested. Commands couple you to a receiver; events don't.

An everyday analogy: a command is a phone call to one person, asking them to do something. An event is an announcement on a notice board: "the basket was checked out". Whoever cares reads it and acts. The person who posted it doesn't need to know who read it, or even be around when they do.

In EShop, checkout is the event. Basket doesn't call Ordering and say "create an order". It announces "this basket was checked out", and Ordering decides what that means for it.

## Slide 22: The role of a message broker  `[29:30]`

So what does the broker in the middle actually do? It **receives** messages, **stores** them until consumers are ready, even if a consumer is down, **routes** them to the right queues, and **delivers and retries**. It decouples services in **time**, because they don't need to be up together, and in **location**, because the producer doesn't need the consumer's address.

**RabbitMQ** is an open-source broker that implements the AMQP protocol. The key concept: producers don't send directly to queues. They send to an **exchange**, and the exchange routes to queues based on its type: **direct** by exact key, **fanout** to everyone, **topic** by pattern, or **headers**.

On top of RabbitMQ, EShop uses **MassTransit**, a .NET library that creates the exchanges and queues and handles serialization and retries. So our code thinks in terms of events, not exchanges.

RabbitMQ also comes with a management web UI, on port 15672 in our setup. It shows every exchange, queue and message rate, and it's the first place to look when a message seems to be lost.

## Slide 23: RabbitMQ vs. Kafka  `[31:00]`

RabbitMQ isn't the only broker. The other one you'll hear about most is **Kafka**, and the key difference is in one word each: RabbitMQ is a **queue**, Kafka is a **topic log**.

In **RabbitMQ**, the broker pushes each message to a consumer, and once the consumer acknowledges it, the message is gone. A message is a task to be delivered.

In **Kafka**, messages are appended to a **log**, organized into topics and partitions. They stay there for a retention period, even after they're read. Each consumer group keeps its own position in the log, so two groups can read the same messages independently, and you can **replay** old messages.

The table sums it up. RabbitMQ routes with exchanges and keeps order per queue; Kafka routes by topic and partition key and keeps order per partition. RabbitMQ speaks AMQP; Kafka has its own protocol. So RabbitMQ is a great fit for commands and integration events between services, like our checkout. Kafka shines for high-volume event streaming, logs and analytics. EShop uses RabbitMQ, with MassTransit on top.

## Slide 24: Basic pub/sub pattern  `[32:30]`

The most common way to use a broker is **publish/subscribe**.

Basket publishes **one** `BasketCheckoutEvent` to an exchange. The broker **fans out** a copy to every subscriber's queue. Today in EShop there's one subscriber, Ordering. But tomorrow we could add a Notification service to send an email, and an Analytics service to record the sale, **without changing a single line in Basket**. That's the real power of pub/sub.

Compare it with a plain queue: a queue gives each message to **one** consumer; pub/sub gives **every** subscriber its own copy.

On the right, the costs. Brokers usually deliver **at least once**, so a message can arrive twice, and consumers must be **idempotent**: processing it twice must be safe. Order isn't guaranteed. The broker becomes critical, so it must be highly available. And there's **eventual consistency**: the order appears a moment after checkout, not instantly.

## Slide 25: Messaging setup  `[33:45]`

Here's how EShop sets this up. On the left is the **contract**: an `IntegrationEvent` base record, and `BasketCheckoutEvent` with user name, total price, address and payment. Both Basket and Ordering reference this same class.

On the right is one shared extension method, `AddMessageBroker`. It configures MassTransit, optionally registers consumers from an assembly, and connects to RabbitMQ with settings from configuration. Every service calls this one method.

Two details. `ConfigureEndpoints` tells MassTransit to create a queue for every consumer it found, and bind it to the right exchange, so we never declare queues by hand. And the broker address comes from configuration: locally `amqp://localhost:5672`, and inside Docker `amqp://ecommerce-mq:5672`, set by an environment variable in docker-compose. Same code, different environment.

## Slide 26: Publish and consume  `[35:45]`

And both ends of the checkout.

On the left, the **publisher** in Basket. The checkout handler gets an `IPublishEndpoint` from MassTransit. It loads the basket, maps the checkout data into a `BasketCheckoutEvent`, sets the total price, and calls **`Publish`**. Then it deletes the basket and returns success, **immediately**. It doesn't wait for an order.

On the right, the **consumer** in Ordering. It implements **`IConsumer<BasketCheckoutEvent>`**. When a message arrives, `Consume` logs it, maps it into a `CreateOrderCommand`, and sends it through MediatR, which creates the order.

Notice: **Basket has no idea Ordering exists.** It only publishes an event. That's loose coupling in practice.

One question worth asking about this handler: what if `Publish` succeeds, but deleting the basket fails? Or the other way round? Writing to the database and publishing a message are two separate operations, and either can fail. That's called the **dual write problem**. The standard fix is the **Outbox pattern**: save the event in the same database transaction as the data, and publish it afterwards. The course covers it later; for today, just know the problem exists.


## Slide 27: Transactions across services: the Saga pattern  `[37:30]`

One last async topic: transactions. Each service has its own database, so there's **no single database transaction** that covers several services. How do we keep a multi-step business operation consistent?

The answer is the **Saga pattern**: a sequence of **local transactions**, one per service. Look at the happy path: Ordering creates a pending order, Payment charges the card, Inventory reserves the stock, and Ordering confirms the order. Each step commits on its own.

Now the failure path. Inventory is out of stock. We can't roll back Payment's database, because that transaction is already committed, in another service. Instead, every step has a **compensating action** that undoes it in business terms: Payment **refunds** the card, Ordering **cancels** the order. The saga runs these compensations in reverse to bring the system back to a consistent state. So it's compensation, not rollback, and the services stay independent.

There are two ways to coordinate a saga. **Choreography**: each service listens to events from the others and reacts, with no central coordinator. **Orchestration**: one orchestrator, for example a MassTransit state machine, tells each service what to do next.

The course explains this flow; EShop's code itself uses a single event for checkout and doesn't implement a saga.

---

## Slide 28: Part 4 divider  `[37:45]`

Last topic: the API gateway, and the Backends for Frontends pattern.

## Slide 29: The role of an API Gateway  `[39:15]`

Without a gateway, every client must know every service's address, and every service must handle authentication, SSL and rate limiting on its own.

A gateway is a **single entry point** between clients and services, and it does three jobs. **Gateway routing**: send each request to the right internal service. If we move or split a service, only the gateway config changes, not the clients. **Gateway aggregation**: combine several service calls into one response, so a mobile app makes one call instead of five. And **gateway offloading**: handle cross-cutting concerns in one place.

On the right is the pipeline a request passes through: validation, IP allowlisting, authentication, rate limiting, service discovery, and protocol conversion, before it reaches a service.

The risk: one gateway for everything can become a **bottleneck** and a **single point of failure**. Which leads to the next pattern.

## Slide 30: Backends for Frontends  `[40:30]`

**Backends for Frontends**, or BFF, means **one gateway per type of client**, instead of one gateway for everyone.

Why? Different clients need different things. A phone screen needs less data than a desktop page, in a different shape. If one gateway serves everyone, it fills up with client-specific logic and becomes a bottleneck that every team has to change. With BFF, the web team owns the web gateway, the mobile team owns the mobile gateway, and each returns exactly what its frontend needs.

EShop has only one client, Shopping.Web, so one gateway is enough. You'd add BFFs when, say, a mobile app arrives.

## Slide 31: Introduction to YARP  `[42:00]`

EShop implements its gateway with **YARP**, which stands for "Yet Another Reverse Proxy". It's a reverse-proxy library from Microsoft, built on ASP.NET Core. It supports HTTP/2, gRPC and WebSockets, load balancing and health checks, and it plugs into normal ASP.NET Core middleware, like the rate limiter.

The code on the left is the *entire* EShop gateway: add the reverse proxy, load its config, add a rate limiter allowing 5 requests per 10 seconds, and map the proxy.

On the right is the config. A **route** matches a path, like `/catalog-service/` plus anything, and points to a **cluster**. The **transform** removes the prefix. The cluster says where the service lives: `catalog.api:8080`. So `/catalog-service/products` becomes `catalog.api:8080/products`. And the ordering route also applies the rate limiting policy.

## Slide 32: Ocelot, and YARP vs. Ocelot  `[44:00]`

The other well-known .NET option is **Ocelot**, an open-source API gateway library configured with an `ocelot.json` file. EShop doesn't use it, so at the bottom of the slide I've written the same Catalog route for Ocelot, just for comparison. An **upstream** path is what the client calls; a **downstream** path and host are where Ocelot forwards it. The startup code is just `AddOcelot` and `UseOcelot`.

How do they compare? Let's go through the table row by row.

**From**: YARP is built by Microsoft; Ocelot is an open-source community project.

**What it is**: YARP is a reverse-proxy **toolkit**. It gives you fast building blocks that you customize in C#. Ocelot is a ready-made **API gateway**: you mostly configure it rather than code it.

**Config**: YARP uses **routes and clusters**, in `appsettings.json` or in code. Ocelot uses **upstream and downstream** routes in its own `ocelot.json` file.

**Built in**: YARP gives you routing, load balancing, health checks and transforms. Ocelot gives you routing plus rate limiting, authentication, caching and request aggregation.

**Extra features**: with YARP, you add them through standard ASP.NET Core middleware, like the rate limiter we saw on the previous slide. With Ocelot, you mostly switch them on in its own config.

And **in EShop**: YARP is used, Ocelot isn't.

Both implement the gateway routing pattern. YARP is the one this course uses.

---

## Slide 33: Summary  `[45:15]`

Let's wrap up.

**Microservices** are small, autonomous services built around business capabilities, each with its own data and deployment. Adopt them only for a real reason.

**DDD** tells us **where to split**, using bounded contexts and a ubiquitous language, and **how to model** each service, with entities, value objects and aggregates.

For **synchronous** communication, **REST** for client-facing APIs and **gRPC with Protobuf** for fast internal calls. For **asynchronous** communication, **events** through a **message broker** with pub/sub, for decoupled workflows. And an **API gateway**, YARP or Ocelot, as the single front door, with **BFF** when you have several kinds of clients.

## Slide 34: References  `[45:30]`

The course, the slides and all the code are listed here, from section 4 up to section 20, the complete system. For DDD, Eric Evans' book is the original, and Microsoft's free e-book on .NET microservices architecture is the best next step.

## Slide 35: Questions  `[60:00]`

That's all from me. Thank you! I'm happy to take your questions.

---

## Likely questions and short answers

**"Is a bounded context always one microservice?"**
Usually, but not always. A bounded context is a logical boundary in the model; a microservice is a deployment boundary. One bounded context can be split into several services if parts need to scale differently. The reverse, one service spanning several bounded contexts, is a warning sign.

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
