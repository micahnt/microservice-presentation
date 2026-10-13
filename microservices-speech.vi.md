## Slide 1: Title  `[1:00]`

hnay e sẽ thuyết trình về microservice. Mình sẽ đi qua bốn phần. Một: microservices là gì, và Domain-Driven Design giúp mình quyết định nên tách hệ thống ở đâu. Hai: các service giao tiếp với nhau theo kiểu synchronous như thế nào, với REST và gRPC. Ba: giao tiếp asynchronous, với event và message broker. Và bốn: API gateway đứng phía trước tất cả.

## Slide 2: Agenda  `[2:15]`
// coi lại
Đây là kế hoạch của buổi hôm nay.

Mình bắt đầu với về microservices và Domain-Driven Design: monolith so với microservices, các design principle, và các ý tưởng của DDD như bounded context, entity và value object.


Tiếp theo là synchronous communication, asynchronous communication, cho API gateway và pattern Backends for Frontends.

Rồi một live demo ngắn về lời gọi gRPC giữa Basket và Discount, chạy ngay trên laptop của mình, không cần Docker. Và mình sẽ kết thúc bằng phần tóm tắt và câu hỏi của mọi người. Nếu có chỗ nào chưa rõ, mọi người cứ hỏi ngay nhé.

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

Về **deployment**, monolith deploy đơn giản nhưng phải deploy cả khối (sửa một dòng code cũng phải deploy lại toàn bộ.) Microservices deploy độc lập, không downtime, qua CI/CD pipeline riêng của từng service. Hình vẽ ở dưới minh họa điều này. Với monolith, ba team cùng commit vào **một code base duy nhất** với **một pipeline**, nên mỗi lần release lên prod đều phải phối hợp. Với microservices, mỗi team có **code riêng, pipeline riêng, và service riêng**, và release khi nào sẵn sàng thì release.

Về **data**, một database dùng chung so với mỗi service một database.

Về **failure**, một bug, ví dụ memory leak, có thể làm sập cả monolith, trong khi microservices cô lập lỗi trong một service (fault isolation).

Và về **team fit**, monolith phù hợp với team nhỏ hoặc team mới. Microservices cần một team có kinh nghiệm, quen với container, DevOps và monitoring. Thiếu những kỹ năng đó thì độ phức tạp tăng thêm sẽ gây hại nhiều hơn lợi.

Không bên nào là "tốt hơn". Với nhiều hệ thống, monolith là một lựa chọn hoàn toàn hợp lý. (vd Dự án mới khởi nghiệp (MVP), Team size tầm 15-20 người,...)


## Slide 7: Benefits and challenges  `[8:15]`

Đây là pros cons.

Các **benefit**: agility, vì các team nhỏ có thể ship mà không cần phối hợp một đợt release khổng lồ; **independent scaling**; codebase nhỏ; deploy độc lập; chọn đúng công nghệ cho đúng việc; và **fault isolation**: (nếu Discount bị crash, khách hàng vẫn xem được sản phẩm.)

Các **challenge** thì có thật. Toàn hệ thống **phức tạp** hơn. Mỗi call bây giờ là một **network call** có thể chậm hoặc thất bại. End-to-end testing khó hơn. **Data consistency** khó hơn (vì không còn một database transaction duy nhất bao trùm nhiều service). Và debug xuyên qua nhiều service rất khó.

Nên cơ bản là microservices không xóa bỏ độ phức tạp. Nó **chuyển** độ khó từ bên trong code sang giữa các service.

## Slide 8: When to use, and when not to  `[9:15]`

Hãy dùng microservice khi mình cần **zero-downtime independent deployment**, **independent scaling**, các data store khác nhau, có nhiều team tự chủ, hoặc là khi mình có một lý do (hệ thống Monolith hiện tại không thể chịu tải được nữa, team size quá lớn, mn giẫm chân nhau).

Hãy tránh khi mình sẽ xây một **distributed monolith**: tức là microservice nhưng các service bị tight coupling đến mức vẫn phải deploy cùng nhau. Cũng nên tránh khi chưa có Devops, khi chỉ có **team nhỏ**, khi là **startup mới** mà domain còn thay đổi liên tục, và đừng bao giờ để các service **dùng chung database**.

## Slide 9: Design principles  `[10:45]`

Nếu đã chọn microservices, sáu nguyên tắc này giúp mình tránh bị distributed monolith.

**Một: single responsibility.** Mỗi service tương ứng với một **business capability**, như Catalog hay Ordering, chứ không phải một tầng kỹ thuật như "database service".

**Hai: loose coupling, high cohesion.** Mình có thể thay đổi một service mà không phải đổi các service khác, và mọi thứ về một capability nằm cùng một chỗ.

**Ba: autonomy.** Mỗi service tự deploy và có db riêng.

