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
