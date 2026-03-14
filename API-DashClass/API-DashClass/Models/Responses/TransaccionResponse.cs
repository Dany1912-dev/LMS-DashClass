namespace API_DashClass.Models.Responses
{
    public class TransaccionResponse
    {
        public int IdTransaccion { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public int BalanceDespues { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
