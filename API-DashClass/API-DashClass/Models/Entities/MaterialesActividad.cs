using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("materiales_actividad")]
    public class MaterialesActividad
    {
        public enum TipoMaterial
        {
            Archivo,
            Enlace
        }

        [Key]
        [Column("id_material")]
        public int IdMaterial { get; set; }

        [Column("id_actividad")]
        [Required]
        public int IdActividad { get; set; }

        [Column("tipo")]
        [Required]
        public TipoMaterial Tipo { get; set; }

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