**Bốn: well-defined contracts.** Các service chỉ giao tiếp qua API và event. Fowler gọi đây là "smart endpoints and dumb pipes": logic nằm trong các service, còn network chỉ làm nhiệm vụ chuyển dữ liệu.

**Năm: design for failure.** Trong một distributed system, có thể lỗi ở chỗ nào đó bất cứ lúc nào. Mình dùng timeout, retry và circuit breaker để một service lỗi không kéo sập các service còn lại.

**Sáu: automate và observe.** nên có CI/CD, container, health check và centralized logging vì thiếu thì khó thể quản lý nổi một đống service.

## Slide 10: Introduction to DDD  `[12:15]`

Giờ, câu hỏi khó nhất trong microservices là: **mình vạch ranh giới giữa các service ở đâu?** Đó là chỗ Domain-Driven Design xuất hiện.

DDD được **Eric Evans** giới thiệu trong cuốn sách năm 2003 của ông. Nó không phải một công nghệ hay một framework. Nó là một cách **mô hình hóa phần mềm xoay quanh nghiệp vụ**. Ý tưởng cốt lõi rất đơn giản: chia một vấn đề phức tạp thành các phần nhỏ hơn, và cho mỗi phần một model riêng.

DDD có hai nửa. **Strategic DDD** là bức tranh lớn: hiểu business domain, tìm ra các subdomain và bounded context, và vẽ ra mối quan hệ giữa chúng. Nó cho mình biết **tách ở đâu** thành các microservice. **Tactical DDD** làm việc bên trong một service: các pattern như entity, value object và aggregate. Nó cho mình biết **xây dựng** model của từng service **như thế nào**.

Mình sẽ đi qua cả hai: strategic trước, tactical sau.

## Slide 11: DDD core concepts  `[13:30]`

Năm khái niệm mà mọi cuộc trò chuyện về DDD đều nhắc đến.

**Domain** là vấn đề nghiệp vụ mà phần mềm giải quyết: với mình là e-commerce. **Subdomain** là một mảng chuyên môn bên trong nó: product catalog, shopping cart, discount, order.

**Ubiquitous language** là bộ từ vựng chung được dùng bởi các domain expert *và cả* trong code. Nếu phía business nói "order item" và "shipping address", thì code cũng gọi là `OrderItem` và `ShippingAddress`, chứ không phải "row" hay "data object". Cách này loại bỏ cả một lớp hiểu lầm.

**Bounded context** là một ranh giới, bên trong đó áp dụng một model và một ngôn ngữ. Trong EShop: Catalog, Basket, Discount, và Ordering.

Và **context map** cho thấy các context liên hệ với nhau thế nào. Với mình: Basket hỏi Discount qua gRPC, và Basket thông báo cho Ordering bằng một event.

## Slide 12: Bounded contexts → microservices  `[14:45]`

Trong thực tế, làm sao tìm ra các bounded context? Khóa học dùng một phương pháp đơn giản. Bắt đầu từ các **user story**. Tìm các **danh từ**, chúng trở thành entity, và các **động từ**, chúng trở thành use case. Rồi nhóm chúng lại.

"List và filter products" cho mình **Catalog**. "Put products in the cart" cho mình **Basket**. "Apply coupons" cho mình **Discount**. "Check out và create an order" cho mình **Ordering**.

Giờ hãy nhìn vào bullet đầu tiên, vì đó là trái tim của ý tưởng bounded context: **cùng một từ có thể mang nghĩa khác nhau trong các context khác nhau.** Trong Catalog, một `Product` có name, description, image, categories và price, vì việc của Catalog là hiển thị sản phẩm. Trong Ordering, một `Product` chỉ có ID, name và price, vì một order chỉ cần có vậy. Hai model, mỗi cái đúng trong ranh giới của riêng nó. Cố ép một model "Product" dùng chung cho tất cả mọi người chính là cách các monolith bị rối.

Một bounded context thường là một microservice, nhưng bạn có thể tách nhỏ hơn nữa nếu một phần cần scale khác đi.

## Slide 13: Entities, value objects, aggregates  `[16:00]`

Giờ đến tactical DDD: cách mô hình hóa bên trong một service. Ba building block.

**Entity** được định danh bằng **ID**. Hai order có dữ liệu giống hệt nhau vẫn là hai order khác nhau, vì ID khác nhau. Entity có **lifecycle**: một order được tạo, được thanh toán, được giao.

**Value object** không có ID. Nó được định nghĩa bởi **các giá trị** của nó. Hai address có cùng street, city và zip code là cùng một address. Và value object là **immutable**: mình không sửa một address, mà thay nó bằng một cái mới.

