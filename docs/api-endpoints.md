# API Endpoints Specification

This document outlines the API endpoints for the sports field booking system, aligned with `Models-v2.0.0.cs`. All endpoints use JSON for request/response bodies, require HTTPS, and follow RESTful conventions. Authentication uses JWT Bearer Tokens unless specified otherwise.

## Table of Contents

1. [Authentication](#1-authentication)
2. [User Management](#2-user-management)
3. [Sport Categories](#3-sport-categories)
4. [Field Management](#4-field-management)
5. [Promotion Management](#5-promotion-management)
6. [Booking Management](#6-booking-management)
7. [Payment Processing](#7-payment-processing)
8. [Review System](#8-review-system)
9. [Notification System](#9-notification-system)
10. [Owner Dashboard](#10-owner-dashboard)
11. [Statistics & Analytics](#11-statistics--analytics)
12. [Admin Management](#12-admin-management)

---

## 1. Authentication

### 1.1 Register

**Description**: Creates a new user or owner account and sends a verification email.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/register`  
**Authorization**: None

**Request Body**:

```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "fullName": "Nguyen Van A",
  "phone": "0909123456",
  "role": "User" // or "Owner"
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "accountId": 123,
    "token": "...",
    "refreshToken": "...",
    "message": "Tài khoản đã được đăng ký thành công. Vui lòng xác minh email của bạn."
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "Mật khẩu và xác nhận mật khẩu không khớp."
  }
  ```

- **409 Conflict**:

  ```json
  {
    "error": "Email đã được đăng ký",
    "details": "Email đã tồn tại."
  }
  ```

- **500 Internal Server Error**:

  ```json
  {
    "error": "Lỗi hệ thống",
    "details": "Không thể gửi email xác minh."
  }
  ```

**Note**:

- `role` must be `"User"` or `"Owner"`. Admin role registration is not allowed.
- Sends a verification email with a link containing `email` and `token`.
- `Account.Email` is unique.
- Uses `BCrypt` for password hashing.

### 1.2 Login

**Description**: Authenticates a user or owner and returns tokens.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/login`  
**Authorization**: None

**Request Body**:

```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "token": "...",
    "refreshToken": "...",
    "expiresIn": 3600,
    "role": "User",
    "message": "Đăng nhập thành công"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "..."
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Không được phép",
    "details": "Email hoặc mật khẩu không đúng."
  }
  ```

**Note**:

- Updates `Account.LastLogin` on successful login.
- Checks if account is active (`IsActive = true`).
- `role` indicates the account type (`User` or `Owner`).

### 1.3 Refresh Token

**Description**: Refreshes the access token using a refresh token.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/refresh-token`  
**Authorization**: None

**Request Body**:

```json
{
  "refreshToken": "...refresh_token"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "token": "...",
    "refreshToken": "...",
    "expiresIn": 3600,
    "message": "Token đã được làm mới thành công"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "..."
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Không được phép",
    "details": "Refresh token không hợp lệ hoặc đã hết hạn."
  }
  ```

**Note**:

- Revokes the old refresh token and generates a new one.
- Validates `RefreshToken.Expires` and `RefreshToken.Revoked`.

### 1.4 Forgot Password

**Description**: Sends a password reset email with a link.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/forgot-password`  
**Authorization**: None

**Request Body**:

```json
{
  "email": "user@example.com"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Email đặt lại mật khẩu đã được gửi"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "..."
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Email không tồn tại",
    "details": "Email không tồn tại."
  }
  ```

**Note**:

- Generates `Account.ResetToken` and `Account.ResetTokenExpiry` (1-hour validity).
- Sends a reset link to the provided email.

### 1.5 Reset Password

**Description**: Resets the password using a reset token.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/reset-password`  
**Authorization**: None

**Request Body**:

```json
{
  "email": "user@example.com",
  "token": "...",
  "newPassword": "NewPassword123!"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Mật khẩu đã được đặt lại thành công"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "..."
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Token không hợp lệ hoặc đã hết hạn",
    "details": "Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn."
  }
  ```

**Note**:

- Validates `Account.ResetToken` and `Account.ResetTokenExpiry`.
- Clears `Account.ResetToken` and `Account.ResetTokenExpiry` after successful reset.
- Uses `BCrypt` to hash the new password.

### 1.6 Logout

**Description**: Invalidates the refresh token for the authenticated user.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/logout`  
**Authorization**: Bearer Token

**Request Body**:

```json
{
  "refreshToken": "...refresh_token"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Đăng xuất thành công"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "..."
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Không được phép",
    "details": "Refresh token không hợp lệ."
  }
  ```

**Note**:

- Sets `RefreshToken.Revoked` to the current timestamp.

### 1.7 Get Current User

**Description**: Retrieves information of the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/auth/me`  
**Authorization**: Bearer Token

**Request**:

```http
GET /api/auth/me
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "accountId": 123,
    "email": "user@example.com",
    "role": "User",
    "fullName": "Nguyen Van A",
    "phone": "0909123456"
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Không được phép",
    "details": "Invalid or missing token"
  }
  ```

**Note**:

- Retrieves `Account` details and associated `User` or `Owner` information based on `Account.Role`.

### 1.8 Change Password

**Description**: Changes the password of the authenticated user.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/change-password`  
**Authorization**: Bearer Token

**Request Body**:

```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Mật khẩu đã được thay đổi thành công"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "Mật khẩu hiện tại không đúng."
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Không được phép",
    "details": "Invalid or missing token"
  }
  ```

**Note**:

- Verifies `currentPassword` using `BCrypt`.
- Hashes `newPassword` using `BCrypt`.
- Updates `Account.UpdatedAt`.

### 1.9 Verify Email

**Description**: Verifies the email address using a link sent via email.

**HTTP Method**: GET  
**Endpoint**: `/api/auth/verify-email?email={email}&token={token}`  
**Authorization**: None

**Request Parameters**:

- `email`: Email address of the account.
- `token`: Verification token.

**Response**:

- **200 OK** (if `Accept: application/json`):

  ```json
  {
    "message": "Email đã được xác minh thành công"
  }
  ```

- **302 Found** (if not JSON):

  - Success: Redirects to `{FEUrl}/auth/verified?status=success&message=Email đã được xác minh thành công`
  - Failure: Redirects to `{FEUrl}/auth/verify-email?status=error&message=Email hoặc token không hợp lệ`

- **400 Bad Request** (if `Accept: application/json`):

  ```json
  {
    "error": "Email hoặc token không hợp lệ",
    "details": "Vui lòng kiểm tra email và token."
  }
  ```

**Note**:

- Sets `Account.IsActive` to `true` and clears `Account.VerificationToken` and `Account.VerificationTokenExpiry`.
- Validates `Account.VerificationTokenExpiry`.

### 1.10 Resend Verification Email

**Description**: Resends the verification or restoration email.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/resend-verification-email`  
**Authorization**: None

**Request Body**:

```json
{
  "email": "user@example.com"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Email xác minh hoặc khôi phục đã được gửi"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "..."
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Email không tồn tại",
    "details": "Email không tồn tại."
  }
  ```

**Note**:

- Generates a new `Account.VerificationToken` and `Account.VerificationTokenExpiry` (24-hour validity).
- Sends a verification email for unverified accounts or a restoration email for deleted accounts.

### 1.11 Restore Account

**Description**: Restores a soft-deleted account using email and token.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/restore-account`  
**Authorization**: None

**Request Body**:

```json
{
  "email": "user@example.com",
  "token": "..."
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Tài khoản đã được khôi phục thành công"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Yêu cầu không hợp lệ",
    "details": "Token không hợp lệ, đã hết hạn, hoặc tài khoản không ở trạng thái đã xóa."
  }
  ```

- **500 Internal Server Error**:

  ```json
  {
    "error": "Lỗi hệ thống",
    "details": "Đã xảy ra lỗi không mong muốn."
  }
  ```

**Note**:

- Resets `Account.DeletedAt`, `User.DeletedAt`, or `Owner.DeletedAt` to `null`.
- Sets `Account.IsActive` to `true`.
- Clears `Account.VerificationToken` and `Account.VerificationTokenExpiry`.
- Sends a restoration notification email.

### 1.12 Verify Token

**Description**: Verifies the validity of an access token.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/verify-token`  
**Authorization**: Bearer Token

**Request Body**:

```json
{
  "token": "...access_token"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "isValid": true,
    "role": "User",
    "message": "Token hợp lệ"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Dữ liệu đầu vào không hợp lệ",
    "details": "..."
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Không được phép",
    "details": "Token không hợp lệ hoặc đã hết hạn"
  }
  ```

**Note**:

- Validates JWT using `JwtSettings:Secret`, `Issuer`, `Audience`, and lifetime.

## 2. User Management

### 2.1 Get Profile

**Description**: Retrieves the profile of the authenticated user or owner.

**HTTP Method**: GET  
**Endpoint**: `/api/users/profile`  
**Authorization**: Bearer Token (User or Owner)

**Request Example**:

```http
GET /api/users/profile
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "userId": 1, // For User role
    "ownerId": 1, // For Owner role
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "phone": "0909123456",
    "city": "Hà Nội", // For User role
    "district": "Đống Đa", // For User role
    "avatarUrl": "https://cloudinary.com/avatar.jpg", // For User role
    "dateOfBirth": "1990-01-01", // For User role
    "description": "Field owner" // For Owner role
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "account",
        "message": "Thông tin người dùng không tồn tại."
      }
    ]
  }
  ```

**Note**:

- Response fields vary based on `Account.Role`:
  - `User`: Includes `userId`, `fullName`, `email`, `phone`, `city`, `district`, `avatarUrl`, `dateOfBirth`.
  - `Owner`: Includes `ownerId`, `fullName`, `email`, `phone`, `description`.

### 2.2 Update Profile

**Description**: Updates the profile of the authenticated user or owner.

**HTTP Method**: PUT  
**Endpoint**: `/api/users/profile`  
**Authorization**: Bearer Token (User or Owner)

**Request Body**:

```json
{
  "fullName": "Nguyen Van A",
  "phone": "0909123456",
  "city": "Hà Nội", // For User role
  "district": "Đống Đa", // For User role
  "avatarUrl": "https://cloudinary.com/avatar.jpg", // For User role
  "dateOfBirth": "1990-01-01", // For User role
  "description": "Field owner" // For Owner role
}
```

**Request Example**:

```http
PUT /api/users/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "fullName": "Nguyen Van A",
  "phone": "0909123456",
  "city": "Hà Nội",
  "district": "Đống Đa",
  "dateOfBirth": "1990-01-01"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "userId": 1, // For User role
    "ownerId": 1, // For Owner role
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "phone": "0909123456",
    "city": "Hà Nội", // For User role
    "district": "Đống Đa", // For User role
    "avatarUrl": "https://cloudinary.com/avatar.jpg", // For User role
    "dateOfBirth": "1990-01-01", // For User role
    "description": "Field owner", // For Owner role
    "message": "Profile updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "phone",
        "message": "Invalid phone format"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Request fields vary based on `Account.Role`:
  - `User`: Can update `fullName`, `phone`, `city`, `district`, `avatarUrl`, `dateOfBirth`.
  - `Owner`: Can update `fullName`, `phone`, `description`.
- Updates `User.UpdatedAt` or `Owner.UpdatedAt`.

### 2.3 Delete Profile

**Description**: Deletes the profile of the authenticated user or owner (soft delete).

**HTTP Method**: DELETE  
**Endpoint**: `/api/users/profile`  
**Authorization**: Bearer Token (User or Owner)

**Request Example**:

```http
DELETE /api/users/profile
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Profile deleted successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid request",
    "message": "Cannot delete profile due to active bookings."
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Sets `Account.DeletedAt`, `User.DeletedAt`, or `Owner.DeletedAt` for soft delete.
- Sets `Account.IsActive` to `false`.
- Checks for active bookings (`User`) or fields (`Owner`) before deletion.
- Sends a deletion notification email with restoration instructions.

### 2.4 Get Loyalty Points

**Description**: Retrieves the total loyalty points of the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/users/loyalty-points`  
**Authorization**: Bearer Token (User)

**Request Example**:

```http
GET /api/users/loyalty-points
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "userId": 1,
    "loyaltyPoints": 150
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Only users can access loyalty points."
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "account",
        "message": "Thông tin người dùng không tồn tại."
      }
    ]
  }
  ```

**Note**:

- Returns the total loyalty points stored in `User.LoyaltyPoints`.
- Only accessible to `User` role.

### 2.5 Get Favorite Fields

**Description**: Retrieves the favorite fields of the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/users/favorites`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `sort` (optional, string): Sort by `FieldName:asc` or `FieldName:desc`.
- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of entries per page.

**Request Example**:

```http
GET /api/users/favorites?page=1&pageSize=10&sort=FieldName:asc
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "Sân Bóng Đá ABC",
        "address": "123 Đường Láng, Đống Đa",
        "averageRating": 4.5
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "pagination",
        "message": "Page and pageSize must be positive."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Only users can access favorite fields."
  }
  ```

**Note**:

- Only accessible to `User` role.
- Supports sorting by `FieldName`.

### 2.6 Add Favorite Field

**Description**: Adds a field to the authenticated user's favorites.

**HTTP Method**: POST  
**Endpoint**: `/api/users/favorites`  
**Authorization**: Bearer Token (User)

**Request Body**:

```json
{
  "fieldId": 1
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "fieldId": 1,
    "message": "Field added to favorites"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "fieldId",
        "message": "Field is already in favorites"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Only users can add favorite fields."
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

**Note**:

- Only accessible to `User` role.
- Checks if the field exists and is not already in favorites.

### 2.7 Remove Favorite Field

**Description**: Removes a field from the authenticated user's favorites.

**HTTP Method**: DELETE  
**Endpoint**: `/api/users/favorites/{fieldId}`  
**Authorization**: Bearer Token (User)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field to remove.

**Request Example**:

```http
DELETE /api/users/favorites/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Favorite field removed successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "fieldId",
        "message": "Field is not in favorites"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Only users can remove favorite fields."
  }
  ```

**Note**:

- Only accessible to `User` role.
- Validates that the field exists in the user's favorites.

### 2.8 Get Booking History

**Description**: Retrieves the booking history of the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/users/bookings`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `status` (optional, string): Filter by status (`Confirmed`, `Pending`, `Cancelled`).
- `startDate` (optional, date: YYYY-MM-DD): Filter bookings from this date.
- `endDate` (optional, date: YYYY-MM-DD): Filter bookings up to this date.
- `sort` (optional, string): Sort by `BookingDate:asc` or `BookingDate:desc`.
- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of entries per page.

**Request Example**:

```http
GET /api/users/bookings?page=1&pageSize=10&status=Confirmed&startDate=2025-05-01&endDate=2025-05-31&sort=BookingDate:desc
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "bookingId": 1,
        "fieldName": "Sân Bóng Đá ABC",
        "subFieldName": "Sân 5A",
        "bookingDate": "2025-06-01",
        "startTime": "14:00",
        "endTime": "15:00",
        "totalPrice": 500000,
        "status": "Confirmed",
        "paymentStatus": "Paid"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "dateRange",
        "message": "startDate cannot be greater than endDate."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Only users can access booking history."
  }
  ```

**Note**:

- Filters bookings by `Booking.UserId` and optional parameters.
- `paymentStatus` reflects `Booking.PaymentStatus` (`Paid`, `Unpaid`, `Refunded`).
- Only accessible to `User` role.

### 2.9 Get User Search History

**Description**: Retrieves the search history of the authenticated user. Supports pagination and filtering by date range.

**HTTP Method**: GET  
**Endpoint**: `/api/users/search-history`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `startDate` (optional, date: YYYY-MM-DD): Filter history from this date.
- `endDate` (optional, date: YYYY-MM-DD): Filter history up to this date.
- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of entries per page.

**Request Example**:

```http
GET /api/users/search-history?page=1&pageSize=10&startDate=2025-05-01&endDate=2025-05-31
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "searchId": 1,
        "userId": 1,
        "keyword": "football field Hanoi",
        "searchDateTime": "2025-05-25T10:00:00Z",
        "fieldId": 1,
        "latitude": 21.0001,
        "longitude": 105.0001
      },
      {
        "searchId": 2,
        "userId": 1,
        "keyword": "tennis court near me",
        "searchDateTime": "2025-05-24T15:30:00Z",
        "fieldId": null,
        "latitude": null,
        "longitude": null
      }
    ],
    "totalCount": 2,
    "page": 1,
    "pageSize": 10,
    "message": "Search history retrieved successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "dateRange",
        "message": "startDate cannot be greater than endDate."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Only users can access search history."
  }
  ```

**Note**:

- Returns `SearchHistory` records for the authenticated user.
- `fieldId`, `latitude`, and `longitude` are optional and may be null.
- Only accessible to `User` role.

### 2.10 Clear Search History

**Description**: Clears all search history records for the authenticated user by soft deleting them.

**HTTP Method**: DELETE  
**Endpoint**: `/api/users/search-history`  
**Authorization**: Bearer Token (User)

**Request Example**:

```http
DELETE /api/users/search-history
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Search history cleared successfully"
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Only users can clear search history."
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "User is not authorized to access this resource"
  }
  ```

**Note**:

- Performs a soft delete by setting `DeletedAt` to the current timestamp for all `SearchHistory` records.
- Only accessible to `User` role.
- No request body is required.

### 2.11 Get User Reviews

**Description**: Retrieves reviews made by the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/users/reviews`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `sort` (optional, string): Sort by `CreatedAt:asc` or `CreatedAt:desc`.
- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of entries per page.

**Request Example**:

```http
GET /api/users/reviews?page=1&pageSize=10&sort=CreatedAt:desc
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "reviewId": 1,
        "fieldName": "Sân Bóng Đá ABC",
        "rating": 5,
        "comment": "Great field!",
        "createdAt": "2025-06-01T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "pagination",
        "message": "Page and pageSize must be positive."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Only users can access reviews."
  }
  ```

**Note**:

- Returns only visible reviews (`Review.IsVisible = true`).
- Only accessible to `User` role.

### 2.12 Upload Avatar

**Description**: Uploads a new avatar image for the authenticated user.

**HTTP Method**: POST  
**Endpoint**: `/api/users/avatar`  
**Authorization**: Bearer Token (User)

**Request Body**:

- Form data with a file field named "file"

**Request Example**:

```http
POST /api/users/avatar
Authorization: Bearer {token}
Content-Type: multipart/form-data

[Form Data: file=@path/to/avatar.jpg]
```

**Response**:

- **200 OK**:

  ```json
  {
    "avatarUrl": "https://cloudinary.com/userAvatars/image123.jpg",
    "message": "Avatar uploaded successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "file",
        "message": "Định dạng file không hỗ trợ. Chỉ chấp nhận JPG, JPEG, PNG và GIF."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Chỉ người dùng có thể tải lên ảnh đại diện."
  }
  ```

**Note**:

- Only accessible to `User` role.
- Accepts JPG, JPEG, PNG, and GIF formats.
- Maximum file size: 5MB.
- Image will be resized to fit within 500x500 pixels.
- Updates `User.AvatarUrl` with the Cloudinary URL.

## 3. Sport Categories

### 3.1 Get Sports

**Description**: Retrieves a list of all active sports with optional filtering, sorting, and pagination.

**HTTP Method**: GET  
**Endpoint**: `/api/sports`  
**Authorization**: None (Public)

**Query Parameters**:

- `keyword` (optional, string): Search by sport name.
- `sort` (optional, string): Sort by `SportName:asc`, `SportName:desc`, `CreatedAt:asc`, or `CreatedAt:desc`.
- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of entries per page.

**Request Example**:

```http
GET /api/sports?keyword=football&sort=SportName:asc&page=1&pageSize=10
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "sportId": 1,
        "sportName": "Football",
        "description": "A team sport played with a spherical ball.",
        "imageUrl": "https://res.cloudinary.com/your_cloud_name/image/upload/football_icon.jpg",
        "createdAt": "2025-01-01T00:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "pagination",
        "message": "Page and pageSize must be positive."
      }
    ]
  }
  ```

**Note**:

- Returns sports where `Sport.DeletedAt` is null.
- Supports filtering by keyword (partial match on `SportName`).
- Supports pagination and sorting.

### 3.2 Get Sport By ID

**Description**: Retrieves detailed information about a specific sport by its ID.

**HTTP Method**: GET  
**Endpoint**: `/api/sports/{sportId}`  
**Authorization**: None (Public)

**Path Parameters**:

- `sportId` (required, integer): The ID of the sport.

**Request Example**:

```http
GET /api/sports/1
```

**Response**:

- **200 OK**:

  ```json
  {
    "sportId": 1,
    "sportName": "Football",
    "description": "A team sport played with a spherical ball.",
    "imageUrl": "https://res.cloudinary.com/your_cloud_name/image/upload/football_icon.jpg",
    "createdAt": "2025-01-01T00:00:00Z",
    "updatedAt": "2025-01-02T00:00:00Z"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "sportId",
        "message": "SportId must be positive."
      }
    ]
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Sport not found"
  }
  ```

**Note**:

- Returns sport where `Sport.DeletedAt` is null.

### 3.3 Create Sport

**Description**: Creates a new sport category for the authenticated admin.

**HTTP Method**: POST  
**Endpoint**: `/api/sports`  
**Authorization**: Bearer Token (Admin)  
**Content-Type**: `application/json`

**Request Body**:

- `sportName` (required, string): Name of the sport.
- `description` (optional, string): Description of the sport.

**Request Example**:

```http
POST /api/sports
Authorization: Bearer {token}
Content-Type: application/json

{
  "sportName": "Football",
  "description": "A team sport played with a spherical ball."
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "sportId": 1,
    "sportName": "Football",
    "description": "A team sport played with a spherical ball.",
    "imageUrl": null,
    "createdAt": "2025-01-20T00:00:00Z",
    "message": "Sport created successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "sportName",
        "message": "Sport name is required."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only admins can create sports."
  }
  ```

**Note**:

- Only accessible to `Admin` role.
- Sets `Sport.CreatedAt` and `Sport.UpdatedAt`.
- Image can be uploaded separately via `Upload Sport Icon` endpoint.

### 3.4 Update Sport

**Description**: Updates an existing sport category for the authenticated admin.

**HTTP Method**: PUT  
**Endpoint**: `/api/sports/{sportId}`  
**Authorization**: Bearer Token (Admin)  
**Content-Type**: `application/json`

**Path Parameters**:

- `sportId` (required, integer): The ID of the sport to update.

**Request Body**:

- `sportName` (required, string): Name of the sport.
- `description` (optional, string): Description of the sport.

**Request Example**:

```http
PUT /api/sports/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "sportName": "Football",
  "description": "Updated description for football."
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "sportId": 1,
    "sportName": "Football",
    "description": "Updated description for football.",
    "imageUrl": "https://res.cloudinary.com/your_cloud_name/image/upload/football_icon.jpg",
    "createdAt": "2025-01-20T00:00:00Z",
    "updatedAt": "2025-01-21T00:00:00Z",
    "message": "Sport updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "sportId",
        "message": "SportId must be positive."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only admins can update sports."
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Sport not found"
  }
  ```

**Note**:

- Only accessible to `Admin` role.
- Updates `Sport.UpdatedAt`.
- Image can be updated separately via `Upload Sport Icon` endpoint.

### 3.5 Delete Sport

**Description**: Soft deletes an existing sport category for the authenticated admin.

**HTTP Method**: DELETE  
**Endpoint**: `/api/sports/{sportId}`  
**Authorization**: Bearer Token (Admin)

**Path Parameters**:

- `sportId` (required, integer): The ID of the sport to delete.

**Request Example**:

```http
DELETE /api/sports/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Sport deleted successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "sportId",
        "message": "SportId must be positive."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only admins can delete sports."
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Sport not found"
  }
  ```

**Note**:

- Only accessible to `Admin` role.
- Sets `Sport.DeletedAt` to current timestamp.
- Does not physically remove the sport from the database.

### 3.6 Upload Sport Icon

**Description**: Uploads or updates the icon image for a sport category for the authenticated admin.

**HTTP Method**: POST  
**Endpoint**: `/api/sports/{sportId}/icon`  
**Authorization**: Bearer Token (Admin)  
**Content-Type**: `multipart/form-data`

**Path Parameters**:

- `sportId` (required, integer): The ID of the sport to upload the icon for.

**Form Data**:

- `file` (required): Image file for the sport icon (JPEG, PNG, GIF).

**Request Example**:

```http
POST /api/sports/1/icon
Authorization: Bearer {token}
Content-Type: multipart/form-data

Content-Disposition: form-data; name="file"; filename="football_icon.jpg"
Content-Type: image/jpeg
{...binary image data...}
```

**Response**:

- **200 OK**:

  ```json
  {
    "sportId": 1,
    "imageUrl": "https://res.cloudinary.com/your_cloud_name/image/upload/football_icon.jpg",
    "message": "Sport icon uploaded successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "image",
        "message": "Image file is required."
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only admins can upload sport icons."
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Sport not found"
  }
  ```

**Note**:

- Only accessible to `Admin` role.
- Image is uploaded to Cloudinary, and the returned URL is stored.
- Overwrites any existing icon for the sport.
- Supports JPEG, PNG, and GIF formats with a maximum file size of 5MB.

# 4. API Endpoints: Field Management

This document outlines the API endpoints for managing sports fields, including CRUD operations, availability checking, reviews, and bookings. All endpoints use JSON format for requests and responses unless specified otherwise. Authentication is required for Owner-specific actions using Bearer Token.

## 4.1 Get Fields

**Description**: Retrieve a paginated list of fields with filtering and sorting options.

**HTTP Method**: GET  
**Endpoint**: `/api/fields`  
**Authorization**: None

**Query Parameters**:

- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of items per page.
- `city` (optional, string): Filter by city.
- `district` (optional, string): Filter by district.
- `sportId` (optional, integer): Filter by sport ID.
- `search` (optional, string): Search by field name or address.
- `latitude` (optional, double): User's latitude for distance-based filtering.
- `longitude` (optional, double): User's longitude for distance-based filtering.
- `radius` (optional, double, default: 10): Search radius in kilometers.
- `minPrice` (optional, decimal): Minimum price per slot.
- `maxPrice` (optional, decimal): Maximum price per slot.
- `sortBy` (optional, string): Sort by `averageRating`, `distance`, `price` (default: `fieldId`).
- `sortOrder` (optional, string): Sort order (`asc` or `desc`, default: `asc`).

**Request Example**:

```http
GET /api/fields?page=1&pageSize=10&city=Hà Nội&sportId=1&latitude=21.0123&longitude=105.8234&radius=5&minPrice=100000&maxPrice=500000&sortBy=averageRating&sortOrder=desc
```

**Response**:

- **200 OK**:
  ```json
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "Sân Bóng Đá ABC",
        "description": "Sân bóng đá hiện đại tại Đống Đa",
        "address": "123 Đường Láng, Đống Đa",
        "city": "Hà Nội",
        "district": "Đống Đa",
        "latitude": 21.0123,
        "longitude": 105.8234,
        "openTime": "06:00",
        "closeTime": "22:00",
        "averageRating": 4.5,
        "sportId": 1,
        "distance": 2.5,
        "minPricePerSlot": 200000,
        "maxPricePerSlot": 300000
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **400 Bad Request**:
  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "page",
        "message": "Page must be greater than 0"
      }
    ]
  }
  ```

**Notes**:

- Only returns fields with `Status != Deleted` and `DeletedAt` is null.
- Distance is calculated using the Haversine formula if `latitude` and `longitude` are provided.
- `minPrice` and `maxPrice` filter based on `SubField.PricingRules.TimeSlots.PricePerSlot` or `DefaultPricePerSlot`.
- `sortBy=distance` requires `latitude` and `longitude`.
- Results are cached in Redis for performance.

## 4.2 Get Field By ID

**Description**: Retrieve detailed information about a specific field by ID.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): Field ID.

**Query Parameters**:

- `include` (optional, string): Comma-separated list of related data to include (`subfields`, `services`, `amenities`, `images`).

**Request Example**:

```http
GET /api/fields/1?include=subfields,services,amenities,images
```

**Response**:

- **200 OK**:
  ```json
  {
    "fieldId": 1,
    "fieldName": "Sân Bóng Đá ABC",
    "description": "Sân bóng đá hiện đại tại Đống Đa",
    "address": "123 Đường Láng, Đống Đa",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "latitude": 21.0123,
    "longitude": 105.8234,
    "openTime": "06:00",
    "closeTime": "22:00",
    "averageRating": 4.5,
    "sportId": 1,
    "subFields": [
      {
        "subFieldId": 1,
        "subFieldName": "Sân 5.1",
        "fieldType": "5-a-side",
        "description": "Sân cỏ nhân tạo 5 người chất lượng cao",
        "status": "Active",
        "capacity": 10,
        "openTime": "06:00",
        "closeTime": "22:00",
        "defaultPricePerSlot": 200000,
        "pricingRules": [
          {
            "pricingRuleId": 1,
            "appliesToDays": [
              "Monday",
              "Tuesday",
              "Wednesday",
              "Thursday",
              "Friday"
            ],
            "timeSlots": [
              {
                "startTime": "06:00",
                "endTime": "17:00",
                "pricePerSlot": 200000
              },
              {
                "startTime": "17:00",
                "endTime": "21:00",
                "pricePerSlot": 300000
              }
            ]
          },
          {
            "pricingRuleId": 2,
            "appliesToDays": ["Saturday", "Sunday"],
            "timeSlots": [
              {
                "startTime": "06:00",
                "endTime": "22:00",
                "pricePerSlot": 250000
              }
            ]
          }
        ],
        "parent7aSideId": 4,
        "child5aSideIds": []
      },
      {
        "subFieldId": 4,
        "subFieldName": "Sân 7.1",
        "fieldType": "7-a-side",
        "description": "Sân cỏ nhân tạo 7 người rộng rãi",
        "status": "Active",
        "capacity": 14,
        "openTime": "06:00",
        "closeTime": "22:00",
        "defaultPricePerSlot": 600000,
        "pricingRules": [
          {
            "pricingRuleId": 3,
            "appliesToDays": [
              "Monday",
              "Tuesday",
              "Wednesday",
              "Thursday",
              "Friday"
            ],
            "timeSlots": [
              {
                "startTime": "06:00",
                "endTime": "17:00",
                "pricePerSlot": 600000
              },
              {
                "startTime": "17:00",
                "endTime": "21:00",
                "pricePerSlot": 900000
              }
            ]
          },
          {
            "pricingRuleId": 4,
            "appliesToDays": ["Saturday", "Sunday"],
            "timeSlots": [
              {
                "startTime": "06:00",
                "endTime": "22:00",
                "pricePerSlot": 750000
              }
            ]
          }
        ],
        "parent7aSideId": null,
        "child5aSideIds": [1, 2, 3]
      }
    ],
    "services": [
      {
        "fieldServiceId": 1,
        "serviceName": "Nước uống",
        "price": 10000,
        "description": "Nước suối 500ml",
        "isActive": true
      }
    ],
    "amenities": [
      {
        "fieldAmenityId": 1,
        "amenityName": "Bãi đỗ xe",
        "description": "Miễn phí cho 50 xe",
        "iconUrl": "https://example.com/parking-icon.png"
      }
    ],
    "images": [
      {
        "fieldImageId": 1,
        "imageUrl": "https://cloudinary.com/images/field_abc_main.jpg",
        "publicId": "field_abc_main",
        "thumbnail": "https://cloudinary.com/images/field_abc_main_thumb.jpg",
        "isPrimary": true,
        "uploadedAt": "2025-06-01T10:00:00Z"
      }
    ]
  }
  ```
- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

**Notes**:

- Only returns fields with `Status != Deleted` and `DeletedAt` is null.
- Related data (subfields, services, amenities, images) is included only if specified in `include`.
- `pricingRules` supports 30-minute slots with `appliesToDays` and `timeSlots`.

## 4.3 Get Owner Fields

**Description**: Retrieve a list of fields owned by the currently logged-in owner.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/my-fields`  
**Authorization**: Bearer Token (Owner)

**Query Parameters**:

- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of items per page.
- `search` (optional, string): Search by field name or address.
- `status` (optional, string): Filter by status (`Active`, `Inactive`).
- `sportId` (optional, integer): Filter by sport ID.
- `sortBy` (optional, string): Sort by `fieldName`, `createdAt`, `rating`, `bookingCount` (default: `createdAt`).
- `sortOrder` (optional, string): Sort order (`asc` or `desc`, default: `desc`).

**Request Example**:

```http
GET /api/fields/my-fields?page=1&pageSize=10&status=Active&sortBy=bookingCount&sortOrder=desc
```

**Response**:

- **200 OK**:
  ```json
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "Sân Bóng Đá ABC",
        "address": "123 Đường Láng, Đống Đa",
        "city": "Hà Nội",
        "district": "Đống Đa",
        "averageRating": 4.5,
        "status": "Active",
        "bookingCount": 156,
        "subFieldCount": 3,
        "createdAt": "2025-01-01T10:00:00Z",
        "updatedAt": "2025-06-01T15:30:00Z",
        "primaryImage": "https://cloudinary.com/images/field_abc_main.jpg",
        "recentBookings": [
          {
            "bookingId": 145,
            "userName": "Nguyen Van A",
            "bookingDate": "2025-06-02T00:00:00Z",
            "status": "Confirmed",
            "totalPrice": 400000,
            "createdAt": "2025-06-01T14:30:00Z"
          },
          {
            "bookingId": 144,
            "userName": "Tran Van B",
            "bookingDate": "2025-06-01T00:00:00Z",
            "status": "Completed",
            "totalPrice": 600000,
            "createdAt": "2025-05-30T09:15:00Z"
          }
        ]
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```
- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "User is not an owner"
  }
  ```
  **Notes**:
- Only returns fields owned by the currently logged-in owner.
- Provides overview information about booking counts and recent bookings.
- Supports filtering by field status and search functionality.
- Results are cached in Redis for performance.

## 4.4 Validate Address

**Description**: Validate a field's address and return geocoding information.

**HTTP Method**: POST  
**Endpoint**: `/api/fields/validate-address`  
**Authorization**: None

**Request Body**:

```json
{
  "fieldName": "Sân Bóng Đá ABC",
  "address": "123 Đường Láng, Đống Đa",
  "city": "Hà Nội",
  "district": "Đống Đa"
}
```

**Response**:

- **200 OK**:
  ```json
  {
    "isValid": true,
    "formattedAddress": "123 Đường Láng, Đống Đa, Hà Nội",
    "latitude": 21.0123,
    "longitude": 105.8234
  }
  ```
- **400 Bad Request**:
  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "address",
        "message": "Address is required"
      }
    ]
  }
  ```
- **429 Too Many Requests**:
  ```json
  {
    "error": "Rate limit exceeded",
    "message": "Geocoding service rate limit exceeded. Please try again later."
  }
  ```

**Notes**:

- Uses OpenCage geocoding service.
- Results are cached in Redis to reduce external requests.
- Retry logic is implemented for temporary errors (429, 503).

## 4.5 Create Field

**Description**: Create a new field with subfields, services, amenities, and images, including support for 7-a-side field composition from 5-a-side fields.

**HTTP Method**: POST  
**Endpoint**: `/api/fields`  
**Authorization**: Bearer Token (Owner)

**Request Body**:

```json
{
  "fieldName": "Sân Bóng Đá ABC",
  "description": "Sân bóng đá hiện đại tại Đống Đa",
  "address": "123 Đường Láng, Đống Đa",
  "city": "Hà Nội",
  "district": "Đống Đa",
  "openTime": "06:00",
  "closeTime": "22:00",
  "sportId": 1,
  "subFields": [
    {
      "subFieldName": "Sân 5.1",
      "fieldType": "5-a-side",
      "description": "Sân cỏ nhân tạo 5 người chất lượng cao",
      "capacity": 10,
      "openTime": "06:00",
      "closeTime": "22:00",
      "defaultPricePerSlot": 200000,
      "pricingRules": [
        {
          "appliesToDays": [
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday"
          ],
          "timeSlots": [
            {
              "startTime": "06:00",
              "endTime": "17:00",
              "pricePerSlot": 200000
            },
            { "startTime": "17:00", "endTime": "21:00", "pricePerSlot": 300000 }
          ]
        },
        {
          "appliesToDays": ["Saturday", "Sunday"],
          "timeSlots": [
            { "startTime": "06:00", "endTime": "22:00", "pricePerSlot": 250000 }
          ]
        }
      ],
      "parent7aSideId": 4
    },
    {
      "subFieldName": "Sân 7.1",
      "fieldType": "7-a-side",
      "description": "Sân cỏ nhân tạo 7 người rộng rãi",
      "capacity": 14,
      "openTime": "06:00",
      "closeTime": "22:00",
      "defaultPricePerSlot": 600000,
      "pricingRules": [
        {
          "appliesToDays": [
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday"
          ],
          "timeSlots": [
            {
              "startTime": "06:00",
              "endTime": "17:00",
              "pricePerSlot": 600000
            },
            { "startTime": "17:00", "endTime": "21:00", "pricePerSlot": 900000 }
          ]
        },
        {
          "appliesToDays": ["Saturday", "Sunday"],
          "timeSlots": [
            { "startTime": "06:00", "endTime": "22:00", "pricePerSlot": 750000 }
          ]
        }
      ],
      "child5aSideIds": [1, 2, 3]
    }
  ],
  "services": [
    {
      "serviceName": "Nước uống",
      "price": 10000,
      "description": "Nước suối 500ml"
    }
  ],
  "amenities": [
    {
      "amenityName": "Bãi đỗ xe",
      "description": "Miễn phí cho 50 xe",
      "iconUrl": "https://example.com/parking-icon.png"
    }
  ],
  "images": [
    {
      "imageUrl": "https://cloudinary.com/images/field_abc_main.jpg",
      "publicId": "field_abc_main",
      "thumbnail": "https://cloudinary.com/images/field_abc_main_thumb.jpg",
      "isPrimary": true
    }
  ]
}
```

**Response**:

- **201 Created**:
  ```json
  {
    "fieldId": 1,
    "fieldName": "Sân Bóng Đá ABC",
    "description": "Sân bóng đá hiện đại tại Đống Đa",
    "address": "123 Đường Láng, Đống Đa",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "openTime": "06:00",
    "closeTime": "22:00",
    "latitude": 21.0123,
    "longitude": 105.8234,
    "sportId": 1,
    "subFields": [
      {
        "subFieldId": 1,
        "subFieldName": "Sân 5.1",
        "fieldType": "5-a-side",
        "description": "Sân cỏ nhân tạo 5 người chất lượng cao",
        "status": "Active",
        "capacity": 10,
        "openTime": "06:00",
        "closeTime": "22:00",
        "defaultPricePerSlot": 200000,
        "pricingRules": [
          {
            "pricingRuleId": 1,
            "appliesToDays": [
              "Monday",
              "Tuesday",
              "Wednesday",
              "Thursday",
              "Friday"
            ],
            "timeSlots": [
              {
                "startTime": "06:00",
                "endTime": "17:00",
                "pricePerSlot": 200000
              },
              {
                "startTime": "17:00",
                "endTime": "21:00",
                "pricePerSlot": 300000
              }
            ]
          },
          {
            "pricingRuleId": 2,
            "appliesToDays": ["Saturday", "Sunday"],
            "timeSlots": [
              {
                "startTime": "06:00",
                "endTime": "22:00",
                "pricePerSlot": 250000
              }
            ]
          }
        ],
        "parent7aSideId": 4,
        "child5aSideIds": []
      },
      {
        "subFieldId": 4,
        "subFieldName": "Sân 7.1",
        "fieldType": "7-a-side",
        "description": "Sân cỏ nhân tạo 7 người rộng rãi",
        "status": "Active",
        "capacity": 14,
        "openTime": "06:00",
        "closeTime": "22:00",
        "defaultPricePerSlot": 600000,
        "pricingRules": [
          {
            "pricingRuleId": 3,
            "appliesToDays": [
              "Monday",
              "Tuesday",
              "Wednesday",
              "Thursday",
              "Friday"
            ],
            "timeSlots": [
              {
                "startTime": "06:00",
                "endTime": "17:00",
                "pricePerSlot": 600000
              },
              {
                "startTime": "17:00",
                "endTime": "21:00",
                "pricePerSlot": 900000
              }
            ]
          },
          {
            "pricingRuleId": 4,
            "appliesToDays": ["Saturday", "Sunday"],
            "timeSlots": [
              {
                "startTime": "06:00",
                "endTime": "22:00",
                "pricePerSlot": 750000
              }
            ]
          }
        ],
        "parent7aSideId": null,
        "child5aSideIds": [1, 2, 3]
      }
    ],
    "services": [
      {
        "fieldServiceId": 1,
        "serviceName": "Nước uống",
        "price": 10000,
        "description": "Nước suối 500ml",
        "isActive": true
      }
    ],
    "amenities": [
      {
        "fieldAmenityId": 1,
        "amenityName": "Bãi đỗ xe",
        "description": "Miễn phí cho 50 xe",
        "iconUrl": "https://example.com/parking-icon.png"
      }
    ],
    "images": [
      {
        "fieldImageId": 1,
        "imageUrl": "https://cloudinary.com/images/field_abc_main.jpg",
        "publicId": "field_abc_main",
        "thumbnail": "https://cloudinary.com/images/field_abc_main_thumb.jpg",
        "isPrimary": true,
        "uploadedAt": "2025-06-01T10:00:00Z"
      }
    ],
    "message": "Field created successfully"
  }
  ```
- **400 Bad Request**:
  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "fieldName",
        "message": "Field name is required"
      },
      {
        "field": "subFields[0].timeSlots",
        "message": "Time slots must not overlap"
      }
    ]
  }
  ```
- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```
- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "User is not an owner"
  }
  ```
- **429 Too Many Requests**:
  ```json
  {
    "error": "Rate limit exceeded",
    "message": "Geocoding service rate limit exceeded. Please try again later."
  }
  ```

**Notes**:

- `OwnerId` is extracted from the token.
- Address is validated using OpenCage, and `latitude`/`longitude` are set automatically.
- Atomicity is ensured using `UnitOfWork`.
- Constraints:
  - `openTime` < `closeTime`.
  - `subFieldName` must be unique within the field.
  - `sportId` must exist.
  - Maximum: 10 subfields, 50 services, 50 amenities, 50 images.
  - `child5aSideIds` must reference existing `subFieldId`s of 5-a-side subfields within the same field and not assigned to another 7-a-side subfield.
  - `parent7aSideId` must reference a valid 7-a-side subfield within the same field.
  - `timeSlots` must not overlap and should cover `openTime` to `closeTime` (or use `defaultPricePerSlot`).
  - `startTime` and `endTime` in `timeSlots` must be in 30-minute increments.
- New field is cached in Redis.

## 4.6 Upload Field Image

**Description**: Upload an image for a field.

**HTTP Method**: POST  
**Endpoint**: `/api/fields/{fieldId}/images`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): Field ID.

**Request Body**: Form-data

