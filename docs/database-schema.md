# Database Schema

This document describes the database schema for the C4F-ISports application, based on the `Models-v2.0.0.cs`. The schema includes tables, columns, data types, constraints, relationships, and indexes.

## Tables

### 1. Accounts
Stores user account information, including authentication details.

| Column                  | Data Type          | Constraints                              | Description                                      |
|-------------------------|--------------------|------------------------------------------|------------------------------------------------|
| AccountId               | int                | PK, Auto-increment                       | Primary key                                      |
| Email                   | varchar(256)       | NOT NULL, UNIQUE                         | Email address (unique)                          |
| Password                | varchar(256)       | NULL                                     | Hashed password using bcrypt (nullable for OAuth) |
| Role                    | varchar(50)        | NOT NULL                                 | Role: "Admin", "Owner", "User"                  |
| IsActive                | bit                | NOT NULL, DEFAULT 1                      | Account status (active/inactive)                |
| CreatedAt               | datetime2          | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                              |
| UpdatedAt               | datetime2          | NULL                                     | Last update timestamp                           |
| LastLogin               | datetime2          | NULL                                     | Last login timestamp                            |
| OAuthProvider           | varchar(50)        | NULL                                     | OAuth provider (e.g., "Google")                 |
| OAuthId                 | varchar(100)       | NULL, UNIQUE                             | OAuth provider ID                               |
| AccessToken             | varchar(512)       | NULL                                     | OAuth access token                              |
| VerificationToken       | varchar(256)       | NULL                                     | Email verification token                        |
| VerificationTokenExpiry | datetime2          | NULL                                     | Verification token expiry                       |
| ResetToken              | varchar(256)       | NULL                                     | Password reset token                            |
| ResetTokenExpiry        | datetime2          | NULL                                     | Reset token expiry                              |

**Relationships**:
- 1-1 with `Users` (FK: `Users.AccountId`)
- 1-1 with `Owners` (FK: `Owners.AccountId`)
- 1-N with `RefreshTokens` (FK: `RefreshTokens.AccountId`)

**Indexes**:
- UNIQUE INDEX on `Email`
- UNIQUE INDEX on `OAuthId`

---

### 2. Users
Stores user profile information.

| Column         | Data Type    | Constraints                              | Description                           |
|----------------|--------------|------------------------------------------|---------------------------------------|
| UserId         | int          | PK, Auto-increment                       | Primary key                           |
| AccountId      | int          | NOT NULL, UNIQUE, FK to Accounts         | Foreign key to Accounts               |
| FullName       | varchar(100) | NOT NULL                                 | Full name                             |
| Phone          | varchar(20)  | NOT NULL                                 | Phone number                          |
| Gender         | varchar(10)  | NULL                                     | Gender: "Male", "Female", "Other"     |
| DateOfBirth    | datetime2    | NULL                                     | Date of birth                         |
| AvatarUrl      | varchar(500) | NULL                                     | Avatar image URL                      |
| LoyaltyPoints  | decimal(18,2)| NOT NULL, DEFAULT 0                      | Loyalty points                        |
| CreatedAt      | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                    |
| UpdatedAt      | datetime2    | NULL                                     | Last update timestamp                 |
| City           | varchar(100) | NULL                                     | City                                  |
| District       | varchar(100) | NULL                                     | District                              |

**Relationships**:
- 1-1 with `Accounts` (FK: `AccountId`)
- 1-N with `Bookings` (FK: `Bookings.UserId`)
- 1-N with `Reviews` (FK: `Reviews.UserId`)
- 1-N with `Notifications` (FK: `Notifications.UserId`)
- 1-N with `FavoriteFields` (FK: `FavoriteFields.UserId`)
- 1-N with `SearchHistories` (FK: `SearchHistories.UserId`)

---

### 3. Owners
Stores owner profile information.

| Column       | Data Type    | Constraints                              | Description                           |
|--------------|--------------|------------------------------------------|---------------------------------------|
| OwnerId      | int          | PK, Auto-increment                       | Primary key                           |
| AccountId    | int          | NOT NULL, UNIQUE, FK to Accounts         | Foreign key to Accounts               |
| FullName     | varchar(100) | NOT NULL                                 | Full name                             |
| Phone        | varchar(20)  | NOT NULL                                 | Phone number                          |
| Description  | varchar(1000)| NULL                                     | Owner description                     |
| CreatedAt    | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                    |
| UpdatedAt    | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Last update timestamp                 |