Trong Ordering service, `Order`, `OrderItem`, `Customer` và `Product` là entity. `Address`, `Payment`, `OrderName`, và thậm chí cả `OrderId` là value object.

**Aggregate** là một cụm các entity và value object được xem là một đơn vị consistency. Entity chính của nó là **aggregate root**, và đó là cổng vào duy nhất. Ở đây, `Order` là root. Không ai thêm order item trực tiếp; mình gọi `Order.Add`, và order tự bảo đảm các quy tắc của nó được giữ, ví dụ quantity và price phải dương.

## Slide 14: Entities and value objects in code  `[17:00]`

Đây là hình dạng của nó trong code Ordering.

Bên trái là base class `Entity`: một ID cộng với các audit field. `Order` kế thừa từ `Aggregate`, tức là một entity có thể raise domain event. Để ý rằng danh sách order item là **private**; bên ngoài chỉ nhận được một bản read-only. Cách duy nhất để thêm một item là method `Add`, method này validate quantity và price trước. Đó chính là aggregate root đang bảo vệ các quy tắc của nó.

Bên phải là các value object. `Address` là một C# **record**, nên hai address có cùng giá trị thì tự động bằng nhau. Nó được tạo qua một factory method tên là `Of`, method này validate input. Và `OrderId` là một **strongly typed ID**: thay vì truyền các GUID thô đi khắp nơi, nơi rất dễ nhầm order ID với customer ID, mỗi loại ID có một type riêng, và một GUID rỗng sẽ bị từ chối.

## Slide 15: The big picture  `[18:00]`

Và đây là cách toàn bộ hệ thống khớp với nhau. Đọc từ trái sang phải.

Đây là hệ thống tham chiếu cho phần còn lại của buổi nói, nên đáng để dành vài giây.

**Shopping.Web**, client, chỉ nói chuyện với **YARP API gateway**. Gateway route đến **Catalog**, **Basket** và **Ordering**, mỗi service có database riêng. Catalog và Basket dùng Vertical Slice architecture; Ordering dùng Clean Architecture với DDD model mà mình vừa xem.

Và để ý hai kết nối đặc biệt. **Basket gọi Discount qua gRPC**: synchronous. Và **Basket publish một event lên RabbitMQ**, **Ordering consume** nó: asynchronous. Hai mũi tên đó chính là Part 2 và Part 3 của buổi nói. Còn gateway phía trước tất cả là Part 4. Nên mọi thứ mình bàn từ giờ trở đi đều nằm đâu đó trên một diagram này.

*Nếu ai đó so sánh với slide "Big Picture" của khóa học: hình đó vẽ thêm một mũi tên từ gateway đến Discount. Trong code thật, config YARP chỉ route đến Catalog, Basket và Ordering.*

---

## Slide 16: Part 2 divider  `[18:15]`

Phần hai: synchronous communication.

## Slide 17: Service-to-service communication  `[20:45]`
Có hai kiểu. **Synchronous**: client gửi request và **chờ** cho đến khi nhận được response. Các công nghệ phổ biến gồm HTTP với REST, gRPC, GraphQL. **temporal coupling** (ràng buộc về mặt tgian): cả hai service phải đang chạy cùng một lúc.

**Asynchronous**: client gửi một message và **không chờ**. Message đi đến một broker, như RabbitMQ hay Kafka, dùng một protocol như AMQP.

Có hai kiểu. **One-to-one**, dùng **queue**: mỗi message chỉ được xử lý bởi đúng một consumer, giống như giao một việc cho một người làm. **One-to-many**, dùng **publish/subscribe**: một message được gửi đến mọi subscriber, giống như một thông báo mà nhiều service có thể cùng phản ứng.

Và ràng buộc giữa các service là **loose coupling** (lỏng). Bên gửi chỉ biết broker, không cần biết bên nhận. Nếu bên nhận đang down, broker **lưu** message lại và giao khi bên nhận chạy lại, và **retry** nếu xử lý bị lỗi. Nên hai service không cần chạy cùng một lúc.

EShop dùng cả hai. Basket hỏi Discount **synchronous bằng gRPC**, vì nó cần giá ngay lúc đó. Web app gọi các API **bằng REST**. Còn Basket báo cho Ordering về một lượt checkout thanh toán **asynchronous**.

rule of thumb là **REST cho public API, gRPC cho internal call, messaging cho các workflow xuyên nhiều service.**

## Slide 18: Introduction to Protobuf and gRPC  `[22:30]`
REST thì mn đã quá quen thuộc rồi nên e sẽ gthieu về grpc.
là 1 một gọi thủ tục từ xa framework của Google (Remote Procedure Call framework). Nó dùng protocol **HTTP/2**, và client gọi một remote method **như thể đó là một local object**. (tức là gọi hàm ở servcer khác bằng cú pháp đơn giản như gọi hàm ngay trên máy (local object), framework tự tạo các đoạn code kết nối mạng phức tạp. Như framework này sẽ tự tạo ra một object giả lập ở phía Client gọi là stub, âm thầm tự đóng gói dữ liệu thành mã nhị phân, gửi qua giao thức HTTP/2, chạy qua dây mạng đến server bên kia để lấy kết quả về)

