# API Endpoints (Version 2.0.0)

This document outlines the API endpoints for the online field booking system. All endpoints use JSON format and follow RESTful conventions. Authentication is required for most endpoints using JWT tokens (`Bearer` scheme).

## Base URL
```
https://api.bookingsystem.com
```

## 1. Authentication
Handles user registration, login, token management, and password recovery.

### 1.1 POST /api/auth/register
Register a new account (User or Owner).

**Request Body**:
```json
{
  "email": "string",
  "password": "string",
  "role": "string", // "User" or "Owner"
  "fullName": "string",
  "phone": "string",
  "city": "string",
  "district": "string",
  "description": "string", // Optional, for Owner only
  "gender": "string", // Optional, "Male", "Female", "Other"
  "dateOfBirth": "string" // Optional, YYYY-MM-DD
}
```

**Response**:
- **201 Created**: `{ "message": "Registration successful, please verify your email" }`
- **400 Bad Request**: `{ "error": "Email already exists" }`

**Validation**:
- `email`: Required, valid email format.
- `password`: Required, min 6 characters.
- `role`: Required, "User" or "Owner".
- `fullName`, `phone`: Required.
- `city`, `district`, `description`, `gender`, `dateOfBirth`: Optional.

### 1.2 POST /api/auth/login
Log in to an account.

**Request Body**:
```json
{
  "email": "string",
  "password": "string"
}
```

**Response**:
- **200 OK**:
  ```json
  {
    "accessToken": "string",
    "refreshToken": "string",
    "user": {
      "accountId": "integer",
      "userId": "integer",
      "email": "string",
      "fullName": "string",
      "role": "string"
    }
  }
  ```
- **401 Unauthorized**: `{ "error": "Invalid credentials" }`

### 1.3 POST /api/auth/refresh
Refresh access token using refresh token.

**Request Body**:
```json
{
  "refreshToken": "string"
}
```

**Response**:
- **200 OK**: `{ "accessToken": "string", "refreshToken": "string" }`
- **401 Unauthorized**: `{ "error": "Invalid or expired refresh token" }`

### 1.4 POST /api/auth/logout
Log out and revoke refresh token.

**Request Body**:
```json
{
  "refreshToken": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Logged out successfully" }`
- **400 Bad Request**: `{ "error": "Invalid refresh token" }`

**Notes**:
- Updates `RefreshToken.Revoked` to `true`.

### 1.5 POST /api/auth/forgot-password
Request a password reset link.

**Request Body**:
```json
{
  "email": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Password reset link sent" }`
- **404 Not Found**: `{ "error": "Email not found" }`

### 1.6 POST /api/auth/reset-password
Reset password using reset token.

**Request Body**:
```json
{
  "resetToken": "string",
  "newPassword": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Password reset successfully" }`
- **400 Bad Request**: `{ "error": "Invalid or expired token" }`

### 1.7 GET /api/auth/me
Get current user's profile.

**Response**:
- **200 OK**:
  ```json
  {
    "accountId": "integer",
    "userId": "integer",
    "email": "string",
    "fullName": "string",
    "role": "string"
  }
  ```
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token.

### 1.8 PUT /api/auth/change-password
Change current user's password.

**Request Body**:
```json
{
  "currentPassword": "string",
  "newPassword": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Password changed successfully" }`
- **400 Bad Request**: `{ "error": "Invalid current password" }`

**Auth**: Requires JWT token.

### 1.9 POST /api/auth/verify-email
Verify email using verification token.

**Request Body**:
```json
{
  "verificationToken": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Email verified successfully" }`
- **400 Bad Request**: `{ "error": "Invalid or expired token" }`

### 1.10 POST /api/auth/resend-verification
Resend email verification link.

**Request Body**:
```json
{
  "email": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Verification email resent" }`
- **404 Not Found**: `{ "error": "Email not found" }`

### 1.11 GET /api/auth/verify-token
Verify if JWT token is valid.

**Response**:
- **200 OK**: `{ "message": "Token is valid" }`
- **401 Unauthorized**: `{ "error": "Invalid token" }`

**Auth**: Requires JWT token.

