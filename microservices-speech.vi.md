# Kịch bản thuyết trình: Microservices với .NET 8 (60 phút)

Đi kèm với `microservices-slides.md`. Mỗi mục bên dưới tương ứng với một slide. Con số trong ngoặc vuông là **mốc đồng hồ mục tiêu khi bạn rời slide đó**, để bạn kiểm tra tốc độ của mình trong lúc nói.

Giả định: nói với tốc độ vừa phải, có ngắt nghỉ ngắn ở các slide code và diagram. Phần lời nói tương đương bản tiếng Anh (khoảng 44 phút); phần demo và các lúc ngắt nghỉ lấp đầy thời gian còn lại. Bản tiếng Việt chưa được bấm giờ thực tế, nên hãy đọc thử một lượt và điều chỉnh các mốc nếu cần. Các dòng *in nghiêng* là chỉ dẫn sân khấu, không đọc thành lời.

Quy ước: thuật ngữ kỹ thuật, tên pattern, tên class/method và tên slide giữ nguyên tiếng Anh để khớp với slide và code.

---

## Timing plan

| Phần | Slides | Thời lượng | Đồng hồ khi kết thúc |
|---|---|---|---|
| Opening | 1–2 | 2:00 | 2:00 |
| Part 1: Introduction to Microservices & DDD | 3–16 | 16:00 | 18:00 |
| Part 2: Building a microservice (ngắn) | 17–20 | 5:00 | 23:00 |
| Part 3: Synchronous communication (REST & gRPC) | 21–27 | 10:00 | 33:00 |
| Part 4: Asynchronous communication & message brokers | 28–34 | 10:00 | 43:00 |
| Part 5: API Gateway & BFF | 35–40 | 7:00 | 50:00 |
| Demo | 41–42 | 6:00 | 56:00 |
| Summary và Q&A | 43–45 | 4:00 | 60:00 |

**Nếu bạn đang bị trễ giờ**, có thể rút gọn các slide sau xuống còn một câu mà không mất mạch câu chuyện:
- Slide 8 (benefits and challenges): "Lợi ích là tốc độ và khả năng scale độc lập; cái giá phải trả là độ phức tạp, các network call, và việc giữ consistency khó hơn."
- Slide 9 (when to use): "Chỉ dùng khi có lý do thực sự, không bao giờ dùng khi thiếu DevOps, và không bao giờ cho các service dùng chung database."
- Slide 20 (containerize): "Mỗi service được đóng gói thành một Docker image, và docker-compose chạy tất cả cùng nhau."
- Slide 33 (messaging setup): "Một extension method dùng chung cấu hình MassTransit cho mọi service."
- Slide 40 (choosing a method): slide này nhắc lại Part 3–5, có thể bỏ qua.

**Nếu đến lúc demo mà bạn trễ hơn 2 phút**, bỏ lệnh đầu tiên (store basket) và bước đọc lại basket, chỉ chạy checkout và danh sách order.

---

## Trước buổi nói (15 phút trước giờ bắt đầu)

*Làm xong trước khi mọi người đến. Tuyệt đối không khởi động Docker trực tiếp trên sân khấu: chỉ riêng việc pull image SQL Server và RabbitMQ đã có thể mất vài phút.*

1. Khởi động các container từ thư mục `section20`:
   `docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --build`
2. Chạy `docker ps` và xác nhận cả 11 container đang chạy.
3. Làm nóng (warm up) các service để lần gọi demo đầu tiên không bị chậm: gọi `http://localhost:6004/catalog-service/products` một lần.
4. Dọn dữ liệu của lần chạy thử trước: `curl -X DELETE http://localhost:6004/basket-service/basket/swn`
5. Mở sẵn các cửa sổ/tab sau, mỗi thứ một tab hoặc một cửa sổ riêng:
   - Một terminal với ba lệnh demo đã sẵn sàng (hoặc Postman với `EShopMicroservices.postman_collection.json` đã import)
   - Một terminal thứ hai trong `section20`, sẵn sàng chạy `docker-compose logs -f discount.grpc ordering.api`
   - RabbitMQ UI tại http://localhost:15672 (đăng nhập guest / guest), đang ở tab **Exchanges**
6. **Phương án dự phòng:** chụp screenshot từng bước demo khi mọi thứ đang chạy tốt. Nếu Docker lỗi trên sân khấu, chiếu screenshot và tiếp tục nói.

---

## Slide 1: Title  `[1:00]`

hnay e xin thuyết trình về microservice. Mình sẽ đi qua bốn phần. Một: microservices là gì, và Domain-Driven Design giúp mình quyết định nên tách hệ thống ở đâu. Hai: các service giao tiếp với nhau theo kiểu synchronous như thế nào, với REST và gRPC. Ba: giao tiếp asynchronous, với event và message broker. Và bốn: API gateway đứng phía trước tất cả.

## Slide 2: Agenda  `[2:15]`

Đây là kế hoạch của buổi hôm nay.

Mình bắt đầu với về microservices và Domain-Driven Design: monolith so với microservices, các design principle, và các ý tưởng của DDD như bounded context, entity và value object.

Sau đó là về việc cần gì để xây một microservice.

Tiếp theo là synchronous communication, asynchronous communication, cho API gateway và pattern Backends for Frontends.

Rồi một live demo ngắn, trong đó một request checkout duy nhất sẽ dùng đến mọi thứ mình đã nói. Và mình sẽ kết thúc bằng phần tóm tắt và câu hỏi của mọi người. Nếu có chỗ nào chưa rõ, mọi người cứ hỏi ngay nhé.

---

## Slide 3: Part 1 divider  `[2:30]`

Mình bắt đầu từ bức tranh tổng thể: microservices là gì, và mình thiết kế chúng như thế nào?

## Slide 4: Architecture design journey  `[3:30]`

Microservices chỉ là một bước trong hành trình gồm bốn kiểu kiến trúc.

Ngoài cùng bên trái là **monolith** cổ điển: một application với một database. Tiếp theo là **modular monolith**. Nó vẫn là một deployment, nhưng được chia thành vài phần lớn, có thể hiểu là các **macroservices**. Sau đó là **microservices**: nhiều service nhỏ, và mỗi service sở hữu một database riêng. Và ngoài cùng bên phải là **serverless**, nơi các đơn vị trở thành từng function riêng lẻ, đôi khi được gọi là **nanoservices**.

