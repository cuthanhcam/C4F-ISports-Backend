## Thứ tự triển khai các API Endpoint

### 1. Xác thực (Authentication)
**Lý do**: Xác thực là nền tảng, cần triển khai đầu tiên vì hầu hết các endpoint yêu cầu Bearer Token (User, Owner, Admin).

- **1.1 Đăng ký**: `/api/auth/register` (POST)
- **1.2 Đăng nhập**: `/api/auth/login` (POST)
- **1.3 Làm mới Token**: `/api/auth/refresh-token` (POST)
- **1.4 Quên mật khẩu**: `/api/auth/forgot-password` (POST)
- **1.5 Đặt lại mật khẩu**: `/api/auth/reset-password` (POST)
- **1.6 Đăng xuất**: `/api/auth/logout` (POST)
- **1.7 Lấy thông tin người dùng hiện tại**: `/api/auth/me` (GET)
- **1.8 Thay đổi mật khẩu**: `/api/auth/change-password` (POST)
- **1.9 Xác minh email**: `/api/auth/verify-email` (POST)
- **1.10 Gửi lại email xác minh**: `/api/auth/resend-verification` (POST)
- **1.11 Xác minh Token**: `/api/auth/verify-token` (POST)

**Ghi chú triển khai**:
- Đăng ký và đăng nhập tạo tài khoản và xác thực.
- Các endpoint liên quan đến mật khẩu và email đảm bảo bảo mật.
- Lấy thông tin người dùng và xác minh token hỗ trợ kiểm thử.

---

### 2. Quản lý người dùng (User Management)
**Lý do**: Sau khi xác thực, quản lý người dùng cần được triển khai để hỗ trợ cập nhật hồ sơ, theo dõi lịch sử, và các chức năng cá nhân hóa (như yêu thích sân, lịch sử tìm kiếm). Những chức năng này không phụ thuộc vào sân hay đặt chỗ, nhưng cần tài khoản đã xác thực.

- **2.1 Lấy hồ sơ**: `/api/users/profile` (GET)
- **2.2 Cập nhật hồ sơ**: `/api/users/profile` (PUT)
- **2.3 Xóa hồ sơ**: `/api/users/profile` (DELETE)
- **2.9 Lấy điểm tích lũy**: `/api/users/loyalty-points` (GET)
- **2.5 Lấy sân yêu thích**: `/api/users/favorites` (GET)
- **2.6 Thêm sân yêu thích**: `/api/users/favorites` (POST)
- **2.7 Xóa sân yêu thích**: `/api/users/favorites/{fieldId}` (DELETE)
- **2.10 Lấy lịch sử tìm kiếm**: `/api/users/search-history` (GET)
- **2.4 Lấy lịch sử đặt chỗ**: `/api/users/bookings` (GET)
- **2.8 Lấy đánh giá của người dùng**: `/api/users/reviews` (GET)

**Ghi chú triển khai**:
- **Lấy và cập nhật hồ sơ**: Cung cấp thông tin người dùng cơ bản, cần ngay sau xác thực.
- **Xóa hồ sơ**: Đặt sau vì cần kiểm tra ràng buộc (đặt chỗ, sân).
- **Điểm tích lũy**: Hỗ trợ hệ thống khuyến khích, triển khai sớm để tích hợp với đặt chỗ sau.
- **Sân yêu thích**: Cho phép cá nhân hóa, phụ thuộc vào danh sách sân nhưng có thể triển khai trước đặt chỗ.
- **Lịch sử tìm kiếm**: Hỗ trợ cải thiện trải nghiệm người dùng, không phụ thuộc vào đặt chỗ.
- **Lịch sử đặt chỗ và đánh giá**: Phụ thuộc vào đặt chỗ và đánh giá, nên đặt sau cùng trong nhóm này.

---

### 3. Danh mục thể thao (Sport Categories)
**Lý do**: Danh mục thể thao cần được triển khai trước quản lý sân vì sân yêu cầu `sportId`. Các endpoint không yêu cầu xác thực (trừ quản lý admin) nên có thể triển khai sớm để hỗ trợ truy xuất dữ liệu công khai.