**Relationships**:
- 1-1 with `Accounts` (FK: `AccountId`)
- 1-N with `Fields` (FK: `Fields.OwnerId`)

---

### 4. Sports
Stores sport types.

| Column       | Data Type    | Constraints                              | Description                           |
|--------------|--------------|------------------------------------------|---------------------------------------|
| SportId      | int          | PK, Auto-increment                       | Primary key                           |
| SportName    | varchar(50)  | NOT NULL                                 | Sport name (e.g., "Football")         |
| Description  | varchar(500) | NULL                                     | Sport description                     |
| IconUrl      | varchar(500) | NULL                                     | Icon URL                              |
| IsActive     | bit          | NOT NULL, DEFAULT 1                      | Active status                         |

**Relationships**:
- 1-N with `Fields` (FK: `Fields.SportId`)

---

### 5. Fields
Stores main field information.

| Column         | Data Type    | Constraints                              | Description                           |
|----------------|--------------|------------------------------------------|---------------------------------------|
| FieldId        | int          | PK, Auto-increment                       | Primary key                           |
| SportId        | int          | NOT NULL, FK to Sports                   | Foreign key to Sports                 |
| FieldName      | varchar(100) | NOT NULL                                 | Field name                            |
| Phone          | varchar(20)  | NOT NULL                                 | Contact phone                         |
| Address        | varchar(500) | NOT NULL                                 | Address                               |
| OpenHours      | varchar(100) | NOT NULL                                 | Open hours (e.g., "06:00-23:00")      |
| OpenTime       | time         | NULL                                     | Open time                             |
| CloseTime      | time         | NULL                                     | Close time                            |
| OwnerId        | int          | NOT NULL, FK to Owners                   | Foreign key to Owners                 |
| Status         | varchar(20)  | NOT NULL                                 | Status: "Active", "Inactive", "Maintenance" |
| Latitude       | decimal(9,6) | NOT NULL                                 | Latitude                              |
| Longitude      | decimal(9,6) | NOT NULL                                 | Longitude                             |
| City           | varchar(100) | NULL                                     | City                                  |
| District       | varchar(100) | NULL                                     | District                              |
| AverageRating  | decimal(3,1) | NULL                                     | Average rating (0-5)                  |
| CreatedAt      | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                    |
| UpdatedAt      | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Last update timestamp                 |

**Relationships**:
- 1-N with `SubFields` (FK: `SubFields.FieldId`)
- 1-N with `Reviews` (FK: `Reviews.FieldId`)
- 1-N with `FieldImages` (FK: `FieldImages.FieldId`)
- 1-N with `FieldAmenities` (FK: `FieldAmenities.FieldId`)
- 1-N with `FieldDescriptions` (FK: `FieldDescriptions.FieldId`)
- 1-N with `FieldServices` (FK: `FieldServices.FieldId`)
- 1-N with `FavoriteFields` (FK: `FavoriteFields.FieldId`)

---

### 6. SubFields
Stores sub-field (smaller fields within a main field) information.

| Column        | Data Type    | Constraints                              | Description                           |
|---------------|--------------|------------------------------------------|---------------------------------------|
| SubFieldId    | int          | PK, Auto-increment                       | Primary key                           |
| FieldId       | int          | NOT NULL, FK to Fields                   | Foreign key to Fields                 |
| SubFieldName  | varchar(100) | NOT NULL                                 | Sub-field name (e.g., "Sân 5a")       |
| FieldType     | varchar(50)  | NOT NULL                                 | Type: "5-a-side", "7-a-side", "Badminton" |
| Status        | varchar(20)  | NOT NULL                                 | Status: "Active", "Inactive"          |
| Capacity      | int          | NOT NULL                                 | Maximum players                       |
| Description   | varchar(500) | NULL                                     | Description                           |

**Relationships**:
- 1-N with `FieldPricings` (FK: `FieldPricings.SubFieldId`)
- 1-N with `Bookings` (FK: `Bookings.SubFieldId`)

**Indexes**:
- INDEX on `FieldId` (ensures efficient queries for retrieving sub-fields by field)

