using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class ActualizarLogroRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [MaxLength(255, ErrorMessage = "El icono no puede exceder 255 caracteres")]
        public string? Icono { get; set; }

        [MaxLength(500, ErrorMessage = "La condición de desbloqueo no puede exceder 500 caracteres")]
        public string? CondicionDesbloqueo { get; set; }

        public bool Estatus { get; set; }
    }
}