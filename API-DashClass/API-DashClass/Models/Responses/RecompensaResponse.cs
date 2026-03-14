namespace API_DashClass.Models.Responses
{
    public class RecompensaResponse
    {
        public int IdRecompensa { get; set; }
        public int IdCurso { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Costo { get; set; }
        public int? StockGlobal { get; set; }
        public int? StockRestante { get; set; }
        public bool EsIlimitado { get; set; }
        public bool Destacado { get; set; }
        public bool Estatus { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NombreCreador { get; set; } = string.Empty;
        public bool DisponibleParaCanje { get; set; }
    }
}