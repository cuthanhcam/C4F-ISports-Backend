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

**Description**: Registers a new user or owner account and sends a verification email.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/register`  
**Authorization**: None

**Request Body**:

```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "fullName": "Nguyen Van A",
  "phone": "0909123456",
  "role": "User", // or "Owner"
  "city": "Hà Nội", // Optional for User
  "district": "Đống Đa", // Optional for User
  "description": "Field owner" // Optional for Owner
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "accountId": 1,
    "email": "user@example.com",
    "verificationToken": "abc123",
    "message": "Registration successful. Please verify your email."
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ]
  }
  ```

- **409 Conflict**:
  ```json
  {
    "error": "Conflict",
    "message": "Email already registered"
  }
  ```

**Note**:

- `role` must be either "User" or "Owner".
- Sends a verification email with a link containing `verificationToken`.
- For `User`, `city` and `district` are optional.
- For `Owner`, `description` is optional.
- `Account.Email` is unique.

### 1.2 Login

**Description**: Authenticates a user or owner and returns access and refresh tokens.

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
    "accessToken": "jwt_token",
    "refreshToken": "refresh_token",
    "expiresIn": 3600,
    "role": "User",
    "message": "Login successful"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ]
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid email or password"
  }
  ```

**Note**:

- Updates `Account.LastLogin` on successful login.
- Returns `Account.Role` in `role`.

### 1.3 Refresh Token

**Description**: Refreshes the access token using a refresh token.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/refresh-token`  
**Authorization**: None

**Request Body**:

```json
{
  "refreshToken": "refresh_token"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "accessToken": "new_jwt_token",
    "refreshToken": "new_refresh_token",
    "expiresIn": 3600,
    "message": "Token refreshed successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "refreshToken",
        "message": "Refresh token is required"
      }
    ]
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or expired refresh token"
  }
  ```

### 1.4 Forgot Password

**Description**: Sends a password reset email with a reset token.

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
    "message": "Password reset email sent"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ]
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Email not found"
  }
  ```

**Note**:

- Generates `Account.ResetToken` and sends it via email.

### 1.5 Reset Password

**Description**: Resets the password using a reset token.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/reset-password`  
**Authorization**: None

**Request Body**:

```json
{
  "resetToken": "reset_token",
  "newPassword": "NewPassword123!"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Password reset successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "resetToken",
        "message": "Invalid or expired reset token"
      },
      {
        "field": "newPassword",
        "message": "Password must be at least 8 characters"
      }
    ]
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

- Clears `Account.ResetToken` after successful reset.

### 1.6 Logout

**Description**: Invalidates the refresh token for the authenticated user or owner.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/logout`  
**Authorization**: Bearer Token (User or Owner)

**Request Body**:

```json
{
  "refreshToken": "refresh_token"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Logout successful"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "refreshToken",
        "message": "Refresh token is required"
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

### 1.7 Get Current User

**Description**: Retrieves information of the authenticated user or owner.

**HTTP Method**: GET  
**Endpoint**: `/api/auth/me`  
**Authorization**: Bearer Token (User or Owner)

**Request Example**:

```http
GET /api/auth/me
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "accountId": 1,
    "userId": 1, // For User role
    "ownerId": 1, // For Owner role
    "email": "user@example.com",
    "fullName": "Nguyen Van A",
    "role": "User",
    "phone": "0909123456",
    "city": "Hà Nội", // For User role
    "district": "Đống Đa", // For User role
    "avatarUrl": "https://cloudinary.com/avatar.jpg", // For User role
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

**Note**:

- Response fields vary based on `Account.Role`:
  - `User`: Includes `userId`, `city`, `district`, `avatarUrl`.
  - `Owner`: Includes `ownerId`, `description`.

### 1.8 Change Password

**Description**: Changes the password of the authenticated user or owner.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/change-password`  
**Authorization**: Bearer Token (User or Owner)

**Request Body**:

```json
{
  "currentPassword": "Password123!",
  "newPassword": "NewPassword123!"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Password changed successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "currentPassword",
        "message": "Current password is incorrect"
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

### 1.9 Verify Email

**Description**: Verifies the email address using a verification token.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/verify-email`  
**Authorization**: None

**Request Body**:

```json
{
  "verificationToken": "abc123"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Email verified successfully"
  }
  ```

- **400 Bad Request**:
  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "verificationToken",
        "message": "Invalid or expired verification token"
      }
    ]
  }
  ```

### 1.10 Resend Verification Email

**Description**: Resends the verification email to the user or owner.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/resend-verification`  
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
    "message": "Verification email resent"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ]
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Email not found"
  }
  ```

### 1.11 Verify Token

**Description**: Verifies the validity of an access token.

**HTTP Method**: POST  
**Endpoint**: `/api/auth/verify-token`  
**Authorization**: Bearer Token (User or Owner)

**Request Body**:

```json
{
  "accessToken": "jwt_token"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "isValid": true,
    "role": "User",
    "message": "Token is valid"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "accessToken",
        "message": "Access token is required"
      }
    ]
  }
  ```

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or expired token"
  }
  ```

---

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

**Note**:

- Response fields vary based on `Account.Role`:
  - `User`: Includes `userId`, `city`, `district`, `avatarUrl`.
  - `Owner`: Includes `ownerId`, `description`.

### 2.2 Update Profile

**Description**: Updates the profile of the authenticated user or owner.

**HTTP Method**: PUT  
**Endpoint**: `/api/users/profile`  
**Authorization**: Bearer Token (User or Owner)

**Request Body**:

