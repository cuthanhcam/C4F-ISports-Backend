# API Endpoints - C4F-ISports v2.0.0

## Base URL
```
https://api.c4f-isports.com/v2
```

## Authentication
- Most endpoints require a **JWT Bearer Token** in the `Authorization` header: `Authorization: Bearer <token>`.
- Endpoints marked with `[Public]` are accessible without authentication.
- Endpoints marked with `[User]`, `[Owner]`, or `[Admin]` require specific roles.
- OAuth2 (Google) is supported for login via `/api/auth/oauth/google`.

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

### 1.3. Google OAuth Login
- **Method**: POST
- **Path**: `/api/auth/oauth/google`
- **Role**: [Public]
- **Description**: Authenticates a user via Google OAuth2 access token.
- **Request Body**:
  ```json
  {
    "accessToken": "string"
  }
  ```
- **Response**:
  - **200 OK**: Same as `/api/auth/login`.
  - **401 Unauthorized**:
    ```json
    {
      "error": "Unauthorized",
      "message": "Invalid Google access token"
    }
    ```
- **Example**:
  ```json
  // Request
  {
    "accessToken": "ya29.a0AfH6SMD..."
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
  curl -X POST https://api.c4f-isports.com/v2/api/auth/oauth/google \
  -H "Content-Type: application/json" \
  -d '{"accessToken":"ya29.a0AfH6SMD..."}'
  ```

### 1.4. Refresh Token
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

### 1.5. Forgot Password
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

### 1.6. Reset Password
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

### 1.7. Logout
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

### 1.8. Get Current User
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

### 1.9. Change Password
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

### 1.10. Verify Email
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

### 1.11. Resend Verification Email
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

### 1.12. Verify Token
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

## 3. Field Management

### 3.1. Get Fields
- **Method**: GET
- **Path**: `/api/fields`
- **Role**: [Public]
- **Description**: Retrieves a list of fields with filtering and pagination.
- **Query Parameters**:
  - `city`: Filter by city (e.g., "Hà Nội").
  - `district`: Filter by district (e.g., "Đống Đa").
  - `sportId`: Filter by sport (e.g., 1 for Football).
  - `latitude`, `longitude`, `radius`: Filter by location (e.g., `radius` in kilometers).
  - `sort`: Sort by field (e.g., "FieldName:asc", "AverageRating:desc").
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
          "longitude": 106.700,
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
        "longitude": 106.700,
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
  curl -X GET https://api.c4f-isports.com/v2/api/fields?city=Hà%20Nội&sportId=1&sort=AverageRating:desc&page=1&pageSize=10
  ```

### 3.2. Create Field
- **Method**: POST
- **Path**: `/api/fields`
- **Role**: [Owner]
- **Description**: Creates a new field.
- **Request Body**:
  ```json
  {
    "sportId": 1,
    "fieldName": "string",
    "phone": "string",
    "address": "string",
    "city": "string",
    "district": "string",
    "latitude": 10.776,
    "longitude": 106.700,
    "openTime": "06:00:00",
    "closeTime": "22:00:00",
    "subFields": [
      {
        "subFieldName": "string",
        "fieldType": "string", // e.g., "5-a-side"
        "capacity": 10
      }
    ]
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "fieldId": 1,
      "fieldName": "ABC Field",
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
- **Example**:
  ```json
  // Request
  {
    "sportId": 1,
    "fieldName": "ABC Field",
    "phone": "0909123456",
    "address": "123 Lang Street",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "latitude": 10.776,
    "longitude": 106.700,
    "openTime": "06:00:00",
    "closeTime": "22:00:00",
    "subFields": [
      {
        "subFieldName": "Field A",
        "fieldType": "5-a-side",
        "capacity": 10
      }
    ]
  }

  // Response
  {
    "fieldId": 1,
    "fieldName": "ABC Field",
    "message": "Field created successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"sportId":1,"fieldName":"ABC Field","phone":"0909123456","address":"123 Lang Street","city":"Hà Nội","district":"Đống Đa","latitude":10.776,"longitude":106.700,"openTime":"06:00:00","closeTime":"22:00:00","subFields":[{"subFieldName":"Field A","fieldType":"5-a-side","capacity":10}]}'
  ```

### 3.3. Get Field Details
- **Method**: GET
- **Path**: `/api/fields/{id}`
- **Role**: [Public]
- **Description**: Retrieves details of a specific field.
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
      "longitude": 106.700,
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
    "longitude": 106.700,
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
    ]
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/1
  ```

### 3.4. Update Field
- **Method**: PUT
- **Path**: `/api/fields/{id}`
- **Role**: [Owner]
- **Description**: Updates information of an existing field.
- **Request Body**: Same as POST `/api/fields`.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Field updated successfully",
      "fieldId": 1
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
      "message": "You do not own this field"
    }
    ```
