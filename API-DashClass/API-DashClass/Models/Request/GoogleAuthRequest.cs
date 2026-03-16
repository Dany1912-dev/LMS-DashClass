using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class GoogleAuthRequest
    {
        /// <summary>
        /// Token de ID que devuelve Google al usuario después de autenticarse
        /// </summary>
        [Required(ErrorMessage = "El token de Google es requerido")]
        public string IdToken { get; set; } = string.Empty;
    }
}