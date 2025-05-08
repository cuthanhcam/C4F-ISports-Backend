# API Endpoints C4F-ISports v2.0.0

## 1. Authentication & Authorization Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| POST | `/api/auth/register` | Đăng ký tài khoản | `{ email, password, fullName, phone }` | `{ accessToken, refreshToken, user }` | None |
| POST | `/api/auth/login` | Đăng nhập local | `{ email, password }` | `{ accessToken, refreshToken, user }` | None |
| POST | `/api/auth/oauth/login` | Đăng nhập OAuth2 | `{ provider, code }` | `{ accessToken, refreshToken, user }` | None |
| POST | `/api/auth/refresh-token` | Làm mới token | `{ refreshToken }` | `{ accessToken, refreshToken }` | None |
| POST | `/api/auth/logout` | Đăng xuất | `{ refreshToken }` | `{ message: "Logged out" }` | None |
| GET | `/api/auth/me` | Lấy thông tin tài khoản | None | `{ user }` | User, Owner, Admin |

## 2. User Management Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| GET | `/api/users/profile` | Xem thông tin cá nhân | None | `{ user, loyaltyPoints }` | User |
| PUT | `/api/users/profile` | Cập nhật thông tin | `{ fullName, phone, avatarUrl }` | `{ user }` | User |
| GET | `/api/users/bookings` | Lịch sử đặt sân | None | `{ bookings: [] }` | User |
| GET | `/api/users/favorite-fields` | Sân yêu thích | None | `{ fields: [] }` | User |
| POST | `/api/users/favorite-fields/{fieldId}` | Thêm sân yêu thích | None | `{ message: "Field added" }` | User |
| DELETE | `/api/users/favorite-fields/{fieldId}` | Xóa sân yêu thích | None | `{ message: "Field removed" }` | User |
| GET | `/api/users/search-history` | Lịch sử tìm kiếm | None | `{ searchHistories: [] }` | User |
| GET | `/api/users/loyalty-points` | Xem điểm thưởng | None | `{ loyaltyPoints }` | User |

## 3. Field Management Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| GET | `/api/fields` | Lấy danh sách sân lớn | None | `{ fields: [] }` | None |
| POST | `/api/fields` | Thêm sân lớn | `{ fieldName, sportId, address, ... }` | `{ field }` | Owner |
| GET | `/api/fields/{id}` | Chi tiết sân lớn | None | `{ field, subFields, services, amenities }` | None |
| PUT | `/api/fields/{id}` | Cập nhật sân lớn | `{ fieldName, address, ... }` | `{ field }` | Owner |
| DELETE | `/api/fields/{id}` | Xóa sân lớn | None | `{ message: "Field deleted" }` | Owner |
| GET | `/api/fields/{id}/subfields` | Lấy danh sách sân nhỏ | None | `{ subFields: [] }` | None |
| POST | `/api/fields/{id}/subfields` | Thêm sân nhỏ | `{ subFieldName, fieldType, status }` | `{ subField }` | Owner |
| GET | `/api/fields/{fieldId}/subfields/{subFieldId}` | Chi tiết sân nhỏ | None | `{ subField, pricings }` | None |
| PUT | `/api/fields/{fieldId}/subfields/{subFieldId}` | Cập nhật sân nhỏ | `{ subFieldName, fieldType, status }` | `{ subField }` | Owner |
| DELETE | `/api/fields/{fieldId}/subfields/{subFieldId}` | Xóa sân nhỏ | None | `{ message: "SubField deleted" }` | Owner |
| GET | `/api/fields/{fieldId}/subfields/{subFieldId}/availability` | Khung giờ trống | `{ date }` | `{ timeSlots: [] }` | None |
| POST | `/api/fields/{fieldId}/subfields/{subFieldId}/pricings` | Thêm giá thuê | `{ startTime, endTime, dayOfWeek, price }` | `{ pricing }` | Owner |
| GET | `/api/fields/{id}/services` | Lấy dịch vụ | None | `{ services: [] }` | None |
| POST | `/api/fields/{id}/services` | Thêm dịch vụ | `{ serviceName, price }` | `{ service }` | Owner |
| GET | `/api/fields/{id}/amenities` | Lấy tiện ích | None | `{ amenities: [] }` | None |
| POST | `/api/fields/{id}/amenities` | Thêm tiện ích | `{ amenityName }` | `{ amenity }` | Owner |
| GET | `/api/fields/{id}/reviews` | Đánh giá sân | None | `{ reviews: [] }` | None |
| GET | `/api/fields/nearby` | Sân gần vị trí | `{ latitude, longitude }` | `{ fields: [] }` | None |
| GET | `/api/fields/suggested` | Gợi ý sân | None | `{ fields: [] }` | User |