```json
{
  // For User role
  "fullName": "Nguyen Van A",
  "phone": "0909123456",
  "city": "Hà Nội",
  "district": "Đống Đa",
  "avatarUrl": "https://cloudinary.com/avatar.jpg",
  // For Owner role
  "description": "Field owner"
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
  "district": "Đống Đa"
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "userId": 1, // For User role
    "ownerId": 1, // For Owner role
    "fullName": "Nguyen Van A",
    "phone": "0909123456",
    "city": "Hà Nội", // For User role
    "district": "Đống Đa", // For User role
    "avatarUrl": "https://cloudinary.com/avatar.jpg", // For User role
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
  - `User`: Can update `fullName`, `phone`, `city`, `district`, `avatarUrl`.
  - `Owner`: Can update `fullName`, `phone`, `description`.

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
    "error": "Invalid input",
    "details": [
      {
        "field": "account",
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

**Note**:

- Sets `Account.DeletedAt` for soft delete.
- Checks for active bookings (`User`) or fields (`Owner`) before deletion.

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
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Returns the total loyalty points stored in `User.LoyaltyPoints`.
- No pagination is required as this endpoint returns a single value.

### 2.5 Get Favorite Fields

**Description**: Retrieves the favorite fields of the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/users/favorites`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/users/favorites?page=1&pageSize=10
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

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

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
    "message": "Invalid or missing token"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

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
    "message": "Invalid or missing token"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

### 2.8 Get User Search History

**Description**: Retrieves the search history of the authenticated user. Supports pagination and filtering by date range. Search history includes keywords and optional metadata like field ID or location coordinates.

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
      "searchId": "1",
      "userId": "1",
      "keyword": "football field Hanoi",
      "searchDateTime": "2025-05-25T10:00:00Z",
      "fieldId": "1",
      "latitude": 21.0001,
      "longitude": 105.0001
    },
    {
      "searchId": "2",
      "userId": "1",
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
  "message": "User is not authorized to access this resource"
}
```

**Note**:

- Returns `SearchHistory` records for the authenticated user.
- `userId` maps to `SearchHistory.UserId`.
- `searchId` maps to `SearchHistory.SearchId`.
- Supports filtering by `startDate` and `endDate` for `SearchHistory.SearchDateTime`.
- `fieldId`, `latitude`, and `longitude` are optional and may be null.

### 2.9 Get Booking History

**Description**: Retrieves the booking history of the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/users/bookings`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `status` (optional, string: Confirmed|Pending|Cancelled)
- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)

**Request Example**:

```http
GET /api/users/bookings?page=1&pageSize=10&status=Confirmed
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
- `paymentStatus` reflects `Booking.PaymentStatus` (Paid|Unpaid|Refunded).

### 2.10 Get User Reviews

**Description**: Retrieves reviews made by the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/users/reviews`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/users/reviews?page=1&pageSize=10
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "reviewId": 1,
        "fieldId": 1,
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

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

---

## 3. Sport Categories

### 3.1 Get Sports

**Description**: Retrieves a list of sport categories.

