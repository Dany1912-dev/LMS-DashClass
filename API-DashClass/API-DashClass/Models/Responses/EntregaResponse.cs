namespace API_DashClass.Models.Responses
{
    public class EntregaResponse
    {
        public int IdEntrega { get; set; }
        public int IdActividad { get; set; }
        public string TituloActividad { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string? Comentarios { get; set; }
        public DateTime FechaEntrega { get; set; }
        public bool EsTardia { get; set; }
        public int Version { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<RecursoEntregaResponse> Recursos { get; set; } = new();
        public CalificacionResponse? Calificacion { get; set; }
    }

    public class RecursoEntregaResponse
    {
        public int IdRecurso { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? UrlArchivo { get; set; }
        public long? TamanoArchivo { get; set; }
        public string? UrlExterna { get; set; }
        public DateTime FechaSubida { get; set; }
    }

    public class CalificacionResponse
    {
        public int IdCalificacion { get; set; }
        public int IdEntrega { get; set; }
        public decimal Puntuacion { get; set; }
        public string? Retroalimentacion { get; set; }
        public int IdUsuario { get; set; }
        public string NombreProfesor { get; set; } = string.Empty;
        public DateTime FechaCalificacion { get; set; }
    }
}