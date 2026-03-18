using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class CrearCategoriaRequest
    {
        [Required]
        public int IdCurso { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100, ErrorMessage = "El peso debe estar entre 0.01 y 100")]
        public decimal Peso { get; set; }

        [MaxLength(255)]
        public string? Descripcion { get; set; }
    }

    public class ActualizarCategoriaRequest
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100, ErrorMessage = "El peso debe estar entre 0.01 y 100")]
        public decimal Peso { get; set; }

        [MaxLength(255)]
        public string? Descripcion { get; set; }
    }
}