using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("miembros_cursos")]
    public class MiembrosCursos
    {
        public enum RolMiembro
        {
            Estudiante,
            Profesor
        }

        [Key]
        [Column("id_miembro_curso")]
        public int Id { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("rol")]
        [Required]
        public RolMiembro Rol { get; set; }

        [Column("fecha_inscripcion")]
        public DateTime FechaIngreso { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }
    }
}