- **8.1 Lấy danh sách thể thao**: `/api/sports` (GET)
- **8.2 Lấy thể thao theo ID**: `/api/sports/{sportId}` (GET)
- **8.6 Lấy sân theo thể thao**: `/api/sports/{sportId}/fields` (GET)
- **8.4 Tạo thể thao**: `/api/sports` (POST)
- **8.3 Cập nhật thể thao**: `/api/sports/{sportId}` (PUT)
- **8.5 Xóa thể thao**: `/api/sports/{sportId}` (DELETE)

**Ghi chú triển khai**:
- **Lấy danh sách và chi tiết thể thao**: Hỗ trợ truy xuất công khai, cần cho giao diện người dùng.
- **Lấy sân theo thể thao**: Phụ thuộc vào danh sách sân, nhưng vẫn có thể triển khai sớm để kiểm thử.
- **Tạo, cập nhật, xóa thể thao**: Chỉ dành cho admin, đặt sau vì ít ưu tiên hơn chức năng công khai.

---

### 4. Quản lý sân (Field Management - Core)
**Lý do**: Sân là thực thể cốt lõi, cần sau xác thực và danh mục thể thao để tạo dữ liệu cho đặt chỗ.

- **3.1 Lấy danh sách sân**: `/api/fields` (GET)
- **3.4 Lấy sân theo ID**: `/api/fields/{fieldId}` (GET)
- **3.31 Xác thực địa chỉ**: `/api/fields/validate-address` (POST)
- **3.2 Tạo sân**: `/api/fields` (POST)
- **3.6 Tạo sân đầy đủ**: `/api/fields/full` (POST)
- **3.9 Tạo sân con**: `/api/fields/{fieldId}/subfields` (POST)
- **3.19 Tạo quy tắc giá**: `/api/subfields/{subFieldId}/pricing` (POST)
- **3.15 Tạo dịch vụ sân**: `/api/fields/{fieldId}/services` (POST)
- **3.33 Tạo tiện ích sân**: `/api/fields/{fieldId}/amenities` (POST)
- **3.12 Tải lên ảnh sân**: `/api/fields/services/{fieldServiceId}` (POST)
- **3.24 Thêm mô tả sân**: `/api/fields/{fieldId}/descriptions` (POST)
- **3.3 Cập nhật sân**: `/api/fields/{fieldId}` (PUT)
- **3.7 Cập nhật đầy đủ**: `/api/fields/{fieldId}/full` (PUT)
- **3.10 Cập nhật sân con**: `/api/subfields/{subFieldId}` (PUT)
- **3.20 Cập nhật quy tắc giá**: `/api/subfields/pricing}/{pricingRuleId}` (PUT)
- **3.16 Cập nhật dịch vụ sân**: `/api/fields/services/{fieldServiceId}` (PUT)
- **3.34 Cập nhật tiện ích sân**: `/api/fields/{fieldId}/amenities/{fieldAmenityId}` (PUT)
- **3.25 Cập nhật mô tả sân**: `/api/fields/descriptions/{fieldDescriptionId}` (PUT)
- **3.11 Xóa sân con**: `/api/subfields/{subFieldId}` (DELETE)
- **3.21 Xóa quy tắc giá**: `/api/subfields/pricing/{pricingRuleId}` (DELETE)
- **3.17 Xóa dịch vụ sân**: `/api/fields/services/{fieldServiceId}}` (DELETE)
- **3.35 Xóa tiện ích sân**: `/api/fields/{fieldId}/amenities/{fieldAmenityId}` (DELETE)
- **3.13 Xóa ảnh sân**: `/api/fields/{fieldId}/images/{imageId}` (DELETE)
- **3.26 Xóa mô tả sân**: `/api/fields/descriptions/{fieldDescriptionId}}` (DELETE)
- **3.5 Xóa sân**: `/api/fields/{fieldId}` (DELETE)
- **3.8 Lấy khung giờ trống của sân**: `/api/fields/{fieldId}/availability` (GET)
- **3.14 Lấy ảnh sân**: `/api/fields/{fieldId}/images` (GET)
- **3.18 Lấy dịch vụ sân**: `/api/fields/{fieldId}/services` (GET)
- **3.32 Lấy tiện ích sân**: `/api/fields/{fieldId}/amenities` (GET)
- **3.23 Lấy mô tả sân**: `/api/fields/{fieldId}/descriptions` (GET)
- **3.27 Lấy sân con**: `/api/fields/{fieldId}/subfields` (GET)
- **3.30 Lấy sân con theo ID**: `/api/subfields/{subFieldId}` (GET)
- **3.22 Lấy quy tắc giá**: `/api/subfields/{subFieldId}/pricing` (GET)
- **3.28 Lấy đánh giá sân**: `/api/fields/{fieldId}/reviews` (GET)
- **3.29 Lấy đặt chỗ của sân**: `/api/fields/{fieldId}/bookings` (GET)

