## 12. Statistics & Analytics

### 12.1 Get User Analytics

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

### 12.2 Get Field Analytics

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