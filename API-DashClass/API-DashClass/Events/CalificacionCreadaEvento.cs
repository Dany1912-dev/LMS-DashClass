namespace API_DashClass.Events
{
    /// <summary>
    /// Evento disparado cuando se crea una calificación
    /// </summary>
    public class CalificacionCreadaEvento : IEvento
    {
        public int IdEntrega { get; set; }
        public int IdActividad { get; set; }
        public int IdUsuario { get; set; }
        public int IdCurso { get; set; }
        public decimal Puntuacion { get; set; }
        public int PuntosMaximos { get; set; }
        public int PuntosGamificacionMaximos { get; set; }
        public DateTime OcurrioEn { get; set; } = DateTime.UtcNow;
    }
}