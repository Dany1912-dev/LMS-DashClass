using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("materiales_anuncio")]
    public class MaterialesAnuncio
    {
        public enum TipoMaterial
        {
            Archivo,
            Enlace
        }

        [Key]
        [Column("id_material")]
        public int IdMaterial { get; set; }

        [Column("id_anuncio")]
        [Required]
        public int IdAnuncio { get; set; }

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

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Anuncio al que pertenece este material (FK)
        [ForeignKey("IdAnuncio")]
        public Anuncios? Anuncio { get; set; }
    }
}