## 2. User Management
Manages user profiles, bookings, favorites, reviews, and loyalty points.

### 2.1 GET /api/users/profile
Get current user's profile.

**Response**:
- **200 OK**:
  ```json
  {
    "userId": "integer",
    "fullName": "string",
    "email": "string",
    "phone": "string",
    "city": "string",
    "district": "string",
    "avatarUrl": "string",
    "gender": "string",
    "dateOfBirth": "string", // YYYY-MM-DD
    "loyaltyPoints": "integer"
  }
  ```
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token (User role).

### 2.2 PUT /api/users/profile
Update current user's profile.

**Request Body**:
```json
{
  "fullName": "string",
  "phone": "string",
  "city": "string",
  "district": "string",
  "avatarUrl": "string",
  "gender": "string", // "Male", "Female", "Other"
  "dateOfBirth": "string" // YYYY-MM-DD
}
```

**Response**:
- **200 OK**: `{ "message": "Profile updated successfully" }`
- **400 Bad Request**: `{ "error": "Invalid input" }`

**Auth**: Requires JWT token (User role).

### 2.3 DELETE /api/users/profile
Delete current user's account (soft delete).

**Response**:
- **200 OK**: `{ "message": "Account deleted successfully" }`
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token (User role).

### 2.4 GET /api/users/bookings
Get user's booking history.

**Query Parameters**:
- `status`: Optional, filter by status ("Pending", "Confirmed", "Cancelled").
- `page`: Optional, default 1.
- `pageSize`: Optional, default 10.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "bookingId": "integer",
      "fieldName": "string",
      "subFieldName": "string",
      "bookingDate": "string", // ISO 8601
      "startTime": "string", // HH:mm:ss
      "endTime": "string", // HH:mm:ss
      "totalPrice": "number",
      "status": "string",
      "paymentStatus": "string",
      "isReminderSent": "boolean"
    }
  ]
  ```
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token (User role).

### 2.5 GET /api/users/favorites
Get user's favorite fields.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "fieldId": "integer",
      "fieldName": "string",
      "address": "string",
      "city": "string",
      "district": "string",
      "averageRating": "number"
    }
  ]
  ```
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token (User role).

### 2.6 POST /api/users/favorites
Add a field to user's favorites.

**Request Body**:
```json
{
  "fieldId": "integer"
}
```

**Response**:
- **201 Created**: `{ "message": "Field added to favorites" }`
- **400 Bad Request**: `{ "error": "Field already in favorites" }`

**Auth**: Requires JWT token (User role).

### 2.7 DELETE /api/users/favorites/{fieldId}
Remove a field from user's favorites.

**Response**:
- **200 OK**: `{ "message": "Field removed from favorites" }`
- **404 Not Found**: `{ "error": "Field not in favorites" }`

**Auth**: Requires JWT token (User role).

### 2.8 GET /api/users/reviews
Get user's reviews.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "reviewId": "integer",
      "fieldName": "string",
      "rating": "integer",
      "comment": "string",
      "createdAt": "string", // ISO 8601
      "ownerReply": "string",
      "replyDate": "string" // ISO 8601
    }
  ]
  ```
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token (User role).

### 2.9 GET /api/users/loyalty-points
Get user's loyalty points.

**Response**:
- **200 OK**:
  ```json
  {
    "userId": "integer",
    "loyaltyPoints": "integer"
  }
  ```
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token (User role).

### 2.10 POST /api/users/loyalty-points/redeem
Redeem loyalty points for a discount.

**Request Body**:
```json
{
  "points": "integer"
}
```

**Response**:
- **200 OK**: `{ "message": "Points redeemed successfully", "discountCode": "string" }`
- **400 Bad Request**: `{ "error": "Insufficient points" }`

**Auth**: Requires JWT token (User role).

## 3. Field Management
Manages fields, subfields, pricing, images, services, and amenities.

### 3.1 GET /api/fields
Search and list fields.

**Query Parameters**:
- `city`: Optional.
- `district`: Optional.
- `sportId`: Optional.
- `keyword`: Optional, search by field name or address.
- `latitude`: Optional, for location-based search.
- `longitude`: Optional.
- `radius`: Optional, in kilometers.
- `page`: Optional, default 1.
- `pageSize`: Optional, default 10.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "fieldId": "integer",
      "fieldName": "string",
      "address": "string",
      "city": "string",
      "district": "string",
      "latitude": "number",
      "longitude": "number",
      "openTime": "string", // HH:mm:ss
      "closeTime": "string", // HH:mm:ss
      "averageRating": "number",
      "sportId": "integer",
      "sportName": "string"
    }
  ]
  ```

