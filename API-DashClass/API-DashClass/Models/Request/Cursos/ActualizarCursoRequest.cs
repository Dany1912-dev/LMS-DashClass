using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class ActualizarCursoRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(255, ErrorMessage = "El nombre no puede exceder 255 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [MaxLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
        public string? ImagenBanner { get; set; }
    }
}