Càng đi sang phải, các mảnh càng nhỏ và số lượng database càng tăng. Đây là một phổ (spectrum), không phải một cái thang mà bên phải luôn tốt hơn.

Vậy nên chọn cái nào? **Tùy vào project của bạn.** Nhiều hệ thống dùng **mixed architecture** và lên kế hoạch cho các **transition** khi hệ thống lớn dần. Cả Martin Fowler và Sam Newman đều khuyên nên bắt đầu với **monolith first**, rồi tách từng service ra khi có nhu cầu thực sự.

## Slide 5: What are microservices?  `[4:45]`

Vậy chính xác thì microservice là gì? Sáu ý.

Chúng **nhỏ, độc lập, và loosely coupled**. Mỗi service có **codebase riêng**, do một **team nhỏ** sở hữu. Chúng chỉ giao tiếp với nhau qua các **API được định nghĩa rõ ràng**: HTTP, gRPC, hoặc message. Mỗi service được **deploy độc lập**: mình có thể release một Basket service mới mà không đụng đến Ordering. Chúng **technology agnostic**, tức là không phụ thuộc vào một công nghệ cụ thể. Và mỗi service có **database riêng**, không chia sẻ với ai.

Câu trích dẫn là từ bài viết gốc của Martin Fowler: "một bộ các service nhỏ, mỗi service chạy trong process riêng của nó và giao tiếp với nhau bằng các lightweight mechanism." Nếu hôm nay mọi người chỉ nhớ một câu, hãy nhớ câu này.

## Slide 6: Monolith vs. microservices  `[6:00]`

Giờ mình so sánh trực tiếp hai bên.

Về **scalability**, monolith scale như một khối; microservices scale từng service riêng. Vào ngày Black Friday, mình có thể chỉ scale Basket và Ordering. Về **data**, một database dùng chung so với mỗi service một database. Về **failure**, một bug có thể làm sập cả monolith, trong khi microservices cô lập lỗi (fault isolation).

Hình vẽ ở dưới minh họa **deployment**. Với monolith, ba team cùng commit vào **một code base duy nhất** với **một pipeline**, nên mỗi lần release đều phải phối hợp. Với microservices, mỗi team có **code riêng, pipeline riêng, và service riêng**, và release khi nào sẵn sàng thì release.

Không bên nào là "tốt hơn". Với nhiều hệ thống, monolith là một lựa chọn hoàn toàn hợp lý.

Và nhiều hệ thống microservices thành công ban đầu cũng là một monolith. Monolith giúp team hiểu ranh giới thật sự nằm ở đâu, và *sau đó* họ mới tách.

## Slide 7: Design principles  `[7:30]`

Nếu đã chọn microservices, sáu nguyên tắc này giúp mình tránh được rắc rối.

**Một: single responsibility.** Mỗi service tương ứng với một **business capability**, như Catalog hay Ordering, chứ không phải một tầng kỹ thuật như "database service".

**Hai: loose coupling, high cohesion.** Mình có thể thay đổi một service mà không phải đổi các service khác, và mọi thứ về một capability nằm cùng một chỗ.

**Ba: autonomy.** Mỗi service tự deploy và **sở hữu dữ liệu của nó**.

**Bốn: well-defined contracts.** Các service chỉ giao tiếp qua API và event. Fowler gọi đây là "smart endpoints and dumb pipes": logic nằm trong các service, còn network chỉ làm nhiệm vụ chuyển dữ liệu.

**Năm: design for failure.** Trong một distributed system, lúc nào cũng có chỗ nào đó đang hỏng. Mình dùng timeout, retry và circuit breaker để một service lỗi không kéo sập các service còn lại.

**Sáu: automate và observe.** CI/CD, container, health check và centralized logging không phải là phần thêm tùy chọn; thiếu chúng, một đống service sẽ không thể quản lý nổi.

## Slide 8: Benefits and challenges  `[8:45]`

Đây là cả hai mặt, nói thẳng thắn.

Các **benefit**: agility, vì các team nhỏ có thể ship mà không cần phối hợp một đợt release khổng lồ; **independent scaling**; codebase nhỏ; deploy độc lập; chọn đúng công cụ cho đúng việc; và **fault isolation**: nếu Discount bị crash, khách hàng vẫn xem được sản phẩm.

Các **challenge** thì có thật. Toàn hệ thống **phức tạp** hơn. Mỗi lời gọi bây giờ là một **network call** có thể chậm hoặc thất bại. End-to-end testing khó hơn. **Data consistency** khó hơn, vì không còn một database transaction duy nhất bao trùm nhiều service. Và debug xuyên qua nhiều service rất khó.

Nên microservices không xóa bỏ độ phức tạp. Chúng **chuyển** nó, từ bên trong code sang giữa các service.

## Slide 9: When to use, and when not to  `[9:45]`

Hãy dùng chúng khi mình cần **zero-downtime independent deployment**, **independent scaling**, các data store khác nhau, hoặc có nhiều team tự chủ. Và quan trọng nhất, khi mình có một *lý do thật sự tốt*.

Hãy tránh khi mình sắp xây một **distributed monolith**: các service gắn chặt với nhau đến mức vẫn phải deploy cùng nhau. Như vậy mình phải chịu mọi sự phức tạp mà không được lợi ích nào. Cũng nên tránh khi chưa có **DevOps foundation**, khi chỉ có **team nhỏ**, khi là **startup mới** mà domain còn thay đổi liên tục, và đừng bao giờ để các service **dùng chung database**.

## Slide 10: Database-per-service  `[10:45]`

Điểm cuối cùng đó là quy tắc quan trọng nhất: **database per service**. Dữ liệu của một service chỉ được truy cập **thông qua API của nó**. Ordering không bao giờ được chạy SQL query vào database của Catalog. Chính điều đó cho phép Catalog đổi schema vào ngày mai mà không làm hỏng ai.

