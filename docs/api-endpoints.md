# API Endpoints - C4F-ISports v2.0.0

## Base URL

```
https://api.c4f-isports.com/v2
```

## Authentication

- Most endpoints require a **JWT Bearer Token** in the `Authorization` header: `Authorization: Bearer <token>`.
- Endpoints marked with `[Public]` are accessible without authentication.
- Endpoints marked with `[User]`, `[Owner]`, or `[Admin]` require specific roles.
- Authentication is handled via JWT Bearer Tokens.

## Error Responses

- **400 Bad Request**: Invalid input or request parameters.
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
- **401 Unauthorized**: Missing or invalid authentication token.
  ```json
  {
    "error": "Unauthorized",
    "message": "Invalid or missing token"
  }
  ```
- **403 Forbidden**: User lacks permission for the action.
  ```json
  {
    "error": "Forbidden",
    "message": "You do not have permission to perform this action"
  }
  ```
- **404 Not Found**: Resource not found.
  ```json
  {
    "error": "Resource not found",
    "message": "The requested resource does not exist"
  }
  ```
- **500 Internal Server Error**: Server-side issue.
  ```json
  {
    "error": "Internal server error",
    "message": "An unexpected error occurred"
  }
  ```

---

## 1. Authentication

### 1.1. Register

- **Method**: POST
- **Path**: `/api/auth/register`
- **Role**: [Public]
- **Description**: Registers a new user or owner account. Sends a verification email after successful registration.
- **Request Body**:
  ```json
  {
    "email": "string",
    "password": "string",
    "fullName": "string",
    "phone": "string",
    "role": "string", // "User" or "Owner"
    "city": "string", // Optional (for User)
    "district": "string", // Optional (for User)
    "description": "string" // Optional (for Owner)
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "accountId": 1,
      "email": "user@example.com",
      "role": "User",
      "message": "Account created. Please verify your email.",
      "verificationToken": "abc123"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "email",
          "message": "Email is already registered"
        }
      ]
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "email": "user@example.com",
    "password": "Password123!",
    "fullName": "Nguyen Van A",
    "phone": "0909123456",
    "role": "User",
    "city": "Hà Nội",
    "district": "Đống Đa"
  }

  // Response
  {
    "accountId": 1,
    "email": "user@example.com",
    "role": "User",
    "message": "Account created. Please verify your email.",
    "verificationToken": "abc123"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!","fullName":"Nguyen Van A","phone":"0909123456","role":"User","city":"Hà Nội","district":"Đống Đa"}'
  ```

---

### 1.2. Login

- **Method**: POST
- **Path**: `/api/auth/login`
- **Role**: [Public]
- **Description**: Authenticates a user or owner and returns a JWT and refresh token.
- **Request Body**:
  ```json
  {
    "email": "string",
    "password": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "accountId": 1,
      "email": "user@example.com",
      "role": "User",
      "token": "jwt-token",
      "refreshToken": "refresh-token",
      "expiresIn": 3600
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid email or password"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "Account not verified"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "email": "user@example.com",
    "password": "Password123!"
  }

  // Response
  {
    "accountId": 1,
    "email": "user@example.com",
    "role": "User",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "xyz456",
    "expiresIn": 3600
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!"}'
  ```

---

### 1.3. Refresh Token

- **Method**: POST
- **Path**: `/api/auth/refresh`
- **Role**: [Public]
- **Description**: Refreshes JWT using a valid refresh token.
- **Request Body**:
  ```json
  {
    "refreshToken": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "token": "new-jwt-token",
      "refreshToken": "new-refresh-token",
      "expiresIn": 3600
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or expired refresh token"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "refreshToken": "xyz456"
  }

  // Response
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new-xyz789",
    "expiresIn": 3600
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"xyz456"}'
  ```

---

### 1.4. Forgot Password

- **Method**: POST
- **Path**: `/api/auth/forgot-password`
- **Role**: [Public]
- **Description**: Sends a password reset link to the user's email.
- **Request Body**:
  ```json
  {
    "email": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Password reset link sent to your email"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Email not registered"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "email": "user@example.com"
  }

  // Response
  {
    "message": "Password reset link sent to your email"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'
  ```

---

### 1.5. Reset Password

- **Method**: POST
- **Path**: `/api/auth/reset-password`
- **Role**: [Public]
- **Description**: Resets the password using a reset token.
- **Request Body**:
  ```json
  {
    "resetToken": "string",
    "newPassword": "string"
  }
  ```
