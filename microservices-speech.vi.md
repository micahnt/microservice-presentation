## Slide 1: Title  `[1:00]`

hnay e sẽ thuyết trình về microservice. Mình sẽ đi qua bốn phần. Một: microservices là gì, và Domain-Driven Design giúp mình quyết định nên tách hệ thống ở đâu. Hai: các service giao tiếp với nhau theo kiểu synchronous như thế nào, với REST và gRPC. Ba: giao tiếp asynchronous, với event và message broker. Và bốn: API gateway đứng phía trước tất cả.

## Slide 2: Agenda  `[2:15]`
// coi lại
Đây là kế hoạch của buổi hôm nay.

Mình bắt đầu với về microservices và Domain-Driven Design: monolith so với microservices, các design principle, và ý tưởng bounded context của DDD.

Tiếp theo là synchronous communication, asynchronous communication, cho API gateway và pattern Backends for Frontends.

Và mình sẽ kết thúc bằng phần tóm tắt và câu hỏi của mọi người. Nếu có chỗ nào chưa rõ, mọi người cứ hỏi ngay nhé.

---

## Slide 3: Part 1 divider  `[2:30]`

Mình bắt đầu từ bức tranh tổng thể: microservices là gì, và mình thiết kế chúng như thế nào?

## Slide 4: Architecture design journey  `[3:30]`

Microservices là một bước trong journey gồm bốn kiểu kiến trúc.

Ngoài cùng bên trái là **monolith** : một application với một database. Tiếp theo là **modular monolith**. Nó vẫn là một deployment unit, nhưng được chia thành vài phần lớn, có thể hiểu là các **macroservices**. Sau đó là **microservices**: nhiều service nhỏ, và mỗi service sở hữu một database riêng. Và ngoài cùng bên phải là **serverless**, nơi các đơn vị trở thành từng function riêng lẻ, đôi khi được gọi là **nanoservices**.

Càng đi sang phải, (code & nghiệp vụ) càng đc phân nhỏ và số lượng database càng tăng.

Vậy nên chọn cái nào? **Tùy vào project.** Nhiều hệ thống dùng **mixed architecture** và lên kế hoạch cho các chuyển đổi khi hệ thống lớn dần. Cả Martin Fowler và Sam Newman đều khuyên nên bắt đầu với **monolith first**, rồi tách từng service ra khi có nhu cầu thực sự.

## Slide 5: What are microservices?  `[4:45]`

Vậy chính xác thì microservice là gì? Sáu ý.

- Chúng **nhỏ, độc lập, và loosely coupled**. 
- Mỗi service có **codebase riêng**, do một **team nhỏ** sở hữu. 
- Chúng chỉ giao tiếp với nhau qua các **API được định nghĩa rõ ràng**: HTTP, gRPC, hoặc message. - - Mỗi service được **deploy độc lập** (mình có thể release một Basket service mới mà không đụng đến Ordering.)
- Chúng **technology agnostic**, tức là không phụ thuộc vào một công nghệ cụ thể. 
- Và mỗi service có **database riêng**, không chia sẻ với ai.

(nchung là: "một bộ các service nhỏ, mỗi service chạy trong process riêng của nó và giao tiếp với nhau bằng các lightweight mechanism." )

## Slide 6: Monolith vs. microservices  `[7:00]`

Giờ mình so sánh trực tiếp hai bên.

Về **structure**, monolith là một khối duy nhất, một app và một database. Microservices là nhiều service khác nhau, mỗi service có database riêng, và có thể dùng công nghệ riêng.

Về **scalability**, monolith scale như một khối, nên muốn chịu thêm tải thì phải nhân bản cả app. Microservices scale từng service riêng. Vào ngày Black Friday, mình có thể chỉ scale Basket và Ordering.

Về **deployment**, đơn vị deploy khác nhau. Monolith là **một đơn vị deploy duy nhất**: sửa một dòng code cũng phải build và deploy lại toàn bộ application. Microservices là **nhiều đơn vị deploy**: mỗi service là một container riêng, có CI/CD pipeline riêng, và release độc lập. Hệ thống nhỏ có thể host các container đó trên **Azure Container Apps**; hệ thống lớn thường chạy chúng trên **Kubernetes**, ví dụ AKS trên Azure. Hình vẽ ở dưới minh họa điều này.

Một lưu ý: zero downtime không phải là tính năng riêng của microservices. Đó là chuyện vận hành, triển khai, chứ không phải chuyện kiến trúc. Một monolith đặt sau **load balancer** có thể chạy song song version 1 và version 2, rồi chuyển traffic sang version 2 mà không làm gián đoạn người dùng.

