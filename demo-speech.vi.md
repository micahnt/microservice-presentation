# Kịch bản thuyết trình: Tactical DDD và gRPC Demo (khoảng 11 phút)

Đi kèm với `demo-slides.html`. Mỗi mục bên dưới tương ứng với một slide. Con số trong ngoặc vuông là **mốc đồng hồ mục tiêu khi bạn rời slide đó**. Các dòng *in nghiêng* là chỉ dẫn sân khấu, không đọc thành lời.

---

## Trước buổi demo (15 phút trước giờ bắt đầu)

*Không cần Docker: demo chỉ cần .NET 8 SDK.*

1. Build trước một lần để không phải compile trên sân khấu: trong thư mục `grpc-demo`, chạy `dotnet build`.
2. Reset dữ liệu: xóa `grpc-demo/Discount.Grpc/discountdb*`, để coupon Huawei từ lần tập thử không còn. File sẽ được tạo lại và seed khi server khởi động.
3. Khởi động hai service trong hai terminal, đặt cạnh nhau để thấy log của cả hai:
   `dotnet run --project Discount.Grpc`, rồi `dotnet run --project Basket.API`
4. Làm nóng (warm up): gửi request 1 trong `demo.http` một lần, để lời gọi gRPC đầu tiên trên sân khấu không bị chậm. Gửi lại chỉ ghi đè basket. **Không** gửi request 3, vì nó tạo coupon.
5. Mở `grpc-demo/demo.http` trong VS Code (extension REST Client) hoặc Visual Studio. Tăng cỡ chữ terminal để đọc được các dòng log.
6. **Phương án dự phòng:** chụp screenshot từng bước demo khi mọi thứ đang chạy tốt. Nếu có lỗi trên sân khấu, chiếu screenshot và tiếp tục nói.

---

## Slide 1: Title  `[0:30]`

Phần này đi sâu hơn một bậc so với bài nói chính. Đầu tiên là cách mô hình hóa bên trong một service bằng tactical DDD, lấy Ordering service làm ví dụ. Sau đó là live demo lời gọi gRPC giữa Basket và Discount.

## Slide 2: Entities, value objects, aggregates  `[1:45]`

Giờ đến tactical DDD: cách mô hình hóa bên trong một service. Ba thành phần cốt lõi.

**Entity** được định danh bằng **ID**. Hai order có dữ liệu giống hệt nhau vẫn là hai order khác nhau, vì ID khác nhau. Entity có **lifecycle**: một order được tạo, được thanh toán, được giao.

**Value object** không có ID. Nó được định nghĩa bởi **các giá trị** của nó. Hai address có cùng street, city và zip code là cùng một address. Và value object là **immutable**: mình không sửa một address, mà thay nó bằng một cái mới.

Trong Ordering service, `Order`, `OrderItem`, `Customer` và `Product` là entity. `Address`, `Payment`, `OrderName`, và thậm chí cả `OrderId` là value object.

**Aggregate** là một cụm các entity và value object được xem là một đơn vị consistency. Entity chính của nó là **aggregate root**, và đó là cổng vào duy nhất. Ở đây, `Order` là root. Không ai thêm order item trực tiếp; mình gọi `Order.Add`, và order tự bảo đảm các quy tắc của nó được giữ, ví dụ quantity và price phải dương.

## Slide 3: Entities and value objects in code  `[2:45]`

Đây là hình dạng của nó trong code Ordering.

Bên trái là base class `Entity`: một ID cộng với các audit field. `Order` kế thừa từ `Aggregate`, tức là một entity có thể raise domain event. Để ý rằng danh sách order item là **private**; bên ngoài chỉ nhận được một bản read-only. Cách duy nhất để thêm một item là method `Add`, method này validate quantity và price trước. Đó chính là aggregate root đang bảo vệ các quy tắc của nó.

Bên phải là các value object. `Address` là một C# **record**, nên hai address có cùng giá trị thì tự động bằng nhau. Nó được tạo qua một factory method tên là `Of`, method này validate input. Và `OrderId` là một **strongly typed ID**: thay vì truyền các GUID thô đi khắp nơi, nơi rất dễ nhầm order ID với customer ID, mỗi loại ID có một type riêng, và một GUID rỗng sẽ bị từ chối.

---

## Slide 4: Choosing an invocation method  `[4:45]`

