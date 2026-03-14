using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class ActualizarRecompensaRequest
    {
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
        public int? StockGlobal { get; set; }

        public bool Destacado { get; set; }

        public bool Estatus { get; set; }
    }
}