Về **data**, một database dùng chung so với mỗi service một database.

Về **failure**, một bug, ví dụ memory leak, có thể làm sập cả monolith, trong khi microservices cô lập lỗi trong một service (fault isolation).

Và về **typical fit**, monolith không phải là "dành cho team nhỏ"; nó tùy vào business domain. Monolith thường là lựa chọn cho dự án **MVP** hoặc dự án ban đầu, khi domain và các bounded context còn đang được tìm ra. Microservices phù hợp với các domain đã rõ ràng, ổn định, và cần một team quen với container, DevOps và monitoring.

Không bên nào là "tốt hơn". Với nhiều hệ thống, monolith là một lựa chọn hoàn toàn hợp lý. (vd Dự án mới khởi nghiệp (MVP), Team size tầm 15-20 người,...)

## Slide 7: Benefits and challenges  `[8:15]`

Đây là pros cons.

Các **benefit**: agility, vì các team nhỏ có thể ship mà không cần phối hợp một đợt release khổng lồ; **independent scaling**; codebase nhỏ; deploy độc lập; chọn đúng công nghệ cho đúng việc; và **fault isolation**: (nếu Discount bị crash, khách hàng vẫn xem được sản phẩm.)

Các **challenge** thì có thật. Toàn hệ thống **phức tạp** hơn. Mỗi call bây giờ là một **network call** có thể chậm hoặc thất bại. End-to-end testing khó hơn. **Data consistency** khó hơn (vì không còn một database transaction duy nhất bao trùm nhiều service). Và debug xuyên qua nhiều service rất khó.

Nên cơ bản là microservices không xóa bỏ độ phức tạp. Nó **chuyển** độ khó từ bên trong code sang giữa các service.

## Slide 8: When to use, and when not to  `[9:15]`

Hãy dùng microservice khi mình cần **release từng phần của hệ thống độc lập**, **independent scaling**, các data store khác nhau, có nhiều team tự chủ, hoặc là khi mình có một lý do (hệ thống Monolith hiện tại không thể chịu tải được nữa, team size quá lớn, mn giẫm chân nhau). Chỉ riêng zero downtime thì không phải là lý do: như vừa thấy, monolith đặt sau load balancer cũng deploy được mà không downtime.

Hãy tránh khi mình sẽ xây một **distributed monolith**: tức là microservice nhưng các service bị tight coupling đến mức vẫn phải deploy cùng nhau. Cũng nên tránh khi chưa có Devops, khi là dự án **MVP hoặc dự án ban đầu** mà các bounded context còn chưa rõ, và đừng bao giờ để các service **dùng chung database**.

## Slide 9: Design principles  `[10:45]`

Nếu đã chọn microservices, sáu nguyên tắc này giúp mình tránh bị distributed monolith.

**Một: single responsibility.** Mỗi service tương ứng với một **business capability**, như Catalog hay Ordering, chứ không phải một tầng kỹ thuật như "database service".

**Hai: loose coupling, high cohesion.** Mình có thể thay đổi một service mà không phải đổi các service khác, và mọi thứ về một capability nằm cùng một chỗ.

**Ba: autonomy.** Mỗi service tự deploy và có db riêng.

**Bốn: well-defined contracts.** Các service chỉ giao tiếp qua API và event. Fowler gọi đây là "smart endpoints and dumb pipes": logic nằm trong các service, còn network chỉ làm nhiệm vụ chuyển dữ liệu.

**Năm: design for failure.** Trong một distributed system, có thể lỗi ở chỗ nào đó bất cứ lúc nào. Mình dùng timeout, retry và circuit breaker để một service lỗi không kéo sập các service còn lại.

**Sáu: automate và observe.** nên có CI/CD, container, health check và centralized logging vì thiếu thì khó thể quản lý nổi một đống service.

## Slide 10: Introduction to DDD  `[12:15]`

Có 1 câu hỏi trong microservices là: **mình vạch ranh giới giữa các service ở đâu?** Đó là chỗ Domain-Driven Design xuất hiện.

DDD được **Eric Evans** giới thiệu trong cuốn sách năm 2003 của ông. Nó không phải một công nghệ hay một framework. Nó là một cách **mô hình hóa phần mềm xoay quanh nghiệp vụ**. Ý tưởng cốt lõi rất đơn giản: chia một vấn đề phức tạp thành các phần nhỏ hơn, và cho mỗi phần một model riêng (model ở đây là Data Model - Database Schema & Business Logic - Domain Logic).

