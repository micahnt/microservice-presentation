# Demo gRPC: Basket gọi Discount (không dùng Docker)

Hai service chạy trực tiếp trên máy bằng `dotnet run`. Chỉ cần .NET 8 SDK, không cần Docker hay database server.


```
 REST / JSON                      gRPC (HTTP/2 + Protobuf)
 Postman ──────► Basket.API ─────────────────────────────► Discount.Grpc ──► SQLite (file discountdb)
                 :5001           GetDiscount / CreateDiscount   :5002
                 (client gRPC, giỏ hàng lưu trong RAM)          (server gRPC)
```

| Project | Vai trò |
|---|---|
| `Discount.Grpc` | **gRPC server**: contract `discount.proto`, `DiscountService`, EF Core + SQLite | 
| `Basket.API` | **gRPC client**: nhận request REST, gọi Discount qua gRPC để trừ giảm giá cho từng item | 

## 1. Yêu cầu

- **.NET 8 SDK** (kiểm tra: `dotnet --version`, kết quả bắt đầu bằng `8.`)
- Một công cụ gửi request, chọn một trong ba: **Postman**, VS Code với extension **REST Client**, hoặc **curl**

## 2. Cấu trúc thư mục

```
grpc-demo/
├── Discount.Grpc/                  gRPC server (port 5002)
│   ├── Protos/discount.proto       contract Protobuf
│   ├── Services/DiscountService.cs implement 4 method gRPC
│   ├── Data/, Models/, Migrations/ EF Core + SQLite, dữ liệu mẫu
│   └── appsettings.json            connection string + ép HTTP/2
├── Basket.API/                     client (port 5001)
│   └── Program.cs                  toàn bộ API + lời gọi gRPC (~50 dòng)
├── GrpcDemo.postman_collection.json   5 request kèm test cho Postman
├── demo.http                          cùng 5 request cho REST Client
└── GrpcDemo.sln
```

## 3. Cách chạy

### Bước 1: Build (làm trước một lần)

```bash
cd grpc-demo
dotnet build
```

Kết quả phải là `Build succeeded`.

### Bước 2: Reset dữ liệu (trước mỗi lần demo)

Xóa các file `Discount.Grpc/discountdb*`. Đây là file SQLite chứa coupon; nó được tạo lại cùng 2 coupon mẫu khi server khởi động. Xem mục 6 để biết vì sao cần reset.

### Bước 3: Chạy 2 service (2 terminal, đặt cạnh nhau)

```bash
# Terminal 1: gRPC server, chạy trước
dotnet run --project Discount.Grpc
# đợi dòng: Now listening on: http://localhost:5002

# Terminal 2: client
dotnet run --project Basket.API
# đợi dòng: Now listening on: http://localhost:5001
```

Giữ cả hai terminal mở: khi demo sẽ chỉ vào log của cả hai.

Dữ liệu mẫu có sẵn (tạo ở lần chạy đầu): **IPhone X giảm 150**, **Samsung 10 giảm 100**. Sản phẩm khác không có coupon ("No Discount", amount 0).

## 4. Các cách gửi request

### Postman (khuyên dùng)

1. **Import** → chọn `GrpcDemo.postman_collection.json`.
2. Collection có 5 request đánh số 1–5. Biến `basket` = `http://localhost:5001` và `userName` = `swn` đã đặt sẵn.
3. Chạy từng request theo thứ tự, hoặc bấm **Run collection** để chạy cả 5 cùng lúc. Mỗi request có test kiểm tra giá, kết quả hiện ở tab **Test Results**.

### VS Code REST Client

Mở `demo.http`, bấm **Send Request** phía trên từng dòng `###`.

### curl

```bash
curl -X POST http://localhost:5001/basket -H "Content-Type: application/json" \
  -d '{"userName":"swn","items":[{"productId":"5334c996-8457-4cf0-815c-ed2b77c4ff61","productName":"IPhone X","price":950,"quantity":1,"color":"Black"}]}'
# → giá còn 800 (950 − 150)
```

## 5. Kịch bản demo

| # | Request | Điều xảy ra phía sau | Kết quả mong đợi |
|---|---|---|---|
| 1 | `POST /basket`: IPhone X 950, Samsung 10 840 ×2, Huawei Plus 650 | Basket gọi gRPC **`GetDiscount` 3 lần**, mỗi item một lần | IPhone X **800**, Samsung 10 **740**, Huawei **650** (chưa có coupon), trả về `201` |
| 2 | `GET /basket/swn` | Chỉ đọc lại, không gọi gRPC | Giá đã giảm được lưu, `totalPrice` = **2930** |
| 3 | `POST /discount`: Huawei Plus, amount 50 | Basket gọi gRPC **`CreateDiscount`** | Coupon mới `id: 3`, amount 50 |
| 4 | `POST /basket`: Huawei Plus 650 | Basket gọi lại `GetDiscount` | Huawei giờ còn **600** (650 − 50) |
| 5 | `DELETE /basket/swn` | Xóa giỏ hàng | `204 No Content` |