Một tác dụng phụ rất hay là **polyglot persistence**. Trong EShop, **Catalog** dùng PostgreSQL như một document database thông qua Marten. **Basket** dùng PostgreSQL cộng với **Redis** làm cache. **Discount** dùng SQLite. Và **Ordering** dùng **SQL Server**, vì order có quan hệ (relational) rất phong phú. Bốn service, bốn công nghệ lưu trữ, và như vậy hoàn toàn ổn.

## Slide 11: Introduction to DDD  `[12:15]`

Giờ, câu hỏi khó nhất trong microservices là: **mình vạch ranh giới giữa các service ở đâu?** Đó là chỗ Domain-Driven Design xuất hiện.

DDD được **Eric Evans** giới thiệu trong cuốn sách năm 2003 của ông. Nó không phải một công nghệ hay một framework. Nó là một cách **mô hình hóa phần mềm xoay quanh nghiệp vụ**. Ý tưởng cốt lõi rất đơn giản: chia một vấn đề phức tạp thành các phần nhỏ hơn, và cho mỗi phần một model riêng.

DDD có hai nửa. **Strategic DDD** là bức tranh lớn: hiểu business domain, tìm ra các subdomain và bounded context, và vẽ ra mối quan hệ giữa chúng. Nó cho mình biết **tách ở đâu** thành các microservice. **Tactical DDD** làm việc bên trong một service: các pattern như entity, value object và aggregate. Nó cho mình biết **xây dựng** model của từng service **như thế nào**.

Mình sẽ đi qua cả hai: strategic trước, tactical sau.

## Slide 12: DDD core concepts  `[13:30]`

Năm khái niệm mà mọi cuộc trò chuyện về DDD đều nhắc đến.

**Domain** là vấn đề nghiệp vụ mà phần mềm giải quyết: với mình là e-commerce. **Subdomain** là một mảng chuyên môn bên trong nó: product catalog, shopping cart, discount, order.

**Ubiquitous language** là bộ từ vựng chung được dùng bởi các domain expert *và cả* trong code. Nếu phía business nói "order item" và "shipping address", thì code cũng gọi là `OrderItem` và `ShippingAddress`, chứ không phải "row" hay "data object". Cách này loại bỏ cả một lớp hiểu lầm.

**Bounded context** là một ranh giới, bên trong đó áp dụng một model và một ngôn ngữ. Trong EShop: Catalog, Basket, Discount, và Ordering.

Và **context map** cho thấy các context liên hệ với nhau thế nào. Với mình: Basket hỏi Discount qua gRPC, và Basket thông báo cho Ordering bằng một event.

## Slide 13: Bounded contexts → microservices  `[14:45]`

Trong thực tế, làm sao tìm ra các bounded context? Khóa học dùng một phương pháp đơn giản. Bắt đầu từ các **user story**. Tìm các **danh từ**, chúng trở thành entity, và các **động từ**, chúng trở thành use case. Rồi nhóm chúng lại.

"List và filter products" cho mình **Catalog**. "Put products in the cart" cho mình **Basket**. "Apply coupons" cho mình **Discount**. "Check out và create an order" cho mình **Ordering**.

Giờ hãy nhìn vào bullet đầu tiên, vì đó là trái tim của ý tưởng bounded context: **cùng một từ có thể mang nghĩa khác nhau trong các context khác nhau.** Trong Catalog, một `Product` có name, description, image, categories và price, vì việc của Catalog là hiển thị sản phẩm. Trong Ordering, một `Product` chỉ có ID, name và price, vì một order chỉ cần có vậy. Hai model, mỗi cái đúng trong ranh giới của riêng nó. Cố ép một model "Product" dùng chung cho tất cả mọi người chính là cách các monolith bị rối.

Một bounded context thường là một microservice, nhưng bạn có thể tách nhỏ hơn nữa nếu một phần cần scale khác đi.

## Slide 14: Entities, value objects, aggregates  `[16:00]`

Giờ đến tactical DDD: cách mô hình hóa bên trong một service. Ba building block.

**Entity** được định danh bằng **ID**. Hai order có dữ liệu giống hệt nhau vẫn là hai order khác nhau, vì ID khác nhau. Entity có **lifecycle**: một order được tạo, được thanh toán, được giao.

**Value object** không có ID. Nó được định nghĩa bởi **các giá trị** của nó. Hai address có cùng street, city và zip code là cùng một address. Và value object là **immutable**: mình không sửa một address, mà thay nó bằng một cái mới.

Trong Ordering service, `Order`, `OrderItem`, `Customer` và `Product` là entity. `Address`, `Payment`, `OrderName`, và thậm chí cả `OrderId` là value object.

**Aggregate** là một cụm các entity và value object được xem là một đơn vị consistency. Entity chính của nó là **aggregate root**, và đó là cổng vào duy nhất. Ở đây, `Order` là root. Không ai thêm order item trực tiếp; mình gọi `Order.Add`, và order tự bảo đảm các quy tắc của nó được giữ, ví dụ quantity và price phải dương.

## Slide 15: Entities and value objects in code  `[17:00]`

Đây là hình dạng của nó trong code Ordering.

Bên trái là base class `Entity`: một ID cộng với các audit field. `Order` kế thừa từ `Aggregate`, tức là một entity có thể raise domain event. Để ý rằng danh sách order item là **private**; bên ngoài chỉ nhận được một bản read-only. Cách duy nhất để thêm một item là method `Add`, method này validate quantity và price trước. Đó chính là aggregate root đang bảo vệ các quy tắc của nó.

Bên phải là các value object. `Address` là một C# **record**, nên hai address có cùng giá trị thì tự động bằng nhau. Nó được tạo qua một factory method tên là `Of`, method này validate input. Và `OrderId` là một **strongly typed ID**: thay vì truyền các GUID thô đi khắp nơi, nơi rất dễ nhầm order ID với customer ID, mỗi loại ID có một type riêng, và một GUID rỗng sẽ bị từ chối.

## Slide 16: The big picture  `[18:00]`

Và đây là cách toàn bộ hệ thống khớp với nhau. Đọc từ trái sang phải.

Đây là hệ thống tham chiếu cho phần còn lại của buổi nói, nên đáng để dành vài giây.

**Shopping.Web**, client, chỉ nói chuyện với **YARP API gateway**. Gateway route đến **Catalog**, **Basket** và **Ordering**, mỗi service có database riêng. Catalog và Basket dùng Vertical Slice architecture; Ordering dùng Clean Architecture với DDD model mà mình vừa xem.