DDD có hai nửa. **Strategic DDD** là bức tranh lớn: hiểu business domain, tìm ra các subdomain và bounded context, và vẽ ra mối quan hệ giữa chúng. Nó cho mình biết **tách ở đâu** thành các microservice. **Tactical DDD** làm việc bên trong một service: các pattern như entity, value object và aggregate. Nó cho mình biết **xây dựng** model của từng service **như thế nào**.

E sẽ chủ yếu đi qua phần strategic vì nó liên quan đến việc thiết kế kiến trúc microservices, phần Tactical liên quan đến implement/code

## Slide 11: DDD core concepts  `[13:30]`

Năm khái niệm mà mọi cuộc trò chuyện về DDD đều nhắc đến.

**Domain** là vấn đề nghiệp vụ mà phần mềm giải quyết: với mình là e-commerce. **Subdomain** là một mảng chuyên môn bên trong nó: product catalog, shopping cart, discount, order.

**Ubiquitous language** là bộ từ vựng chung được dùng bởi các domain expert *và cả* trong code. Nếu phía business nói "order item" và "shipping address", thì code cũng gọi là `OrderItem` và `ShippingAddress`, chứ không phải đặt biến tên khác. Cách này loại bỏ nhiều hiểu lầm.

**Bounded context** là một ranh giới logic, bên trong đó áp dụng một model và một ngôn ngữ. Trong EShop: Catalog, Basket, Discount, và Ordering.

Và **context map** cho thấy các context liên hệ với nhau thế nào. Vd: Basket gọi Discount qua gRPC, và Basket thông báo cho Ordering bằng một event.

## Slide 12: Bounded contexts → microservices  `[14:45]`

Trong thực tế, làm sao tìm ra các bounded context? Có một phương pháp đơn giản. Từ hành động của user (User Story) ➔ Nhặt từ khóa (Noun/Verb) ➔ Nhóm lại thành một vùng nghiệp vụ (Bounded Context) ➔ Biến vùng đó thành một Service.
  (Bắt đầu từ các **user story**. Tìm các **danh từ**, chúng trở thành entity, và các **động từ**, chúng trở thành use case. Rồi nhóm chúng lại. )

"List and filter products by category" cho mình noun là **Product, Category**, ì các từ này đều nói về thông tin gốc của hàng hóa, ta nhóm chúng lại thành ngữ cảnh Catalog - Danh mục sản phẩm. Đây sẽ là Catalog Service.

Cốt lõi của ý tưởng bounded context: **cùng một từ có thể mang nghĩa khác nhau trong các context khác nhau.** (Trong Catalog, một `Product` có name, description, image, categories và price, vì việc của Catalog là hiển thị sản phẩm. Trong Ordering, một `Product` chỉ có ID, name và price, vì một order chỉ cần có vậy. Hai model, mỗi cái đúng trong ranh giới của riêng nó. Cố ép một model "Product" dùng chung cho tất cả mọi người chính là cách các monolith bị rối.)

Một bounded context thường là một microservice, nhưng ta có thể tách nhỏ hơn nữa nếu một phần bên trong cần scale khác 
  (1 phần bên trong = domain component + technical component
  Order Context tách ra Tạo đơn hàng, Xem lịch sử đơn hàng, Xuất báo cáo tài chính cuối tháng).

## Slide 13: The big picture  `[15:45]`

vd hệ thống ecommerce ta có

**Shopping.Web**, client, chỉ nói chuyện với **YARP API gateway**. Gateway route đến **Catalog**, **Basket** và **Ordering**, mỗi service có database riêng. 

Catalog và Basket có thể dùng Vertical Slice architecture; Ordering dùng Clean Architecture với một DDD domain model.

 **Basket gọi Discount qua gRPC**: synchronous call. Và **Basket publish một event lên RabbitMQ**, **Ordering consume** nó: asynchronous call. Hai mũi tên đó chính là Part 2 và Part 3 của buổi nói. Còn gateway phía trước tất cả là Part 4. 


---

## Slide 14: Part 2 divider  `[16:00]`

Phần hai: synchronous communication.

## Slide 15: Service-to-service communication  `[18:30]`
Có hai kiểu. **Synchronous**: client gửi request và **chờ** cho đến khi nhận được response. Các công nghệ phổ biến gồm HTTP với REST, gRPC, GraphQL. **temporal coupling** (ràng buộc về mặt tgian): cả hai service phải đang chạy cùng một lúc.

**Asynchronous**: client gửi một message và **không chờ**. Message đi đến một broker, như RabbitMQ hay Kafka, dùng một protocol như AMQP.

