using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace API_DashClass.Models.Request
{
    /// <summary>
    /// Marcar como entregada sin archivos
    /// </summary>
    public class MarcarEntregadaRequest
    {
        [Required]
        public int IdActividad { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        public string? Comentarios { get; set; }
    }

    /// <summary>
    /// Entregar con archivos (multipart/form-data)
    /// </summary>
    public class SubirEntregaRequest
    {
        [Required]
        public int IdActividad { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        public string? Comentarios { get; set; }

        /// <summary>
        /// Uno o varios archivos adjuntos
        /// </summary>
        public List<IFormFile>? Archivos { get; set; }
    }

    /// <summary>
    /// Calificar una entrega
    /// </summary>
    public class CalificarEntregaRequest
    {
        [Required]
        public int IdUsuarioProfesor { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "La puntuación debe estar entre 0 y 100")]
        public decimal Puntuacion { get; set; }

        public string? Retroalimentacion { get; set; }
    }
}