Và để ý hai kết nối đặc biệt. **Basket gọi Discount qua gRPC**: synchronous. Và **Basket publish một event lên RabbitMQ**, **Ordering consume** nó: asynchronous. Hai mũi tên đó chính là Part 3 và Part 4 của buổi nói. Còn gateway phía trước tất cả là Part 5. Nên mọi thứ mình bàn từ giờ trở đi đều nằm đâu đó trên một diagram này.

*Nếu ai đó so sánh với slide "Big Picture" của khóa học: hình đó vẽ thêm một mũi tên từ gateway đến Discount. Trong code thật, config YARP chỉ route đến Catalog, Basket và Ordering.*

---

## Slide 17: Part 2 divider  `[18:15]`

Trước khi kết nối các service với nhau, mình xem nhanh một service trông như thế nào.

## Slide 18: From analysis to a running service  `[19:45]`

Mỗi service trong project này đều đi theo cùng năm bước. **Domain analysis**: model, use case, endpoint và data store. **Technical analysis**: architecture style, pattern và library. Sau đó **develop** với ASP.NET Core Minimal APIs, **containerize** với Dockerfile, và **orchestrate** với docker-compose.

Và mọi `Program.cs` đều có cùng một dáng năm bước: tạo builder, đăng ký service vào dependency injection, build, cấu hình pipeline, và run.

Bên phải là microservice nhỏ nhất có thể, TodoApi từ section 4. Nó đăng ký một in-memory database, map một endpoint GET và một endpoint POST, rồi chạy. Chỉ khoảng mười lăm dòng, và nó đã thỏa định nghĩa: process riêng, một responsibility, dữ liệu riêng, và một HTTP API.

## Slide 19: Catalog.API  `[21:30]`

Một service thật cần cấu trúc nhiều hơn. Catalog dùng **Vertical Slice Architecture**: mỗi feature một folder, như CreateProduct hay GetProductById, trong đó chứa endpoint và handler của nó. Muốn đổi một feature, mình chỉ mở một folder.

Bên phải là luồng của một request: **HTTP, endpoint, MediatR, handler, database.** Endpoint chỉ chuyển HTTP thành một query và gửi nó qua MediatR. Handler load product bằng Marten, và nếu không tìm thấy thì throw một exception mà global handler sẽ biến thành 404. Endpoint không biết gì về database, và handler không biết gì về HTTP.

Có hai pattern đang hoạt động ở đây. **CQRS**, Command Query Responsibility Segregation, nghĩa là đọc và ghi được xử lý riêng: `GetProductByIdQuery` là một query, nó chỉ đọc; `CreateProductCommand` là một command, nó thay đổi dữ liệu. Và **MediatR** là một in-process mediator: endpoint gửi một message, và MediatR tìm handler duy nhất cho nó. Một tác dụng phụ rất hay là MediatR có thể bọc mọi request trong các **pipeline behavior**. EShop dùng hai cái: một cái validate mọi command bằng FluentValidation, và một cái log mọi request. Nhờ vậy không handler nào phải lặp lại đoạn code đó.

## Slide 20: Containerize and orchestrate  `[23:00]`

Cuối cùng, mình đóng gói nó. Dockerfile có nhiều stage: build bằng image .NET SDK đầy đủ, rồi chỉ copy phần output đã publish sang image runtime nhỏ gọn. Image cuối cùng nhỏ hơn và không chứa source code.

Bên phải, docker-compose chạy service cạnh database của nó, với environment variable, một startup dependency, và một port mapping. Bên trong Docker, các service tìm nhau **bằng tên**: gateway gọi `http://catalog.api:8080`. EShop chạy mười một container theo cách này.

Port mapping `6000:8080` nghĩa là: port 8080 bên trong container có thể truy cập qua port 6000 trên laptop của mình. Mọi service đều listen ở 8080 bên trong container, và có một port riêng trong dải 6000 ở bên ngoài. Mười một container gồm năm backing service, hai database PostgreSQL, Redis, SQL Server và RabbitMQ, cộng với sáu application. Một lệnh duy nhất khởi động tất cả, và đó cũng là cách demo sẽ chạy.

Giờ mình đã có các service. Hãy kết nối chúng lại.

---

## Slide 21: Part 3 divider  `[23:15]`

Phần ba: synchronous communication.

## Slide 22: Service-to-service communication  `[24:45]`

Các service phải hợp tác với nhau theo hai cách: **xin dữ liệu**, như "discount của sản phẩm này là bao nhiêu?", hoặc **kích hoạt công việc**, như "tạo order này".

Có hai kiểu. **Synchronous**: client gửi request và **chờ**. HTTP với REST, gRPC, GraphQL. Cái bẫy là **temporal coupling**: cả hai service phải đang chạy cùng một lúc.

**Asynchronous**: client gửi một message và **không chờ**. Message đi đến một broker, dùng một protocol như AMQP. Nếu bên nhận đang down, broker giữ message lại.

EShop dùng cả hai. Basket hỏi Discount **synchronous bằng gRPC**, vì nó cần giá ngay lúc đó. Web app gọi các API **bằng REST**. Còn Basket báo cho Ordering về một lần checkout **asynchronous**.

Quy tắc ngón tay cái: **REST cho public API, gRPC cho các lời gọi nội bộ, messaging cho các workflow xuyên nhiều service.**

## Slide 23: REST over HTTP  `[25:45]`

REST là thứ quen thuộc nhất: các resource URL như `GET /products` và `POST /basket`, với JSON mà client nào cũng hiểu.

Gọi REST bằng `HttpClient` thuần nghĩa là code lặp đi lặp lại: dựng URL, serialize, gửi, deserialize. Shopping.Web tránh điều đó bằng **Refit**. Mình viết một **interface**, đặt route lên từng method dưới dạng attribute, và Refit tự sinh ra phần implementation. Đăng ký bằng `AddRefitClient`, trỏ nó vào gateway, và trong một Razor page mình chỉ cần gọi `GetProduct(id)` như một method C# bình thường.

## Slide 24: Introduction to Protobuf and gRPC  `[27:30]`

