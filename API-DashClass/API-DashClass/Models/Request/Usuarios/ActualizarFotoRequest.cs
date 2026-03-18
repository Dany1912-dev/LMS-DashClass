using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class ActualizarFotoRequest
    {
        [Required(ErrorMessage = "La URL de la foto es requerida")]
        [MaxLength(500, ErrorMessage = "La URL no puede exceder 500 caracteres")]
        public string FotoPerfilUrl { get; set; } = string.Empty;
    }
}