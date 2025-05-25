# Compact API Endpoints (Version 2.0.0)

| No.  | Method | Path                                      | Description                                      |
|------|--------|-------------------------------------------|--------------------------------------------------|
| 1.1  | POST   | /api/auth/register                        | Register a new User or Owner account             |
| 1.2  | POST   | /api/auth/login                           | Log in to an account                             |
| 1.3  | POST   | /api/auth/refresh                         | Refresh JWT token                                |
| 1.4  | POST   | /api/auth/logout                          | Log out and revoke refresh token                 |
| 1.5  | POST   | /api/auth/forgot-password                 | Request password reset link                      |
| 1.6  | POST   | /api/auth/reset-password                  | Reset password using reset token                 |
| 1.7  | GET    | /api/auth/me                              | Get current user's profile                       |
| 1.8  | PUT    | /api/auth/change-password                 | Change current user's password                   |
| 1.9  | POST   | /api/auth/verify-email                    | Verify email using verification token            |
| 1.10 | POST   | /api/auth/resend-verification             | Resend email verification link                   |
| 1.11 | GET    | /api/auth/verify-token                    | Verify JWT token validity                        |
| 2.1  | GET    | /api/users/profile                        | Get current user's profile                       |
| 2.2  | PUT    | /api/users/profile                        | Update current user's profile                    |
| 2.3  | DELETE | /api/users/profile                        | Delete current user's account (soft delete)      |
| 2.4  | GET    | /api/users/bookings                       | Get user's booking history                       |
| 2.5  | GET    | /api/users/favorites                      | Get user's favorite fields                       |
| 2.6  | POST   | /api/users/favorites                      | Add a field to user's favorites                  |
| 2.7  | DELETE | /api/users/favorites/{fieldId}            | Remove a field from user's favorites             |
| 2.8  | GET    | /api/users/reviews                        | Get user's reviews                               |
| 2.9  | GET    | /api/users/loyalty-points                 | Get user's loyalty points                        |
| 2.10 | POST   | /api/users/loyalty-points/redeem          | Redeem loyalty points for a discount             |
| 3.1  | GET    | /api/fields                               | Search and list fields                           |
| 3.2  | POST   | /api/fields                               | Create a new field (Owner only)                  |
| 3.3  | GET    | /api/fields/{id}                          | Get field details                                |
| 3.4  | PUT    | /api/fields/{id}                          | Update field details (Owner only)                |
| 3.5  | DELETE | /api/fields/{id}                          | Delete a field (soft delete, Owner only)         |
| 3.6  | GET    | /api/fields/availability                  | Check field availability                         |
| 3.7  | POST   | /api/fields/{id}/subfields                | Add a subfield to a field (Owner only)           |
| 3.8  | PUT    | /api/fields/{id}/subfields/{subFieldId}   | Update a subfield (Owner only)                   |
| 3.9  | DELETE | /api/subfields/{id}                       | Delete a subfield (Owner only)                   |
| 3.10 | POST   | /api/fields/{id}/images                   | Upload field images (Owner only)                 |
| 3.11 | POST   | /api/fields/{id}/services                 | Add a service to a field (Owner only)            |
| 3.12 | PUT    | /api/fields/{id}/services/{serviceId}     | Update a field service (Owner only)              |
| 3.13 | DELETE | /api/fields/{id}/services/{serviceId}     | Delete a field service (Owner only)              |
| 3.14 | POST   | /api/fields/{id}/amenities                | Add an amenity to a field (Owner only)           |
| 3.15 | PUT    | /api/fields/{id}/amenities/{amenityId}    | Update a field amenity (Owner only)              |
| 3.16 | DELETE | /api/fields/{id}/amenities/{amenityId}    | Delete a field amenity (Owner only)              |
| 4.1  | POST   | /api/bookings/preview                     | Preview a booking with pricing details           |
| 4.2  | POST   | /api/bookings                             | Create a new booking                             |
| 4.3  | GET    | /api/bookings/{id}                        | Get booking details                              |
| 4.4  | PUT    | /api/bookings/{id}/cancel                 | Cancel a booking                                 |
| 5.1  | POST   | /api/payments                             | Process a payment for a booking                  |
| 5.2  | POST   | /api/payments/refunds                     | Request a refund for a payment                   |
| 6.1  | POST   | /api/reviews                              | Submit a review for a field                      |
| 6.2  | PUT    | /api/reviews/{id}                         | Update a review                                  |
| 6.3  | DELETE | /api/reviews/{id}                         | Delete a review                                  |
| 6.4  | POST   | /api/reviews/{id}/reply                   | Reply to a review (Owner only)                   |
| 7.1  | GET    | /api/notifications                        | Get user's notifications                         |
| 7.2  | PUT    | /api/notifications/{id}/read              | Mark a notification as read                      |
| 8.1  | GET    | /api/sports                               | List all sports                                  |
| 8.2  | GET    | /api/sports/{id}                          | Get sport details                                |
| 8.3  | GET    | /api/sports/popular                       | Get popular sports based on booking count        |
| 9.1  | GET    | /api/promotions                           | List active promotions                           |
| 9.2  | POST   | /api/promotions                           | Create a promotion (Owner or Admin only)         |
| 10.1 | GET    | /api/owners/fields/{id}/stats             | Get statistics for a field (Owner only)          |
| 10.2 | GET    | /api/owners/dashboard                     | Get owner's dashboard summary                    |
| 11.1 | GET    | /api/admin/users                          | List all users (Admin only)                      |
| 11.2 | PUT    | /api/admin/users/{id}/status              | Update user account status (Admin only)          |
| 11.3 | GET    | /api/admin/fields                         | List all fields (Admin only)                     |
| 11.4 | PUT    | /api/admin/fields/{id}/status             | Update field status (Admin only)                 |
| 11.5 | GET    | /api/admin/reviews                        | List all reviews (Admin only)                    |
| 11.6 | PUT    | /api/admin/reviews/{id}/visibility        | Update review visibility (Admin only)            |
| 11.7 | GET    | /api/statistics                           | Get system-wide statistics (Admin only)          |
| 11.8 | GET    | /api/statistics/trends                    | Get booking and revenue trends (Admin only)      |

## Notes
- Base URL: `https://api.bookingsystem.com`.
- Authentication: Most endpoints require JWT token (`Bearer <token>`).
- Use `api-endpoints-v2.0.0.md` for detailed request/response formats.