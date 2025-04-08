using System;

namespace api.Dtos.User
{
    public class UserProfileResponseDto
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public string DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
    }
} 