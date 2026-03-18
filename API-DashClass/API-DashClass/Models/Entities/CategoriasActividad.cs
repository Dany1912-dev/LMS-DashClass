using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("categorias_actividad")]
    public class CategoriasActividad
    {
        [Key]
        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Column("peso")]
        [Required]
        public decimal Peso { get; set; }

        [Column("descripcion")]
        [MaxLength(255)]
        public string? Descripcion { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }

        public ICollection<Actividades>? Actividades { get; set; }
    }
}