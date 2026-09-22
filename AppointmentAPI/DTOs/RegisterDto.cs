using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace AppointmentAPI.DTOs

{
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? Role { get; set; }
    }
    
}
