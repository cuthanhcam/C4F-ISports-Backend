using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; }
    }
} 