mình sẽ viết 1 Contract được viết bằng **Protocol Buffers**, tên file `.proto`. vd Bên phải là contract thật của Discount. Một block `service` liệt kê các method: GetDiscount, Create, Update và Delete. Các block `message` định nghĩa data: một `GetDiscountRequest` gồm 1 string product name, và một `CouponModel` với ID, name, description và amount.

Hai điều mà mọi người hay bỏ sót. Thứ nhất, các con số như `= 1` và `= 2` là **field number** dùng trong binary encoding, tức là Mã số vị trí của trường dữ liệu đó khi biến đổi thành mã nhị phân, không phải giá trị mặc định. Một khi đã dùng thì không được tái sử dụng hay đổi chúng. Thứ hai, Protobuf là **binary**: nhỏ hơn và parse nhanh hơn JSON rất nhiều.

Và trong .NET, mình không phải viết code networking. Quá trình build **tự sinh** một server base class và một client class từ file `.proto`.

## Slide 19: gRPC server  `[23:45]`

Đây là phía server. Trong `Program.cs`: `AddGrpc`, database, và `MapGrpcService`.

`DiscountService` **kế thừa `DiscountProtoServiceBase`**, class được sinh ra từ file `.proto`. Mình chỉ cần override `GetDiscount`: tìm coupon theo product name, hoặc trả về một coupon "No Discount" với amount bằng không. Để ý là không có routing, không có JSON và không có status code; contract đã định nghĩa tất cả những thứ đó.

Cách xử lý lỗi cũng khác. Thay vì trả về HTTP 404, service throw một `RpcException` kèm một gRPC status code. Ví dụ, `DeleteDiscount` throw `StatusCode.NotFound` nếu coupon không tồn tại, còn `Create` và `Update` throw `InvalidArgument` khi request sai. Client nhận chúng dưới dạng các typed exception. Ba method còn lại, Create, Update và Delete, theo cùng một khuôn: map Protobuf message sang database model, lưu, rồi map ngược lại.

## Slide 20: gRPC client  `[25:30]`

Và phía client, trong Basket. Ba bước.

**Một**: project Basket include *cùng* file `.proto` đó, với `GrpcServices="Client"`, nên chỉ phần client được sinh ra. Contract được chia sẻ; code thì không.

Thực ra, project Basket thậm chí không copy file. Nó link thẳng đến file `.proto` của Discount, nên nếu contract thay đổi, cả hai phía sẽ thấy thay đổi ở lần build kế tiếp.

**Hai**: đăng ký service (một typed client) bằng `AddGrpcClient` và đọc địa chỉ của Discount từ configuration. Handler phụ ở đó chấp nhận self-signed development certificate. Điều đó ổn trên laptop, nhưng đừng bao giờ ship nó lên production.

**Ba**: inject client và gọi. Với mỗi cart item, gọi `GetDiscountAsync` với product name, rồi trừ đi amount của coupon. Nó đọc như một lời gọi method cục bộ, nhưng thật ra là một binary HTTP/2 request đến một container khác.

## Slide 21: When to use REST, when to use gRPC  `[28:00]`

Vậy khi nào chọn cái nào? So 2 thằng trước.

Cả hai đều có thể chạy trên HTTP/2, nhưng gRPC thì bắt buộc. REST gửi **text JSON**; gRPC gửi **binary Protobuf**, nhỏ hơn và nhanh hơn. Với REST, contract là tùy chọn, ví dụ OpenAPI; với gRPC nó là **bắt buộc**, và code được sinh ra từ đó. **Browser** nói REST một cách tự nhiên, còn gRPC cần gRPC-Web hoặc một proxy. gRPC hỗ trợ **streaming** hai chiều; REST là request và response. Và REST dễ test bằng tay với curl hoặc browser; gRPC cần công cụ riêng như postman.

Vậy: **chọn REST** cho public API, browser, bên thứ ba và CRUD đơn giản. **Chọn gRPC** cho các lời gọi nội bộ, khối lượng lớn, độ trễ thấp giữa các service của chính mình.

vd như EShop chia: REST phía sau gateway cho web app, và gRPC cho Basket gọi Discount, nơi được gọi một lần cho mỗi item trong mỗi giỏ hàng.

