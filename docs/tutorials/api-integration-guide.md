# Hướng Dẫn Tích Hợp API C4F-ISports v2.0.0

## 1. Tổng Quan
Tài liệu này hướng dẫn frontend team tích hợp với backend APIs của C4F-ISports v2.0.0. APIs sử dụng **REST** với base URL `/api/v2`, định dạng JSON, và xác thực qua **JWT**. Tất cả endpoints được mô tả chi tiết trong `api-endpoints.markdown` và Swagger (`/swagger`).

## 2. Cấu Hình Ban Đầu
- **Base URL**: `https://api.c4f-isports.com/api/v2` (production) hoặc `https://localhost:5231/api/v2` (development).
- **Content-Type**: `application/json`.
- **Authorization**: Bearer token trong header `Authorization`.
- **Environment Variables**:
  - `API_BASE_URL`: Base URL của backend.
  - `GOOGLE_CLIENT_ID`: Cho OAuth2 login.

## 3. Xác Thực
### 3.1. Đăng Nhập (Local)
- **Endpoint**: `POST /api/auth/login`
- **Request**:
  ```json
  {
    "email": "user@example.com",
    "password": "password123"
  }
  ```
- **Response** (200 OK):
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "abc123xyz456...",
    "role": "User",
    "expiresIn": 3600
  }
  ```
- **Lưu ý**:
  - Lưu `token` trong localStorage hoặc secure cookie.
  - Lưu `refreshToken` để làm mới token khi hết hạn.

### 3.2. Đăng Nhập qua Google (OAuth2)
- **Endpoint**: `POST /api/auth/oauth/google`
- **Request**:
  ```json
  {
    "accessToken": "google_access_token"
  }
  ```
- **Response** (200 OK): Tương tự đăng nhập local.
- **Lưu ý**:
  - Sử dụng Google Sign-In SDK để lấy `accessToken`.
  - Gửi `accessToken` tới backend để nhận JWT.

### 3.3. Làm Mới Token
- **Endpoint**: `POST /api/auth/refresh`
- **Request**:
  ```json
  {
    "refreshToken": "abc123xyz456"
  }
  ```
- **Response** (200 OK):
  ```json
  {
    "token": "new_jwt_token",
    "refreshToken": "new_refresh_token",
    "expiresIn": 3600
  }
  ```
- **Lưu ý**: Gọi endpoint này khi token hết hạn (HTTP 401).

### 3.4. Header Xác Thực
- Thêm header cho các endpoint yêu cầu xác thực:
  ```http
  Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
  ```

## 4. Ví Dụ Tích Hợp Các Tính Năng Chính

### 4.1. Tìm Kiếm Sân
- **Endpoint**: `GET /api/fields?latitude=10.776&longitude=106.700&radius=5`
- **Response** (200 OK):
  ```json
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "Sân ABC",
        "address": "123 Đường Láng, HCM",
        "latitude": 10.776,
        "longitude": 106.700,
        "openHours": "06:00-22:00",
        "averageRating": 4.5,
        "subFields": [
          {
            "subFieldId": 1,
            "subFieldName": "Sân 5 người",
            "fieldType": "5-a-side",
            "status": "Available"
          }
        ]
      }
    ],
    "total": 1,
    "skip": 0,
    "take": 10
  }
  ```
- **Lưu ý**: Sử dụng query params `latitude`, `longitude`, `radius` để lọc sân theo vị trí.

### 4.2. Đặt Sân
- **Endpoint**: `POST /api/bookings`
- **Request**:
  ```json
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00",
    "endTime": "09:00",
    "services": [
      {
        "fieldServiceId": 1,
        "quantity": 2
      }
    ],
    "promotionCode": "SUMMER2025"
  }
  ```
- **Response** (201 Created):
  ```json
  {
    "bookingId": 1,
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "totalPrice": 300000,
    "status": "Pending",
    "paymentStatus": "Pending",
    "promotionId": 1
  }
  ```
- **Lưu ý**:
  - Kiểm tra khung giờ trống trước bằng `GET /api/bookings/availability`.
  - Sau khi tạo booking, gọi `/api/payments` để thanh toán.

### 4.3. Thanh Toán
- **Endpoint**: `POST /api/payments`
- **Request**:
  ```json
  {
    "bookingId": 1,
    "paymentMethod": "VNPay"
  }
  ```
- **Response** (200 OK):
  ```json
  {
    "paymentUrl": "https://sandbox.vnpayment.vn/..."
  }
  ```
- **Lưu ý**:
  - Chuyển hướng người dùng tới `paymentUrl`.
  - Lắng nghe webhook hoặc polling `/api/bookings/{id}` để kiểm tra `paymentStatus`.

### 4.4. Đánh Giá Sân
- **Endpoint**: `POST /api/reviews`
- **Request**:
  ```json
  {
    "fieldId": 1,
    "rating": 5,
    "comment": "Sân sạch sẽ, nhân viên thân thiện!"
  }
  ```
- **Response** (201 Created):
  ```json
  {
    "reviewId": 1,
    "fieldId": 1,
    "rating": 5,
    "comment": "Sân sạch sẽ, nhân viên thân thiện!",
    "createdAt": "2025-05-11T10:00:00Z"
  }
  ```
- **Lưu ý**: Chỉ người dùng đã đặt sân mới được đánh giá.

## 5. Xử Lý Lỗi
- **HTTP Status Codes**:
  - `200 OK`: Thành công.
  - `201 Created`: Tạo tài nguyên thành công.
  - `400 Bad Request`: Request không hợp lệ (validation error).
  - `401 Unauthorized`: Thiếu hoặc token không hợp lệ.
  - `403 Forbidden`: Không có quyền truy cập.
  - `404 Not Found`: Tài nguyên không tồn tại.
  - `500 Internal Server Error`: Lỗi server.
- **Error Response**:
  ```json
  {
    "error": {
      "code": "ValidationError",
      "message": "Email is required",
      "details": [
        {
          "field": "email",
          "error": "Required"
        }
      ]
    }
  }
  ```
- **Lưu ý**: Luôn kiểm tra `error.message` và `error.details` để hiển thị thông báo cho người dùng.

## 6. Lưu Ý Khi Tích Hợp
- **Token Expiration**: Làm mới token bằng `/api/auth/refresh` khi nhận lỗi 401.
- **Pagination**: Sử dụng `skip` và `take` để phân trang dữ liệu (e.g., `/api/fields?skip=0&take=10`).
- **Rate Limiting**: Một số endpoint có giới hạn request (e.g., 100 request/phút). Kiểm tra header `X-RateLimit-Remaining`.
- **Caching**: Cache dữ liệu `Field`, `SubField` phía client để giảm request (TTL 1 giờ).
- **Mock API**: Sử dụng Swagger (`/swagger`) để mock API trong giai đoạn phát triển.

## 7. Công Cụ Hỗ Trợ
- **Swagger**: `https://<host>/swagger` để test API.
- **Postman Collection**: Sẽ được cung cấp sau Giai đoạn 2.
- **Mock Data**: Dữ liệu mẫu trong `Seeders` (e.g., `SubFieldSeeder.cs`).
- **Contact**: Liên hệ backend lead qua Slack (#backend-channel) nếu gặp vấn đề.

## 8. Liên Kết Tài Liệu
- **API Endpoints**: `api-endpoints.markdown`.
- **Database Schema**: `database-schema.markdown`.
- **Models**: `Models-v2.0.0.cs`.
- **Architecture Overview**: `architecture-overview.md`.