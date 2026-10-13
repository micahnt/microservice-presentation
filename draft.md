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

**Nếu đến lúc demo mà bạn trễ hơn 2 phút**, chỉ gửi request 1 và chỉ vào hai cửa sổ log; bỏ qua request 2–4.

---

## Trước buổi nói (15 phút trước giờ bắt đầu)

*Không cần Docker: demo chỉ cần .NET 8 SDK.*

1. Build trước một lần để không phải compile trên sân khấu: trong thư mục `grpc-demo`, chạy `dotnet build`.
2. Reset dữ liệu: xóa `grpc-demo/Discount.Grpc/discountdb*`, để coupon Huawei từ lần tập thử không còn. File sẽ được tạo lại và seed khi server khởi động.
3. Khởi động hai service trong hai terminal, đặt cạnh nhau để thấy log của cả hai:
   `dotnet run --project Discount.Grpc`, rồi `dotnet run --project Basket.API`
4. Làm nóng (warm up): gửi request 1 trong `demo.http` một lần, để lời gọi gRPC đầu tiên trên sân khấu không bị chậm. Gửi lại chỉ ghi đè basket. **Không** gửi request 3, vì nó tạo coupon.
5. Mở `grpc-demo/demo.http` trong VS Code (extension REST Client) hoặc Visual Studio. Tăng cỡ chữ terminal để đọc được các dòng log.
6. **Phương án dự phòng:** chụp screenshot từng bước demo khi mọi thứ đang chạy tốt. Nếu có lỗi trên sân khấu, chiếu screenshot và tiếp tục nói.

---


Ví dụ thực tế về một hệ thống Mixed Architecture

Hãy tưởng tượng bạn đang xây dựng một ứng dụng Thương mại điện tử (E-commerce) lớn. Hệ thống của bạn có thể được chia như sau:
• Khối Monolith (hoặc Modular Monolith): Dùng cho các tính năng Lõi (Core Business) như Quản lý sản phẩm, Quản lý tài khoản, Đơn hàng và Thanh toán. Đây là những tính năng có logic nghiệp vụ rất phức tạp, dữ liệu quan hệ chặt chẽ, cần độ an toàn cao và ít khi thay đổi cấu trúc đột ngột.
• Khối Microservices: Dùng cho các tính năng cần độc lập và scale (mở rộng) mạnh mẽ như hệ thống Chat giữa người mua - người bán, hoặc hệ thống Gợi ý sản phẩm (Recommendation System) chạy bằng AI. Các dịch vụ này có thể viết bằng các ngôn ngữ khác nhau (như Python cho AI, Go cho Chat) để tối ưu hiệu năng.
• Khối Serverless (Functions): Dùng cho các tác vụ chạy theo sự kiện (Event-driven) và không liên tục. Ví dụ: Khi người dùng tải ảnh sản phẩm lên, một hàm Serverless (như AWS Lambda) sẽ tự động kích hoạt để nén ảnh và tạo ảnh thu nhỏ (thumbnail), sau đó tự tắt đi để tiết kiệm chi phí.

---

Hệ thống thực tế thường kết hợp chúng. Lời khuyên để mang về: giữ các chuỗi gọi synchronous **ngắn**, và dùng messaging cho bất cứ thứ gì không cần câu trả lời ngay.

Một phép thử đơn giản mà mọi người có thể dùng: người gọi có cần câu trả lời *ngay bây giờ* để tiếp tục không? Basket cần discount ngay lúc đó để hiển thị giá, nên đó là một synchronous call. Basket không cần số order để hoàn tất checkout, nên đó là một event. 
   (• Để hiển thị giá lúc đó: Basket bắt buộc phải gọi sang service Discount để lấy giá giảm ngay lập tức (Synchronous). Không có giá giảm, Basket không thể hiển thị đúng số tiền cho user bấm nút.
   • Khi user bấm Checkout: Nhiệm vụ của Basket đến đây là KẾT THÚC. Nó chỉ cần gom đống sản phẩm + tổng tiền lại, ném một thông báo (Event) ra hệ thống: "User X vừa bấm đặt hàng đống này nè!".

   Basket không quan tâm và không cần biết số Order ID là gì, vì việc sinh ra số Order, trừ kho, giao hàng... là việc của các service phía sau. Basket có thể lập tức xóa giỏ hàng trống và báo cho user: "Đang xử lý đơn hàng" mà không cần đứng đợi xem số order tạo ra là số mấy.)