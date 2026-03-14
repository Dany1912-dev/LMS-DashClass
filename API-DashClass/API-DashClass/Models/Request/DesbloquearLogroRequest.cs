using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class DesbloquearLogroRequest
    {
        [Required(ErrorMessage = "El ID del logro es requerido")]
        public int IdLogro { get; set; }

        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int IdUsuario { get; set; }
    }
}