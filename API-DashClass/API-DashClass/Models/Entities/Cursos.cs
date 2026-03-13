using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("cursos")]
    public class Cursos
    {
        [Key]
        [Column("id_curso")]
        public int IdCurso { get; set; }

        [Column("codigo")]
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("imagen_banner")]
        [MaxLength(500)]
        public string? ImagenBanner { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; } = true;
    }
}