vd Một giỏ hàng có mười item nghĩa là mười lời gọi Discount mỗi lần basket được lưu, cho mọi user. Với gRPC, những lời gọi đó dùng chung một kết nối HTTP/2 và mỗi lời gọi chỉ gửi vài byte binary, thay vì mở request riêng và parse JSON. Đó là chỗ gRPC phát huy giá trị.

(Và đây không phải là lựa chọn một trong hai cho cả hệ thống. Hầu hết hệ thống thực tế làm đúng như EShop: REST ở rìa, nơi browser và đối tác kết nối, và gRPC ở bên trong, giữa các service mình sở hữu. ASP.NET Core thậm chí có thể expose một gRPC service dưới dạng REST luôn, bằng một tính năng gọi là JSON transcoding, nếu mình cần cả hai.)

---

## Slide 22: Part 3 divider  `[28:15]`

Phần ba: asynchronous communication, và vì sao nó quan trọng.

## Slide 23: Basic event-driven architecture  `[30:00]`
Bắt đầu với vấn đề ở bên trái. Hãy tưởng tượng tạo một order mà chỉ dùng các lời gọi synchronous. Gateway gọi ShoppingCart. ShoppingCart gọi Product và Pricing, rồi gọi Order. Order gọi Billing và Shipping. Sáu call chỉ cho một cú click. Mọi service đều phải đang chạy, độ trễ cộng dồn lại, và nếu Shipping chậm thì tất cả đều chậm.

Event-driven architecture khắc phục điều này. Một **event** là **một sự thật đã xảy ra**, được đặt tên ở thì quá khứ: *BasketCheckedOut*, *OrderCreated*. **Producer** publish nó rồi đi tiếp. Nó **không biết** ai đang lắng nghe. Các **consumer**  tự xử lý dữ liệu theo nhịp độ riêng, không phụ thuộc vào nhau.

Và một sự phân biệt hữu ích: một **command** bảo một service cụ thể phải làm gì, như "CreateOrder". Một **event** chỉ thông báo điều gì đã xảy ra, cho bất cứ ai quan tâm. Command ràng buộc mình với bên nhận; event thì không.


Trong EShop, checkout là event. Basket không gọi Ordering rồi bảo "hãy tạo một order". Nó thông báo "basket này đã được checkout", và Ordering tự quyết định điều đó có nghĩa gì với nó.

## Slide 24: Domain vs. integration events  `[31:30]`

Trong các hệ thống DDD, có hai loại event.

Một **domain event** ở lại **bên trong** một service. Nó được raise bởi một aggregate và được giao ngay trong process, trong bộ nhớ. Trong EShop, khi `Order.Create` chạy, aggregate `Order` raise một `OrderCreatedEvent`, và MediatR giao nó cho một handler trong cùng service.

Một **integration event** đi **giữa** các service, thông qua một message broker. Trong EShop, đó là `BasketCheckoutEvent`, từ Basket đến Ordering, qua RabbitMQ.

Diagram cho thấy chúng nối chuỗi với nhau thế nào. Integration event đến Ordering, handler tạo một order, order raise một domain event, và handler của domain event đó có thể publish một integration event mới, `OrderCreatedIntegrationEvent`, cho các service khác như shipping. Trong project này, bước cuối đó nằm sau một feature flag và đang tắt.

## Slide 25: The role of a message broker  `[33:15]`

Vậy broker ở giữa thực sự làm gì? Nó **nhận** message, **lưu** chúng cho đến khi consumer sẵn sàng, kể cả khi consumer đang down, **route** chúng đến đúng các queue, và **giao và retry**. Nó tách rời (decouple) các service về **thời gian**, vì chúng không cần chạy cùng lúc, và về **vị trí**, vì producer không cần biết địa chỉ của consumer.

**RabbitMQ** là một broker open-source implement protocol AMQP. Khái niệm then chốt: producer không gửi trực tiếp vào queue. Chúng gửi đến một **exchange**, và exchange route đến các queue dựa trên loại của nó: **direct** theo key chính xác, **fanout** đến tất cả mọi người, **topic** theo pattern, hoặc **headers**.

Bên trên RabbitMQ, EShop dùng **MassTransit**, một thư viện .NET tạo exchange và queue, đồng thời xử lý serialization và retry. Nên code của mình nghĩ theo event, không phải theo exchange.

RabbitMQ cũng có sẵn một management web UI, ở port 15672 trong setup của mình. Nó hiển thị mọi exchange, queue và message rate, và đó là nơi đầu tiên nên xem khi một message có vẻ bị thất lạc.

## Slide 26: Basic pub/sub pattern  `[34:45]`

Cách dùng broker phổ biến nhất là **publish/subscribe**.

Basket publish **một** `BasketCheckoutEvent` đến một exchange. Broker **fan out** một bản sao đến queue của từng subscriber. Hôm nay trong EShop chỉ có một subscriber, là Ordering. Nhưng ngày mai mình có thể thêm một Notification service để gửi email, và một Analytics service để ghi nhận doanh số, **mà không phải sửa một dòng nào trong Basket**. Đó là sức mạnh thật sự của pub/sub.