### 3.2 POST /api/fields
Create a new field (Owner only).

**Request Body**:
```json
{
  "fieldName": "string",
  "phone": "string",
  "address": "string",
  "city": "string",
  "district": "string",
  "latitude": "number",
  "longitude": "number",
  "openTime": "string", // HH:mm:ss
  "closeTime": "string", // HH:mm:ss
  "sportId": "integer",
  "status": "string" // "Active", "Inactive", "Pending"
}
```

**Response**:
- **201 Created**: `{ "fieldId": "integer" }`
- **400 Bad Request**: `{ "error": "Invalid input" }`

**Auth**: Requires JWT token (Owner role).

### 3.3 GET /api/fields/{id}
Get field details.

**Response**:
- **200 OK**:
  ```json
  {
    "fieldId": "integer",
    "fieldName": "string",
    "phone": "string",
    "address": "string",
    "city": "string",
    "district": "string",
    "latitude": "number",
    "longitude": "number",
    "openTime": "string",
    "closeTime": "string",
    "averageRating": "number",
    "sportId": "integer",
    "sportName": "string",
    "subFields": [
      {
        "subFieldId": "integer",
        "subFieldName": "string",
        "type": "string",
        "status": "string"
      }
    ],
    "images": [
      {
        "fieldImageId": "integer",
        "imageUrl": "string",
        "isPrimary": "boolean"
      }
    ],
    "services": [
      {
        "fieldServiceId": "integer",
        "name": "string",
        "description": "string",
        "price": "number"
      }
    ],
    "amenities": [
      {
        "fieldAmenityId": "integer",
        "name": "string",
        "description": "string"
      }
    ],
    "descriptions": [
      {
        "fieldDescriptionId": "integer",
        "content": "string"
      }
    ],
    "pricing": [
      {
        "fieldPricingId": "integer",
        "subFieldId": "integer",
        "pricingType": "string",
        "price": "number",
        "startTime": "string",
        "endTime": "string",
        "dayOfWeek": "string"
      }
    ]
  }
  ```
- **404 Not Found**: `{ "error": "Field not found" }`

### 3.4 PUT /api/fields/{id}
Update field details (Owner only).

**Request Body**: Same as POST /api/fields.

**Response**:
- **200 OK**: `{ "message": "Field updated successfully" }`
- **404 Not Found**: `{ "error": "Field not found" }`

**Auth**: Requires JWT token (Owner role).

### 3.5 DELETE /api/fields/{id}
Delete a field (soft delete, Owner only).

**Response**:
- **200 OK**: `{ "message": "Field deleted successfully" }`
- **404 Not Found**: `{ "error": "Field not found" }`

**Auth**: Requires JWT token (Owner role).

### 3.6 GET /api/fields/availability
Check field availability.

**Query Parameters**:
- `fieldId`: Required.
- `date`: Required, format YYYY-MM-DD.
- `duration`: Optional, in hours.
- `subFieldType`: Optional, e.g., "5-a-side".

**Response**:
- **200 OK**:
  ```json
  [
    {
      "fieldId": "integer",
      "fieldName": "string",
      "subFieldId": "integer",
      "subFieldName": "string",
      "date": "string",
      "startTime": "string",
      "endTime": "string",
      "price": "number",
      "promotion": {
        "promotionId": "integer",
        "code": "string",
        "discountType": "string",
        "discountValue": "number"
      }
    }
  ]
  ```
- **400 Bad Request**: `{ "error": "Invalid parameters" }`

### 3.7 POST /api/fields/{id}/subfields
Add a subfield to a field (Owner only).

**Request Body**:
```json
{
  "subFieldName": "string",
  "type": "string",
  "status": "string"
}
```

