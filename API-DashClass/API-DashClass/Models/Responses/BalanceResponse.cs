namespace API_DashClass.Models.Responses
{
    public class BalanceResponse
    {
        public int IdUsuario { get; set; }
        public int IdCurso { get; set; }
        public int PuntosActuales { get; set; }
        public int TotalGanado { get; set; }
        public int TotalGastado { get; set; }
        public int TotalTransferido { get; set; }
        public DateTime UltimaActualizacion { get; set; }
    }
}
