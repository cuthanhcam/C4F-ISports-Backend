## API Endpoints Compact

| **No.** | **Module**                | **Endpoint**                                   | **HTTP Method** | **Authorization**       | **Description**                                                  | **Dependencies**                           |
|---------|---------------------------|-----------------------------------------------|-----------------|-------------------------|------------------------------------------------------------------|--------------------------------------------|
| 1       | Authentication            | `/api/auth/register`                          | POST            | None                    | Register a new user account                                      | None                                       |
| 2       | Authentication            | `/api/auth/login`                             | POST            | None                    | Log in and generate token                                        | None                                       |
| 3       | Authentication            | `/api/auth/refresh-token`                     | POST            | None                    | Refresh access token                                             | Login                                      |
| 4       | Authentication            | `/api/auth/forgot-password`                   | POST            | None                    | Request password reset                                           | None                                       |
| 5       | Authentication            | `/api/auth/reset-password`                    | POST            | None                    | Reset password                                                   | Forgot Password                            |
| 6       | Authentication            | `/api/auth/logout`                            | POST            | Bearer (User/Owner)     | Log out and invalidate token                                     | Login                                      |
| 7       | Authentication            | `/api/auth/me`                                | GET             | Bearer (User/Owner)     | Get current user info                                            | Login                                      |
| 8       | Authentication            | `/api/auth/change-password`                   | POST            | Bearer (User/Owner)     | Change user password                                             | Login                                      |
| 9       | Authentication            | `/api/auth/verify-email`                      | POST            | None                    | Verify email address                                             | Register                                   |
| 10      | Authentication            | `/api/auth/resend-verification`               | POST            | None                    | Resend email verification                                        | Register                                   |
| 11      | Authentication            | `/api/auth/verify-token`                      | POST            | None                    | Verify token validity                                            | Login                                      |
| 12      | User Management           | `/api/users/profile`                          | GET             | Bearer (User/Owner)     | Get user or owner profile                                        | Authentication                             |
| 13      | User Management           | `/api/users/profile`                          | PUT             | Bearer (User/Owner)     | Update user or owner profile                                     | Authentication                             |
| 14      | User Management           | `/api/users/profile`                          | DELETE          | Bearer (User/Owner)     | Soft delete user or owner profile                                | Authentication, Bookings, Fields           |
| 15      | User Management           | `/api/users/loyalty-points`                   | GET             | Bearer (User)           | Get user loyalty points                                          | Authentication                             |
| 16      | User Management           | `/api/users/favorites`                        | GET             | Bearer (User)           | Get user favorite fields                                         | Authentication, Fields                     |
| 17      | User Management           | `/api/users/favorites`                        | POST            | Bearer (User)           | Add field to favorites                                           | Authentication, Fields                     |
| 18      | User Management           | `/api/users/favorites/{fieldId}`              | DELETE          | Bearer (User)           | Remove field from favorites                                      | Authentication, Fields                     |
| 19      | User Management           | `/api/users/search-history`                   | GET             | Bearer (User)           | Get user search history                                          | Authentication                             |
| 20      | User Management           | `/api/users/bookings`                         | GET             | Bearer (User)           | Get user booking history                                         | Authentication, Booking Management        |
| 21      | User Management           | `/api/users/reviews`                          | GET             | Bearer (User)           | Get user reviews                                                 | Authentication, Review System              |
| 22      | Sport Categories          | `/api/sports`                                 | GET             | None                    | Get list of sport categories                                     | None                                       |
| 23      | Sport Categories          | `/api/sports/{sportId}`                       | GET             | None                    | Get sport by ID                                                  | None                                       |
| 24      | Sport Categories          | `/api/sports/{sportId}/fields`                | GET             | None                    | Get fields by sport                                              | Fields                                     |
| 25      | Sport Categories          | `/api/sports`                                 | POST            | Bearer (Admin)          | Create new sport category                                        | Authentication                             |
| 26      | Sport Categories          | `/api/sports/{sportId}`                       | PUT             | Bearer (Admin)          | Update sport category                                            | Authentication                             |
| 27      | Sport Categories          | `/api/sports/{sportId}`                       | DELETE          | Bearer (Admin)          | Deactivate sport category                                        | Authentication                             |
| 28      | Field Management          | `/api/fields`                                 | GET             | None                    | Get list of fields                                               | Sports                                     |
| 29      | Field Management          | `/api/fields/{fieldId}`                       | GET             | None                    | Get field by ID                                                  | Sports                                     |
| 30      | Field Management          | `/api/fields/validate-address`                | POST            | Bearer (Owner)          | Validate field address                                           | Authentication                             |
| 31      | Field Management          | `/api/fields`                                 | POST            | Bearer (Owner)          | Create new field                                                 | Authentication, Sports                     |
| 32      | Field Management          | `/api/fields/full`                            | POST            | Bearer (Owner)          | Create field with full details                                   | Authentication, Sports                     |
| 33      | Field Management          | `/api/fields/{fieldId}/subfields`             | POST            | Bearer (Owner)          | Create subfield                                                  | Fields                                     |
| 34      | Field Management          | `/api/subfields/{subFieldId}/pricing`         | POST            | Bearer (Owner)          | Create pricing rule                                              | Subfields                                  |
| 35      | Field Management          | `/api/fields/{fieldId}/services`              | POST            | Bearer (Owner)          | Create field service                                             | Fields                                     |
| 36      | Field Management          | `/api/fields/{fieldId}/amenities`             | POST            | Bearer (Owner)          | Create field amenity                                             | Fields                                     |
| 37      | Field Management          | `/api/fields/services/{fieldServiceId}`       | POST            | Bearer (Owner)          | Upload field image                                               | Fields                                     |
| 38      | Field Management          | `/api/fields/{fieldId}/descriptions`          | POST            | Bearer (Owner)          | Add field description                                            | Fields                                     |
| 39      | Field Management          | `/api/fields/{fieldId}`                       | PUT             | Bearer (Owner)          | Update field                                                     | Fields                                     |
| 40      | Field Management          | `/api/fields/{fieldId}/full`                  | PUT             | Bearer (Owner)          | Update field with full details                                   | Fields                                     |
| 41      | Field Management          | `/api/subfields/{subFieldId}`                 | PUT             | Bearer (Owner)          | Update subfield                                                  | Subfields                                  |
| 42      | Field Management          | `/api/subfields/pricing/{pricingRuleId}`      | PUT             | Bearer (Owner)          | Update pricing rule                                              | Pricing Rules                              |
| 43      | Field Management          | `/api/fields/services/{fieldServiceId}`       | PUT             | Bearer (Owner)          | Update field service                                             | Services                                   |
| 44      | Field Management          | `/api/fields/{fieldId}/amenities/{fieldAmenityId}` | PUT       | Bearer (Owner)          | Update field amenity                                             | Amenities                                  |
| 45      | Field Management          | `/api/fields/descriptions/{fieldDescriptionId}` | PUT         | Bearer (Owner)          | Update field description                                         | Descriptions                               |
| 46      | Field Management          | `/api/subfields/{subFieldId}`                 | DELETE          | Bearer (Owner)          | Delete subfield                                                  | Subfields                                  |
| 47      | Field Management          | `/api/subfields/pricing/{pricingRuleId}`      | DELETE          | Bearer (Owner)          | Delete pricing rule                                              | Pricing Rules                              |
| 48      | Field Management          | `/api/fields/services/{fieldServiceId}`       | DELETE          | Bearer (Owner)          | Delete field service                                             | Services                                   |
| 49      | Field Management          | `/api/fields/{fieldId}/amenities/{fieldAmenityId}` | DELETE    | Bearer (Owner)          | Delete field amenity                                             | Amenities                                  |
| 50      | Field Management          | `/api/fields/{fieldId}/images/{imageId}`      | DELETE          | Bearer (Owner)          | Delete field image                                               | Images                                     |
| 51      | Field Management          | `/api/fields/descriptions/{fieldDescriptionId}` | DELETE       | Bearer (Owner)          | Delete field description                                         | Descriptions                               |
| 52      | Field Management          | `/api/fields/{fieldId}`                       | DELETE          | Bearer (Owner)          | Delete field                                                     | Fields                                     |
| 53      | Field Management          | `/api/fields/{fieldId}/availability`          | GET             | None                    | Get field availability                                           | Fields, Subfields                          |
| 54      | Field Management          | `/api/fields/{fieldId}/images`                | GET             | None                    | Get field images                                                 | Fields                                     |
| 55      | Field Management          | `/api/fields/{fieldId}/services`              | GET             | None                    | Get field services                                               | Fields                                     |
| 56      | Field Management          | `/api/fields/{fieldId}/amenities`             | GET             | None                    | Get field amenities                                              | Fields                                     |
| 57      | Field Management          | `/api/fields/{fieldId}/descriptions`          | GET             | None                    | Get field descriptions                                           | Fields                                     |
| 58      | Field Management          | `/api/fields/{fieldId}/subfields`             | GET             | None                    | Get subfields                                                    | Fields                                     |
| 59      | Field Management          | `/api/subfields/{subFieldId}`                 | GET             | None                    | Get subfield by ID                                               | Subfields                                  |
| 60      | Field Management          | `/api/subfields/{subFieldId}/pricing`         | GET             | None                    | Get pricing rules                                                | Subfields                                  |
| 61      | Field Management          | `/api/fields/{fieldId}/reviews`               | GET             | None                    | Get field reviews                                                | Reviews                                    |
| 62      | Field Management          | `/api/fields/{fieldId}/bookings`              | GET             | Bearer (Owner)          | Get field bookings                                               | Bookings                                   |
| 63      | Promotion Management      | `/api/promotions`                             | GET             | None                    | Get active promotions                                            | Fields                                     |
| 64      | Promotion Management      | `/api/promotions/{promotionId}`               | GET             | None                    | Get promotion by ID                                              | Promotions                                 |
| 65      | Promotion Management      | `/api/promotions`                             | POST            | Bearer (Owner)          | Create new promotion                                             | Authentication, Fields                     |
| 66      | Promotion Management      | `/api/promotions/{promotionId}`               | PUT             | Bearer (Owner)          | Update promotion                                                 | Promotions                                 |
| 67      | Promotion Management      | `/api/promotions/{promotionId}`               | DELETE          | Bearer (Owner)          | Soft delete promotion                                            | Promotions                                 |
| 68      | Promotion Management      | `/api/promotions/apply`                       | POST            | Bearer (User)           | Apply promotion to booking preview                               | Authentication, Bookings, Promotions       |
| 69      | Booking Management        | `/api/bookings/preview`                       | POST            | Bearer (User)           | Preview booking details                                          | Authentication, Fields, Promotions         |
| 70      | Booking Management        | `/api/bookings/simple`                        | POST            | Bearer (User)           | Create simple booking                                            | Authentication, Fields                     |
| 71      | Booking Management        | `/api/bookings`                               | POST            | Bearer (User)           | Create booking                                                   | Authentication, Fields, Promotions         |
| 72      | Booking Management        | `/api/bookings/{bookingId}/services`          | POST            | Bearer (User)           | Add service to booking                                           | Bookings, Field Services                   |
| 73      | Booking Management        | `/api/bookings/{bookingId}`                   | GET             | Bearer (User/Owner)     | Get booking by ID                                                | Bookings                                   |
| 74      | Booking Management        | `/api/bookings`                               | GET             | Bearer (User)           | Get user bookings                                                | Bookings                                   |
| 75      | Booking Management        | `/api/bookings/{bookingId}/services`          | GET             | Bearer (User/Owner)     | Get booking services                                             | Bookings, Field Services                   |
| 76      | Booking Management        | `/api/bookings/{bookingId}`                   | PUT             | Bearer (User)           | Update booking                                                   | Bookings                                   |
| 77      | Booking Management        | `/api/bookings/{bookingId}/confirm`           | POST            | Bearer (Owner)          | Confirm booking                                                  | Bookings                                   |
| 78      | Booking Management        | `/api/bookings/{bookingId}/reschedule`        | POST            | Bearer (User)           | Reschedule booking                                               | Bookings                                   |
| 79      | Booking Management        | `/api/bookings/{bookingId}/cancel`            | POST            | Bearer (User)           | Cancel booking                                                   | Bookings                                   |
| 80      | Payment Processing        | `/api/payments`                               | POST            | Bearer (User)           | Initiate payment for booking                                     | Authentication, Bookings                   |
| 81      | Payment Processing        | `/api/payments/{paymentId}`                   | GET             | Bearer (User/Owner)     | Get payment status                                               | Payments                                   |
| 82      | Payment Processing        | `/api/payments`                               | GET             | Bearer (User/Owner)     | Get payment history                                              | Payments                                   |
| 83      | Payment Processing        | `/api/payments/webhook`                       | POST            | None (Webhook Signature) | Handle payment updates                                           | Payments                                   |
| 84      | Payment Processing        | `/api/payments/{paymentId}/refund`            | POST            | Bearer (User)           | Request refund for payment                                       | Payments                                   |
| 85      | Payment Processing        | `/api/payments/refunds/{refundId}/process`    | POST            | Bearer (Owner/Admin)    | Process refund request                                           | Payments, Refunds                          |
| 86      | Review System             | `/api/reviews`                                | POST            | Bearer (User)           | Create review for field                                          | Authentication, Bookings, Payments         |
| 87      | Review System             | `/api/reviews/{reviewId}`                     | PUT             | Bearer (User)           | Update review                                                    | Reviews                                    |
| 88      | Review System             | `/api/reviews/{reviewId}`                     | DELETE          | Bearer (User/Admin)     | Soft delete review                                               | Reviews                                    |
| 89      | Review System             | `/api/reviews`                                | GET             | Bearer (User)           | Get reviews by user                                              | Reviews                                    |
| 90      | Notification System       | `/api/notifications`                          | GET             | Bearer (User/Owner)     | Get notifications                                                | Authentication, Bookings, Payments, Reviews |
| 91      | Notification System       | `/api/notifications/unread-count`             | GET             | Bearer (User/Owner)     | Get unread notification count                                    | Notifications                              |
| 92      | Notification System       | `/api/notifications/{notificationId}/read`    | POST            | Bearer (User/Owner)     | Mark notification as read                                        | Notifications                              |
| 93      | Notification System       | `/api/notifications/read-all`                 | POST            | Bearer (User/Owner)     | Mark all notifications as read                                   | Notifications                              |
| 94      | Notification System       | `/api/notifications/{notificationId}`         | DELETE          | Bearer (User/Owner)     | Delete notification                                              | Notifications                              |
| 95      | Owner Dashboard           | `/api/owner/dashboard`                        | GET             | Bearer (Owner)          | Get owner dashboard stats                                        | Authentication, Fields, Bookings, Payments |
| 96      | Owner Dashboard           | `/api/owner/fields/{fieldId}/stats`           | GET             | Bearer (Owner)          | Get field stats                                                  | Fields, Bookings, Payments                 |
| 97      | Statistics & Analytics    | `/api/analytics/users`                        | GET             | Bearer (Admin)          | Get user analytics                                               | Authentication, Users, Bookings            |
| 98      | Statistics & Analytics    | `/api/analytics/fields`                       | GET             | Bearer (Admin)          | Get field analytics                                              | Fields, Bookings, Payments                 |
| 99      | Admin Management          | `/api/admin/users`                            | GET             | Bearer (Admin)          | Get all users                                                    | Authentication, Users                      |
| 100     | Admin Management          | `/api/admin/users/{accountId}`                | GET             | Bearer (Admin)          | Get user by ID                                                   | Users                                      |
| 101     | Admin Management          | `/api/admin/users/{accountId}`                | PUT             | Bearer (Admin)          | Update user                                                      | Users                                      |
| 102     | Admin Management          | `/api/admin/users/{accountId}`                | DELETE          | Bearer (Admin)          | Delete user                                                      | Users                                      |
| 103     | Admin Management          | `/api/admin/fields`                           | GET             | Bearer (Admin)          | Get all fields                                                   | Fields                                     |
| 104     | Admin Management          | `/api/admin/fields/{fieldId}/status`          | PUT             | Bearer (Admin)          | Update field status                                              | Fields                                     |
| 105     | Admin Management          | `/api/admin/refunds`                          | GET             | Bearer (Admin)          | Manage refunds                                                   | Payments, Refunds                          |
| 106     | Admin Management          | `/api/admin/stats`                            | GET             | Bearer (Admin)          | Get system stats                                                 | Users, Fields, Bookings, Payments          |