---

### 7. FieldPricings
Stores pricing for sub-fields based on time and day.

| Column        | Data Type    | Constraints                              | Description                           |
|---------------|--------------|------------------------------------------|---------------------------------------|
| FieldPricingId| int          | PK, Auto-increment                       | Primary key                           |
| SubFieldId    | int          | NOT NULL, FK to SubFields                | Foreign key to SubFields              |
| StartTime     | time         | NOT NULL                                 | Start time                            |
| EndTime       | time         | NOT NULL                                 | End time                              |
| DayOfWeek     | int          | NOT NULL, CHECK (0 <= DayOfWeek <= 6)    | Day of week (0=Sunday, 6=Saturday)    |
| Price         | decimal(18,2)| NOT NULL                                 | Price per hour                        |
| IsActive      | bit          | NOT NULL, DEFAULT 1                      | Active status                         |

**Relationships**:
- Belongs to `SubFields` (FK: `SubFieldId`)

**Indexes**:
- INDEX on `SubFieldId, StartTime, EndTime`

---

### 8. FieldAmenities
Stores amenities for fields.

| Column        | Data Type    | Constraints                              | Description                           |
|---------------|--------------|------------------------------------------|---------------------------------------|
| FieldAmenityId| int          | PK, Auto-increment                       | Primary key                           |
| FieldId       | int          | NOT NULL, FK to Fields                   | Foreign key to Fields                 |
| AmenityName   | varchar(100) | NOT NULL                                 | Amenity name (e.g., "Toilet")         |
| Description   | varchar(500) | NULL                                     | Description                           |
| IconUrl       | varchar(500) | NULL                                     | Icon URL                              |

**Relationships**:
- Belongs to `Fields` (FK: `FieldId`)

---

### 9. FieldDescriptions
Stores descriptions for fields.

| Column             | Data Type    | Constraints                              | Description                           |
|--------------------|--------------|------------------------------------------|---------------------------------------|
| FieldDescriptionId | int          | PK, Auto-increment                       | Primary key                           |
| FieldId            | int          | NOT NULL, FK to Fields                   | Foreign key to Fields                 |
| Description        | varchar(2000)| NOT NULL                                 | Description                           |

**Relationships**:
- Belongs to `Fields` (FK: `FieldId`)

---

### 10. FieldImages
Stores images for fields.

| Column        | Data Type    | Constraints                              | Description                           |
|---------------|--------------|------------------------------------------|---------------------------------------|
| FieldImageId  | int          | PK, Auto-increment                       | Primary key                           |
| FieldId       | int          | NOT NULL, FK to Fields                   | Foreign key to Fields                 |
| ImageUrl      | varchar(500) | NOT NULL                                 | Image URL                             |
| Thumbnail     | varchar(500) | NULL                                     | Thumbnail URL                         |
| IsPrimary     | bit          | NOT NULL, DEFAULT 0                      | Primary image flag                    |
| UploadedAt    | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Upload timestamp                      |

**Relationships**:
- Belongs to `Fields` (FK: `FieldId`)

---

### 11. FieldServices
Stores services offered by fields.

| Column        | Data Type    | Constraints                              | Description                           |
|---------------|--------------|------------------------------------------|---------------------------------------|
| FieldServiceId| int          | PK, Auto-increment                       | Primary key                           |
| FieldId       | int          | NOT NULL, FK to Fields                   | Foreign key to Fields                 |
| ServiceName   | varchar(100) | NOT NULL                                 | Service name (e.g., "Water")          |
| Price         | decimal(18,2)| NOT NULL                                 | Service price                         |
| Description   | varchar(500) | NULL                                     | Description                           |
| IsActive      | bit          | NOT NULL, DEFAULT 1                      | Active status                         |

**Relationships**:
- Belongs to `Fields` (FK: `FieldId`)
- 1-N with `BookingServices` (FK: `BookingServices.FieldServiceId`)

---

### 12. Bookings
Stores booking information for sub-fields.

