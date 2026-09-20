using System.ComponentModel.DataAnnotations;

namespace Wuzzef.Application.DTOs.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email or username is required.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
