using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int AccountId { get; set; }

        [Required, StringLength(256)]
        public string Token { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public DateTime? Revoked { get; set; }

        [StringLength(256)]
        public string? ReplacedByToken { get; set; }

        public Account Account { get; set; }
    }
}