**Ghi chú triển khai**:
- Tạo và quản lý sân sau khi danh mục thể thao sẵn sàng.
- Xác thực địa chỉ hỗ trợ tạo sân với tọa độ hợp lệ.
- Các endpoint lấy dữ liệu (khung giờ trống, đánh giá, đặt chỗ) đặt cuối vì phụ thuộc vào các hệ thống khác.

---

### 5. Quản lý khuyến mãi (Promotion Management)
**Lý do**: Khuyến mãi phụ thuộc vào sân và cần trước đặt chỗ để áp dụng giảm giá trong quá trình đặt. Triển khai sau quản lý sân để đảm bảo có dữ liệu sân.

- **9.1 Lấy danh sách khuyến mãi**: `/api/promotions` (GET)
- **9.6 Lấy khuyến mãi theo ID**: `/api/promotions/{promotionId}` (GET)
- **9.2 Tạo khuyến mãi**: `/api/promotions` (POST)
- **9.3 Cập nhật khuyến mãi**: `/api/promotions/{promotionId}` (PUT)
- **9.4 Xóa khuyến mãi**: `/api/promotions/{promotionId}` (DELETE)
- **9.5 Áp dụng khuyến mãi**: `/api/promotions/apply` (POST)

**Ghi chú triển khai**:
- **Lấy danh sách và chi tiết khuyến mãi**: Công khai, hỗ trợ giao diện người dùng.
- **Tạo, cập nhật, xóa khuyến mãi**: Dành cho chủ sân, đặt sau vì phụ thuộc vào sân.
- **Áp dụng khuyến mãi**: Phụ thuộc vào xem trước đặt chỗ, nên đặt cuối nhóm này.

---

### 6. Quản lý đặt chỗ (Booking Management)
**Lý do**: Đặt chỗ phụ thuộc vào sân, sân con, và khuyến mãi. Đây là chức năng cốt lõi để người dùng tương tác.

- **4.9 Xem trước đặt chỗ**: `/api/bookings/preview` (POST)
- **4.7 Tạo đặt chỗ đơn giản**: `/api/bookings/simple` (POST)
- **4.1 Tạo đặt chỗ**: `/api/bookings` (POST)
- **4.10 Thêm dịch vụ vào đặt chỗ**: `/api/bookings/{bookingId}/services` (POST)
- **4.2 Lấy đặt chỗ theo ID**: `/api/bookings/{bookingId}` (GET)
- **4.6 Lấy đặt chỗ của người dùng**: `/api/bookings` (GET)
- **4.8 Lấy dịch vụ của đặt chỗ**: `/api/bookings/{bookingId}/services` (GET)
- **4.3 Cập nhật đặt chỗ**: `/api/bookings/{bookingId}` (PUT)
- **4.4 Xác nhận đặt chỗ**: `/api/bookings/{bookingId}/confirm` (POST)
- **4.11 Lên lịch lại đặt chỗ**: `/api/bookings/{bookingId}/reschedule` (POST)
- **4.5 Hủy đặt chỗ**: `/api/bookings/{bookingId}/cancel` (POST)

**Ghi chú triển khai**:
- Xem trước đặt chỗ tích hợp khuyến mãi để tính giá.
- Tạo và quản lý đặt chỗ là cốt lõi của luồng người dùng.
- Xác nhận và lên lịch lại đặt chỗ hoàn thiện vòng đời đặt chỗ.

---

### 7. Xử lý thanh toán (Payment Processing)
**Lý do**: Thanh toán phụ thuộc vào đặt chỗ, cần triển khai ngay sau để hoàn thành quy trình đặt chỗ. Webhook và hoàn tiền được đặt cuối vì phụ thuộc vào thanh toán thành công.