- `image` (required, file): Image file (jpg, png).
- `isPrimary` (optional, boolean): Whether the image is primary.

**Request Example**:

```http
POST /api/fields/1/images
Authorization: Bearer {token}
Content-Type: multipart/form-data

[image: field_abc_new.jpg]
[isPrimary: false]
```

**Response**:

- **201 Created**:
  ```json
  {
    "fieldImageId": 2,
    "imageUrl": "https://cloudinary.com/images/field_abc_new.jpg",
    "publicId": "field_abc_new",
    "thumbnail": "https://cloudinary.com/images/field_abc_new_thumb.jpg",
    "isPrimary": false,
    "uploadedAt": "2025-06-01T10:00:00Z",
    "message": "Image uploaded successfully"
  }
  ```
- **400 Bad Request**:
  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "image",
        "message": "Image file is required"
      }
    ]
  }
  ```
- **401 Unauthorized**, **403 Forbidden**, **404 Not Found**:
  ```json
  {
    "error": "Not found",
    "message": "Field not found"
  }
  ```
- **413 Payload Too Large**:
  ```json
  {
    "error": "Payload too large",
    "message": "Image size exceeds maximum limit"
  }
  ```

**Notes**:

- Uses Cloudinary for image storage.
- Validates file format (jpg, png) and size (max 5MB).
- Only the owner of the field can upload images.

## 4.7 Update Field

**Description**: Update a field and its subfields, services, amenities, and images.

**HTTP Method**: PUT  
**Endpoint**: `/api/fields/{fieldId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): Field ID.

