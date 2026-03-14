namespace API_DashClass.Models.Responses
{
    public class RankingResponse
    {
        public int Posicion { get; set; }
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public int TotalPuntos { get; set; }
    }
}