- **5.1 Tạo thanh toán**: `/api/payments` (POST)
- **5.2 Lấy trạng thái thanh toán**: `/api/payments/{paymentId}` (GET)
- **5.3 Lấy lịch sử thanh toán**: `/api/payments` (GET)
- **5.4 Webhook thanh toán**: `/api/payments/webhook` (POST)
- **5.5 Yêu cầu hoàn tiền**: `/api/payments/{paymentId}/refund` (POST)
- **5.6 Xử lý hoàn tiền**: `/api/payments/refunds/{refundId}/process` (POST)

**Ghi chú triển khai**:
- **Tạo và lấy thanh toán**: Hỗ trợ quy trình thanh toán sau đặt chỗ.
- **Webhook**: Tích hợp với cổng thanh toán để cập nhật trạng thái.
- **Hoàn tiền**: Phụ thuộc vào thanh toán hoàn tất, đặt cuối nhóm này.

---

### 8. Hệ thống đánh giá (Review System)
**Lý do**: Đánh giá phụ thuộc vào đặt chỗ đã xác nhận và thanh toán, nên triển khai sau thanh toán để người dùng có thể đánh giá sân.

- **6.1 Tạo đánh giá**: `/api/reviews` (POST)
- **6.2 Cập nhật đánh giá**: `/api/reviews/{reviewId}` (PUT)
- **6.3 Xóa đánh giá**: `/api/reviews/{reviewId}` (DELETE)
- **6.4 Lấy đánh giá của người dùng**: `/api/reviews` (GET)

**Ghi chú triển khai**:
- Tạo đánh giá yêu cầu đặt chỗ đã thanh toán.
- Cập nhật và xóa đánh giá hỗ trợ quản lý nội dung người dùng.
- Lấy đánh giá giúp hiển thị lịch sử đánh giá cá nhân.

---

### 9. Hệ thống thông báo (Notification System)
**Lý do**: Thông báo phụ thuộc vào các sự kiện như đặt chỗ, thanh toán, và đánh giá. Triển khai sau để thông báo về trạng thái hệ thống.

- **7.1 Lấy thông báo**: `/api/notifications` (GET)
- **7.5 Lấy số lượng thông báo chưa đọc**: `/api/notifications/unread-count` (GET)
- **7.2 Đánh dấu thông báo đã đọc**: `/api/notifications/{notificationId}/read` (POST)
- **7.3 Đánh dấu tất cả thông báo đã đọc**: `/api/notifications/read-all` (POST)
- **7.4 Xóa thông báo**: `/api/notifications/{notificationId}` (DELETE)

**Ghi chú triển khai**:
- Tích hợp thông báo với các sự kiện đặt chỗ, thanh toán, và đánh giá.
- Quản lý trạng thái thông báo hoàn thiện trải nghiệm người dùng.

---

### 10. Bảng điều khiển chủ sân (Owner Dashboard)
**Lý do**: Bảng điều khiển tổng hợp dữ liệu từ sân, đặt chỗ, và thanh toán, nên triển khai sau khi các hệ thống này ổn định.

- **10.1 Lấy thống kê bảng điều khiển**: `/api/owner/dashboard` (GET)
- **10.2 Lấy thống kê sân**: `/api/owner/fields/{fieldId}/stats` (GET)

**Ghi chú triển khai**:
- Cung cấp thông tin phân tích cho chủ sân.
- Phụ thuộc vào dữ liệu từ sân và đặt chỗ.

---

### 11. Thống kê và phân tích (Statistics & Analytics)
**Lý do**: Phân tích yêu cầu dữ liệu tổng hợp từ người dùng, sân, và đặt chỗ, nên triển khai gần cuối khi các hệ thống khác đã hoàn thiện.

- **12.1 Lấy phân tích người dùng**: `/api/analytics/users` (GET)
- **12.2 Lấy phân tích sân**: `/api/analytics/fields` (GET)

**Ghi chú triển khai**:
- Chỉ dành cho admin, cung cấp cái nhìn tổng quan về hệ thống.
- Yêu cầu dữ liệu từ nhiều bảng (`Account`, `Field`, `Booking`).

---

### 12. Quản lý admin (Admin Management)
**Lý do**: Admin quản lý toàn bộ hệ thống, triển khai cuối cùng khi các thành phần khác đã ổn định.

