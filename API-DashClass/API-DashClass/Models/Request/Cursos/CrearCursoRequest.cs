using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class CrearCursoRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(255, ErrorMessage = "El nombre no puede exceder 255 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [MaxLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
        public string? ImagenBanner { get; set; }

        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Grupos iniciales del curso — se requiere al menos uno
        /// </summary>
        [Required(ErrorMessage = "Debes crear al menos un grupo")]
        [MinLength(1, ErrorMessage = "Debes crear al menos un grupo")]
        public List<CrearGrupoRequest> Grupos { get; set; } = new();
    }

    public class CrearGrupoRequest
    {
        [Required(ErrorMessage = "El nombre del grupo es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre del grupo no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }
    }
}