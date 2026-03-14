namespace API_DashClass.Models.Responses
{
    public class ProgresoLogrosResponse
    {
        public int IdUsuario { get; set; }
        public int IdCurso { get; set; }
        public int LogrosDesbloqueados { get; set; }
        public int LogrosTotales { get; set; }
        public decimal PorcentajeCompletado { get; set; }
    }
}