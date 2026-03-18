using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class ActualizarPerfilRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [MaxLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        public string Apellidos { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La biografía no puede exceder 500 caracteres")]
        public string? Biografia { get; set; }
    }
}