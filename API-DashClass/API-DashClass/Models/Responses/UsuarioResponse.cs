namespace API_DashClass.Models.Responses
{
    public class UsuarioResponse
    {
        public int IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string? Biografia { get; set; }
        public string ProveedorAuthPrincipal { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool Estatus { get; set; }
    }

    public class EstadisticasUsuarioResponse
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int TotalCursos { get; set; }
        public int TotalLogros { get; set; }
        public int TotalCanjes { get; set; }
        public int TotalAsistencias { get; set; }
        public List<PuntosPorCursoResponse> PuntosPorCurso { get; set; } = new();
    }

    public class PuntosPorCursoResponse
    {
        public int IdCurso { get; set; }
        public string NombreCurso { get; set; } = string.Empty;
        public int PuntosActuales { get; set; }
        public int PuntosGanados { get; set; }
        public int PuntosGastados { get; set; }
    }

    public class MetodoAuthResponse
    {
        public int IdMetodo { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool Verificado { get; set; }
        public DateTime VinculadoEn { get; set; }
        public DateTime? UltimoUso { get; set; }
    }

}