So sánh với một queue thông thường: queue giao mỗi message cho **một** consumer; pub/sub cho **mỗi** subscriber một bản sao riêng.

Bên phải là các cái giá phải trả. Broker thường giao **at least once**, nên một message có thể đến hai lần, và consumer phải **idempotent**: xử lý hai lần thì vẫn phải an toàn. Thứ tự không được bảo đảm. Broker trở nên quan trọng sống còn, nên phải có high availability. Và có **eventual consistency**: order xuất hiện sau checkout một lúc, không phải ngay lập tức.

## Slide 27: Messaging setup  `[36:00]`

Đây là cách EShop thiết lập việc này. Bên trái là **contract**: một base record `IntegrationEvent`, và `BasketCheckoutEvent` với user name, total price, address và payment. Cả Basket và Ordering đều reference cùng class này.

Bên phải là một extension method dùng chung, `AddMessageBroker`. Nó cấu hình MassTransit, đăng ký các consumer từ một assembly nếu có, và kết nối đến RabbitMQ với các setting lấy từ configuration. Mọi service đều gọi đúng một method này.

Hai chi tiết. `ConfigureEndpoints` bảo MassTransit tạo một queue cho mỗi consumer mà nó tìm thấy, và bind nó vào đúng exchange, nên mình không bao giờ phải khai báo queue bằng tay. Và địa chỉ broker lấy từ configuration: chạy local là `amqp://localhost:5672`, còn bên trong Docker là `amqp://ecommerce-mq:5672`, được set bằng environment variable trong docker-compose. Cùng một code, khác môi trường.

## Slide 28: Publish and consume  `[38:00]`

Và cả hai đầu của luồng checkout.

Bên trái là **publisher** trong Basket. Checkout handler lấy một `IPublishEndpoint` từ MassTransit. Nó load basket, map dữ liệu checkout thành một `BasketCheckoutEvent`, set total price, rồi gọi **`Publish`**. Sau đó nó xóa basket và trả về success, **ngay lập tức**. Nó không chờ một order.

Bên phải là **consumer** trong Ordering. Nó implement **`IConsumer<BasketCheckoutEvent>`**. Khi một message đến, `Consume` log nó, map thành một `CreateOrderCommand`, và gửi qua MediatR, nơi order được tạo ra.

Để ý: **Basket không hề biết Ordering tồn tại.** Nó chỉ publish một event. Đó là loose coupling trong thực tế.

Có một câu hỏi đáng đặt ra về handler này: nếu `Publish` thành công, nhưng việc xóa basket thất bại thì sao? Hoặc ngược lại? Ghi vào database và publish một message là hai thao tác riêng biệt, và cái nào cũng có thể thất bại. Đó gọi là **dual write problem**. Cách khắc phục chuẩn là **Outbox pattern**: lưu event trong cùng database transaction với dữ liệu, rồi publish sau. Khóa học sẽ đề cập sau; hôm nay mọi người chỉ cần biết vấn đề này tồn tại.

---

## Slide 29: Part 4 divider  `[38:15]`

Chủ đề cuối cùng: API gateway, và pattern Backends for Frontends.

## Slide 30: The role of an API Gateway  `[39:45]`

Không có gateway, mỗi client phải biết địa chỉ của mọi service, và mỗi service phải tự xử lý authentication, SSL và rate limiting.

Gateway là một **điểm vào duy nhất** giữa client và các service, và nó làm ba việc. **Gateway routing**: gửi mỗi request đến đúng service nội bộ. Nếu mình di chuyển hoặc tách một service, chỉ có config của gateway thay đổi, không phải client. **Gateway aggregation**: gộp nhiều lời gọi service thành một response, để một mobile app chỉ gọi một lần thay vì năm lần. Và **gateway offloading**: xử lý các cross-cutting concern ở một nơi.

Bên phải là pipeline mà một request đi qua: validation, IP allowlisting, authentication, rate limiting, service discovery, và protocol conversion, trước khi đến một service.

Rủi ro: một gateway cho tất cả mọi thứ có thể trở thành **bottleneck** và **single point of failure**. Điều đó dẫn đến pattern tiếp theo.

## Slide 31: Backends for Frontends  `[41:00]`

**Backends for Frontends**, hay BFF, nghĩa là **một gateway cho mỗi loại client**, thay vì một gateway cho tất cả mọi người.