**Response**:
- **201 Created**: `{ "subFieldId": "integer" }`
- **400 Bad Request**: `{ "error": "Invalid input" }`

**Auth**: Requires JWT token (Owner role).

### 3.8 PUT /api/fields/{id}/subfields/{subFieldId}
Update a subfield (Owner only).

**Request Body**: Same as POST /api/fields/{id}/subfields.

**Response**:
- **200 OK**: `{ "message": "Subfield updated successfully" }`
- **404 Not Found**: `{ "error": "Subfield not found" }`

**Auth**: Requires JWT token (Owner role).

### 3.9 DELETE /api/subfields/{id}
Delete a subfield (Owner only).

**Response**:
- **200 OK**: `{ "message": "Subfield deleted successfully" }`
- **404 Not Found**: `{ "error": "Subfield not found" }`

**Auth**: Requires JWT token (Owner role).

### 3.10 POST /api/fields/{id}/images
Upload field images (Owner only).

**Request Body** (multipart/form-data):
- `images`: List of image files.
- `isPrimary`: Optional, boolean.

**Response**:
- **201 Created**:
  ```json
  [
    {
      "fieldImageId": "integer",
      "imageUrl": "string",
      "isPrimary": "boolean"
    }
  ]
  ```
- **400 Bad Request**: `{ "error": "Invalid image format" }`

**Auth**: Requires JWT token (Owner role).

**Notes**:
- Integrates with Cloudinary for image storage.

### 3.11 POST /api/fields/{id}/services
Add a service to a field (Owner only).

**Request Body**:
```json
{
  "name": "string",
  "description": "string",
  "price": "number"
}
```

**Response**:
- **201 Created**: `{ "fieldServiceId": "integer" }`
- **400 Bad Request**: `{ "error": "Invalid input" }`

**Auth**: Requires JWT token (Owner role).

### 3.12 PUT /api/fields/{id}/services/{serviceId}
Update a field service (Owner only).

**Request Body**: Same as POST /api/fields/{id}/services.

**Response**:
- **200 OK**: `{ "message": "Service updated successfully" }`
- **404 Not Found**: `{ "error": "Service not found" }`

**Auth**: Requires JWT token (Owner role).

### 3.13 DELETE /api/fields/{id}/services/{serviceId}
Delete a field service (Owner only).

**Response**:
- **200 OK**: `{ "message": "Service deleted successfully" }`
- **404 Not Found**: `{ "error": "Service not found" }`

**Auth**: Requires JWT token (Owner role).

### 3.14 POST /api/fields/{id}/amenities
Add an amenity to a field (Owner only).

**Request Body**:
```json
{
  "name": "string",
  "description": "string"
}
```

**Response**:
- **201 Created**: `{ "fieldAmenityId": "integer" }`
- **400 Bad Request**: `{ "error": "Invalid input" }`

**Auth**: Requires JWT token (Owner role).

### 3.15 PUT /api/fields/{id}/amenities/{amenityId}
Update a field amenity (Owner only).

**Request Body**: Same as POST /api/fields/{id}/amenities.

**Response**:
- **200 OK**: `{ "message": "Amenity updated successfully" }`
- **404 Not Found**: `{ "error": "Amenity not found" }`

**Auth**: Requires JWT token (Owner role).

### 3.16 DELETE /api/fields/{id}/amenities/{amenityId}
Delete a field amenity (Owner only).

**Response**:
- **200 OK**: `{ "message": "Amenity deleted successfully" }`
- **404 Not Found**: `{ "error": "Amenity not found" }`

**Auth**: Requires JWT token (Owner role).

## 4. Booking Management
Manages field bookings.

### 4.1 POST /api/bookings/preview
Preview a booking with pricing details.

**Request Body**:
```json
{
  "subFieldId": "integer",
  "bookingDate": "string", // YYYY-MM-DD
  "startTime": "string", // HH:mm:ss
  "endTime": "string", // HH:mm:ss
  "promotionCode": "string", // Optional
  "services": [
    {
      "fieldServiceId": "integer",
      "quantity": "integer"
    }
  ]
}
```

