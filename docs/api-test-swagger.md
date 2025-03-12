# Hướng Dẫn Test API Giai Đoạn 2: Authentication & Authorization Trên Swagger

## Giới thiệu

Đây là hướng dẫn sử dụng Swagger để test các API Authentication & Authorization trong Giai đoạn 2 của dự án **C4F-ISports**. Các API này được triển khai trên backend tại URL `https://localhost:5231`.

---

## 1. Chuẩn bị

### Chạy ứng dụng backend

1. Mở terminal, vào thư mục dự án backend:
   ```bash
   cd D:\Workspace\C4F-ISports\C4F-ISports-Backend
   ```
2. Chạy lệnh:
   ```bash
   dotnet run
   ```
3. Đảm bảo ứng dụng chạy trên `https://localhost:5231` (kiểm tra terminal để xác nhận port).

### Truy cập Swagger

1. Mở trình duyệt (Chrome/Edge), nhập: `https://localhost:5231/swagger`.
2. Bạn sẽ thấy giao diện Swagger với danh sách các API trong nhóm `Auth`.

### Database

Đảm bảo database `c4fisports` đã được tạo và migrations đã được áp dụng:
```bash
dotnet ef database update
```

---

## 2. Danh sách API Endpoints

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh-token`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/auth/logout`
- `GET /api/auth/verify-token`

---

## 3. Hướng dẫn test từng endpoint

### 3.1. Đăng ký tài khoản mới
#### `POST /api/auth/register`

- **Mô tả**: Đăng ký tài khoản mới (`User` hoặc `Owner`).
- **Bước thực hiện**:
  1. Mở `POST /api/auth/register` trong Swagger.
  2. Nhấn **Try it out**.
  3. Nhập dữ liệu:
     ```json
     {
       "email": "testuser5@example.com",
       "password": "Password123!",
       "role": "User",
       "fullName": "Test User 5",
       "phone": "0123456789"
     }
     ```
  4. Nhấn **Execute**.
- **Response mong đợi**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "b8f6c8d2-4e7a-4d9f-8e2b-5f9d8c7e6b4a"
  }
  ```
- **Lưu ý**: Nếu nhận lỗi `"Email already exists"`, đổi sang email khác (ví dụ: `testuser6@example.com`).

---

### 3.2. Đăng nhập tài khoản
#### `POST /api/auth/login`

- **Mô tả**: Đăng nhập bằng tài khoản vừa đăng ký.
- **Bước thực hiện**:
  1. Mở `POST /api/auth/login`.
  2. Nhấn **Try it out**.
  3. Nhập dữ liệu:
     ```json
     {
       "email": "testuser5@example.com",
       "password": "Password123!"
     }
     ```
  4. Nhấn **Execute**.
- **Response mong đợi**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4-5e6f-7g8h-9i0j-k1l2m3n4o5p6"
  }
  ```

---

### 3.3. Làm mới token
#### `POST /api/auth/refresh-token`

- **Mô tả**: Làm mới token khi token cũ hết hạn.
- **Bước thực hiện**:
  1. Mở `POST /api/auth/refresh-token`.
  2. Nhấn **Try it out**.
  3. Nhập vào Request body:
     ```json
     "a1b2c3d4-5e6f-7g8h-9i0j-k1l2m3n4o5p6"
     ```
  4. Nhấn **Execute**.

---

### 3.4. Quên mật khẩu
#### `POST /api/auth/forgot-password`

- **Mô tả**: Yêu cầu reset mật khẩu.
- **Bước thực hiện**:
  1. Mở `POST /api/auth/forgot-password`.
  2. Nhấn **Try it out**.
  3. Nhập email:
     ```text
     testuser5@example.com
     ```
  4. Nhấn **Execute**.
- **Kiểm tra database**:
  ```sql
  SELECT ResetToken, ResetTokenExpiry FROM Accounts WHERE Email = 'testuser5@example.com';
  ```
  Lưu `ResetToken` để sử dụng trong `/reset-password`.

---

### 3.5. Đặt lại mật khẩu
#### `POST /api/auth/reset-password`

- **Mô tả**: Đặt lại mật khẩu bằng reset token.
- **Bước thực hiện**:
  1. Mở `POST /api/auth/reset-password`.
  2. Nhập vào Request body:
     ```json
     {
       "email": "testuser5@example.com",
       "token": "550e8400-e29b-41d4-a716-446655440000",
       "newPassword": "NewPassword456!"
     }
     ```
  3. Nhấn **Execute**.

---

### 3.6. Đăng xuất
#### `POST /api/auth/logout`

- **Mô tả**: Đăng xuất, revoke refresh token.
- **Bước thực hiện**:
  1. Mở `POST /api/auth/logout`.
  2. Nhập vào Request body:
     ```json
     "x9y8z7w6-5v4u3t2-1s0r9q8-p7o6n5m4l3k2"
     ```
  3. Nhấn **Execute**.
- **Kiểm tra database**:
  ```sql
  SELECT Revoked FROM RefreshTokens WHERE Token = 'x9y8z7w6-5v4u3t2-1s0r9q8-p7o6n5m4l3k2';
  ```

---

## 4. Kịch bản test đầy đủ

1. **Đăng ký**: `/register`
2. **Đăng nhập**: `/login`
3. **Xác minh token**: `/verify-token`
4. **Làm mới token**: `/refresh-token`
5. **Quên mật khẩu**: `/forgot-password`
6. **Đặt lại mật khẩu**: `/reset-password`
7. **Đăng nhập lại**: `/login`
8. **Đăng xuất**: `/logout`

---

## 5. Lưu ý quan trọng cho Frontend
- **Headers**: `Content-Type: application/json`
- **Token**: Dùng cho các API bảo vệ
- **Xử lý lỗi**: Kiểm tra response để hiển thị thông báo phù hợp

---

## 6. Kết luận
Hướng dẫn này giúp nhóm Frontend hiểu cách hoạt động của các API Authentication & Authorization.