Giờ đến gRPC, một remote procedure call framework của Google. Nó chạy trên **HTTP/2**, và client gọi một remote method **như thể đó là một object cục bộ**.

Contract được viết bằng **Protocol Buffers**, trong một file `.proto`. Bên phải là contract thật của Discount. Một block `service` liệt kê các method: GetDiscount, Create, Update và Delete. Các block `message` định nghĩa dữ liệu: một `GetDiscountRequest` với product name, và một `CouponModel` với ID, name, description và amount.

Hai điều mà mọi người hay bỏ sót. Thứ nhất, các con số như `= 1` và `= 2` là **field number** dùng trong binary encoding, không phải giá trị mặc định. Một khi đã dùng thì không được tái sử dụng hay đổi chúng. Thứ hai, Protobuf là **binary**: nhỏ hơn và parse nhanh hơn JSON rất nhiều.

Và trong .NET, mình không phải viết code networking. Quá trình build **tự sinh** một server base class và một client class từ file `.proto`.

## Slide 25: gRPC server  `[28:45]`

Đây là phía server. Trong `Program.cs`: `AddGrpc`, database, và `MapGrpcService`.

`DiscountService` **kế thừa `DiscountProtoServiceBase`**, class được sinh ra từ file `.proto`. Mình chỉ cần override `GetDiscount`: tìm coupon theo product name, hoặc trả về một coupon "No Discount" với amount bằng không. Để ý là không có routing, không có JSON và không có status code; contract đã định nghĩa tất cả những thứ đó.

Cách xử lý lỗi cũng khác. Thay vì trả về HTTP 404, service throw một `RpcException` kèm một gRPC status code. Ví dụ, `DeleteDiscount` throw `StatusCode.NotFound` nếu coupon không tồn tại, còn `Create` và `Update` throw `InvalidArgument` khi request sai. Client nhận chúng dưới dạng các typed exception. Ba method còn lại, Create, Update và Delete, theo cùng một khuôn: map Protobuf message sang database model, lưu, rồi map ngược lại.

## Slide 26: gRPC client  `[30:30]`

Và phía client, trong Basket. Ba bước.

**Một**: project Basket include *cùng* file `.proto` đó, với `GrpcServices="Client"`, nên chỉ phần client được sinh ra. Contract được chia sẻ; code thì không.

Thực ra, project Basket thậm chí không copy file. Nó link thẳng đến file `.proto` của Discount, nên nếu contract thay đổi, cả hai phía sẽ thấy thay đổi ở lần build kế tiếp.

**Hai**: đăng ký một typed client bằng `AddGrpcClient` và đọc địa chỉ của Discount từ configuration. Handler phụ ở đó chấp nhận self-signed development certificate. Điều đó ổn trên laptop, nhưng đừng bao giờ ship nó lên production.

**Ba**: inject client và gọi. Với mỗi cart item, gọi `GetDiscountAsync` với product name, rồi trừ đi amount của coupon. Nó đọc như một lời gọi method cục bộ, nhưng thật ra là một binary HTTP/2 request đến một container khác.

## Slide 27: When to use REST, when to use gRPC  `[33:00]`

Vậy khi nào chọn cái nào? Mình đi từng hàng một.

Cả hai đều có thể chạy trên HTTP/2, nhưng gRPC thì bắt buộc. REST gửi **text JSON**; gRPC gửi **binary Protobuf**, nhỏ hơn và nhanh hơn. Với REST, contract là tùy chọn, ví dụ OpenAPI; với gRPC nó là **bắt buộc**, và code được sinh ra từ đó. **Browser** nói REST một cách tự nhiên, còn gRPC cần gRPC-Web hoặc một proxy. gRPC hỗ trợ **streaming** hai chiều; REST là request và response. Và REST dễ test bằng tay với curl hoặc browser; gRPC cần công cụ riêng.

Vậy: **chọn REST** cho public API, browser, bên thứ ba và CRUD đơn giản. **Chọn gRPC** cho các lời gọi nội bộ, khối lượng lớn, độ trễ thấp giữa các service của chính mình.

Đó chính xác là cách EShop chia: REST phía sau gateway cho web app, và gRPC cho Basket gọi Discount, nơi được gọi một lần cho mỗi item trong mỗi giỏ hàng.

Hãy nghĩ xem điều đó nghĩa là gì khi tính thành số. Một giỏ hàng có mười item nghĩa là mười lời gọi Discount mỗi lần basket được lưu, cho mọi user. Với gRPC, những lời gọi đó dùng chung một kết nối HTTP/2 và mỗi lời gọi chỉ gửi vài byte binary, thay vì mở request riêng và parse JSON. Đó là chỗ gRPC phát huy giá trị.

Và đây không phải là lựa chọn một trong hai cho cả hệ thống. Hầu hết hệ thống thực tế làm đúng như EShop: REST ở rìa, nơi browser và đối tác kết nối, và gRPC ở bên trong, giữa các service mình sở hữu. ASP.NET Core thậm chí có thể expose một gRPC service dưới dạng REST luôn, bằng một tính năng gọi là JSON transcoding, nếu mình cần cả hai.

---

## Slide 28: Part 4 divider  `[33:15]`

Phần bốn: asynchronous communication, và vì sao nó quan trọng.

## Slide 29: Basic event-driven architecture  `[35:00]`

Bắt đầu với vấn đề ở bên trái. Hãy tưởng tượng tạo một order mà chỉ dùng các lời gọi synchronous. Gateway gọi ShoppingCart. ShoppingCart gọi Product và Pricing, rồi gọi Order. Order gọi Billing và Shipping. Sáu lời gọi chỉ cho một cú click. Mọi service đều phải đang chạy, độ trễ cộng dồn lại, và nếu Shipping chậm thì tất cả đều chậm.

Event-driven architecture đảo ngược điều này. Một **event** là **một sự thật đã xảy ra**, được đặt tên ở thì quá khứ: *BasketCheckedOut*, *OrderCreated*. **Producer** publish nó rồi đi tiếp. Nó **không biết** ai đang lắng nghe. Các **consumer** phản ứng theo nhịp của riêng mình.

Và một sự phân biệt hữu ích: một **command** bảo một service cụ thể phải làm gì, như "CreateOrder". Một **event** chỉ thông báo điều gì đã xảy ra, cho bất kỳ ai quan tâm. Command ràng buộc mình với bên nhận; event thì không.

