using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class CrearInvitacionRequest
    {
        [Required(ErrorMessage = "El ID del grupo es requerido")]
        public int IdGrupo { get; set; }

        /// <summary>
        /// Duración de la invitación
        /// </summary>
        [Required(ErrorMessage = "La duración es requerida")]
        public DuracionInvitacion Duracion { get; set; }
    }

    public enum DuracionInvitacion
    {
        UnDia,
        UnaSemana,
        UnMes,
        SinExpiracion
    }
}