**Request Body**: Same as `Create Field`.

**Response**:

- **200 OK**: Same as `Create Field` response.
- **400 Bad Request**, **401 Unauthorized**, **403 Forbidden**, **404 Not Found**, **429 Too Many Requests**: Same as `Create Field`.

**Notes**:

- Only updates fields with `Status != Deleted` and `DeletedAt` is null.
- Only the owner of the field can update.
- Atomicity is ensured using `UnitOfWork`.
- Updated field is cached in Redis.
- Same constraints as `Create Field` apply.

## 4.8 Delete Field

**Description**: Soft delete a field.

**HTTP Method**: DELETE  
**Endpoint**: `/api/fields/{fieldId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): Field ID.

**Response**:

- **200 OK**:
  ```json
  {
    "fieldId": 1,
    "status": "Deleted",
    "deletedAt": "2025-06-01T10:00:00Z",
    "message": "Field deleted successfully"
  }
  ```
- **400 Bad Request**:
  ```json
  {
    "error": "Invalid operation",
    "message": "Cannot delete field with active bookings"
  }
  ```
- **401 Unauthorized**, **403 Forbidden**, **404 Not Found**: Same as `Create Field`.

**Notes**:

- Sets `Status` to `Deleted` and `DeletedAt` to current timestamp.
- Only the owner of the field can delete.
- Checks for active bookings (`Status = Confirmed` or `Pending` and `DeletedAt` is null) before deletion.
- Cache is invalidated after deletion.

## 4.9 Get Field Availability

**Description**: Retrieve available time slots for a field's subfields, considering 30-minute slots and 7-a-side field composition.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/availability`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): Field ID.

