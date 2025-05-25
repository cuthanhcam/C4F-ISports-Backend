using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Account
    {
        public int AccountId { get; set; }

        [Required, StringLength(100)]
        public required string Email { get; set; }

        [Required, StringLength(256)] // Lưu password đã băm
        public required string Password { get; set; }

        [Required, StringLength(20)]
        public required string Role { get; set; } // "User" or "Owner"

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        [StringLength(256)]
        public string? VerificationToken { get; set; }

        public DateTime? VerificationTokenExpiry { get; set; }

        [StringLength(256)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }
        public DateTime? DeletedAt { get; set; } // Hỗ trợ soft delete

        public User? User { get; set; }
        public Owner? Owner { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}