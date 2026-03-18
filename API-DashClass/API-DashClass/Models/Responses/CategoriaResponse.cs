namespace API_DashClass.Models.Responses
{
    /// <summary>
    /// Response de una categoría de actividad
    /// </summary>
    public class CategoriaResponse
    {
        public int IdCategoria { get; set; }
        public int IdCurso { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Peso { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int TotalActividades { get; set; }
    }

    /// <summary>
    /// Calificación final ponderada de un estudiante en un curso
    /// </summary>
    public class CalificacionFinalResponse
    {
        public int IdUsuario { get; set; }
        public int IdCurso { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;
        public decimal CalificacionFinal { get; set; }
        public decimal PesoTotalConfigurado { get; set; }
        public List<DesgloseCategoriaResponse> Desglose { get; set; } = new();
    }

    /// <summary>
    /// Detalle de cómo aportó cada categoría a la calificación final
    /// </summary>
    public class DesgloseCategoriaResponse
    {
        public int IdCategoria { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public decimal Peso { get; set; }

        /// <summary>Promedio del estudiante en esta categoría (0–100)</summary>
        public decimal PromedioCategoria { get; set; }

        /// <summary>Cuánto aportó al final: PromedioCategoria * Peso / 100</summary>
        public decimal AporteAlFinal { get; set; }

        public int ActividadesCalificadas { get; set; }
        public int ActividadesTotales { get; set; }
    }
}