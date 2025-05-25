## 4. Booking Management

### 4.1 Create Booking

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

### 4.2 Get Booking By Id

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

### 4.3 Update Booking

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

### 4.4 Confirm Booking

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

### 4.5 Cancel Booking

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

### 4.6 Get User Bookings

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

### 4.7 Create Simple Booking

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

### 4.8 Get Booking Services

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

### 4.9 Preview Booking

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

### 4.10 Add Booking Service

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

### 4.11 Reschedule Booking

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
