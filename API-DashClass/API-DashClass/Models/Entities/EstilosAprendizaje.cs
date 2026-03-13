using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("estilos_aprendizaje")]
    public class EstilosAprendizaje
    {
        [Key]
        [Column("id_estilo")]
        public int IdEstilo { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("porcentaje_activo")]
        [Required]
        public decimal PorcentajeActivo { get; set; }

        [Column("porcentaje_reflexivo")]
        [Required]
        public decimal PorcentajeReflexivo { get; set; }

        [Column("porcentaje_sensorial")]
        [Required]
        public decimal PorcentajeSensorial { get; set; }

        [Column("porcentaje_intuitivo")]
        [Required]
        public decimal PorcentajeIntuitivo { get; set; }

        [Column("porcentaje_visual")]
        [Required]
        public decimal PorcentajeVisual { get; set; }

        [Column("porcentaje_verbal")]
        [Required]
        public decimal PorcentajeVerbal { get; set; }

        [Column("porcentaje_secuencial")]
        [Required]
        public decimal PorcentajeSecuencial { get; set; }

        [Column("porcentaje_global")]
        [Required]
        public decimal PorcentajeGlobal { get; set; }

        [Column("fecha_evaluacion")]
        public DateTime FechaEvaluacion { get; set; }

        [Column("respuestas_cuestionario")]
        public string? RespuestasCuestionario { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Usuario al que pertenece este estilo de aprendizaje (1:1) (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }
    }
}