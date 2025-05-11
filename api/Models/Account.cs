using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        [Required, StringLength(256), EmailAddress]
        public string Email { get; set; }

        [StringLength(256)] // Hash password
        public string? Password { get; set; } // Nullable cho OAuth accounts

        [Required, StringLength(50)]
        public string Role { get; set; } // "Admin", "Owner", "User"

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        [StringLength(50)]
        public string? OAuthProvider { get; set; } // "Google"

        [StringLength(100)]
        public string? OAuthId { get; set; }

        [StringLength(512)]
        public string? AccessToken { get; set; }

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