Một ví dụ đời thường: command giống như gọi điện cho một người, nhờ họ làm một việc. Event giống như dán thông báo lên bảng tin: "basket đã được checkout". Ai quan tâm thì đọc và hành động. Người dán thông báo không cần biết ai đã đọc, thậm chí không cần có mặt lúc họ đọc.

Trong EShop, checkout là event. Basket không gọi Ordering rồi bảo "hãy tạo một order". Nó thông báo "basket này đã được checkout", và Ordering tự quyết định điều đó có nghĩa gì với nó.

## Slide 30: Domain vs. integration events  `[36:30]`

Trong các hệ thống DDD, có hai loại event.

Một **domain event** ở lại **bên trong** một service. Nó được raise bởi một aggregate và được giao ngay trong process, trong bộ nhớ. Trong EShop, khi `Order.Create` chạy, aggregate `Order` raise một `OrderCreatedEvent`, và MediatR giao nó cho một handler trong cùng service.

Một **integration event** đi **giữa** các service, thông qua một message broker. Trong EShop, đó là `BasketCheckoutEvent`, từ Basket đến Ordering, qua RabbitMQ.

Diagram cho thấy chúng nối chuỗi với nhau thế nào. Integration event đến Ordering, handler tạo một order, order raise một domain event, và handler của domain event đó có thể publish một integration event mới, `OrderCreatedIntegrationEvent`, cho các service khác như shipping. Trong project này, bước cuối đó nằm sau một feature flag và đang tắt.

## Slide 31: The role of a message broker  `[38:15]`

Vậy broker ở giữa thực sự làm gì? Nó **nhận** message, **lưu** chúng cho đến khi consumer sẵn sàng, kể cả khi consumer đang down, **route** chúng đến đúng các queue, và **giao và retry**. Nó tách rời (decouple) các service về **thời gian**, vì chúng không cần chạy cùng lúc, và về **vị trí**, vì producer không cần biết địa chỉ của consumer.

**RabbitMQ** là một broker open-source implement protocol AMQP. Khái niệm then chốt: producer không gửi trực tiếp vào queue. Chúng gửi đến một **exchange**, và exchange route đến các queue dựa trên loại của nó: **direct** theo key chính xác, **fanout** đến tất cả mọi người, **topic** theo pattern, hoặc **headers**.

Bên trên RabbitMQ, EShop dùng **MassTransit**, một thư viện .NET tạo exchange và queue, đồng thời xử lý serialization và retry. Nên code của mình nghĩ theo event, không phải theo exchange.

RabbitMQ cũng có sẵn một management web UI, ở port 15672 trong setup của mình. Nó hiển thị mọi exchange, queue và message rate, và mình sẽ dùng nó trong demo để cho thấy event của mình đi qua.

## Slide 32: Basic pub/sub pattern  `[39:45]`

Cách dùng broker phổ biến nhất là **publish/subscribe**.

Basket publish **một** `BasketCheckoutEvent` đến một exchange. Broker **fan out** một bản sao đến queue của từng subscriber. Hôm nay trong EShop chỉ có một subscriber, là Ordering. Nhưng ngày mai mình có thể thêm một Notification service để gửi email, và một Analytics service để ghi nhận doanh số, **mà không phải sửa một dòng nào trong Basket**. Đó là sức mạnh thật sự của pub/sub.

So sánh với một queue thông thường: queue giao mỗi message cho **một** consumer; pub/sub cho **mỗi** subscriber một bản sao riêng.

Bên phải là các cái giá phải trả. Broker thường giao **at least once**, nên một message có thể đến hai lần, và consumer phải **idempotent**: xử lý hai lần thì vẫn phải an toàn. Thứ tự không được bảo đảm. Broker trở nên quan trọng sống còn, nên phải có high availability. Và có **eventual consistency**: order xuất hiện sau checkout một lúc, không phải ngay lập tức.

## Slide 33: Messaging setup  `[41:00]`

Đây là cách EShop thiết lập việc này. Bên trái là **contract**: một base record `IntegrationEvent`, và `BasketCheckoutEvent` với user name, total price, address và payment. Cả Basket và Ordering đều reference cùng class này.

Bên phải là một extension method dùng chung, `AddMessageBroker`. Nó cấu hình MassTransit, đăng ký các consumer từ một assembly nếu có, và kết nối đến RabbitMQ với các setting lấy từ configuration. Mọi service đều gọi đúng một method này.

Hai chi tiết. `ConfigureEndpoints` bảo MassTransit tạo một queue cho mỗi consumer mà nó tìm thấy, và bind nó vào đúng exchange, nên mình không bao giờ phải khai báo queue bằng tay. Và địa chỉ broker lấy từ configuration: chạy local là `amqp://localhost:5672`, còn bên trong Docker là `amqp://ecommerce-mq:5672`, được set bằng environment variable trong docker-compose. Cùng một code, khác môi trường.

## Slide 34: Publish and consume  `[43:00]`

Và cả hai đầu của luồng checkout.

Bên trái là **publisher** trong Basket. Checkout handler lấy một `IPublishEndpoint` từ MassTransit. Nó load basket, map dữ liệu checkout thành một `BasketCheckoutEvent`, set total price, rồi gọi **`Publish`**. Sau đó nó xóa basket và trả về success, **ngay lập tức**. Nó không chờ một order.

Bên phải là **consumer** trong Ordering. Nó implement **`IConsumer<BasketCheckoutEvent>`**. Khi một message đến, `Consume` log nó, map thành một `CreateOrderCommand`, và gửi qua MediatR, nơi order được tạo ra.

Để ý: **Basket không hề biết Ordering tồn tại.** Nó chỉ publish một event. Đó là loose coupling trong thực tế.

Có một câu hỏi đáng đặt ra về handler này: nếu `Publish` thành công, nhưng việc xóa basket thất bại thì sao? Hoặc ngược lại? Ghi vào database và publish một message là hai thao tác riêng biệt, và cái nào cũng có thể thất bại. Đó gọi là **dual write problem**. Cách khắc phục chuẩn là **Outbox pattern**: lưu event trong cùng database transaction với dữ liệu, rồi publish sau. Khóa học sẽ đề cập sau; hôm nay mọi người chỉ cần biết vấn đề này tồn tại.