Có hai kiểu. **One-to-one**, dùng **queue**: mỗi message chỉ được xử lý bởi đúng một consumer, giống như giao một việc cho một người làm. **One-to-many**, dùng **publish/subscribe**: một message được gửi đến mọi subscriber, giống như một thông báo mà nhiều service có thể cùng phản ứng.

Và ràng buộc giữa các service là **loose coupling** (lỏng). Bên gửi chỉ biết broker, không cần biết bên nhận. Nếu bên nhận đang down, broker **lưu** message lại và giao khi bên nhận chạy lại, và **retry** nếu xử lý bị lỗi. Nên hai service không cần chạy cùng một lúc.

EShop dùng cả hai. Basket hỏi Discount **synchronous bằng gRPC**, vì nó cần giá ngay lúc đó. Web app gọi các API **bằng REST**. Còn Basket báo cho Ordering về một lượt checkout thanh toán **asynchronous**.

rule of thumb là **REST cho public API, gRPC cho internal call, messaging cho các workflow xuyên nhiều service.**

## Slide 16: Introduction to Protobuf and gRPC  `[20:15]`
REST thì mn đã quá quen thuộc rồi nên e sẽ gthieu về grpc.
là 1 một gọi thủ tục từ xa framework của Google (Remote Procedure Call framework). Nó dùng protocol **HTTP/2**, và client gọi một remote method **như thể đó là một local object**. (tức là gọi hàm ở servcer khác bằng cú pháp đơn giản như gọi hàm ngay trên máy (local object), framework tự tạo các đoạn code kết nối mạng phức tạp. Như framework này sẽ tự tạo ra một object giả lập ở phía Client gọi là stub, âm thầm tự đóng gói dữ liệu thành mã nhị phân, gửi qua giao thức HTTP/2, chạy qua dây mạng đến server bên kia để lấy kết quả về)

mình sẽ viết 1 Contract được viết bằng **Protocol Buffers**, tên file `.proto`. vd Bên phải là contract thật của Discount. Một block `service` liệt kê các method: GetDiscount, Create, Update và Delete. Các block `message` định nghĩa data: một `GetDiscountRequest` gồm 1 string product name, và một `CouponModel` với ID, name, description và amount.

Hai điều mà mọi người hay bỏ sót. Thứ nhất, các con số như `= 1` và `= 2` là **field number** dùng trong binary encoding, tức là Mã số vị trí của trường dữ liệu đó khi biến đổi thành mã nhị phân, không phải giá trị mặc định. Một khi đã dùng thì không được tái sử dụng hay đổi chúng. Thứ hai, Protobuf là **binary**: nhỏ hơn và parse nhanh hơn JSON rất nhiều.

Và trong .NET, mình không phải viết code networking. Quá trình build **tự sinh** một server base class và một client class từ file `.proto`.

## Slide 17: gRPC server  `[21:30]`

Đây là phía server. Trong `Program.cs`: `AddGrpc`, database, và `MapGrpcService`.

`DiscountService` **kế thừa `DiscountProtoServiceBase`**, class được sinh ra từ file `.proto`. Mình chỉ cần override `GetDiscount`: tìm coupon theo product name, hoặc trả về một coupon "No Discount" với amount bằng không. Để ý là không có routing, không có JSON và không có status code; contract đã định nghĩa tất cả những thứ đó.

Cách xử lý lỗi cũng khác. Thay vì trả về HTTP 404, service throw một `RpcException` kèm một gRPC status code. Ví dụ, `DeleteDiscount` throw `StatusCode.NotFound` nếu coupon không tồn tại, còn `Create` và `Update` throw `InvalidArgument` khi request sai. Client nhận chúng dưới dạng các typed exception. Ba method còn lại, Create, Update và Delete, theo cùng một khuôn: map Protobuf message sang database model, lưu, rồi map ngược lại.

## Slide 18: gRPC client  `[23:15]`

Và phía client, trong Basket. Ba bước.

**Một**: project Basket include *cùng* file `.proto` đó, với `GrpcServices="Client"`, nên chỉ phần client được sinh ra. Contract được chia sẻ; code thì không.

Thực ra, project Basket thậm chí không copy file. Nó link thẳng đến file `.proto` của Discount, nên nếu contract thay đổi, cả hai phía sẽ thấy thay đổi ở lần build kế tiếp.

**Hai**: đăng ký service (một typed client) bằng `AddGrpcClient` và đọc địa chỉ của Discount từ configuration. Handler phụ ở đó chấp nhận self-signed development certificate. Điều đó ổn trên laptop, nhưng đừng bao giờ ship nó lên production.

**Ba**: inject client và gọi. Với mỗi cart item, gọi `GetDiscountAsync` với product name, rồi trừ đi amount của coupon. Nó đọc như một lời gọi method cục bộ, nhưng thật ra là một binary HTTP/2 request đến một container khác.