**Query Parameters**:

- `subFieldId` (optional, integer): Filter by subfield ID.
- `date` (required, date): Date in YYYY-MM-DD format.
- `sportId` (optional, integer): Filter by sport ID.
- `startTime` (optional, string): Start time in HH:mm format (must be in 30-minute increments).
- `endTime` (optional, string): End time in HH:mm format (must be in 30-minute increments).

**Request Example**:

```http
GET /api/fields/1/availability?date=2025-06-01&subFieldId=1&startTime=14:00&endTime=20:00
```

**Response**:

- **200 OK**:
  ```json
  {
    "data": [
      {
        "subFieldId": 1,
        "subFieldName": "Sân 5.1",
        "availableSlots": [
          {
            "startTime": "14:00",
            "endTime": "14:30",
            "pricePerSlot": 200000,
            "isAvailable": true
          },
          {
            "startTime": "14:30",
            "endTime": "15:00",
            "pricePerSlot": 200000,
            "isAvailable": true
          }
        ]
      },
      {
        "subFieldId": 4,
        "subFieldName": "Sân 7.1",
        "availableSlots": [
          {
            "startTime": "14:00",
            "endTime": "14:30",
            "pricePerSlot": 600000,
            "isAvailable": false,
            "unavailableReason": "Child subfield booked"
          }
        ]
      }
    ]
  }
  ```
- **400 Bad Request**:
  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "startTime",
        "message": "Start time must be in 30-minute increments"
      },
      {
        "field": "startTime",
        "message": "Start time must be within subfield open time"
      }
    ]
  }
  ```
- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

**Notes**:

- Returns slots not booked (based on `Booking.TimeSlots` with `DeletedAt` is null).
- Slots are generated in 30-minute increments (e.g., 14:00-14:30, 14:30-15:00).
- `startTime` and `endTime` must be within `SubField.OpenTime` and `SubField.CloseTime`.
- Applies `PricingRules` based on the day of the week and time slot; falls back to `DefaultPricePerSlot` if no rule matches.
- Considers 7-a-side field composition:
  - If any 5-a-side subfield in `Child5aSideIds` is booked, the 7-a-side subfield is marked as unavailable (`unavailableReason`: "Child subfield booked").
  - If a 7-a-side subfield is booked, all corresponding 5-a-side subfields are marked as unavailable (`unavailableReason`: "Parent subfield booked").
- Results are cached in Redis for performance.

## 4.10 Get Field Reviews

**Description**: Retrieve reviews for a field.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/reviews`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): Field ID.

**Query Parameters**:

- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of items per page.
- `minRating` (optional, integer): Minimum rating (1-5).
- `sortBy` (optional, string): Sort by `createdAt` or `rating` (default: `createdAt`).
- `sortOrder` (optional, string): Sort order (`asc` or `desc`, default: `desc`).

**Request Example**:

```http
GET /api/fields/1/reviews?page=1&pageSize=10&minRating=4&sortBy=createdAt&sortOrder=desc
```

**Response**:

- **200 OK**:
  ```json
  {
    "data": [
      {
        "reviewId": 1,
        "userId": 1,
        "fullName": "Nguyen Van A",
        "rating": 5,
        "comment": "Great field!",
        "createdAt": "2025-06-01T10:00:00Z",
        "ownerReply": "Thank you for your feedback!",
        "replyDate": "2025-06-02T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

**Notes**:

- Only returns reviews for fields with `Status != Deleted` and `DeletedAt` is null.
- Only returns reviews with `IsVisible = true`.
- Supports sorting and filtering for better user experience.

## 4.11 Get Field Bookings

**Description**: Retrieve bookings for a field (Owner only).

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/bookings`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): Field ID.

**Query Parameters**:

- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of items per page.
- `status` (optional, string): Filter by status (`Confirmed`, `Pending`, `Cancelled`).
- `startDate` (optional, date): Start date in YYYY-MM-DD format.
- `endDate` (optional, date): End date in YYYY-MM-DD format.

**Request Example**:

```http
GET /api/fields/1/bookings?page=1&pageSize=10&status=Confirmed&startDate=2025-06-01&endDate=2025-06-02
```

**Response**:

- **200 OK**:
  ```json
  {
    "data": [
      {
        "bookingId": 1,
        "subFieldId": 1,
        "subFieldName": "Sân 5.1",
        "userId": 2,
        "fullName": "Nguyen Van B",
        "bookingDate": "2025-06-01",
        "timeSlots": [
          {
            "startTime": "14:00",
            "endTime": "14:30",
            "price": 200000
          },
          {
            "startTime": "14:30",
            "endTime": "15:00",
            "price": 200000
          }
        ],
        "services": [
          {
            "bookingServiceId": 1,
            "fieldServiceId": 1,
            "serviceName": "Nước uống",
            "quantity": 10,
            "price": 10000,
            "description": "Nước suối 500ml"
          }
        ],
        "totalPrice": 410000,
        "status": "Confirmed",
        "paymentStatus": "Paid",
        "createdAt": "2025-06-01T09:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **401 Unauthorized**, **403 Forbidden**, **404 Not Found**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Notes**:

- Only the owner of the field can view bookings.
- Filters by `status`, `startDate`, and `endDate` for flexible querying.
- `timeSlots` reflect 30-minute increments.
- `services` include additional services booked with the field.

## 5. Promotion Management

### 5.1 Get Promotions

**Description**: Retrieves active promotions for a field or all fields.

**HTTP Method**: GET  
**Endpoint**: `/api/promotions`  
**Authorization**: None

**Query Parameters**:

- `fieldId` (optional, integer): Filter by field.
- `code` (optional, string): Filter by promotion code.

**Request Example**:

```http
GET /api/promotions?fieldId=1
```

**Response**:

- **200 OK**:

```json
{
  "data": [
    {
      "promotionId": "1",
      "code": "SUMMER2025",
      "description": "Summer discount",
      "discountType": "Percentage",
      "discountValue": 10.0,
      "startDate": "2025-06-01",
      "endDate": "2025-08-31",
      "minBookingValue": 500000.0,
      "maxDiscountAmount": 100000.0,
      "isActive": true
    }
  ],
  "message": "Promotions retrieved successfully"
}
```

- **400 Bad Request**:

```json
{
  "error": "Invalid input",
  "details": [
    {
      "field": "fieldId",
      "message": "Invalid field ID"
    }
  ]
}
```

**Note**:

- Returns `Promotion` records where `Promotion.IsActive` is true and within `StartDate` and `EndDate`.

### 5.2 Get Promotion By ID

**Description**: Retrieves details of a specific promotion.

**HTTP Method**: GET  
**Endpoint**: `/api/promotions/{promotionId}`  
**Authorization**: None

**Path Parameters**:

- `promotionId` (required, integer): The ID of the promotion.

**Request Example**:

```http
GET /api/promotions/1
```

**Response**:

- **200 OK**:

  ```json
  {
    "promotionId": 1,
    "fieldId": 1,
    "code": "SUMMER2025",
    "discountType": "Percentage",
    "discountValue": 20,
    "startDate": "2025-06-01",
    "endDate": "2025-08-31",
    "usageLimit": 100,
    "minBookingAmount": 500000
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Promotion not found"
  }
  ```

### 5.3 Create Promotion

**Description**: Creates a new promotion for a field (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/promotions`  
**Authorization**: Bearer Token (Owner)

**Request Body**:

```json
{
  "fieldId": 1,
  "code": "SUMMER2025",
  "discountType": "Percentage",
  "discountValue": 20,
  "startDate": "2025-06-01",
  "endDate": "2025-08-31",
  "usageLimit": 100,
  "minBookingAmount": 500000
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "promotionId": 1,
    "fieldId": 1,
    "code": "SUMMER2025",
    "discountType": "Percentage",
    "discountValue": 20,
    "startDate": "2025-06-01",
    "endDate": "2025-08-31",
    "usageLimit": 100,
    "minBookingAmount": 500000,
    "message": "Promotion created successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "code",
        "message": "Promotion code is required"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "Only the field owner can create promotions"
  }
  ```

**Note**:

- `discountType` must be "Percentage" or "Fixed".
- `usageLimit` specifies the maximum number of times the promotion can be used.
- `code` maps to `Promotion.Code`, `usageLimit` to `Promotion.UsageLimit`.

### 5.4 Update Promotion

**Description**: Updates an existing promotion (Owner only).