---

## Slide 35: Part 5 divider  `[43:15]`

Chủ đề cuối cùng: API gateway, và pattern Backends for Frontends.

## Slide 36: The role of an API Gateway  `[44:45]`

Không có gateway, mỗi client phải biết địa chỉ của mọi service, và mỗi service phải tự xử lý authentication, SSL và rate limiting.

Gateway là một **điểm vào duy nhất** giữa client và các service, và nó làm ba việc. **Gateway routing**: gửi mỗi request đến đúng service nội bộ. Nếu mình di chuyển hoặc tách một service, chỉ có config của gateway thay đổi, không phải client. **Gateway aggregation**: gộp nhiều lời gọi service thành một response, để một mobile app chỉ gọi một lần thay vì năm lần. Và **gateway offloading**: xử lý các cross-cutting concern ở một nơi.

Bên phải là pipeline mà một request đi qua: validation, IP allowlisting, authentication, rate limiting, service discovery, và protocol conversion, trước khi đến một service.

Rủi ro: một gateway cho tất cả mọi thứ có thể trở thành **bottleneck** và **single point of failure**. Điều đó dẫn đến pattern tiếp theo.

## Slide 37: Backends for Frontends  `[46:00]`

**Backends for Frontends**, hay BFF, nghĩa là **một gateway cho mỗi loại client**, thay vì một gateway cho tất cả mọi người.

Tại sao? Vì các client khác nhau cần những thứ khác nhau. Màn hình điện thoại cần ít dữ liệu hơn trang desktop, và ở một hình dạng khác. Nếu một gateway phục vụ tất cả, nó sẽ đầy logic riêng của từng client và trở thành bottleneck mà team nào cũng phải sửa. Với BFF, web team sở hữu web gateway, mobile team sở hữu mobile gateway, và mỗi cái trả về đúng những gì frontend của nó cần.

EShop chỉ có một client, Shopping.Web, nên một gateway là đủ. Mình sẽ thêm BFF khi, ví dụ, có một mobile app xuất hiện.

## Slide 38: Introduction to YARP  `[47:30]`

EShop implement gateway của nó bằng **YARP**, viết tắt của "Yet Another Reverse Proxy". Đây là một reverse-proxy library của Microsoft, xây trên ASP.NET Core. Nó hỗ trợ HTTP/2, gRPC và WebSockets, load balancing và health check, và cắm vào các middleware ASP.NET Core thông thường, như rate limiter.

Đoạn code bên trái là *toàn bộ* gateway của EShop: thêm reverse proxy, load config của nó, thêm một rate limiter cho phép 5 request mỗi 10 giây, và map proxy.

Bên phải là config. Một **route** khớp với một path, như `/catalog-service/` cộng với bất cứ thứ gì phía sau, và trỏ đến một **cluster**. **Transform** bỏ đi phần prefix. Cluster cho biết service nằm ở đâu: `catalog.api:8080`. Nên `/catalog-service/products` trở thành `catalog.api:8080/products`. Và route ordering còn áp dụng thêm rate limiting policy.

## Slide 39: Ocelot, and YARP vs. Ocelot  `[49:00]`

Lựa chọn nổi tiếng khác trong .NET là **Ocelot**, một API gateway library open-source được cấu hình bằng file `ocelot.json`. EShop không dùng nó, nên ở cuối slide mình viết cùng route Catalog đó theo kiểu Ocelot, chỉ để so sánh. Đường dẫn **upstream** là cái client gọi; đường dẫn và host **downstream** là nơi Ocelot forward đến. Code khởi động chỉ là `AddOcelot` và `UseOcelot`.

So sánh thế nào? **YARP** đến từ Microsoft và là một **toolkit** reverse-proxy: rất nhanh, tùy biến rất sâu bằng C#, và lấy thêm tính năng từ các middleware ASP.NET Core chuẩn. **Ocelot** là một project cộng đồng và là một **API gateway** làm sẵn: các tính năng như rate limiting, authentication, caching và request aggregation được bật trong file config riêng của nó.

Cả hai đều implement gateway routing pattern. YARP là cái mà khóa học này dùng.

## Slide 40: Choosing an invocation method  `[50:00]`

Giờ mình ráp tất cả lại. **REST** cho các API hướng client. **gRPC** cho các lời gọi nội bộ nhanh với contract chặt chẽ. **Messaging** cho các workflow xuyên service và để decoupling. Và **API gateway** làm cánh cửa trước duy nhất.

Hệ thống thực tế thường kết hợp chúng. Lời khuyên để mang về: giữ các chuỗi gọi synchronous **ngắn**, và dùng messaging cho bất cứ thứ gì không cần câu trả lời ngay.

Một phép thử đơn giản mà mọi người có thể dùng: người gọi có cần câu trả lời *ngay bây giờ* để tiếp tục không? Basket cần discount ngay lúc đó để hiển thị giá, nên đó là một synchronous call. Basket không cần số order để hoàn tất checkout, nên đó là một event.

Slide thế là đủ. Giờ xem nó chạy.

---

## Slide 41: Demo setup  `[51:00]`

*(Giữ phần này ngắn. Mọi thứ đã chạy sẵn.)*

Kế hoạch là: một lần checkout, ba lệnh. Thứ nhất, lưu một basket: REST qua gateway, rồi gRPC đến Discount. Thứ hai, checkout: Basket publish một event lên RabbitMQ, và Ordering consume nó. Thứ ba, liệt kê các order để xem kết quả.

Mình đã khởi động cả mười một container trước buổi nói bằng một lệnh docker-compose. Mọi thứ đi qua gateway ở port 6004.

Bên phải màn hình của mình là log của Discount và Ordering service, để mình thấy từng service phản ứng.

## Slide 42: Demo commands  `[56:00]`

*[Đảm bảo terminal log (`docker-compose logs -f discount.grpc ordering.api`) hiển thị cạnh terminal chạy lệnh.]*

Đầu tiên, mình lưu một basket cho user "swn" với một chiếc iPhone X giá 950.

*[Chạy POST đến `/basket-service/basket`.]*