## Slide 19: When to use REST, when to use gRPC  `[25:45]`

Vậy khi nào chọn cái nào? So 2 thằng trước.

Cả hai đều có thể chạy trên HTTP/2, nhưng gRPC thì bắt buộc. REST gửi **text JSON**; gRPC gửi **binary Protobuf**, nhỏ hơn và nhanh hơn. Với REST, contract là tùy chọn, ví dụ OpenAPI; với gRPC nó là **bắt buộc**, và code được sinh ra từ đó. **Browser** nói REST một cách tự nhiên, còn gRPC cần gRPC-Web hoặc một proxy. gRPC hỗ trợ **streaming** hai chiều; REST là request và response. Và REST dễ test bằng tay với curl hoặc browser; gRPC cần công cụ riêng như postman.

Vậy: **chọn REST** cho public API, browser, bên thứ ba và CRUD đơn giản. **Chọn gRPC** cho các lời gọi nội bộ, khối lượng lớn, độ trễ thấp giữa các service của chính mình.

vd như EShop chia: REST phía sau gateway cho web app, và gRPC cho Basket gọi Discount, nơi được gọi một lần cho mỗi item trong mỗi giỏ hàng.

vd Một giỏ hàng có mười item nghĩa là mười lời gọi Discount mỗi lần basket được lưu, cho mọi user. Với gRPC, những lời gọi đó dùng chung một kết nối HTTP/2 và mỗi lời gọi chỉ gửi vài byte binary, thay vì mở request riêng và parse JSON. Đó là chỗ gRPC phát huy giá trị.

(Và đây không phải là lựa chọn một trong hai cho cả hệ thống. Hầu hết hệ thống thực tế làm đúng như EShop: REST ở rìa, nơi browser và đối tác kết nối, và gRPC ở bên trong, giữa các service mình sở hữu. ASP.NET Core thậm chí có thể expose một gRPC service dưới dạng REST luôn, bằng một tính năng gọi là JSON transcoding, nếu mình cần cả hai.)

---

## Slide 20: Part 3 divider  `[26:00]`

Phần ba: asynchronous communication, và vì sao nó quan trọng.

## Slide 21: Basic event-driven architecture  `[27:45]`
Bắt đầu với vấn đề ở bên trái. Hãy tưởng tượng tạo một order mà chỉ dùng các lời gọi synchronous. Gateway gọi ShoppingCart. ShoppingCart gọi Product và Pricing, rồi gọi Order. Order gọi Billing và Shipping. Sáu call chỉ cho một cú click. Mọi service đều phải đang chạy, độ trễ cộng dồn lại, và nếu Shipping chậm thì tất cả đều chậm.

Event-driven architecture khắc phục điều này. Một **event** là **một sự thật đã xảy ra**, được đặt tên ở thì quá khứ: *BasketCheckedOut*, *OrderCreated*. **Producer** publish nó rồi đi tiếp. Nó **không biết** ai đang lắng nghe. Các **consumer**  tự xử lý dữ liệu theo nhịp độ riêng, không phụ thuộc vào nhau.

Và một sự phân biệt hữu ích: một **command** bảo một service cụ thể phải làm gì, như "CreateOrder". Một **event** chỉ thông báo điều gì đã xảy ra, cho bất cứ ai quan tâm. Command ràng buộc mình với bên nhận; event thì không.

Trong EShop, checkout là event. Basket không gọi Ordering rồi bảo "hãy tạo một order". Nó thông báo "basket này đã được checkout", và Ordering tự quyết định điều đó có nghĩa gì với nó.

## Slide 22: The role of a message broker  `[29:30]`

Vậy broker ở giữa thực sự làm gì? Nó **nhận** message, **lưu** chúng cho đến khi consumer sẵn sàng, kể cả khi consumer đang down, **route** chúng đến đúng các queue, và **giao và retry**. Nó tách rời (decouple) các service về **thời gian**, vì chúng không cần chạy cùng lúc, và về **vị trí**, vì producer không cần biết địa chỉ của consumer.

vd **RabbitMQ** là một broker open-source implement protocol AMQP, ngoài ra còn Apache Kafka, Azure Service Bus, ActiveMQ

Bên cạnh đó, Producer không gửi event trực tiếp vào queue. Chúng gửi đến một **exchange**, và exchange route đến các queue dựa trên exchange type như: **direct** theo routing key chính xác, **fanout** đến tất cả mọi người (spam toàn hệ thống), **topic** theo pattern, hoặc **headers**.