**HTTP Method**: PUT  
**Endpoint**: `/api/promotions/{promotionId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `promotionId` (required, integer): The ID of the promotion.

**Request Body**:

```json
{
  "code": "SUMMER2025",
  "discountType": "Percentage",
  "discountValue": 25,
  "startDate": "2025-06-01",
  "endDate": "2025-08-31",
  "usageLimit": 150,
  "minBookingAmount": 500000
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "promotionId": 1,
    "fieldId": 1,
    "code": "SUMMER2025",
    "discountType": "Percentage",
    "discountValue": 25,
    "startDate": "2025-06-01",
    "endDate": "2025-08-31",
    "usageLimit": 150,
    "minBookingAmount": 500000,
    "message": "Promotion updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "code",
        "message": "Promotion code is required"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the field owner can update promotions"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Promotion not found"
  }
  ```

### 5.5 Delete Promotion

**Description**: Deletes a promotion (Owner only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/promotions/{promotionId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `promotionId` (required, integer): The ID of the promotion.

**Request Example**:

```http
DELETE /api/promotions/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Promotion deleted successfully"
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the field owner can delete promotions"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Promotion not found"
  }
  ```

**Note**:

- Soft deletes by setting `Promotion.DeletedAt`.

### 5.6 Apply Promotion

**Description**: Applies a promotion code to a booking preview.

**HTTP Method**: POST  
**Endpoint**: `/api/promotions/apply`  
**Authorization**: Bearer Token (User)

**Request Body**:

```json
{
  "bookingId": 1,
  "code": "SUMMER2025"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "bookingId": 1,
    "promotionId": 1,
    "discount": 100000,
    "newTotalPrice": 400000,
    "message": "Promotion applied successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "code",
        "message": "Invalid or expired promotion code"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Booking not found"
  }
  ```

## 6. Booking Management

### 6.1 Preview Booking

**Description**: Previews a booking with pricing and availability details.

**HTTP Method**: POST  
**Endpoint**: `/api/bookings/preview`  
**Authorization**: Bearer Token (User)

**Request Body**:

```json
{
  "subFieldId": 1,
  "bookingDate": "2025-06-01",
  "startTime": "14:00",
  "endTime": "15:00",
  "promotionCode": "SUMMER2025",
  "services": [
    {
      "fieldServiceId": 1,
      "quantity": 2
    }
  ]
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "14:00",
    "endTime": "15:00",
    "basePrice": 500000,
    "servicePrice": 20000,
    "discount": 100000,
    "totalPrice": 420000,
    "isAvailable": true
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "subFieldId",
        "message": "Subfield is not available"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Validates `SubField` availability and `Promotion.Code`.
- `services` and `promotionCode` are optional.
- Does not create a `Booking` record.
- Base price is calculated using `FieldPricing.PricePerHour` or `SubField.DefaultPricePerSlot` if no pricing schedule is found.
- Time format must be `HH:mm`.

### 6.2 Create Simple Booking

**Description**: Creates a simple booking without services or promotion.

**HTTP Method**: POST  
**Endpoint**: `/api/bookings/simple`  
**Authorization**: Bearer Token (User)

**Request Body**:

```json
{
  "subFieldId": 1,
  "bookingDate": "2025-06-01",
  "startTime": "14:00",
  "endTime": "15:00"
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "bookingId": 1,
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "14:00",
    "endTime": "15:00",
    "totalPrice": 500000,
    "status": "Pending",
    "paymentStatus": "Pending",
    "createdAt": "2025-06-01T10:00:00Z",
    "message": "Booking created successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "subFieldId",
        "message": "Subfield is not available"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Validates `SubField` availability.
- Sets `Booking.UserId` to the authenticated user’s ID.
- `createdAt` reflects `Booking.CreatedAt`.
- Time format must be `HH:mm`.
- Base price is calculated using `FieldPricing.PricePerHour` or `SubField.DefaultPricePerSlot`.

### 6.3 Create Booking

**Description**: Creates one or more bookings for multiple subfields with multiple time slots (including 30-minute increments like 16:30, 17:00), optional services, and an optional promotion code. Bookings are linked by a `mainBookingId` for unified payment processing.

**HTTP Method**: POST  
**Endpoint**: `/api/bookings`  
**Authorization**: Bearer Token (User)

**Request Body**:

```json
{
  "bookings": [
    {
      "subFieldId": 1,
      "bookingDate": "2025-06-01",
      "timeSlots": [
        {
          "startTime": "16:30",
          "endTime": "17:00"
        },
        {
          "startTime": "17:30",
          "endTime": "18:00"
        }
      ],
      "services": [
        {
          "fieldServiceId": 1,
          "quantity": 2
        }
      ]
    },
    {
      "subFieldId": 2,
      "bookingDate": "2025-06-01",
      "timeSlots": [
        {
          "startTime": "17:00",
          "endTime": "18:00"
        }
      ],
      "services": []
    }
  ],
  "promotionCode": "SUMMER2025"
}
```

**Response**:

- **201 Created**:

```json
{
  "mainBookingId": "1",
  "bookings": [
    {
      "bookingId": "1",
      "subFieldId": "1",
      "subFieldName": "Sân 5A",
      "fieldName": "ABC Football Field",
      "bookingDate": "2025-06-01",
      "timeSlots": [
        {
          "startTime": "16:30",
          "endTime": "17:00",
          "price": 250000.0
        },
        {
          "startTime": "17:30",
          "endTime": "18:00",
          "price": 250000.0
        }
      ],
      "services": [
        {
          "fieldServiceId": "1",
          "serviceName": "Water",
          "quantity": 2,
          "price": 20000.0
        }
      ],
      "subTotal": 540000.0,
      "status": "Pending",
      "paymentStatus": "Pending"
    },
    {
      "bookingId": "2",
      "subFieldId": "2",
      "subFieldName": "Sân 5B",
      "fieldName": "ABC Football Field",
      "bookingDate": "2025-06-01",
      "timeSlots": [
        {
          "startTime": "17:00",
          "endTime": "18:00",
          "price": 300000.0
        }
      ],
      "services": [],
      "subTotal": 300000.0,
      "status": "Pending",
      "paymentStatus": "Pending"
    }
  ],
  "totalPrice": 840000.0,
  "discount": 100000.0,
  "finalPrice": 740000.0,
  "paymentUrl": "https://sandbox.vnpay.vn/pay/123",
  "message": "Bookings created successfully"
}
```

- **400 Bad Request**:

```json
{
  "error": "Invalid input",
  "errorCode": "INVALID_INPUT",
  "details": [
    {
      "field": "bookings[0].timeSlots[0]",
      "message": "Time slot must be in 30-minute increments"
    },
    {
      "field": "bookings[1].subFieldId",
      "message": "Subfield is inactive"
    }
  ]
}
```

- **401 Unauthorized**:

```json
{
  "error": "Unauthorized",
  "errorCode": "UNAUTHORIZED",
  "message": "Invalid or missing token"
}
```

- **404 Not Found**:

```json
{
  "error": "Resource not found",
  "errorCode": "NOT_FOUND",
  "message": "Subfield or service not found"
}
```

- **409 Conflict**:

```json
{
  "error": "Conflict",
  "errorCode": "TIME_SLOT_UNAVAILABLE",
  "message": "One or more time slots are already booked",
  "details": [
    {
      "subFieldId": 1,
      "timeSlot": {
        "startTime": "16:30",
        "endTime": "17:00"
      },
      "message": "Time slot is already booked"
    }
  ]
}
```

**Note**:

- Creates multiple `Booking` records linked by `Booking.MainBookingId`.
- `MainBookingId` is a separate `Booking` record to group bookings for payment.
- `timeSlots` are stored in `BookingTimeSlot` and support 30-minute increments.
- `services` are stored in `BookingService`.
- `promotionCode` applies a discount to the `totalPrice`, stored in `Promotion.Code`.
- Prices are calculated based on `FieldPricing.PricePerHour` or `SubField.DefaultPricePerSlot` if no pricing schedule is found.
- Returns a `paymentUrl` for VNPay payment processing.
- `bookingId` and `subFieldId` are strings in JSON but map to integers in the database.
- Validates:
  - Time slot availability against existing `Booking` and `BookingTimeSlot` records.
  - `timeSlots` within `Field.OpenTime` and `Field.CloseTime`.
  - `subFieldId` belongs to an active `SubField`.
  - `promotionCode` is valid and applicable.
- Maximum limits:
  - 5 bookings per request.
  - 10 time slots per booking.
  - 20 services per booking.
- Operation is atomic: if any booking fails validation, no bookings are created.
- Time format must be `HH:mm`.

### 6.4 Add Booking Service

**Description**: Adds a service to an existing booking.

**HTTP Method**: POST  
**Endpoint**: `/api/bookings/{bookingId}/services`  
**Authorization**: Bearer Token (User)

**Path Parameters**:

- `bookingId` (required, integer): The ID of the booking.

**Request Body**:

```json
{
  "fieldServiceId": 1,
  "quantity": 2
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "bookingServiceId": 1,
    "bookingId": 1,
    "fieldServiceId": 1,
    "serviceName": "Water Bottle",
    "quantity": 2,
    "price": 10000,
    "message": "Service added successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "fieldServiceId",
        "message": "Service is not available for this field"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the booking user can add services"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Booking not found"
  }
  ```

**Note**:

- Updates `Booking.TotalPrice` after adding the service.
- Validates `FieldService` availability for the booking’s `SubField`.

### 6.5 Get Booking By Id

**Description**: Retrieves details of a specific booking.

**HTTP Method**: GET  
**Endpoint**: `/api/bookings/{bookingId}`  
**Authorization**: Bearer Token (User or Owner)

**Path Parameters**:

- `bookingId` (required, integer): The ID of the booking.

**Request Example**:

```http
GET /api/bookings/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

```json
{
  "bookingId": "1",
  "userId": "1",
  "subFieldId": "1",
  "subFieldName": "Sân 5A",
  "fieldName": "ABC Football Field",
  "bookingDate": "2025-06-01",
  "timeSlots": [
    {
      "startTime": "16:30",
      "endTime": "17:00",
      "price": 250000.0
    },
    {
      "startTime": "17:30",
      "endTime": "18:00",
      "price": 250000.0
    }
  ],
  "services": [
    {
      "fieldServiceId": "1",
      "serviceName": "Water",
      "quantity": 2,
      "price": 20000.0
    }
  ],
  "totalPrice": 540000.0,
  "status": "Pending",
  "paymentStatus": "Pending",
  "message": "Booking details retrieved successfully"
}
```

- **401 Unauthorized**:

```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing token"
}
```

- **403 Forbidden**:

```json
{
  "error": "Forbidden",
  "message": "User is not authorized to view this booking"
}
```

- **404 Not Found**:

```json
{
  "error": "Resource not found",
  "message": "Booking not found"
}
```

**Note**:

- Returns `Booking`, `BookingTimeSlot`, and `BookingService` records.
- Only the booking user or field owner can access.
- Multiple `timeSlots` may be returned if the booking has multiple slots.
- Time format is `HH:mm`.

### 6.6 Get User Bookings

**Description**: Retrieves bookings of the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/bookings`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `status` (optional, string: Confirmed|Pending|Cancelled)
- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)

**Request Example**:

```http
GET /api/bookings?page=1&pageSize=10&status=Confirmed
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "bookingId": 1,
        "fieldName": "Sân Bóng Đá ABC",
        "subFieldName": "Sân 5A",
        "bookingDate": "2025-06-01",
        "startTime": "14:00",
        "endTime": "15:00",
        "totalPrice": 500000,
        "status": "Confirmed",
        "paymentStatus": "Paid"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "startDate",
        "message": "Invalid date format"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Filters bookings by `Booking.UserId` and optional parameters.
- Only returns bookings with `MainBookingId == null` (main bookings, not sub-bookings).
- `fieldName` and `subFieldName` are derived from `Booking.SubField.Field.FieldName` and `Booking.SubField.SubFieldName`.
- `paymentStatus` reflects `Booking.PaymentStatus` (Paid|Pending|Refunded).
- Time format is `HH:mm`.

### 6.7 Get Booking Services

**Description**: Retrieves services associated with a booking.

**HTTP Method**: GET  
**Endpoint**: `/api/bookings/{bookingId}/services`  
**Authorization**: Bearer Token (User or Owner)

**Path Parameters**:

- `bookingId` (required, integer): The ID of the booking.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/bookings/1/services?page=1&pageSize=10
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "bookingServiceId": 1,
        "fieldServiceId": 1,
        "serviceName": "Water Bottle",
        "quantity": 2,
        "price": 10000
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the booking user or field owner can view services"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Booking not found"
  }
  ```

**Note**:

- Returns `BookingService` records for the specified `Booking`.
- Accessible by the booking’s user or the field’s owner.
- Pagination added for consistency with other list endpoints.

### 6.8 Update Booking

**Description**: Updates an existing booking (Owner only).

**HTTP Method**: PUT  
**Endpoint**: `/api/bookings/{bookingId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `bookingId` (required, integer): The ID of the booking.

**Request Body**:

```json
{
  "status": "Confirmed"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "bookingId": 1,
    "status": "Confirmed",
    "message": "Booking updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "status",
        "message": "Invalid status"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the field owner can update this booking"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Booking not found"
  }
  ```

### 6.9 Confirm Booking

**Description**: Confirms a pending booking (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/bookings/{bookingId}/confirm`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `bookingId` (required, integer): The ID of the booking.

**Request Example**:

```http
POST /api/bookings/1/confirm
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "bookingId": 1,
    "status": "Confirmed",
    "message": "Booking confirmed successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "bookingId",
        "message": "Booking is not pending"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the field owner can confirm this booking"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Booking not found"
  }
  ```

### 6.10 Reschedule Booking

**Description**: Reschedules an existing booking.

**HTTP Method**: POST  
**Endpoint**: `/api/bookings/{bookingId}/reschedule`  
**Authorization**: Bearer Token (User)

**Path Parameters**:

- `bookingId` (required, integer): The ID of the booking.

**Request Body**:

```json
{
  "newDate": "2025-06-02",
  "newStartTime": "15:00",
  "newEndTime": "16:00"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "bookingId": 1,
    "bookingDate": "2025-06-02",
    "startTime": "15:00",
    "endTime": "16:00",
    "message": "Booking rescheduled successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "newDate",
        "message": "New time slot is not available"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the booking user can reschedule"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Booking not found"
  }
  ```

**Note**:

- Validates `SubField` availability for the new time slot.
- Updates `Booking.BookingDate` and all `BookingTimeSlot` records.
- `newDate` must be in `YYYY-MM-DD` format.
- Time format must be `HH:mm`.

### 6.11 Cancel Booking

**Description**: Cancels a booking (User or Owner).

**HTTP Method**: POST  
**Endpoint**: `/api/bookings/{bookingId}/cancel`  
**Authorization**: Bearer Token (User or Owner)

**Path Parameters**:

- `bookingId` (required, integer): The ID of the booking.

**Request Example**:

```http
POST /api/bookings/1/cancel
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "bookingId": 1,
    "status": "Cancelled",
    "message": "Booking cancelled successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "bookingId",
        "message": "Cannot cancel past or already cancelled booking"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the booking user or field owner can cancel"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Booking not found"
  }
  ```

## 7. Payment Processing

### 7.1 Create Payment

**Description**: Initiates a payment for one or more bookings linked by `mainBookingId`. Returns a payment URL for the user to complete the transaction.

**HTTP Method**: POST  
**Endpoint**: `/api/payments`  
**Authorization**: Bearer Token (User)

**Request Body**:

```json
{
  "mainBookingId": "1",
  "amount": 740000.0,
  "paymentMethod": "VNPay"
}
```

**Response**:

- **201 Created**:

```json
{
  "paymentId": "1",
  "mainBookingId": "1",
  "amount": 740000.0,
  "status": "Pending",
  "paymentUrl": "https://sandbox.vnpay.vn/pay/123",
  "message": "Payment initiated successfully"
}
```

- **400 Bad Request**:

```json
{
  "error": "Invalid input",
  "details": [
    {
      "field": "amount",
      "message": "Amount must match booking total"
    }
  ]
}
```

- **401 Unauthorized**:

```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing token"
}
```

- **403 Forbidden**:

```json
{
  "error": "Forbidden",
  "message": "Only the booking user can initiate payment"
}
```

- **404 Not Found**:

```json
{
  "error": "Resource not found",
  "message": "Booking not found"
}
```

**Note**:

- Creates a `Payment` record linked to `Booking.MainBookingId`.
- Validates `amount` against the total of all bookings with the same `mainBookingId`.
- `mainBookingId` is a string representing the `BookingId` (integer).
- `paymentUrl` is generated by the VNPay payment gateway (e.g., sandbox URL).
- Currently supports `VNPay` as the only `paymentMethod`.

### 7.2 Get Payment Status

**Description**: Retrieves the status of a payment.

**HTTP Method**: GET  
**Endpoint**: `/api/payments/{paymentId}`  
**Authorization**: Bearer Token (User or Owner)

**Path Parameters**:

- `paymentId` (required, integer): The ID of the payment.

**Request Example**:

```http
GET /api/payments/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "paymentId": 1,
    "bookingId": 1,
    "amount": 500000,
    "status": "Completed",
    "createdAt": "2025-06-01T10:00:00Z"
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the booking user or field owner can view payment"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Payment not found"
  }
  ```

**Note**:

- `status` reflects `Payment.Status` (Pending|Completed|Failed).

### 7.3 Get Payment History

**Description**: Retrieves payment history of the authenticated user or owner.

**HTTP Method**: GET  
**Endpoint**: `/api/payments`  
**Authorization**: Bearer Token (User or Owner)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `status` (optional, string: Pending|Completed|Failed)
- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)

**Request Example**:

```http
GET /api/payments?page=1&pageSize=10&status=Completed
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "paymentId": 1,
        "bookingId": 1,
        "amount": 500000,
        "status": "Completed",
        "createdAt": "2025-06-01T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "startDate",
        "message": "Invalid date format"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- For `User`, returns payments linked to their bookings.
