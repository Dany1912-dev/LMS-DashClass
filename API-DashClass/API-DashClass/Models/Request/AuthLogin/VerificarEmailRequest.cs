using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class VerificarEmailRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
        public string Codigo { get; set; } = string.Empty;
    }
}