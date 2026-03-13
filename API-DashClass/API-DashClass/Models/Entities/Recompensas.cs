using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("recompensas")]
    public class Recompensas
    {
        [Key]
        [Column("id_recompensa")]
        public int IdRecompensa { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("url_imagen")]
        [Required]
        public string? UrlImagen { get; set; }

        [Column("costo")]
        [Required]
        public decimal Costo { get; set; }

        [Column("limite_por_usuario")]
        public int? LimitePorUsuario { get; set; }

        [Column("stock_global")]
        public int? StockGlobal { get; set; }

        [Column("stock_restante")]
        public int? StockRestante { get; set; }

        [Column("fecha_inicio")]
        [Required]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_fin")]
        [Required]
        public DateTime FechaFin { get; set; }

        [Column("destacado")]
        public bool Destacado { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
    }
}