**HTTP Method**: GET  
**Endpoint**: `/api/sports`  
**Authorization**: None

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/sports?page=1&pageSize=10
```

**Response**:

- **200 OK**:
  ```json
  {
    "data": [
      {
        "sportId": 1,
        "sportName": "Football",
        "description": "Soccer fields for 5-a-side or 11-a-side",
        "isActive": true
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

### 3.2 Get Sport By ID

**Description**: Retrieves details of a specific sport.

**HTTP Method**: GET  
**Endpoint**: `/api/sports/{sportId}`  
**Authorization**: None

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
    "description": "Soccer fields for 5-a-side or 11-a-side",
    "isActive": true
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Sport not found"
  }
  ```

### 3.3 Get Fields By Sport

**Description**: Retrieves fields associated with a sport.

**HTTP Method**: GET  
**Endpoint**: `/api/sports/{sportId}/fields`  
**Authorization**: None

**Path Parameters**:

- `sportId` (required, integer): The ID of the sport.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `city` (optional, string)
- `district` (optional, string)

**Request Example**:

```http
GET /api/sports/1/fields?page=1&pageSize=10&city=Hà Nội
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
        "averageRating": 4.5
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
    "message": "Sport not found"
  }
  ```

**Note**:

- Filters `Field` records by `Field.SportId` and optional parameters.

### 3.4 Create Sport

**Description**: Creates a new sport category (Admin only).

**HTTP Method**: POST  
**Endpoint**: `/api/sports`  
**Authorization**: Bearer Token (Admin)

**Request Body**:

```json
{
  "sportName": "Basketball",
  "description": "Basketball courts",
  "isActive": true
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "sportId": 2,
    "sportName": "Basketball",
    "description": "Basketball courts",
    "isActive": true,
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
        "message": "Sport name is required"
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

- `isActive` defaults to true if not specified.

### 3.5 Update Sport

**Description**: Updates an existing sport (Admin only).

**HTTP Method**: PUT  
**Endpoint**: `/api/sports/{sportId}`  
**Authorization**: Bearer Token (Admin)

**Path Parameters**:

- `sportId` (required, integer): The ID of the sport.

**Request Body**:

```json
{
  "sportName": "Football",
  "description": "Updated description",
  "isActive": true
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "sportId": 1,
    "sportName": "Football",
    "description": "Updated description",
    "isActive": true,
    "message": "Sport updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "sportName",
        "message": "Sport name is required"
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
    "message": "Sport not found"
  }
  ```

### 3.6 Delete Sport

**Description**: Deactivates a sport category (Admin only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/sports/{sportId}`  
**Authorization**: Bearer Token (Admin)

**Path Parameters**:

- `sportId` (required, integer): The ID of the sport.

**Request Example**:

```http
DELETE /api/sports/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "sportId": 1,
    "isActive": false,
    "message": "Sport deactivated successfully"
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
    "message": "Sport not found"
  }
  ```

---

## 4. Field Management

### 4.1 Get Fields

**Description**: Retrieves a list of fields with optional filters.

**HTTP Method**: GET  
**Endpoint**: `/api/fields`  
**Authorization**: None

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `city` (optional, string)
- `district` (optional, string)
- `sportId` (optional, integer)
- `search` (optional, string: field name or address)

**Request Example**:

```http
GET /api/fields?page=1&pageSize=10&city=Hà Nội&sportId=1
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
        "latitude": 21.0123,
        "longitude": 105.8234,
        "openTime": "06:00",
        "closeTime": "22:00",
        "averageRating": 4.5,
        "sportId": 1
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```

### 4.2 Get Field By ID

**Description**: Retrieves details of a specific field.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Request Example**:

```http
GET /api/fields/1
```

**Response**:

- **200 OK**:

  ```json
  {
    "fieldId": 1,
    "fieldName": "Sân Bóng Đá ABC",
    "address": "123 Đường Láng, Đống Đa",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "latitude": 21.0123,
    "longitude": 105.8234,
    "openTime": "06:00",
    "closeTime": "22:00",
    "averageRating": 4.5,
    "sportId": 1
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

### 4.3 Validate Address

**Description**: Validates an address and returns geocoding information.

**HTTP Method**: POST  
**Endpoint**: `/api/fields/validate-address`  
**Authorization**: None

**Request Body**:

```json
{
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

**Note**:

- Uses a geocoding service to validate and derive `latitude` and `longitude`.

### 4.4 Create Field

**Description**: Creates a new field for the authenticated owner.

**HTTP Method**: POST  
**Endpoint**: `/api/fields`  
**Authorization**: Bearer Token (Owner)

**Request Body**:

```json
{
  "fieldName": "Sân Bóng Đá ABC",
  "address": "123 Đường Láng, Đống Đa",
  "city": "Hà Nội",
  "district": "Đống Đa",
  "openTime": "06:00",
  "closeTime": "22:00",
  "sportId": 1
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "fieldId": 1,
    "fieldName": "Sân Bóng Đá ABC",
    "address": "123 Đường Láng, Đống Đa",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "openTime": "06:00",
    "closeTime": "22:00",
    "sportId": 1,
    "latitude": 21.0123,
    "longitude": 105.8234,
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

- `latitude` and `longitude` are derived via geocoding from `address`, `city`, and `district`.
- `OwnerId` is automatically set to the authenticated owner's ID.
- `sportId` must correspond to an existing `Sport`.

### 4.5 Create Full Field

**Description**: Creates a new field with all associated components (subfields, services, amenities, images, pricing rules) in a single request (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/fields/full`  
**Authorization**: Bearer Token (Owner)

**Request Body**:

```json
{
  "fieldName": "Sân Bóng Đá ABC",
  "address": "123 Đường Láng, Đống Đa",
  "city": "Hà Nội",
  "district": "Đống Đa",
  "openTime": "06:00",
  "closeTime": "22:00",
  "sportId": 1,
  "subFields": [
    {
      "subFieldName": "Sân 5A",
      "fieldType": "5-a-side",
      "status": "Active",
      "capacity": 10,
      "description": "Sân cỏ nhân tạo",
      "pricingRules": [
        {
          "dayOfWeek": "Monday",
          "startTime": "14:00",
          "endTime": "15:00",
          "pricePerHour": 600000
        }
      ]
    }
  ],
  "services": [
    {
      "serviceName": "Water Bottle",
      "price": 10000,
      "description": "500ml water bottle"
    }
  ],
  "amenities": [
    {
      "amenityName": "Parking",
      "description": "Free parking for 50 cars"
    }
  ],
  "images": [
    {
      "imageUrl": "https://cloudinary.com/field-image.jpg",
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
    "address": "123 Đường Láng, Đống Đa",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "openTime": "06:00",
    "closeTime": "22:00",
    "sportId": 1,
    "latitude": 21.0123,
    "longitude": 105.8234,
    "subFields": [
      {
        "subFieldId": 1,
        "subFieldName": "Sân 5A",
        "fieldType": "5-a-side",
        "status": "Active",
        "capacity": 10,
        "description": "Sân cỏ nhân tạo",
        "pricingRules": [
          {
            "pricingRuleId": 1,
            "dayOfWeek": "Monday",
            "startTime": "14:00",
            "endTime": "15:00",
            "pricePerHour": 600000
          }
        ]
      }
    ],
    " Jon    {
      "fieldServiceId": 1,
      "serviceName": "Water Bottle",
      "price": 10000,
      "description": "500ml water bottle"
    }
  ],
  "amenities": [
    {
      "fieldAmenityId": 1,
      "amenityName": "Parking",
      "description": "Free parking for 50 cars"
    }
  ],
  "images": [
    {
      "imageId": 1,
      "imageUrl": "https://cloudinary.com/field-image.jpg",
      "isPrimary": true
    }
  ],
  "message": "Field and components created successfully"
  }
  ```

````

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
        "field": "subFields[0].subFieldName",
        "message": "Subfield name is required"
      }
    ]
  }
````

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

````

- **403 Forbidden**:
  ```json
  {
    "error": "Forbidden",
    "message": "Only owners can create fields"
  }
````

**Note**:

- `latitude` and `longitude` are derived via geocoding from `address`, `city`, and `district`.
- `OwnerId` is automatically set to the authenticated owner's ID.
- `sportId` must correspond to an existing `Sport`.
- All components (`subFields`, `services`, `amenities`, `images`, `pricingRules`) are optional but validated if provided.
- The operation is atomic: if any component fails validation, no data is saved.
- Maximum limits: 10 subfields, 50 services, 50 amenities, 50 images per request.

### 4.6 Create SubField

**Description**: Creates a new subfield for a field (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/fields/{fieldId}/subfields`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Request Body**:

```json
{
  "subFieldName": "Sân 5A",
  "pricePerHour": 500000
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "subFieldId": 1,
    "fieldId": 1,
    "subFieldName": "Sân 5A",
    "pricePerHour": 500000,
    "message": "Subfield created successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "subFieldName",
        "message": "Subfield name is required"
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
    "message": "Only the field owner can create subfields"
  }
  ```

### 4.7 Create Pricing Rule

**Description**: Creates a pricing rule for a subfield (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/subfields/{subFieldId}/pricing`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `subFieldId` (required, integer): The ID of the subfield.

**Request Body**:

```json
{
  "dayOfWeek": "Monday",
  "startTime": "14:00",
  "endTime": "15:00",
  "pricePerHour": 600000
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "pricingRuleId": 1,
    "subFieldId": 1,
    "dayOfWeek": "Monday",
    "startTime": "14:00",
    "endTime": "15:00",
    "pricePerHour": 600000,
    "message": "Pricing rule created successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "dayOfWeek",
        "message": "Invalid day of week"
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
    "message": "Only the field owner can create pricing rules"
  }
  ```

### 4.8 Create Field Service

**Description**: Creates a service for a field (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/fields/{fieldId}/services`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Request Body**:

```json
{
  "serviceName": "Water Bottle",
  "price": 10000
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "fieldServiceId": 1,
    "fieldId": 1,
    "serviceName": "Water Bottle",
    "price": 10000,
    "message": "Service created successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "serviceName",
        "message": "Service name is required"
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
    "message": "Only the field owner can create services"
  }
  ```

### 4.9 Create Field Amenity

**Description**: Creates a new amenity for a specific field. Only the field owner or admin can create amenities.

**HTTP Method**: POST  
**Endpoint**: `/api/fields/{fieldId}/amenities`  
**Authorization**: Bearer Token (Owner or Admin)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Request Body**:

```json
{
  "amenityName": "Shower",
  "description": "Hot and cold shower facilities",
  "iconUrl": "https://example.com/icons/shower.png"
}
```

**Response**:

- **201 Created**:

```json
{
  "fieldAmenityId": "3",
  "fieldId": "1",
  "amenityName": "Shower",
  "description": "Hot and cold shower facilities",
  "iconUrl": "https://example.com/icons/shower.png",
  "status": "Active",
  "message": "Amenity created successfully"
}
```

- **400 Bad Request**:

```json
{
  "error": "Invalid input",
  "details": [
    {
      "field": "amenityName",
      "message": "Amenity name is required"
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
  "message": "Only the field owner or admin can create amenities"
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

- Creates a `FieldAmenity` record with `status` set to Active by default.
- `amenityName`, `description`, and `iconUrl` map to `FieldAmenity.AmenityName`, `FieldAmenity.Description`, and `FieldAmenity.IconUrl`.

### 4.10 Upload Field Image

**Description**: Uploads an image for a field (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/fields/{fieldId}/images`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Request Body**: Form-data with key `image` (file)

**Request Example**:

```http
POST /api/fields/1/images
Authorization: Bearer {token}
Content-Type: multipart/form-data

[image: field_image.jpg]
```

**Response**:

- **201 Created**:

  ```json
  {
    "imageId": 1,
    "imageUrl": "https://cloudinary.com/field-image.jpg",
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
    "message": "Only the field owner can upload images"
  }
  ```

- **413 Payload Too Large**:
  ```json
  {
    "error": "Payload too large",
    "message": "Image file exceeds maximum size"
  }
  ```

### 4.11 Add Field Description

**Description**: Adds a description to a field (Owner only).

**HTTP Method**: POST  
**Endpoint**: `/api/fields/{fieldId}/descriptions`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Request Body**:

```json
{
  "description": "Modern football field with artificial turf."
}
```

**Response**:

- **201 Created**:

  ```json
  {
    "fieldDescriptionId": 1,
    "fieldId": 1,
    "description": "Modern football field with artificial turf.",
    "message": "Description added successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "description",
        "message": "Description is required"
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
    "message": "Only the field owner can add descriptions"
  }
  ```

### 4.12 Update Field

**Description**: Updates an existing field (Owner only).

**HTTP Method**: PUT  
**Endpoint**: `/api/fields/{fieldId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field to update.

**Request Body**:

```json
{
  "fieldName": "Sân Bóng Đá ABC",
  "address": "123 Đường Láng, Đống Đa",
  "city": "Hà Nội",
  "district": "Đống Đa",
  "openTime": "06:00",
  "closeTime": "22:00",
  "sportId": 1
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "fieldId": 1,
    "fieldName": "Sân Bóng Đá ABC",
    "address": "123 Đường Láng, Đống Đa",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "openTime": "06:00",
    "closeTime": "22:00",
    "sportId": 1,
    "latitude": 21.0123,
    "longitude": 105.8234,
    "message": "Field updated successfully"
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
    "message": "Only the field owner can update this field"
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

- Updates `latitude` and `longitude` via geocoding if `address`, `city`, or `district` changes.
- Checks if the authenticated owner owns the field.

### 4.13 Update Full Field

**Description**: Updates an existing field and its associated components (subfields, services, amenities, images, pricing rules) in a single request (Owner only).

**HTTP Method**: PUT  
**Endpoint**: `/api/fields/{fieldId}/full`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field to update.

**Request Body**:

```json
{
  "fieldName": "Sân Bóng Đá ABC",
  "address": "123 Đường Láng, Đống Đa",
  "city": "Hà Nội",
  "district": "Đống Đa",
  "openTime": "06:00",
  "closeTime": "22:00",
  "sportId": 1,
  "subFields": [
    {
      "subFieldId": 1, // Existing subfield
      "subFieldName": "Sân 5A Updated",
      "fieldType": "5-a-side",
      "status": "Active",
      "capacity": 10,
      "description": "Sân cỏ nhân tạo",
      "pricingRules": [
        {
          "pricingRuleId": 1, // Existing pricing rule
          "dayOfWeek": "Monday",
          "startTime": "14:00",
          "endTime": "15:00",
          "pricePerHour": 650000
        },
        {
          // New pricing rule
          "dayOfWeek": "Tuesday",
          "startTime": "14:00",
          "endTime": "15:00",
          "pricePerHour": 600000
        }
      ]
    },
    {
      // New subfield
      "subFieldName": "Sân 7B",
      "fieldType": "7-a-side",
      "status": "Active",
      "capacity": 14,
      "description": "Sân cỏ tự nhiên"
    }
  ],
  "services": [
    {
      "fieldServiceId": 1, // Existing service
      "serviceName": "Water Bottle",
      "price": 12000,
      "description": "500ml water bottle"
    },
    {
      // New service
      "serviceName": "Towel",
      "price": 15000,
      "description": "Clean towel"
    }
  ],
  "amenities": [
    {
      "fieldAmenityId": 1, // Existing amenity
      "amenityName": "Parking",
      "description": "Free parking for 50 cars"
    },
    {
      // New amenity
      "amenityName": "Shower",
      "description": "Hot water showers"
    }
  ],
  "images": [
    {
      "imageId": 1, // Existing image
      "imageUrl": "https://cloudinary.com/field-image-updated.jpg",
      "isPrimary": true
    },
    {
      // New image
      "imageUrl": "https://cloudinary.com/field-image-new.jpg",
      "isPrimary": false
    }
  ]
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "fieldId": 1,
    "fieldName": "Sân Bóng Đá ABC",
    "address": "123 Đường Láng, Đống Đa",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "openTime": "06:00",
    "closeTime": "22:00",
    "sportId": 1,
    "latitude": 21.0123,
    "longitude": 105.8234,
    "subFields": [
      {
        "subFieldId": 1,
        "subFieldName": "Sân 5A Updated",
        "fieldType": "5-a-side",
        "status": "Active",
        "capacity": 10,
        "description": "Sân cỏ nhân tạo",
        "pricingRules": [
          {
            "pricingRuleId": 1,
            "dayOfWeek": "Monday",
            "startTime": "14:00",
            "endTime": "15:00",
            "pricePerHour": 650000
          },
          {
            "pricingRuleId": 2,
            "dayOfWeek": "Tuesday",
            "startTime": "14:00",
            "endTime": "15:00",
            "pricePerHour": 600000
          }
        ]
      },
      {
        "subFieldId": 2,
        "subFieldName": "Sân 7B",
        "fieldType": "7-a-side",
        "status": "Active",
        "capacity": 14,
        "description": "Sân cỏ tự nhiên"
      }
    ],
    "services": [
      {
        "fieldServiceId": 1,
        "serviceName": "Water Bottle",
        "price": 12000,
        "description": "500ml water bottle"
      },
      {
        "fieldServiceId": 2,
        "serviceName": "Towel",
        "price": 15000,
        "description": "Clean towel"
      }
    ],
    "amenities": [
      {
        "fieldAmenityId": 1,
        "amenityName": "Parking",
        "description": "Free parking for 50 cars"
      },
      {
        "fieldAmenityId": 2,
        "amenityName": "Shower",
        "description": "Hot water showers"
      }
    ],
    "images": [
      {
        "imageId": 1,
        "imageUrl": "https://cloudinary.com/field-image-updated.jpg",
        "isPrimary": true
      },
      {
        "imageId": 2,
        "imageUrl": "https://cloudinary.com/field-image-new.jpg",
        "isPrimary": false
      }
    ],
    "message": "Field and components updated successfully"
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
        "field": "subFields[0].subFieldId",
        "message": "Subfield ID does not exist"
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
    "message": "Only the field owner can update this field"
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

- Updates `latitude` and `longitude` via geocoding if `address`, `city`, or `district` changes.
- Checks if the authenticated owner owns the field.
- Components (`subFields`, `services`, `amenities`, `images`, `pricingRules`) are optional:
  - If `subFieldId`, `fieldServiceId`, `fieldAmenityId`, or `imageId` is provided, updates the existing record.
  - If not provided, creates a new record.
- The operation is atomic: if any component fails validation, no changes are applied.
- Maximum limits: 10 subfields, 50 services, 50 amenities, 50 images per request.
- Existing components not included in the request remain unchanged.

### 4.14 Update SubField

**Description**: Updates an existing subfield (Owner only).

**HTTP Method**: PUT  
**Endpoint**: `/api/subfields/{subFieldId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `subFieldId` (required, integer): The ID of the subfield.

**Request Body**:

```json
{
  "subFieldName": "Sân 5A",
  "pricePerHour": 550000
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "subFieldId": 1,
    "fieldId": 1,
    "subFieldName": "Sân 5A",
    "pricePerHour": 550000,
    "message": "Subfield updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "subFieldName",
        "message": "Subfield name is required"
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
    "message": "Only the field owner can update this subfield"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Subfield not found"
  }
  ```

### 4.15 Update Pricing Rule

**Description**: Updates an existing pricing rule (Owner only).

**HTTP Method**: PUT  
**Endpoint**: `/api/subfields/pricing/{pricingRuleId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `pricingRuleId` (required, integer): The ID of the pricing rule.

**Request Body**:

```json
{
  "dayOfWeek": "Monday",
  "startTime": "14:00",
  "endTime": "15:00",
  "pricePerHour": 650000
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "pricingRuleId": 1,
    "subFieldId": 1,
    "dayOfWeek": "Monday",
    "startTime": "14:00",
    "endTime": "15:00",
    "pricePerHour": 650000,
    "message": "Pricing rule updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "dayOfWeek",
        "message": "Invalid day of week"
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
    "message": "Only the field owner can update pricing rules"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Pricing rule not found"
  }
  ```

### 4.16 Update Field Service

**Description**: Updates an existing field service (Owner only).

**HTTP Method**: PUT  
**Endpoint**: `/api/fields/services/{fieldServiceId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldServiceId` (required, integer): The ID of the service.

**Request Body**:

```json
{
  "serviceName": "Water Bottle",
  "price": 12000
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "fieldServiceId": 1,
    "fieldId": 1,
    "serviceName": "Water Bottle",
    "price": 12000,
    "message": "Service updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "serviceName",
        "message": "Service name is required"
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
    "message": "Only the field owner can update services"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Service not found"
  }
  ```

### 4.17 Update Field Amenity

**Description**: Updates an existing amenity for a specific field. Only the field owner or admin can update amenities.

**HTTP Method**: PUT  
**Endpoint**: `/api/fields/{fieldId}/amenities/{fieldAmenityId}`  
**Authorization**: Bearer Token (Owner or Admin)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.
- `fieldAmenityId` (required, integer): The ID of the amenity.

**Request Body**:

```json
{
  "amenityName": "Shower",
  "description": "Updated shower facilities with hot water",
  "iconUrl": "https://example.com/icons/shower-updated.png",
  "status": "Active"
}
```

**Response**:

- **200 OK**:

```json
{
  "fieldAmenityId": "3",
  "fieldId": "1",
  "amenityName": "Shower",
  "description": "Updated shower facilities with hot water",
  "iconUrl": "https://example.com/icons/shower-updated.png",
  "status": "Active",
  "message": "Amenity updated successfully"
}
```

- **400 Bad Request**:

```json
{
  "error": "Invalid input",
  "details": [
    {
      "field": "amenityName",
      "message": "Amenity name is required"
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
  "message": "Only the field owner or admin can update amenities"
}
```

- **404 Not Found**:

```json
{
  "error": "Resource not found",
  "message": "Field or amenity not found"
}
```

**Note**:

- Updates `FieldAmenity.AmenityName`, `FieldAmenity.Description`, `FieldAmenity.IconUrl`, and `FieldAmenity.Status`.
- `status` can be Active or Inactive.

### 4.18 Update Field Description

**Description**: Updates an existing field description (Owner only).

**HTTP Method**: PUT  
**Endpoint**: `/api/fields/descriptions/{fieldDescriptionId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldDescriptionId` (required, integer): The ID of the description.

**Request Body**:

```json
{
  "description": "Updated modern football field description."
}
```

**Response**:

- **200 OK**:

  ```json
  {
    "fieldDescriptionId": 1,
    "fieldId": 1,
    "description": "Updated modern football field description.",
    "message": "Description updated successfully"
  }
  ```

- **400 Bad Request**:

  ```json
  {
    "error": "Invalid input",
    "details": [
      {
        "field": "description",
        "message": "Description is required"
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
    "message": "Only the field owner can update descriptions"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Description not found"
  }
  ```

### 4.19 Delete SubField

**Description**: Soft deletes a subfield by setting its `DeletedAt` timestamp (Owner only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/subfields/{subFieldId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `subFieldId` (required, integer): The ID of the subfield to delete.

**Request Example**:

```http
DELETE /api/subfields/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "subFieldId": 1,
    "message": "Subfield deleted successfully"
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
    "message": "Only the field owner can delete this subfield"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Subfield not found"
  }
  ```

**Note**:

- Sets `SubField.DeletedAt` to the current timestamp for soft delete.
- Checks if the authenticated owner owns the parent field.

### 4.20 Delete Pricing Rule

**Description**: Deletes a pricing rule (Owner only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/subfields/pricing/{pricingRuleId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `pricingRuleId` (required, integer): The ID of the pricing rule.

**Request Example**:

```http
DELETE /api/subfields/pricing/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Pricing rule deleted successfully"
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
    "message": "Only the field owner can delete pricing rules"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Pricing rule not found"
  }
  ```

### 4.21 Delete Field Service

**Description**: Deletes a field service (Owner only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/fields/services/{fieldServiceId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldServiceId` (required, integer): The ID of the service.

**Request Example**:

```http
DELETE /api/fields/services/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Service deleted successfully"
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
    "message": "Only the field owner can delete services"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Service not found"
  }
  ```

### 4.22 Delete Field Amenity

**Description**: Soft deletes an amenity by setting its `DeletedAt` timestamp. Only the field owner or admin can delete amenities.

**HTTP Method**: DELETE  
**Endpoint**: `/api/fields/{fieldId}/amenities/{fieldAmenityId}`  
**Authorization**: Bearer Token (Owner or Admin)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.
- `fieldAmenityId` (required, integer): The ID of the amenity.

**Request Example**:

```http
DELETE /api/fields/1/amenities/3
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

```json
{
  "message": "Amenity deleted successfully"
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
  "message": "Only the field owner or admin can delete amenities"
}
```

- **404 Not Found**:

```json
{
  "error": "Resource not found",
  "message": "Field or amenity not found"
}
```

**Note**:

- Sets `FieldAmenity.DeletedAt` for soft delete.
- Does not physically remove the record from the database.

### 4.23 Delete Field Image

**Description**: Deletes an image from a field (Owner only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/fields/{fieldId}/images/{imageId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.
- `imageId` (required, integer): The ID of the image.

**Request Example**:

```http
DELETE /api/fields/1/images/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Image deleted successfully"
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
    "message": "Only the field owner can delete images"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Image not found"
  }
  ```

### 4.24 Delete Field Description

**Description**: Deletes a field description (Owner only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/fields/descriptions/{fieldDescriptionId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldDescriptionId` (required, integer): The ID of the description.

**Request Example**:

```http
DELETE /api/fields/descriptions/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "message": "Description deleted successfully"
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
    "message": "Only the field owner can delete descriptions"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Description not found"
  }
  ```

### 4.25 Delete Field

**Description**: Soft deletes a field by setting its status to `Deleted` (Owner only).

**HTTP Method**: DELETE  
**Endpoint**: `/api/fields/{fieldId}`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field to delete.

**Request Example**:

```http
DELETE /api/fields/1
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "fieldId": 1,
    "status": "Deleted",
    "message": "Field deleted successfully"
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
    "message": "Only the field owner can delete this field"
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

- Sets `Field.Status` to `Deleted` for soft delete.
- Checks if the authenticated owner owns the field.

### 4.26 Get Field Availability

**Description**: Retrieves available time slots for a field or subfield.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/availability`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `subFieldId` (optional, integer)
- `date` (required, date: YYYY-MM-DD)
- `sportId` (optional, integer)

**Request Example**:

```http
GET /api/fields/1/availability?date=2025-06-01&subFieldId=1
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "subFieldId": 1,
        "subFieldName": "Sân 5A",
        "availableSlots": [
          {
            "startTime": "14:00",
            "endTime": "15:00"
          }
        ]
      },
      {
        "subFieldId": 2,
        "subFieldName": "Sân 7B",
        "availableSlots": [
          {
            "startTime": "15:00",
            "endTime": "16:00"
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
        "field": "date",
        "message": "Invalid date format"
      }
    ]
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "No fields found matching the criteria"
  }
  ```

**Note**:

- Checks `SubField` availability based on `Booking` records.

### 4.27 Get Field Images

**Description**: Retrieves images of a field.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/images`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/fields/1/images?page=1&pageSize=10
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "imageId": 1,
        "imageUrl": "https://cloudinary.com/field-image.jpg"
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

### 4.28 Get Field Services

**Description**: Retrieves services of a field.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/services`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/fields/1/services?page=1&pageSize=10
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "fieldServiceId": 1,
        "serviceName": "Water Bottle",
        "price": 10000
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

### 4.29 Get Field Amenities

**Description**: Retrieves a list of amenities for a specific field. Supports pagination and status filtering. Amenities are publicly accessible for viewing.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/amenities`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `status` (optional, enum: Active|Inactive): Filter amenities by status.
- `page` (optional, integer, default: 1): Page number.
- `pageSize` (optional, integer, default: 10): Number of entries per page.

**Request Example**:

```http
GET /api/fields/1/amenities?page=1&pageSize=10&status=Active
```

**Response**:

- **200 OK**:

```json
{
  "data": [
    {
      "fieldAmenityId": "1",
      "fieldId": "1",
      "amenityName": "Wi-Fi",
      "description": "Free high-speed Wi-Fi",
      "iconUrl": "https://example.com/icons/w Ascending
      {
        "fieldAmenityId": "2",
        "fieldId": "1",
        "amenityName": "Parking",
        "description": "Free parking for 50 cars",
        "iconUrl": "https://example.com/icons/parking.png",
        "status": "Active"
      }
    ],
    "totalCount": 2,
    "page": 1,
    "pageSize": 10,
    "message": "Amenities retrieved successfully"
  }
```

- **400 Bad Request**:

```json
{
  "error": "Invalid input",
  "details": [
    {
      "field": "page",
      "message": "Page number must be greater than or equal to 1"
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

**Note**:

- Returns `FieldAmenity` records for the specified `fieldId`.
- `fieldAmenityId` and `fieldId` are returned as strings for consistency.
- `status` defaults to Active if not specified.
- `iconUrl` is optional and maps to `FieldAmenity.IconUrl`.

### 4.30 Get Field Descriptions

**Description**: Retrieves descriptions of a field.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/descriptions`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/fields/1/descriptions?page=1&pageSize=10
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "fieldDescriptionId": 1,
        "description": "Modern football field with artificial turf."
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

### 4.31 Get SubFields

**Description**: Retrieves subfields of a field.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/subfields`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/fields/1/subfields?page=1&pageSize=10
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "subFieldId": 1,
        "subFieldName": "Sân 5A",
        "pricePerHour": 500000
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

### 4.32 Get SubField By ID

**Description**: Retrieves details of a specific subfield.

**HTTP Method**: GET  
**Endpoint**: `/api/subfields/{subFieldId}`  
**Authorization**: None

**Path Parameters**:

- `subFieldId` (required, integer): The ID of the subfield.

**Request Example**:

```http
GET /api/subfields/1
```

**Response**:

- **200 OK**:

  ```json
  {
    "subFieldId": 1,
    "fieldId": 1,
    "subFieldName": "Sân 5A",
    "pricePerHour": 500000
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Subfield not found"
  }
  ```

### 4.33 Get Pricing Rules

**Description**: Retrieves pricing rules for a subfield.

**HTTP Method**: GET  
**Endpoint**: `/api/subfields/{subFieldId}/pricing`  
**Authorization**: None

**Path Parameters**:

- `subFieldId` (required, integer): The ID of the subfield.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/subfields/1/pricing?page=1&pageSize=10
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "pricingRuleId": 1,
        "dayOfWeek": "Monday",
        "startTime": "14:00",
        "endTime": "15:00",
        "pricePerHour": 600000
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
    "message": "Subfield not found"
  }
  ```

### 4.34 Get Field Reviews

**Description**: Retrieves reviews for a field.

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/reviews`  
**Authorization**: None

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `minRating` (optional, integer: 1-5)

**Request Example**:

```http
GET /api/fields/1/reviews?page=1&pageSize=10&minRating=4
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
        "createdAt": "2025-06-01T10:00:00Z"
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

### 4.35 Get Field Bookings

**Description**: Retrieves bookings for a field (Owner only).

**HTTP Method**: GET  
**Endpoint**: `/api/fields/{fieldId}/bookings`  
**Authorization**: Bearer Token (Owner)

**Path Parameters**:

- `fieldId` (required, integer): The ID of the field.

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)
- `status` (optional, string: Confirmed|Pending|Cancelled)
- `startDate` (optional, date: YYYY-MM-DD)
- `endDate` (optional, date: YYYY-MM-DD)

**Request Example**:

```http
GET /api/fields/1/bookings?page=1&pageSize=10&status=Confirmed
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "bookingId": 1,
        "userId": 1,
        "subFieldId": 1,
        "bookingDate": "2025-06-01",
        "startTime": "14:00",
        "endTime": "15:00",
        "totalPrice": 500000,
        "status": "Confirmed"
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
    "message": "Only the field owner can view bookings"
  }
  ```

- **404 Not Found**:
  ```json
  {
    "error": "Resource not found",
    "message": "Field not found"
  }
  ```

---

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

---

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
    "paymentStatus": "Unpaid",
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
  "paymentUrl": "https://payment-gateway.com/pay/123",
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
- `timeSlots` are stored in `BookingTimeSlot` and support 30-minute increments.
- `services` are stored in `BookingService`.
- `promotionCode` applies a discount to the `totalPrice`, stored in `Promotion.Code`.
- Prices are calculated based on `FieldPricing.PricePerHour` and `FieldService.Price`.
- Returns a `paymentUrl` for unified payment processing, valid until `validUntil`.
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
  "totalPrice": 290000.0,
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
- `fieldName` and `subFieldName` are derived from `Booking.SubField.Field.FieldName` and `Booking.SubField.SubFieldName`.
- `paymentStatus` reflects `Booking.PaymentStatus` (Paid|Unpaid|Refunded).

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
- Updates `Booking.BookingDate`, `Booking.StartTime`, and `Booking.EndTime`.

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

---

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
  "paymentMethod": "CreditCard"
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
  "paymentUrl": "https://payment-gateway.com/pay/123",
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
- `paymentUrl` is generated by the payment gateway.

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
- For `Owner`, returns payments linked to their fields’ bookings.

### 7.4 Payment Webhook

**Description**: Handles payment updates from the payment gateway.

**HTTP Method**: POST  
**Endpoint**: `/api/payments/webhook`  
**Authorization**: None (secured by webhook signature)

**Request Body**:

```json
{
  "paymentId": 1,
  "status": "Completed",
  "transactionId": "txn_123",
  "amount": 500000,
  "timestamp": "2025-06-01T10:00:00Z"
}
```

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
- Secured by a signature verified by the payment gateway.

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

### 7.6 Process Refund

**Description**: Processes a refund request (Owner or Admin only).

**HTTP Method**: POST  
**Endpoint**: `/api/payments/refunds/{refundId}/process`  
**Authorization**: Bearer Token (Owner or Admin)

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
    "message": "Only the field owner or admin can process refunds"
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

---

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

### 8.4 Get Reviews By User

**Description**: Retrieves reviews made by the authenticated user.

**HTTP Method**: GET  
**Endpoint**: `/api/reviews`  
**Authorization**: Bearer Token (User)

**Query Parameters**:

- `page` (optional, integer, default: 1)
- `pageSize` (optional, integer, default: 10)

**Request Example**:

```http
GET /api/reviews?page=1&pageSize=10
Authorization: Bearer {token}
```

**Response**:

- **200 OK**:

  ```json
  {
    "data": [
      {
        "reviewId": 1,
        "fieldId": 1,
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

- **401 Unauthorized**:
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```

**Note**:

- Returns `Review` records for the authenticated user.

---

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

---

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

---

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

---

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