| Column         | Data Type    | Constraints                              | Description                           |
|----------------|--------------|------------------------------------------|---------------------------------------|
| BookingId      | int          | PK, Auto-increment                       | Primary key                           |
| UserId         | int          | NOT NULL, FK to Users                    | Foreign key to Users                  |
| SubFieldId     | int          | NOT NULL, FK to SubFields                | Foreign key to SubFields              |
| MainBookingId  | int          | NULL, FK to Bookings                     | Foreign key to main Booking (self-referential) |
| BookingDate    | date         | NOT NULL                                 | Booking date                          |
| StartTime      | time         | NOT NULL                                 | Start time                            |
| EndTime        | time         | NOT NULL                                 | End time                              |
| TotalPrice     | decimal(18,2)| NOT NULL                                 | Total price                           |
| Status         | varchar(20)  | NOT NULL                                 | Status: "Confirmed", "Pending", "Cancelled" |
| PaymentStatus  | varchar(20)  | NOT NULL                                 | Payment status: "Paid", "Pending", "Failed" |
| Notes          | varchar(1000)| NULL                                     | Additional notes                      |
| CreatedAt      | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                    |
| UpdatedAt      | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Last update timestamp                 |
| IsReminderSent | bit          | NOT NULL, DEFAULT 0                      | Reminder sent flag                    |
| PromotionId    | int          | NULL, FK to Promotions                   | Foreign key to Promotions             |

**Relationships**:
- Belongs to `Users` (FK: `UserId`)
- Belongs to `SubFields` (FK: `SubFieldId`)
- Self-referential (FK: `MainBookingId` to `Bookings.BookingId`)
- 1-N with `RelatedBookings` (self-referential)
- Belongs to `Promotions` (FK: `PromotionId`)
- 1-N with `BookingServices` (FK: `BookingServices.BookingId`)
- 1-N with `Payments` (FK: `Payments.BookingId`)
- 1-N with `BookingTimeSlots` (FK: `BookingTimeSlots.BookingId`)

---

### 13. BookingServices
Stores services booked with a booking.

| Column           | Data Type    | Constraints                              | Description                           |
|------------------|--------------|------------------------------------------|---------------------------------------|
| BookingServiceId | int          | PK, Auto-increment                       | Primary key                           |
| BookingId        | int          | NOT NULL, FK to Bookings                 | Foreign key to Bookings               |
| FieldServiceId   | int          | NOT NULL, FK to FieldServices            | Foreign key to FieldServices          |
| Quantity         | int          | NOT NULL                                 | Quantity                              |
| Price            | decimal(18,2)| NOT NULL                                 | Price per unit                        |
| Description      | varchar(500) | NULL                                     | Service description at booking time   |

**Relationships**:
- Belongs to `Bookings` (FK: `BookingId`)
- Belongs to `FieldServices` (FK: `FieldServiceId`)

---

### 14. BookingTimeSlots
Stores time slot details for bookings (for flexible pricing).

| Column             | Data Type    | Constraints                              | Description                           |
|--------------------|--------------|------------------------------------------|---------------------------------------|
| BookingTimeSlotId  | int          | PK, Auto-increment                       | Primary key                           |
| BookingId          | int          | NOT NULL, FK to Bookings                 | Foreign key to Bookings               |
| StartTime          | time         | NOT NULL                                 | Start time                            |
| EndTime            | time         | NOT NULL                                 | End time                              |
| Price              | decimal(18,2)| NOT NULL                                 | Price for this slot                   |

**Relationships**:
- Belongs to `Bookings` (FK: `BookingId`)

---

### 15. Payments
Stores payment information for bookings.

| Column         | Data Type    | Constraints                              | Description                           |
|----------------|--------------|------------------------------------------|---------------------------------------|
| PaymentId      | int          | PK, Auto-increment                       | Primary key                           |
| BookingId      | int          | NOT NULL, FK to Bookings                 | Foreign key to Bookings               |
| Amount         | decimal(18,2)| NOT NULL                                 | Payment amount                        |
| PaymentMethod  | varchar(50)  | NOT NULL                                 | Method: "CreditCard", "BankTransfer", "Cash" |
| TransactionId  | varchar(100) | NOT NULL                                 | Transaction ID                        |
| Status         | varchar(20)  | NOT NULL                                 | Status: "Success", "Pending", "Failed" |
| Currency       | varchar(3)   | NOT NULL, DEFAULT 'VND'                  | Currency (e.g., "VND")                |
| CreatedAt      | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                    |
| PaymentDate    | datetime2    | NULL                                     | Payment timestamp                     |

