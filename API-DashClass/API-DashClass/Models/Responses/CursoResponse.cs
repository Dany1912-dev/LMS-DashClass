namespace API_DashClass.Models.Responses
{
    public class CursoResponse
    {
        public int IdCurso { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? ImagenBanner { get; set; }
        public int IdUsuario { get; set; }
        public string NombreProfesor { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalGrupos { get; set; }
        public List<GrupoResponse> Grupos { get; set; } = new();
    }

    public class GrupoResponse
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estatus { get; set; }
        public int TotalMiembros { get; set; }
        public InvitacionCursoResponse? Invitacion { get; set; }
    }

    public class InvitacionCursoResponse
    {
        public string? Codigo { get; set; }
        public string? Token { get; set; }
        public string? EnlaceInvitacion { get; set; }
        public string? NombreGrupo { get; set; }
        public DateTime? FechaExpiracion { get; set; }
    }

    public class MiembroCursoResponse
    {
        public int IdMiembroCurso { get; set; }
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string? NombreGrupo { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public bool Estatus { get; set; }
    }
}