## 4. Booking Management Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| POST | `/api/bookings` | Tạo đơn đặt sân | `{ subFieldId, date, startTime, endTime, services: [] }` | `{ booking }` | User |
| GET | `/api/bookings` | Danh sách đặt sân | None | `{ bookings: [] }` | User, Owner |
| GET | `/api/bookings/{id}` | Chi tiết đặt sân | None | `{ booking }` | User, Owner |
| PUT | `/api/bookings/{id}` | Cập nhật đặt sân | `{ date, startTime, endTime }` | `{ booking }` | User |
| DELETE | `/api/bookings/{id}` | Hủy đặt sân | None | `{ message: "Booking canceled" }` | User |
| PUT | `/api/bookings/{id}/status` | Cập nhật trạng thái | `{ status }` | `{ booking }` | Owner |
| POST | `/api/bookings/preview` | Xem trước đơn | `{ subFieldId, date, startTime, endTime, serviceIds: [], promotionCode }` | `{ totalPrice, discount, finalPrice, services: [] }` | User |

## 5. Payment Processing Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| POST | `/api/payments` | Tạo thanh toán | `{ bookingId, amount, paymentMethod }` | `{ paymentUrl }` | User |
| GET | `/api/payments/{id}` | Chi tiết thanh toán | None | `{ payment }` | User, Owner |
| POST | `/api/payments/webhook` | Webhook thanh toán | `{ transactionId, status, ... }` | `{ message: "Webhook processed" }` | None |
| POST | `/api/payments/{id}/refund` | Hoàn tiền | `{ reason }` | `{ message: "Refund processed" }` | Owner, Admin |

## 6. Review System Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| POST | `/api/reviews` | Thêm đánh giá | `{ fieldId, rating, comment }` | `{ review }` | User |
| PUT | `/api/reviews/{id}` | Cập nhật đánh giá | `{ rating, comment }` | `{ review }` | User |
| DELETE | `/api/reviews/{id}` | Xóa đánh giá | None | `{ message: "Review deleted" }` | User |
| POST | `/api/reviews/{id}/reply` | Trả lời đánh giá | `{ content }` | `{ message: "Reply added" }` | Owner |

## 7. Notification System Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| GET | `/api/notifications` | Danh sách thông báo | None | `{ notifications: [] }` | User, Owner |
| PUT | `/api/notifications/{id}/read` | Đánh dấu đã đọc | None | `{ message: "Notification marked as read" }` | User, Owner |
| DELETE | `/api/notifications/{id}` | Xóa thông báo | None | `{ message: "Notification deleted" }` | User, Owner |

## 8. Sport Categories Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| GET | `/api/sports` | Danh sách loại thể thao | None | `{ sports: [] }` | None |
| POST | `/api/sports` | Thêm loại thể thao | `{ sportName }` | `{ sport }` | Admin |

## 9. Owner Dashboard Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| GET | `/api/owners/dashboard` | Tổng quan dashboard | None | `{ totalBookings, totalRevenue, averageRating }` | Owner |
| GET | `/api/owners/fields` | Danh sách sân lớn | None | `{ fields: [] }` | Owner |
| GET | `/api/owners/bookings` | Danh sách đơn đặt | None | `{ bookings: [] }` | Owner |
| GET | `/api/owners/revenues` | Báo cáo doanh thu | None | `{ revenues: [] }` | Owner |

## 10. Promotion Management Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| POST | `/api/promotions` | Tạo khuyến mãi | `{ code, discountType, discountValue, ... }` | `{ promotion }` | Owner, Admin |
| GET | `/api/promotions` | Danh sách khuyến mãi | None | `{ promotions: [] }` | None |
| POST | `/api/promotions/verify` | Kiểm tra khuyến mãi | `{ code }` | `{ promotion }` | User |

## 11. Admin Management Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| GET | `/api/admin/users` | Quản lý người dùng | None | `{ users: [] }` | Admin |
| GET | `/api/admin/fields` | Quản lý sân lớn | None | `{ fields: [] }` | Admin |
| GET | `/api/admin/reviews` | Quản lý đánh giá | None | `{ reviews: [] }` | Admin |

## 12. Statistics & Analytics Endpoints
| Method | Endpoint | Description | Request Body | Response | Authorization |
|--------|----------|-------------|--------------|----------|---------------|
| GET | `/api/statistics/fields` | Thống kê sân lớn | None | `{ fields: [] }` | Owner, Admin |
| GET | `/api/statistics/bookings` | Thống kê đặt sân | None | `{ bookings: [] }` | Owner, Admin |