namespace API_DashClass.Models.Responses
{
    public class ActividadResponse
    {
        public int IdActividad { get; set; }
        public int IdCurso { get; set; }
        public int IdUsuario { get; set; }
        public string NombreProfesor { get; set; } = string.Empty;

        // Categoría (nullable — null = sin categoría, no afecta calificación final)
        public int? IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public decimal? PesoCategoria { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int PuntosMaximos { get; set; }
        public int PuntosGamificacionMaximos { get; set; }
        public DateTime? FechaLimite { get; set; }
        public bool PermiteEntregasTardias { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public DateTime? FechaPublicacion { get; set; }
        public DateTime? FechaProgramada { get; set; }
        public DateTime FechaCreacion { get; set; }

        public List<GrupoActividadResponse> Grupos { get; set; } = new();
        public bool EsParaTodos { get; set; }
    }

    public class GrupoActividadResponse
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}