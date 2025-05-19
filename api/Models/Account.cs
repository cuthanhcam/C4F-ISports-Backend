using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        [Required, StringLength(256), EmailAddress]
        public required string Email { get; set; }

        [Required, StringLength(256)] // Mật khẩu băm
        public required string Password { get; set; }

        [Required, StringLength(50)]
        public required string Role { get; set; } // "Admin", "Owner", "User"

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        [StringLength(256)]
        public string? VerificationToken { get; set; }

        public DateTime? VerificationTokenExpiry { get; set; }

        [StringLength(256)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }

        public User? User { get; set; }
        public Owner? Owner { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}