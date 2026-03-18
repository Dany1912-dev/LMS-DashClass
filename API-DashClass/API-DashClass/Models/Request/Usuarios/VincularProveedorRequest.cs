using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class VincularGoogleRequest
    {
        [Required(ErrorMessage = "El token de Google es requerido")]
        public string IdToken { get; set; } = string.Empty;
    }

    public class VincularMicrosoftRequest
    {
        [Required(ErrorMessage = "El token de Microsoft es requerido")]
        public string IdToken { get; set; } = string.Empty;
    }
}