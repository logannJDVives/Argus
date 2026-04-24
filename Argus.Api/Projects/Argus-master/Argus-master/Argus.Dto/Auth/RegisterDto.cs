using System.ComponentModel.DataAnnotations;

namespace Argus.Dto.Auth
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
