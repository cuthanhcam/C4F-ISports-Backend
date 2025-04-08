using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int AccountId { get; set; }         // Liên kết với Account
        public string Token { get; set; }          // Token string
        public DateTime Expires { get; set; }      // Thời gian hết hạn
        public DateTime Created { get; set; }      // Thời gian tạo
        public DateTime? Revoked { get; set; }     // Thời gian thu hồi (nullable)
        public string? ReplacedByToken { get; set; } // Token thay thế (nullable)

        public Account Account { get; set; }
    }
}