**Response**:
- **200 OK**:
  ```json
  {
    "fieldName": "string",
    "subFieldName": "string",
    "bookingDate": "string",
    "startTime": "string",
    "endTime": "string",
    "basePrice": "number",
    "servicePrice": "number",
    "discount": "number",
    "totalPrice": "number",
    "promotion": {
      "promotionId": "integer",
      "code": "string",
      "discountType": "string",
      "discountValue": "number"
    }
  }
  ```
- **400 Bad Request**: `{ "error": "Invalid time slot or promotion" }`

**Auth**: Requires JWT token (User role).

### 4.2 POST /api/bookings
Create a new booking.

**Request Body**:
```json
{
  "subFieldId": "integer",
  "bookingDate": "string", // YYYY-MM-DD
  "startTime": "string", // HH:mm:ss
  "endTime": "string", // HH:mm:ss
  "promotionCode": "string", // Optional
  "services": [
    {
      "fieldServiceId": "integer",
      "quantity": "integer"
    }
  ],
  "paymentMethod": "string" // "VNPay", "BankCard"
}
```

**Response**:
- **201 Created**:
  ```json
  {
    "bookingId": "integer",
    "totalPrice": "number",
    "paymentUrl": "string" // For VNPay redirect
  }
  ```
- **400 Bad Request**: `{ "error": "Time slot unavailable" }`

**Auth**: Requires JWT token (User role).

### 4.3 GET /api/bookings/{id}
Get booking details.

**Response**:
- **200 OK**:
  ```json
  {
    "bookingId": "integer",
    "fieldName": "string",
    "subFieldName": "string",
    "bookingDate": "string",
    "startTime": "string",
    "endTime": "string",
    "totalPrice": "number",
    "status": "string",
    "paymentStatus": "string",
    "isReminderSent": "boolean",
    "services": [
      {
        "fieldServiceId": "integer",
        "name": "string",
        "quantity": "integer",
        "price": "number"
      }
    ],
    "promotion": {
      "promotionId": "integer",
      "code": "string",
      "discountType": "string",
      "discountValue": "number"
    }
  }
  ```
- **404 Not Found**: `{ "error": "Booking not found" }`

**Auth**: Requires JWT token (User or Owner role).

### 4.4 PUT /api/bookings/{id}/cancel
Cancel a booking.

**Response**:
- **200 OK**: `{ "message": "Booking cancelled successfully" }`
- **400 Bad Request**: `{ "error": "Cannot cancel confirmed booking" }`

**Auth**: Requires JWT token (User role).

## 5. Payment & Refund
Manages payments and refunds.

### 5.1 POST /api/payments
Process a payment for a booking.

**Request Body**:
```json
{
  "bookingId": "integer",
  "amount": "number",
  "paymentMethod": "string", // "VNPay", "BankCard"
  "currency": "string" // "VND"
}
```

**Response**:
- **201 Created**:
  ```json
  {
    "paymentId": "integer",
    "transactionId": "string",
    "status": "string",
    "paymentUrl": "string" // For VNPay
  }
  ```
- **400 Bad Request**: `{ "error": "Invalid booking" }`

**Auth**: Requires JWT token (User role).

**Notes**:
- Integrates with VNPay for payment processing.

### 5.2 POST /api/payments/refunds
Request a refund for a payment.

**Request Body**:
```json
{
  "paymentId": "integer",
  "reason": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Refund request submitted" }`
- **400 Bad Request**: `{ "error": "Invalid payment" }`

**Auth**: Requires JWT token (User role).

## 6. Review System
Manages field reviews.

### 6.1 POST /api/reviews
Submit a review for a field.

**Request Body**:
```json
{
  "fieldId": "integer",
  "rating": "integer", // 1-5
  "comment": "string"
}
```

**Response**:
- **201 Created**: `{ "reviewId": "integer" }`
- **400 Bad Request**: `{ "error": "User must have a confirmed booking" }`

**Auth**: Requires JWT token (User role).

**Notes**:
- User must have a confirmed and paid booking for the field.

### 6.2 PUT /api/reviews/{id}
Update a review.

**Request Body**:
```json
{
  "rating": "integer",
  "comment": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Review updated successfully" }`
