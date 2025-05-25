## 9. Promotion Management

### 9.1 Get Promotions

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

### 9.2 Create Promotion

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

### 9.3 Update Promotion

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

### 9.4 Delete Promotion

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

### 9.5 Apply Promotion

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

**Note**:

- Validates `Promotion.Code`, `Promotion.StartDate`, `Promotion.EndDate`, and `Promotion.UsageLimit`.

### 9.6 Get Promotion By ID

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
