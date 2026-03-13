using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("grupos")]
    public class Grupos
    {
        [Key]
        [Column("id_grupo")]
        public int IdGrupo { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Curso al que pertenece este grupo (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }

        // Miembros de este grupo
        public ICollection<MiembrosCursos>? Miembros { get; set; }

        // Invitaciones para este grupo
        public ICollection<InvitacionesCurso>? Invitaciones { get; set; }

        // Actividades asignadas a este grupo
        public ICollection<ActividadesGrupos>? ActividadesGrupos { get; set; }

        // Anuncios dirigidos a este grupo
        public ICollection<Anuncios>? Anuncios { get; set; }

        // Sesiones de asistencia para este grupo
        public ICollection<SesionesAsistencia>? SesionesAsistencia { get; set; }

        // Evaluaciones para este grupo
        public ICollection<Evaluaciones>? Evaluaciones { get; set; }
    }
}