Bên trên RabbitMQ, ta có thể dùng **MassTransit**, một thư viện .NET tạo exchange và queue, đồng thời xử lý serialization và retry. (Nên code của mình nghĩ theo event, không phải theo exchange.)

RabbitMQ cũng có sẵn một management web UI. Nó hiển thị mọi exchange, queue và message rate, và đó là nơi đầu tiên nên xem khi một message có vẻ bị thất lạc.

## Slide 23: RabbitMQ vs. Kafka  `[31:00]`

RabbitMQ không phải broker duy nhất. Broker thứ hai mọi người hay nghe nhất là **Kafka**, và khác biệt chính gói gọn trong một chữ: RabbitMQ là **queue**, Kafka là **topic log**.

Với **RabbitMQ**, broker đẩy (push) từng message đến một consumer, và khi consumer xác nhận (ack) thì message bị xóa. Message giống như một việc cần được giao.

Với **Kafka**, message được ghi nối tiếp vào một **log**, chia theo topic và partition. Message được giữ lại trong một khoảng thời gian (retention), kể cả sau khi đã được đọc. Mỗi consumer group giữ vị trí đọc riêng trong log, nên hai group có thể đọc cùng các message một cách độc lập, và có thể **đọc lại (replay)** message cũ.

Bảng tóm tắt lại: RabbitMQ route bằng exchange và giữ thứ tự theo từng queue; Kafka route theo topic và partition key, giữ thứ tự theo từng partition. RabbitMQ dùng AMQP; Kafka có protocol riêng. Nên RabbitMQ rất hợp cho command và integration event giữa các service, như checkout của mình. Kafka mạnh ở event streaming khối lượng lớn, log và analytics. EShop dùng RabbitMQ, với MassTransit bên trên.

## Slide 24: Basic pub/sub pattern  `[32:30]`

Cách dùng broker phổ biến nhất là mô hình **publish/subscribe** Trong đó, một service chỉ cần phát sự kiện lên hệ thống, và tất cả các service khác có đăng ký quan tâm sẽ tự động nhận được sự kiện đó để xử lý.

vd như Basket publish **một** `BasketCheckoutEvent` đến một exchange. Broker **fan out** một bản sao đến queue của từng subscriber. Hôm nay trong EShop chỉ có một subscriber, là Ordering. Nhưng ngày mai mình có thể thêm một Notification service để gửi email, và một Analytics service để ghi nhận doanh số, **mà không phải sửa một dòng nào trong Basket**. Đó là sức mạnh thật sự của pub/sub.

So sánh với một queue thông thường: queue giao mỗi message cho **một** consumer; pub/sub cho **mỗi** subscriber một bản copy riêng.

Bên phải là các cái giá phải trả cho Event-Driven Architecture / Asynchronous Messaging
- Broker thường giao **at least once**, nên một message có thể đến hai lần, và consumer phải **idempotent**: xử lý hai lần thì vẫn phải an toàn. 
- Thứ tự không được bảo đảm. 
- Broker trở nên quan trọng sống còn, nên phải có high availability, tức là  thiết lập hệ thống sao cho Broker luôn luôn hoạt động liên tục. (Cluster (Cụm máy chủ) và Replication (Nhân bản dữ liệu))
- Khó trace và debug.
- Và có **eventual consistency**: order xuất hiện sau checkout một lúc, không phải ngay lập tức.

## Slide 25: Messaging setup  `[33:45]`

Đây là cách EShop thiết lập việc này. Bên trái là **contract**: một base record `IntegrationEvent`, và `BasketCheckoutEvent` với user name, total price, address và payment. Cả Basket và Ordering đều reference cùng class này.

Bên phải là một extension method dùng chung, `AddMessageBroker`. Nó cấu hình MassTransit, đăng ký các consumer từ một assembly nếu có, và kết nối đến RabbitMQ với các setting lấy từ configuration. Mọi service đều gọi đúng một method này.

Hai chi tiết. `ConfigureEndpoints` bảo MassTransit tạo một queue cho mỗi consumer mà nó tìm thấy, và bind nó vào đúng exchange, nên mình không bao giờ phải khai báo queue bằng tay. Và địa chỉ broker lấy từ configuration: chạy local là `amqp://localhost:5672`, còn bên trong Docker là `amqp://ecommerce-mq:5672`, được set bằng environment variable trong docker-compose. Cùng một code, khác môi trường.

## Slide 26: Publish and consume  `[35:45]`

Và cả hai đầu của luồng checkout.