- **404 Not Found**: `{ "error": "Review not found" }`

**Auth**: Requires JWT token (User role).

### 6.3 DELETE /api/reviews/{id}
Delete a review.

**Response**:
- **200 OK**: `{ "message": "Review deleted successfully" }`
- **404 Not Found**: `{ "error": "Review not found" }`

**Auth**: Requires JWT token (User or Admin role).

### 6.4 POST /api/reviews/{id}/reply
Reply to a review (Owner only).

**Request Body**:
```json
{
  "reply": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Reply submitted successfully" }`
- **404 Not Found**: `{ "error": "Review not found" }`

**Auth**: Requires JWT token (Owner role).

## 7. Notification System
Manages user notifications.

### 7.1 GET /api/notifications
Get user's notifications.

**Query Parameters**:
- `isRead`: Optional, boolean.
- `page`: Optional, default 1.
- `pageSize`: Optional, default 10.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "notificationId": "integer",
      "title": "string",
      "content": "string",
      "isRead": "boolean",
      "createdAt": "string" // ISO 8601
    }
  ]
  ```
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token (User or Owner role).

### 7.2 PUT /api/notifications/{id}/read
Mark a notification as read.

**Response**:
- **200 OK**: `{ "message": "Notification marked as read" }`
- **404 Not Found**: `{ "error": "Notification not found" }`

**Auth**: Requires JWT token (User or Owner role).

## 8. Sport Categories
Manages sport categories.

### 8.1 GET /api/sports
List all sports.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "sportId": "integer",
      "sportName": "string",
      "description": "string",
      "isActive": "boolean"
    }
  ]
  ```

### 8.2 GET /api/sports/{id}
Get sport details.

**Response**:
- **200 OK**:
  ```json
  {
    "sportId": "integer",
    "sportName": "string",
    "description": "string",
    "isActive": "boolean"
  }
  ```
- **404 Not Found**: `{ "error": "Sport not found" }`

### 8.3 GET /api/sports/popular
Get popular sports based on booking count.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "sportId": "integer",
      "sportName": "string",
      "bookingCount": "integer"
    }
  ]
  ```

## 9. Promotion Management
Manages promotions.

### 9.1 GET /api/promotions
List active promotions.

**Query Parameters**:
- `fieldId`: Optional.
- `code`: Optional.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "promotionId": "integer",
      "fieldId": "integer",
      "code": "string",
      "description": "string",
      "discountType": "string",
      "discountValue": "number",
      "startDate": "string",
      "endDate": "string"
    }
  ]
  ```

### 9.2 POST /api/promotions
Create a promotion (Owner or Admin only).

**Request Body**:
```json
{
  "fieldId": "integer", // Optional
  "code": "string",
  "description": "string",
  "discountType": "string",
  "discountValue": "number",
  "startDate": "string",
  "endDate": "string",
  "minBookingValue": "number",
  "maxDiscountAmount": "number",
  "usageLimit": "integer"
}
```

**Response**:
- **201 Created**: `{ "promotionId": "integer" }`
- **400 Bad Request**: `{ "error": "Invalid input" }`

**Auth**: Requires JWT token (Owner or Admin role).

## 10. Owner Dashboard
Manages owner-specific statistics and field management.

### 10.1 GET /api/owners/fields/{id}/stats
Get statistics for a field (Owner only).

**Response**:
- **200 OK**:
  ```json
  {
    "fieldId": "integer",
    "totalBookings": "integer",
    "totalRevenue": "number",
    "averageRating": "number",
    "bookingTrends": [
      {
        "date": "string",
        "bookingCount": "integer",
        "revenue": "number"
      }
    ]
  }
  ```
- **404 Not Found**: `{ "error": "Field not found" }`

**Auth**: Requires JWT token (Owner role).

### 10.2 GET /api/owners/dashboard
Get owner's dashboard summary.

**Response**:
- **200 OK**:
  ```json
  {
    "totalFields": "integer",
    "totalBookings": "integer",
    "totalRevenue": "number",
    "recentBookings": [
      {
        "bookingId": "integer",
        "fieldName": "string",
        "subFieldName": "string",
        "bookingDate": "string",
        "totalPrice": "number"
      }
    ]
  }
  ```
