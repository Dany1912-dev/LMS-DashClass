using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("anuncios")]
    public class Anuncios
    {
        [Key]
        [Column("id_anuncio")]
        public int Id { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("id_grupo")]
        public int? IdGrupo { get; set; }

        [Column("titulo")]
        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; }

        [Column("contenido")]
        [Required]
        public string Contenido { get; set; }

        [Column("destacado")]
        public bool Destacado { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_publicacion")]
        public DateTime FechaPublicacion { get; set; }

        [Column("fecha_ultima_edicion")]
        public DateTime? FechaUltimaEdicion { get; set; }
    }
}