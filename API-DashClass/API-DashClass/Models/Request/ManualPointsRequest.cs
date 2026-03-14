using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class ManualPointsRequest
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El ID del curso es requerido")]
        public int IdCurso { get; set; }

        [Required(ErrorMessage = "La cantidad de puntos es requerida")]
        [Range(-1000, 1000, ErrorMessage = "La cantidad debe estar entre -1000 y 1000")]
        public int Cantidad { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }
    }
}
