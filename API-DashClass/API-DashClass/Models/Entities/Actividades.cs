using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    public enum EstatusActividad
    {
        Borrador,
        Publicado,
        Programado
    }

    [Table("actividades")]
    public class Actividades
    {
        [Key]
        [Column("id_actividad")]
        public int IdActividad { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("titulo")]
        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Column("puntos_maximos")]
        [Required]
        public int PuntosMaximos { get; set; }

        [Column("puntos_gamificacion_maximos")]
        [Required]
        public int PuntosGamificacionMaximos { get; set; } 

        [Column("fecha_limite")]
        public DateTime? FechaLimite { get; set; }

        [Column("permite_entregas_tardia")]
        public bool PermiteEntregasTardia { get; set; }

        [Column("estatus")]
        public EstatusActividad Estatus { get; set; }

        [Column("fecha_publicacion")]
        public DateTime? FechaPublicacion { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
    }
}