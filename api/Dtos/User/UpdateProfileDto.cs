using System;
using System.ComponentModel.DataAnnotations;

namespace api.Dtos.User
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string AvatarUrl { get; set; }
    }
}