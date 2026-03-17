namespace API_DashClass.Events
{
    /// <summary>
    /// Evento disparado cuando se registra asistencia
    /// </summary>
    public class AsistenciaRegistradaEvento : IEvento
    {
        public int IdRegistroAsistencia { get; set; }
        public int IdSesionAsistencia { get; set; }
        public int IdUsuario { get; set; }
        public int IdCurso { get; set; }
        public DateTime OcurrioEn { get; set; } = DateTime.UtcNow;
    }
}