**Relationships**:
- Belongs to `Bookings` (FK: `BookingId`)

---

### 16. Promotions
Stores promotion codes for discounts.

| Column           | Data Type    | Constraints                              | Description                           |
|------------------|--------------|------------------------------------------|---------------------------------------|
| PromotionId      | int          | PK, Auto-increment                       | Primary key                           |
| Code             | varchar(50)  | NOT NULL, UNIQUE                         | Promotion code                        |
| Description      | varchar(500) | NOT NULL                                 | Description                           |
| DiscountType     | varchar(20)  | NOT NULL                                 | Type: "Percentage", "Fixed"           |
| DiscountValue    | decimal(18,2)| NOT NULL                                 | Discount value                        |
| StartDate        | datetime2    | NOT NULL                                 | Start date                            |
| EndDate          | datetime2    | NOT NULL                                 | End date                              |
| MinBookingValue  | decimal(18,2)| NOT NULL                                 | Minimum booking value                 |
| MaxDiscountAmount| decimal(18,2)| NOT NULL                                 | Maximum discount amount               |
| IsActive         | bit          | NOT NULL, DEFAULT 1                      | Active status                         |
| UsageLimit       | int          | NULL                                     | Maximum usage limit                   |
| UsageCount       | int          | NOT NULL, DEFAULT 0                      | Current usage count                   |

**Relationships**:
- 1-N with `Bookings` (FK: `Bookings.PromotionId`)

**Indexes**:
- UNIQUE INDEX on `Code`

---

### 17. Reviews
Stores user reviews for fields.

| Column       | Data Type    | Constraints                              | Description                           |
|--------------|--------------|------------------------------------------|---------------------------------------|
| ReviewId     | int          | PK, Auto-increment                       | Primary key                           |
| UserId       | int          | NOT NULL, FK to Users                    | Foreign key to Users                  |
| FieldId      | int          | NOT NULL, FK to Fields                   | Foreign key to Fields                 |
| Rating       | int          | NOT NULL, CHECK (1 <= Rating <= 5)       | Rating (1-5)                          |
| Comment      | varchar(1000)| NOT NULL                                 | Review comment                        |
| CreatedAt    | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                    |
| UpdatedAt    | datetime2    | NULL                                     | Last update timestamp                 |
| OwnerReply   | varchar(1000)| NULL                                     | Owner's reply                         |
| ReplyDate    | datetime2    | NULL                                     | Reply timestamp                       |
| IsVisible    | bit          | NOT NULL, DEFAULT 1                      | Visibility flag                       |

**Relationships**:
- Belongs to `Users` (FK: `UserId`)
- Belongs to `Fields` (FK: `FieldId`)

---

### 18. Notifications
Stores notifications for users.

| Column           | Data Type    | Constraints                              | Description                           |
|------------------|--------------|------------------------------------------|---------------------------------------|
| NotificationId   | int          | PK, Auto-increment                       | Primary key                           |
| UserId           | int          | NOT NULL, FK to Users                    | Foreign key to Users                  |
| Title            | varchar(100) | NOT NULL                                 | Notification title                    |
| Content          | varchar(2000)| NOT NULL                                 | Notification content                  |
| IsRead           | bit          | NOT NULL, DEFAULT 0                      | Read status                           |
| CreatedAt        | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                    |
| NotificationType | varchar(50)  | NULL                                     | Type: "Booking", "Promotion", "System" |

**Relationships**:
- Belongs to `Users` (FK: `UserId`)

---

### 19. FavoriteFields
Stores fields favorited by users.

| Column       | Data Type    | Constraints                              | Description                           |
|--------------|--------------|------------------------------------------|---------------------------------------|
| FavoriteId   | int          | PK, Auto-increment                       | Primary key                           |
| UserId       | int          | NOT NULL, FK to Users                    | Foreign key to Users                  |
| FieldId      | int          | NOT NULL, FK to Fields                   | Foreign key to Fields                 |
| AddedDate    | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Date added                            |

**Relationships**:
- Belongs to `Users` (FK: `UserId`)
- Belongs to `Fields` (FK: `FieldId`)

---

### 20. SearchHistories
Stores user search history.

