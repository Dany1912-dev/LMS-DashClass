namespace API_DashClass.Models.Responses
{
    public class LogroUsuarioResponse
    {
        public int IdLogroUsuario { get; set; }
        public int IdLogro { get; set; }
        public string NombreLogro { get; set; } = string.Empty;
        public string? DescripcionLogro { get; set; }
        public string? IconoLogro { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime FechaDesbloqueo { get; set; }
    }
}