Nhìn vào log: "Discount is retrieved for ProductName: IPhone X, Amount: 150." Dòng đó đến từ Discount service, vì **Basket vừa gọi nó qua gRPC**.

*[Chạy GET để lấy basket.]*

Và đọc lại basket, giá là **800**: 950 trừ coupon 150.

*Nếu giá vẫn là 950: nhiều khả năng lời gọi gRPC đã thất bại vì HTTPS certificate. Hãy nói "đây chính xác là loại lỗi mạng mà mình thiết kế để đối phó", chiếu screenshot, rồi đi tiếp.*

Giờ đến checkout.

*[Chạy POST checkout.]*

`isSuccess: true`, và nó trả về **ngay lập tức**. Basket không chờ Ordering; nó publish một event rồi trả về.

*[Chuyển sang RabbitMQ UI, tab Exchanges.]*

Trong RabbitMQ, đây là exchange mà MassTransit đã tạo, được đặt tên theo event type của mình, và message rate cho thấy message của mình đã đi qua.

*[Quay lại phần log.]*

Và trong log của Ordering, hai dòng: "Integration Event handled: BasketCheckoutEvent", đó là event từ Basket, đi qua RabbitMQ. Rồi "Domain Event handled: OrderCreatedEvent", đó là aggregate `Order` raise domain event của chính nó bên trong Ordering. Cả hai loại event từ Part 4, trong một luồng duy nhất.

*[Chạy GET để lấy danh sách orders.]*

Và đây là kết quả. ORD_1 và ORD_2 là seed data, còn cái cuối là order mới của mình, "swn". Hai service, hai database, chỉ nối với nhau bằng một event.

Một chi tiết cần nói thật: các item trong order này là các demo item cố định từ code của khóa học, không phải chiếc iPhone trong basket của mình. Event handler đánh dấu chỗ này là một TODO.

---

## Slide 43: Summary  `[57:15]`

Mình tóm tắt lại.

**Microservices** là các service nhỏ, tự chủ, xây quanh các business capability, mỗi service có dữ liệu và deployment riêng. Chỉ áp dụng khi có lý do thực sự.

**DDD** cho mình biết **tách ở đâu**, bằng bounded context và ubiquitous language, và **mô hình hóa** từng service **thế nào**, bằng entity, value object và aggregate.

Với giao tiếp **synchronous**, dùng **REST** cho các API hướng client và **gRPC với Protobuf** cho các lời gọi nội bộ nhanh. Với giao tiếp **asynchronous**, dùng **event** qua **message broker** với pub/sub, cho các workflow decoupled. Và một **API gateway**, YARP hoặc Ocelot, làm cánh cửa trước duy nhất, cùng **BFF** khi có nhiều loại client.

Và như mọi người đã thấy, một lần checkout đã dùng đến tất cả.

## Slide 44: References  `[57:30]`

Khóa học, các slide và toàn bộ code được liệt kê ở đây, từ section 4 đến section 20, là hệ thống hoàn chỉnh. Về DDD, cuốn sách của Eric Evans là bản gốc, và e-book miễn phí của Microsoft về kiến trúc .NET microservices là bước tiếp theo tốt nhất.

## Slide 45: Questions  `[60:00]`

Phần của mình đến đây là hết. Cảm ơn mọi người! Mình sẵn sàng nhận câu hỏi.

---

## Các câu hỏi có thể gặp và câu trả lời ngắn

**"Một bounded context có luôn là một microservice không?"**
Thường là vậy, nhưng không phải luôn luôn. Bounded context là một ranh giới logic trong model; microservice là một ranh giới deployment. Một bounded context có thể được tách thành nhiều service nếu các phần cần scale khác nhau. Chiều ngược lại, một service trải dài nhiều bounded context, là một dấu hiệu cảnh báo.

**"Tại sao tạo OrderId thành một type thay vì dùng thẳng Guid?"**
Đó là tránh cái gọi là "primitive obsession". Nếu mọi ID đều là `Guid`, rất dễ truyền nhầm customer ID vào chỗ cần order ID, và compiler không phát hiện ra. Với strongly typed ID, lỗi đó không compile được, và phần validation, như từ chối ID rỗng, nằm ở đúng một chỗ.

**"Làm sao giữ dữ liệu consistent khi không có shared database transaction?"**
Bằng eventual consistency và event. Muốn bảo đảm mạnh hơn thì có hai pattern. **Outbox pattern** lưu event trong cùng database transaction với dữ liệu, và một background process publish nó sau, giúp tránh "dual write problem". **Saga pattern** điều phối các business transaction nhiều bước, với các compensating action khi một bước thất bại. Cả hai đều được đề cập ở phần sau của khóa học.

**"Tại sao dùng gRPC cho Discount mà các service khác lại dùng REST?"**
Discount chỉ được các service khác gọi, mỗi cart item một lần, nên tốc độ và contract chặt chẽ quan trọng. Các service còn lại được một web app chạy trên browser gọi qua gateway, và browser làm việc tốt nhất với REST và JSON.

**"Chuyện gì xảy ra nếu RabbitMQ down trong lúc checkout?"**
Với code này, việc publish sẽ thất bại hoặc timeout, và request checkout sẽ trả về lỗi. Đó là lý do hệ thống production chạy broker dưới dạng cluster, và cũng là lý do Outbox pattern tồn tại.

**"Nếu Discount service down thì sao?"**
Khi đó lời gọi store basket sẽ thất bại, vì lời gọi gRPC là synchronous. Trên production mình sẽ thêm timeout, retry và circuit breaker, ví dụ với Polly, và có thể fallback về "không có discount".

**"Nên chọn YARP hay Ocelot?"**
Cả hai đều dùng được. YARP đến từ Microsoft, rất nhanh, và mình xây tính năng bằng các middleware ASP.NET Core chuẩn. Ocelot cho sẵn nhiều tính năng gateway hơn thông qua configuration. Với một project .NET mới, mình sẽ bắt đầu với YARP, như khóa học này.

**"Cái này chạy trên production như thế nào?"**
Cùng những container image đó chạy trên Kubernetes, như Azure AKS hoặc Amazon EKS, nơi bổ sung service discovery, scaling, restart dựa trên health check và rolling deployment. docker-compose chỉ dùng cho local development.
