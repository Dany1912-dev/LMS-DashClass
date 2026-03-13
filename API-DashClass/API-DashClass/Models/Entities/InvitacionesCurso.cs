using System;
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
            Enlace
        }

        [Key]
        [Column("id_invitacion")]
        public int IdInvitacion { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("id_grupo")]
        public int? IdGrupo { get; set; }

        [Column("tipo")]
        [Required]
        public TipoInvitacion Tipo { get; set; }

        [Column("codigo")]
        [MaxLength(6)]
        public string? Codigo { get; set; }

        [Column("token")]
        [MaxLength(255)]
        public string? Token { get; set; }

        [Column("fecha_expiracion")]
        public DateTime? FechaExpiracion { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Curso al que pertenece esta invitación (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }

        // Grupo al que está dirigida esta invitación (opcional) (FK)
        [ForeignKey("IdGrupo")]
        public Grupos? Grupo { get; set; }
    }
}