- **11.1 Lấy tất cả người dùng**: `/api/admin/users` (GET)
- **11.2 Lấy người dùng theo ID**: `/api/admin/users/{accountId}` (GET)
- **11.3 Cập nhật người dùng**: `/api/admin/users/{accountId}` (PUT)
- **11.4 Xóa người dùng**: `/api/admin/users/{accountId}` (DELETE)
- **11.5 Lấy tất cả sân**: `/api/admin/fields` (GET)
- **11.6 Cập nhật trạng thái sân**: `/api/admin/fields/{fieldId}/status` (PUT)
- **11.8 Quản lý hoàn tiền**: `/api/admin/refunds` (GET)
- **11.7 Lấy thống kê hệ thống**: `/api/admin/stats` (GET)

**Ghi chú triển khai**:
- Quản lý người dùng, sân, và hoàn tiền phụ thuộc vào các hệ thống khác.
- Thống kê hệ thống tổng hợp dữ liệu toàn diện.

---

## Ghi chú triển khai tổng quát
- **Quản lý người dùng**: Đặt sau xác thực vì cần tài khoản để quản lý hồ sơ, yêu thích, và lịch sử. Các endpoint như lịch sử đặt chỗ và đánh giá phụ thuộc vào đặt chỗ và đánh giá, nên nằm ở cuối nhóm này.
- **Danh mục thể thao**: Đặt trước quản lý sân vì sân yêu cầu `sportId`. Các endpoint công khai được ưu tiên để hỗ trợ giao diện người dùng sớm.
- **Quản lý khuyến mãi**: Đặt sau quản lý sân vì phụ thuộc vào `fieldId`, nhưng trước đặt chỗ để hỗ trợ áp dụng giảm giá.
- **Xử lý thanh toán**: Đặt ngay sau đặt chỗ để hoàn thành quy trình đặt chỗ. Hoàn tiền và webhook phụ thuộc vào thanh toán, nên ở cuối nhóm này.
- **Hệ thống đánh giá**: Đặt sau thanh toán vì yêu cầu đặt chỗ đã thanh toán. Đánh giá là chức năng bổ sung, không cốt lõi như đặt chỗ.
- **Thống kê và phân tích**: Đặt gần cuối vì tổng hợp dữ liệu từ nhiều hệ thống, chỉ cần thiết khi các thành phần khác đã ổn định.
- **Cơ sở dữ liệu**: Đảm bảo schema hỗ trợ các thực thể mới như `Sport`, `Promotion`, `Payment`, `Review`, `SearchHistory`, `LoyaltyPoints`. Thiết lập quan hệ giữa các bảng (`Field.SportId`, `Booking.PromotionId`, `Payment.BookingId`).
- **Xác thực và phân quyền**: Sử dụng middleware JWT để kiểm tra vai trò (`User`, `Owner`, `Admin`) và quyền truy cập (ví dụ: chỉ admin cập nhật danh mục thể thao).
- **Tích hợp bên thứ ba**: 
  - Thanh toán: Tích hợp cổng thanh toán (Stripe, VNPay) cho `/api/payments` và `/api/payments/webhook`.
  - Geocoding: Sử dụng Google Maps API cho `/api/fields/validate-address`.
- **Hiệu suất**: 
  - Sử dụng phân trang (`page`, `pageSize`) cho các endpoint danh sách (`/api/users/bookings`, `/api/reviews`, `/api/payments`).
  - Cache dữ liệu công khai như danh mục thể thao (`/api/sports`) và khuyến mãi (`/api/promotions`).
- **Kiểm thử**: 
  - Viết unit test cho xác thực, quản lý sân, đặt chỗ, và thanh toán trước.
  - Kiểm tra tích hợp cho webhook thanh toán và áp dụng khuyến mãi.
- **Thông báo tự động**: Tích hợp hệ thống thông báo với các sự kiện như thanh toán thành công, đánh giá mới, hoặc khuyến mãi được áp dụng.
- **Bảo mật**: 
  - Xác minh chữ ký cho webhook thanh toán (`/api/payments/webhook`).
  - Kiểm tra ràng buộc (ví dụ: không cho xóa người dùng có đặt chỗ đang hoạt động).