- For `Owner`, returns payments linked to bookings on their fields.

### 7.4 Payment Webhook

**Description**: Handles payment updates from the payment gateway.

**HTTP Method**: POST  
**Endpoint**: `/api/payments/webhook`  
**Authorization**: None (secured by webhook signature)

**Request Body**:

```json
{
  "paymentId": 1,
  "status": "Success",
  "transactionId": "txn_123",
  "amount": 500000,
  "timestamp": "2025-06-01T10:00:00Z"
}
```

**Query Parameters**:

- `vnp_SecureHash` (required, string): Signature to verify the webhook.

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Webhook processed successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "paymentId",
        "message": "Payment ID is required"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid webhook signature"
  }
  ```

**Note**:

- Updates `Payment.Status` and `Booking.PaymentStatus`.
- Secured by `vnp_SecureHash` verified by the VNPay payment gateway.
- `status` is either `"Success"` or `"Failed"`.

### 7.5 Request Refund

**Description**: Requests a refund for a booking.

**HTTP Method**: POST  
**Endpoint**: `/api/payments/{paymentId}/refund`  
**Authorization**: Bearer Token (User)

**Path Parameters**:

- `paymentId` (required, integer): The ID of the payment.

**Request Body**:

```json
{
  "amount": 500000,
  "reason": "Booking cancelled"
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "refundId": 1,
    "paymentId": 1,
    "amount": 500000,
    "status": "Pending",
    "message": "Refund requested successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "amount",
        "message": "Refund amount exceeds payment amount"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the booking user can request a refund"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Payment not found"
  }
  ```

**Note**:

- Creates a `RefundRequest` record.
- Validates `Payment.Status` is Completed and refund eligibility.
- `amount` must not exceed the original `Payment.Amount`.

### 7.6 Process Refund

**Description**: Processes a refund request (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/payments/refunds/{refundId}/process`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `refundId` (required, integer): The ID of the refund request.

**Request Body**:

```json
{
  "status": "Approved",
  "note": "Refund approved due to cancellation"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "refundId": 1,
    "status": "Approved",
    "message": "Refund processed successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "status",
        "message": "Invalid status"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the field owner can process refunds"
  }
  ```

- **404 Not Found**:

  ```json
  {
    "error": "Resource not found",
    "message": "Refund request not found"
  }
  ```

**Note**:

- Updates `RefundRequest.Status` (Approved|Rejected).
- If approved, updates `Payment.Status` and `Booking.PaymentStatus` to Refunded.

### 7.7 Payment Return

**Description**: Handles callback from the VNPay payment gateway after payment completion.

**HTTP Method**: GET  
**Endpoint**: `/api/payments/return`  
**Authorization**: None

**Query Parameters**:

- VNPay-specific parameters (e.g., `vnp_TxnRef`, `vnp_Amount`, `vnp_ResponseCode`, `vnp_SecureHash`).

**Request Example**:

```http
GET /api/payments/return?vnp_TxnRef=1&vnp_Amount=50000000&vnp_ResponseCode=00&vnp_SecureHash=abc123
```

**Response**:

- **200 OK**: Redirects to frontend success page (`/payment/success`) or failure page (`/payment/failed`) with query parameters `paymentId` and optional `message`.

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid query",
    "message": "Không tìm thấy thông tin thanh toán"
  }
  ```

**Note**:

- Redirects to `{frontendUrl}/payment/success?paymentId={paymentId}` if payment is successful.
- Redirects to `{frontendUrl}/payment/failed?paymentId={paymentId}&message={error}` if payment fails.
- `frontendUrl` is configured in `appsettings.json` (defaults to `http://localhost:5173`).

### 7.8 Payment IPN

**Description**: Handles Instant Payment Notification (IPN) from the VNPay payment gateway to update payment status.

**HTTP Method**: GET  
**Endpoint**: `/api/payments/ipn`  
**Authorization**: None

**Query Parameters**:

- VNPay-specific parameters (e.g., `vnp_TxnRef`, `vnp_Amount`, `vnp_ResponseCode`, `vnp_SecureHash`).

**Request Example**:

```http
GET /api/payments/ipn?vnp_TxnRef=1&vnp_Amount=50000000&vnp_ResponseCode=00&vnp_SecureHash=abc123
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "IPN processed successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Payment failed",
    "message": "Transaction failed"
  }
  ```

**Note**:

- Updates `Payment.Status` and `Booking.PaymentStatus` via webhook processing.
- Secured by `vnp_SecureHash` verified by VNPay.
- `vnp_Amount` is divided by 100 to convert to VND.

## 8. Review System

### 8.1 Create Review

**Description**: Creates a review for a field after a confirmed booking.

**HTTP Method**: POST  
**Endpoint**: `/api/reviews`  
**Authorization**: Bearer Token (User)

**Request Body**:

```json
{
  "fieldId": 1,
  "bookingId": 1,
  "rating": 5,
  "comment": "Great field!"
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "reviewId": 1,
    "fieldId": 1,
    "rating": 5,
    "comment": "Great field!",
    "createdAt": "2025-06-01T10:00:00Z",
    "message": "Review created successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "rating",
        "message": "Rating must be between 1 and 5"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "User is not authorized to review this field"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Booking or field not found"
  }
  ```

**Note**:

- Requires `Booking.Status` to be Confirmed and `Booking.PaymentStatus` to be Paid.
- `createdAt` reflects `Review.CreatedAt`.
- Updates `Field.AverageRating`.

### 8.2 Update Review

**Description**: Updates an existing review.

**HTTP Method**: PUT  
**Endpoint**: `/api/reviews/{reviewId}`  
**Authorization**: Bearer Token (User)

**Path Parameters**:

- `reviewId` (required, integer): The ID of the review.

**Request Body**:

```json
{
  "rating": 4,
  "comment": "Updated: Good field but needs better lighting."
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "reviewId": 1,
    "fieldId": 1,
    "rating": 4,
    "comment": "Updated: Good field but needs better lighting.",
    "message": "Review updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "rating",
        "message": "Rating must be between 1 and 5"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the review author can update this review"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Review not found"
  }
  ```

**Note**:

- Updates `Review.Rating` and `Review.Comment`.
- Recalculates `Field.AverageRating`.

### 8.3 Delete Review

**Description**: Deletes a review (User or Admin only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/reviews/{reviewId}`  
**Authorization**: Bearer Token (User or Admin)

**Path Parameters**:

- `reviewId` (required, integer): The ID of the review.

**Request Example**:

```http
DELETE /api/reviews/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Review deleted successfully"
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the review author or admin can delete this review"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Review not found"
  }
  ```

**Note**:

- Soft deletes by setting `Review.DeletedAt`.
- Recalculates `Field.AverageRating`.

## 9. Notification System

### 9.1 Get Notifications

**Description**: Retrieves notifications for the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/notifications`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `isRead` (optional, boolean): Filter by read/unread status.
- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of entries per page.

**Request Example**:

