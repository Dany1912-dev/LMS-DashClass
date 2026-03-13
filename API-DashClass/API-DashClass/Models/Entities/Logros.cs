using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("logros")]
    public class Logros
    {
        [Key]
        [Column("id_logro")]
        public int IdLogro { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("url_icono")]
        [MaxLength(500)]
        public string? UrlIcono { get; set; }

        [Column("criterios")]
        public string? Criterios { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Curso al que pertenece este logro (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }

        // Usuario (profesor) que creó este logro (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? CreadoPor { get; set; }

        // Desbloqueos de este logro por estudiantes
        public ICollection<LogrosUsuario>? LogrosUsuario { get; set; }
    }
}