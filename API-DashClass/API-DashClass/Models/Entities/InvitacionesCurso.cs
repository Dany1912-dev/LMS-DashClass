using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("invitaciones_curso")]
    public class InvitacionesCurso
    {
        public enum TipoInvitacion
        {
            Codigo,
            Enlace,
            Email
        }

        [Key]
        [Column("id_invitacion")]
        public int Id { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("id_grupo")]
        public int? IdGrupo { get; set; }

        [Column("tipo")]
        [Required]
        public TipoInvitacion Tipo { get; set; }

        [Column("codigo")]
        [Required]
        [MaxLength(6)]
        public string Codigo { get; set; }

        [Column("token")]
        [Required]
        [MaxLength(255)]
        public string Token { get; set; }

        [Column("fecha_expiracion")]
        public DateTime? FechaExpiracion { get; set; }

        [Column("fecha_creacion")]
        [Required]
        public DateTime FechaCreacion { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; } = true;
    }
}