using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class CanjearRecompensaRequest
    {
        [Required(ErrorMessage = "El ID de la recompensa es requerido")]
        public int IdRecompensa { get; set; }

        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El ID del curso es requerido")]
        public int IdCurso { get; set; }
    }
}