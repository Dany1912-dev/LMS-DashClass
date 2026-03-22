using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class Solicitar2FARequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}