Giờ mình ráp tất cả lại. Bảng này đọc từ trái sang phải: phương thức, kiểu giao tiếp, phù hợp nhất cho việc gì, và EShop dùng nó ở đâu.

**REST over HTTP**: synchronous, dùng JSON. Phù hợp nhất cho các public API và API hướng client, vì client nào cũng hiểu được. Trong EShop, đó là web app đi qua gateway đến Catalog, Basket và Ordering.

**gRPC**: cũng synchronous, nhưng là binary, chạy trên HTTP/2. Phù hợp nhất cho các lời gọi nội bộ cần nhanh, khi hai bên dùng chung một contract chặt chẽ. Trong EShop, đó là Basket hỏi Discount giá của từng item, cũng chính là phần demo sắp tới.

**Messaging với RabbitMQ**: asynchronous, dùng event. Phù hợp nhất cho các workflow xuyên nhiều service, khi cần decoupling, hoặc fan-out đến nhiều consumer. Trong EShop, đó là checkout: Basket publish, Ordering consume.

Và **API gateway với YARP**: synchronous, là một reverse proxy. Nó không phải cách để các service nói chuyện với nhau, mà là cửa vào duy nhất cho client, nơi đặt routing, rate limiting và security. Trong EShop, Shopping.Web chỉ gọi đến YarpApiGateway.

Hệ thống thực tế thường kết hợp chúng. Lời khuyên để mang về: giữ các chuỗi gọi synchronous **ngắn**, và dùng messaging cho bất cứ thứ gì không cần câu trả lời ngay.

Một phép thử đơn giản mà mọi người có thể dùng: người gọi có cần câu trả lời *ngay bây giờ* để tiếp tục không? Basket cần discount ngay lúc đó để hiển thị giá, nên đó là một synchronous call. Basket không cần số order để hoàn tất checkout, nên đó là một event.

Slide thế là đủ. Giờ xem nó chạy.

---

## Slide 5: Demo setup  `[5:45]`

*(Hai service đã chạy sẵn trong hai terminal đặt cạnh nhau, để thấy log của cả hai. VS Code đang mở `demo.http`.)*

Với phần demo, mình chỉ lấy phần gRPC của khóa học, section 10 đến 12, và cho nó chạy trên laptop chỉ với .NET SDK. Không Docker, không database server.

Có hai project. **Discount.Grpc** là gRPC server: contract `.proto`, `DiscountService`, và một file SQLite nhỏ với hai coupon có sẵn: giảm 150 cho iPhone X và giảm 100 cho Samsung 10. **Basket.API** là client. Mình đã rút gọn nó chỉ còn phần cốt lõi: nó giữ basket trong bộ nhớ, và khi mình lưu một basket, nó gọi Discount qua gRPC cho từng item.

Cả hai project dùng chung một file `discount.proto`. Project server sinh ra server base class, project client sinh ra client. Một contract, hai phía được sinh code.

Và có một điểm khác so với khóa học: ở đây client gọi `localhost:5002` bằng HTTP/2 thường, không có TLS, nên không cần cách xử lý development certificate như trong code của khóa học.

## Slide 6: Demo steps  `[10:45]`

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

*Tùy chọn, nếu còn một phút: dừng Discount.Grpc (Ctrl+C) rồi gửi lại request 1. Request sẽ báo lỗi server, vì lời gọi gRPC là synchronous: Basket không thể hoàn thành khi thiếu Discount. Nói: "Đó là temporal coupling, và đó chính là lý do checkout dùng event chứ không dùng lời gọi trực tiếp." Sau đó khởi động lại Discount.Grpc.*

Đó là gRPC trong thực tế: một contract, code được sinh ra ở cả hai phía, và một lời gọi từ xa nhưng đọc như lời gọi local. Code nằm trong thư mục `grpc-demo`, có README hướng dẫn cách chạy.

---

## Các câu hỏi có thể gặp và câu trả lời ngắn

**"Tại sao tạo OrderId thành một type thay vì dùng thẳng Guid?"**
Đó là tránh cái gọi là "primitive obsession". Nếu mọi ID đều là `Guid`, rất dễ truyền nhầm customer ID vào chỗ cần order ID, và compiler không phát hiện ra. Với strongly typed ID, lỗi đó không compile được, và phần validation, như từ chối ID rỗng, nằm ở đúng một chỗ.
