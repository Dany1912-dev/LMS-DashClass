namespace API_DashClass.Models.Responses
{
    public class TransferResponse
    {
        public int IdTransferencia { get; set; }
        public string CodigoTransferencia { get; set; } = string.Empty;
        public int DesdeIdUsuario { get; set; }
        public int HaciaIdUsuario { get; set; }
        public int Cantidad { get; set; }
        public bool Anonima { get; set; }
        public DateTime FechaTransferencia { get; set; }
        public string Mensaje { get; set; } = "Transferencia exitosa";
    }
}
