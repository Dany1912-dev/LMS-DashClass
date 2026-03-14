using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class CrearRecompensaRequest
    {
        [Required(ErrorMessage = "El ID del curso es requerido")]
        public int IdCurso { get; set; }

        [Required(ErrorMessage = "El ID del usuario creador es requerido")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El costo en puntos es requerido")]
        [Range(1, 10000, ErrorMessage = "El costo debe estar entre 1 y 10000 puntos")]
        public int Costo { get; set; }

        /// <summary>
        /// Stock global (null = ilimitado)
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int? StockGlobal { get; set; } = null;  // null = ilimitado

        public bool Destacado { get; set; } = false;
    }
}