Bên trái là **publisher** trong Basket. Checkout handler lấy một `IPublishEndpoint` từ MassTransit. Nó load basket, map dữ liệu checkout thành một `BasketCheckoutEvent`, set total price, rồi gọi **`Publish`**. Sau đó nó xóa basket và trả về success, **ngay lập tức**. Nó không chờ một order.

Bên phải là **consumer** trong Ordering. Nó implement **`IConsumer<BasketCheckoutEvent>`**. Khi một message đến, `Consume` log nó, map thành một `CreateOrderCommand`, và gửi qua MediatR, nơi order được tạo ra.

Để ý: **Basket không hề biết Ordering tồn tại.** Nó chỉ publish một event. Đó là loose coupling trong thực tế.

Nói thêm 1 chút về gtiep bất đồng bộ thì: nếu `Publish` thành công, nhưng việc xóa giỏ hàng thất bại thì sao? Hoặc ngược lại? Ghi vào database và publish một message là hai thao tác riêng biệt, và cái nào cũng có thể thất bại. Đó gọi là **dual write problem**. Cách khắc phục chuẩn là **Outbox pattern**: lưu event trong cùng database transaction với dữ liệu, rồi publish sau. 


## Slide 27: Transactions across services: the Saga pattern  `[37:30]`

Một chủ đề async cuối cùng: transaction. Mỗi service có database riêng, nên **không có một database transaction duy nhất** bao trùm nhiều service. Vậy làm sao giữ một nghiệp vụ nhiều bước được nhất quán?

Câu trả lời là **Saga pattern**: một chuỗi các **local transaction**, mỗi service một transaction. Nhìn luồng thành công: Ordering tạo order ở trạng thái pending, Payment trừ tiền thẻ, Inventory giữ hàng trong kho, rồi Ordering xác nhận order. Mỗi bước tự commit.

Giờ đến luồng lỗi. Inventory hết hàng. Mình không thể rollback database của Payment, vì transaction đó đã commit rồi, ở một service khác. Thay vào đó, mỗi bước có một **compensating action** (hành động bù trừ) để hoàn tác nó về mặt nghiệp vụ: Payment **hoàn tiền**, Ordering **hủy** order. Saga chạy các bước bù trừ này theo thứ tự ngược lại để đưa hệ thống về trạng thái nhất quán ban đầu. Tức là bù trừ, không phải rollback như transaction của database, và các service vẫn độc lập với nhau.

Có hai cách điều phối saga. **Choreography**: mỗi service lắng nghe event của các service khác và tự phản ứng, không có bộ điều phối trung tâm. **Orchestration**: một orchestrator trung tâm, ví dụ một state machine của MassTransit, chỉ định từng service làm gì tiếp theo.

Khóa học có giải thích luồng này; còn code của EShop chỉ dùng một event cho checkout và không implement saga.

---

## Slide 28: Part 4 divider  `[37:45]`

Chủ đề cuối cùng: API gateway, và pattern Backends for Frontends.

## Slide 29: The role of an API Gateway  `[39:15]`

Không có gateway, mỗi client phải biết địa chỉ của mọi service, và mỗi service phải tự xử lý authentication, SSL và rate limiting.

Gateway là một **điểm vào duy nhất** giữa client và các service, và nó làm ba việc. **Gateway routing**: gửi mỗi request đến đúng internal service. Nếu mình di chuyển hoặc tách một service, chỉ có config của gateway thay đổi, không phải client. **Gateway aggregation**: gộp nhiều lời gọi service thành một response, để một client chỉ gọi một lần thay vì nhiều service khác nhau. Và **gateway offloading**: xử lý các cross-cutting concern ở một nơi. cross-cutting concern là những tính năng hoặc logic nghiệp vụ không thuộc về riêng một Microservice nào, mà service nào cũng cần phải có, vd authen author, logging, rate limit, caching,...

Bên phải là pipeline mà một request đi qua. Trước khi đến được service, request có thể sẽ đi qua một pipeline tại API Gateway gồm các bước như: validation, IP allowlisting, authentication, rate limiting, service discovery, và protocol conversion. Toàn bộ các thành phần này đều là những tính năng tùy chọn, có thể bật/tắt hoặc cấu hình linh hoạt tùy theo nhu cầu hệ thống.

Rủi ro: một gateway cho tất cả mọi thứ có thể trở thành **bottleneck** và **single point of failure**. Điều đó dẫn đến pattern tiếp theo.

## Slide 30: Backends for Frontends  `[40:30]`

**Backends for Frontends**, hay BFF, nghĩa là **một gateway cho mỗi loại client**, thay vì một gateway cho tất cả client.