- **401 Unauthorized**: `{ "error": "Unauthorized" }`

**Auth**: Requires JWT token (Owner role).

## 11. Admin Management
Manages system-wide operations (Admin only).

### 11.1 GET /api/admin/users
List all users.

**Query Parameters**:
- `role`: Optional, "User" or "Owner".
- `page`: Optional, default 1.
- `pageSize`: Optional, default 10.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "userId": "integer",
      "email": "string",
      "fullName": "string",
      "role": "string",
      "isActive": "boolean"
    }
  ]
  ```

**Auth**: Requires JWT token (Admin role).

### 11.2 PUT /api/admin/users/{id}/status
Update user account status (Admin only).

**Request Body**:
```json
{
  "isActive": "boolean"
}
```

**Response**:
- **200 OK**: `{ "message": "User status updated" }`
- **404 Not Found**: `{ "error": "User not found" }`

**Auth**: Requires JWT token (Admin role).

### 11.3 GET /api/admin/fields
List all fields.

**Query Parameters**:
- `status`: Optional, "Active", "Inactive", "Pending".
- `page`: Optional, default 1.
- `pageSize`: Optional, default 10.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "fieldId": "integer",
      "fieldName": "string",
      "ownerId": "integer",
      "status": "string"
    }
  ]
  ```

**Auth**: Requires JWT token (Admin role).

### 11.4 PUT /api/admin/fields/{id}/status
Update field status (Admin only).

**Request Body**:
```json
{
  "status": "string"
}
```

**Response**:
- **200 OK**: `{ "message": "Field status updated" }`
- **404 Not Found**: `{ "error": "Field not found" }`

**Auth**: Requires JWT token (Admin role).

### 11.5 GET /api/admin/reviews
List all reviews.

**Query Parameters**:
- `fieldId`: Optional.
- `isVisible`: Optional, boolean.
- `page`: Optional, default 1.
- `pageSize`: Optional, default 10.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "reviewId": "integer",
      "fieldId": "integer",
      "userId": "integer",
      "rating": "integer",
      "comment": "string",
      "isVisible": "boolean"
    }
  ]
  ```

**Auth**: Requires JWT token (Admin role).

### 11.6 PUT /api/admin/reviews/{id}/visibility
Update review visibility (Admin only).

**Request Body**:
```json
{
  "isVisible": "boolean"
}
```

**Response**:
- **200 OK**: `{ "message": "Review visibility updated" }`
- **404 Not Found**: `{ "error": "Review not found" }`

**Auth**: Requires JWT token (Admin role).

### 11.7 GET /api/statistics
Get system-wide statistics (Admin only).

**Response**:
- **200 OK**:
  ```json
  {
    "totalUsers": "integer",
    "totalFields": "integer",
    "totalBookings": "integer",
    "totalRevenue": "number"
  }
  ```

**Auth**: Requires JWT token (Admin role).

### 11.8 GET /api/statistics/trends
Get booking and revenue trends (Admin only).

**Query Parameters**:
- `startDate`: Optional, format YYYY-MM-DD.
- `endDate`: Optional, format YYYY-MM-DD.

**Response**:
- **200 OK**:
  ```json
  [
    {
      "date": "string",
      "bookingCount": "integer",
      "revenue": "number"
    }
  ]
  ```

**Auth**: Requires JWT token (Admin role).

## Error Handling
All endpoints return standard error responses:
```json
{
  "error": "string",
  "details": "string" // Optional
}
```

## Notes
- All endpoints requiring authentication expect a JWT token in the `Authorization` header (`Bearer <token>`).
- Use Redis for caching frequently accessed endpoints (`/api/fields`, `/api/fields/availability`, `/api/statistics`).
- Integrations:
  - Cloudinary for image uploads (`/api/fields/{id}/images`).
  - Google Maps API for location-based search (`/api/fields`).
  - VNPay for payment processing (`/api/payments`).
- Validation is handled via DTOs in the backend, ensuring required fields and formats are enforced.
- BackgroundService uses `Booking.IsReminderSent` to send email reminders for upcoming bookings.