- **Response**:
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
      "message": "Invalid or expired reset token"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "resetToken": "abc123",
    "newPassword": "NewPassword123!"
  }

  // Response
  {
    "message": "Password reset successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"resetToken":"abc123","newPassword":"NewPassword123!"}'
  ```

---

### 1.6. Logout

- **Method**: POST
- **Path**: `/api/auth/logout`
- **Role**: [User, Owner, Admin]
- **Description**: Invalidates the refresh token to log out the user.
- **Request Body**:
  ```json
  {
    "refreshToken": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Logged out successfully"
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid refresh token"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "refreshToken": "xyz456"
  }

  // Response
  {
    "message": "Logged out successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/logout \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"refreshToken":"xyz456"}'
  ```

---

### 1.7. Get Current User

- **Method**: GET
- **Path**: `/api/auth/me`
- **Role**: [User, Owner, Admin]
- **Description**: Retrieves information about the authenticated user.
- **Response**:
  - **200 OK**:
    ```json
    {
      "accountId": 1,
      "userId": 1,
      "email": "user@example.com",
      "fullName": "Nguyen Van A",
      "role": "User"
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or missing token"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "accountId": 1,
    "userId": 1,
    "email": "user@example.com",
    "fullName": "Nguyen Van A",
    "role": "User"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/auth/me \
  -H "Authorization: Bearer <token>"
  ```

---

### 1.8. Change Password

- **Method**: POST
- **Path**: `/api/auth/change-password`
- **Role**: [User, Owner, Admin]
- **Description**: Changes the user's password.
- **Request Body**:
  ```json
  {
    "currentPassword": "string",
    "newPassword": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Password changed successfully"
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Incorrect current password"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "currentPassword": "Password123!",
    "newPassword": "NewPassword123!"
  }

  // Response
  {
    "message": "Password changed successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"currentPassword":"Password123!","newPassword":"NewPassword123!"}'
  ```

---

### 1.9. Verify Email

- **Method**: POST
- **Path**: `/api/auth/verify-email`
- **Role**: [Public]
- **Description**: Verifies the user's email using a verification token.
- **Request Body**:
  ```json
  {
    "verificationToken": "string"
  }
  ```
- **Response**:
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
      "message": "Invalid or expired verification token"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "verificationToken": "abc123"
  }

  // Response
  {
    "message": "Email verified successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/verify-email \
  -H "Content-Type: application/json" \
  -d '{"verificationToken":"abc123"}'
  ```

---

### 1.10. Resend Verification Email

- **Method**: POST
- **Path**: `/api/auth/resend-verification`
- **Role**: [Public]
- **Description**: Resends the email verification link.
- **Request Body**:
  ```json
  {
    "email": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Verification email resent"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Email not registered"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "email": "user@example.com"
  }

  // Response
  {
    "message": "Verification email resent"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/auth/resend-verification \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'
  ```

---

### 1.11. Verify Token

- **Method**: GET
- **Path**: `/api/auth/verify-token`
- **Role**: [User, Owner, Admin]
- **Description**: Checks if the JWT token is valid.
- **Response**:
  - **200 OK**:
    ```json
    {
      "isValid": true,
      "role": "User",
      "accountId": 1
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or expired token"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "isValid": true,
    "role": "User",
    "accountId": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/auth/verify-token \
  -H "Authorization: Bearer <token>"
  ```

---

## 2. User Management

### 2.1. Get Profile

- **Method**: GET
- **Path**: `/api/users/profile`
- **Role**: [User, Owner]
- **Description**: Retrieves the current user's profile information.
- **Response**:
  - **200 OK**:
    ```json
    {
      "userId": 1,
      "fullName": "Nguyen Van A",
      "email": "user@example.com",
      "phone": "0909123456",
      "city": "Hà Nội",
      "district": "Đống Đa",
      "avatarUrl": "https://cloudinary.com/avatar.jpg"
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or missing token"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "userId": 1,
    "fullName": "Nguyen Van A",
    "email": "user@example.com",
    "phone": "0909123456",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "avatarUrl": "https://cloudinary.com/avatar.jpg"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/users/profile \
  -H "Authorization: Bearer <token>"
  ```

---

### 2.2. Update Profile

- **Method**: PUT
- **Path**: `/api/users/profile`
- **Role**: [User, Owner]
- **Description**: Updates the current user's profile information.
- **Request Body**:
  ```json
  {
    "fullName": "string",
    "phone": "string",
    "city": "string",
    "district": "string",
    "avatarUrl": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Profile updated successfully",
      "user": {
        "userId": 1,
        "fullName": "Nguyen Van A",
        "email": "user@example.com",
        "phone": "0909123456"
      }
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "phone",
          "message": "Invalid phone number"
        }
      ]
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "fullName": "Nguyen Van B",
    "phone": "0987654321",
    "city": "Hà Nội",
    "district": "Cầu Giấy",
    "avatarUrl": "https://cloudinary.com/new-avatar.jpg"
  }

  // Response
  {
    "message": "Profile updated successfully",
    "user": {
      "userId": 1,
      "fullName": "Nguyen Van B",
      "email": "user@example.com",
      "phone": "0987654321"
    }
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/users/profile \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"fullName":"Nguyen Van B","phone":"0987654321","city":"Hà Nội","district":"Cầu Giấy","avatarUrl":"https://cloudinary.com/new-avatar.jpg"}'
  ```

---

### 2.3. Delete Profile

- **Method**: DELETE
- **Path**: `/api/users/profile`
- **Role**: [User, Owner]
- **Description**: Deletes the current user's account.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Account deleted successfully"
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or missing token"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Account deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/users/profile \
  -H "Authorization: Bearer <token>"
  ```

---

### 2.4. Get Booking History

- **Method**: GET
- **Path**: `/api/users/bookings`
- **Role**: [User]
- **Description**: Retrieves the user's booking history.
- **Query Parameters**:
  - `status`: Filter by status (e.g., "Confirmed", "Pending").
  - `sort`: Sort by field (e.g., "BookingDate:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "bookingId": 1,
          "fieldName": "ABC Field",
          "subFieldName": "Field A",
          "bookingDate": "2025-06-01",
          "startTime": "08:00:00",
          "endTime": "09:00:00",
          "totalPrice": 300000,
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "bookingId": 1,
        "fieldName": "ABC Field",
        "subFieldName": "Field A",
        "bookingDate": "2025-06-01",
        "startTime": "08:00:00",
        "endTime": "09:00:00",
        "totalPrice": 300000,
        "status": "Confirmed"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/users/bookings?status=Confirmed&sort=BookingDate:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 2.5. Get Favorite Fields

- **Method**: GET
- **Path**: `/api/users/favorites`
- **Role**: [User]
- **Description**: Retrieves the user's favorite fields.
- **Query Parameters**:
  - `sort`: Sort by field (e.g., "FieldName:asc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldId": 1,
          "fieldName": "ABC Field",
          "address": "123 Lang Street, Hanoi",
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "ABC Field",
        "address": "123 Lang Street, Hanoi",
        "averageRating": 4.5
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/users/favorites?sort=FieldName:asc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 2.6. Add Favorite Field

- **Method**: POST
- **Path**: `/api/users/favorites`
- **Role**: [User]
- **Description**: Adds a field to the user's favorite list.
- **Request Body**:
  ```json
  {
    "fieldId": 1
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "message": "Field added to favorites",
      "favoriteId": 1
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Field already in favorites"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field does not exist"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "fieldId": 1
  }

  // Response
  {
    "message": "Field added to favorites",
    "favoriteId": 1
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/users/favorites \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"fieldId":1}'
  ```

---

### 2.7. Remove Favorite Field

- **Method**: DELETE
- **Path**: `/api/users/favorites/{fieldId}`
- **Role**: [User]
- **Description**: Removes a field from the user's favorite list.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Field removed from favorites"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field not in favorites"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Field removed from favorites"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/users/favorites/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 2.8. Get User Reviews

- **Method**: GET
- **Path**: `/api/users/reviews`
- **Role**: [User]
- **Description**: Retrieves the reviews posted by the current user.
- **Query Parameters**:
  - `sort`: Sort by field (e.g., "CreatedAt:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "reviewId": 1,
          "fieldName": "ABC Field",
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "reviewId": 1,
        "fieldName": "ABC Field",
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
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/users/reviews?sort=CreatedAt:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

## 3. Field Management

### 3.1. Get Fields

- **Method**: GET
- **Path**: `/api/fields`
- **Role**: [Public]
- **Description**: Retrieves all fields with pagination, filtering, and sorting.
- **Query Parameters**:
  - `sportId`: Filter by sport.
  - `city`: Filter by city.
  - `district`: Filter by district.
  - `search`: Search by field name.
  - `sort`: Sort by field (e.g., "FieldName:asc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldId": 1,
          "fieldName": "ABC Field",
          "address": "123 Lang Street, Hanoi",
          "city": "Hà Nội",
          "district": "Đống Đa",
          "latitude": 10.776,
          "longitude": 106.7,
          "openTime": "06:00:00",
          "closeTime": "22:00:00",
          "averageRating": 4.5,
          "sportId": 1
        }
      ],
      "total": 1,
      "page": 1,
      "pageSize": 10
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "ABC Field",
        "address": "123 Lang Street, Hanoi",
        "city": "Hà Nội",
        "district": "Đống Đa",
        "latitude": 10.776,
        "longitude": 106.7,
        "openTime": "06:00:00",
        "closeTime": "22:00:00",
        "averageRating": 4.5,
        "sportId": 1
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields?sportId=1&city=Hà%20Nội&sort=FieldName:asc&page=1&pageSize=10
  ```

---

### 3.2 Create Field

- **Method**: POST
- **Path**: `/api/fields`
- **Role**: [Owner]
- **Description**: Creates a new field for the owner. The `latitude` and `longitude` are automatically determined using a geocoding service (e.g., Google Maps API) based on the provided `address`, `city`, and `district`.  
  _Note_: Backend must integrate a geocoding service to convert address to coordinates. Ensure error handling for invalid addresses. Response includes coordinates for owner confirmation.
- **Request Body**:
  ```json
  {
    "fieldName": "string",
    "address": "string",
    "city": "string",
    "district": "string",
    "openTime": "string",
    "closeTime": "string",
    "sportId": 0
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "fieldId": 1,
      "fieldName": "ABC Field",
      "latitude": 10.776,
      "longitude": 106.7,
      "message": "Field created successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "address",
          "message": "Could not determine coordinates for the provided address"
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
- **Example**:

  ```json
  // Request
  {
    "fieldName": "ABC Field",
    "address": "123 Lang Street",
    "city": "Hanoi",
    "district": "Cau Giay",
    "openTime": "08:00",
    "closeTime": "22:00",
    "sportId": 1
  }

  // Response
  {
    "fieldId": 1,
    "fieldName": "ABC Field",
    "latitude": 10.776,
    "longitude": 106.7,
    "message": "Field created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer <token>" \
    -d '{"fieldName":"ABC Field","address":"123 Lang Street","city":"Hanoi","district":"Cau Giay","openTime":"08:00","closeTime":"22:00","sportId":1}'
  ```

---

### 3.3. Get Field Details

- **Method**: GET
- **Path**: `/api/fields/{id}`
- **Role**: [Public]
- **Description**: Retrieves details of a specific field, including subfields, images, services, amenities, descriptions, and pricing.
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldId": 1,
      "fieldName": "ABC Field",
      "address": "123 Lang Street, Hanoi",
      "city": "Hà Nội",
      "district": "Đống Đa",
      "latitude": 10.776,
      "longitude": 106.7,
      "openTime": "06:00:00",
      "closeTime": "22:00:00",
      "averageRating": 4.5,
      "sportId": 1,
      "subFields": [
        {
          "subFieldId": 1,
          "subFieldName": "Field A",
          "fieldType": "5-a-side",
          "capacity": 10
        }
      ],
      "images": [
        {
          "fieldImageId": 1,
          "imageUrl": "https://cloudinary.com/field1.jpg"
        }
      ],
      "services": [
        {
          "fieldServiceId": 1,
          "serviceName": "Water Bottle",
          "price": 10000,
          "description": "500ml bottled water",
          "isActive": true
        }
      ],
      "amenities": [
        {
          "fieldAmenityId": 1,
          "amenityName": "Parking",
          "description": "Free parking for 50 cars",
          "iconUrl": "https://cloudinary.com/parking-icon.png"
        }
      ],
      "descriptions": [
        {
          "fieldDescriptionId": 1,
          "description": "Modern football field with artificial turf."
        }
      ],
      "pricing": [
        {
          "fieldPricingId": 1,
          "subFieldId": 1,
          "startTime": "08:00:00",
          "endTime": "09:00:00",
          "dayOfWeek": 1,
          "price": 300000,
          "isActive": true
        }
      ]
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "fieldId": 1,
    "fieldName": "ABC Field",
    "address": "123 Lang Street, Hanoi",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "latitude": 10.776,
    "longitude": 106.7,
    "openTime": "06:00:00",
    "closeTime": "22:00:00",
    "averageRating": 4.5,
    "sportId": 1,
    "subFields": [
      {
        "subFieldId": 1,
        "subFieldName": "Field A",
        "fieldType": "5-a-side",
        "capacity": 10
      }
    ],
    "images": [
      {
        "fieldImageId": 1,
        "imageUrl": "https://cloudinary.com/field1.jpg"
      }
    ],
    "services": [
      {
        "fieldServiceId": 1,
        "serviceName": "Water Bottle",
        "price": 10000,
        "description": "500ml bottled water",
        "isActive": true
      }
    ],
    "amenities": [
      {
        "fieldAmenityId": 1,
        "amenityName": "Parking",
        "description": "Free parking for 50 cars",
        "iconUrl": "https://cloudinary.com/parking-icon.png"
      }
    ],
    "descriptions": [
      {
        "fieldDescriptionId": 1,
        "description": "Modern football field with artificial turf."
      }
    ],
    "pricing": [
      {
        "fieldPricingId": 1,
        "subFieldId": 1,
        "startTime": "08:00:00",
        "endTime": "09:00:00",
        "dayOfWeek": 1,
        "price": 300000,
        "isActive": true
      }
    ]
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/1
  ```

---

### 3.4 Update Field

- **Method**: PUT
- **Path**: `/api/fields/{id}`
- **Role**: [Owner]
- **Description**: Updates an existing field. The `latitude` and `longitude` are automatically updated using a geocoding service if the `address`, `city`, or `district` is changed.  
  _Note_: Backend should check if address fields are modified before calling geocoding API to optimize performance. Response includes updated coordinates for confirmation.
- **Request Body**:
  ```json
  {
    "fieldName": "string",
    "address": "string",
    "city": "string",
    "district": "string",
    "openTime": "string",
    "closeTime": "string",
    "sportId": 0
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldId": 1,
      "fieldName": "Updated ABC Field",
      "latitude": 10.776,
      "longitude": 106.7,
      "message": "Field updated successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "address",
          "message": "Could not determine coordinates for the provided address"
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
      "message": "You do not have permission to update this field"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field does not exist"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "fieldName": "Updated ABC Field",
    "address": "123 Lang Street",
    "city": "Hanoi",
    "district": "Cau Giay",
    "openTime": "08:00",
    "closeTime": "22:00",
    "sportId": 1
  }

  // Response
  {
    "fieldId": 1,
    "fieldName": "Updated ABC Field",
    "latitude": 10.776,
    "longitude": 106.7,
    "message": "Field updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/fields/1 \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer <token>" \
    -d '{"fieldName":"Updated ABC Field","address":"123 Lang Street","city":"Hanoi","district":"Cau Giay","openTime":"08:00","closeTime":"22:00","sportId":1}'
  ```

---

### 3.5 Delete Field

- **Method**: DELETE
- **Path**: `/api/fields/{id}`
- **Role**: [Owner]
- **Description**: Marks a field as deleted (soft delete). The field will no longer be visible to users but remains in the database for historical purposes. Checks for active bookings before deletion.  
  _Note_: Backend must add `Status` field to `Field` model for soft delete. Ensure no confirmed or pending bookings exist before deletion. The `status` field in response indicates the field’s new state.
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldId": 1,
      "status": "Inactive",
      "message": "Field deleted successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Cannot delete field with active bookings"
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
      "message": "You do not have permission to delete this field"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "fieldId": 1,
    "status": "Inactive",
    "message": "Field deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/fields/1 \
    -H "Authorization: Bearer <token>"
  ```

---

### 3.6 Get Field Availability

- **Method**: GET
- **Path**: `/api/fields/availability`
- **Role**: [User]
- **Description**: Retrieves available time slots for fields based on filters. Includes promotion information if applicable.  
  _Note_: Backend should filter slots with duration ≥ specified `duration`. Include `promotion` in response for accurate pricing.
- **Query Parameters**:
  - `fieldId`: Filter by field ID (optional).
  - `subFieldId`: Filter by subfield ID (optional).
  - `sportId`: Filter by sport ID (optional).
  - `city`: Filter by city (optional).
  - `district`: Filter by district (optional).
  - `date`: Filter by date (format: YYYY-MM-DD, optional).
  - `startTime`: Filter by start time (format: HH:MM:SS, optional).
  - `endTime`: Filter by end time (format: HH:MM:SS, optional).
  - `duration`: Minimum duration of available slots in minutes (e.g., 60, optional).
  - `page`: Page number for pagination (default: 1).
  - `pageSize`: Number of items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldId": 1,
          "fieldName": "ABC Field",
          "subFieldId": 1,
          "subFieldName": "Field A",
          "date": "2025-05-21",
          "startTime": "08:00:00",
          "endTime": "09:00:00",
          "price": 300000,
          "promotion": {
            "promotionId": 1,
            "promotionCode": "SUMMER2025",
            "discountValue": 20,
            "discountType": "Percentage"
          }
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
          "field": "date",
          "message": "Invalid date or time format"
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "ABC Field",
        "subFieldId": 1,
        "subFieldName": "Field A",
        "date": "2025-05-21",
        "startTime": "08:00:00",
        "endTime": "09:00:00",
        "price": 300000,
        "promotion": {
          "promotionId": 1,
          "promotionCode": "SUMMER2025",
          "discountValue": 20,
          "discountType": "Percentage"
        }
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/availability?fieldId=1&date=2025-05-21&duration=60&page=1&pageSize=10 \
    -H "Authorization: Bearer <token>"
  ```

---

### 3.7. Create SubField

- **Method**: POST
- **Path**: `/api/fields/{id}/subfields`
- **Role**: [Owner]
- **Description**: Creates a new subfield for a field.
- **Request Body**:
  ```json
  {
    "subFieldName": "string",
    "fieldType": "string",
    "capacity": 0
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "subFieldId": 1,
      "subFieldName": "Field A",
      "message": "SubField created successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "subFieldName",
          "message": "SubField name is required"
        }
      ]
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "subFieldName": "Field A",
    "fieldType": "5-a-side",
    "capacity": 10
  }

  // Response
  {
    "subFieldId": 1,
    "subFieldName": "Field A",
    "message": "SubField created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields/1/subfields \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"subFieldName":"Field A","fieldType":"5-a-side","capacity":10}'
  ```

---

### 3.8. Update SubField

- **Method**: PUT
- **Path**: `/api/fields/{id}/subfields/{subFieldId}`
- **Role**: [Owner]
- **Description**: Updates an existing subfield.
- **Request Body**:
  ```json
  {
    "subFieldName": "string",
    "fieldType": "string",
    "capacity": 0
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "subFieldId": 1,
      "subFieldName": "Updated Field A",
      "message": "SubField updated successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "subFieldName",
          "message": "SubField name is required"
        }
      ]
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "SubField does not exist"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "subFieldName": "Updated Field A",
    "fieldType": "5-a-side",
    "capacity": 10
  }

  // Response
  {
    "subFieldId": 1,
    "subFieldName": "Updated Field A",
    "message": "SubField updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/fields/1/subfields/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"subFieldName":"Updated Field A","fieldType":"5-a-side","capacity":10}'
  ```

---

### 3.9 Delete SubField

- **Method**: DELETE
- **Path**: `/api/subfields/{id}`
- **Role**: [Owner]
- **Description**: Marks a subfield as deleted (soft delete). The subfield will no longer be available for bookings. Checks for active bookings before deletion.  
  _Note_: Backend must add `Status` field to `SubField` model for soft delete. Ensure no confirmed or pending bookings exist before deletion. The `status` field in response indicates the subfield’s new state.
- **Response**:
  - **200 OK**:
    ```json
    {
      "subFieldId": 1,
      "status": "Inactive",
      "message": "Subfield deleted successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Cannot delete subfield with active bookings"
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
      "message": "You do not have permission to delete this subfield"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Subfield does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "subFieldId": 1,
    "status": "Inactive",
    "message": "Subfield deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/subfields/1 \
    -H "Authorization: Bearer <token>"
  ```

---

### 3.10. Upload Field Image

- **Method**: POST
- **Path**: `/api/fields/{id}/images`
- **Role**: [Owner]
- **Description**: Uploads an image for a field.
- **Request Body**: Form-data with file (image).
- **Response**:
  - **201 Created**:
    ```json
    {
      "fieldImageId": 1,
      "imageUrl": "https://cloudinary.com/field1.jpg",
      "message": "Image uploaded successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Image file is required"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "fieldImageId": 1,
    "imageUrl": "https://cloudinary.com/field1.jpg",
    "message": "Image uploaded successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields/1/images \
  -H "Authorization: Bearer <token>" \
  -F "image=@/path/to/image.jpg"
  ```

---

### 3.11. Delete Field Image

- **Method**: DELETE
- **Path**: `/api/fields/{id}/images/{imageId}`
- **Role**: [Owner]
- **Description**: Deletes a field image.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Image deleted successfully"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Image does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Image deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/fields/1/images/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 3.12. Get Field Reviews

- **Method**: GET
- **Path**: `/api/fields/{id}/reviews`
- **Role**: [Public]
- **Description**: Retrieves reviews for a specific field with pagination.
- **Query Parameters**:
  - `sort`: Sort by field (e.g., "Rating:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "reviewId": 1,
          "userId": 1,
          "username": "John Doe",
          "rating": 5,
          "comment": "Great field!",
          "createdAt": "2025-05-21T10:00:00Z"
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
      "message": "Field does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "reviewId": 1,
        "userId": 1,
        "username": "John Doe",
        "rating": 5,
        "comment": "Great field!",
        "createdAt": "2025-05-21T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/1/reviews?sort=Rating:desc&page=1&pageSize=10
  ```

---

### 3.13. Get Field Services

- **Method**: GET
- **Path**: `/api/fields/{id}/services`
- **Role**: [Public]
- **Description**: Retrieves all services of a specific field.
- **Query Parameters**:
  - `isActive`: Filter by active status (true/false).
  - `sort`: Sort by field (e.g., "ServiceName:asc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldServiceId": 1,
          "serviceName": "Water Bottle",
          "price": 10000,
          "description": "500ml bottled water",
          "isActive": true
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
      "message": "Field does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldServiceId": 1,
        "serviceName": "Water Bottle",
        "price": 10000,
        "description": "500ml bottled water",
        "isActive": true
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/1/services?isActive=true&sort=ServiceName:asc&page=1&pageSize=10
  ```

---

### 3.14. Create Field Service

- **Method**: POST
- **Path**: `/api/fields/{id}/services`
- **Role**: [Owner]
- **Description**: Creates a new service for a field.
- **Request Body**:
  ```json
  {
    "serviceName": "string",
    "price": 10000,
    "description": "string",
    "isActive": true
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "fieldServiceId": 1,
      "serviceName": "Water Bottle",
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
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "serviceName": "Water Bottle",
    "price": 10000,
    "description": "500ml bottled water",
    "isActive": true
  }

  // Response
  {
    "fieldServiceId": 1,
    "serviceName": "Water Bottle",
    "message": "Service created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields/1/services \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"serviceName":"Water Bottle","price":10000,"description":"500ml bottled water","isActive":true}'
  ```

---

### 3.15. Update Field Service

- **Method**: PUT
- **Path**: `/api/fields/{id}/services/{serviceId}`
- **Role**: [Owner]
- **Description**: Updates an existing field service.
- **Request Body**:
  ```json
  {
    "serviceName": "string",
    "price": 12000,
    "description": "string",
    "isActive": true
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldServiceId": 1,
      "serviceName": "Water Bottle",
      "message": "Service updated successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Service does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "serviceName": "Water Bottle",
    "price": 12000,
    "description": "500ml bottled water",
    "isActive": true
  }

  // Response
  {
    "fieldServiceId": 1,
    "serviceName": "Water Bottle",
    "message": "Service updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/fields/1/services/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"serviceName":"Water Bottle","price":12000,"description":"500ml bottled water","isActive":true}'
  ```

---

### 3.16. Delete Field Service

- **Method**: DELETE
- **Path**: `/api/fields/{id}/services/{serviceId}`
- **Role**: [Owner]
- **Description**: Deletes a field service.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Service deleted successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Service does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Service deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/fields/1/services/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 3.17. Get Field Amenities

- **Method**: GET
- **Path**: `/api/fields/{id}/amenities`
- **Role**: [Public]
- **Description**: Retrieves all amenities of a specific field.
- **Query Parameters**:
  - `sort`: Sort by field (e.g., "AmenityName:asc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldAmenityId": 1,
          "amenityName": "Parking",
          "description": "Free parking for 50 cars",
          "iconUrl": "https://cloudinary.com/parking-icon.png"
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
      "message": "Field does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldAmenityId": 1,
        "amenityName": "Parking",
        "description": "Free parking for 50 cars",
        "iconUrl": "https://cloudinary.com/parking-icon.png"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/1/amenities?sort=AmenityName:asc&page=1&pageSize=10
  ```

---

### 3.18. Create Field Amenity

- **Method**: POST
- **Path**: `/api/fields/{id}/amenities`
- **Role**: [Owner]
- **Description**: Creates a new amenity for a field.
- **Request Body**:
  ```json
  {
    "amenityName": "string",
    "description": "string",
    "iconUrl": "string"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "fieldAmenityId": 1,
      "amenityName": "Parking",
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
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "amenityName": "Parking",
    "description": "Free parking for 50 cars",
    "iconUrl": "https://cloudinary.com/parking-icon.png"
  }

  // Response
  {
    "fieldAmenityId": 1,
    "amenityName": "Parking",
    "message": "Amenity created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields/1/amenities \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"amenityName":"Parking","description":"Free parking for 50 cars","iconUrl":"https://cloudinary.com/parking-icon.png"}'
  ```

---

### 3.19. Update Field Amenity

- **Method**: PUT
- **Path**: `/api/fields/{id}/amenities/{amenityId}`
- **Role**: [Owner]
- **Description**: Updates an existing field amenity.
- **Request Body**:
  ```json
  {
    "amenityName": "string",
    "description": "string",
    "iconUrl": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldAmenityId": 1,
      "amenityName": "Parking",
      "message": "Amenity updated successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Amenity does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "amenityName": "Parking",
    "description": "Free parking for 50 cars",
    "iconUrl": "https://cloudinary.com/parking-icon.png"
  }

  // Response
  {
    "fieldAmenityId": 1,
    "amenityName": "Parking",
    "message": "Amenity updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/fields/1/amenities/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"amenityName":"Parking","description":"Free parking for 50 cars","iconUrl":"https://cloudinary.com/parking-icon.png"}'
  ```

---

### 3.20. Delete Field Amenity

- **Method**: DELETE
- **Path**: `/api/fields/{id}/amenities/{amenityId}`
- **Role**: [Owner]
- **Description**: Deletes a field amenity.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Amenity deleted successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Amenity does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Amenity deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/fields/1/amenities/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 3.21. Get Field Descriptions

- **Method**: GET
- **Path**: `/api/fields/{id}/descriptions`
- **Role**: [Public]
- **Description**: Retrieves all descriptions of a specific field.
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldDescriptionId": 1,
          "description": "Modern football field with artificial turf."
        }
      ],
      "total": 1
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldDescriptionId": 1,
        "description": "Modern football field with artificial turf."
      }
    ],
    "total": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/1/descriptions
  ```

---

### 3.22. Create Field Description

- **Method**: POST
- **Path**: `/api/fields/{id}/descriptions`
- **Role**: [Owner]
- **Description**: Creates a new description for a field.
- **Request Body**:
  ```json
  {
    "description": "string"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "fieldDescriptionId": 1,
      "description": "Modern football field with artificial turf.",
      "message": "Description created successfully"
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
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "description": "Modern football field with artificial turf."
  }

  // Response
  {
    "fieldDescriptionId": 1,
    "description": "Modern football field with artificial turf.",
    "message": "Description created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields/1/descriptions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"description":"Modern football field with artificial turf."}'
  ```

---

### 3.23. Update Field Description

- **Method**: PUT
- **Path**: `/api/fields/{id}/descriptions/{descriptionId}`
- **Role**: [Owner]
- **Description**: Updates an existing field description.
- **Request Body**:
  ```json
  {
    "description": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldDescriptionId": 1,
      "description": "Updated football field description.",
      "message": "Description updated successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Description does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "description": "Updated football field description."
  }

  // Response
  {
    "fieldDescriptionId": 1,
    "description": "Updated football field description.",
    "message": "Description updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/fields/1/descriptions/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"description":"Updated football field description."}'
  ```

---

### 3.24. Delete Field Description

- **Method**: DELETE
- **Path**: `/api/fields/{id}/descriptions/{descriptionId}`
- **Role**: [Owner]
- **Description**: Deletes a field description.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Description deleted successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Description does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Description deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/fields/1/descriptions/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 3.25. Get Field Pricing

- **Method**: GET
- **Path**: `/api/fields/{id}/pricing`
- **Role**: [Public]
- **Description**: Retrieves all pricing rules for a field’s subfields.
- **Query Parameters**:
  - `subFieldId`: Filter by subfield.
  - `isActive`: Filter by active status (true/false).
  - `sort`: Sort by field (e.g., "Price:asc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldPricingId": 1,
          "subFieldId": 1,
          "startTime": "08:00:00",
          "endTime": "09:00:00",
          "dayOfWeek": 1,
          "price": 300000,
          "isActive": true
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
      "message": "Field does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldPricingId": 1,
        "subFieldId": 1,
        "startTime": "08:00:00",
        "endTime": "09:00:00",
        "dayOfWeek": 1,
        "price": 300000,
        "isActive": true
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/1/pricing?subFieldId=1&isActive=true&sort=Price:asc&page=1&pageSize=10
  ```

---

### 3.26. Create Field Pricing

- **Method**: POST
- **Path**: `/api/fields/{id}/pricing`
- **Role**: [Owner]
- **Description**: Creates a new pricing rule for a subfield.
- **Request Body**:
  ```json
  {
    "subFieldId": 1,
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "dayOfWeek": 1,
    "price": 300000,
    "isActive": true
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "fieldPricingId": 1,
      "subFieldId": 1,
      "message": "Pricing created successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "price",
          "message": "Price must be greater than 0"
        }
      ]
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "subFieldId": 1,
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "dayOfWeek": 1,
    "price": 300000,
    "isActive": true
  }

  // Response
  {
    "fieldPricingId": 1,
    "subFieldId": 1,
    "message": "Pricing created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields/1/pricing \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"subFieldId":1,"startTime":"08:00:00","endTime":"09:00:00","dayOfWeek":1,"price":300000,"isActive":true}'
  ```

---

### 3.27. Update Field Pricing

- **Method**: PUT
- **Path**: `/api/fields/{id}/pricing/{pricingId}`
- **Role**: [Owner]
- **Description**: Updates an existing pricing rule.
- **Request Body**:
  ```json
  {
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "dayOfWeek": 1,
    "price": 350000,
    "isActive": true
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldPricingId": 1,
      "subFieldId": 1,
      "message": "Pricing updated successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Pricing rule does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "dayOfWeek": 1,
    "price": 350000,
    "isActive": true
  }

  // Response
  {
    "fieldPricingId": 1,
    "subFieldId": 1,
    "message": "Pricing updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/fields/1/pricing/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"startTime":"08:00:00","endTime":"09:00:00","dayOfWeek":1,"price":350000,"isActive":true}'
  ```

---

### 3.28. Delete Field Pricing

- **Method**: DELETE
- **Path**: `/api/fields/{id}/pricing/{pricingId}`
- **Role**: [Owner]
- **Description**: Deletes a pricing rule.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Pricing deleted successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Pricing rule does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not own this field"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Pricing deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/fields/1/pricing/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 3.29 Validate Address

- **Method**: POST
- **Path**: `/api/fields/validate-address`
- **Role**: [Owner]
- **Description**: Validates an address and returns its coordinates using a geocoding service.  
  _Note_: Backend must integrate a geocoding service (e.g., Google Maps API) to validate address and return formatted address and coordinates. Useful for owners before creating/updating fields.
- **Request Body**:
  ```json
  {
    "address": "string",
    "city": "string",
    "district": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "latitude": 10.776,
      "longitude": 106.7,
      "formattedAddress": "123 Lang Street, Hanoi",
      "isValid": true
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "address",
          "message": "Could not validate the provided address"
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
- **Example**:

  ```json
  // Request
  {
    "address": "123 Lang Street",
    "city": "Hanoi",
    "district": "Cau Giay"
  }

  // Response
  {
    "latitude": 10.776,
    "longitude": 106.7,
    "formattedAddress": "123 Lang Street, Hanoi",
    "isValid": true
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields/validate-address \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer <token>" \
    -d '{"address":"123 Lang Street","city":"Hanoi","district":"Cau Giay"}'
  ```

---

## 4. Booking Management

### 4.1. Create Booking

- **Method**: POST
- **Path**: `/api/bookings`
- **Role**: [User]
- **Description**: Creates a new booking for a subfield.
- **Request Body**:
  ```json
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "services": [
      {
        "fieldServiceId": 1,
        "quantity": 2
      }
    ],
    "promotionCode": "string"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "bookingId": 1,
      "subFieldId": 1,
      "totalPrice": 300000,
      "status": "Pending",
      "message": "Booking created successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Time slot is not available"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "services": [
      {
        "fieldServiceId": 1,
        "quantity": 2
      }
    ],
    "promotionCode": "SUMMER2025"
  }

  // Response
  {
    "bookingId": 1,
    "subFieldId": 1,
    "totalPrice": 300000,
    "status": "Pending",
    "message": "Booking created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/bookings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"subFieldId":1,"bookingDate":"2025-06-01","startTime":"08:00:00","endTime":"09:00:00","services":[{"fieldServiceId":1,"quantity":2}],"promotionCode":"SUMMER2025"}'
  ```

---

### 4.2. Get Bookings

- **Method**: GET
- **Path**: `/api/bookings`
- **Role**: [User, Owner]
- **Description**: Retrieves bookings (User: own bookings; Owner: bookings for their fields).
- **Query Parameters**:
  - `status`: Filter by status (e.g., "Confirmed").
  - `sort`: Sort by field (e.g., "BookingDate:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "bookingId": 1,
          "fieldName": "ABC Field",
          "subFieldName": "Field A",
          "bookingDate": "2025-06-01",
          "startTime": "08:00:00",
          "endTime": "09:00:00",
          "totalPrice": 300000,
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "bookingId": 1,
        "fieldName": "ABC Field",
        "subFieldName": "Field A",
        "bookingDate": "2025-06-01",
        "startTime": "08:00:00",
        "endTime": "09:00:00",
        "totalPrice": 300000,
        "status": "Confirmed"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/bookings?status=Confirmed&sort=BookingDate:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 4.3. Get Booking Details

- **Method**: GET
- **Path**: `/api/bookings/{id}`
- **Role**: [User, Owner]
- **Description**: Retrieves details of a specific booking.
- **Response**:
  - **200 OK**:
    ```json
    {
      "bookingId": 1,
      "fieldName": "ABC Field",
      "subFieldName": "Field A",
      "bookingDate": "2025-06-01",
      "startTime": "08:00:00",
      "endTime": "09:00:00",
      "totalPrice": 300000,
      "status": "Confirmed",
      "services": [
        {
          "fieldServiceId": 1,
          "serviceName": "Water Bottle",
          "quantity": 2,
          "price": 10000
        }
      ],
      "promotionCode": "SUMMER2025",
      "promotionDiscount": 50000
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Booking does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to view this booking"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "bookingId": 1,
    "fieldName": "ABC Field",
    "subFieldName": "Field A",
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "totalPrice": 300000,
    "status": "Confirmed",
    "services": [
      {
        "fieldServiceId": 1,
        "serviceName": "Water Bottle",
        "quantity": 2,
        "price": 10000
      }
    ],
    "promotionCode": "SUMMER2025",
    "promotionDiscount": 50000
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/bookings/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 4.4. Update Booking

- **Method**: PUT
- **Path**: `/api/bookings/{id}`
- **Role**: [User, Owner]
- **Description**: Updates a booking's details (e.g., services or promotion code). Users can update their own bookings; Owners can update bookings for their fields.
- **Request Body**:
  ```json
  {
    "services": [
      {
        "fieldServiceId": 1,
        "quantity": 3
      }
    ],
    "promotionCode": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "bookingId": 1,
      "totalPrice": 320000,
      "message": "Booking updated successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid promotion code"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Booking does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to update this booking"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "services": [
      {
        "fieldServiceId": 1,
        "quantity": 3
      }
    ],
    "promotionCode": "SUMMER2025"
  }

  // Response
  {
    "bookingId": 1,
    "totalPrice": 320000,
    "message": "Booking updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/bookings/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"services":[{"fieldServiceId":1,"quantity":3}],"promotionCode":"SUMMER2025"}'
  ```

---

### 4.5. Cancel Booking

- **Method**: DELETE
- **Path**: `/api/bookings/{id}`
- **Role**: [User, Owner]
- **Description**: Cancels a booking.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Booking cancelled successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Booking does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to cancel this booking"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Booking cancelled successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/bookings/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 4.6. Update Booking Status

- **Method**: PUT
- **Path**: `/api/bookings/{id}/status`
- **Role**: [Owner]
- **Description**: Updates the status of a booking (e.g., "Confirmed", "Cancelled").
- **Request Body**:
  ```json
  {
    "status": "string" // e.g., "Confirmed", "Cancelled"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "bookingId": 1,
      "status": "Confirmed",
      "message": "Booking status updated successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid status"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Booking does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to update this booking"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "status": "Confirmed"
  }

  // Response
  {
    "bookingId": 1,
    "status": "Confirmed",
    "message": "Booking status updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/bookings/1/status \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"status":"Confirmed"}'
  ```

---

### 4.7. Create Simple Booking

- **Method**: POST
- **Path**: `/api/bookings/simple`
- **Role**: [User]
- **Description**: Creates a booking with minimal input (no services or promotion).
- **Request Body**:
  ```json
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "bookingId": 1,
      "subFieldId": 1,
      "totalPrice": 300000,
      "status": "Pending",
      "message": "Booking created successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Time slot is not available"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00"
  }

  // Response
  {
    "bookingId": 1,
    "subFieldId": 1,
    "totalPrice": 300000,
    "status": "Pending",
    "message": "Booking created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/bookings/simple \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"subFieldId":1,"bookingDate":"2025-06-01","startTime":"08:00:00","endTime":"09:00:00"}'
  ```

---

### 4.8. Get Booking Services

- **Method**: GET
- **Path**: `/api/bookings/{id}/services`
- **Role**: [User, Owner]
- **Description**: Retrieves all services booked for a specific booking.
- **Response**:
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
      "total": 1
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Booking does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to view this booking"
    }
    ```
- **Example**:
  ```json
  // Response
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
    "total": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/bookings/1/services \
  -H "Authorization: Bearer <token>"
  ```

---

### 4.9. Preview Booking

- **Method**: POST
- **Path**: `/api/bookings/preview`
- **Role**: [User]
- **Description**: Previews the cost and availability of a booking without creating it.
- **Request Body**:
  ```json
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "services": [
      {
        "fieldServiceId": 1,
        "quantity": 2
      }
    ],
    "promotionCode": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "subFieldId": 1,
      "available": true,
      "basePrice": 300000,
      "servicePrice": 20000,
      "promotionDiscount": 50000,
      "totalPrice": 270000
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Time slot is not available"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "services": [
      {
        "fieldServiceId": 1,
        "quantity": 2
      }
    ],
    "promotionCode": "SUMMER2025"
  }

  // Response
  {
    "subFieldId": 1,
    "available": true,
    "basePrice": 300000,
    "servicePrice": 20000,
    "promotionDiscount": 50000,
    "totalPrice": 270000
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/bookings/preview \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"subFieldId":1,"bookingDate":"2025-06-01","startTime":"08:00:00","endTime":"09:00:00","services":[{"fieldServiceId":1,"quantity":2}],"promotionCode":"SUMMER2025"}'
  ```

---

### 4.10. Get Booking Invoice

- **Method**: GET
- **Path**: `/api/bookings/{id}/invoice`
- **Role**: [User, Owner]
- **Description**: Retrieves the invoice for a booking.
- **Response**:
  - **200 OK**:
    ```json
    {
      "bookingId": 1,
      "fieldName": "ABC Field",
      "subFieldName": "Field A",
      "bookingDate": "2025-06-01",
      "startTime": "08:00:00",
      "endTime": "09:00:00",
      "basePrice": 300000,
      "services": [
        {
          "serviceName": "Water Bottle",
          "quantity": 2,
          "price": 10000
        }
      ],
      "servicePrice": 20000,
      "promotionCode": "SUMMER2025",
      "promotionDiscount": 50000,
      "totalPrice": 270000,
      "paymentStatus": "Paid"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Booking does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to view this booking"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "bookingId": 1,
    "fieldName": "ABC Field",
    "subFieldName": "Field A",
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "basePrice": 300000,
    "services": [
      {
        "serviceName": "Water Bottle",
        "quantity": 2,
        "price": 10000
      }
    ],
    "servicePrice": 20000,
    "promotionCode": "SUMMER2025",
    "promotionDiscount": 50000,
    "totalPrice": 270000,
    "paymentStatus": "Paid"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/bookings/1/invoice \
  -H "Authorization: Bearer <token>"
  ```

---

### 4.11. Reschedule Booking

- **Method**: POST
- **Path**: `/api/bookings/{id}/reschedule`
- **Role**: [User]
- **Description**: Submits a request to reschedule a booking to a new date and time.
- **Request Body**:
  ```json
  {
    "newDate": "2025-06-02",
    "newStartTime": "09:00:00",
    "newEndTime": "10:00:00"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "rescheduleRequestId": 1,
      "bookingId": 1,
      "status": "Pending",
      "message": "Reschedule request submitted successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "New time slot is not available"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Booking does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to reschedule this booking"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "newDate": "2025-06-02",
    "newStartTime": "09:00:00",
    "newEndTime": "10:00:00"
  }

  // Response
  {
    "rescheduleRequestId": 1,
    "bookingId": 1,
    "status": "Pending",
    "message": "Reschedule request submitted successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/bookings/1/reschedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"newDate":"2025-06-02","newStartTime":"09:00:00","newEndTime":"10:00:00"}'
  ```

---

## 5. Payment Processing

### 5.1. Create Payment

- **Method**: POST
- **Path**: `/api/payments`
- **Role**: [User]
- **Description**: Initiates a payment for a booking.
- **Request Body**:
  ```json
  {
    "bookingId": 1,
    "paymentMethod": "string", // e.g., "CreditCard", "BankTransfer"
    "amount": 270000
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "paymentId": 1,
      "bookingId": 1,
      "amount": 270000,
      "status": "Pending",
      "paymentUrl": "https://payment-gateway.com/pay/xyz123",
      "message": "Payment initiated successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid booking or amount"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Booking does not exist"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "bookingId": 1,
    "paymentMethod": "CreditCard",
    "amount": 270000
  }

  // Response
  {
    "paymentId": 1,
    "bookingId": 1,
    "amount": 270000,
    "status": "Pending",
    "paymentUrl": "https://payment-gateway.com/pay/xyz123",
    "message": "Payment initiated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/payments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"bookingId":1,"paymentMethod":"CreditCard","amount":270000}'
  ```

---

### 5.2. Get Payments

- **Method**: GET
- **Path**: `/api/payments`
- **Role**: [User, Owner]
- **Description**: Retrieves payments (User: own payments; Owner: payments for their fields).
- **Query Parameters**:
  - `status`: Filter by status (e.g., "Paid").
  - `sort`: Sort by field (e.g., "CreatedAt:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "paymentId": 1,
          "bookingId": 1,
          "amount": 270000,
          "paymentMethod": "CreditCard",
          "status": "Paid",
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "paymentId": 1,
        "bookingId": 1,
        "amount": 270000,
        "paymentMethod": "CreditCard",
        "status": "Paid",
        "createdAt": "2025-06-01T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/payments?status=Paid&sort=CreatedAt:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 5.3. Get Payment Details

- **Method**: GET
- **Path**: `/api/payments/{id}`
- **Role**: [User, Owner]
- **Description**: Retrieves details of a specific payment.
- **Response**:
  - **200 OK**:
    ```json
    {
      "paymentId": 1,
      "bookingId": 1,
      "amount": 270000,
      "paymentMethod": "CreditCard",
      "status": "Paid",
      "createdAt": "2025-06-01T10:00:00Z",
      "transactionId": "xyz123"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Payment does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to view this payment"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "paymentId": 1,
    "bookingId": 1,
    "amount": 270000,
    "paymentMethod": "CreditCard",
    "status": "Paid",
    "createdAt": "2025-06-01T10:00:00Z",
    "transactionId": "xyz123"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/payments/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 5.4. Payment Webhook

- **Method**: POST
- **Path**: `/api/payments/webhook`
- **Role**: [Public]
- **Description**: Handles payment status updates from the payment gateway.
- **Request Body**:
  ```json
  {
    "paymentId": 1,
    "transactionId": "string",
    "status": "string", // e.g., "Paid", "Failed"
    "amount": 270000,
    "timestamp": "2025-06-01T10:00:00Z"
  }
  ```
- **Response**:
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
      "message": "Invalid webhook data"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "paymentId": 1,
    "transactionId": "xyz123",
    "status": "Paid",
    "amount": 270000,
    "timestamp": "2025-06-01T10:00:00Z"
  }

  // Response
  {
    "message": "Webhook processed successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/payments/webhook \
  -H "Content-Type: application/json" \
  -d '{"paymentId":1,"transactionId":"xyz123","status":"Paid","amount":270000,"timestamp":"2025-06-01T10:00:00Z"}'
  ```

---

### 5.5. Request Refund

- **Method**: POST
- **Path**: `/api/payments/{id}/refund`
- **Role**: [User]
- **Description**: Submits a refund request for a payment.
- **Request Body**:
  ```json
  {
    "amount": 270000,
    "reason": "string"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "refundRequestId": 1,
      "paymentId": 1,
      "amount": 270000,
      "status": "Pending",
      "message": "Refund request submitted successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid amount or reason"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Payment does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to request a refund for this payment"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "amount": 270000,
    "reason": "Booking cancelled due to weather"
  }

  // Response
  {
    "refundRequestId": 1,
    "paymentId": 1,
    "amount": 270000,
    "status": "Pending",
    "message": "Refund request submitted successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/payments/1/refund \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"amount":270000,"reason":"Booking cancelled due to weather"}'
  ```

---

### 5.6 Get Refund History

- **Method**: GET
- **Path**: `/api/payments/refunds`
- **Role**: [User, Owner]
- **Description**: Retrieves refund history for the user or owner.  
  _Note_: Backend should filter refunds by `UserId` (for User role) or `OwnerId` (for Owner role, via associated fields). Support pagination for large datasets.
- **Query Parameters**:
  - `startDate`: Filter by start date (format: YYYY-MM-DD, optional).
  - `endDate`: Filter by end date (format: YYYY-MM-DD, optional).
  - `status`: Filter by refund status (e.g., Pending, Completed, optional).
  - `page`: Page number for pagination (default: 1).
  - `pageSize`: Number of items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "refundId": 1,
          "bookingId": 1,
          "fieldId": 1,
          "fieldName": "ABC Field",
          "amount": 300000,
          "status": "Completed",
          "createdAt": "2025-05-21T10:00:00Z"
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "refundId": 1,
        "bookingId": 1,
        "fieldId": 1,
        "fieldName": "ABC Field",
        "amount": 300000,
        "status": "Completed",
        "createdAt": "2025-05-21T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/payments/refunds?startDate=2025-05-01&endDate=2025-05-31&page=1&pageSize=10 \
    -H "Authorization: Bearer <token>"
  ```

---

## 6. Review System

### 6.1 Create Review

- **Method**: POST
- **Path**: `/api/reviews`
- **Role**: [User]
- **Description**: Submits a review for a field after a booking. Only users with a confirmed and paid booking for the specified field can submit a review. Prevents duplicate reviews for the same booking.  
  _Note_: Backend must verify `Booking` has `UserId` matching the current user, `FieldId` matching `fieldId` (via `SubField.FieldId`), `Status = Confirmed`, and `PaymentStatus = Paid`. Check for existing reviews by `bookingId` to prevent duplicates.
- **Request Body**:
  ```json
  {
    "fieldId": 1,
    "bookingId": 1,
    "rating": 5,
    "comment": "string"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "reviewId": 1,
      "fieldId": 1,
      "bookingId": 1,
      "rating": 5,
      "message": "Review submitted successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "A review for this booking already exists"
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
      "message": "You must have a confirmed and paid booking to review this field"
    }
    ```
- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/reviews \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"fieldId":1,"bookingId":1,"rating":5,"comment":"Great field!"}'
  ```

---

### 6.2. Update Review

- **Method**: PUT
- **Path**: `/api/reviews/{id}`
- **Role**: [User]
- **Description**: Updates an existing review.
- **Request Body**:
  ```json
  {
    "rating": 4,
    "comment": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "reviewId": 1,
      "rating": 4,
      "message": "Review updated successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid rating"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Review does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to update this review"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "rating": 4,
    "comment": "Good field, but service could be better."
  }

  // Response
  {
    "reviewId": 1,
    "rating": 4,
    "message": "Review updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/reviews/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"rating":4,"comment":"Good field, but service could be better."}'
  ```

---

### 6.3. Delete Review

- **Method**: DELETE
- **Path**: `/api/reviews/{id}`
- **Role**: [User]
- **Description**: Deletes a review.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Review deleted successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Review does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to delete this review"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Review deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/reviews/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 6.4. Reply to Review

- **Method**: POST
- **Path**: `/api/reviews/{id}/reply`
- **Role**: [Owner]
- **Description**: Allows the field owner to reply to a review.
- **Request Body**:
  ```json
  {
    "reply": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "reviewId": 1,
      "ownerReply": "Thank you for your feedback!",
      "replyDate": "2025-06-02T10:00:00Z",
      "message": "Reply submitted successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Reply is required"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Review does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to reply to this review"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "reply": "Thank you for your feedback!"
  }

  // Response
  {
    "reviewId": 1,
    "ownerReply": "Thank you for your feedback!",
    "replyDate": "2025-06-02T10:00:00Z",
    "message": "Reply submitted successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/reviews/1/reply \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"reply":"Thank you for your feedback!"}'
  ```

---

## 7. Notification System

### 7.1. Get Notifications

- **Method**: GET
- **Path**: `/api/notifications`
- **Role**: [User, Owner]
- **Description**: Retrieves the user's or owner's notifications.
- **Query Parameters**:
  - `isRead`: Filter by read status (true/false).
  - `sort`: Sort by field (e.g., "CreatedAt:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "notificationId": 1,
          "title": "Booking Confirmed",
          "message": "Your booking on 2025-06-01 is confirmed.",
          "isRead": false,
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "notificationId": 1,
        "title": "Booking Confirmed",
        "message": "Your booking on 2025-06-01 is confirmed.",
        "isRead": false,
        "createdAt": "2025-06-01T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/notifications?isRead=false&sort=CreatedAt:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 7.2. Mark Notification as Read

- **Method**: PUT
- **Path**: `/api/notifications/{id}/read`
- **Role**: [User, Owner]
- **Description**: Marks a notification as read.
- **Response**:
  - **200 OK**:
    ```json
    {
      "notificationId": 1,
      "isRead": true,
      "message": "Notification marked as read"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Notification does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to update this notification"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "notificationId": 1,
    "isRead": true,
    "message": "Notification marked as read"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/notifications/1/read \
  -H "Authorization: Bearer <token>"
  ```

---

### 7.3. Delete Notification

- **Method**: DELETE
- **Path**: `/api/notifications/{id}`
- **Role**: [User, Owner]
- **Description**: Deletes a notification.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Notification deleted successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Notification does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to delete this notification"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Notification deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/notifications/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 7.4. Mark All Notifications as Read

- **Method**: PUT
- **Path**: `/api/notifications/read-all`
- **Role**: [User, Owner]
- **Description**: Marks all notifications for the user or owner as read.
- **Response**:
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
- **Example**:
  ```json
  // Response
  {
    "message": "All notifications marked as read"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/notifications/read-all \
  -H "Authorization: Bearer <token>"
  ```

---

### 7.5. Get Unread Notification Count

- **Method**: GET
- **Path**: `/api/notifications/count`
- **Role**: [User, Owner]
- **Description**: Retrieves the count of unread notifications.
- **Response**:
  - **200 OK**:
    ```json
    {
      "unreadCount": 3
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or missing token"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "unreadCount": 3
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/notifications/count \
  -H "Authorization: Bearer <token>"
  ```

---

## 8. Sport Categories

### 8.1. Get Sports

- **Method**: GET
- **Path**: `/api/sports`
- **Role**: [Public]
- **Description**: Retrieves a list of available sports.
- **Query Parameters**:
  - `sort`: Sort by field (e.g., "SportName:asc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "sportId": 1,
          "sportName": "Football",
          "description": "5-a-side and 7-a-side fields"
        }
      ],
      "total": 1,
      "page": 1,
      "pageSize": 10
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "sportId": 1,
        "sportName": "Football",
        "description": "5-a-side and 7-a-side fields"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/sports?sort=SportName:asc&page=1&pageSize=10
  ```

---

### 8.2. Get Sport Details

- **Method**: GET
- **Path**: `/api/sports/{id}`
- **Role**: [Public]
- **Description**: Retrieves details of a specific sport.
- **Response**:
  - **200 OK**:
    ```json
    {
      "sportId": 1,
      "sportName": "Football",
      "description": "5-a-side and 7-a-side fields"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Sport does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "sportId": 1,
    "sportName": "Football",
    "description": "5-a-side and 7-a-side fields"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/sports/1
  ```

---

### 8.3. Get Popular Sports

- **Method**: GET
- **Path**: `/api/sports/popular`
- **Role**: [Public]
- **Description**: Retrieves a list of popular sports based on booking frequency.
- **Query Parameters**:
  - `limit`: Number of sports to return (default: 5).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "sportId": 1,
          "sportName": "Football",
          "bookingCount": 150
        }
      ],
      "total": 1
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "sportId": 1,
        "sportName": "Football",
        "bookingCount": 150
      }
    ],
    "total": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/sports/popular?limit=5
  ```

---

### 8.4 Create Sport

- **Method**: POST
- **Path**: `/api/sports`
- **Role**: [Admin]
- **Description**: Creates a new sport category.  
  _Note_: Backend should validate `sportName` for uniqueness and ensure `description` is optional. Add to `Sport` model if not already present.
- **Request Body**:
  ```json
  {
    "sportName": "string",
    "description": "string"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "sportId": 1,
      "sportName": "Badminton",
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
          "message": "Sport name is required or already exists"
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
- **Example**:

  ```json
  // Request
  {
    "sportName": "Badminton",
    "description": "A racket sport played with a shuttlecock"
  }

  // Response
  {
    "sportId": 1,
    "sportName": "Badminton",
    "message": "Sport created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/sports \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer <token>" \
    -d '{"sportName":"Badminton","description":"A racket sport played with a shuttlecock"}'
  ```

---

### 8.5 Update Sport

- **Method**: PUT
- **Path**: `/api/sports/{id}`
- **Role**: [Admin]
- **Description**: Updates an existing sport category.  
  _Note_: Backend should validate `sportName` for uniqueness (excluding current sport). Ensure `description` is optional.
- **Request Body**:
  ```json
  {
    "sportName": "string",
    "description": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "sportId": 1,
      "sportName": "Badminton",
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
          "message": "Sport name is required or already exists"
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
      "message": "Sport does not exist"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "sportName": "Badminton",
    "description": "Updated description for badminton"
  }

  // Response
  {
    "sportId": 1,
    "sportName": "Badminton",
    "message": "Sport updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/sports/1 \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer <token>" \
    -d '{"sportName":"Badminton","description":"Updated description for badminton"}'
  ```

---

### 8.6 Delete Sport

- **Method**: DELETE
- **Path**: `/api/sports/{id}`
- **Role**: [Admin]
- **Description**: Marks a sport category as deleted (soft delete). Checks for associated fields before deletion.  
  _Note_: Backend must add `Status` field to `Sport` model for soft delete. Ensure no fields are linked to this sport before deletion. The `status` field in response indicates the sport’s new state.
- **Response**:
  - **200 OK**:
    ```json
    {
      "sportId": 1,
      "isActive": false,
      "message": "Sport deleted successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Cannot delete sport with associated fields"
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
      "message": "Sport does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "sportId": 1,
    "status": "Deleted",
    "message": "Sport deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/sports/1 \
    -H "Authorization: Bearer <token>"
  ```

---

## 9. Promotion Management

### 9.1. Get Promotions

- **Method**: GET
- **Path**: `/api/promotions`
- **Role**: [Public]
- **Description**: Retrieves a list of available promotions.
- **Query Parameters**:
  - `fieldId`: Filter by field.
  - `status`: Filter by status (e.g., "Active").
  - `sort`: Sort by field (e.g., "StartDate:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "promotionId": 1,
          "promotionCode": "SUMMER2025",
          "discountType": "Percentage",
          "discountValue": 20,
          "startDate": "2025-06-01",
          "endDate": "2025-08-31",
          "status": "Active"
        }
      ],
      "total": 1,
      "page": 1,
      "pageSize": 10
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "promotionId": 1,
        "promotionCode": "SUMMER2025",
        "discountType": "Percentage",
        "discountValue": 20,
        "startDate": "2025-06-01",
        "endDate": "2025-08-31",
        "status": "Active"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/promotions?status=Active&sort=StartDate:desc&page=1&pageSize=10
  ```

---

### 9.2. Create Promotion

- **Method**: POST
- **Path**: `/api/promotions`
- **Role**: [Owner]
- **Description**: Creates a new promotion for a field.
- **Request Body**:
  ```json
  {
    "fieldId": 1,
    "promotionCode": "string",
    "discountType": "string", // "Percentage" or "Fixed"
    "discountValue": 20,
    "startDate": "2025-06-01",
    "endDate": "2025-08-31",
    "maxUsage": 100,
    "minBookingAmount": 200000
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "promotionId": 1,
      "promotionCode": "SUMMER2025",
      "message": "Promotion created successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid discount value"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to create a promotion for this field"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "fieldId": 1,
    "promotionCode": "SUMMER2025",
    "discountType": "Percentage",
    "discountValue": 20,
    "startDate": "2025-06-01",
    "endDate": "2025-08-31",
    "maxUsage": 100,
    "minBookingAmount": 200000
  }

  // Response
  {
    "promotionId": 1,
    "promotionCode": "SUMMER2025",
    "message": "Promotion created successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/promotions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"fieldId":1,"promotionCode":"SUMMER2025","discountType":"Percentage","discountValue":20,"startDate":"2025-06-01","endDate":"2025-08-31","maxUsage":100,"minBookingAmount":200000}'
  ```

---

### 9.3. Update Promotion

- **Method**: PUT
- **Path**: `/api/promotions/{id}`
- **Role**: [Owner]
- **Description**: Updates an existing promotion.
- **Request Body**:
  ```json
  {
    "promotionCode": "string",
    "discountType": "string",
    "discountValue": 25,
    "startDate": "2025-06-01",
    "endDate": "2025-09-30",
    "maxUsage": 150,
    "minBookingAmount": 250000
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "promotionId": 1,
      "promotionCode": "SUMMER2025",
      "message": "Promotion updated successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid discount value"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Promotion does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to update this promotion"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "promotionCode": "SUMMER2025",
    "discountType": "Percentage",
    "discountValue": 25,
    "startDate": "2025-06-01",
    "endDate": "2025-09-30",
    "maxUsage": 150,
    "minBookingAmount": 250000
  }

  // Response
  {
    "promotionId": 1,
    "promotionCode": "SUMMER2025",
    "message": "Promotion updated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/promotions/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"promotionCode":"SUMMER2025","discountType":"Percentage","discountValue":25,"startDate":"2025-06-01","endDate":"2025-09-30","maxUsage":150,"minBookingAmount":250000}'
  ```

---

### 9.4. Delete Promotion

- **Method**: DELETE
- **Path**: `/api/promotions/{id}`
- **Role**: [Owner]
- **Description**: Deletes a promotion.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Promotion deleted successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Promotion does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to delete this promotion"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Promotion deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/promotions/1 \
  -H "Authorization: Bearer <token>"
  ```

---

### 9.5. Get Suggested Promotions

- **Method**: GET
- **Path**: `/api/promotions/suggestions`
- **Role**: [User]
- **Description**: Retrieves suggested promotions based on user’s booking history or location.
- **Query Parameters**:
  - `fieldId`: Filter by field.
  - `sort`: Sort by field (e.g., "DiscountValue:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "promotionId": 1,
          "promotionCode": "SUMMER2025",
          "discountType": "Percentage",
          "discountValue": 20,
          "startDate": "2025-06-01",
          "endDate": "2025-08-31",
          "status": "Active"
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "promotionId": 1,
        "promotionCode": "SUMMER2025",
        "discountType": "Percentage",
        "discountValue": 20,
        "startDate": "2025-06-01",
        "endDate": "2025-08-31",
        "status": "Active"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/promotions/suggestions?sort=DiscountValue:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 9.6 Validate Promotion

- **Method**: POST
- **Path**: `/api/promotions/validate`
- **Role**: [User]
- **Description**: Validates a promotion code for a specific booking context.  
  _Note_: Backend must verify `promotionCode` is active, not expired, and applicable to `fieldId` and `bookingAmount`. Response includes discount details if valid.
- **Request Body**:
  ```json
  {
    "promotionCode": "string",
    "fieldId": 1,
    "bookingAmount": 300000
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "promotionId": 1,
      "promotionCode": "SUMMER2025",
      "discountType": "Percentage",
      "discountValue": 20,
      "isValid": true,
      "message": "Promotion code is valid"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "details": [
        {
          "field": "promotionCode",
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
- **Example**:

  ```json
  // Request
  {
    "promotionCode": "SUMMER2025",
    "fieldId": 1,
    "bookingAmount": 300000
  }

  // Response
  {
    "promotionId": 1,
    "promotionCode": "SUMMER2025",
    "discountType": "Percentage",
    "discountValue": 20,
    "isValid": true,
    "message": "Promotion code is valid"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/promotions/validate \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer <token>" \
    -d '{"promotionCode":"SUMMER2025","fieldId":1,"bookingAmount":300000}'
  ```

---

## 10. Owner Dashboard

### 10.1. Get Field Statistics

- **Method**: GET
- **Path**: `/api/owners/fields/{id}/stats`
- **Role**: [Owner]
- **Description**: Retrieves statistics for a specific field (e.g., bookings, revenue).
- **Query Parameters**:
  - `startDate`: Start date for stats (e.g., "2025-06-01").
  - `endDate`: End date for stats (e.g., "2025-06-30").
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldId": 1,
      "fieldName": "ABC Field",
      "totalBookings": 50,
      "totalRevenue": 15000000,
      "averageRating": 4.5,
      "bookingCompletionRate": 0.95
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field does not exist"
    }
    ```
  - **403 Forbidden**:
    ```json
    {
      "error": "Forbidden",
      "message": "You do not have permission to view this field"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "fieldId": 1,
    "fieldName": "ABC Field",
    "totalBookings": 50,
    "totalRevenue": 15000000,
    "averageRating": 4.5,
    "bookingCompletionRate": 0.95
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/owners/fields/1/stats?startDate=2025-06-01&endDate=2025-06-30 \
  -H "Authorization: Bearer <token>"
  ```

---

### 10.2. Get Owner Dashboard

- **Method**: GET
- **Path**: `/api/owners/dashboard`
- **Role**: [Owner]
- **Description**: Retrieves an overview of the owner’s fields, bookings, and revenue.
- **Query Parameters**:
  - `startDate`: Start date for stats (e.g., "2025-06-01").
  - `endDate`: End date for stats (e.g., "2025-06-30").
- **Response**:
  - **200 OK**:
    ```json
    {
      "totalFields": 3,
      "totalBookings": 150,
      "totalRevenue": 45000000,
      "fields": [
        {
          "fieldId": 1,
          "fieldName": "ABC Field",
          "bookings": 50,
          "revenue": 15000000
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
- **Example**:
  ```json
  // Response
  {
    "totalFields": 3,
    "totalBookings": 150,
    "totalRevenue": 45000000,
    "fields": [
      {
        "fieldId": 1,
        "fieldName": "ABC Field",
        "bookings": 50,
        "revenue": 15000000
      }
    ]
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/owners/dashboard?startDate=2025-06-01&endDate=2025-06-30 \
  -H "Authorization: Bearer <token>"
  ```

---

## 11. Admin Management

### 11.1. Get All Users

- **Method**: GET
- **Path**: `/api/admin/users`
- **Role**: [Admin]
- **Description**: Retrieves a list of all users.
- **Query Parameters**:
  - `role`: Filter by role (e.g., "User", "Owner").
  - `sort`: Sort by field (e.g., "CreatedAt:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "userId": 1,
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
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or missing token"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "userId": 1,
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
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/admin/users?role=User&sort=CreatedAt:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 11.2. Ban User

- **Method**: PUT
- **Path**: `/api/admin/users/{id}/ban`
- **Role**: [Admin]
- **Description**: Bans a user account.
- **Request Body**:
  ```json
  {
    "reason": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "userId": 1,
      "message": "User banned successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "User does not exist"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "reason": "Violated terms of service"
  }

  // Response
  {
    "userId": 1,
    "message": "User banned successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/admin/users/1/ban \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"reason":"Violated terms of service"}'
  ```

---

### 11.3. Unban User

- **Method**: PUT
- **Path**: `/api/admin/users/{id}/unban`
- **Role**: [Admin]
- **Description**: Unbans a user account.
- **Response**:
  - **200 OK**:
    ```json
    {
      "userId": 1,
      "message": "User unbanned successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "User does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "userId": 1,
    "message": "User unbanned successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/admin/users/1/unban \
  -H "Authorization: Bearer <token>"
  ```

---

### 11.4. Get All Fields

- **Method**: GET
- **Path**: `/api/admin/fields`
- **Role**: [Admin]
- **Description**: Retrieves a list of all fields.
- **Query Parameters**:
  - `status`: Filter by status (e.g., "Active").
  - `sort`: Sort by field (e.g., "CreatedAt:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldId": 1,
          "fieldName": "ABC Field",
          "ownerId": 2,
          "status": "Active",
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldId": 1,
        "fieldName": "ABC Field",
        "ownerId": 2,
        "status": "Active",
        "createdAt": "2025-06-01T10:00:00Z"
      }
    ],
    "total": 1,
    "page": 1,
    "pageSize": 10
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/admin/fields?status=Active&sort=CreatedAt:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 11.5. Approve Field

- **Method**: PUT
- **Path**: `/api/admin/fields/{id}/approve`
- **Role**: [Admin]
- **Description**: Approves a field to make it active.
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldId": 1,
      "message": "Field approved successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field does not exist"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "fieldId": 1,
    "message": "Field approved successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/admin/fields/1/approve \
  -H "Authorization: Bearer <token>"
  ```

---

### 11.6. Reject Field

- **Method**: PUT
- **Path**: `/api/admin/fields/{id}/reject`
- **Role**: [Admin]
- **Description**: Rejects a field with a reason.
- **Request Body**:
  ```json
  {
    "reason": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "fieldId": 1,
      "message": "Field rejected successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Field does not exist"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "reason": "Incomplete field information"
  }

  // Response
  {
    "fieldId": 1,
    "message": "Field rejected successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/admin/fields/1/reject \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"reason":"Incomplete field information"}'
  ```

---

### 11.7. Get All Reviews

- **Method**: GET
- **Path**: `/api/admin/reviews`
- **Role**: [Admin]
- **Description**: Retrieves a list of all reviews for moderation.
- **Query Parameters**:
  - `fieldId`: Filter by field.
  - `sort`: Sort by field (e.g., "CreatedAt:desc").
  - `page`: Page number (default: 1).
  - `pageSize`: Items per page (default: 10).
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "reviewId": 1,
          "fieldId": 1,
          "userId": 1,
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "reviewId": 1,
        "fieldId": 1,
        "userId": 1,
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
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/admin/reviews?fieldId=1&sort=CreatedAt:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

---

### 11.8. Moderate Review

- **Method**: PUT
- **Path**: `/api/admin/reviews/{id}/moderate`
- **Role**: [Admin]
- **Description**: Approves or removes a review.
- **Request Body**:
  ```json
  {
    "status": "string", // "Approved", "Removed"
    "reason": "string" // Required if status is "Removed"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "reviewId": 1,
      "status": "Approved",
      "message": "Review moderated successfully"
    }
    ```
  - **404 Not Found**:
    ```json
    {
      "error": "Resource not found",
      "message": "Review does not exist"
    }
    ```
- **Example**:

  ```json
  // Request
  {
    "status": "Approved"
  }

  // Response
  {
    "reviewId": 1,
    "status": "Approved",
    "message": "Review moderated successfully"
  }
  ```

- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/admin/reviews/1/moderate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"status":"Approved"}'
  ```

---

## 12. Statistics & Analytics

### 12.1. Get Platform Statistics

- **Method**: GET
- **Path**: `/api/statistics`
- **Role**: [Admin]
- **Description**: Retrieves overall platform statistics.
- **Query Parameters**:
  - `startDate`: Start date for stats (e.g., "2025-06-01").
  - `endDate`: End date for stats (e.g., "2025-06-30").
- **Response**:
  - **200 OK**:
    ```json
    {
      "totalUsers": 1000,
      "totalFields": 50,
      "totalBookings": 5000,
      "totalRevenue": 1500000000,
      "averageBookingPrice": 300000
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or missing token"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "totalUsers": 1000,
    "totalFields": 50,
    "totalBookings": 5000,
    "totalRevenue": 1500000000,
    "averageBookingPrice": 300000
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/statistics?startDate=2025-06-01&endDate=2025-06-30 \
  -H "Authorization: Bearer <token>"
  ```

---

### 12.2. Get Trends

- **Method**: GET
- **Path**: `/api/statistics/trends`
- **Role**: [Admin]
- **Description**: Retrieves trending data (e.g., bookings, revenue) over time.
- **Query Parameters**:
  - `startDate`: Start date for trends (e.g., "2025-06-01").
  - `endDate`: End date for trends (e.g., "2025-06-30").
  - `interval`: Time interval (e.g., "daily", "weekly", "monthly").
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "date": "2025-06-01",
          "bookings": 100,
          "revenue": 30000000
        },
        {
          "date": "2025-06-02",
          "bookings": 120,
          "revenue": 36000000
        }
      ],
      "interval": "daily"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid date range or interval"
    }
    ```
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid or missing token"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "date": "2025-06-01",
        "bookings": 100,
        "revenue": 30000000
      },
      {
        "date": "2025-06-02",
        "bookings": 120,
        "revenue": 36000000
      }
    ],
    "interval": "daily"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/statistics/trends?startDate=2025-06-01&endDate=2025-06-30&interval=daily \
  -H "Authorization: Bearer <token>"
  ```
