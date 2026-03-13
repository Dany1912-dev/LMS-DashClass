using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("miembros_curso")]
    public class MiembrosCursos
    {
        public enum RolMiembro
        {
            Profesor,
            Estudiante
        }

        [Key]
        [Column("id_miembro_curso")]
        public int IdMiembroCurso { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("id_grupo")]
        public int? IdGrupo { get; set; }

        [Column("rol")]
        [Required]
        public RolMiembro Rol { get; set; }

        [Column("fecha_inscripcion")]
        public DateTime FechaInscripcion { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Curso al que pertenece esta membresía (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }

        // Usuario que es miembro (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        // Grupo al que pertenece (opcional) (FK)
        [ForeignKey("IdGrupo")]
        public Grupos? Grupo { get; set; }
    }
}