| Column           | Data Type    | Constraints                              | Description                           |
|------------------|--------------|------------------------------------------|---------------------------------------|
| SearchHistoryId  | int          | PK, Auto-increment                       | Primary key                           |
| UserId           | int          | NOT NULL, FK to Users                    | Foreign key to Users                  |
| SearchQuery      | varchar(500) | NOT NULL                                 | Search query                          |
| SearchDate       | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Search timestamp                      |
| FieldId          | int          | NULL, FK to Fields                       | Foreign key to Fields (if applicable) |
| Latitude         | decimal(9,6) | NULL                                     | Search latitude                       |
| Longitude        | decimal(9,6) | NULL                                     | Search longitude                      |

**Relationships**:
- Belongs to `Users` (FK: `UserId`)
- Belongs to `Fields` (FK: `FieldId`, nullable)

---

### 21. RefreshTokens
Stores refresh tokens for authentication.

| Column           | Data Type    | Constraints                              | Description                           |
|------------------|--------------|------------------------------------------|---------------------------------------|
| RefreshTokenId   | int          | PK, Auto-increment                       | Primary key                           |
| AccountId        | int          | NOT NULL, FK to Accounts                 | Foreign key to Accounts               |
| Token            | varchar(256) | NOT NULL, UNIQUE                         | Refresh token                         |
| Expires          | datetime2    | NOT NULL                                 | Expiry timestamp                      |
| Created          | datetime2    | NOT NULL, DEFAULT CURRENT_TIMESTAMP      | Creation timestamp                    |
| Revoked          | datetime2    | NULL                                     | Revocation timestamp                  |
| ReplacedByToken  | varchar(256) | NULL                                     | Replacement token                     |

**Relationships**:
- Belongs to `Accounts` (FK: `AccountId`)

**Indexes**:
- UNIQUE INDEX on `Token`

---

## Key Relationships
- `Accounts` has a 1-1 relationship with `Users` or `Owners`.
- `Fields` has a 1-N relationship with `SubFields`, which in turn has a 1-N relationship with `Bookings`.
- `Bookings` supports complex bookings via self-referential `MainBookingId` for grouping multiple sub-field bookings.
- `Users` can have multiple `Notifications`, `FavoriteFields`, `SearchHistories`, and `Reviews`.

## Validation Notes
- `Fields.AverageRating` is constrained to [0, 5] via model validation.
- `FieldImages.ImageUrl` and `FieldAmenities.IconUrl` are validated as URLs.
- `Accounts.Email` is validated for format and uniqueness.
- `Reviews.Rating` is constrained to [1, 5].
- `Bookings.TotalPrice` and `Payments.Amount` use `decimal(18,2)` for precision.

## Notes
- All `datetime` columns use `datetime2` for high precision and UTC for consistency.
- Decimal fields (e.g., `Price`, `TotalPrice`) use `decimal(18,2)` for monetary precision.
- The schema supports multiple sub-field bookings via `Bookings.MainBookingId` and related bookings.
- `BookingTimeSlots` supports flexible pricing within a booking (e.g., different prices for peak hours).
- `Accounts.RefreshToken` and `Accounts.TokenExpiry` have been removed in `Models-v2.0.0.cs`. Refresh tokens are managed exclusively via the `RefreshTokens` table.
- `Accounts.Password` uses `varchar(256)` to store hashed passwords (e.g., using bcrypt).
- Validation constraints from models:
  - `Fields.AverageRating`: Constrained to [0, 5] via `[Range(0, 5)]`.
  - `FieldImages.ImageUrl`, `FieldAmenities.IconUrl`: Validated as URLs via `[Url]`.
  - `Accounts.Email`: Maximum length 256 characters via `[StringLength(256)]` and validated as email via `[EmailAddress]`.
  - `Reviews.Rating`: Constrained to [1, 5] via `[Range(1, 5)]`.
- Key Relationships:
  - `Accounts` 1-1 with `Users` or `Owners` via `AccountId`.
  - `Fields` 1-N with `SubFields`, which in turn 1-N with `Bookings`.
  - `Bookings` self-referential via `MainBookingId` for complex multi-sub-field bookings.
  - `Users` 1-N with `Bookings`, `Reviews`, `Notifications`, `FavoriteFields`, `SearchHistories`.