Tại sao? Vì các client khác nhau cần những thứ khác nhau. Màn hình điện thoại cần ít dữ liệu hơn trang desktop, và ở một hình dạng khác. Nếu một gateway phục vụ tất cả, nó sẽ đầy logic riêng của từng client và trở thành bottleneck mà team nào cũng phải sửa. Với BFF, web team sở hữu web gateway, mobile team sở hữu mobile gateway, và mỗi cái trả về đúng những gì frontend của nó cần.

EShop chỉ có một client, Shopping.Web, nên một gateway là đủ. Mình sẽ thêm BFF khi, ví dụ, có một mobile app xuất hiện.

## Slide 32: Introduction to YARP  `[42:30]`

EShop implement gateway của nó bằng **YARP**, viết tắt của "Yet Another Reverse Proxy". Đây là một reverse-proxy library của Microsoft, xây trên ASP.NET Core. Nó hỗ trợ HTTP/2, gRPC và WebSockets, load balancing và health check, và cắm vào các middleware ASP.NET Core thông thường, như rate limiter.

Đoạn code bên trái là *toàn bộ* gateway của EShop: thêm reverse proxy, load config của nó, thêm một rate limiter cho phép 5 request mỗi 10 giây, và map proxy.

Bên phải là config. Một **route** khớp với một path, như `/catalog-service/` cộng với bất cứ thứ gì phía sau, và trỏ đến một **cluster**. **Transform** bỏ đi phần prefix. Cluster cho biết service nằm ở đâu: `catalog.api:8080`. Nên `/catalog-service/products` trở thành `catalog.api:8080/products`. Và route ordering còn áp dụng thêm rate limiting policy.

## Slide 33: Ocelot, and YARP vs. Ocelot  `[44:00]`

Lựa chọn nổi tiếng khác trong .NET là **Ocelot**, một API gateway library open-source được cấu hình bằng file `ocelot.json`. EShop không dùng nó, nên ở cuối slide mình viết cùng route Catalog đó theo kiểu Ocelot, chỉ để so sánh. Đường dẫn **upstream** là cái client gọi; đường dẫn và host **downstream** là nơi Ocelot forward đến. Code khởi động chỉ là `AddOcelot` và `UseOcelot`.

So sánh thế nào? **YARP** đến từ Microsoft và là một **toolkit** reverse-proxy: rất nhanh, tùy biến rất sâu bằng C#, và lấy thêm tính năng từ các middleware ASP.NET Core chuẩn. **Ocelot** là một project cộng đồng và là một **API gateway** làm sẵn: các tính năng như rate limiting, authentication, caching và request aggregation được bật trong file config riêng của nó.

Cả hai đều implement gateway routing pattern. YARP là cái mà khóa học này dùng.

## Slide 34: Choosing an invocation method  `[46:00]`

Giờ mình ráp tất cả lại. Bảng này đọc từ trái sang phải: phương thức, kiểu giao tiếp, phù hợp nhất cho việc gì, và EShop dùng nó ở đâu.

**REST over HTTP**: synchronous, dùng JSON. Phù hợp nhất cho các public API và API hướng client, vì client nào cũng hiểu được. Trong EShop, đó là web app đi qua gateway đến Catalog, Basket và Ordering.

**gRPC**: cũng synchronous, nhưng là binary, chạy trên HTTP/2. Phù hợp nhất cho các lời gọi nội bộ cần nhanh, khi hai bên dùng chung một contract chặt chẽ. Trong EShop, đó là Basket hỏi Discount giá của từng item, cũng chính là phần demo sắp tới.

**Messaging với RabbitMQ**: asynchronous, dùng event. Phù hợp nhất cho các workflow xuyên nhiều service, khi cần decoupling, hoặc fan-out đến nhiều consumer. Trong EShop, đó là checkout: Basket publish, Ordering consume.

Và **API gateway với YARP**: synchronous, là một reverse proxy. Nó không phải cách để các service nói chuyện với nhau, mà là cửa vào duy nhất cho client, nơi đặt routing, rate limiting và security. Trong EShop, Shopping.Web chỉ gọi đến YarpApiGateway.

Hệ thống thực tế thường kết hợp chúng. Lời khuyên để mang về: giữ các chuỗi gọi synchronous **ngắn**, và dùng messaging cho bất cứ thứ gì không cần câu trả lời ngay.

Một phép thử đơn giản mà mọi người có thể dùng: người gọi có cần câu trả lời *ngay bây giờ* để tiếp tục không? Basket cần discount ngay lúc đó để hiển thị giá, nên đó là một synchronous call. Basket không cần số order để hoàn tất checkout, nên đó là một event.

Slide thế là đủ. Giờ xem nó chạy.

---

## Slide 35: Demo setup  `[47:00]`

*(Hai service đã chạy sẵn trong hai terminal đặt cạnh nhau, để thấy log của cả hai. VS Code đang mở `demo.http`.)*