Tại sao? Vì các client khác nhau cần những thứ khác nhau. Màn hình điện thoại cần ít dữ liệu hơn trang desktop, khác DTO. Nếu một gateway phục vụ tất cả, nó sẽ chứa nhiều logic riêng của từng client và trở thành bottleneck mà team nào cũng phải sửa. Với BFF, web team sở hữu web gateway, mobile team sở hữu mobile gateway, và mỗi cái trả về đúng những gì frontend của nó cần.


## Slide 31: Introduction to YARP  `[42:00]`

**YARP**, viết tắt của "Yet Another Reverse Proxy". Đây là một reverse-proxy library của Microsoft, xây trên .NET Core. Nó hỗ trợ HTTP/2, gRPC và WebSockets, load balancing, health check, và có thể tận dụng trực tiếp các middleware của .NET Core, vd như Rate Limiting hay Authentication.

Đoạn code bên trái là cách implement yarp trong program.cs: thêm reverse proxy, load config của nó, thêm một rate limiter và config rate limiter (cho phép 5 request mỗi 10 giây), và map proxy.

Bên phải là config trong appsettings. Một **route**, Là lối vào hay là cái đường dẫn mà Client từ ngoài gọi lên Gateway, khớp với một path, như /catalog-service/ cộng với gì đó phía sau, và trỏ đến một **cluster**, Là lối ra hay là địa chỉ IP/Domain thực tế của service: catalog.api:8080
Ở đây có config transform là bỏ đi phần tiền tố (prefix) /catalog-service/
(Transform: Là hành động sửa đổi đường dẫn trước khi ném vào service nội bộ.)
vì vậy client gọi đến gateway /catalog-service/products tức là gọi catalog.api:8080/products

Và route ordering còn áp dụng thêm rate limiting policy.


## Slide 32: Ocelot, and YARP vs. Ocelot  `[44:00]`

1 lựa chọn khác trong .NET là **Ocelot**, một API gateway library open-source được cấu hình bằng file `ocelot.json`. Đường dẫn **upstream** là cái client gọi; đường dẫn và host **downstream** là nơi Ocelot forward đến. Code khởi động trong program.cs chỉ là `AddOcelot` và `UseOcelot`.

So sánh thế nào? Mình đi qua bảng từng dòng.

**From**: YARP do Microsoft phát triển; Ocelot là một project open-source của cộng đồng.

**What it is**: YARP là một **toolkit** reverse-proxy. Nó cung cấp các building block nhanh để mình tự customize bằng C#. Ocelot là một **API gateway** làm sẵn: chủ yếu là cấu hình, ít phải viết code.

**Config**: YARP dùng **routes và clusters**, trong `appsettings.json` hoặc trong code. Ocelot dùng các route **upstream và downstream** trong file `ocelot.json` riêng.

**Built in**: YARP có sẵn routing, load balancing, health check và transform. Ocelot có routing cùng với rate limiting, authentication, caching và request aggregation.

**Extra features**: với YARP, mình thêm qua các middleware ASP.NET Core chuẩn, như rate limiter ở slide trước. Với Ocelot, chủ yếu là bật trong file config riêng của nó.

Và **trong EShop**: dùng YARP, không dùng Ocelot.

Cả hai đều implement gateway routing pattern. 
---

## Slide 33: Summary  `[45:15]`

Mình tóm tắt lại.

**Microservices** là các service nhỏ, tự chủ, xây quanh các business capability, mỗi service có dữ liệu và deployment riêng. Chỉ áp dụng khi có lý do thực sự.

**DDD** cho mình biết **tách ở đâu**, bằng bounded context và ubiquitous language, 

Với giao tiếp **synchronous**, dùng **REST** cho các API hướng client và **gRPC với Protobuf** cho các lời gọi nội bộ nhanh. Với giao tiếp **asynchronous**, dùng **event** qua **message broker** với pub/sub, cho các workflow decoupled. Và một **API gateway**, YARP hoặc Ocelot, làm cánh cửa trước duy nhất, cùng **BFF** khi có nhiều loại client.


## Slide 34: References `[47:00]`

cuốn sách của Eric Evans,
e-book của Microsoft

## Slide 35: Questions `[60:00]`

Phần tbay của e đến đây là hết, mn ai có câu hỏi j ko ạ


---

## Các câu hỏi có thể gặp và câu trả lời ngắn

**"Một bounded context có luôn là một microservice không?"**
Thường là vậy, nhưng không phải luôn luôn. Bounded context là một ranh giới logic trong model; microservice là một ranh giới deployment. Một bounded context có thể được tách thành nhiều service nếu các phần cần scale khác nhau. Chiều ngược lại, một service trải dài nhiều bounded context, là một dấu hiệu cảnh báo.

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
