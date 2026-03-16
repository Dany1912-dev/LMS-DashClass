using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class MicrosoftAuthRequest
    {
        /// <summary>
        /// Token de ID que devuelve Microsoft al usuario después de autenticarse
        /// </summary>
        [Required(ErrorMessage = "El token de Microsoft es requerido")]
        public string IdToken { get; set; } = string.Empty;
    }
}