Với phần demo, mình chỉ lấy phần gRPC của khóa học, section 10 đến 12, và cho nó chạy trên laptop chỉ với .NET SDK. Không Docker, không database server.

Có hai project. **Discount.Grpc** là gRPC server, đúng như service mình đã thấy trên slide: contract `.proto`, `DiscountService`, và một file SQLite nhỏ với hai coupon có sẵn: giảm 150 cho iPhone X và giảm 100 cho Samsung 10. **Basket.API** là client. Mình đã rút gọn nó chỉ còn phần cốt lõi: nó giữ basket trong bộ nhớ, và khi mình lưu một basket, nó gọi Discount qua gRPC cho từng item.

Cả hai project dùng chung một file `discount.proto`. Project server sinh ra server base class, project client sinh ra client. Một contract, hai phía được sinh code.

Và có một điểm khác so với khóa học: ở đây client gọi `localhost:5002` bằng HTTP/2 thường, không có TLS, nên không cần cách xử lý development certificate như ở slide trước.

## Slide 36: Demo steps  `[52:00]`

*[Gửi request 1 trong `demo.http`: lưu basket với ba item.]*

Mình lưu một basket cho user "swn" với ba chiếc điện thoại: một iPhone X giá 950, hai Samsung 10 giá 840, và một Huawei Plus giá 650. Đây là một REST call bình thường, JSON vào và JSON ra.

Nhìn vào response: iPhone còn **800**, Samsung còn **740**, Huawei vẫn **650**.

Giờ xem log. Bên Discount: "Discount is retrieved for ProductName: IPhone X, Amount: 150", rồi Samsung với 100, và với Huawei là "No Discount", amount bằng 0. Bên Basket là các dòng tương ứng: "gRPC GetDiscount returned amount 150", v.v. Ba item, ba lời gọi gRPC, và trong code mỗi lời gọi trông như một method C# bình thường.

*[Gửi request 2: lấy basket.]*

Đọc lại thì thấy giá đã giảm được lưu lại. Tổng là 2.930.

*[Gửi request 3: tạo coupon.]*

Giờ mình thay đổi dữ liệu ở phía bên kia. Request này gửi đến Basket, và Basket gọi một gRPC method *khác*, `CreateDiscount`, để thêm coupon giảm 50 cho Huawei. Response là `CouponModel` mới, với id 3: đó là Protobuf message, được chuyển lại thành JSON để mình đọc.

*[Gửi request 4: lưu lại basket Huawei.]*

Và nếu mình lưu lại basket Huawei: **600**. Basket không thay đổi gì; Discount service thay đổi, và Basket nhận được giá mới ở lần gọi tiếp theo.

*Tùy chọn, nếu còn một phút: dừng Discount.Grpc (Ctrl+C) rồi gửi lại request 1. Request sẽ báo lỗi server, vì lời gọi gRPC là synchronous: Basket không thể hoàn thành khi thiếu Discount. Nói: "Đó là temporal coupling ở Part 2, và đó chính là lý do checkout dùng event chứ không dùng lời gọi trực tiếp." Sau đó khởi động lại Discount.Grpc.*

Đó là gRPC trong thực tế: một contract, code được sinh ra ở cả hai phía, và một lời gọi từ xa nhưng đọc như lời gọi local.

---

## Slide 37: Summary  `[53:15]`

Mình tóm tắt lại.

**Microservices** là các service nhỏ, tự chủ, xây quanh các business capability, mỗi service có dữ liệu và deployment riêng. Chỉ áp dụng khi có lý do thực sự.

**DDD** cho mình biết **tách ở đâu**, bằng bounded context và ubiquitous language, và **mô hình hóa** từng service **thế nào**, bằng entity, value object và aggregate.

Với giao tiếp **synchronous**, dùng **REST** cho các API hướng client và **gRPC với Protobuf** cho các lời gọi nội bộ nhanh. Với giao tiếp **asynchronous**, dùng **event** qua **message broker** với pub/sub, cho các workflow decoupled. Và một **API gateway**, YARP hoặc Ocelot, làm cánh cửa trước duy nhất, cùng **BFF** khi có nhiều loại client.

Và trong demo, mọi người đã thấy phần synchronous chạy thật: Basket gọi Discount qua gRPC, với một contract dùng chung.

## Slide 38: References  `[53:30]`

Khóa học, các slide và toàn bộ code được liệt kê ở đây, từ section 4 đến section 20, là hệ thống hoàn chỉnh. Về DDD, cuốn sách của Eric Evans là bản gốc, và e-book miễn phí của Microsoft về kiến trúc .NET microservices là bước tiếp theo tốt nhất. Còn project demo gRPC mình vừa chạy nằm trong thư mục `grpc-demo`, có README hướng dẫn cách chạy.

## Slide 39: Questions  `[60:00]`

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
