using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("recursos_entrega")]
    public class RecursosEntrega
    {
        public enum TipoRecurso
        {
            Archivo,
            Enlace
        }

        [Key]
        [Column("id_recurso")]
        public int IdRecurso { get; set; }

        [Column("id_entrega")]
        [Required]
        public int IdEntrega { get; set; }

        [Column("tipo")]
        [Required]
        public TipoRecurso Tipo { get; set; }

        [Column("nombre")]
        [MaxLength(255)]
        [Required]
        public string Nombre { get; set; }

        [Column("url_archivo")]
        [MaxLength(500)]
        public string? UrlArchivo { get; set; }

        [Column("tamano_archivo")]
        public long? TamanoArchivo { get; set; }

        [Column("url_externa")]
        [MaxLength(500)]
        public string? UrlExterna { get; set; }

        [Column("fecha_subida")]
        public DateTime FechaSubida { get; set; }
    }
}
