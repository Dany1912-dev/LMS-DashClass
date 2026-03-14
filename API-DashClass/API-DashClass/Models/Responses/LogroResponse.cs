namespace API_DashClass.Models.Responses
{
    public class LogroResponse
    {
        public int IdLogro { get; set; }
        public int IdCurso { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Icono { get; set; }
        public string? CondicionDesbloqueo { get; set; }
        public bool Estatus { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NombreCreador { get; set; } = string.Empty;
        public int TotalDesbloqueado { get; set; }
    }
}