## Notes
- **No.**: Sequential numbering for easy tracking and reference (1 to 106).
- **Module**: Groups endpoints by functionality (e.g., Authentication, User Management).
- **Endpoint**: API path, sourced from provided documents.
- **HTTP Method**: GET, POST, PUT, or DELETE.
- **Authorization**: Specifies required authentication (Bearer Token for User/Owner/Admin, None, or Webhook Signature).
- **Description**: Brief function of the endpoint.
- **Dependencies**: Modules or components that must be implemented first (e.g., Bookings depend on Fields).
- **Order**: Modules are arranged in implementation sequence:
  - **Authentication**: Foundation for user authentication.
  - **User Management**: Manages profiles and personalization.
  - **Sport Categories**: Required before Fields for `sportId`.
  - **Field Management**: Core entity for bookings.
  - **Promotion Management**: Supports discounts before bookings.
  - **Booking Management**: Main user interaction.
  - **Payment Processing**: Completes booking flow.
  - **Review System**: Depends on paid bookings.
  - **Notification System**: Notifies events from other modules.
  - **Owner Dashboard**: Aggregates data for owners.
  - **Statistics & Analytics**: System-wide analytics.
  - **Admin Management**: Oversees entire system, implemented last.

## Usage Guidelines
- **Implementation**: Follow the numbered order, ensuring dependencies are complete. Start with Authentication (No. 1-11), then User Management (No. 12-21), etc.
- **Testing**: Write unit tests per module, prioritizing Authentication, Field Management, Booking Management, and Payment Processing.
- **Integration**:
  - Use JWT middleware for Bearer Token validation.
  - Integrate payment gateway (e.g., Stripe, VNPay) for `/api/payments` and `/api/payments/webhook`.
  - Use geocoding service (e.g., Google Maps API) for `/api/fields/validate-address`.
- **Performance**:
  - Apply pagination (`page`, `pageSize`) for list endpoints (e.g., `/api/users/bookings`, `/api/reviews`).
  - Cache public data (e.g., `/api/sports`, `/api/promotions`) for efficiency.
- **Security**:
  - Verify webhook signatures for `/api/payments/webhook`.
  - Enforce constraints (e.g., prevent user deletion with active bookings).