```http
GET /api/notifications?page=1&pageSize=10&isRead=false
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

```json
{
  "data": [
    {
      "notificationId": "1",
      "title": "Booking Confirmed",
      "content": "Your booking for Sân 5A on 2025-06-01 is confirmed.",
      "isRead": false,
      "createdAt": "2025-05-25T10:00:00Z",
      "notificationType": "Booking"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "message": "Notifications retrieved successfully"
}
```

- **401 Unauthorized**:

```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing token"
}
```

**Note**:

- Returns `Notification` records for the authenticated user.
- `notificationType` maps to `Notification.NotificationType`.

### 9.2 Get Unread Notification Count

**Description**: Retrieves the count of unread notifications for the authenticated user or owner.

**HTTP Method**: GET  
**Endpoint**: `/api/notifications/unread-count`  
**Authorization**: Bearer Token (User or Owner)

**Request Example**:

```http
GET /api/notifications/unread-count
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "unreadCount": 5
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Counts `Notification` records where `IsRead` is false.

### 9.3 Mark Notification As Read

**Description**: Marks a notification as read.

**HTTP Method**: POST  
**Endpoint**: `/api/notifications/{notificationId}/read`  
**Authorization**: Bearer Token (User or Owner)

**Path Parameters**:

- `notificationId` (required, integer): The ID of the notification.

**Request Example**:

```http
POST /api/notifications/1/read
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "notificationId": 1,
    "isRead": true,
    "message": "Notification marked as read"
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the notification recipient can mark it as read"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Notification not found"
  }
  ```

**Note**:

- Sets `Notification.IsRead` to true.

### 9.4 Mark All Notifications As Read

**Description**: Marks all notifications for the authenticated user or owner as read.

**HTTP Method**: POST  
**Endpoint**: `/api/notifications/read-all`  
**Authorization**: Bearer Token (User or Owner)

**Request Example**:

```http
POST /api/notifications/read-all
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "All notifications marked as read"
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Sets `Notification.IsRead` to true for all notifications of the authenticated user or owner.

### 9.5 Delete Notification

**Description**: Deletes a notification.

**HTTP Method**: DELETE  
**Endpoint**: `/api/notifications/{notificationId}`  
**Authorization**: Bearer Token (User or Owner)

**Path Parameters**:

- `notificationId` (required, integer): The ID of the notification.

**Request Example**:

```http
DELETE /api/notifications/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Notification deleted successfully"
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the notification recipient can delete it"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Notification not found"
  }
  ```

**Note**:

- Soft deletes by setting `Notification.DeletedAt`.

## 10. Owner Dashboard

### 10.1 Get Dashboard Stats

**Description**: Retrieves key statistics for the authenticated owner’s fields.

**HTTP Method**: GET  
**Endpoint**: `/api/owner/dashboard`  
**Authorization**: Bearer Token (Owner)

**Query Parameters**:

- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)

**Request Example**:

```http
GET /api/owner/dashboard?startDate=2025-06-01&endDate=2025-06-30
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "totalBookings": 50,
    "totalRevenue": 25000000,
    "averageRating": 4.5,
    "fieldCount": 3,
    "completedBookings": 40,
    "pendingBookings": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "startDate",
        "message": "Invalid date format"
      }
    ]
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Aggregates data from `Field`, `Booking`, and `Payment` for the owner’s fields.
- `averageRating` is the average of `Field.AverageRating` across all fields.

### 10.2 Get Field Stats

**Description**: Retrieves detailed statistics for a specific field.

**HTTP Method**: GET  
**Endpoint**: `/api/owner/fields/{fieldId}/stats`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)

**Request Example**:

```http
GET /api/owner/fields/1/stats?startDate=2025-06-01&endDate=2025-06-30
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "fieldId": 1,
    "totalBookings": 20,
    "totalRevenue": 10000000,
    "averageRating": 4.5,
    "bookingCompletionRate": 0.95,
    "mostBookedSubFieldId": 1
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "startDate",
        "message": "Invalid date format"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Only the field owner can view stats"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

**Note**:

- Aggregates data from `Booking` and `Payment` for the specified field.

## 11. Statistics & Analytics

### 11.1 Get User Analytics

**Description**: Retrieves analytics for user activity (Admin only).

**HTTP Method**: GET  
**Endpoint**: `/api/analytics/users`  
**Authorization**: Bearer Token (Admin)

**Query Parameters**:

- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)
- `role` (optional, string: User|Owner)

**Request Example**:

```http
GET /api/analytics/users?startDate=2025-06-01&endDate=2025-06-30&role=User
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "totalUsers": 1000,
    "newUsers": 100,
    "activeUsers": 800,
    "averageBookingsPerUser": 5,
    "totalLoyaltyPoints": 150000
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "startDate",
        "message": "Invalid date format"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

**Note**:

- Aggregates data from `Account`, `User`, and `Booking`.

### 11.2 Get Field Analytics

**Description**: Retrieves analytics for field performance (Admin only).

**HTTP Method**: GET  
**Endpoint**: `/api/analytics/fields`  
**Authorization**: Bearer Token (Admin)

**Query Parameters**:

- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)
- `sportId` (optional, integer)
- `city` (optional, string)

**Request Example**:

```http
GET /api/analytics/fields?startDate=2025-06-01&endDate=2025-06-30&sportId=1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "totalFields": 200,
    "activeFields": 180,
    "averageBookingsPerField": 25,
    "totalRevenue": 250000000,
    "averageRating": 4.3
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "startDate",
        "message": "Invalid date format"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

**Note**:

- Aggregates data from `Field`, `Booking`, and `Payment`.

## 12. Admin Management

### 12.1 Get All Users

**Description**: Retrieves a list of all users and owners (Admin only).

**HTTP Method**: GET  
**Endpoint**: `/api/admin/users`  
**Authorization**: Bearer Token (Admin)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `role` (optional, string: User|Owner)
- `search` (optional, string: email or full name)

**Request Example**:

```http
GET /api/admin/users?page=1&pageSize=10&role=User
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "userId": 1, // For User role
        "ownerId": 1, // For Owner role
        "fullName": "Nguyen Van A",
        "email": "user@example.com",
        "role": "User",
        "createdAt": "2025-06-01T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "role",
        "message": "Invalid role"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

**Note**:

- Response includes both `User` and `Owner` accounts based on `Account.Role`.
- `userId` or `ownerId` is included depending on `role`.

### 12.2 Get User By ID

**Description**: Retrieves details of a specific user or owner (Admin only).

**HTTP Method**: GET  
**Endpoint**: `/api/admin/users/{accountId}`  
**Authorization**: Bearer Token (Admin)

**Path Parameters**:

- `accountId` (required, integer): The ID of the account.

**Request Example**:

```http
GET /api/admin/users/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "accountId": 1,
    "userId": 1, // For User role
    "ownerId": 1, // For Owner role
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "role": "User",
    "phone": "0909123456",
    "city": "Hà Nội", // For User role
    "district": "Đống Đa", // For User role
    "avatarUrl": "https://cloudinary.com/avatar.jpg", // For User role
    "description": "Field owner", // For Owner role
    "createdAt": "2025-06-01T10:00:00Z"
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "User not found"
  }
  ```

**Note**:

- Response fields vary based on `Account.Role`.

### 12.3 Update User

**Description**: Updates a user or owner’s account (Admin only).

**HTTP Method**: PUT  
**Endpoint**: `/api/admin/users/{accountId}`  
**Authorization**: Bearer Token (Admin)

**Path Parameters**:

- `accountId` (required, integer): The ID of the account.

**Request Body**:

```json
{
  "fullName": "Nguyen Van A",
  "phone": "0909123456",
  "city": "Hà Nội", // For User role
  "district": "Đống Đa", // For User role
  "avatarUrl": "https://cloudinary.com/avatar.jpg", // For User role
  "description": "Field owner" // For Owner role
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "accountId": 1,
    "userId": 1, // For User role
    "ownerId": 1, // For Owner role
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "role": "User",
    "phone": "0909123456",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "avatarUrl": "https://cloudinary.com/avatar.jpg",
    "description": "Field owner",
    "message": "User updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "phone",
        "message": "Invalid phone format"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "User not found"
  }
  ```

**Note**:

- Request fields vary based on `Account.Role`.

### 12.4 Delete User

**Description**: Deletes a user or owner’s account (Admin only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/admin/users/{accountId}`  
**Authorization**: Bearer Token (Admin)

**Path Parameters**:

- `accountId` (required, integer): The ID of the account.

**Request Example**:

```http
DELETE /api/admin/users/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "User deleted successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "accountId",
        "message": "Cannot delete account with active bookings or fields"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "User not found"
  }
  ```

**Note**:

- Soft deletes by setting `Account.DeletedAt`.
- Checks for active bookings (`User`) or fields (`Owner`) before deletion.

### 12.5 Get All Fields

**Description**: Retrieves a list of all fields (Admin only).

**HTTP Method**: GET  
**Endpoint**: `/api/admin/fields`  
**Authorization**: Bearer Token (Admin)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `city` (optional, string)
- `district` (optional, string)
- `sportId` (optional, integer)
- `status` (optional, string: Active|Inactive|Deleted)

**Request Example**:

```http
GET /api/admin/fields?page=1&pageSize=10&city=Hà Nội
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "Sân Bóng Đá ABC",
        "address": "123 Đường Láng, Đống Đa",
        "city": "Hà Nội",
        "district": "Đống Đa",
        "status": "Active",
        "sportId": 1
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "status",
        "message": "Invalid status"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

### 12.6 Update Field Status

**Description**: Updates the status of a field (Admin only).

**HTTP Method**: PUT  
**Endpoint**: `/api/admin/fields/{fieldId}/status`  
**Authorization**: Bearer Token (Admin)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Request Body**:

```json
{
  "status": "Inactive"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "fieldId": 1,
    "status": "Inactive",
    "message": "Field status updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "status",
        "message": "Invalid status"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:

  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

**Note**:

- Updates `Field.Status` (Active|Inactive|Deleted).

### 12.7 Manage Refunds

**Description**: Retrieves and manages refund requests (Admin only).

**HTTP Method**: GET  
**Endpoint**: `/api/admin/refunds`  
**Authorization**: Bearer Token (Admin)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `status` (optional, string: Pending|Approved|Rejected)

**Request Example**:

```http
GET /api/admin/refunds?page=1&pageSize=10&status=Pending
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "refundId": 1,
        "paymentId": 1,
        "amount": 500000,
        "status": "Pending",
        "reason": "Booking cancelled",
        "createdAt": "2025-06-01T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "status",
        "message": "Invalid status"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

**Note**:

- Returns `RefundRequest` records filtered by `status`.

### 12.8 Get System Stats

**Description**: Retrieves system-wide statistics (Admin only).

**HTTP Method**: GET  
**Endpoint**: `/api/admin/stats`  
**Authorization**: Bearer Token (Admin)

**Query Parameters**:

- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)

**Request Example**:

```http
GET /api/admin/stats?startDate=2025-06-01&endDate=2025-06-30
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "totalUsers": 1000,
    "totalOwners": 50,
    "totalFields": 200,
    "totalBookings": 5000,
    "totalRevenue": 250000000,
    "averageBookingValue": 50000
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "startDate",
        "message": "Invalid date format"
      }
    ]
  }
  ```

- **401 Unauthorized**:

  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "Admin access required"
  }
  ```

**Note**:

- Aggregates data from `Account`, `Field`, `Booking`, and `Payment`.