- **Example**:
  ```json
  // Request
  {
    "sportId": 1,
    "fieldName": "Updated ABC Field",
    "phone": "0987654321",
    "address": "123 Lang Street",
    "city": "Hà Nội",
    "district": "Đống Đa",
    "latitude": 10.776,
    "longitude": 106.700,
    "openTime": "06:00:00",
    "closeTime": "22:00:00"
  }

  // Response
  {
    "message": "Field updated successfully",
    "fieldId": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/fields/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"sportId":1,"fieldName":"Updated ABC Field","phone":"0987654321","address":"123 Lang Street","city":"Hà Nội","district":"Đống Đa","latitude":10.776,"longitude":106.700,"openTime":"06:00:00","closeTime":"22:00:00"}'
  ```

### 3.5. Delete Field
- **Method**: DELETE
- **Path**: `/api/fields/{id}`
- **Role**: [Owner]
- **Description**: Deletes a field.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Field deleted successfully"
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
      "message": "You do not own this field"
    }
    ```
- **Example**:
  ```json
  // Response
  {
    "message": "Field deleted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X DELETE https://api.c4f-isports.com/v2/api/fields/1 \
  -H "Authorization: Bearer <token>"
  ```

### 3.6. Check Field Availability
- **Method**: GET
- **Path**: `/api/fields/availability`
- **Role**: [Public]
- **Description**: Checks availability of a subfield for a specific date and time.
- **Query Parameters**:
  - `fieldId`: Field ID (required).
  - `subFieldId`: SubField ID (required).
  - `date`: Booking date (e.g., "2025-06-01", required).
  - `startTime`: Start time (e.g., "08:00:00", required).
  - `endTime`: End time (e.g., "09:00:00", required).
- **Response**:
  - **200 OK**:
    ```json
    {
      "available": true,
      "subFieldId": 1,
      "price": 300000
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Invalid date or time range"
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
  // Response
  {
    "available": true,
    "subFieldId": 1,
    "price": 300000
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/availability?fieldId=1&subFieldId=1&date=2025-06-01&startTime=08:00:00&endTime=09:00:00
  ```

### 3.7. Upload Field Images
- **Method**: POST
- **Path**: `/api/fields/{id}/images`
- **Role**: [Owner]
- **Description**: Uploads images for a field (multipart/form-data).
- **Request Body**:
  ```
  Content-Type: multipart/form-data
  image: <file>
  ```
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
      "message": "Invalid image format"
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
  -F "image=@/path/to/field1.jpg"
  ```

### 3.8. Get Field Images
- **Method**: GET
- **Path**: `/api/fields/{id}/images`
- **Role**: [Public]
- **Description**: Retrieves all images of a field.
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldImageId": 1,
          "imageUrl": "https://cloudinary.com/field1.jpg"
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
        "fieldImageId": 1,
        "imageUrl": "https://cloudinary.com/field1.jpg"
      }
    ],
    "total": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/fields/1/images
  ```

### 3.9. Get Owner’s Fields
- **Method**: GET
- **Path**: `/api/fields/owner`
- **Role**: [Owner]
- **Description**: Retrieves all fields owned by the current owner.
- **Query Parameters**:
  - `status`: Filter by status (e.g., "Active").
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
        "fieldId": 1,
        "fieldName": "ABC Field",
        "address": "123 Lang Street, Hanoi",
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
  curl -X GET https://api.c4f-isports.com/v2/api/fields/owner?status=Active&sort=FieldName:asc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

### 3.10. Find Nearby Fields
- **Method**: GET
- **Path**: `/api/fields/nearby`
- **Role**: [Public]
- **Description**: Finds fields near a specified location.
- **Query Parameters**:
  - `latitude`: Latitude (required).
  - `longitude`: Longitude (required).
  - `radius`: Radius in kilometers (required).
  - `sportId`: Filter by sport.
  - `sort`: Sort by field (e.g., "Distance:asc").
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
          "distance": 2.5,
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
      "message": "Latitude and longitude are required"
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
        "distance": 2.5,
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
  curl -X GET https://api.c4f-isports.com/v2/api/fields/nearby?latitude=10.776&longitude=106.700&radius=5&sort=Distance:asc&page=1&pageSize=10
  ```

### 3.11. Report Field
- **Method**: POST
- **Path**: `/api/fields/{id}/report`
- **Role**: [User]
- **Description**: Reports a field for inappropriate content or issues.
- **Request Body**:
  ```json
  {
    "reason": "string"
  }
  ```
- **Response**:
  - **201 Created**:
    ```json
    {
      "message": "Report submitted successfully"
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
    "reason": "Inappropriate field description"
  }

  // Response
  {
    "message": "Report submitted successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/fields/1/report \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"reason":"Inappropriate field description"}'
  ```

### 3.12. Get Suggested Fields
- **Method**: GET
- **Path**: `/api/fields/suggested`
- **Role**: [User]
- **Description**: Retrieves suggested fields based on user’s search history and favorites.
- **Query Parameters**:
  - `sort`: Sort by field (e.g., "AverageRating:desc").
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
  curl -X GET https://api.c4f-isports.com/v2/api/fields/suggested?sort=AverageRating:desc&page=1&pageSize=10 \
  -H "Authorization: Bearer <token>"
  ```

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
          "serviceName": "Water",
          "quantity": 2,
          "price": 20000
        }
      ]
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
        "serviceName": "Water",
        "quantity": 2,
        "price": 20000
      }
    ]
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/bookings/1 \
  -H "Authorization: Bearer <token>"
  ```

### 4.4. Update Booking
- **Method**: PUT
- **Path**: `/api/bookings/{id}`
- **Role**: [User]
- **Description**: Updates a booking before confirmation or payment.
- **Request Body**: Same as POST `/api/bookings`.
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Booking updated successfully",
      "bookingId": 1
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Time slot is not available"
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
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "09:00:00",
    "endTime": "10:00:00",
    "services": [
      {
        "fieldServiceId": 1,
        "quantity": 1
      }
    ]
  }

  // Response
  {
    "message": "Booking updated successfully",
    "bookingId": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/bookings/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"subFieldId":1,"bookingDate":"2025-06-01","startTime":"09:00:00","endTime":"10:00:00","services":[{"fieldServiceId":1,"quantity":1}]}'
  ```

### 4.5. Cancel Booking
- **Method**: DELETE
- **Path**: `/api/bookings/{id}`
- **Role**: [User]
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
      "message": "Booking cannot be cancelled"
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

### 4.6. Create Simple Booking
- **Method**: POST
- **Path**: `/api/bookings/simple`
- **Role**: [User]
- **Description**: Creates a simple booking without additional services.
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

### 4.7. Update Booking Status
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
      "message": "Booking status updated successfully",
      "bookingId": 1
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
- **Example**:
  ```json
  // Request
  {
    "status": "Confirmed"
  }

  // Response
  {
    "message": "Booking status updated successfully",
    "bookingId": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X PUT https://api.c4f-isports.com/v2/api/bookings/1/status \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"status":"Confirmed"}'
  ```

### 4.8. Get Booking Services
- **Method**: GET
- **Path**: `/api/bookings/{id}/services`
- **Role**: [User, Owner]
- **Description**: Retrieves the services associated with a booking.
- **Response**:
  - **200 OK**:
    ```json
    {
      "data": [
        {
          "fieldServiceId": 1,
          "serviceName": "Water",
          "quantity": 2,
          "price": 20000
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
- **Example**:
  ```json
  // Response
  {
    "data": [
      {
        "fieldServiceId": 1,
        "serviceName": "Water",
        "quantity": 2,
        "price": 20000
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

### 4.9. Preview Booking
- **Method**: POST
- **Path**: `/api/bookings/preview`
- **Role**: [User]
- **Description**: Previews booking details, including price, services, and discounts.
- **Request Body**:
  ```json
  {
    "subFieldId": 1,
    "bookingDate": "2025-06-01",
    "startTime": "08:00:00",
    "endTime": "09:00:00",
    "serviceIds": [1],
    "promotionCode": "string"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "subFieldId": 1,
      "totalPrice": 300000,
      "discount": 30000,
      "finalPrice": 270000,
      "services": [
        {
          "fieldServiceId": 1,
          "serviceName": "Water",
          "quantity": 1,
          "price": 20000
        }
      ]
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
    "serviceIds": [1],
    "promotionCode": "SUMMER2025"
  }

  // Response
  {
    "subFieldId": 1,
    "totalPrice": 300000,
    "discount": 30000,
    "finalPrice": 270000,
    "services": [
      {
        "fieldServiceId": 1,
        "serviceName": "Water",
        "quantity": 1,
        "price": 20000
      }
    ]
  }
  ```
- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/bookings/preview \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"subFieldId":1,"bookingDate":"2025-06-01","startTime":"08:00:00","endTime":"09:00:00","serviceIds":[1],"promotionCode":"SUMMER2025"}'
  ```

### 4.10. Get Booking Invoice
- **Method**: GET
- **Path**: `/api/bookings/{id}/invoice`
- **Role**: [User]
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
      "totalPrice": 300000,
      "services": [
        {
          "serviceName": "Water",
          "quantity": 2,
          "price": 20000
        }
      ],
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
    "services": [
      {
        "serviceName": "Water",
        "quantity": 2,
        "price": 20000
      }
    ],
    "paymentStatus": "Paid"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X GET https://api.c4f-isports.com/v2/api/bookings/1/invoice \
  -H "Authorization: Bearer <token>"
  ```

### 4.11. Reschedule Booking
- **Method**: POST
- **Path**: `/api/bookings/{id}/reschedule`
- **Role**: [User]
- **Description**: Requests to reschedule a booking to a new date and time.
- **Request Body**:
  ```json
  {
    "newDate": "2025-06-02",
    "newStartTime": "09:00:00",
    "newEndTime": "10:00:00"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "message": "Reschedule request sent",
      "bookingId": 1
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
    "message": "Reschedule request sent",
    "bookingId": 1
  }
  ```
- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/bookings/1/reschedule \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"newDate":"2025-06-02","newStartTime":"09:00:00","newEndTime":"10:00:00"}'
  ```

## 5. Payment Processing

### 5.1. Create Payment
- **Method**: POST
- **Path**: `/api/payments`
- **Role**: [User]
- **Description**: Initiates a payment transaction for a booking.
- **Request Body**:
  ```json
  {
    "bookingId": 1,
    "paymentMethod": "string" // e.g., "VNPay"
  }
  ```
- **Response**:
  - **200 OK**:
    ```json
    {
      "paymentId": 1,
      "paymentUrl": "https://sandbox.vnpayment.vn/...",
      "message": "Payment initiated successfully"
    }
    ```
  - **400 Bad Request**:
    ```json
    {
      "error": "Invalid input",
      "message": "Booking is not eligible for payment"
    }
    ```
- **Example**:
  ```json
  // Request
  {
    "bookingId": 1,
    "paymentMethod": "VNPay"
  }

  // Response
  {
    "paymentId": 1,
    "paymentUrl": "https://sandbox.vnpayment.vn/...",
    "message": "Payment initiated successfully"
  }
  ```
- **Example cURL**:
  ```bash
  curl -X POST https://api.c4f-isports.com/v2/api/payments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"bookingId":1,"paymentMethod":"VNPay"}'
  ```

### 5.2. Get Payment History
- **Method**: GET
- **Path**: `/api/payments`
- **Role**: [User]
- **Description**: Retrieves the user’s payment history.
- **Query Parameters**:
  - `status`: Filter by status (e.g., "Paid", "Pending").
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
          "amount": 300000,
          "paymentMethod": "VNPay",
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
        "amount": 300000,
        "paymentMethod": "VNPay",
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
  -