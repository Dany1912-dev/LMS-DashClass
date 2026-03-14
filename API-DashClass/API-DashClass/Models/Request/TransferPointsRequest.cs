using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class TransferPointsRequest
    {
        [Required(ErrorMessage = "El ID del usuario emisor es requerido")]
        public int DesdeIdUsuario { get; set; }

        [Required(ErrorMessage = "El ID del usuario receptor es requerido")]
        public int HaciaIdUsuario { get; set; }

        [Required(ErrorMessage = "El ID del curso es requerido")]
        public int IdCurso { get; set; }

        [Required(ErrorMessage = "La cantidad de puntos es requerida")]
        [Range(1, 500, ErrorMessage = "La cantidad debe estar entre 1 y 500 puntos")]
        public int Cantidad { get; set; }

        [MaxLength(200, ErrorMessage = "El mensaje no puede exceder 200 caracteres")]
        public string? Mensaje { get; set; }

        public bool Anonima { get; set; } = false;
    }
}