Điểm cần nhấn mạnh khi demo:

- Postman chỉ gọi **REST/JSON** vào Basket. Việc Basket hỏi Discount là **gRPC**: nhị phân, HTTP/2, theo contract trong `discount.proto`.
- Bước 3 đổi dữ liệu ở **service bên kia**, và Basket nhận giá mới ở lần gọi tiếp theo mà không cần sửa gì.

### Log cần chỉ vào

- **Terminal 1 (Discount.Grpc):** `Discount is retrieved for ProductName : IPhone X, Amount : 150`. Với Huawei trước khi có coupon: `ProductName : No Discount, Amount : 0`. Sau bước 3: `Discount is successfully created. ProductName : Huawei Plus`.
- **Terminal 2 (Basket.API):** `gRPC GetDiscount(IPhone X) returned amount 150`.

### Tùy chọn: minh họa temporal coupling

Tắt Discount.Grpc (`Ctrl+C` ở Terminal 1) rồi gửi lại request 1. Basket trả lỗi **500**, và log có `RpcException ... StatusCode="Unavailable"`. Vì lời gọi gRPC là synchronous nên Basket không hoàn thành được khi thiếu Discount. Đây là lý do luồng checkout dùng event bất đồng bộ thay cho lời gọi trực tiếp. Sau đó chạy lại Discount.Grpc, và nhớ reset dữ liệu trước lần demo kế tiếp.

## 6. Vì sao phải reset `discountdb`

`discountdb` là file SQLite do `Discount.Grpc` tạo ra khi khởi động lần đầu: tên file lấy từ connection string `Data Source=discountdb` trong `appsettings.json`, bảng và 2 coupon mẫu được tạo bởi migration lúc `UseMigration()` chạy. Các file `discountdb-shm` và `discountdb-wal` là file tạm của SQLite, tự xuất hiện khi service chạy.

Nếu không xóa sau lần demo trước thì **không có lỗi khởi động** (migration đã áp dụng nên bị bỏ qua), nhưng dữ liệu cũ vẫn còn:

- Coupon Huawei từ lần trước còn đó, nên request 1 cho Huawei **600** thay vì 650, và test "still 650" của Postman fail.
- Request 3 vẫn chạy được và tạo thêm coupon Huawei thứ hai (bảng không có ràng buộc unique cho `ProductName`), còn `GetDiscount` chỉ lấy coupon khớp đầu tiên.

Cách reset: tắt Discount.Grpc, xóa `Discount.Grpc/discountdb*`, chạy lại.

## 7. Lỗi thường gặp

| Hiện tượng | Cách xử lý |
|---|---|
| Postman báo `ECONNREFUSED` | Basket.API chưa chạy hoặc sai port. Kiểm tra Terminal 2 và biến `basket`. |
| Request 1 trả `500` | Discount.Grpc chưa chạy hoặc đã tắt. Chạy Terminal 1 trước rồi gửi lại. |
| Giá Huawei ở request 1 là 600, hoặc request 4 vẫn 650 | Dữ liệu chưa reset (mục 6), hoặc request 3 chưa chạy. Reset rồi chạy lại từ request 1. |
| `address already in use` | Port 5001 hoặc 5002 đang bị chiếm. Tắt lần chạy cũ rồi chạy lại. |
| `dotnet build` lỗi | Kiểm tra `dotnet --version` là 8.x. |

## 8. Từng khái niệm nằm ở đâu

| Khái niệm | File | Bài giảng khóa học |
|---|---|---|
| Contract Protobuf (`service`, `message`, field number) | `Discount.Grpc/Protos/discount.proto` | 152 |
| Sinh code server từ `.proto` | `Discount.Grpc/Discount.Grpc.csproj` (`GrpcServices="Server"`) | 153 |
| Implement service (override base class được sinh ra) | `Discount.Grpc/Services/DiscountService.cs` | 154, 160–163 |
| Đăng ký gRPC vào pipeline | `Discount.Grpc/Program.cs` (`AddGrpc`, `MapGrpcService`) | 155 |
| HTTP/2 không TLS | `Discount.Grpc/appsettings.json` (`Kestrel:EndpointDefaults:Protocols = Http2`) | 155 |
| Dùng chung contract, chỉ sinh phần client | `Basket.API/Basket.API.csproj` (`GrpcServices="Client"`, file được link) | 168–169 |
| Đăng ký typed gRPC client | `Basket.API/Program.cs` (`AddGrpcClient`) | 170 |
| Gọi như một method C# bình thường | `Basket.API/Program.cs` (`GetDiscountAsync`) | 168 |
