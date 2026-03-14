namespace API_DashClass.Models.Responses
{
    public class CanjeResponse
    {
        public int IdCanje { get; set; }
        public int IdRecompensa { get; set; }
        public string NombreRecompensa { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;
        public int PuntosGastados { get; set; }
        public string CodigoCanje { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCanje { get; set; }
        public DateTime? FechaReclamado { get; set; }
        public bool PuedeSerCancelado { get; set; }
    }
}