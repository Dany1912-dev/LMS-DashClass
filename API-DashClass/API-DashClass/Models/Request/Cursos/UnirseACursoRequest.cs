using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class UnirseACursoRequest
    {
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Código de 6 dígitos o token del enlace
        /// </summary>
        [Required(ErrorMessage = "El código o token es requerido")]
        public string CodigoOToken { get; set; } = string.Empty;
    }
}