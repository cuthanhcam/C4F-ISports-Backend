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
